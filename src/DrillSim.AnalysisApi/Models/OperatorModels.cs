using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

public sealed record OperatorSession(
    bool Enabled, string? Reason, string? CsrfRequestToken, string CsrfHeaderName,
    string AuditLabelLimitation);

public sealed record OperatorActionAvailability(bool Enabled, string? Reason);
public sealed record OperatorPrediction(bool Sealed, bool Approved, string? SealHash);
public sealed record OperatorPreflight(bool Ready, string? Reason, bool WorldBound);
public sealed record OperatorStage(string Stage, string Name, string Status, int AttemptCount);
public sealed record OperatorRun(
    Guid RunId, string Status, string? CurrentStage, IReadOnlyList<OperatorStage> Stages,
    int ProgressPercent, string? FailureReason);
public sealed record OperatorCompletion(
    bool Available, bool ApprovalEnabled, string Reason, IReadOnlyList<OperatorOpening> Openings,
    string? OpeningsHash = null, string? Status = null, Guid? RunId = null, Guid? ScenarioId = null);
public sealed record OperatorOpening(
    string ReservoirName, string Type, double TopMdM, double BaseMdM, double WellboreRadiusM,
    double Skin, double Efficiency, double UncertaintyM);
public sealed record OperatorActions(
    OperatorActionAvailability ApprovePrediction, OperatorActionAvailability Start,
    OperatorActionAvailability Cancel, OperatorActionAvailability Resume,
    OperatorActionAvailability ApproveCompletion, OperatorActionAvailability Publish,
    OperatorActionAvailability Score)
{
    public OperatorActionAvailability RecoverPublication { get; init; } = new(false, "No guarded publication recovery is available.");
    public OperatorActionAvailability CorrectScore { get; init; } = new(false, "No guarded score correction is available.");
}
public sealed record OperatorScenarioView(
    bool Enabled, string? Reason, Guid ScenarioId, string? ScenarioStatus,
    OperatorPrediction Prediction, OperatorPreflight Preflight, OperatorRun? Run,
    OperatorCompletion Completion, OperatorActions Actions)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OperatorPublicationRecoveryReview? PublicationRecovery { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OperatorScoringCorrectionReview? ScoreCorrection { get; init; }
}
public sealed record OperatorActionResult(
    Guid ScenarioId, Guid? RunId, string Outcome, string? Reason,
    bool RevealSucceeded = false, string? ScoringStatus = null);

public sealed record OperatorSimulationProfile(string ProfileId, string Name, string Description, string WorldModelVersion);
public sealed record OperatorSimulationDefaults(string? ProfileId, string Resolution, int RealizationSeed);
public sealed record OperatorSimulationConfiguration(
    string ProfileId, string ProfileName, string Resolution, int RealizationSeed, string PreparedBy, DateTimeOffset PreparedUtc);
public sealed record OperatorSimulationSetup(
    Guid ScenarioId, bool Available, string? Reason, bool Prepared, OperatorSimulationConfiguration? Current,
    IReadOnlyList<OperatorSimulationProfile> Profiles, OperatorSimulationDefaults Defaults, string? ReviewedSealHash);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OperatorSimulationSetupRequest(
    [property: JsonRequired] string Actor,
    [property: JsonRequired] string ProfileId,
    [property: JsonRequired] string Resolution,
    [property: JsonRequired] int RealizationSeed,
    [property: JsonRequired] string ReviewedSealHash);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OperatorActorRequest(string? Actor);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OperatorPredictionApprovalRequest(string? Actor, string? ReviewedSealHash);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OperatorCompletionApprovalRequest(string? Actor, string? ReviewedOpeningHash);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OperatorPublicationRecoveryRequest(string? Actor, string? Reason, string? ReviewedPublicationHash);

public sealed record OperatorPublicationRecoveryReview(
    Guid ScenarioId, Guid RunId, bool RecoveryEnabled, string Reason,
    string? ReviewedPublicationHash = null, string? StagedManifestSha256 = null,
    string? PublicationPlanSha256 = null, int OperationCount = 0,
    int VerifiedOperationCount = 0, int PendingOperationCount = 0, int CompletedStageCount = 0);

public sealed record OperatorPublicationRecoveryResult(
    Guid ScenarioId, Guid RunId, string Outcome, string PreviousStatus, string Status,
    string ReviewedPublicationHash, Guid AuditId);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record OperatorScoringCorrectionRequest(string? Actor, string? Reason, string? ReviewedCorrectionHash);

public sealed record OperatorScoringCorrectionReview(
    Guid ScenarioId, Guid RunId, bool CorrectionEnabled, string Reason,
    Guid? RejectedScorecardId = null, string? RejectedScorecardSha256 = null,
    string? ScoringInputSha256 = null, string? CorrectionVersion = null,
    int MetricCount = 0, int InvalidMetricCount = 0, int ChangedMetricCount = 0,
    string? ReviewedCorrectionHash = null, Guid? CorrectedScorecardId = null,
    string? CorrectedScorecardSha256 = null, string? CorrectedInputSha256 = null);

public sealed record OperatorScoringCorrectionResult(
    Guid ScenarioId, Guid RunId, string Outcome, string Status,
    Guid RejectedScorecardId, string RejectedScorecardSha256, string ScoringInputSha256,
    Guid CorrectedScorecardId, string CorrectedScorecardSha256, string CorrectedInputSha256,
    string CorrectionVersion, string ReviewedCorrectionHash, Guid AuditId);
