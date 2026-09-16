using System.Text.Json.Nodes;

namespace DrillSim.AnalysisApi.Models;

public sealed record AnalysisPackage(
    DateTimeOffset GeneratedAt,
    Guid FieldId,
    JsonNode Field,
    IReadOnlyList<JsonNode> Clusters,
    IReadOnlyList<JsonNode> Wells,
    IReadOnlyList<JsonNode> WellBores,
    IReadOnlyList<JsonNode> WellBoreArchitectures,
    IReadOnlyList<JsonNode> Trajectories,
    IReadOnlyList<JsonNode> GeologicalProperties,
    SourceCounts SourceCounts,
    IReadOnlyList<string> DataGaps,
    string Sha256);

public sealed record SourceCounts(
    int Fields,
    int Clusters,
    int Wells,
    int WellBores,
    int WellBoreArchitectures,
    int Trajectories,
    int GeologicalProperties);

public sealed record PetrophysicsSample(
    double MeasuredDepth,
    double Porosity,
    double PermeabilityM2,
    double PressureDifferential);

public sealed record NetPayResult(
    string EvidenceId,
    double NetPayThicknessM,
    double MeanPayPorosity,
    double MeanPayPermeabilityM2,
    int QualifyingSampleCount);

public sealed record WellPaySummary(
    Guid WellId,
    string WellEvidenceId,
    IReadOnlyList<string> GeologyEvidenceIds,
    IReadOnlyList<NetPayResult> GeologicalResults,
    double NetPayThicknessM,
    double MeanPayPorosity,
    double MeanPayPermeabilityM2,
    double EastingM,
    double NorthingM);

public sealed record RankedCandidate(
    int Rank,
    string CandidateId,
    double EastingM,
    double NorthingM,
    double LongitudeDegrees,
    double LatitudeDegrees,
    double NearestWellDistanceM,
    double P90NetPayM,
    double P50NetPayM,
    double P10NetPayM,
    double SigmaNetPayM,
    double MeanPayPorosity,
    double MeanPayPermeabilityMd,
    double RelativeUncertainty,
    double Score,
    IReadOnlyList<string> NeighborEvidenceIds)
{
    public CandidateScoreComponents? ScoreComponents { get; init; }
    public CandidateUncertaintyComponents? UncertaintyComponents { get; init; }
}

public sealed record CandidateScoreComponents(
    double P50NetPayM,
    double PorosityFactor,
    double PermeabilityMd,
    double PermeabilityLogFactor,
    double UnpenalizedScore,
    double UncertaintyDivisor);

public sealed record CandidateUncertaintyComponents(
    double DisagreementVarianceM2,
    double WeightedDistanceM,
    double GridDiagonalM,
    double DistanceSigmaM,
    double SigmaNetPayM,
    double QuantileZScore,
    bool Calibrated);

public sealed record CandidateGridBounds(
    double MinEastingM,
    double MaxEastingM,
    double MinNorthingM,
    double MaxNorthingM);

public sealed record CandidateGridPoint(
    string CandidateId,
    double EastingM,
    double NorthingM,
    double LongitudeDegrees,
    double LatitudeDegrees,
    string Status,
    IReadOnlyList<string> Reasons,
    double? NearestWellDistanceM,
    RankedCandidate? Prediction);

public sealed record AnalysisResult(
    DateTimeOffset GeneratedAt,
    Guid FieldId,
    string? ReservoirName,
    string PackageSha256,
    AnalysisMethodology Methodology,
    IReadOnlyList<WellPaySummary> WellSummaries,
    IReadOnlyList<RankedCandidate> Ranking,
    IReadOnlyList<string> DataGaps)
{
    public string ModelVersion { get; init; } = "petrophysics-screening-v1";
    public AnalysisConfiguration Configuration { get; init; } = AnalysisConfiguration.Default;
    public string ConfigurationSha256 { get; init; } = string.Empty;
    public string AnalysisSha256 { get; init; } = string.Empty;
    public CandidateGridBounds? CandidateGridBounds { get; init; }
    public IReadOnlyList<CandidateGridPoint> CandidateGrid { get; init; } = [];
}

public sealed record AnalysisMethodology(
    string Label,
    double PorosityCutoff,
    double PermeabilityCutoffM2,
    string PressureDifferentialRule,
    string NetPayThicknessMethod,
    string CandidateGridMethod,
    int IdwNeighborCount,
    double WellExclusionRadiusM,
    double QuantileZScore,
    string UncertaintyMethod,
    string ScoreFormula);
