using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class AnalysisConfigurationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Test]
    public void DefaultAnalysis_PreservesLegacyOutputs()
    {
        AnalysisResult result = new PetrophysicsAnalysisService(TimeProvider.System).Analyze(CreatePackage(), "Target");
        JsonNode legacy = JsonSerializer.SerializeToNode(new
        {
            result.FieldId, result.ReservoirName, result.PackageSha256, result.Methodology,
            result.WellSummaries, result.Ranking, result.DataGaps
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        foreach (JsonNode? candidate in legacy["ranking"]!.AsArray())
        {
            candidate!.AsObject().Remove("scoreComponents");
            candidate.AsObject().Remove("uncertaintyComponents");
        }
        Assert.That(CanonicalJsonHasher.ComputeCanonicalSha256(legacy),
            Is.EqualTo("30e5715a6f6c504c243a9f9ab42f1b19fb81149c5cf4b024460ede784f134b73"));
    }

    [Test]
    public void ExplicitDefaults_EqualImplicitDefaultsAndDoNotChangeCandidateIdentity()
    {
        var service = new PetrophysicsAnalysisService(new FixedClock(DateTimeOffset.UnixEpoch));
        AnalysisPackage package = CreatePackage();
        AnalysisResult implicitResult = service.Analyze(package, "Target");
        AnalysisResult explicitResult = service.Analyze(package, "Target", AnalysisConfiguration.Default);
        Assert.That(PredictionJson.Canonicalize(explicitResult), Is.EqualTo(PredictionJson.Canonicalize(implicitResult)));
        Assert.That(explicitResult.Ranking.All(candidate => candidate.CandidateId.StartsWith("candidate:", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void Cutoffs_ChangePayAndFingerprintsWithoutChangingPermeabilityUnitConversion()
    {
        var service = new PetrophysicsAnalysisService(TimeProvider.System);
        AnalysisPackage package = CreatePackage();
        AnalysisResult original = service.Analyze(package, "Target");
        AnalysisResult porosity = service.Analyze(package, "Target", AnalysisConfiguration.Default with { PorosityCutoff = .25 });
        AnalysisResult permeability = service.Analyze(package, "Target", AnalysisConfiguration.Default with { PermeabilityCutoffM2 = 4e-15 });
        Assert.Multiple(() =>
        {
            Assert.That(porosity.WellSummaries.Sum(well => well.NetPayThicknessM),
                Is.LessThan(original.WellSummaries.Sum(well => well.NetPayThicknessM)));
            Assert.That(permeability.WellSummaries.Sum(well => well.NetPayThicknessM),
                Is.LessThan(original.WellSummaries.Sum(well => well.NetPayThicknessM)));
            Assert.That(porosity.ConfigurationSha256, Is.Not.EqualTo(original.ConfigurationSha256));
            Assert.That(permeability.AnalysisSha256, Is.Not.EqualTo(original.AnalysisSha256));
            Assert.That(porosity.Ranking.All(candidate => candidate.CandidateId.StartsWith("configured:", StringComparison.Ordinal)), Is.True);
        });
        RankedCandidate candidate = permeability.Ranking[0];
        WellPaySummary[] neighbors = candidate.NeighborEvidenceIds
            .Select(id => permeability.WellSummaries.Single(well => well.WellEvidenceId == id)).ToArray();
        double[] weights = neighbors.Select(well => 1 / Math.Max(1,
            Math.Pow(well.EastingM - candidate.EastingM, 2) + Math.Pow(well.NorthingM - candidate.NorthingM, 2))).ToArray();
        double weightedM2 = neighbors.Select((well, i) => well.MeanPayPermeabilityM2 * weights[i]).Sum() / weights.Sum();
        Assert.That(candidate.MeanPayPermeabilityMd, Is.EqualTo(weightedM2 / 9.869233e-16).Within(1e-10));
    }

    [Test]
    public void FullGrid_IncludesEveryBoundedPointAndHonestExclusionReasons()
    {
        var service = new PetrophysicsAnalysisService(TimeProvider.System);
        AnalysisResult result = service.Analyze(CreatePackage(withTrajectories: true), "Target",
            AnalysisConfiguration.Default with { GridPointsPerAxis = 9 });
        Assert.Multiple(() =>
        {
            Assert.That(result.CandidateGrid, Has.Count.EqualTo(81));
            Assert.That(result.CandidateGridBounds, Is.EqualTo(new CandidateGridBounds(0, 4000, 0, 4000)));
            Assert.That(result.CandidateGrid.Select(point => point.CandidateId), Is.Unique);
            Assert.That(result.CandidateGrid.All(point => point.EastingM >= 0 && point.EastingM <= 4000 &&
                point.NorthingM >= 0 && point.NorthingM <= 4000), Is.True);
            Assert.That(result.CandidateGrid.Count(point => point.Status == "excluded"), Is.EqualTo(5));
            Assert.That(result.CandidateGrid.Where(point => point.Status == "excluded")
                .All(point => point.Reasons.Contains("within-well-exclusion-radius") && point.Prediction is null), Is.True);
            Assert.That(result.CandidateGrid.Where(point => point.Status == "eligible")
                .All(point => point.Reasons.Count == 0 && point.Prediction is not null), Is.True);
        });
        foreach (RankedCandidate ranked in result.Ranking)
            Assert.That(result.CandidateGrid.Single(point => point.CandidateId == ranked.CandidateId).Prediction, Is.EqualTo(ranked));

        AnalysisResult noExclusions = service.Analyze(CreatePackage(withTrajectories: true), "Target",
            AnalysisConfiguration.Default with { GridPointsPerAxis = 9, WellExclusionRadiusM = 0 });
        Assert.That(noExclusions.CandidateGrid.All(point => point.Status == "eligible"), Is.True);
        AnalysisResult excluded = service.Analyze(CreatePackage(withTrajectories: true), "Target",
            AnalysisConfiguration.Default with { GridPointsPerAxis = 9, WellExclusionRadiusM = 100_000 });
        Assert.That(excluded.CandidateGrid.All(point => point.Status == "excluded"), Is.True);
        Assert.That(excluded.Ranking, Is.Empty);
    }

    [Test]
    public void NeighborCount_ChangesInterpolationAndReportsUnsupportedGridWithoutInventingValues()
    {
        var service = new PetrophysicsAnalysisService(TimeProvider.System);
        AnalysisPackage package = CreatePackage();
        AnalysisResult one = service.Analyze(package, "Target", AnalysisConfiguration.Default with { IdwNeighborCount = 1 });
        AnalysisResult four = service.Analyze(package, "Target");
        CandidateGridPoint point = one.CandidateGrid.First(point => point.Status == "eligible");
        CandidateGridPoint equivalent = four.CandidateGrid.Single(other => other.EastingM == point.EastingM && other.NorthingM == point.NorthingM);
        Assert.That(point.Prediction!.NeighborEvidenceIds, Has.Count.EqualTo(1));
        Assert.That(point.Prediction.P50NetPayM, Is.Not.EqualTo(equivalent.Prediction!.P50NetPayM));
        Assert.That(point.Prediction.UncertaintyComponents!.DisagreementVarianceM2, Is.Zero);
        AnalysisResult unsupported = service.Analyze(package, "Target",
            AnalysisConfiguration.Default with { IdwNeighborCount = 6, WellExclusionRadiusM = 0 });
        Assert.That(unsupported.CandidateGrid, Has.Count.EqualTo(225));
        Assert.That(unsupported.CandidateGrid.All(point => point.Status == "unsupported" &&
            point.Prediction is null && point.Reasons.Contains("insufficient-located-well-controls")), Is.True);
        Assert.That(unsupported.Ranking, Is.Empty);
    }

    [Test]
    public void GridWork_IsBoundedAndEmptyControlsDoNotEraseTheSearchArea()
    {
        var service = new PetrophysicsAnalysisService(TimeProvider.System);
        AnalysisPackage package = CreatePackage();
        AnalysisResult maximum = service.Analyze(package, "Target",
            AnalysisConfiguration.Default with { GridPointsPerAxis = 51 });
        Assert.That(maximum.CandidateGrid, Has.Count.EqualTo(2601));
        AnalysisResult unsupported = service.Analyze(package with { GeologicalProperties = [] }, "Target");
        Assert.That(unsupported.CandidateGrid, Has.Count.EqualTo(225));
        Assert.That(unsupported.CandidateGrid.All(point => point.Status == "unsupported" &&
            point.NearestWellDistanceM is null && point.Prediction is null), Is.True);
        AnalysisResult noExtent = service.Analyze(package with { Clusters = [] }, "Target");
        Assert.That(noExtent.CandidateGridBounds, Is.Null);
        Assert.That(noExtent.CandidateGrid, Is.Empty);
        Assert.That(noExtent.DataGaps.Any(gap => gap.Contains("candidate-grid extent", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void ScoreAndUncertaintyComponents_ReconcileToQuantilesAndRanking()
    {
        AnalysisResult result = new PetrophysicsAnalysisService(TimeProvider.System).Analyze(CreatePackage(), "Target");
        foreach (RankedCandidate candidate in result.CandidateGrid.Where(point => point.Prediction is not null).Select(point => point.Prediction!))
        {
            CandidateScoreComponents score = candidate.ScoreComponents!;
            CandidateUncertaintyComponents uncertainty = candidate.UncertaintyComponents!;
            Assert.Multiple(() =>
            {
                Assert.That(candidate.Score, Is.EqualTo(score.UnpenalizedScore / score.UncertaintyDivisor));
                Assert.That(candidate.SigmaNetPayM, Is.EqualTo(Math.Sqrt(
                    uncertainty.DisagreementVarianceM2 + Math.Pow(uncertainty.DistanceSigmaM, 2))));
                Assert.That(candidate.P90NetPayM, Is.EqualTo(Math.Max(0,
                    candidate.P50NetPayM - uncertainty.QuantileZScore * uncertainty.SigmaNetPayM)));
                Assert.That(uncertainty.Calibrated, Is.False);
            });
        }
    }

    [Test]
    public void Fingerprints_BindEvidenceReservoirAndConfigurationButExcludeWallClock()
    {
        AnalysisPackage package = CreatePackage();
        AnalysisResult original = new PetrophysicsAnalysisService(new FixedClock(DateTimeOffset.UnixEpoch))
            .Analyze(package, "Target");
        var laterService = new PetrophysicsAnalysisService(new FixedClock(DateTimeOffset.UnixEpoch.AddYears(20)));
        AnalysisResult later = laterService.Analyze(package with { GeneratedAt = package.GeneratedAt.AddDays(1) }, "Target");
        Assert.That(later.GeneratedAt, Is.Not.EqualTo(original.GeneratedAt));
        Assert.That(later.AnalysisSha256, Is.EqualTo(original.AnalysisSha256));
        Assert.That(later.ConfigurationSha256, Is.EqualTo(original.ConfigurationSha256));
        Assert.That(laterService.Analyze(package with { Sha256 = new string('a', 64) }, "Target").AnalysisSha256,
            Is.Not.EqualTo(original.AnalysisSha256));
        Assert.That(laterService.Analyze(package, "Other").AnalysisSha256, Is.Not.EqualTo(original.AnalysisSha256));
        Assert.That(laterService.Analyze(package, "Target",
            AnalysisConfiguration.Default with { GridPointsPerAxis = 10 }).AnalysisSha256, Is.Not.EqualTo(original.AnalysisSha256));
    }

    [TestCaseSource(nameof(InvalidConfigurations))]
    public void InvalidConfiguration_RejectsBoundsVersionsAndNonfiniteValues(AnalysisConfiguration configuration)
    {
        var exception = Assert.Throws<ScenarioApiException>(() =>
            new PetrophysicsAnalysisService(TimeProvider.System).Analyze(CreatePackage(), "Target", configuration));
        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void ConfigurationJson_RejectsMissingUnknownAndStringNumberProperties()
    {
        string valid = JsonSerializer.Serialize(new ConfiguredAnalysisRequest(AnalysisConfiguration.Default), JsonOptions);
        foreach (string property in new[] { "version", "porosityCutoff", "permeabilityCutoffM2",
                     "wellExclusionRadiusM", "gridPointsPerAxis", "idwNeighborCount" })
        {
            JsonNode json = JsonNode.Parse(valid)!;
            json["configuration"]!.AsObject().Remove(property);
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ConfiguredAnalysisRequest>(json.ToJsonString(), JsonOptions));
        }
        foreach (Action<JsonNode> mutate in new Action<JsonNode>[]
        {
            json => json["configuration"]!["gridExtent"] = 12,
            json => json["configuration"]!["porosityCutoff"] = "0.12",
            json => json["configuration"]!["idwNeighborCount"] = "4",
            json => json["configuration"]!["porosityCutoff"] = "NaN",
            json => json["extraScope"] = "secret",
            json => json.AsObject().Remove("configuration")
        })
        {
            JsonNode json = JsonNode.Parse(valid)!;
            mutate(json);
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ConfiguredAnalysisRequest>(json.ToJsonString(), JsonOptions));
        }
        Assert.Throws<ScenarioApiException>(() => AnalysisConfiguration.Validate(null));
    }

    private static IEnumerable<AnalysisConfiguration> InvalidConfigurations()
    {
        AnalysisConfiguration defaults = AnalysisConfiguration.Default;
        yield return defaults with { Version = "v2" };
        yield return defaults with { PorosityCutoff = -.1 };
        yield return defaults with { PorosityCutoff = 1.1 };
        yield return defaults with { PorosityCutoff = double.NaN };
        yield return defaults with { PermeabilityCutoffM2 = 0 };
        yield return defaults with { PermeabilityCutoffM2 = 1e-7 };
        yield return defaults with { PermeabilityCutoffM2 = double.PositiveInfinity };
        yield return defaults with { WellExclusionRadiusM = -1 };
        yield return defaults with { WellExclusionRadiusM = 100_001 };
        yield return defaults with { WellExclusionRadiusM = double.NegativeInfinity };
        yield return defaults with { GridPointsPerAxis = 1 };
        yield return defaults with { GridPointsPerAxis = 52 };
        yield return defaults with { IdwNeighborCount = 0 };
        yield return defaults with { IdwNeighborCount = 33 };
    }

    private sealed class FixedClock(DateTimeOffset instant) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => instant;
    }

    internal static AnalysisPackage CreatePackage(bool withTrajectories = false)
    {
        Guid fieldId = Id(1, 1);
        var field = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = fieldId },
            ["ReferencePoint"] = Point(0, 0)
        };
        var clusters = new List<JsonNode>();
        var wells = new List<JsonNode>();
        var bores = new List<JsonNode>();
        var geology = new List<JsonNode>();
        var trajectories = new List<JsonNode>();
        (double East, double North)[] positions = [(0, 0), (4000, 0), (0, 4000), (4000, 4000), (2000, 2000)];
        for (int i = 0; i < positions.Length; i++)
        {
            clusters.Add(new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = Id(2, i) },
                ["ReferencePoint"] = Point(positions[i].East, positions[i].North)
            });
            wells.Add(new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = Id(3, i) }, ["ClusterID"] = Id(2, i)
            });
            bores.Add(new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = Id(4, i) }, ["WellID"] = Id(3, i)
            });
            if (withTrajectories)
                trajectories.Add(new JsonObject
                {
                    ["MetaInfo"] = new JsonObject { ["ID"] = Id(6, i) }, ["WellID"] = Id(3, i),
                    ["TieInPoint"] = Point(positions[i].East, positions[i].North)
                });
            geology.Add(new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = Id(5, i) }, ["WellBoreID"] = Id(4, i),
                ["Petrophysics"] = new JsonObject
                {
                    ["FormationIntervals"] = new JsonArray(new JsonObject
                    {
                        ["FormationName"] = "Target",
                        ["TopDepth"] = new JsonObject { ["Value"] = 100 },
                        ["BaseDepth"] = new JsonObject { ["Value"] = 300 }
                    })
                },
                ["GeologicalPropertyTable"] = new JsonArray(
                    Entry(100, .14 + i * .02, (i + 1) * 1e-15),
                    Entry(110 + i * 5, .18 + i * .02, (i + 1) * 2e-15),
                    Entry(120 + i * 10, .22 + i * .02, (i + 1) * 3e-15))
            });
        }
        var counts = new SourceCounts(1, 5, 5, 5, 0, trajectories.Count, 5);
        string hash = new CanonicalJsonHasher().Compute(fieldId, field, clusters, wells, bores, [], trajectories, geology, counts, []);
        return new AnalysisPackage(DateTimeOffset.Parse("2026-01-01T00:00:00Z"), fieldId,
            field, clusters, wells, bores, [], trajectories, geology, counts, [], hash);
    }

    private static Guid Id(int kind, int i) => Guid.Parse($"{kind:D8}-0000-0000-0000-{i:D12}");
    private static JsonObject Point(double east, double north) => new()
    {
        ["RiemannianEast"] = east, ["RiemannianNorth"] = north
    };
    private static JsonObject Entry(double depth, double porosity, double permeability) => new()
    {
        ["MeasuredDepth"] = Gaussian(depth), ["Porosity"] = Gaussian(porosity),
        ["Permeability"] = Gaussian(permeability), ["PressureDifferential"] = Gaussian(1)
    };
    private static JsonObject Gaussian(double mean) => new()
    {
        ["GaussianValue"] = new JsonObject { ["Mean"] = mean }
    };
}
