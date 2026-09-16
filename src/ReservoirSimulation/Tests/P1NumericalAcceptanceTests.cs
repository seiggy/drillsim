using System.Diagnostics;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
[Category("P1Acceptance")]
public sealed class P1NumericalAcceptanceTests
{
    private const double YearSeconds = 365.25 * 24 * 60 * 60;

    [Test]
    public void DefaultPvtGwcContactEquilibrium_TenYearsWithGravity_DriftsLessThanOnePercentPoreVolumePerPhase()
    {
        ReservoirWorld world = CreateContactWorld(4, 3, 10);
        double[] initialOil = (double[])world.OilSaturation.Clone();
        double[] initialWater = (double[])world.WaterSaturation.Clone();
        double[] initialGas = (double[])world.GasSaturation.Clone();
        SimulationExecution execution = new ReservoirSimulator().Run(world, EquilibriumRun(10 * YearSeconds));
        double totalPoreVolume = world.PoreVolumeM3.Sum();

        Assert.Multiple(() =>
        {
            Assert.That(PhaseDrift(world, initialOil, execution.FinalState.OilSaturation) / totalPoreVolume,
                Is.LessThan(0.01));
            Assert.That(PhaseDrift(world, initialWater, execution.FinalState.WaterSaturation) / totalPoreVolume,
                Is.LessThan(0.01));
            Assert.That(PhaseDrift(world, initialGas, execution.FinalState.GasSaturation) / totalPoreVolume,
                Is.LessThan(0.01));
            Assert.That(execution.Result.SimulatedTimeSeconds, Is.EqualTo(10 * YearSeconds).Within(1e-6));
        });
    }

    [Test]
    public void DefaultPvtGwcContactEquilibrium_OneYear_PressureChangesLessThanOnePercentAndBalancesWithinOnePpm()
    {
        ReservoirWorld world = CreateContactWorld(4, 3, 10);
        double[] initialPressure = (double[])world.PressurePa.Clone();

        SimulationExecution execution = new ReservoirSimulator().Run(world, EquilibriumRun(YearSeconds));
        double maximumRelativePressureChange = initialPressure.Zip(
            execution.FinalState.PressurePa,
            (initial, final) => Math.Abs(final - initial) / initial).Max();

        Assert.Multiple(() =>
        {
            Assert.That(maximumRelativePressureChange, Is.LessThan(0.01));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
        });
    }

    [Test]
    public void RepresentativeFiveYearScheduledWaterflood_CompletesQuicklyAndBalancesWithinOnePpm()
    {
        ReservoirWorld world = CreateContactWorld(12, 8, 4);
        InitializeRuntimeOilWaterState(world);
        SimulationRequest request = FiveYearSchedule(world, injectedPoreVolumeFraction: 0.01);

        var stopwatch = Stopwatch.StartNew();
        SimulationExecution execution = new ReservoirSimulator().Run(world, request);
        stopwatch.Stop();
        TestContext.Out.WriteLine($"Reduced-grid five-year run: {stopwatch.Elapsed.TotalMilliseconds:F1} ms");

        Assert.Multiple(() =>
        {
            Assert.That(execution.Result.SimulatedTimeSeconds, Is.EqualTo(5 * YearSeconds).Within(1e-6));
            Assert.That(execution.Result.Steps.AcceptedSteps, Is.GreaterThan(0));
            Assert.That(execution.Result.MaterialBalance.Total.InPlaceNormalizedBalanceErrorFraction,
                Is.LessThanOrEqualTo(1e-6));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(10)));
        });
    }

    [Test]
    [Explicit("Opt-in uniform one-year pressure branch diagnostic.")]
    [Category("UniformOneYearDiagnostic")]
    public void UniformFullGrid_OneYearPressureBranchDiagnostic()
    {
        ReservoirWorld world = CreateContactWorld(64, 64, 20);
        InitializeRuntimeOilWaterState(world);
        double rate = 0.000005 * world.PoreVolumeM3.Sum() / (5 * YearSeconds);
        var request = new SimulationRequest
        {
            DurationSeconds = YearSeconds,
            InitialTimeStepSeconds = YearSeconds,
            Solver = Solver(YearSeconds),
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Schedule =
            [
                new ScheduleSegment
                {
                    StartTimeSeconds = 0, DurationSeconds = YearSeconds,
                    Wells = ScheduledWells(world, rate)
                }
            ]
        };
        var stopwatch = Stopwatch.StartNew();
        SimulationExecution execution = new ReservoirSimulator().Run(world, request);
        stopwatch.Stop();
        TestContext.Out.WriteLine(
            $"uniform one-year elapsed={stopwatch.Elapsed.TotalSeconds:F3}s, " +
            $"accepted={execution.Result.Steps.AcceptedSteps}, rejected={execution.Result.Steps.RejectedSteps}, " +
            $"cg={execution.Result.Steps.TotalCgIterations}, matrixApps={execution.Work.MatrixApplications}, " +
            $"matrix={execution.Work.MatrixTime.TotalSeconds:F3}s, " +
            $"preconditioner={execution.Work.PreconditionerTime.TotalSeconds:F3}s");
        Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
    }


    [Test]
    [Explicit("Opt-in 81,920-cell five-year runtime acceptance benchmark.")]
    [Category("FullGridBenchmark")]
    public void FullGrid81920CellFiveYearSchedule_CompletesWithinSixtySeconds()
    {
        ReservoirWorld world = CreateContactWorld(64, 64, 20);
        InitializeRuntimeOilWaterState(world);
        SimulationRequest request = FiveYearSchedule(world, injectedPoreVolumeFraction: 0.000005);

        var stopwatch = Stopwatch.StartNew();
        SimulationExecution execution = new ReservoirSimulator().Run(world, request);
        stopwatch.Stop();
        TestContext.Out.WriteLine($"Full-grid five-year run: {stopwatch.Elapsed.TotalSeconds:F3} s");
        TestContext.Out.WriteLine(
            $"branch={execution.Work.PressurePath}, cg={execution.Result.Steps.TotalCgIterations}, " +
            $"matrixApps={execution.Work.MatrixApplications}");

        Assert.Multiple(() =>
        {
            Assert.That(execution.Result.SimulatedTimeSeconds, Is.EqualTo(5 * YearSeconds).Within(1e-6));
            Assert.That(execution.Result.Steps.AcceptedSteps, Is.GreaterThan(0));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThanOrEqualTo(1e-6));
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(60)));
        });
    }

    private static SimulationRequest EquilibriumRun(double duration) => new()
    {
        DurationSeconds = duration,
        InitialTimeStepSeconds = YearSeconds,
        Solver = Solver(maximumTimeStep: YearSeconds),
        Fluids = new FluidModelOptions(),
        Wells = []
    };

    private static SimulationRequest FiveYearSchedule(
        ReservoirWorld world, double injectedPoreVolumeFraction)
    {
        double baseRate = injectedPoreVolumeFraction * world.PoreVolumeM3.Sum() / (5 * YearSeconds);
        double[] factors = [0.8, 1.0, 1.2, 1.0, 1.0];
        var segments = new ScheduleSegment[5];
        for (int year = 0; year < segments.Length; year++)
        {
            double rate = baseRate * factors[year];
            segments[year] = new ScheduleSegment
            {
                StartTimeSeconds = year * YearSeconds,
                DurationSeconds = YearSeconds,
                Wells = ScheduledWells(world, rate)
            };
        }
        return new SimulationRequest
        {
            DurationSeconds = 5 * YearSeconds,
            InitialTimeStepSeconds = YearSeconds,
            Solver = Solver(maximumTimeStep: YearSeconds),
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Schedule = segments
        };
    }

    private static WellControl[] ScheduledWells(ReservoirWorld world, double rate)
    {
        WellConnection[] injectorConnections = Enumerable.Range(0, world.Grid.CountY)
            .SelectMany(j => Enumerable.Range(0, world.Grid.CountZ).Select(k => new WellConnection
            {
                I = 0,
                J = j,
                K = k,
                WellboreRadiusM = 0.1,
                OpenFraction = 1
            }))
            .ToArray();
        WellConnection[] producerConnections = Enumerable.Range(0, world.Grid.CountY)
            .SelectMany(j => Enumerable.Range(0, world.Grid.CountZ).Select(k => new WellConnection
            {
                I = world.Grid.CountX - 1,
                J = j,
                K = k,
                WellboreRadiusM = 0.1,
                OpenFraction = 1
            }))
            .ToArray();
        return
        [
            new WellControl
            {
                Name = "I-5Y", ControlMode = WellControlMode.Rate,
                TotalRateM3PerSecond = rate, InjectionWaterFraction = 1,
                Connections = injectorConnections
            },
            new WellControl
            {
                Name = "P-5Y", ControlMode = WellControlMode.Rate,
                TotalRateM3PerSecond = -rate, Connections = producerConnections
            }
        ];
    }

    private static SolverOptions Solver(double maximumTimeStep) => new()
    {
        MinimumTimeStepSeconds = 60,
        MaximumTimeStepSeconds = maximumTimeStep,
        MaximumSaturationChange = 0.05,
        GrowthSaturationChange = 0.005,
        TimeStepGrowthFactor = 1.5,
        TimeStepShrinkFactor = 0.5,
        CgRelativeTolerance = 1e-8,
        CgMaximumIterations = 1_000,
        MaximumStepAttempts = 10_000,
        MaximumSamplesPerWell = 100
    };

    private static ReservoirWorld CreateContactWorld(int countX, int countY, int countZ)
    {
        ConditioningPoint Property(double easting, double northing) => new()
        {
            EastingM = easting,
            NorthingM = northing,
            ReservoirTopDepthM = 1_000,
            ReservoirBaseDepthM = 1_100,
            Porosity = 0.22,
            PermeabilityM2 = 1e-11,
            PressurePa = 30_000_000,
            WaterSaturation = 0.2,
            GasSaturation = 0.05,
            NetToGross = 1
        };
        ReservoirWorld world = new ReservoirWorldFactory().Create(new WorldGenerationRequest
        {
            FieldId = Guid.Parse("04e36bf7-51f8-4569-97f8-78187e86e4d5"),
            ReservoirName = $"P1 acceptance {countX}x{countY}x{countZ}",
            Seed = 42,
            CalibrationArtifact = TestData.CalibrationArtifact(),
            Grid = new GridOptions
            {
                CountX = countX,
                CountY = countY,
                CountZ = countZ,
                HorizontalPaddingM = 50
            },
            Heterogeneity = TestData.ZeroHeterogeneity(),
            StructuralConditioningPoints =
            [
                new StructuralConditioningPoint
                {
                    EastingM = 0, NorthingM = 0,
                    ReservoirTopDepthM = 1_000, ReservoirBaseDepthM = 1_100
                },
                new StructuralConditioningPoint
                {
                    EastingM = 700, NorthingM = 700,
                    ReservoirTopDepthM = 1_000, ReservoirBaseDepthM = 1_100
                }
            ],
            ConditioningPoints = [Property(0, 0), Property(700, 700)],
            FluidContacts = new FluidContactOptions
            {
                TransitionThicknessM = 10,
                GasWater = [new FluidContactPoint { EastingM = 0, NorthingM = 0, ContactDepthTvdM = 1_050 }]
            }
        });
        return world;
    }

    private static void InitializeRuntimeOilWaterState(ReservoirWorld world)
    {
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
        {
            world.PressurePa[cell] = 30_000_000;
            world.OilSaturation[cell] = 0.8;
            world.WaterSaturation[cell] = 0.2;
            world.GasSaturation[cell] = 0;
        }
    }


    private static double PhaseDrift(ReservoirWorld world, double[] initial, double[] final)
    {
        double drift = 0;
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
            drift += world.PoreVolumeM3[cell] * Math.Abs(final[cell] - initial[cell]);
        return drift;
    }
}
