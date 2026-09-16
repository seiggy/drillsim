using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class WorldFactoryTests
{
    private readonly ReservoirWorldFactory _factory = new();

    [Test]
    public void Create_SameRequestAndSeed_ReproducesWorldExactly()
    {
        var request = TestData.ConditionedRequest();

        ReservoirWorld first = _factory.Create(request);
        ReservoirWorld second = _factory.Create(request);

        Assert.Multiple(() =>
        {
            Assert.That(second.Summary.WorldId, Is.EqualTo(first.Summary.WorldId));
            Assert.That(second.TopDepthM, Is.EqualTo(first.TopDepthM));
            Assert.That(second.BaseDepthM, Is.EqualTo(first.BaseDepthM));
            Assert.That(second.Porosity, Is.EqualTo(first.Porosity));
            Assert.That(second.LogPermeability, Is.EqualTo(first.LogPermeability));
            Assert.That(second.PressurePa, Is.EqualTo(first.PressurePa));
            Assert.That(second.OilSaturation, Is.EqualTo(first.OilSaturation));
            Assert.That(second.WaterSaturation, Is.EqualTo(first.WaterSaturation));
            Assert.That(second.GasSaturation, Is.EqualTo(first.GasSaturation));
        });
    }

    [Test]
    public void Create_DifferentSeed_ChangesTruthAwayFromControls()
    {
        ReservoirWorld first = _factory.Create(TestData.ConditionedRequest(1234));
        ReservoirWorld second = _factory.Create(TestData.ConditionedRequest(4321));
        int awayFromControls = first.Grid.CellIndex(3, 4, 1);

        Assert.Multiple(() =>
        {
            Assert.That(second.Summary.WorldId, Is.Not.EqualTo(first.Summary.WorldId));
            Assert.That(second.Porosity[awayFromControls], Is.Not.EqualTo(first.Porosity[awayFromControls]));
            Assert.That(second.LogPermeability[awayFromControls], Is.Not.EqualTo(first.LogPermeability[awayFromControls]));
        });
    }

    [Test]
    public void Create_AtControlLocations_HonorsConditioningValues()
    {
        var request = TestData.ConditionedRequest();
        ReservoirWorld world = _factory.Create(request);
        int firstColumn = world.Grid.ColumnIndex(0, 0);
        int secondColumn = world.Grid.ColumnIndex(7, 7);
        int firstCell = world.Grid.CellIndex(0, 0, 1);
        int secondCell = world.Grid.CellIndex(7, 7, 1);
        var firstControl = request.ConditioningPoints[0];
        var secondControl = request.ConditioningPoints[1];

        Assert.Multiple(() =>
        {
            Assert.That(world.TopDepthM[firstColumn], Is.EqualTo(firstControl.ReservoirTopDepthM).Within(1e-10));
            Assert.That(world.BaseDepthM[firstColumn], Is.EqualTo(firstControl.ReservoirBaseDepthM).Within(1e-10));
            Assert.That(world.Porosity[firstCell], Is.EqualTo(firstControl.Porosity).Within(1e-12));
            Assert.That(world.PermeabilityM2[firstCell], Is.EqualTo(firstControl.PermeabilityM2).Within(1e-24));
            Assert.That(world.PressurePa[firstCell], Is.EqualTo(firstControl.PressurePa).Within(1e-6));
            Assert.That(world.TopDepthM[secondColumn], Is.EqualTo(secondControl.ReservoirTopDepthM).Within(1e-10));
            Assert.That(world.BaseDepthM[secondColumn], Is.EqualTo(secondControl.ReservoirBaseDepthM).Within(1e-10));
            Assert.That(world.Porosity[secondCell], Is.EqualTo(secondControl.Porosity).Within(1e-12));
            Assert.That(world.PermeabilityM2[secondCell], Is.EqualTo(secondControl.PermeabilityM2).Within(1e-24));
            Assert.That(world.WaterSaturation[secondCell], Is.EqualTo(secondControl.WaterSaturation).Within(1e-12));
            Assert.That(world.GasSaturation[secondCell], Is.EqualTo(secondControl.GasSaturation).Within(1e-12));
        });
    }
}
