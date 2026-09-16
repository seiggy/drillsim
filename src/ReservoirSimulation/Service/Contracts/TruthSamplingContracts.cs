using System.Text.Json.Serialization;

namespace ReservoirSimulation.Contracts;

public enum ApprovedPathKind
{
    Planned,
    AsDrilled
}

public sealed record ApprovedPathStation
{
    public double MeasuredDepthM { get; init; }
    public double EastingM { get; init; }
    public double NorthingM { get; init; }
    public double TrueVerticalDepthM { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ApprovedPathBindingRequest
{
    public Guid ScenarioId { get; init; }
    public Guid RunId { get; init; }
    public ApprovedPathKind PathKind { get; init; }
    public string ApprovedSealedPredictionSha256 { get; init; } = string.Empty;
    public IReadOnlyList<ApprovedPathStation> Stations { get; init; } = [];
}

public sealed record ApprovedPathBindingMetadata(
    string BindingId,
    string WorldId,
    Guid ScenarioId,
    Guid RunId,
    ApprovedPathKind PathKind,
    string ApprovedSealedPredictionSha256,
    string CanonicalHash,
    int StationCount,
    DateTimeOffset CreatedUtc);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record TruthSamplingRequest
{
    public string BindingId { get; init; } = string.Empty;
    public double MaximumSpacingM { get; init; } = 10;
    public int MaximumSamples { get; init; } = 10_000;
    public string PropertySetVersion { get; init; } = "stage-b-truth-v1";
    public string CallerLabel { get; init; } = string.Empty;
}

public sealed record RestrictedTruthSample(
    double MeasuredDepthM,
    double EastingM,
    double NorthingM,
    double TrueVerticalDepthM,
    double ReservoirTopDepthM,
    double ReservoirBaseDepthM,
    double Porosity,
    double PermeabilityM2,
    double NetToGross,
    bool IsReservoirQuality,
    double PressurePa,
    double OilSaturation,
    double WaterSaturation,
    double GasSaturation);

public sealed record RestrictedTruthSamplingResult(
    string BindingId,
    string WorldId,
    Guid ScenarioId,
    Guid RunId,
    string PropertySetVersion,
    int SampleCount,
    IReadOnlyList<RestrictedTruthSample> Samples);
