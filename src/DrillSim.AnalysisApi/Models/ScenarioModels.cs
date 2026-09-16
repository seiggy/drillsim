namespace DrillSim.AnalysisApi.Models;

public enum ScenarioStatus
{
    Draft,
    Armed,
    PredictionDrafted,
    PredictionSealed,
    HumanApproved,
    WorldBound,
    Queued,
    Drilling,
    Surveying,
    Logging,
    CompletionDesigned,
    Producing,
    ReadyToReveal,
    Revealed,
    Scored,
    Cancelled,
    Failed,
    PublishFailed,
    Reset
}

public sealed record Scenario(
    Guid ScenarioId,
    Guid SourceFieldId,
    Guid? ClonedFieldId,
    string ReservoirName,
    DateTimeOffset InitialAsOfUtc,
    DateTimeOffset AsOfUtc,
    string SeedLabel,
    string WorldModelVersion,
    string ObservationModelVersion,
    string ScoringModelVersion,
    ScenarioStatus Status,
    string AssumptionsSha256,
    DateTimeOffset CreatedUtc,
    DateTimeOffset ModifiedUtc);

public sealed record EvidenceVisibility(
    Guid ScenarioId,
    string EvidenceId,
    string RecordKind,
    DateTimeOffset VisibleFromUtc,
    DateTimeOffset? VisibleUntilUtc,
    Guid? RevealId);

public sealed record CreateScenarioRequest(
    Guid SourceFieldId,
    string ReservoirName,
    DateTimeOffset AsOfUtc,
    string SeedLabel,
    string AssumptionsSha256);

public enum EvidenceVisibilityStatus
{
    Visible,
    Hidden
}

public sealed record EvidenceVisibilityCount(
    string RecordKind,
    EvidenceVisibilityStatus Status,
    int Count,
    IReadOnlyList<string>? EvidenceIds);

public sealed record EvidenceVisibilitySummary(
    Guid ScenarioId,
    DateTimeOffset AsOfUtc,
    IReadOnlyList<EvidenceVisibilityCount> Counts);

public sealed record ScenarioClock(
    Scenario Scenario,
    EvidenceVisibilitySummary EvidenceVisibility);

