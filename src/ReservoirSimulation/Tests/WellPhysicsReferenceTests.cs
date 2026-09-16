using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class WellPhysicsReferenceTests
{
    [Test]
    public void PeacemanIndex_MatchesThiemSinglePhaseSteadyRadialLimitWithinTwoPercent()
    {
        const double kx = 2e-13;
        const double ky = 8e-13;
        const double dx = 40;
        const double dy = 60;
        const double height = 12;
        const double radius = 0.1;
        const double skin = 1.5;
        const double openFraction = 0.75;
        const double viscosity = 0.001;
        const double rate = 0.002;

        double wellIndex = PeacemanWellModel.ComputeWellIndex(
            kx, ky, dx, dy, height, radius, skin, openFraction);
        double peacemanPressureDrop = rate / (wellIndex / viscosity);
        double equivalentRadius = 0.28 * Math.Sqrt(
            Math.Sqrt(ky / kx) * dx * dx + Math.Sqrt(kx / ky) * dy * dy) /
            (Math.Pow(ky / kx, 0.25) + Math.Pow(kx / ky, 0.25));
        double thiemPressureDrop = rate * viscosity *
            (Math.Log(equivalentRadius / radius) + skin) /
            (2 * Math.PI * Math.Sqrt(kx * ky) * height * openFraction);

        Assert.That(peacemanPressureDrop, Is.EqualTo(thiemPressureDrop).Within(0.02 * thiemPressureDrop));
    }

    [TestCase(-0.002, 24_950_000, null, "Minimum BHP constraint")]
    [TestCase(0.002, null, 25_050_000, "Maximum BHP constraint")]
    public void RateControl_SwitchesDeterministicallyAtBhpConstraints(
        double targetRate, double? minimumBhp, double? maximumBhp, string expectedReason)
    {
        ReservoirWorld world = TestData.UniformWorld(4, 3, 1);
        var well = new WellControl
        {
            Name = "W",
            ControlMode = WellControlMode.Rate,
            TotalRateM3PerSecond = targetRate,
            MinimumBottomHolePressurePa = minimumBhp,
            MaximumBottomHolePressurePa = maximumBhp,
            InjectionWaterFraction = 1,
            Connections = [Connection(1, 1, 0)]
        };

        WellRateSample sample = RunSingleStep(world, well);

        Assert.Multiple(() =>
        {
            Assert.That(sample.RequestedControlMode, Is.EqualTo(WellControlMode.Rate));
            Assert.That(sample.EffectiveControlMode, Is.EqualTo(WellControlMode.Bhp));
            Assert.That(sample.SwitchReason, Is.EqualTo(expectedReason));
            if (minimumBhp is not null)
                Assert.That(sample.BottomHolePressurePa, Is.EqualTo(minimumBhp.Value));
            if (maximumBhp is not null)
                Assert.That(sample.BottomHolePressurePa, Is.EqualTo(maximumBhp.Value));
        });
    }

    [Test]
    public void BhpControl_SwitchesToRateAtMaximumRateConstraint()
    {
        ReservoirWorld world = TestData.UniformWorld(4, 3, 1);
        var well = new WellControl
        {
            Name = "P",
            ControlMode = WellControlMode.Bhp,
            TargetBottomHolePressurePa = 20_000_000,
            MaximumAbsoluteRateM3PerSecond = 0.0005,
            Connections = [Connection(1, 1, 0)]
        };

        WellRateSample sample = RunSingleStep(world, well);
        double totalRate = sample.SignedRatesM3PerSecond.Oil +
            sample.SignedRatesM3PerSecond.Water + sample.SignedRatesM3PerSecond.Gas;

        Assert.Multiple(() =>
        {
            Assert.That(sample.RequestedControlMode, Is.EqualTo(WellControlMode.Bhp));
            Assert.That(sample.EffectiveControlMode, Is.EqualTo(WellControlMode.Rate));
            Assert.That(sample.SwitchReason, Is.EqualTo("Maximum rate constraint"));
            Assert.That(Math.Abs(totalRate), Is.EqualTo(0.0005).Within(1e-12));
        });
    }

    [Test]
    public void RateControl_MultipleConnectionsAllocateByPeacemanProductivity()
    {
        ReservoirWorld world = TestData.UniformWorld(1, 1, 2);
        var well = new WellControl
        {
            Name = "I",
            ControlMode = WellControlMode.Rate,
            TotalRateM3PerSecond = 0.001,
            InjectionWaterFraction = 1,
            Connections =
            [
                Connection(0, 0, 0, openFraction: 0.25),
                Connection(0, 0, 1, openFraction: 1)
            ]
        };
        PhaseState phaseState = FluidPhysics.Evaluate(
            world.PressurePa, world.OilSaturation, world.WaterSaturation, world.GasSaturation,
            new FluidModelOptions());

        WellPreparation preparation = PeacemanWellModel.Prepare(
            world, [well], phaseState.Mobility, world.PressurePa);
        int first = world.Grid.CellIndex(0, 0, 0);
        int second = world.Grid.CellIndex(0, 0, 1);
        double firstRate = preparation.FixedCellPhaseSourceM3PerSecond[
            FluidPhysics.Offset(Phases.Water, first, world.Grid.CellCount)];
        double secondRate = preparation.FixedCellPhaseSourceM3PerSecond[
            FluidPhysics.Offset(Phases.Water, second, world.Grid.CellCount)];

        Assert.Multiple(() =>
        {
            Assert.That(secondRate / firstRate, Is.EqualTo(4).Within(1e-12));
            Assert.That(firstRate + secondRate, Is.EqualTo(0.001).Within(1e-12));
        });
    }

    private static WellRateSample RunSingleStep(ReservoirWorld world, WellControl well)
    {
        var request = new SimulationRequest
        {
            DurationSeconds = 1,
            InitialTimeStepSeconds = 1,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 0.001,
                MaximumTimeStepSeconds = 1,
                MaximumSaturationChange = 0.1,
                GrowthSaturationChange = 0.01,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-10,
                CgMaximumIterations = 500
            },
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells = [well]
        };
        return new ReservoirSimulator().Run(world, request).Result.Wells.Single().Samples.Last();
    }

    private static WellConnection Connection(
        int i, int j, int k, double openFraction = 1) => new()
        {
            I = i,
            J = j,
            K = k,
            WellboreRadiusM = 0.1,
            Skin = 0,
            OpenFraction = openFraction
        };
}
