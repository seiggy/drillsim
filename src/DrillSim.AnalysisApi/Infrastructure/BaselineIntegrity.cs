using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Infrastructure;

internal static class BaselineIntegrity
{
    public static BaselineSnapshot Finalize(BaselineSnapshot baseline)
    {
        JsonNode content = CreateContentNode(baseline);
        string contentSha256 = CanonicalJsonHasher.ComputeCanonicalSha256(content);
        Guid baselineId = DeterministicGuid.Create(CreateIdentityNode(content, contentSha256));
        return baseline with { BaselineId = baselineId, ContentSha256 = contentSha256 };
    }

    public static void Validate(BaselineSnapshot baseline)
    {
        JsonNode content = CreateContentNode(baseline);
        string expectedHash = CanonicalJsonHasher.ComputeCanonicalSha256(content);
        Guid expectedId = DeterministicGuid.Create(CreateIdentityNode(content, expectedHash));
        if (!string.Equals(baseline.ContentSha256, expectedHash, StringComparison.Ordinal) ||
            baseline.BaselineId != expectedId)
        {
            throw new InvalidDataException(
                $"Baseline {baseline.Kind} for scenario {baseline.ScenarioId:D} failed content integrity validation.");
        }
    }

    public static string ComputeBundleSha256(IReadOnlyList<BaselineSnapshot> baselines)
    {
        var bundle = new JsonArray(baselines
            .OrderBy(item => item.Kind)
            .Select(item => (JsonNode)new JsonObject
            {
                ["kind"] = item.Kind.ToString(),
                ["baselineId"] = item.BaselineId,
                ["contentSha256"] = item.ContentSha256
            })
            .ToArray());
        return CanonicalJsonHasher.ComputeCanonicalSha256(bundle);
    }

    private static JsonNode CreateIdentityNode(JsonNode content, string contentSha256)
    {
        var identity = (JsonObject)content.DeepClone();
        identity["contentSha256"] = contentSha256;
        return identity;
    }

    private static JsonNode CreateContentNode(BaselineSnapshot baseline)
    {
        var content = new BaselineContent(
            baseline.ScenarioId,
            baseline.Kind,
            baseline.ModelVersion,
            baseline.PackageSha256,
            baseline.CandidateId,
            baseline.TargetEastingM,
            baseline.TargetNorthingM,
            baseline.ContributingEvidenceIds,
            baseline.FormationTopTrueVerticalDepthM,
            baseline.FormationBaseTrueVerticalDepthM,
            baseline.ExpectedPaydirtM,
            baseline.FluidClasses,
            baseline.ContactPredictions,
            baseline.ProductionForecasts,
            baseline.Limitation,
            baseline.AnalysisBinding);
        return JsonNode.Parse(PredictionJson.Canonicalize(content))
            ?? throw new InvalidOperationException("Could not canonicalize baseline content.");
    }

    private sealed record BaselineContent(
        Guid ScenarioId,
        BaselineKind Kind,
        string ModelVersion,
        string PackageSha256,
        string CandidateId,
        double TargetEastingM,
        double TargetNorthingM,
        IReadOnlyList<string> ContributingEvidenceIds,
        QuantileValues? FormationTopTrueVerticalDepthM,
        QuantileValues? FormationBaseTrueVerticalDepthM,
        QuantileValues? ExpectedPaydirtM,
        IReadOnlyList<PredictedFluidClass>? FluidClasses,
        IReadOnlyList<FluidContactPrediction>? ContactPredictions,
        IReadOnlyList<ProductionForecast>? ProductionForecasts,
        string Limitation,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        BaselineAnalysisBinding? AnalysisBinding);
}
