using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FormationInterpretationNotes(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Rationale,
    [property: JsonRequired] string CorrelationNotes,
    [property: JsonRequired] IReadOnlyList<HypothesisControlNote> ControlNotes);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FormationInterpretationRequest(
    [property: JsonRequired] HypothesisScope Scope,
    [property: JsonRequired] AnalysisConfiguration Configuration,
    [property: JsonRequired] string PackageSha256,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string? SelectedCandidateId,
    [property: JsonRequired] HypothesisReference? SavedHypothesis,
    [property: JsonRequired] string? SnapshotSha256,
    [property: JsonRequired] FormationInterpretationNotes Notes);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FormationInterpretationDraft(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Rationale,
    [property: JsonRequired] string CorrelationNotes,
    [property: JsonRequired] IReadOnlyList<string> CitedEvidenceIds,
    [property: JsonRequired] IReadOnlyList<string> Limitations);

public sealed record FormationInterpretationStatus(bool Configured, string? Reason);

public sealed record FormationInterpretationResponse(
    string Version,
    string PromptVersion,
    HypothesisScope Scope,
    string PackageSha256,
    string AnalysisSha256,
    string ConfigurationSha256,
    string? SelectedCandidateId,
    HypothesisReference? SavedHypothesis,
    string? SnapshotSha256,
    DateTimeOffset GeneratedAt,
    FormationInterpretationDraft Draft);
