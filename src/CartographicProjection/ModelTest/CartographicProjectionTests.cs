using NUnit.Framework;
using ModelNs = NORCE.Drilling.CartographicProjection.Model;
using System;

namespace NORCE.Drilling.CartographicProjection.ModelTest;

public class CartographicProjectionTests
{
    [Test]
    public void GetProj4String_UTM_IncludesZoneAndSouth()
    {
        var proj = new ModelNs.CartographicProjection
        {
            ProjectionType = ModelNs.ProjectionType.UTM,
            Zone = 33,
            IsSouth = true
        };

        string s = proj.GetProj4String();
        Assert.Multiple(() =>
        {
            Assert.That(s, Does.Contain("+proj=utm"));
            Assert.That(s, Does.Contain("+zone=33"));
            Assert.That(s, Does.Contain("+south"));
        });
    }

    [Test]
    public void GetProj4String_TMerc_WithOriginsAndScale()
    {
        double deg(double d) => d * Math.PI / 180.0; // to radians

        var proj = new ModelNs.CartographicProjection
        {
            ProjectionType = ModelNs.ProjectionType.TransverseMercator,
            LatitudeOrigin = deg(0.0),
            LongitudeOrigin = deg(3.0),
            Scaling = 0.9996,
            FalseEasting = 500000.0,
            FalseNorthing = 0.0,
        };

        string s = proj.GetProj4String();
        Assert.Multiple(() =>
        {
            Assert.That(s, Does.Contain("+proj=tmerc"));
            Assert.That(s, Does.Contain("+lat_0=0"));
            Assert.That(s, Does.Contain("+long_0=3"));
            Assert.That(s, Does.Contain("+k_0=0.9996"));
            Assert.That(s, Does.Contain("+x_0=500000"));
            Assert.That(s, Does.Contain("+y_0=0"));
        });
    }
}
