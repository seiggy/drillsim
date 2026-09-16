using Microsoft.Data.Sqlite;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class PersistenceTests
{
    private readonly List<string> _databasePaths = [];
    private readonly ReservoirSimulator _simulator = new();

    [TearDown]
    public void DeleteTemporaryDatabases()
    {
        SqliteConnection.ClearAllPools();
        foreach (string path in _databasePaths)
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + "-wal")) File.Delete(path + "-wal");
            if (File.Exists(path + "-shm")) File.Delete(path + "-shm");
        }
        _databasePaths.Clear();
    }

    [Test]
    public async Task WorldSpec_V2ModelVersion_FailsWithoutSilentMigration()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        ReservoirWorld world = await Manager(repository).CreateAsync(TestData.ConditionedRequest());
        await using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                "UPDATE ReservoirWorldSpecs SET ModelVersion = $version WHERE WorldId = $worldId;";
            command.Parameters.AddWithValue("$version", "reservoir-hidden-world-v2");
            command.Parameters.AddWithValue("$worldId", world.Summary.WorldId);
            await command.ExecuteNonQueryAsync();
        }

        var restarted = Manager(new SqliteReservoirRepository(connectionString));
        PersistenceIntegrityException? exception = Assert.ThrowsAsync<PersistenceIntegrityException>(
            async () => await restarted.GetAsync(world.Summary.WorldId));

        Assert.That(exception!.Message, Does.Contain("reservoir-hidden-world-v2"));
    }


    [Test]
    public async Task WorldSpec_FreshStore_RegeneratesIdenticalTruth()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        var firstManager = Manager(repository, capacity: 1);
        ReservoirWorld created = await firstManager.CreateAsync(TestData.ConditionedRequest());
        ReservoirWorld idempotent = await firstManager.CreateAsync(TestData.ConditionedRequest());

        var restartedRepository = new SqliteReservoirRepository(connectionString);
        await restartedRepository.InitializeAsync();
        var restartedManager = Manager(restartedRepository, capacity: 1);
        ReservoirWorld? restored = await restartedManager.GetAsync(created.Summary.WorldId);

        Assert.Multiple(() =>
        {
            Assert.That(idempotent.Summary.WorldId, Is.EqualTo(created.Summary.WorldId));
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored, Is.Not.SameAs(created));
            Assert.That(restored!.PressurePa, Is.EqualTo(created.PressurePa));
            Assert.That(restored.Porosity, Is.EqualTo(created.Porosity));
            Assert.That(restored.LogPermeability, Is.EqualTo(created.LogPermeability));
            Assert.That(ReservoirWorldFactory.TruthChecksum(restored),
                Is.EqualTo(ReservoirWorldFactory.TruthChecksum(created)));
        });
    }

    [Test]
    public async Task State_RoundTripsCompressedArraysAndChecksum()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        ReservoirWorld world = await Manager(repository).CreateAsync(TestData.ConditionedRequest());
        var stateStore = new PersistentSimulationStateStore(repository, TimeProvider.System);
        SimulationRequest request = ClosedRun(100);
        SimulationExecution execution = _simulator.Run(world, request);
        string stateId = await stateStore.SaveAsync(world, null, request, execution);

        var restartedRepository = new SqliteReservoirRepository(connectionString);
        await restartedRepository.InitializeAsync();
        var restartedStateStore = new PersistentSimulationStateStore(restartedRepository, TimeProvider.System);
        RestoredSimulationState? restored = await restartedStateStore.LoadAsync(world, stateId);

        Assert.Multiple(() =>
        {
            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.StateId, Is.EqualTo(stateId));
            Assert.That(restored.ParentStateId, Is.Null);
            Assert.That(restored.State.PressurePa, Is.EqualTo(execution.FinalState.PressurePa));
            Assert.That(restored.State.OilSaturation, Is.EqualTo(execution.FinalState.OilSaturation));
            Assert.That(restored.State.WaterSaturation, Is.EqualTo(execution.FinalState.WaterSaturation));
            Assert.That(restored.State.GasSaturation, Is.EqualTo(execution.FinalState.GasSaturation));
        });
        double[] alteredPressure = (double[])execution.FinalState.PressurePa.Clone();
        alteredPressure[0] += 1;
        await UpdateBlobAsync(connectionString, stateId, StateBlobCodec.Compress(alteredPressure));
        PersistenceIntegrityException? checksumFailure = Assert.ThrowsAsync<PersistenceIntegrityException>(
            async () => await restartedStateStore.LoadAsync(world, stateId));
        Assert.That(checksumFailure!.Message, Does.Contain("checksum"));
    }

    [Test]
    public async Task Continuation_PersistsParentAndAdvancesCumulativeTime()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        ReservoirWorld world = await Manager(repository).CreateAsync(TestData.UniformWorldRequest(8, 3, 1));
        var stateStore = new PersistentSimulationStateStore(repository, TimeProvider.System);
        SimulationRequest firstRequest = WaterfloodRun(100, null);
        SimulationExecution first = _simulator.Run(world, firstRequest);
        string firstId = await stateStore.SaveAsync(world, null, firstRequest, first);
        RestoredSimulationState restored = (await stateStore.LoadAsync(world, firstId))!;

        SimulationRequest continuationRequest = WaterfloodRun(100, firstId);
        SimulationExecution continuation = _simulator.Run(
            world, continuationRequest, restored.State, restored.SimulatedTimeSeconds);
        string continuationId = await stateStore.SaveAsync(
            world, firstId, continuationRequest, continuation);
        SimulationStateSummary metadata = (await stateStore.GetMetadataAsync(world.Summary.WorldId, continuationId))!;

        Assert.Multiple(() =>
        {
            Assert.That(continuation.Result.SimulatedTimeSeconds, Is.EqualTo(200));
            Assert.That(metadata.ParentStateId, Is.EqualTo(firstId));
            Assert.That(metadata.SimulatedTimeSeconds, Is.EqualTo(200));
            Assert.That(continuation.FinalState.WaterSaturation.Max(),
                Is.GreaterThan(first.FinalState.WaterSaturation.Max()));
        });
    }

    [Test]
    public async Task State_CorruptBlob_FailsLoudly()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        ReservoirWorld world = await Manager(repository).CreateAsync(TestData.ConditionedRequest());
        var stateStore = new PersistentSimulationStateStore(repository, TimeProvider.System);
        SimulationRequest request = ClosedRun(10);
        SimulationExecution execution = _simulator.Run(world, request);
        string stateId = await stateStore.SaveAsync(world, null, request, execution);

        await using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand();
            command.CommandText =
                "UPDATE ReservoirSimulationStates SET PressureBlob = $blob WHERE StateId = $stateId;";
            command.Parameters.AddWithValue("$blob", new byte[] { 1, 2, 3, 4 });
            command.Parameters.AddWithValue("$stateId", stateId);
            await command.ExecuteNonQueryAsync();
        }

        PersistenceIntegrityException? exception = Assert.ThrowsAsync<PersistenceIntegrityException>(
            async () => await stateStore.LoadAsync(world, stateId));
        Assert.That(exception!.Message, Does.Contain("State blob"));
    }

    [Test]
    public async Task StateRetention_KeepsNoMoreThanTwentyPerWorld()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        ReservoirWorld world = await Manager(repository).CreateAsync(TestData.UniformWorldRequest());
        var stateStore = new PersistentSimulationStateStore(repository, TimeProvider.System);

        for (int index = 1; index <= 25; index++)
        {
            SimulationRequest request = ClosedRun(index);
            SimulationExecution execution = _simulator.Run(world, request);
            await stateStore.SaveAsync(world, null, request, execution);
        }

        Assert.That(await repository.CountStatesAsync(world.Summary.WorldId), Is.EqualTo(20));
    }

    [Test]
    public async Task DeleteWorld_PurgesSpecStatesAndCache_Idempotently()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        PersistentWorldManager manager = Manager(repository);
        ReservoirWorld world = await manager.CreateAsync(TestData.ConditionedRequest());
        var stateStore = new PersistentSimulationStateStore(repository, TimeProvider.System);
        SimulationRequest request = ClosedRun(10);
        SimulationExecution execution = _simulator.Run(world, request);
        string stateId = await stateStore.SaveAsync(world, null, request, execution);

        await manager.DeleteAsync(world.Summary.WorldId);
        await manager.DeleteAsync(world.Summary.WorldId);

        ReservoirWorld? deletedWorld = await manager.GetAsync(world.Summary.WorldId);
        PersistedState? deletedState = await repository.LoadStateAsync(world.Summary.WorldId, stateId);
        int stateCount = await repository.CountStatesAsync(world.Summary.WorldId);
        Assert.Multiple(() =>
        {
            Assert.That(deletedWorld, Is.Null);
            Assert.That(deletedState, Is.Null);
            Assert.That(stateCount, Is.Zero);
        });
    }

    [Test]
    public async Task TwoSegmentSchedule_EqualsPersistedBoundaryContinuation()
    {
        string connectionString = await CreateDatabaseAsync();
        var repository = new SqliteReservoirRepository(connectionString);
        ReservoirWorld world = await Manager(repository).CreateAsync(TestData.UniformWorldRequest(8, 3, 1));
        var stateStore = new PersistentSimulationStateStore(repository, TimeProvider.System);

        SimulationRequest fullRequest = ScheduledWaterflood(200, null);
        SimulationExecution full = _simulator.Run(world, fullRequest);
        SimulationRequest firstSegmentRequest = ScheduledWaterflood(100, null);
        SimulationExecution first = _simulator.Run(world, firstSegmentRequest);
        string firstStateId = await stateStore.SaveAsync(world, null, firstSegmentRequest, first);
        RestoredSimulationState parent = (await stateStore.LoadAsync(world, firstStateId))!;
        SimulationRequest continuationRequest = ScheduledWaterflood(200, firstStateId);
        SimulationExecution continued = _simulator.Run(
            world, continuationRequest, parent.State, parent.SimulatedTimeSeconds);
        string finalStateId = await stateStore.SaveAsync(
            world, firstStateId, continuationRequest, continued);
        SimulationStateSummary metadata = (await stateStore.GetMetadataAsync(
            world.Summary.WorldId, finalStateId))!;
        WellRateSample firstContinuationSample = continued.Result.Wells
            .Single(well => well.Name == "I")
            .Samples.First();

        Assert.Multiple(() =>
        {
            Assert.That(continued.Result.SimulatedTimeSeconds, Is.EqualTo(200));
            Assert.That(metadata.ParentStateId, Is.EqualTo(firstStateId));
            Assert.That(firstContinuationSample.TimeSeconds, Is.GreaterThan(100));
            Assert.That(firstContinuationSample.SignedRatesM3PerSecond.Water,
                Is.EqualTo(0.0001).Within(1e-12));
            Assert.That(continued.FinalState.PressurePa, Is.EqualTo(full.FinalState.PressurePa).Within(1e-7));
            Assert.That(continued.FinalState.WaterSaturation,
                Is.EqualTo(full.FinalState.WaterSaturation).Within(1e-10));
            Assert.That(continued.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-6));
        });
    }


    private static async Task UpdateBlobAsync(string connectionString, string stateId, byte[] pressureBlob)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            "UPDATE ReservoirSimulationStates SET PressureBlob = $blob WHERE StateId = $stateId;";
        command.Parameters.AddWithValue("$blob", pressureBlob);
        command.Parameters.AddWithValue("$stateId", stateId);
        await command.ExecuteNonQueryAsync();
    }


    private async Task<string> CreateDatabaseAsync()
    {
        string path = Path.Combine(Path.GetTempPath(), $"drillsim-reservoir-{Guid.NewGuid():N}.db");
        _databasePaths.Add(path);
        string connectionString = $"Data Source={path}";
        var repository = new SqliteReservoirRepository(connectionString);
        await repository.InitializeAsync();
        return connectionString;
    }

    private static PersistentWorldManager Manager(SqliteReservoirRepository repository, int capacity = 4) => new(
        new ReservoirWorldFactory(), new InMemoryWorldStore(capacity), repository, TimeProvider.System);

    private static SimulationRequest ScheduledWaterflood(double duration, string? parentStateId)
    {
        var segments = new List<ScheduleSegment>
        {
            new() { StartTimeSeconds = 0, DurationSeconds = 100, Wells = ScheduledControls(0.0002) }
        };
        if (duration > 100)
            segments.Add(new ScheduleSegment
            {
                StartTimeSeconds = 100,
                DurationSeconds = duration - 100,
                Wells = ScheduledControls(0.0001)
            });
        return new SimulationRequest
        {
            DurationSeconds = duration,
            InitialTimeStepSeconds = 10,
            Solver = FixedSolver(10),
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            ContinueFromStateId = parentStateId,
            Schedule = segments
        };
    }

    private static WellControl[] ScheduledControls(double rate) =>
    [
        new WellControl
        {
            Name = "I",
            ControlMode = WellControlMode.Rate,
            TotalRateM3PerSecond = rate,
            InjectionWaterFraction = 1,
            Connections =
            [
                new WellConnection { I = 0, J = 1, K = 0, WellboreRadiusM = 0.1, OpenFraction = 1 }
            ]
        },
        new WellControl
        {
            Name = "P",
            ControlMode = WellControlMode.Rate,
            TotalRateM3PerSecond = -rate,
            Connections =
            [
                new WellConnection { I = 7, J = 1, K = 0, WellboreRadiusM = 0.1, OpenFraction = 1 }
            ]
        }
    ];


    private static SimulationRequest ClosedRun(double duration) => new()
    {
        DurationSeconds = duration,
        InitialTimeStepSeconds = duration,
        Solver = FixedSolver(duration),
        Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
        Wells = []
    };

    private static SimulationRequest WaterfloodRun(double duration, string? parentStateId) => new()
    {
        DurationSeconds = duration,
        InitialTimeStepSeconds = 10,
        Solver = FixedSolver(10),
        Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
        ContinueFromStateId = parentStateId,
        Wells =
        [
            new WellControl
            {
                Name = "I", I = 0, J = 1, KStart = 0, KEnd = 0,
                TotalRateM3PerSecond = 0.0002, InjectionWaterFraction = 1
            },
            new WellControl
            {
                Name = "P", I = 7, J = 1, KStart = 0, KEnd = 0, TotalRateM3PerSecond = -0.0002
            }
        ]
    };

    private static SolverOptions FixedSolver(double maximumTimeStep) => new()
    {
        MinimumTimeStepSeconds = 0.01,
        MaximumTimeStepSeconds = maximumTimeStep,
        MaximumSaturationChange = 0.05,
        GrowthSaturationChange = 0.01,
        TimeStepGrowthFactor = 1.5,
        TimeStepShrinkFactor = 0.5,
        CgRelativeTolerance = 1e-10,
        CgMaximumIterations = 500
    };
}
