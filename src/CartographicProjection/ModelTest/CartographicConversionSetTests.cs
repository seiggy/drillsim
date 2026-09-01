using NUnit.Framework;
using ModelNs = NORCE.Drilling.CartographicProjection.Model;
using SharedNs = NORCE.Drilling.CartographicProjection.ModelShared;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.CartographicProjection.ModelTest;

public class CartographicConversionSetTests
{
    private static SharedNs.GeodeticDatum BuildWgs84Datum()
    {
        // Minimal WGS84 definition: semi-major axis a = 6378137.0, inverse flattening rf = 298.257223563
        return new SharedNs.GeodeticDatum
        {
            Spheroid = new SharedNs.Spheroid
            {
                SemiMajorAxis = new SharedNs.ScalarDrillingProperty
                {
                    DiracDistributionValue = new SharedNs.DiracDistribution { Value = 6378137.0 }
                },
                InverseFlattening = new SharedNs.ScalarDrillingProperty
                {
                    DiracDistributionValue = new SharedNs.DiracDistribution { Value = 298.257223563 }
                }
            }
        };
    }

    private static ModelNs.CartographicProjection BuildUtm32N()
    {
        return new ModelNs.CartographicProjection
        {
            ProjectionType = ModelNs.ProjectionType.UTM,
            Zone = 32,
            IsSouth = false
        };
    }

    [Test]
    public void ToCarto_Then_FromCarto_RoundTrip_LatLonRadians()
    {
        double deg(double d) => d * Math.PI / 180.0;
        var datum = BuildWgs84Datum();
        var proj = BuildUtm32N();
        var set = new ModelNs.CartographicConversionSet();

        // Bergen-ish
        double lat = deg(60.3913);
        double lon = deg(5.3221);

        set.ToCarto(proj, datum, lat, lon, out double? northing, out double? easting);
        Assert.That(northing, Is.Not.Null);
        Assert.That(easting, Is.Not.Null);

        set.FromCarto(proj, datum, northing!.Value, easting!.Value, out double? lat2, out double? lon2);
        Assert.That(lat2, Is.Not.Null);
        Assert.That(lon2, Is.Not.Null);

        // within ~1e-7 rad (~1e-7*6371000m ~ 0.6m)
        const double tol = 1e-7;
        Assert.That(Math.Abs(lat2!.Value - lat), Is.LessThan(tol), "latitude round-trip error too large");
        Assert.That(Math.Abs(lon2!.Value - lon), Is.LessThan(tol), "longitude round-trip error too large");
    }

    [Test]
    public void CalculateGridConvergence_ReturnsFinite()
    {
        double deg(double d) => d * Math.PI / 180.0;
        var datum = BuildWgs84Datum();
        var proj = BuildUtm32N();
        var set = new ModelNs.CartographicConversionSet();

        double lat = deg(60.0);
        double lon = deg(5.0);

        set.ToCarto(proj, datum, lat, lon, out double? northing, out double? easting);
        Assert.That(northing, Is.Not.Null);
        Assert.That(easting, Is.Not.Null);

        set.CalculateGridConvergence(proj, datum, northing!.Value, easting!.Value, lat, lon, out double? gc);
        Assert.That(gc, Is.Not.Null);
        Assert.That(double.IsFinite(gc!.Value), Is.True);
        Assert.That(Math.Abs(gc.Value), Is.LessThan(Math.PI));
    }

    [Test]
    public void Batch_ToCarto_ProducesSameCount()
    {
        double deg(double d) => d * Math.PI / 180.0;
        var datum = BuildWgs84Datum();
        var proj = BuildUtm32N();
        var set = new ModelNs.CartographicConversionSet();

        var inputs = new List<Tuple<double, double>>
        {
            Tuple.Create(deg(60.0), deg(5.0)),
            Tuple.Create(deg(59.9), deg(10.7))
        };
        var outputs = new List<Tuple<double, double>>();

        set.ToCarto(proj, datum, inputs, outputs);
        Assert.That(outputs.Count, Is.EqualTo(inputs.Count));
    }
}
