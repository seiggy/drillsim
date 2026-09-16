using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RevealRequest(
    string RevealId,
    string RunId,
    string ClonedFieldId,
    DateTimeOffset ValidTimeUtc,
    string ObservationModelVersion,
    string ManifestSha256,
    IReadOnlyList<RevealEvidenceRequest> Evidence,
    ProductionSeriesRequest ProductionSeries,
    AnalysisPackage? ClonePackage = null);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RevealEvidenceRequest(
    string EvidenceId,
    string RecordKind,
    string ContentSha256);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ProductionSeriesRequest(
    string SeriesId,
    string ModelVersion,
    string ContentSha256,
    int MonthCount,
    IReadOnlyList<int> CheckpointYears);

public enum RevealReceiptStatus
{
    Prepared,
    Revealed
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FinalizeRevealRequest(
    string RevealId,
    string ManifestSha256);
public sealed record RevealReceipt(
    Guid ScenarioId,
    Guid RevealId,
    Guid ClonedFieldId,
    DateTimeOffset AsOfUtc,
    RevealReceiptStatus Status,
    string ManifestSha256,
    int EvidenceCount,
    Guid ProductionSeriesId);

public sealed record PublicProductionSeriesMetadata(
    Guid ScenarioId,
    Guid RevealId,
    Guid SeriesId,
    string ModelVersion,
    string ContentSha256,
    int MonthCount,
    IReadOnlyList<int> CheckpointYears,
    DateTimeOffset CreatedUtc);
