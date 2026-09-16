using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class ValidationTests
{
    [Test]
    public void Create_InvalidConditioningValues_FailsExplicitly()
    {
        WorldGenerationRequest valid = TestData.ConditionedRequest();
        ConditioningPoint first = valid.ConditioningPoints[0];
        WorldGenerationRequest invalid = valid with
        {
            ConditioningPoints =
            [
                first with
                {
                    ReservoirBaseDepthM = first.ReservoirTopDepthM,
                    Porosity = double.NaN,
                    PermeabilityM2 = 0,
                    WaterSaturation = 0.8,
                    GasSaturation = 0.4,
                    NetToGross = 1.2
                }
            ]
        };

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirWorldFactory().Create(invalid));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Errors.Keys, Has.Some.Contains("reservoirBaseDepthM"));
            Assert.That(exception.Errors.Keys, Has.Some.Contains("porosity"));
            Assert.That(exception.Errors.Keys, Has.Some.Contains("permeabilityM2"));
            Assert.That(exception.Errors.Keys, Has.Some.Contains("netToGross"));
            Assert.That(exception.Errors.Keys, Does.Contain("conditioningPoints[0]"));
        });
    }

    [Test]
    public void Run_InvalidCompletionAndRate_FailsExplicitly()
    {
        ReservoirWorld world = TestData.UniformWorld();
        var request = new SimulationRequest
        {
            DurationSeconds = 100,
            InitialTimeStepSeconds = 10,
            Wells =
            [
                new WellControl
                {
                    Name = "bad", I = world.Grid.CountX, J = 0, KStart = 1, KEnd = 0,
                    TotalRateM3PerSecond = double.PositiveInfinity,
                    InjectionWaterFraction = 0.8,
                    InjectionGasFraction = 0.5
                }
            ]
        };

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirSimulator().Run(world, request));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Errors.Keys, Has.Some.Contains(".i"));
            Assert.That(exception.Errors.Keys, Has.Some.Contains(".kEnd"));
            Assert.That(exception.Errors.Keys, Has.Some.Contains("totalRate"));
            Assert.That(exception.Errors.Keys, Does.Contain("wells[0]"));
        });
    }

    [Test]
    public void Run_InvalidNumericalBudgetsAndTolerance_FailsValidation()
    {
        ReservoirWorld world = TestData.UniformWorld();
        var request = new SimulationRequest
        {
            DurationSeconds = 100,
            InitialTimeStepSeconds = 10,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 1,
                MaximumTimeStepSeconds = 10,
                CgRelativeTolerance = 1e-5,
                MaximumStepAttempts = 100_001,
                MaximumSamplesPerWell = 0
            }
        };

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirSimulator().Run(world, request));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.Errors.Keys, Does.Contain("solver.cgRelativeTolerance"));
            Assert.That(exception.Errors.Keys, Does.Contain("solver.maximumStepAttempts"));
            Assert.That(exception.Errors.Keys, Does.Contain("solver.maximumSamplesPerWell"));
        });
    }

    [Test]
    public void Run_GappedSchedule_FailsValidation()
    {
        ReservoirWorld world = TestData.UniformWorld();
        var request = new SimulationRequest
        {
            DurationSeconds = 20,
            InitialTimeStepSeconds = 1,
            Schedule =
            [
                new ScheduleSegment { StartTimeSeconds = 0, DurationSeconds = 5, Wells = [] },
                new ScheduleSegment { StartTimeSeconds = 10, DurationSeconds = 10, Wells = [] }
            ]
        };

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirSimulator().Run(world, request));

        Assert.That(exception!.Errors.Keys, Has.Some.Contains("startTimeSeconds"));
    }

    [Test]
    public void Run_DuplicatePeacemanConnections_FailsValidation()
    {
        ReservoirWorld world = TestData.UniformWorld();
        var connection = new WellConnection { I = 0, J = 0, K = 0, WellboreRadiusM = 0.1, OpenFraction = 1 };
        var request = new SimulationRequest
        {
            DurationSeconds = 10,
            InitialTimeStepSeconds = 1,
            Wells =
            [
                new WellControl
                {
                    Name = "W",
                    ControlMode = WellControlMode.Rate,
                    TotalRateM3PerSecond = 0.001,
                    Connections = [connection, connection]
                }
            ]
        };

        ReservoirValidationException? exception = Assert.Throws<ReservoirValidationException>(
            () => new ReservoirSimulator().Run(world, request));

        Assert.That(exception!.Errors.Values.SelectMany(messages => messages),
            Has.Some.Contains("only one connection per grid cell"));
    }
}
