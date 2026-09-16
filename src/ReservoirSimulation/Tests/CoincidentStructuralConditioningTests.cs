using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class CoincidentStructuralConditioningTests
{
    [Test]
    public void Create_FortySixIntervalsAtTwentyNineWellheads_AveragesExactStructureDeterministically()
    {
        StructuralConditioningPoint[] controls = CreateStructuralIntervals();
        WorldGenerationRequest forwardRequest = Request(controls);
        WorldGenerationRequest reverseRequest = Request(controls.Reverse().ToArray());
        var factory = new ReservoirWorldFactory();

        ReservoirWorld forward = factory.Create(forwardRequest);
        ReservoirWorld reverse = factory.Create(reverseRequest);
        int firstWellhead = forward.Grid.ColumnIndex(0, 0);

        Assert.Multiple(() =>
        {
            Assert.That(controls.Select(point => (point.EastingM, point.NorthingM)).Distinct().Count(), Is.EqualTo(29));
            Assert.That(forward.Summary.StructuralControlCount, Is.EqualTo(46));
            Assert.That(forward.TopDepthM[firstWellhead], Is.EqualTo(1_050).Within(1e-12));
            Assert.That(forward.BaseDepthM[firstWellhead], Is.EqualTo(1_120).Within(1e-12));
            Assert.That(reverse.Summary.WorldId, Is.EqualTo(forward.Summary.WorldId));
            Assert.That(reverse.TopDepthM, Is.EqualTo(forward.TopDepthM));
            Assert.That(reverse.BaseDepthM, Is.EqualTo(forward.BaseDepthM));
            Assert.That(ReservoirWorldFactory.TruthChecksum(reverse),
                Is.EqualTo(ReservoirWorldFactory.TruthChecksum(forward)));
        });

        var run = new SimulationRequest
        {
            DurationSeconds = 100,
            InitialTimeStepSeconds = 10,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 1,
                MaximumTimeStepSeconds = 10,
                MaximumSaturationChange = 0.05,
                GrowthSaturationChange = 0.01,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-10,
                CgMaximumIterations = 500
            },
            Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
            Wells = []
        };
        SimulationExecution result = new ReservoirSimulator().Run(forward, run);

        Assert.Multiple(() =>
        {
            Assert.That(result.FinalState.PressurePa, Is.EqualTo(forward.PressurePa).Within(1e-8));
            Assert.That(result.Result.MaximumBalanceErrorFraction, Is.LessThan(1e-12));
            Assert.That(result.Result.Steps.RejectedSteps, Is.Zero);
        });
    }

    [Test]
    public void Create_CoincidentPropertyControls_RemainInvalid()
    {
        ConditioningPoint property = Property();
        WorldGenerationRequest request = Request(CreateStructuralIntervals()) with
        {
            ConditioningPoints = [property, property with { Porosity = 0.3 }]
        };

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirWorldFactory().Create(request));

        Assert.That(exception!.Errors.Values.SelectMany(messages => messages),
            Has.Some.Contains("coordinates must be unique"));
    }

    private static StructuralConditioningPoint[] CreateStructuralIntervals()
    {
        var controls = new List<StructuralConditioningPoint>(46);
        for (int wellhead = 0; wellhead < 29; wellhead++)
        {
            double top = 1_000 + wellhead;
            controls.Add(new StructuralConditioningPoint
            {
                EastingM = wellhead * 100,
                NorthingM = 0,
                ReservoirTopDepthM = top,
                ReservoirBaseDepthM = top + 60
            });
        }
        for (int wellhead = 0; wellhead < 17; wellhead++)
        {
            double top = 1_100 + wellhead;
            controls.Add(new StructuralConditioningPoint
            {
                EastingM = wellhead * 100,
                NorthingM = 0,
                ReservoirTopDepthM = top,
                ReservoirBaseDepthM = top + 80
            });
        }
        return controls.ToArray();
    }

    private static WorldGenerationRequest Request(IReadOnlyList<StructuralConditioningPoint> controls) => new()
    {
        FieldId = Guid.Parse("cddc295f-15b8-435f-ab8a-d7fa56ced0f9"),
        ReservoirName = "Coincident structural intervals",
        Seed = 77,
        CalibrationArtifact = TestData.CalibrationArtifact(),
        Grid = new GridOptions { CountX = 29, CountY = 1, CountZ = 2, HorizontalPaddingM = 50 },
        Heterogeneity = TestData.ZeroHeterogeneity(),
        StructuralConditioningPoints = controls,
        ConditioningPoints = [Property()]
    };

    private static ConditioningPoint Property() => new()
    {
        EastingM = 0,
        NorthingM = 0,
        ReservoirTopDepthM = 1_000,
        ReservoirBaseDepthM = 1_100,
        Porosity = 0.25,
        PermeabilityM2 = 1e-12,
        PressurePa = 20_000_000,
        WaterSaturation = 0.2,
        GasSaturation = 0.05,
        NetToGross = 1
    };
}
