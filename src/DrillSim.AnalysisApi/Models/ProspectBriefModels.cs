using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ProspectBriefRequest(
    [property: JsonRequired] Guid FieldId,
    [property: JsonRequired] string ReservoirName,
    Guid? ScenarioId,
    DateTimeOffset? AsOfUtc,
    [property: JsonRequired] AnalysisConfiguration Configuration,
    [property: JsonRequired] string PackageSha256,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string CandidateId);

public enum ProspectRecommendation { Drill, DrillWithCaution, NotYet }

public enum ProspectRiskSeverity { High, Medium, Low }

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ProspectBriefReason(
    [property: JsonRequired] string Text,
    [property: JsonRequired] IReadOnlyList<string> CitedEvidenceIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ProspectBriefRisk(
    [property: JsonRequired] string Text,
    [property: JsonRequired] ProspectRiskSeverity Severity,
    [property: JsonRequired] IReadOnlyList<string> CitedEvidenceIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ProspectBrief(
    [property: JsonRequired] string Headline,
    [property: JsonRequired] ProspectRecommendation Recommendation,
    [property: JsonRequired] IReadOnlyList<ProspectBriefReason> Reasons,
    [property: JsonRequired] IReadOnlyList<ProspectBriefRisk> Risks,
    [property: JsonRequired] IReadOnlyList<string> NextChecks);

public sealed record ProspectBriefResponse(
    string Version,
    string PromptVersion,
    Guid FieldId,
    string ReservoirName,
    Guid? ScenarioId,
    DateTimeOffset? AsOfUtc,
    string CandidateId,
    string PackageSha256,
    string AnalysisSha256,
    string ConfigurationSha256,
    DateTimeOffset GeneratedAt,
    FormationStructureEstimate StructureEstimate,
    ProspectBrief Brief);
