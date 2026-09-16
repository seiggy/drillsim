using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisScope(
    [property: JsonRequired] Guid FieldId,
    [property: JsonRequired] string ReservoirName,
    [property: JsonRequired] Guid? ScenarioId,
    [property: JsonRequired] DateTimeOffset? AsOfUtc);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisControlNote(
    [property: JsonRequired] string EvidenceId,
    [property: JsonRequired] string Note);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisRevisionInput(
    [property: JsonRequired] AnalysisConfiguration Configuration,
    [property: JsonRequired] string PackageSha256,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string SelectedCandidateId,
    [property: JsonRequired] string Rationale,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? CorrelationNotes = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<HypothesisControlNote>? ControlNotes = null);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisReference(
    [property: JsonRequired] Guid HypothesisId,
    [property: JsonRequired] int Revision);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisCreateRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] HypothesisRevisionInput Input);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisReviseRequest(
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] HypothesisRevisionInput Input);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisBranchRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] HypothesisReference Source,
    [property: JsonRequired] HypothesisRevisionInput Input);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisAnalysisRequest(
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] AnalysisConfiguration Configuration);

public sealed record HypothesisRevision(
    string SchemaVersion,
    Guid HypothesisId,
    int Revision,
    string Name,
    HypothesisScope Scope,
    HypothesisRevisionInput Input,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] HypothesisReference? BranchedFrom,
    DateTimeOffset CreatedUtc,
    AnalysisPackage Package,
    AnalysisResult Analysis,
    string SnapshotSha256);

public sealed record HypothesisSummary(
    Guid HypothesisId,
    int Revision,
    string Name,
    HypothesisScope Scope,
    string PackageSha256,
    string ConfigurationSha256,
    string AnalysisSha256,
    string SelectedCandidateId,
    string SnapshotSha256,
    DateTimeOffset CreatedUtc);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisObjectionInput(
    [property: JsonRequired] string Text,
    [property: JsonRequired] IReadOnlyList<string> CitedEvidenceIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisChallengeRequest(
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string Summary,
    [property: JsonRequired] string Actor,
    [property: JsonRequired] IReadOnlyList<HypothesisObjectionInput> Objections);

public sealed record HypothesisObjection(
    Guid ObjectionId,
    string Text,
    IReadOnlyList<string> CitedEvidenceIds,
    string Disposition,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? DispositionReason,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? DispositionActor);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisDisposition(
    [property: JsonRequired] Guid ObjectionId,
    [property: JsonRequired] string Disposition,
    [property: JsonRequired] string Reason);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisDispositionRequest(
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] string Actor,
    [property: JsonRequired] IReadOnlyList<HypothesisDisposition> Dispositions);

public sealed record HypothesisChallenge(
    string SchemaVersion,
    Guid ChallengeId,
    int Version,
    HypothesisReference Hypothesis,
    string HypothesisSnapshotSha256,
    string AnalysisSha256,
    string Summary,
    string CreatedBy,
    string LastModifiedBy,
    DateTimeOffset CreatedUtc,
    DateTimeOffset ModifiedUtc,
    IReadOnlyList<HypothesisObjection> Objections,
    string Sha256);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisComparisonSelection(
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] HypothesisReference Reference);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record HypothesisCompareRequest(
    [property: JsonRequired] IReadOnlyList<HypothesisComparisonSelection> Revisions);

public sealed record HypothesisDifference(string Field, JsonNode? BaselineValue, JsonNode? Value);

public sealed record HypothesisComparisonEntry(
    HypothesisSummary Revision,
    AnalysisConfiguration Configuration,
    RankedCandidate SelectedCandidate,
    string Rationale,
    string? CorrelationNotes,
    IReadOnlyList<HypothesisControlNote>? ControlNotes,
    bool LikeForLikeEvidence,
    IReadOnlyList<string> ScopeDifferences,
    IReadOnlyList<HypothesisDifference> Differences);

public sealed record HypothesisComparison(
    string SchemaVersion,
    HypothesisReference Baseline,
    IReadOnlyList<HypothesisComparisonEntry> Entries,
    string ComparisonSha256);
