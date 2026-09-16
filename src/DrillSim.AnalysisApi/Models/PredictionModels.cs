using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PredictionBody(
    string CandidateId,
    IReadOnlyList<ProposedWellPathStation> ProposedWellPath,
    IReadOnlyList<FormationPrediction> Formations,
    QuantileValues ExpectedPaydirtM,
    IReadOnlyList<PredictedFluidClass> FluidClasses,
    IReadOnlyList<FluidContactPrediction> ContactPredictions,
    IReadOnlyList<ProductionForecast> ProductionForecasts,
    IReadOnlyList<string> UncertaintyAssumptions,
    IReadOnlyList<string> CitedEvidenceIds,
    string FieldPackageSha256,
    string Rationale,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    PredictionAnalysisBinding? AnalysisBinding = null);

public sealed record ProposedWellPathStation(
    double MeasuredDepthM,
    double TrueVerticalDepthM,
    double EastingM,
    double NorthingM);

public sealed record FormationPrediction(
    string FormationName,
    QuantileValues TopTrueVerticalDepthM,
    QuantileValues BaseTrueVerticalDepthM);

public sealed record QuantileValues(
    double P90,
    double P50,
    double P10);

public enum PredictedFluidClass
{
    Oil,
    Gas,
    Water
}

public enum FluidContactType
{
    GOC,
    GWC,
    OWC
}

public sealed record FluidContactPrediction(
    FluidContactType ContactType,
    QuantileValues TrueVerticalDepthM);

public sealed record ProductionForecast(
    int Year,
    double OilM3,
    double GasM3,
    double WaterM3);

public sealed record PredictionSeal(
    string Sha256,
    string BaselinesSha256,
    DateTimeOffset SealedUtc);

public sealed record PredictionApproval(
    string Actor,
    DateTimeOffset ApprovedUtc,
    string SealedSha256);

public enum BaselineKind
{
    NearestWell,
    FieldMean,
    FourNeighborIdw,
    UncertaintyAwareRank1
}

public sealed record BaselineSnapshot(
    Guid BaselineId,
    string ContentSha256,
    Guid ScenarioId,
    BaselineKind Kind,
    string ModelVersion,
    string PackageSha256,
    string CandidateId,
    double TargetEastingM,
    double TargetNorthingM,
    IReadOnlyList<string> ContributingEvidenceIds,
    QuantileValues? FormationTopTrueVerticalDepthM,
    QuantileValues? FormationBaseTrueVerticalDepthM,
    QuantileValues? ExpectedPaydirtM,
    IReadOnlyList<PredictedFluidClass>? FluidClasses,
    IReadOnlyList<FluidContactPrediction>? ContactPredictions,
    IReadOnlyList<ProductionForecast>? ProductionForecasts,
    string Limitation,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BaselineAnalysisBinding? AnalysisBinding = null);

public sealed record PredictionRecord(
    Guid ScenarioId,
    PredictionBody Body,
    int Revision,
    DateTimeOffset CreatedUtc,
    DateTimeOffset ModifiedUtc,
    PredictionSeal? Seal,
    PredictionApproval? Approval,
    IReadOnlyList<BaselineSnapshot> Baselines);
