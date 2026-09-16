using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class RobustnessTests
{
    private readonly ReservoirSimulator _simulator = new();

    [Test]
    public void Run_HydrostaticWaterColumnWithGravity_RemainsAtEquilibrium()
    {
        const double gravity = 9.80665;
        const double waterDensity = 1_000;
        ReservoirWorld world = TestData.UniformWorld(1, 1, 4);
        double datumDepth = world.CellDepthM[0];
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
        {
            world.OilSaturation[cell] = 0;
            world.WaterSaturation[cell] = 1;
            world.GasSaturation[cell] = 0;
            world.PressurePa[cell] = 25_000_000 +
                waterDensity * gravity * (world.CellDepthM[cell] - datumDepth);
        }
        double[] initialPressure = (double[])world.PressurePa.Clone();

        var request = new SimulationRequest
        {
            DurationSeconds = 1_000,
            InitialTimeStepSeconds = 100,
            Solver = FixedStepSolver(100),
            Fluids = new FluidModelOptions
            {
                GravityMPerS2 = gravity,
                WaterDensityKgPerM3 = waterDensity,
                OilCompressibilityPerPa = 0,
                WaterCompressibilityPerPa = 0,
                GasCompressibilityPerPa = 0,
                RockCompressibilityPerPa = 5e-10
            },
            Wells = []
        };

        SimulationExecution execution = _simulator.Run(world, request);

        Assert.Multiple(() =>
        {
            Assert.That(execution.FinalState.PressurePa, Is.EqualTo(initialPressure).Within(1e-7));
            Assert.That(execution.FinalState.WaterSaturation.Max(value => Math.Abs(value - 1)), Is.LessThan(1e-10));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-12));
            Assert.That(execution.Result.Steps.RejectedSteps, Is.Zero);
        });
    }

    [Test]
    public void Run_MaximumStepAttempts_StopsBeforeUnboundedWork()
    {
        ReservoirWorld world = TestData.UniformWorld();
        var solver = FixedStepSolver(10) with { MaximumStepAttempts = 3 };
        var request = new SimulationRequest
        {
            DurationSeconds = 100,
            InitialTimeStepSeconds = 10,
            Solver = solver,
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells =
            [
                new WellControl
                {
                    Name = "budget-injector", I = 0, J = 0, KStart = 0, KEnd = 0,
                    TotalRateM3PerSecond = 1e-8, InjectionWaterFraction = 1
                },
                new WellControl
                {
                    Name = "budget-producer", I = world.Grid.CountX - 1, J = 0, KStart = 0, KEnd = 0,
                    TotalRateM3PerSecond = -1e-8
                }
            ]
        };

        SimulationFailureException? exception = Assert.Throws<SimulationFailureException>(
            () => _simulator.Run(world, request));

        Assert.That(exception!.Message, Does.Contain("step-attempt budget"));
    }

    [Test]
    public void Run_MaximumSamplesPerWell_TimeAggregatesWithoutLosingVolumes()
    {
        ReservoirWorld world = TestData.UniformWorld(8, 3, 1);
        var solver = FixedStepSolver(10) with
        {
            MaximumStepAttempts = 200,
            MaximumSamplesPerWell = 3
        };
        var request = new SimulationRequest
        {
            DurationSeconds = 1_000,
            InitialTimeStepSeconds = 10,
            Solver = solver,
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells =
            [
                new WellControl
                {
                    Name = "I", I = 0, J = 1, KStart = 0, KEnd = 0,
                    TotalRateM3PerSecond = 0.0002, InjectionWaterFraction = 1
                },
                new WellControl
                {
                    Name = "P", I = 7, J = 1, KStart = 0, KEnd = 0,
                    TotalRateM3PerSecond = -0.0002
                }
            ]
        };

        SimulationExecution execution = _simulator.Run(world, request);
        WellTimeSeries injector = execution.Result.Wells.Single(well => well.Name == "I");

        Assert.Multiple(() =>
        {
            Assert.That(execution.Result.Steps.AcceptedSteps, Is.EqualTo(100));
            Assert.That(injector.Samples, Has.Count.EqualTo(3));
            Assert.That(injector.Samples[^1].TimeSeconds, Is.EqualTo(1_000).Within(1e-10));
            Assert.That(injector.Samples[^1].CumulativeInjectedM3.Water, Is.EqualTo(0.2).Within(1e-12));
            Assert.That(injector.Samples.Sum(sample => sample.TimeStepSeconds), Is.EqualTo(1_000).Within(1e-10));
        });
    }

    private static SolverOptions FixedStepSolver(double timeStep) => new()
    {
        MinimumTimeStepSeconds = 0.1,
        MaximumTimeStepSeconds = timeStep,
        MaximumSaturationChange = 0.05,
        GrowthSaturationChange = 0.01,
        TimeStepGrowthFactor = 1.5,
        TimeStepShrinkFactor = 0.5,
        CgRelativeTolerance = 1e-10,
        CgMaximumIterations = 500
    };
}
