using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class PetrophysicsAnalysisTests
{
    private readonly PetrophysicsAnalysisService _service = new(TimeProvider.System);

    [Test]
    public void CalculateNetPay_SumsOnlyAdjacentQualifyingIntervals()
    {
        PetrophysicsSample[] samples =
        [
            new(100, 0.20, 2e-15, 10),
            new(110, 0.18, 3e-15, 5),
            new(120, 0.08, 3e-15, 5),
            new(130, 0.15, 2e-15, 1),
            new(140, 0.16, 4e-15, 1)
        ];

        NetPayResult result = _service.CalculateNetPay("geology:test", samples);

        Assert.Multiple(() =>
        {
            Assert.That(result.NetPayThicknessM, Is.EqualTo(20).Within(1e-12));
            Assert.That(result.QualifyingSampleCount, Is.EqualTo(4));
            Assert.That(result.MeanPayPorosity, Is.EqualTo(0.1725).Within(1e-12));
            Assert.That(result.MeanPayPermeabilityM2, Is.EqualTo(2.75e-15).Within(1e-27));
        });
    }

    [Test]
    public void BuildQuantiles_UsesPetroleumProbabilityOrdering()
    {
        var quantiles = PetrophysicsAnalysisService.BuildQuantiles(20, 3);

        Assert.Multiple(() =>
        {
            Assert.That(quantiles.P90, Is.LessThan(quantiles.P50));
            Assert.That(quantiles.P50, Is.LessThan(quantiles.P10));
            Assert.That(quantiles.P50, Is.EqualTo(20));
        });
    }

    [Test]
    public void MetricToWgs84_ConvertsRiemannianAxesToDegrees()
    {
        const double earthRadiusM = 6_378_137.0;
        var coordinate = PetrophysicsAnalysisService.MetricToWgs84(
            earthRadiusM * Math.PI / 180,
            0);

        Assert.Multiple(() =>
        {
            Assert.That(coordinate.LongitudeDegrees, Is.EqualTo(1).Within(1e-9));
            Assert.That(coordinate.LatitudeDegrees, Is.EqualTo(0).Within(1e-9));
        });
    }

    [Test]
    public void LocalMetricToWgs84_PreservesGulfOriginAndOffsets()
    {
        double longitude = -90.15 * Math.PI / 180;
        double latitude = 29.075 * Math.PI / 180;

        var coordinate = PetrophysicsAnalysisService.LocalMetricToWgs84(0, 0, longitude, latitude);

        Assert.Multiple(() =>
        {
            Assert.That(coordinate.LongitudeDegrees, Is.EqualTo(-90.15).Within(1e-9));
            Assert.That(coordinate.LatitudeDegrees, Is.EqualTo(29.075).Within(1e-9));
        });
    }

    [Test]
    public void Analyze_WithInsufficientData_ReturnsExplicitGapAndNoRanking()
    {
        Guid fieldId = Guid.NewGuid();
        var package = new AnalysisPackage(
            DateTimeOffset.UtcNow,
            fieldId,
            new JsonObject { ["MetaInfo"] = new JsonObject { ["ID"] = fieldId } },
            [], [], [], [], [], [],
            new SourceCounts(1, 0, 0, 0, 0, 0, 0),
            ["No clusters were returned for the field."],
            "abc");

        AnalysisResult result = _service.Analyze(package);

        Assert.Multiple(() =>
        {
            Assert.That(result.Ranking, Is.Empty);
            Assert.That(result.DataGaps, Has.Some.Contains("At least 4 located wells"));
            Assert.That(result.DataGaps, Does.Contain("No clusters were returned for the field."));
        });
    }

    [Test]
    public void Analyze_IncludesLocatedDryWellAsNegativeEvidence()
    {
        Guid fieldId = Guid.NewGuid();
        Guid clusterId = Guid.NewGuid();
        Guid wellId = Guid.NewGuid();
        Guid wellBoreId = Guid.NewGuid();
        var package = new AnalysisPackage(
            DateTimeOffset.UtcNow,
            fieldId,
            new JsonObject { ["MetaInfo"] = new JsonObject { ["ID"] = fieldId } },
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = clusterId },
                ["ReferencePoint"] = new JsonObject { ["RiemannianEast"] = 10.0, ["RiemannianNorth"] = 20.0 }
            }],
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = wellId },
                ["ClusterID"] = clusterId
            }],
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = wellBoreId },
                ["WellID"] = wellId
            }],
            [],
            [],
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = Guid.NewGuid() },
                ["WellBoreID"] = wellBoreId,
                ["GeologicalPropertyTable"] = new JsonArray(
                    GeologicalEntry(100, .05, 1e-18, 0),
                    GeologicalEntry(110, .06, 1e-18, 0))
            }],
            new SourceCounts(1, 1, 1, 1, 0, 0, 1),
            [],
            "dry");

        AnalysisResult result = _service.Analyze(package);

        Assert.That(result.WellSummaries, Has.Count.EqualTo(1));
        Assert.That(result.WellSummaries[0].NetPayThicknessM, Is.Zero);
    }

    [Test]
    public void Analyze_SelectedReservoirExcludesQualifyingSamplesOutsideInterval()
    {
        Guid fieldId = Guid.NewGuid();
        Guid clusterId = Guid.NewGuid();
        Guid wellId = Guid.NewGuid();
        Guid wellBoreId = Guid.NewGuid();
        var geology = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = Guid.NewGuid() },
            ["WellBoreID"] = wellBoreId,
            ["GeologicalPropertyTable"] = new JsonArray(
                GeologicalEntry(90, .2, 2e-15, 1),
                GeologicalEntry(100, .2, 2e-15, 1),
                GeologicalEntry(110, .2, 2e-15, 1),
                GeologicalEntry(120, .2, 2e-15, 1)),
            ["Petrophysics"] = new JsonObject
            {
                ["FormationIntervals"] = new JsonArray(new JsonObject
                {
                    ["FormationName"] = "Target",
                    ["TopDepth"] = new JsonObject { ["Value"] = 100 },
                    ["BaseDepth"] = new JsonObject { ["Value"] = 120 }
                })
            }
        };
        var package = new AnalysisPackage(
            DateTimeOffset.UtcNow,
            fieldId,
            new JsonObject { ["MetaInfo"] = new JsonObject { ["ID"] = fieldId } },
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = clusterId },
                ["ReferencePoint"] = new JsonObject { ["RiemannianEast"] = 10.0, ["RiemannianNorth"] = 20.0 }
            }],
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = wellId },
                ["ClusterID"] = clusterId
            }],
            [new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = wellBoreId },
                ["WellID"] = wellId
            }],
            [], [], [geology],
            new SourceCounts(1, 1, 1, 1, 0, 0, 1),
            [],
            "reservoir");

        AnalysisResult result = _service.Analyze(package, "Target");

        Assert.That(result.WellSummaries[0].NetPayThicknessM, Is.EqualTo(10));
        Assert.That(result.ReservoirName, Is.EqualTo("Target"));
    }

    [Test]
    public void PackageHash_IsStableForSameCanonicalContent()
    {
        var hasher = new CanonicalJsonHasher();
        Guid fieldId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        JsonNode first = new JsonObject { ["Name"] = "A", ["MetaInfo"] = new JsonObject { ["ID"] = fieldId } };
        JsonNode sameWithDifferentPropertyOrder = new JsonObject { ["MetaInfo"] = new JsonObject { ["ID"] = fieldId }, ["Name"] = "A" };
        var counts = new SourceCounts(1, 0, 0, 0, 0, 0, 0);

        string firstHash = hasher.Compute(fieldId, first, [], [], [], [], [], [], counts, []);
        string secondHash = hasher.Compute(fieldId, sameWithDifferentPropertyOrder, [], [], [], [], [], [], counts, []);

        Assert.That(secondHash, Is.EqualTo(firstHash));
        Assert.That(firstHash, Has.Length.EqualTo(64));
    }

    private static JsonObject GeologicalEntry(double depth, double porosity, double permeability, double pressure) => new()
    {
        ["MeasuredDepth"] = Gaussian(depth),
        ["Porosity"] = Gaussian(porosity),
        ["Permeability"] = Gaussian(permeability),
        ["PressureDifferential"] = Gaussian(pressure)
    };

    private static JsonObject Gaussian(double mean) => new()
    {
        ["GaussianValue"] = new JsonObject { ["Mean"] = mean }
    };
}
