using System.Text.Json.Serialization;
using DrillSim.AnalysisApi.Infrastructure;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
[JsonNumberHandling(JsonNumberHandling.Strict)]
public sealed record AnalysisConfiguration(
    [property: JsonRequired] string Version,
    [property: JsonRequired] double PorosityCutoff,
    [property: JsonRequired] double PermeabilityCutoffM2,
    [property: JsonRequired] double WellExclusionRadiusM,
    [property: JsonRequired] int GridPointsPerAxis,
    [property: JsonRequired] int IdwNeighborCount)
{
    public const string CurrentVersion = "analysis-configuration-v1";
    public static AnalysisConfiguration Default { get; } = new(CurrentVersion, .12, 9.869233e-16, 500, 15, 4);

    public static void Validate(AnalysisConfiguration? configuration)
    {
        if (configuration is null)
            throw Invalid("configuration is required.");
        if (configuration.Version != CurrentVersion)
            throw Invalid($"version must be '{CurrentVersion}'.");
        if (!double.IsFinite(configuration.PorosityCutoff) || configuration.PorosityCutoff is < 0 or > 1)
            throw Invalid("porosityCutoff must be finite and between 0 and 1.");
        if (!double.IsFinite(configuration.PermeabilityCutoffM2) || configuration.PermeabilityCutoffM2 is <= 0 or > 1e-8)
            throw Invalid("permeabilityCutoffM2 must be finite, greater than 0 and at most 1e-8 m2.");
        if (!double.IsFinite(configuration.WellExclusionRadiusM) || configuration.WellExclusionRadiusM is < 0 or > 100_000)
            throw Invalid("wellExclusionRadiusM must be finite and between 0 and 100000 m.");
        if (configuration.GridPointsPerAxis is < 2 or > 51)
            throw Invalid("gridPointsPerAxis must be between 2 and 51 (at most 2601 grid points).");
        if (configuration.IdwNeighborCount is < 1 or > 32)
            throw Invalid("idwNeighborCount must be between 1 and 32.");
    }

    public string ComputeSha256() => PredictionJson.ComputeSha256(this);

    private static ScenarioApiException Invalid(string detail) =>
        new(StatusCodes.Status400BadRequest, "Invalid analysis configuration", detail);
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ConfiguredAnalysisRequest(
    [property: JsonRequired] AnalysisConfiguration Configuration,
    Guid? ScenarioId = null,
    DateTimeOffset? AsOf = null,
    string? Reservoir = null);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PredictionAnalysisBinding(
    [property: JsonRequired] AnalysisConfiguration Configuration,
    [property: JsonRequired] string ConfigurationSha256,
    [property: JsonRequired] string AnalysisSha256)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ModelVersion { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ScenarioId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? FieldId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReservoirName { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? AsOfUtc { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PredictionAnalysisTarget? Target { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
[JsonNumberHandling(JsonNumberHandling.Strict)]
public sealed record PredictionAnalysisTarget(
    [property: JsonRequired] string CandidateId,
    [property: JsonRequired] double EastingM,
    [property: JsonRequired] double NorthingM,
    [property: JsonRequired] QuantileValues ExpectedPaydirtM);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record BaselineAnalysisBinding(
    [property: JsonRequired] string Version,
    [property: JsonRequired] string ModelVersion,
    [property: JsonRequired] AnalysisConfiguration Configuration,
    [property: JsonRequired] string ConfigurationSha256,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string PredictionAnalysisSha256);
