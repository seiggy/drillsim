using NUnit.Framework;
using ModelNs = NORCE.Drilling.CartographicProjection.Model;

namespace NORCE.Drilling.CartographicProjection.ModelTest;

public class CartographicProjectionTypeTests
{
    [Test]
    public void Get_UTM_Prototype_HasExpectedFlags_And_Proj4()
    {
        var proto = ModelNs.CartographicProjectionType.Get(ModelNs.ProjectionType.UTM);
        Assert.That(proto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(proto!.UseZone, Is.True, "UTM should use zone");
            Assert.That(proto!.UseSouth, Is.True, "UTM should allow south flag");
            Assert.That(proto!.GetProj4String(), Does.Contain("+proj=utm"));
        });
    }

    [Test]
    public void Get_Mercator_Prototype_HasExpectedFlags_And_Proj4()
    {
        var proto = ModelNs.CartographicProjectionType.Get(ModelNs.ProjectionType.Mercator);
        Assert.That(proto, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(proto!.UseLongitudeOrigin, Is.True);
            Assert.That(proto!.UseLatitudeTrueScale, Is.True);
            Assert.That(proto!.UseFalseEastingNorthing, Is.True);
            Assert.That(proto!.UseScaling, Is.True);
            Assert.That(proto!.GetProj4String(), Does.Contain("+proj=merc"));
        });
    }
}
