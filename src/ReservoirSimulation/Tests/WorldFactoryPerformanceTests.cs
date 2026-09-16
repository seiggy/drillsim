using System.Diagnostics;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class WorldFactoryPerformanceTests
{
    [Test]
    public void Create_Representative81920CellCalibratedWorld_CompletesWithinTwoSeconds()
    {
        WorldGenerationRequest request = RepresentativeRequest();
        var factory = new ReservoirWorldFactory();
        factory.Create(request with
        {
            Grid = new GridOptions { CountX = 4, CountY = 4, CountZ = 4, HorizontalPaddingM = 250 }
        });

        var stopwatch = Stopwatch.StartNew();
        ReservoirWorld world = factory.Create(request);
        stopwatch.Stop();
        TestContext.Out.WriteLine($"ReservoirWorldFactory.Create: {stopwatch.Elapsed.TotalMilliseconds:F1} ms");

        Assert.Multiple(() =>
        {
            Assert.That(world.Grid.CellCount, Is.EqualTo(81_920));
            Assert.That(world.Summary.StructuralControlCount, Is.EqualTo(46));
            Assert.That(world.Summary.ConditioningControlCount, Is.EqualTo(9));
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));
        });
    }

    private static WorldGenerationRequest RepresentativeRequest()
    {
        StructuralConditioningPoint[] structural = Enumerable.Range(0, 46)
            .Select(index => new StructuralConditioningPoint
            {
                EastingM = (index % 29 % 6) * 1_400,
                NorthingM = (index % 29 / 6) * 1_400,
                ReservoirTopDepthM = 1_500 + 8 * Math.Sin(index % 29),
                ReservoirBaseDepthM = 1_590 + 8 * Math.Sin(index % 29)
            }).ToArray();
        ConditioningPoint[] properties = Enumerable.Range(0, 9)
            .Select(index => new ConditioningPoint
            {
                EastingM = (index % 3) * 3_500,
                NorthingM = (index / 3) * 2_800,
                ReservoirTopDepthM = 1_500,
                ReservoirBaseDepthM = 1_590,
                Porosity = 0.22 + 0.01 * (index % 3),
                PermeabilityM2 = 5e-13 * (1 + index),
                PressurePa = 28_000_000 + index * 100_000,
                WaterSaturation = 0.2,
                GasSaturation = 0.05,
                NetToGross = 0.55 + 0.05 * (index % 3)
            }).ToArray();
        FluidContactPoint[] gasWater =
        [
            Contact(0, 0, 1_545),
            Contact(7_000, 0, 1_548),
            Contact(0, 5_600, 1_542),
            Contact(7_000, 5_600, 1_546)
        ];
        return new WorldGenerationRequest
        {
            FieldId = Guid.Parse("fd7edb87-d35a-4e26-a156-49618e1783a9"),
            ReservoirName = "Representative calibrated Sognefjord",
            Seed = 2_026_090_3,
            CalibrationArtifact = TestData.CalibrationArtifact(),
            Grid = new GridOptions { CountX = 64, CountY = 64, CountZ = 20, HorizontalPaddingM = 250 },
            Heterogeneity = new HeterogeneityOptions
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
            },
            StructuralConditioningPoints = structural,
            ConditioningPoints = properties,
            FluidContacts = new FluidContactOptions
            {
                TransitionThicknessM = 8,
                GasWater = gasWater
            }
        };
    }

    private static FluidContactPoint Contact(double easting, double northing, double depth) => new()
    {
        EastingM = easting,
        NorthingM = northing,
        ContactDepthTvdM = depth
    };
}
