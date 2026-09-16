using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class BuckleyLeverettReferenceTests
{
    private const double InitialWaterSaturation = 0.2;
    private const double ResidualOilSaturation = 0.2;
    private const double InjectedPoreVolumes = 0.3;
    private const double DurationSeconds = 1_000_000;

    [Test]
    public void ImmiscibleOilWaterLimit_AtPointThreePv_MatchesBuckleyLeverettFrontWithinOneBlock()
    {
        FrontResult result = RunFront(40);

        Assert.Multiple(() =>
        {
            Assert.That(result.FrontErrorBlocks, Is.LessThanOrEqualTo(1));
            Assert.That(result.MaximumBalanceErrorFraction, Is.LessThan(1e-6));
        });
    }

    [Test]
    public void ImmiscibleOilWaterLimit_FrontErrorConvergesMonotonicallyWithGridRefinement()
    {
        FrontResult coarse = RunFront(20);
        FrontResult medium = RunFront(40);
        FrontResult fine = RunFront(80);

        Assert.Multiple(() =>
        {
            Assert.That(medium.FrontErrorFraction, Is.LessThanOrEqualTo(coarse.FrontErrorFraction + 1e-12));
            Assert.That(fine.FrontErrorFraction, Is.LessThanOrEqualTo(medium.FrontErrorFraction + 1e-12));
            Assert.That(fine.MaximumBalanceErrorFraction, Is.LessThan(1e-6));
        });
    }

    private static FrontResult RunFront(int countX)
    {
        ReservoirWorld world = CreateWorld(countX);
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
        {
            world.PressurePa[cell] = 50_000_000;
            world.OilSaturation[cell] = 1 - InitialWaterSaturation;
            world.WaterSaturation[cell] = InitialWaterSaturation;
            world.GasSaturation[cell] = 0;
        }
        double totalPoreVolume = world.PoreVolumeM3.Sum();
        double rate = InjectedPoreVolumes * totalPoreVolume / DurationSeconds;
        var fluids = new FluidModelOptions
        {
            ReferencePressurePa = 50_000_000,
            OilViscosityPaS = 0.001,
            WaterViscosityPaS = 0.001,
            OilCompressibilityPerPa = 0,
            WaterCompressibilityPerPa = 0,
            GasCompressibilityPerPa = 0,
            RockCompressibilityPerPa = 1e-9,
            ResidualOilSaturation = ResidualOilSaturation,
            ResidualWaterSaturation = InitialWaterSaturation,
            ResidualGasSaturation = 0,
            OilCoreyExponent = 2,
            WaterCoreyExponent = 2,
            GasCoreyExponent = 2,
            OilRelativePermeabilityEndPoint = 1,
            WaterRelativePermeabilityEndPoint = 1,
            GasRelativePermeabilityEndPoint = 0,
            GravityMPerS2 = 0
        };
        var request = new SimulationRequest
        {
            DurationSeconds = DurationSeconds,
            InitialTimeStepSeconds = DurationSeconds / (15 * countX),
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 0.01,
                MaximumTimeStepSeconds = DurationSeconds / (15 * countX),
                MaximumSaturationChange = 0.03,
                GrowthSaturationChange = 0.005,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-10,
                CgMaximumIterations = 1_000,
                MaximumStepAttempts = 20_000,
                MaximumSamplesPerWell = 10
            },
            Fluids = fluids,
            Wells =
            [
                new WellControl
                {
                    Name = "Injector",
                    ControlMode = WellControlMode.Rate,
                    TotalRateM3PerSecond = rate,
                    InjectionWaterFraction = 1,
                    Connections = [new WellConnection { I = 0, J = 0, K = 0 }]
                },
                new WellControl
                {
                    Name = "Producer",
                    ControlMode = WellControlMode.Rate,
                    TotalRateM3PerSecond = -rate,
                    Connections = [new WellConnection { I = countX - 1, J = 0, K = 0 }]
                }
            ]
        };

        SimulationExecution execution = new ReservoirSimulator().Run(world, request);
        double movableSaturation = 1 - InitialWaterSaturation - ResidualOilSaturation;
        double shockWaterSaturation = InitialWaterSaturation + movableSaturation / Math.Sqrt(2);
        double shockNormalized = (shockWaterSaturation - InitialWaterSaturation) / movableSaturation;
        double fractionalFlowAtShock = shockNormalized * shockNormalized /
            (shockNormalized * shockNormalized + Math.Pow(1 - shockNormalized, 2));
        double analyticalFront = InjectedPoreVolumes * fractionalFlowAtShock /
            (shockWaterSaturation - InitialWaterSaturation);
        double threshold = 0.5 * (InitialWaterSaturation + shockWaterSaturation);
        int frontCell = -1;
        double numericalFront = 0;
        for (int i = 0; i < countX; i++)
            if (execution.FinalState.WaterSaturation[world.Grid.CellIndex(i, 0, 0)] >= threshold) frontCell = i;
        for (int i = 0; i + 1 < countX; i++)
        {
            double left = execution.FinalState.WaterSaturation[world.Grid.CellIndex(i, 0, 0)];
            double right = execution.FinalState.WaterSaturation[world.Grid.CellIndex(i + 1, 0, 0)];
            if (left >= threshold && right < threshold)
            {
                double crossing = (left - threshold) / (left - right);
                numericalFront = (i + 0.5 + crossing) / countX;
                break;
            }
        }
        double error = Math.Abs(numericalFront - analyticalFront);
        double frontErrorBlocks = Math.Abs((frontCell + 0.5) / countX - analyticalFront) * countX;
        return new FrontResult(error, frontErrorBlocks, execution.Result.MaximumBalanceErrorFraction);
    }

    private static ReservoirWorld CreateWorld(int countX)
    {
        ConditioningPoint Control(double easting) => new()
        {
            EastingM = easting,
            NorthingM = 0,
            ReservoirTopDepthM = 1_000,
            ReservoirBaseDepthM = 1_030,
            Porosity = 0.2,
            PermeabilityM2 = 1e-11,
            PressurePa = 50_000_000,
            WaterSaturation = InitialWaterSaturation,
            GasSaturation = 0,
            NetToGross = 1
        };
        return new ReservoirWorldFactory().Create(new WorldGenerationRequest
        {
            FieldId = Guid.Parse("06304482-8f7c-467f-9a46-d0db236f8466"),
            ReservoirName = $"Buckley-Leverett {countX}",
            Seed = 1,
            CalibrationArtifact = TestData.CalibrationArtifact(),
            Grid = new GridOptions { CountX = countX, CountY = 1, CountZ = 1, HorizontalPaddingM = 50 },
            Heterogeneity = TestData.ZeroHeterogeneity(),
            ConditioningPoints = [Control(0), Control(700)]
        });
    }

    private sealed record FrontResult(
        double FrontErrorFraction,
        double FrontErrorBlocks,
        double MaximumBalanceErrorFraction);
}
