using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class PhysicsV2Tests
{
    [Test]
    public void Create_FortySixStructuralAndNinePropertyControls_AreAccepted()
    {
        StructuralConditioningPoint[] structural = Enumerable.Range(0, 46)
            .Select(index => new StructuralConditioningPoint
            {
                EastingM = (index % 10) * 100,
                NorthingM = (index / 10) * 100,
                ReservoirTopDepthM = 1_000 + index,
                ReservoirBaseDepthM = 1_050 + index
            }).ToArray();
        ConditioningPoint[] properties = Enumerable.Range(0, 9)
            .Select(index => Property((index % 3) * 300, (index / 3) * 300, netToGross: 0.7))
            .ToArray();
        WorldGenerationRequest request = BaseRequest() with
        {
            Grid = new GridOptions { CountX = 12, CountY = 8, CountZ = 4, HorizontalPaddingM = 50 },
            StructuralConditioningPoints = structural,
            ConditioningPoints = properties
        };

        ReservoirWorld world = new ReservoirWorldFactory().Create(request);

        Assert.Multiple(() =>
        {
            Assert.That(world.Summary.StructuralControlCount, Is.EqualTo(46));
            Assert.That(world.Summary.ConditioningControlCount, Is.EqualTo(9));
            Assert.That(world.Grid.OriginEastingM, Is.EqualTo(-50));
            Assert.That(world.Grid.OriginNorthingM, Is.EqualTo(-50));
        });
    }

    [Test]
    public void Create_ExplicitStructuralControls_DriveSurfacesInsteadOfPropertyStructure()
    {
        WorldGenerationRequest request = BaseRequest() with
        {
            Grid = new GridOptions { CountX = 8, CountY = 1, CountZ = 2, HorizontalPaddingM = 50 },
            StructuralConditioningPoints =
            [
                Structure(0, 0, 1_000, 1_050),
                Structure(700, 0, 1_100, 1_170)
            ],
            ConditioningPoints =
            [
                Property(0, 0, top: 500, baseDepth: 550),
                Property(700, 0, top: 600, baseDepth: 650)
            ]
        };

        ReservoirWorld world = new ReservoirWorldFactory().Create(request);

        Assert.Multiple(() =>
        {
            Assert.That(world.TopDepthM[world.Grid.ColumnIndex(0, 0)], Is.EqualTo(1_000).Within(1e-12));
            Assert.That(world.BaseDepthM[world.Grid.ColumnIndex(0, 0)], Is.EqualTo(1_050).Within(1e-12));
            Assert.That(world.TopDepthM[world.Grid.ColumnIndex(7, 0)], Is.EqualTo(1_100).Within(1e-12));
            Assert.That(world.BaseDepthM[world.Grid.ColumnIndex(7, 0)], Is.EqualTo(1_170).Within(1e-12));
        });
    }

    [Test]
    public void Create_StructuralOnlyPoints_DoNotDilutePetrophysicalInterpolation()
    {
        ConditioningPoint property = Property(0, 0, porosity: 0.27, permeability: 2e-12);
        WorldGenerationRequest request = BaseRequest() with
        {
            Grid = new GridOptions { CountX = 8, CountY = 1, CountZ = 2, HorizontalPaddingM = 50 },
            StructuralConditioningPoints =
            [
                Structure(0, 0, 1_000, 1_050),
                Structure(700, 0, 1_300, 1_360)
            ],
            ConditioningPoints = [property]
        };

        ReservoirWorld world = new ReservoirWorldFactory().Create(request);

        Assert.Multiple(() =>
        {
            Assert.That(world.Porosity, Is.All.EqualTo(property.Porosity).Within(1e-12));
            Assert.That(world.PermeabilityM2, Is.All.EqualTo(property.PermeabilityM2).Within(1e-24));
        });
    }

    [Test]
    public void Create_NetToGrossRealization_IsDeterministicAndVariesFaciesQuality()
    {
        WorldGenerationRequest request = BaseRequest() with
        {
            Grid = new GridOptions { CountX = 8, CountY = 1, CountZ = 3, HorizontalPaddingM = 50 },
            StructuralConditioningPoints =
            [
                Structure(0, 0, 1_000, 1_060),
                Structure(700, 0, 1_000, 1_060)
            ],
            ConditioningPoints =
            [
                Property(0, 0, porosity: 0.25, permeability: 1e-12, netToGross: 0.1),
                Property(700, 0, porosity: 0.25, permeability: 1e-12, netToGross: 0.9)
            ],
            Heterogeneity = TestData.ZeroHeterogeneity() with { NetToGrossStdDev = 0.08 }
        };
        var factory = new ReservoirWorldFactory();

        ReservoirWorld first = factory.Create(request);
        ReservoirWorld second = factory.Create(request);

        Assert.Multiple(() =>
        {
            Assert.That(second.NetToGross, Is.EqualTo(first.NetToGross));
            Assert.That(first.NetToGross.Max() - first.NetToGross.Min(), Is.GreaterThan(0.5));
            Assert.That(first.Porosity.Max() - first.Porosity.Min(), Is.GreaterThan(0.1));
            Assert.That(first.LogPermeability.Max() - first.LogPermeability.Min(), Is.GreaterThan(5));
        });
    }

    [Test]
    public void Create_GasWaterContact_InitializesCoherentColumn()
    {
        ReservoirWorld world = new ReservoirWorldFactory().Create(ContactRequest(new FluidContactOptions
        {
            TransitionThicknessM = 0,
            GasWater = [Contact(1_050)]
        }));

        Assert.Multiple(() =>
        {
            Assert.That(world.GasSaturation[world.Grid.CellIndex(0, 0, 0)], Is.EqualTo(0.85).Within(1e-12));
            Assert.That(world.WaterSaturation[world.Grid.CellIndex(0, 0, 9)], Is.EqualTo(1).Within(1e-12));
            Assert.That(world.OilSaturation.Max(Math.Abs), Is.LessThan(1e-12));
            Assert.That(MaximumClosureError(world), Is.LessThan(1e-14));
        });
    }

    [Test]
    public void Create_GasOilAndOilWaterContacts_InitializeThreeZones()
    {
        ReservoirWorld world = new ReservoirWorldFactory().Create(ContactRequest(new FluidContactOptions
        {
            TransitionThicknessM = 10,
            GasOil = [Contact(1_030)],
            OilWater = [Contact(1_070)]
        }));

        int gasCell = world.Grid.CellIndex(0, 0, 0);
        int oilCell = world.Grid.CellIndex(0, 0, 4);
        int waterCell = world.Grid.CellIndex(0, 0, 9);
        Assert.Multiple(() =>
        {
            Assert.That(world.GasSaturation[gasCell], Is.GreaterThan(0.7));
            Assert.That(world.OilSaturation[oilCell], Is.GreaterThan(0.7));
            Assert.That(world.WaterSaturation[waterCell], Is.GreaterThan(0.99));
            Assert.That(MaximumClosureError(world), Is.LessThan(1e-14));
        });
    }

    [Test]
    public void Create_InvalidContactOrdering_FailsExplicitly()
    {
        WorldGenerationRequest request = ContactRequest(new FluidContactOptions
        {
            TransitionThicknessM = 5,
            GasOil = [Contact(1_080)],
            OilWater = [Contact(1_040)]
        });

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirWorldFactory().Create(request));

        Assert.That(exception!.Errors.Keys, Does.Contain("fluidContacts"));
    }

    [Test]
    public void Create_ConflictingGasWaterAndThreeZoneContacts_FailsExplicitly()
    {
        WorldGenerationRequest request = ContactRequest(new FluidContactOptions
        {
            TransitionThicknessM = 5,
            GasWater = [Contact(1_050)],
            GasOil = [Contact(1_030)],
            OilWater = [Contact(1_070)]
        });

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirWorldFactory().Create(request));

        Assert.That(exception!.Errors["fluidContacts"], Has.Some.Contains("cannot be combined"));
    }


    [Test]
    public void Run_ContactInitializedColumnWithGravity_HasNegligibleDrift()
    {
        ReservoirWorld world = new ReservoirWorldFactory().Create(ContactRequest(new FluidContactOptions
        {
            TransitionThicknessM = 10,
            GasOil = [Contact(1_030)],
            OilWater = [Contact(1_070)]
        }));
        double[] initialPressure = (double[])world.PressurePa.Clone();
        double[] initialOil = (double[])world.OilSaturation.Clone();
        double[] initialWater = (double[])world.WaterSaturation.Clone();
        double[] initialGas = (double[])world.GasSaturation.Clone();
        var request = new SimulationRequest
        {
            DurationSeconds = 100,
            InitialTimeStepSeconds = 10,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 0.01,
                MaximumTimeStepSeconds = 10,
                MaximumSaturationChange = 0.01,
                GrowthSaturationChange = 0.001,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-12,
                CgMaximumIterations = 500
            },
            Fluids = new FluidModelOptions(),
            Wells = []
        };

        SimulationExecution result = new ReservoirSimulator().Run(world, request);

        Assert.Multiple(() =>
        {
            Assert.That(MaximumDifference(result.FinalState.PressurePa, initialPressure), Is.LessThan(1));
            Assert.That(MaximumDifference(result.FinalState.OilSaturation, initialOil), Is.LessThan(1e-6));
            Assert.That(MaximumDifference(result.FinalState.WaterSaturation, initialWater), Is.LessThan(1e-6));
            Assert.That(MaximumDifference(result.FinalState.GasSaturation, initialGas), Is.LessThan(1e-6));
            Assert.That(result.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-8));
        });
    }

    [Test]
    public void Run_LargeGasWaterColumnWithoutOil_DoesNotAmplifyRoundoffBalance()
    {
        WorldGenerationRequest worldRequest = BaseRequest() with
        {
            Grid = new GridOptions { CountX = 64, CountY = 64, CountZ = 20, HorizontalPaddingM = 50 },
            StructuralConditioningPoints =
            [
                Structure(0, 0, 1_000, 1_100),
                Structure(7_000, 7_000, 1_000, 1_100)
            ],
            ConditioningPoints =
            [
                Property(0, 0),
                Property(7_000, 7_000)
            ],
            FluidContacts = new FluidContactOptions
            {
                TransitionThicknessM = 0,
                GasWater = [Contact(1_050)]
            }
        };
        ReservoirWorld world = new ReservoirWorldFactory().Create(worldRequest);
        var runRequest = new SimulationRequest
        {
            DurationSeconds = 100,
            InitialTimeStepSeconds = 100,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 1,
                MaximumTimeStepSeconds = 100,
                MaximumSaturationChange = 0.01,
                GrowthSaturationChange = 0.001,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-10,
                CgMaximumIterations = 1_000
            },
            Fluids = new FluidModelOptions(),
            Wells = []
        };

        SimulationExecution execution = new ReservoirSimulator().Run(world, runRequest);
        PhaseMaterialBalance oilBalance = execution.Result.MaterialBalance.Oil;
        double totalInitialPoreFluid = world.PoreVolumeM3.Sum();
        double finalOilVolume = world.PoreVolumeM3
            .Zip(execution.FinalState.OilSaturation, (poreVolume, saturation) => poreVolume * saturation)
            .Sum();

        Assert.Multiple(() =>
        {
            Assert.That(world.Grid.CellCount, Is.EqualTo(81_920));
            Assert.That(double.IsFinite(oilBalance.InPlaceNormalizedBalanceErrorFraction), Is.True);
            Assert.That(double.IsFinite(execution.Result.MaximumBalanceErrorFraction), Is.True);
            Assert.That(finalOilVolume / totalInitialPoreFluid, Is.LessThan(1e-10));
            Assert.That(oilBalance.InPlaceNormalizedBalanceErrorFraction, Is.LessThan(1e-6));
            Assert.That(execution.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-6));
            Assert.That(oilBalance.CumulativeInjectedM3, Is.Zero);
            Assert.That(oilBalance.CumulativeProducedM3, Is.Zero);
        });
    }


    private static WorldGenerationRequest BaseRequest() => new()
    {
        FieldId = Guid.Parse("ae19b2af-f240-4c28-aa72-c4d648fd59a1"),
        ReservoirName = "V2 Sand",
        Seed = 2026,
        CalibrationArtifact = TestData.CalibrationArtifact(),
        Grid = new GridOptions { CountX = 1, CountY = 1, CountZ = 10, HorizontalPaddingM = 50 },
        Heterogeneity = TestData.ZeroHeterogeneity(),
        StructuralConditioningPoints = [Structure(0, 0, 1_000, 1_100)],
        ConditioningPoints = [Property(0, 0)]
    };

    private static WorldGenerationRequest ContactRequest(FluidContactOptions contacts) => BaseRequest() with
    {
        FluidContacts = contacts
    };

    private static StructuralConditioningPoint Structure(
        double easting, double northing, double top, double baseDepth) => new()
        {
            EastingM = easting,
            NorthingM = northing,
            ReservoirTopDepthM = top,
            ReservoirBaseDepthM = baseDepth
        };

    private static ConditioningPoint Property(
        double easting,
        double northing,
        double top = 1_000,
        double baseDepth = 1_100,
        double porosity = 0.25,
        double permeability = 1e-12,
        double netToGross = 1) => new()
        {
            EastingM = easting,
            NorthingM = northing,
            ReservoirTopDepthM = top,
            ReservoirBaseDepthM = baseDepth,
            Porosity = porosity,
            PermeabilityM2 = permeability,
            PressurePa = 20_000_000,
            WaterSaturation = 0.2,
            GasSaturation = 0.05,
            NetToGross = netToGross
        };

    private static FluidContactPoint Contact(double depth) => new()
    {
        EastingM = 0,
        NorthingM = 0,
        ContactDepthTvdM = depth
    };

    private static double MaximumClosureError(ReservoirWorld world)
    {
        double maximum = 0;
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
            maximum = Math.Max(maximum, Math.Abs(
                world.OilSaturation[cell] + world.WaterSaturation[cell] + world.GasSaturation[cell] - 1));
        return maximum;
    }

    private static double MaximumDifference(double[] left, double[] right) =>
        left.Zip(right, (first, second) => Math.Abs(first - second)).Max();
}
