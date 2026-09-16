using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class ReservoirSimulatorTests
{
    private readonly ReservoirSimulator _simulator = new();

    [Test]
    public void Run_ClosedUniformWorldWithoutWells_RemainsUnchanged()
    {
        ReservoirWorld world = TestData.UniformWorld(countZ: 2);
        var request = new SimulationRequest
        {
            DurationSeconds = 1_000,
            InitialTimeStepSeconds = 100,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 1,
                MaximumTimeStepSeconds = 500,
                MaximumSaturationChange = 0.05,
                GrowthSaturationChange = 0.01,
                TimeStepGrowthFactor = 2,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-10,
                CgMaximumIterations = 500
            },
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells = []
        };

        SimulationExecution execution = _simulator.Run(world, request);

        Assert.Multiple(() =>
        {
            Assert.That(execution.FinalState.PressurePa, Is.EqualTo(world.PressurePa).Within(1e-9));
            Assert.That(execution.FinalState.OilSaturation, Is.EqualTo(world.OilSaturation).Within(1e-12));
            Assert.That(execution.FinalState.WaterSaturation, Is.EqualTo(world.WaterSaturation).Within(1e-12));
            Assert.That(execution.FinalState.GasSaturation, Is.EqualTo(world.GasSaturation).Within(1e-12));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-12));
            Assert.That(execution.Result.Steps.RejectedSteps, Is.Zero);
            Assert.That(execution.Result.StateRanges.MaximumSaturationClosureError, Is.LessThan(1e-14));
        });
    }

    [Test]
    public void Run_WaterInjectorAndProducer_AdvancesWaterWithBoundedBalance()
    {
        ReservoirWorld world = TestData.UniformWorld(8, 3, 1);
        var request = new SimulationRequest
        {
            DurationSeconds = 10_000,
            InitialTimeStepSeconds = 5_000,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 10,
                MaximumTimeStepSeconds = 5_000,
                MaximumSaturationChange = 0.0002,
                GrowthSaturationChange = 0.00004,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-12,
                CgMaximumIterations = 500
            },
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells =
            [
                new WellControl
                {
                    Name = "I-1", I = 0, J = 1, KStart = 0, KEnd = 0,
                    TotalRateM3PerSecond = 0.002,
                    InjectionWaterFraction = 1,
                    InjectionGasFraction = 0
                },
                new WellControl
                {
                    Name = "P-1", I = 7, J = 1, KStart = 0, KEnd = 0,
                    TotalRateM3PerSecond = -0.002
                }
            ]
        };

        SimulationExecution execution = _simulator.Run(world, request);
        WellTimeSeries injector = execution.Result.Wells.Single(series => series.Name == "I-1");
        WellTimeSeries producer = execution.Result.Wells.Single(series => series.Name == "P-1");

        PhaseMaterialBalance[] balances =
        [
            execution.Result.MaterialBalance.Oil,
            execution.Result.MaterialBalance.Water,
            execution.Result.MaterialBalance.Gas,
            execution.Result.MaterialBalance.Total
        ];
        double worstBalanceFraction = balances.Max(balance => Math.Max(
            balance.InPlaceNormalizedBalanceErrorFraction,
            balance.ThroughputNormalizedBalanceErrorFraction));

        Assert.Multiple(() =>
        {
            Assert.That(execution.FinalState.WaterSaturation.Max(), Is.GreaterThan(world.WaterSaturation.Max()));
            Assert.That(execution.Result.Steps.AcceptedSteps, Is.GreaterThan(1));
            Assert.That(execution.Result.Steps.RejectedSteps, Is.GreaterThan(0));
            Assert.That(execution.Result.Steps.TotalCgIterations, Is.GreaterThan(0));
            Assert.That(injector.Samples[^1].CumulativeInjectedM3.Water, Is.EqualTo(20).Within(1e-9));
            Assert.That(producer.Samples.Any(sample => sample.SignedRatesM3PerSecond.Oil < 0), Is.True);
            Assert.That(producer.Samples.Any(sample => sample.SignedRatesM3PerSecond.Water < 0), Is.True);
            Assert.That(producer.Samples[^1].CumulativeProducedM3.Oil, Is.GreaterThan(0));
            Assert.That(execution.Result.MaterialBalance.Total.ThroughputNormalizedBalanceErrorFraction, Is.LessThan(1e-9));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.EqualTo(worstBalanceFraction));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-7));
            Assert.That(execution.Result.StateRanges.PressurePa.Minimum, Is.GreaterThan(0));
            Assert.That(execution.Result.StateRanges.MaximumSaturationClosureError, Is.LessThan(1e-12));
            Assert.That(AllFiniteAndClosed(execution.FinalState), Is.True);
            Assert.That(execution.Result.KernelName, Does.Contain("Immiscible three-phase"));
            Assert.That(execution.Result.ModelLimitation, Does.Contain("no compositional"));
        });
    }

    private static bool AllFiniteAndClosed(SimulationState state)
    {
        for (int cell = 0; cell < state.PressurePa.Length; cell++)
        {
            if (!double.IsFinite(state.PressurePa[cell]) ||
                !double.IsFinite(state.OilSaturation[cell]) ||
                !double.IsFinite(state.WaterSaturation[cell]) ||
                !double.IsFinite(state.GasSaturation[cell]) ||
                Math.Abs(state.OilSaturation[cell] + state.WaterSaturation[cell] + state.GasSaturation[cell] - 1) > 1e-12)
                return false;
        }
        return true;
    }
}
