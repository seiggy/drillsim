using System.Text.Json.Nodes;

namespace DrillSim.AnalysisApi.Infrastructure;

public static class ScenarioModelVersions
{
    public const string World = "reservoir-hidden-world-v3";
    public const string Observation = "observation-model-v1";
    // v2 scores hidden-truth expected paydirt with the sealed prediction's own rock cutoffs; frozen v1 scenarios
    // keep scoring-model-v1 (and its absolute-error-bounds correction) forever.
    public const string Scoring = "scoring-model-v2";
    public const string LegacyScoring = "scoring-model-v1";

    public static bool IsSupportedScoring(string? version) =>
        version is Scoring or LegacyScoring;
}

internal static class ScenarioIdentity
{
    public static Guid Create(
        Guid sourceFieldId,
        string reservoirName,
        DateTimeOffset asOfUtc,
        string seedLabel,
        string assumptionsSha256)
    {
        var identity = new JsonObject
        {
            ["identityVersion"] = "scenario-identity-v1",
            ["sourceFieldId"] = sourceFieldId,
            ["reservoirName"] = reservoirName,
            ["asOfUtc"] = asOfUtc.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            ["seedLabel"] = seedLabel,
            ["assumptionsSha256"] = assumptionsSha256,
            ["worldModelVersion"] = ScenarioModelVersions.World,
            ["observationModelVersion"] = ScenarioModelVersions.Observation,
            ["scoringModelVersion"] = ScenarioModelVersions.Scoring
        };

        return DeterministicGuid.Create(identity);
    }
}
