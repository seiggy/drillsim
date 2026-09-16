using System.Text.Json.Serialization;

namespace ReservoirSimulation.Contracts;

public enum CompletionOpeningType
{
    Perforated,
    OpenHole,
    Isolated
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ApprovedCompletionOpening
{
    public string OpeningId { get; init; } = string.Empty;
    public string ReservoirName { get; init; } = string.Empty;
    public CompletionOpeningType Type { get; init; }
    [JsonPropertyName("topMdM")]
    public double TopMeasuredDepthM { get; init; }
    [JsonPropertyName("baseMdM")]
    public double BaseMeasuredDepthM { get; init; }
    public double WellboreRadiusM { get; init; } = 0.1;
    public double Skin { get; init; }
    public double Efficiency { get; init; } = 1;
    public double UncertaintyM { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ApprovedCompletionBindingRequest
{
    public string PathBindingId { get; init; } = string.Empty;
    public string CompletionModelVersion { get; init; } = "observed-log-completion-v1";
    public IReadOnlyList<ApprovedCompletionOpening> Openings { get; init; } = [];
}

public sealed record ApprovedCompletionBindingMetadata(
    string CompletionBindingId,
    string WorldId,
    string PathBindingId,
    Guid ScenarioId,
    Guid RunId,
    string ModelVersion,
    int OpeningCount,
    int ProducingConnectionCount,
    string CanonicalRequestHash,
    DateTimeOffset CreatedUtc);
