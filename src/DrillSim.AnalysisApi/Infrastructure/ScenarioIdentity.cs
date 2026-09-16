using System.Text.Json.Nodes;

namespace DrillSim.AnalysisApi.Infrastructure;

public static class ScenarioModelVersions
{
    public const string World = "reservoir-hidden-world-v3";
    public const string Observation = "observation-model-v1";
    public const string Scoring = "scoring-model-v1";
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
