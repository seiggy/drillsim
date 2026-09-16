using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Tests;

internal static class RevealPackageTestData
{
    internal static AnalysisPackage Create(Guid fieldId, DateTimeOffset generatedAt)
    {
        Guid clusterId = Guid.NewGuid();
        Guid wellId = Guid.NewGuid();
        JsonNode field = Entity(fieldId);
        JsonNode cluster = Entity(clusterId, ("FieldID", fieldId));
        JsonArray months = new(Enumerable.Range(1, 60)
            .Select(month => (JsonNode)new JsonObject
            {
                ["Year"] = 2026 + (month - 1) / 12,
                ["Month"] = (month - 1) % 12 + 1,
                ["Oil"] = new JsonObject { ["Value"] = month * 10d, ["Unit"] = "m3" },
                ["Gas"] = new JsonObject { ["Value"] = month * 20d, ["Unit"] = "m3" },
                ["Water"] = new JsonObject { ["Value"] = month * 5d, ["Unit"] = "m3" },
                ["DaysOnProduction"] = 30,
                ["IsAllocated"] = true,
                ["Classification"] = "Synthetic"
            }).ToArray());
        JsonNode well = Entity(wellId, ("ClusterID", clusterId));
        well["Dataset"] = new JsonObject { ["MonthlyProduction"] = months };
        var counts = new SourceCounts(1, 1, 1, 0, 0, 0, 0);
        string[] gaps =
        [
            "No trajectories were returned for the field.",
            "No wellbores were returned for the field wells."
        ];
        string hash = new CanonicalJsonHasher().Compute(
            fieldId, field, [cluster], [well], [], [], [], [], counts, gaps);
        return new(
            generatedAt,
            fieldId,
            field,
            [cluster],
            [well],
            [],
            [],
            [],
            [],
            counts,
            gaps,
            hash);
    }

    internal static IReadOnlyList<RevealEvidenceRequest> Evidence(AnalysisPackage package) =>
    [
        Commitment(package.FieldId, "Field", package.Field),
        Commitment(JsonAccess.MetaId(package.Clusters.Single())!.Value, "Cluster", package.Clusters.Single()),
        Commitment(JsonAccess.MetaId(package.Wells.Single())!.Value, "Well", package.Wells.Single())
    ];

    internal static string ProductionHash(AnalysisPackage package) =>
        RevealPackageValidator.BusinessContentHash(
            package.Wells.Single()["Dataset"]!["MonthlyProduction"]!);

    private static RevealEvidenceRequest Commitment(Guid id, string kind, JsonNode record) =>
        new(id.ToString("D"), kind, RevealPackageValidator.BusinessContentHash(record));

    private static JsonObject Entity(Guid id, params (string Name, Guid Value)[] properties)
    {
        var result = new JsonObject { ["MetaInfo"] = new JsonObject { ["ID"] = id } };
        foreach ((string name, Guid value) in properties)
            result[name] = value;
        return result;
    }
}
