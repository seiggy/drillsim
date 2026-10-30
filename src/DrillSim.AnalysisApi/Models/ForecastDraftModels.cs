using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ForecastDraftRequest(
    [property: JsonRequired] Guid ScenarioId,
    [property: JsonRequired] string CandidateId,
    [property: JsonRequired] AnalysisConfiguration Configuration,
    [property: JsonRequired] string PackageSha256,
    [property: JsonRequired] string AnalysisSha256);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ForecastDraft(
    [property: JsonRequired] double TotalDepthM,
    [property: JsonRequired] QuantileValues ExpectedPaydirtM,
    [property: JsonRequired] QuantileValues FormationTopTrueVerticalDepthM,
    [property: JsonRequired] QuantileValues FormationBaseTrueVerticalDepthM,
    [property: JsonRequired] IReadOnlyList<PredictedFluidClass> FluidClasses,
    [property: JsonRequired] IReadOnlyList<FluidContactPrediction> ContactPredictions,
    [property: JsonRequired] IReadOnlyList<ProductionForecast> ProductionForecasts,
    [property: JsonRequired] IReadOnlyList<string> UncertaintyAssumptions,
    [property: JsonRequired] IReadOnlyList<string> CitedEvidenceIds,
    [property: JsonRequired] string Rationale,
    [property: JsonRequired] IReadOnlyList<string> Limitations);

public sealed record ForecastDraftResponse(
    string Version,
    string PromptVersion,
    Guid ScenarioId,
    string CandidateId,
    string PackageSha256,
    string AnalysisSha256,
    string ConfigurationSha256,
    DateTimeOffset GeneratedAt,
    FormationStructureEstimate StructureEstimate,
    ForecastDraft Draft);

public sealed record FormationStructureControl(
    string GeologyEvidenceId,
    string? WellEvidenceId,
    double EastingM,
    double NorthingM,
    double DistanceM,
    double TopTrueVerticalDepthM,
    double BaseTrueVerticalDepthM,
    string DepthBasis);

public sealed record FormationStructureEstimate(
    string Method,
    double TargetEastingM,
    double TargetNorthingM,
    int ControlCount,
    QuantileValues? TopTrueVerticalDepthM,
    QuantileValues? BaseTrueVerticalDepthM,
    double? GrossThicknessM,
    IReadOnlyList<FormationStructureControl> NearestControls,
    IReadOnlyList<string> Limitations);
