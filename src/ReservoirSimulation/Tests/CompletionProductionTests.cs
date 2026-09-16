using Microsoft.Data.Sqlite;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class CompletionProductionTests
{
    private string? _databasePath;

    [TearDown]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        if (_databasePath is not null && File.Exists(_databasePath)) File.Delete(_databasePath);
    }

    [Test]
    public async Task Run_FiveYears_PersistsSequentialPinnedCheckpointsAndMonthlyTruth()
    {
        Harness harness = await CreateHarnessAsync();
        CompletionProductionRequest request = ValidRequest();

        CompletionProductionResult result = await harness.Production.RunAsync(
            harness.World, harness.Completion.CompletionBindingId, request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Checkpoints.Select(checkpoint => checkpoint.Year), Is.EqualTo(new[] { 1, 3, 5 }));
            Assert.That(result.Checkpoints[0].ParentStateId, Is.Null);
            Assert.That(result.Checkpoints[1].ParentStateId, Is.EqualTo(result.Checkpoints[0].StateId));
            Assert.That(result.Checkpoints[2].ParentStateId, Is.EqualTo(result.Checkpoints[1].StateId));
            Assert.That(result.Checkpoints.All(checkpoint => checkpoint.MaximumBalanceErrorFraction <= 1e-6), Is.True);
            Assert.That(result.MonthlyTruth, Has.Count.EqualTo(60));
            Assert.That(result.MonthlyTruth.All(IsFiniteNonnegative), Is.True);
            Assert.That(IsMonotone(result.MonthlyTruth.Select(month => month.CumulativeOilM3)), Is.True);
            Assert.That(IsMonotone(result.MonthlyTruth.Select(month => month.CumulativeWaterM3)), Is.True);
            Assert.That(IsMonotone(result.MonthlyTruth.Select(month => month.CumulativeGasM3)), Is.True);
            Assert.That(IsMonotone(result.Checkpoints.Select(checkpoint => checkpoint.CumulativeProducedM3.Oil)), Is.True);
            Assert.That(result.MonthlyTruth[^1].CumulativeOilM3,
                Is.EqualTo(result.Checkpoints[^1].CumulativeProducedM3.Oil).Within(1e-8));
            Assert.That(result.MonthlyTruth[^1].CumulativeWaterM3,
                Is.EqualTo(result.Checkpoints[^1].CumulativeProducedM3.Water).Within(1e-8));
            Assert.That(result.MonthlyTruth[^1].CumulativeGasM3,
                Is.EqualTo(result.Checkpoints[^1].CumulativeProducedM3.Gas).Within(1e-8));
            Assert.That(result.MonthlyTruth.Any(month => month.SwitchReason == "Shut-in"), Is.True);
            Assert.That(result.MonthlyTruth.Any(month => month.SwitchReason == "Maximum rate constraint" &&
                month.EffectiveControlMode == WellControlMode.Rate), Is.True);
        });
        foreach (CompletionProductionCheckpoint checkpoint in result.Checkpoints)
        {
            Assert.That(await harness.StateRepository.CountStatePinsAsync(checkpoint.StateId), Is.EqualTo(1));
            Assert.That(await harness.StateStore.LoadAsync(harness.World, checkpoint.StateId), Is.Not.Null);
        }
    }

    [Test]
    public async Task Run_IsIdempotentRestartSafeAndTamperProtected()
    {
        Harness harness = await CreateHarnessAsync();
        CompletionProductionRequest request = ValidRequest();
        CompletionProductionResult first = await harness.Production.RunAsync(
            harness.World, harness.Completion.CompletionBindingId, request);
        CompletionProductionResult second = await harness.Production.RunAsync(
            harness.World, harness.Completion.CompletionBindingId, request);

        var restartedRepository = new SqliteCompletionProductionRepository(ConnectionString());
        await restartedRepository.InitializeAsync();
        var restarted = new CompletionProductionService(
            restartedRepository, harness.StateRepository, harness.CompletionService,
            harness.StateStore, new ReservoirSimulator(), TimeProvider.System);
        CompletionProductionResult restored = (await restarted.GetAsync(
            harness.World, harness.Completion.CompletionBindingId, first.ProductionRunId))!;

        Assert.Multiple(() =>
        {
            Assert.That(second.ProductionRunId, Is.EqualTo(first.ProductionRunId));
            Assert.That(restored.ProductionRunId, Is.EqualTo(first.ProductionRunId));
            Assert.That(restored.MonthlyTruth, Is.EqualTo(first.MonthlyTruth));
            Assert.That(restored.Checkpoints.Select(checkpoint => checkpoint.StateId),
                Is.EqualTo(first.Checkpoints.Select(checkpoint => checkpoint.StateId)));
        });

        await using var connection = new SqliteConnection(ConnectionString());
        await connection.OpenAsync();
        await using SqliteCommand tamper = connection.CreateCommand();
        tamper.CommandText =
            "UPDATE CompletionProductionRuns SET ResponseHash = $hash WHERE ProductionRunId = $id;";
        tamper.Parameters.AddWithValue("$hash", new string('f', 64));
        tamper.Parameters.AddWithValue("$id", first.ProductionRunId);
        Assert.ThrowsAsync<SqliteException>(async () => await tamper.ExecuteNonQueryAsync());
        await using SqliteCommand auditCount = connection.CreateCommand();
        auditCount.CommandText =
            "SELECT COUNT(*), COUNT(DISTINCT Action) FROM CompletionProductionAudit WHERE ProductionRunId = $id;";
        auditCount.Parameters.AddWithValue("$id", first.ProductionRunId);
        await using SqliteDataReader reader = await auditCount.ExecuteReaderAsync();
        Assert.That(await reader.ReadAsync(), Is.True);
        Assert.That(reader.GetInt32(0), Is.EqualTo(3));
        Assert.That(reader.GetInt32(1), Is.EqualTo(3));
        await reader.DisposeAsync();
        await using SqliteCommand auditTamper = connection.CreateCommand();
        auditTamper.CommandText =
            "UPDATE CompletionProductionAudit SET CheckpointCount = 99 WHERE ProductionRunId = $id;";
        auditTamper.Parameters.AddWithValue("$id", first.ProductionRunId);
        Assert.ThrowsAsync<SqliteException>(async () => await auditTamper.ExecuteNonQueryAsync());
    }

    [Test]
    public async Task PinnedCheckpoints_SurviveMoreThanTwentyUnpinnedStates()
    {
        Harness harness = await CreateHarnessAsync();
        CompletionProductionResult production = await harness.Production.RunAsync(
            harness.World, harness.Completion.CompletionBindingId, ValidRequest());
        var simulator = new ReservoirSimulator();
        for (int index = 1; index <= 25; index++)
        {
            var request = new SimulationRequest
            {
                DurationSeconds = index,
                InitialTimeStepSeconds = index,
                Solver = SimpleSolver(index) with { MinimumTimeStepSeconds = 0.01 },
                Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
                Wells = []
            };
            SimulationExecution execution = simulator.Run(harness.World, request);
            await harness.StateStore.SaveAsync(harness.World, null, request, execution);
        }

        Assert.That(await harness.StateRepository.CountStatesAsync(harness.World.Summary.WorldId), Is.EqualTo(23));
        foreach (CompletionProductionCheckpoint checkpoint in production.Checkpoints)
            Assert.That(await harness.StateStore.LoadAsync(harness.World, checkpoint.StateId), Is.Not.Null);
    }

    [TestCase(true, false, false, false)]
    [TestCase(false, true, false, false)]
    [TestCase(false, false, true, false)]
    [TestCase(false, false, false, true)]
    public async Task Run_InvalidScheduleGapOverlapHorizonOrMissingShutIn_IsRejected(
        bool gap, bool overlap, bool shortHorizon, bool removeShutIn)
    {
        Harness harness = await CreateHarnessAsync();
        CompletionProductionRequest valid = ValidRequest();
        CompletionProductionScheduleSegment[] segments = valid.Schedule.ToArray();
        if (gap)
            segments[1] = segments[1] with { StartTimeSeconds = segments[1].StartTimeSeconds + 1_000 };
        if (overlap)
            segments[1] = segments[1] with { StartTimeSeconds = segments[1].StartTimeSeconds - 1_000 };
        if (shortHorizon)
            segments[^1] = segments[^1] with { DurationSeconds = segments[^1].DurationSeconds - 1_000 };
        if (removeShutIn)
            segments[1] = segments[1] with
            {
                ShutIn = false,
                ControlMode = WellControlMode.Rate,
                TargetRateM3PerSecond = -0.00005
            };

        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await harness.Production.RunAsync(harness.World, harness.Completion.CompletionBindingId,
                valid with { Schedule = segments }));
    }

    [Test]
    public async Task Run_CompletionWorldMismatchIsRejected()
    {
        Harness harness = await CreateHarnessAsync();
        var worldManager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(4),
            new SqliteReservoirRepository(ConnectionString()), TimeProvider.System);
        ReservoirWorld other = await worldManager.CreateAsync(TestData.UniformWorldRequest() with
        {
            FieldId = Guid.Parse("7ed3410f-6f65-46ed-b13d-a979ad47d7e1")
        });

        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await harness.Production.RunAsync(other, harness.Completion.CompletionBindingId, ValidRequest()));
    }

    private async Task<Harness> CreateHarnessAsync()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-production-{Guid.NewGuid():N}.db");
        string connectionString = ConnectionString();
        var stateRepository = new SqliteReservoirRepository(connectionString);
        await stateRepository.InitializeAsync();
        var pathRepository = new SqliteTruthSamplingRepository(connectionString);
        await pathRepository.InitializeAsync();
        var completionRepository = new SqliteCompletionBindingRepository(connectionString);
        await completionRepository.InitializeAsync();
        var productionRepository = new SqliteCompletionProductionRepository(connectionString);
        await productionRepository.InitializeAsync();
        var worldManager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(4), stateRepository, TimeProvider.System);
        ReservoirWorld world = await worldManager.CreateAsync(TestData.UniformWorldRequest(8, 3, 1));
        var pathService = new RestrictedTruthSamplingService(pathRepository, TimeProvider.System);
        ApprovedPathBindingMetadata path = await pathService.RegisterAsync(world, new ApprovedPathBindingRequest
        {
            ScenarioId = Guid.Parse("83716d28-1ec9-4e1c-8411-b7d8db865dc5"),
            RunId = Guid.Parse("f0baed86-7ac5-4550-800f-6c590154f193"),
            PathKind = ApprovedPathKind.AsDrilled,
            ApprovedSealedPredictionSha256 =
                "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff",
            Stations =
            [
                new ApprovedPathStation
                {
                    MeasuredDepthM = 0, EastingM = 0, NorthingM = 0, TrueVerticalDepthM = 1_015
                },
                new ApprovedPathStation
                {
                    MeasuredDepthM = 700, EastingM = 700, NorthingM = 0, TrueVerticalDepthM = 1_015
                }
            ]
        });
        var completionService = new ApprovedCompletionBindingService(
            completionRepository, pathService, TimeProvider.System);
        ApprovedCompletionBindingMetadata completion = await completionService.RegisterAsync(world,
            new ApprovedCompletionBindingRequest
            {
                PathBindingId = path.BindingId,
                CompletionModelVersion = "observed-log-completion-v1",
                Openings =
                [
                    new ApprovedCompletionOpening
                    {
                        OpeningId = "producer-open-hole", ReservoirName = "Uniform Sand",
                        Type = CompletionOpeningType.OpenHole,
                        TopMeasuredDepthM = 0, BaseMeasuredDepthM = 700,
                        WellboreRadiusM = 0.1, Skin = 0, Efficiency = 1, UncertaintyM = 0
                    }
                ]
            });
        var stateStore = new PersistentSimulationStateStore(stateRepository, TimeProvider.System);
        var production = new CompletionProductionService(
            productionRepository, stateRepository, completionService, stateStore,
            new ReservoirSimulator(), TimeProvider.System);
        return new Harness(world, completion, completionService, stateRepository, stateStore, production);
    }

    private string ConnectionString() => $"Data Source={_databasePath}";

    private static CompletionProductionRequest ValidRequest()
    {
        double year = CompletionProductionService.YearSeconds;
        return new CompletionProductionRequest
        {
            ProductionModelVersion = "completion-production-v1",
            InitialTimeStepSeconds = 30 * 24 * 60 * 60,
            Solver = SimpleSolver(30 * 24 * 60 * 60) with { MaximumSamplesPerWell = 120 },
            Schedule =
            [
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 0, DurationSeconds = year,
                    ControlMode = WellControlMode.Rate, TargetRateM3PerSecond = -0.0001
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = year, DurationSeconds = 0.25 * year, ShutIn = true
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 1.25 * year, DurationSeconds = 1.75 * year,
                    ControlMode = WellControlMode.Bhp, TargetBottomHolePressurePa = 1_000_000,
                    MaximumAbsoluteRateM3PerSecond = 0.00005
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 3 * year, DurationSeconds = 2 * year,
                    ControlMode = WellControlMode.Rate, TargetRateM3PerSecond = -0.00008,
                }
            ]
        };
    }

    private static SolverOptions SimpleSolver(double maximumTimeStep) => new()
    {
        MinimumTimeStepSeconds = 60,
        MaximumTimeStepSeconds = maximumTimeStep,
        MaximumSaturationChange = 0.05,
        GrowthSaturationChange = 0.005,
        TimeStepGrowthFactor = 1.5,
        TimeStepShrinkFactor = 0.5,
        CgRelativeTolerance = 1e-9,
        CgMaximumIterations = 1_000,
        MaximumStepAttempts = 10_000,
        MaximumSamplesPerWell = 120
    };

    private static bool IsFiniteNonnegative(CompletionProductionMonthlyTruth month) =>
        double.IsFinite(month.OilRateM3PerSecond) && month.OilRateM3PerSecond >= 0 &&
        double.IsFinite(month.WaterRateM3PerSecond) && month.WaterRateM3PerSecond >= 0 &&
        double.IsFinite(month.GasRateM3PerSecond) && month.GasRateM3PerSecond >= 0 &&
        double.IsFinite(month.CumulativeOilM3) && month.CumulativeOilM3 >= 0 &&
        double.IsFinite(month.CumulativeWaterM3) && month.CumulativeWaterM3 >= 0 &&
        double.IsFinite(month.CumulativeGasM3) && month.CumulativeGasM3 >= 0 &&
        double.IsFinite(month.BottomHolePressurePa) && month.BottomHolePressurePa > 0;

    private static bool IsMonotone(IEnumerable<double> values)
    {
        double previous = double.NegativeInfinity;
        foreach (double value in values)
        {
            if (value < previous) return false;
            previous = value;
        }
        return true;
    }

    private sealed record Harness(
        ReservoirWorld World,
        ApprovedCompletionBindingMetadata Completion,
        ApprovedCompletionBindingService CompletionService,
        SqliteReservoirRepository StateRepository,
        PersistentSimulationStateStore StateStore,
        CompletionProductionService Production);
}
