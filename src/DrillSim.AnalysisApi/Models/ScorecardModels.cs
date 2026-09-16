using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

public enum ScoreMetricBasis
{
    HiddenTruth,
    RevealedObservation,
    ObservationGap,
    Baseline
}

public enum ScoreMetricStatus
{
    Scored,
    Unavailable
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ScorecardRequest(
    string ScorecardId,
    string RunId,
    string RevealId,
    string ScoringModelVersion,
    string InputSha256,
    string? HeadlineMetric,
    IReadOnlyList<ScorecardMetric> Metrics,
    DateTimeOffset CreatedValidTimeUtc,
    string? Limitation)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScorecardCorrectionProvenance? Correction { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ScorecardMetric(
    string Name,
    ScoreMetricBasis Basis,
    ScoreMetricStatus Status,
    double? Value,
    string? Unit,
    double? LowerBound,
    double? UpperBound,
    string? Limitation);

public sealed record PublicScorecard(
    Guid ScenarioId,
    Guid ScorecardId,
    Guid RevealId,
    string ScoringModelVersion,
    string InputSha256,
    string? HeadlineMetric,
    IReadOnlyList<ScorecardMetric> Metrics,
    DateTimeOffset CreatedValidTimeUtc,
    string? Limitation,
    string ContentSha256)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScorecardCorrectionProvenance? Correction { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ScorecardCorrectionProvenance(
    string CorrectionVersion, string OriginalScoringModelVersion,
    string SupersedesScorecardId, string SupersedesOutputSha256, string OriginalInputSha256);

public static class ScoringCorrectionVersions
{
    public const string Correction = "absolute-error-bounds-v2";
    public const string ScoringModel = "scoring-model-v1-absolute-error-bounds-v2";
}
