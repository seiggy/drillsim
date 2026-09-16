using Microsoft.Data.Sqlite;
using System.Diagnostics;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class FullGridCompletionSolverTests
{
    private string? _databasePath;

    [TearDown]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        if (_databasePath is not null && File.Exists(_databasePath)) File.Delete(_databasePath);
    }

    [TestCase(-1e-5)]
    [TestCase(-1e-8)]
    public async Task FullGridGwcDeviatedCompletion_FirstProductionStepConvergesIndependentlyOfUpwindAndConnectionOrder(
        double targetRate)
    {
        Harness harness = await CreateHarnessAsync();
        WellConnection[] connections = harness.Resolved.Openings
            .SelectMany(opening => opening.Connections)
            .ToArray();
        SimulationRequest forwardRequest = ShortRun(connections, targetRate);
        SimulationRequest reversedRequest = ShortRun(connections.Reverse().ToArray(), targetRate);

        var stopwatch = Stopwatch.StartNew();
        SimulationExecution forward = new ReservoirSimulator().Run(harness.World, forwardRequest);
        SimulationExecution reversed = new ReservoirSimulator(reverseInitialUpwindForDiagnostics: true)
            .Run(harness.World, reversedRequest);
        stopwatch.Stop();
        TestContext.Out.WriteLine($"Two first-step solves at {targetRate:G3} m3/s: {stopwatch.Elapsed.TotalSeconds:F3} s");
        double produced = forward.Result.MaterialBalance.Total.CumulativeProducedM3;
        double poreVolumeFraction = produced / harness.World.PoreVolumeM3.Sum();
        TestContext.Out.WriteLine(
            $"forward accepted={forward.Result.Steps.AcceptedSteps}, rejected={forward.Result.Steps.RejectedSteps}, " +
            $"dt=[{forward.Result.Steps.MinimumAcceptedTimeStepSeconds:G6}," +
            $"{forward.Result.Steps.MaximumAcceptedTimeStepSeconds:G6}], cg={forward.Result.Steps.TotalCgIterations}, " +
            $"producedPV={poreVolumeFraction:G6}");

        Assert.Multiple(() =>
        {
            Assert.That(connections.Length, Is.GreaterThan(1));
            Assert.That(forward.Result.Steps.AcceptedSteps, Is.GreaterThan(0));
            Assert.That(reversed.Result.Steps.AcceptedSteps, Is.GreaterThan(0));
            Assert.That(forward.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
            Assert.That(reversed.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
            Assert.That(forward.FinalState.PressurePa.All(double.IsFinite), Is.True);
            Assert.That(forward.FinalState.OilSaturation.All(double.IsFinite), Is.True);
            Assert.That(MaximumDifference(forward.FinalState.PressurePa, reversed.FinalState.PressurePa),
                Is.LessThan(0.01));
            Assert.That(MaximumDifference(forward.FinalState.WaterSaturation, reversed.FinalState.WaterSaturation),
                Is.LessThan(1e-10));
            Assert.That(forward.Result.Wells[0].Samples[^1].SignedRatesM3PerSecond.Oil +
                forward.Result.Wells[0].Samples[^1].SignedRatesM3PerSecond.Water +
                forward.Result.Wells[0].Samples[^1].SignedRatesM3PerSecond.Gas,
                Is.EqualTo(targetRate).Within(1e-12));
        });
    }

    [TestCase(-1e-4, 30, Category = "CompletionDiagnosticRate1e4")]
    [TestCase(-1e-5, 30, Category = "CompletionDiagnosticRate1e5")]
    [TestCase(-1e-6, 30, Category = "CompletionDiagnosticRate1e6")]
    [TestCase(-1e-7, 30, Category = "CompletionDiagnosticRate1e7")]
    [TestCase(-1e-8, 30, Category = "CompletionDiagnosticRate1e8")]
    [TestCase(-8e-9, 45, Category = "CompletionDiagnosticP3Active")]
    [TestCase(-1e-5, 365.25, Category = "CompletionDiagnosticOneYear")]
    [TestCase(-1e-8, 365.25, Category = "CompletionDiagnosticOneYearLowRate")]
    [Explicit("Opt-in full-grid completion timestep diagnostics.")]
    [Category("FullGridCompletionDiagnostics")]
    public async Task FullGridGwcDeviatedCompletion_DiagnosticHorizon(double targetRate, double days)
    {
        Harness harness = await CreateHarnessAsync();
        WellConnection[] connections = harness.Resolved.Openings
            .SelectMany(opening => opening.Connections)
            .ToArray();
        double duration = days * 24 * 60 * 60;
        double maximumStep = targetRate == -8e-9 ? duration : Math.Min(duration, 30 * 24 * 60 * 60);
        SimulationRequest request = ShortRun(connections, targetRate) with
        {
            DurationSeconds = duration,
            InitialTimeStepSeconds = maximumStep,
            Solver = Solver(maximumStep)
        };
        var stopwatch = Stopwatch.StartNew();
        SimulationExecution execution = new ReservoirSimulator().Run(harness.World, request);
        stopwatch.Stop();
        double producedPv = execution.Result.MaterialBalance.Total.CumulativeProducedM3 /
            harness.World.PoreVolumeM3.Sum();
        double pressureOffset = execution.FinalState.PressurePa.Zip(
            harness.World.PressurePa, (final, initial) => final - initial).Average();
        double maximumPressureResidual = execution.FinalState.PressurePa.Zip(
            harness.World.PressurePa, (final, initial) => Math.Abs((final - initial) - pressureOffset)).Max();
        double maximumSaturationDelta = new[]
        {
            execution.FinalState.OilSaturation.Zip(harness.World.OilSaturation, (final, initial) => Math.Abs(final - initial)).Max(),
            execution.FinalState.WaterSaturation.Zip(harness.World.WaterSaturation, (final, initial) => Math.Abs(final - initial)).Max(),
            execution.FinalState.GasSaturation.Zip(harness.World.GasSaturation, (final, initial) => Math.Abs(final - initial)).Max()
        }.Max();
        TestContext.Out.WriteLine(
            $"rate={targetRate:G3}, days={days:G6}, elapsed={stopwatch.Elapsed.TotalSeconds:F3}s, " +
            $"accepted={execution.Result.Steps.AcceptedSteps}, rejected={execution.Result.Steps.RejectedSteps}, " +
            $"dt=[{execution.Result.Steps.MinimumAcceptedTimeStepSeconds:G6}," +
            $"{execution.Result.Steps.MaximumAcceptedTimeStepSeconds:G6}], " +
            $"cg={execution.Result.Steps.TotalCgIterations}, producedPV={producedPv:G6}, " +
            $" matrixApps={execution.Work.MatrixApplications}, " +
            $"matrix={execution.Work.MatrixTime.TotalSeconds:F3}s, preconditioner={execution.Work.PreconditionerTime.TotalSeconds:F3}s, " +
            $"upwind={execution.Work.UpwindTime.TotalSeconds:F3}s, well={execution.Work.WellTime.TotalSeconds:F3}s, " +
            $"pressureResidual={maximumPressureResidual:G6}, saturationDelta={maximumSaturationDelta:G6}");
        Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
    }


    [Test]
    [Explicit("Opt-in BHP facility-switch diagnostic.")]
    [Category("CompletionDiagnosticP3Bhp")]
    public async Task FullGridGwcDeviatedCompletion_BhpSwitchDiagnostic()
    {
        Harness harness = await CreateHarnessAsync();
        WellConnection[] connections = harness.Resolved.Openings.SelectMany(opening => opening.Connections).ToArray();
        double duration = 45 * 24 * 60 * 60;
        var request = new SimulationRequest
        {
            DurationSeconds = duration,
            InitialTimeStepSeconds = duration,
            Solver = Solver(duration),
            Fluids = new FluidModelOptions(),
            Wells =
            [
                new WellControl
                {
                    Name = "bhp-producer", ControlMode = WellControlMode.Bhp,
                    TargetBottomHolePressurePa = 1_000_000,
                    MaximumAbsoluteRateM3PerSecond = 8e-9, Connections = connections
                }
            ]
        };
        var stopwatch = Stopwatch.StartNew();
        SimulationExecution execution = new ReservoirSimulator().Run(harness.World, request);
        stopwatch.Stop();
        TestContext.Out.WriteLine(
            $"BHP switch elapsed={stopwatch.Elapsed.TotalSeconds:F3}s, accepted={execution.Result.Steps.AcceptedSteps}, " +
            $"rejected={execution.Result.Steps.RejectedSteps}, cg={execution.Result.Steps.TotalCgIterations}, " +
            $"branch={execution.Work.PressurePath}, preconditioner={execution.Work.PreconditionerTime.TotalSeconds:F3}s");
        Assert.That(execution.Result.Wells[0].Samples[^1].EffectiveControlMode, Is.EqualTo(WellControlMode.Rate));
    }


    [Test]
    [Explicit("Opt-in full-grid five-year completion-bound production integration.")]
    [Category("FullGridCompletionProduction")]
    public async Task FullGridGwcDeviatedCompletion_FiveYearProductionCompletes()
    {
        Harness harness = await CreateHarnessAsync(includeProduction: true, transitionThicknessM: 8, uniformProperties: false);
        double year = CompletionProductionService.YearSeconds;
        double activeProductionSeconds = 3 * 45 * 24 * 60 * 60;
        double targetProducedPv = 8e-9 * activeProductionSeconds / harness.World.PoreVolumeM3.Sum();
        CompletionProductionRateEnvelope envelope = CompletionProductionService.CalculateRateEnvelope(
            harness.World, harness.Resolved, 45 * 24 * 60 * 60, 0.05,
            activeProductionSeconds, targetProducedPoreVolumeFraction: targetProducedPv);
        double productionRate = envelope.SelectedRateM3PerSecond;
        var request = new CompletionProductionRequest
        {
            ProductionModelVersion = "completion-production-v1",
            InitialTimeStepSeconds = 45 * 24 * 60 * 60,
            Solver = Solver(year) with { CgRelativeTolerance = 1e-6, MaximumSamplesPerWell = 120 },
            Schedule =
            [
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 0, DurationSeconds = 45 * 24 * 60 * 60,
                    ControlMode = WellControlMode.Rate, TargetRateM3PerSecond = -productionRate
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 45 * 24 * 60 * 60,
                    DurationSeconds = year - 45 * 24 * 60 * 60, ShutIn = true
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = year, DurationSeconds = 45 * 24 * 60 * 60,
                    ControlMode = WellControlMode.Bhp, TargetBottomHolePressurePa = 1_000_000,
                    MaximumAbsoluteRateM3PerSecond = productionRate
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = year + 45 * 24 * 60 * 60,
                    DurationSeconds = 2 * year - 45 * 24 * 60 * 60, ShutIn = true
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 3 * year, DurationSeconds = 45 * 24 * 60 * 60,
                    ControlMode = WellControlMode.Rate, TargetRateM3PerSecond = -productionRate
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 3 * year + 45 * 24 * 60 * 60,
                    DurationSeconds = 2 * year - 45 * 24 * 60 * 60, ShutIn = true
                }
            ]
        };

        var stopwatch = Stopwatch.StartNew();
        CompletionProductionResult result = await harness.Production!.RunAsync(
            harness.World, harness.Completion.CompletionBindingId, request);
        stopwatch.Stop();
        PhaseVolumes produced = result.Checkpoints[^1].CumulativeProducedM3;
        double producedPv = (produced.Oil + produced.Water + produced.Gas) / harness.World.PoreVolumeM3.Sum();
        TestContext.Out.WriteLine(
            $"full completion five-year elapsed={stopwatch.Elapsed.TotalSeconds:F3}s, " +
            $"rate={productionRate:G6} m3/s, CFLmax={envelope.RecommendedMaximumRateM3PerSecond:G6}, " +
            $"targetPV={envelope.TargetProducedPoreVolumeFraction:G6}, producedPV={producedPv:G6}");
        foreach (CheckpointDiagnostic checkpoint in harness.CheckpointDiagnostics)
            TestContext.Out.WriteLine(
                $"year={checkpoint.Year}, accepted={checkpoint.Steps.AcceptedSteps}, rejected={checkpoint.Steps.RejectedSteps}, " +
                $"branch={checkpoint.Work.PressurePath}, " +
                $"cg={checkpoint.Steps.TotalCgIterations}, matrixApps={checkpoint.Work.MatrixApplications}, " +
                $"matrix={checkpoint.Work.MatrixTime.TotalSeconds:F3}s, " +
                $"preconditioner={checkpoint.Work.PreconditionerTime.TotalSeconds:F3}s, " +
                $"upwind={checkpoint.Work.UpwindTime.TotalSeconds:F3}s, well={checkpoint.Work.WellTime.TotalSeconds:F3}s");

        Assert.Multiple(() =>
        {
            Assert.That(result.Checkpoints, Has.Count.EqualTo(3));
            Assert.That(produced.Water, Is.GreaterThan(0));
            Assert.That(produced.Gas, Is.GreaterThan(0));
            Assert.That(producedPv, Is.GreaterThan(1e-12));
            Assert.That(productionRate, Is.EqualTo(8e-9).Within(1e-18));
            Assert.That(productionRate, Is.LessThanOrEqualTo(envelope.RecommendedMaximumRateM3PerSecond));
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(60)));
            Assert.That(result.MonthlyTruth, Has.Count.EqualTo(60));
            Assert.That(result.MonthlyTruth.Any(month => month.SwitchReason == "Shut-in"), Is.True);
            Assert.That(result.MonthlyTruth.Any(month => month.SwitchReason == "Maximum rate constraint"), Is.True);
            Assert.That(result.Checkpoints.All(checkpoint => checkpoint.MaximumBalanceErrorFraction <= 1e-6),
                Is.True);
        });
    }

    private async Task<Harness> CreateHarnessAsync(
        bool includeProduction = false, double transitionThicknessM = 8,
        bool uniformProperties = false)
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-full-completion-{Guid.NewGuid():N}.db");
        string connectionString = $"Data Source={_databasePath}";
        var stateRepository = new SqliteReservoirRepository(connectionString);
        await stateRepository.InitializeAsync();
        var pathRepository = new SqliteTruthSamplingRepository(connectionString);
        await pathRepository.InitializeAsync();
        var completionRepository = new SqliteCompletionBindingRepository(connectionString);
        await completionRepository.InitializeAsync();
        var productionRepository = new SqliteCompletionProductionRepository(connectionString);
        await productionRepository.InitializeAsync();
        var worldManager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(2), stateRepository, TimeProvider.System);
        ReservoirWorld world = await worldManager.CreateAsync(
            WorldRequest(transitionThicknessM, uniformProperties));
        var pathService = new RestrictedTruthSamplingService(pathRepository, TimeProvider.System);
        ApprovedPathBindingMetadata path = await pathService.RegisterAsync(world, PathRequest());
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
                        OpeningId = "deviated-open-hole", ReservoirName = "Full GWC",
                        Type = CompletionOpeningType.OpenHole,
                        TopMeasuredDepthM = 0, BaseMeasuredDepthM = 2_000,
                        WellboreRadiusM = 0.1, Skin = 0, Efficiency = 1, UncertaintyM = 0
                    }
                ]
            });
        ResolvedCompletionBinding resolved = (await completionService.ResolveAsync(
            world, completion.CompletionBindingId))!;
        var checkpointDiagnostics = new List<CheckpointDiagnostic>();
        CompletionProductionService? production = includeProduction
            ? new CompletionProductionService(
                productionRepository, stateRepository, completionService,
                new PersistentSimulationStateStore(stateRepository, TimeProvider.System),
                new ReservoirSimulator(), TimeProvider.System,
                (year, execution) =>
                {
                    checkpointDiagnostics.Add(new CheckpointDiagnostic(
                        year, execution.Result.Steps, execution.Work));
                    TestContext.Out.WriteLine($"checkpoint year {year} completed");
                })
            : null;
        return new Harness(world, completion, resolved, production, checkpointDiagnostics);
    }

    private static SimulationRequest ShortRun(
        IReadOnlyList<WellConnection> connections, double targetRate) => new()
        {
            DurationSeconds = 3_600,
            InitialTimeStepSeconds = 3_600,
            Solver = Solver(3_600) with { CgRelativeTolerance = 1e-8 },
            Fluids = new FluidModelOptions(),
            Wells =
        [
            new WellControl
            {
                Name = "realistic-producer", ControlMode = WellControlMode.Rate,
                TotalRateM3PerSecond = targetRate, Connections = connections
            }
        ]
        };

    private static SolverOptions Solver(double maximumTimeStep) => new()
    {
        MinimumTimeStepSeconds = 60,
        MaximumTimeStepSeconds = maximumTimeStep,
        MaximumSaturationChange = 0.05,
        GrowthSaturationChange = 0.005,
        TimeStepGrowthFactor = 1.5,
        TimeStepShrinkFactor = 0.5,
        CgRelativeTolerance = 1e-6,
        CgMaximumIterations = 1_000,
        MaximumStepAttempts = 10_000,
        MaximumSamplesPerWell = 120
    };

    private static WorldGenerationRequest WorldRequest(
        double transitionThicknessM, bool uniformProperties)
    {
        ConditioningPoint Property(double easting, double northing, int index) => new()
        {
            EastingM = easting,
            NorthingM = northing,
            ReservoirTopDepthM = 1_500,
            ReservoirBaseDepthM = 1_590,
            Porosity = uniformProperties ? 0.22 : 0.22 + 0.005 * index,
            PermeabilityM2 = uniformProperties ? 5e-13 : 5e-13 * (index + 1),
            PressurePa = 28_000_000,
            WaterSaturation = 0.2,
            GasSaturation = 0.05,
            NetToGross = 0.7
        };
        StructuralConditioningPoint[] structure = Enumerable.Range(0, 46)
            .Select(index => new StructuralConditioningPoint
            {
                EastingM = (index % 29 % 6) * 1_400,
                NorthingM = (index % 29 / 6) * 1_400,
                ReservoirTopDepthM = 1_500,
                ReservoirBaseDepthM = 1_590
            }).ToArray();
        ConditioningPoint[] properties = Enumerable.Range(0, 9)
            .Select(index => Property((index % 3) * 3_500, (index / 3) * 2_800, index))
            .ToArray();
        return new WorldGenerationRequest
        {
            FieldId = Guid.Parse("5eaac03a-41c7-4686-b54e-dbe41c81cd6a"),
            ReservoirName = "Full GWC",
            Seed = 314159,
            CalibrationArtifact = TestData.CalibrationArtifact(),
            Grid = new GridOptions { CountX = 64, CountY = 64, CountZ = 20, HorizontalPaddingM = 250 },
            Heterogeneity = uniformProperties ? TestData.ZeroHeterogeneity() : RepresentativeHeterogeneity(),
            StructuralConditioningPoints = structure,
            ConditioningPoints = properties,
            FluidContacts = new FluidContactOptions
            {
                TransitionThicknessM = transitionThicknessM,
                GasWater =
                [
                    Contact(0, 0, 1_545), Contact(7_000, 0, 1_545),
                    Contact(0, 5_600, 1_545), Contact(7_000, 5_600, 1_545)
                ]
            }
        };
    }

    private static HeterogeneityOptions RepresentativeHeterogeneity() => new()
    {
        IdwPower = 2,
        SpectralModeCount = 24,
        CorrelationLengthXM = 1_400,
        CorrelationLengthYM = 1_400,
        CorrelationLengthZM = 12,
        ControlFadeDistanceM = 1_200,
        TopDepthStdDevM = 4,
        BaseDepthStdDevM = 4,
        PorosityStdDev = 0.025,
        LogPermeabilityStdDev = 0.7,
        PressureStdDevPa = 500_000,
        WaterSaturationStdDev = 0.025,
        GasSaturationStdDev = 0.015,
        NetToGrossStdDev = 0.12,
        ShalePorosity = 0.05,
        ShalePermeabilityM2 = 1e-20
    };


    private static ApprovedPathBindingRequest PathRequest() => new()
    {
        ScenarioId = Guid.Parse("3ae5465f-dfc7-42b5-9b4e-dd949ac83c12"),
        RunId = Guid.Parse("bff49b93-81bb-4064-9f1d-54178f21e243"),
        PathKind = ApprovedPathKind.AsDrilled,
        ApprovedSealedPredictionSha256 =
            "abababababababababababababababababababababababababababababababab",
        Stations =
        [
            Station(0, 1_000, 1_000, 1_502),
            Station(1_000, 3_000, 2_500, 1_545),
            Station(2_000, 5_000, 4_000, 1_588)
        ]
    };

    private static ApprovedPathStation Station(
        double md, double easting, double northing, double tvd) => new()
        {
            MeasuredDepthM = md,
            EastingM = easting,
            NorthingM = northing,
            TrueVerticalDepthM = tvd
        };

    private static FluidContactPoint Contact(double easting, double northing, double depth) => new()
    {
        EastingM = easting,
        NorthingM = northing,
        ContactDepthTvdM = depth
    };

    private static double MaximumDifference(double[] left, double[] right) =>
        left.Zip(right, (first, second) => Math.Abs(first - second)).Max();

    private sealed record CheckpointDiagnostic(
        int Year, SimulationStepDiagnostics Steps, NumericalWorkDiagnostics Work);

    private sealed record Harness(
        ReservoirWorld World,
        ApprovedCompletionBindingMetadata Completion,
        ResolvedCompletionBinding Resolved,
        CompletionProductionService? Production,
        List<CheckpointDiagnostic> CheckpointDiagnostics);
}
