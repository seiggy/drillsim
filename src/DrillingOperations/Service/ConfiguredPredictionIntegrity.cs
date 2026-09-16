using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace DrillingOperations;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
[JsonNumberHandling(JsonNumberHandling.Strict)]
public sealed record AnalysisConfigurationDto(
    [property: JsonRequired] string Version,
    [property: JsonRequired] double PorosityCutoff,
    [property: JsonRequired] double PermeabilityCutoffM2,
    [property: JsonRequired] double WellExclusionRadiusM,
    [property: JsonRequired] int GridPointsPerAxis,
    [property: JsonRequired] int IdwNeighborCount);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
[JsonNumberHandling(JsonNumberHandling.Strict)]
public sealed record AnalysisPredictionTargetDto(
    [property: JsonRequired] string CandidateId,
    [property: JsonRequired] double EastingM,
    [property: JsonRequired] double NorthingM,
    [property: JsonRequired] AnalysisQuantilesDto ExpectedPaydirtM);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AnalysisPredictionBindingDto(
    [property: JsonRequired] AnalysisConfigurationDto Configuration,
    [property: JsonRequired] string ConfigurationSha256,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string Version,
    [property: JsonRequired] string ModelVersion,
    [property: JsonRequired] Guid ScenarioId,
    [property: JsonRequired] Guid FieldId,
    [property: JsonRequired] string ReservoirName,
    [property: JsonRequired] DateTimeOffset AsOfUtc,
    [property: JsonRequired] AnalysisPredictionTargetDto Target);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AnalysisBaselineBindingDto(
    [property: JsonRequired] string Version,
    [property: JsonRequired] string ModelVersion,
    [property: JsonRequired] AnalysisConfigurationDto Configuration,
    [property: JsonRequired] string ConfigurationSha256,
    [property: JsonRequired] string AnalysisSha256,
    [property: JsonRequired] string PredictionAnalysisSha256);

public static class ConfiguredPredictionIntegrity
{
    public const string BindingVersion = "prediction-analysis-binding-v1";
    public const string BaselineBindingVersion = "baseline-analysis-binding-v1";
    public const string AnalysisModelVersion = "petrophysics-screening-v1";

    public static void ValidateApproved(AnalysisPredictionDto prediction, AnalysisScenarioDto scenario)
    {
        ValidateSemantics(prediction);
        if (prediction.Body?.AnalysisBinding is not AnalysisPredictionBindingDto binding) return;
        if (binding.ScenarioId != scenario.ScenarioId || binding.FieldId != scenario.SourceFieldId ||
            binding.ReservoirName != scenario.ReservoirName || binding.AsOfUtc != scenario.InitialAsOfUtc)
            throw Invalid("The configured prediction scope differs from the authoritative source scenario.");
        ScoreArtifactIntegrity.ValidatePrediction(scenario.ScenarioId, prediction,
            prediction.Seal?.Sha256 ?? string.Empty, prediction.Body.FieldPackageSha256);
    }

    public static void ValidateSemantics(AnalysisPredictionDto prediction)
    {
        AnalysisPredictionBodyDto? body = prediction.Body;
        if (body?.AnalysisBinding is not AnalysisPredictionBindingDto binding)
        {
            if (body?.CandidateId?.StartsWith("configured:", StringComparison.Ordinal) == true ||
                prediction.Baselines?.Any(item => item?.AnalysisBinding is not null) == true)
                throw Invalid("Configured prediction/baseline metadata cannot be removed or treated as legacy defaults.");
            return;
        }
        ValidateConfiguration(binding.Configuration, binding.ConfigurationSha256);
        AnalysisPredictionTargetDto? target = binding.Target;
        if (binding.Version != BindingVersion || binding.ModelVersion != AnalysisModelVersion ||
            binding.ScenarioId == Guid.Empty || binding.ScenarioId != prediction.ScenarioId || binding.FieldId == Guid.Empty ||
            string.IsNullOrWhiteSpace(binding.ReservoirName) || binding.ReservoirName.Length > 200 ||
            binding.AsOfUtc == default || binding.AsOfUtc == DateTimeOffset.MaxValue || binding.AsOfUtc.Offset != TimeSpan.Zero ||
            !AnalysisScorecardClient.LowerHash(binding.AnalysisSha256) ||
            !AnalysisScorecardClient.LowerHash(body.FieldPackageSha256) || target is null ||
            target.CandidateId != body.CandidateId || string.IsNullOrWhiteSpace(body.CandidateId) || body.CandidateId.Length > 200 ||
            !double.IsFinite(target.EastingM) || !double.IsFinite(target.NorthingM) ||
            Math.Abs(target.EastingM) > 1e9 || Math.Abs(target.NorthingM) > 1e9 ||
            target.ExpectedPaydirtM != body.ExpectedPaydirtM || !ValidQuantiles(target.ExpectedPaydirtM) ||
            body.ProposedWellPath is not { Count: >= 2 and <= 2000 } ||
            body.Formations is null || !body.Formations.Any(item => item is not null && item.FormationName == binding.ReservoirName))
            throw Invalid("Configured prediction version, scope, target or quantiles are invalid.");
        double priorMd = -1;
        foreach (AnalysisPathStationDto? station in body.ProposedWellPath)
        {
            if (station is null || !double.IsFinite(station.MeasuredDepthM) || !double.IsFinite(station.TrueVerticalDepthM) ||
                station.MeasuredDepthM < 0 || station.MeasuredDepthM <= priorMd || station.TrueVerticalDepthM < 0 ||
                station.EastingM != target.EastingM || station.NorthingM != target.NorthingM)
                throw Invalid("Configured point-screening geometry was changed or is invalid.");
            priorMd = station.MeasuredDepthM;
        }
        if (prediction.Baselines is not { Count: 4 } || prediction.Baselines.Any(item => item is null) ||
            prediction.Baselines.Select(item => item.Kind).Distinct().Count() != 4)
            throw Invalid("Four configured immutable baseline definitions are required.");
        foreach (AnalysisBaselineDto baseline in prediction.Baselines)
        {
            string expectedVersion = baseline.Kind switch
            {
                "NearestWell" => "baseline-nearest-well-v3",
                "FieldMean" => "baseline-field-mean-v3",
                "FourNeighborIdw" => "baseline-four-neighbor-idw-v3",
                "UncertaintyAwareRank1" => "baseline-uncertainty-aware-rank1-v3",
                _ => throw Invalid("Unknown configured baseline kind.")
            };
            AnalysisBaselineBindingDto metadata = baseline.AnalysisBinding ?? throw Invalid("Configured baseline metadata is required.");
            AnalysisConfigurationDto expectedConfig = baseline.Kind == "FourNeighborIdw"
                ? binding.Configuration with { IdwNeighborCount = 4 } : binding.Configuration;
            ValidateConfiguration(metadata.Configuration, metadata.ConfigurationSha256);
            if (baseline.ModelVersion != expectedVersion || baseline.ScenarioId != prediction.ScenarioId ||
                baseline.PackageSha256 != body.FieldPackageSha256 || metadata.Version != BaselineBindingVersion ||
                metadata.ModelVersion != binding.ModelVersion || metadata.Configuration != expectedConfig ||
                !double.IsFinite(baseline.TargetEastingM) || !double.IsFinite(baseline.TargetNorthingM) ||
                Math.Abs(baseline.TargetEastingM) > 1e9 || Math.Abs(baseline.TargetNorthingM) > 1e9 ||
                metadata.PredictionAnalysisSha256 != binding.AnalysisSha256 || !AnalysisScorecardClient.LowerHash(metadata.AnalysisSha256) ||
                baseline.ContributingEvidenceIds is null || baseline.ContributingEvidenceIds.Count == 0 ||
                baseline.ContributingEvidenceIds.Distinct(StringComparer.Ordinal).Count() != baseline.ContributingEvidenceIds.Count ||
                !ValidQuantiles(baseline.ExpectedPaydirtM) || baseline.FormationTopTrueVerticalDepthM is not null ||
                baseline.FormationBaseTrueVerticalDepthM is not null || baseline.FluidClasses is not null ||
                baseline.ContactPredictions is not null || baseline.ProductionForecasts is not null)
                throw Invalid("Configured baseline semantics, configuration or source/result binding changed.");
            if ((expectedConfig == binding.Configuration && metadata.AnalysisSha256 != binding.AnalysisSha256) ||
                (baseline.Kind != "UncertaintyAwareRank1" && (baseline.CandidateId != target.CandidateId ||
                    baseline.TargetEastingM != target.EastingM || baseline.TargetNorthingM != target.NorthingM)) ||
                (baseline.Kind == "NearestWell" && baseline.ContributingEvidenceIds.Count != 1) ||
                (baseline.Kind == "FieldMean" && baseline.ContributingEvidenceIds.Count < 4) ||
                (baseline.Kind == "FourNeighborIdw" && baseline.ContributingEvidenceIds.Count != 4) ||
                (baseline.Kind == "UncertaintyAwareRank1" && baseline.ContributingEvidenceIds.Count != binding.Configuration.IdwNeighborCount))
                throw Invalid("Configured baseline target or neighbor-count definition changed.");
        }
    }

    public static string CanonicalBaselineContent(AnalysisBaselineDto baseline) => CanonicalJson.Serialize(new BaselineContent(
        baseline.ScenarioId, baseline.Kind, baseline.ModelVersion, baseline.PackageSha256, baseline.CandidateId,
        baseline.TargetEastingM, baseline.TargetNorthingM, baseline.ContributingEvidenceIds,
        baseline.FormationTopTrueVerticalDepthM, baseline.FormationBaseTrueVerticalDepthM, baseline.ExpectedPaydirtM,
        baseline.FluidClasses, baseline.ContactPredictions, baseline.ProductionForecasts, baseline.Limitation, baseline.AnalysisBinding));

    public static void ValidateBaselineIdentity(AnalysisBaselineDto baseline)
    {
        JsonObject identity = JsonNode.Parse(CanonicalBaselineContent(baseline))!.AsObject();
        identity["contentSha256"] = baseline.ContentSha256;
        byte[] bytes = Convert.FromHexString(DeterministicIdentity.Sha256(CanonicalJson.Serialize(identity)));
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x80);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        if (baseline.BaselineId != new Guid(bytes.AsSpan(0, 16), bigEndian: true))
            throw Invalid("Configured baseline identity does not match its canonical content.");
    }

    private static void ValidateConfiguration(AnalysisConfigurationDto? configuration, string? hash)
    {
        if (configuration is null || configuration.Version != "analysis-configuration-v1" ||
            !double.IsFinite(configuration.PorosityCutoff) || configuration.PorosityCutoff is < 0 or > 1 ||
            !double.IsFinite(configuration.PermeabilityCutoffM2) || configuration.PermeabilityCutoffM2 is <= 0 or > 1e-8 ||
            !double.IsFinite(configuration.WellExclusionRadiusM) || configuration.WellExclusionRadiusM is < 0 or > 100_000 ||
            configuration.GridPointsPerAxis is < 2 or > 51 || configuration.IdwNeighborCount is < 1 or > 32 ||
            !AnalysisScorecardClient.LowerHash(hash) || DeterministicIdentity.Sha256(CanonicalJson.Serialize(configuration)) != hash)
            throw Invalid("Unsupported, invalid or changed analysis configuration.");
    }

    private static bool ValidQuantiles(AnalysisQuantilesDto? q) =>
        q is not null && double.IsFinite(q.P90) && double.IsFinite(q.P50) && double.IsFinite(q.P10) &&
        q.P90 >= 0 && q.P90 <= q.P50 && q.P50 <= q.P10;

    private static ScoringException Invalid(string message) => new(409, "ConfiguredPredictionIntegrityMismatch", message);

    private sealed record BaselineContent(
        Guid ScenarioId, string Kind, string ModelVersion, string PackageSha256, string CandidateId,
        double TargetEastingM, double TargetNorthingM, IReadOnlyList<string> ContributingEvidenceIds,
        AnalysisQuantilesDto? FormationTopTrueVerticalDepthM, AnalysisQuantilesDto? FormationBaseTrueVerticalDepthM,
        AnalysisQuantilesDto? ExpectedPaydirtM, IReadOnlyList<string>? FluidClasses,
        IReadOnlyList<AnalysisContactPredictionDto>? ContactPredictions,
        IReadOnlyList<AnalysisProductionForecastDto>? ProductionForecasts, string Limitation,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] AnalysisBaselineBindingDto? AnalysisBinding);
}
