using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

internal static class BaselineFactory
{
    private const int RequiredIdwNeighborCount = 4;
    private const string UnsupportedMetrics =
        "Formation top/base, fluid classes/contacts, and production forecasts are unsupported by deterministic petrophysics baselines and are null.";

    public static IReadOnlyList<BaselineSnapshot> Create(
        Guid scenarioId,
        PredictionBody prediction,
        AnalysisResult analysis,
        AnalysisResult? fourNeighborAnalysis = null)
    {
        PredictionValidator.EnsureSealable(prediction);
        bool configured = prediction.AnalysisBinding is not null;
        if (!configured && analysis.Configuration != AnalysisConfiguration.Default)
            throw Unavailable("Configured analyses cannot use legacy default baselines.");
        if (analysis.WellSummaries.Count == 0)
            throw Unavailable("No located well summaries are available for nearest-well and field-mean baselines.");
        if (analysis.Ranking.Count == 0)
            throw Unavailable("No current scenario analysis ranking is available for prediction baselines.");

        RankedCandidate? matchingCandidate = configured
            ? ConfiguredPredictionBinding.ValidateResult(prediction, analysis)
            : analysis.Ranking.FirstOrDefault(candidate => string.Equals(candidate.CandidateId, prediction.CandidateId, StringComparison.Ordinal));
        if (matchingCandidate is null)
            throw Unavailable("Prediction CandidateId does not match a current scenario analysis ranked candidate.");
        AnalysisResult idwAnalysis = configured
            ? fourNeighborAnalysis ?? throw Unavailable("A separately computed four-neighbor configured analysis is required.")
            : analysis;
        if (configured && (idwAnalysis.Configuration != (analysis.Configuration with { IdwNeighborCount = RequiredIdwNeighborCount }) ||
            idwAnalysis.FieldId != analysis.FieldId || idwAnalysis.ReservoirName != analysis.ReservoirName ||
            idwAnalysis.PackageSha256 != analysis.PackageSha256 || idwAnalysis.ModelVersion != analysis.ModelVersion ||
            idwAnalysis.ConfigurationSha256 != idwAnalysis.Configuration.ComputeSha256() ||
            string.IsNullOrEmpty(idwAnalysis.AnalysisSha256)))
            throw Unavailable("Four-neighbor baseline inputs do not match the configured evidence and assumptions.");
        RankedCandidate? idwCandidate = configured
            ? idwAnalysis.CandidateGrid.FirstOrDefault(point => point.Status == "eligible" &&
                point.EastingM == matchingCandidate.EastingM && point.NorthingM == matchingCandidate.NorthingM)?.Prediction
            : matchingCandidate;
        if (idwAnalysis.Methodology.IdwNeighborCount != RequiredIdwNeighborCount ||
            idwCandidate is null || idwCandidate.NeighborEvidenceIds.Count != RequiredIdwNeighborCount ||
            idwCandidate.NeighborEvidenceIds.Distinct(StringComparer.Ordinal).Count() != RequiredIdwNeighborCount)
        {
            throw Unavailable("The matching candidate does not contain the required unique four-neighbor IDW evidence.");
        }

        RankedCandidate rankOne = analysis.Ranking
            .OrderBy(candidate => candidate.Rank)
            .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
            .First();
        if (rankOne.NeighborEvidenceIds.Count == 0 ||
            rankOne.NeighborEvidenceIds.Distinct(StringComparer.Ordinal).Count() != rankOne.NeighborEvidenceIds.Count)
        {
            throw Unavailable("The uncertainty-aware rank-1 candidate has invalid contributing evidence.");
        }

        ProposedWellPathStation collar = prediction.ProposedWellPath[0];
        WellPaySummary nearestWell = analysis.WellSummaries
            .OrderBy(well => Distance(collar.EastingM, collar.NorthingM, well.EastingM, well.NorthingM))
            .ThenBy(well => well.WellEvidenceId, StringComparer.Ordinal)
            .First();
        var nearestPay = new QuantileValues(
            nearestWell.NetPayThicknessM,
            nearestWell.NetPayThicknessM,
            nearestWell.NetPayThicknessM);

        double mean = analysis.WellSummaries.Average(well => well.NetPayThicknessM);
        double sigma = Math.Sqrt(analysis.WellSummaries
            .Average(well => Math.Pow(well.NetPayThicknessM - mean, 2)));
        var fieldQuantiles = PetrophysicsAnalysisService.BuildQuantiles(mean, sigma);
        var fieldMeanPay = new QuantileValues(fieldQuantiles.P90, fieldQuantiles.P50, fieldQuantiles.P10);
        string[] fieldEvidence = analysis.WellSummaries
            .Select(well => well.WellEvidenceId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return
        [
            Build(
                scenarioId,
                BaselineKind.NearestWell,
                configured ? "baseline-nearest-well-v3" : "baseline-nearest-well-v2",
                analysis.PackageSha256,
                prediction.CandidateId,
                collar.EastingM,
                collar.NorthingM,
                [nearestWell.WellEvidenceId],
                nearestPay,
                configured ? Binding(analysis, analysis) : null),
            Build(
                scenarioId,
                BaselineKind.FieldMean,
                configured ? "baseline-field-mean-v3" : "baseline-field-mean-v2",
                analysis.PackageSha256,
                prediction.CandidateId,
                collar.EastingM,
                collar.NorthingM,
                fieldEvidence,
                fieldMeanPay,
                configured ? Binding(analysis, analysis) : null),
            Build(
                scenarioId,
                BaselineKind.FourNeighborIdw,
                configured ? "baseline-four-neighbor-idw-v3" : "baseline-four-neighbor-idw-v2",
                analysis.PackageSha256,
                prediction.CandidateId,
                matchingCandidate.EastingM,
                matchingCandidate.NorthingM,
                idwCandidate.NeighborEvidenceIds,
                ToQuantiles(idwCandidate),
                configured ? Binding(idwAnalysis, analysis) : null),
            Build(
                scenarioId,
                BaselineKind.UncertaintyAwareRank1,
                configured ? "baseline-uncertainty-aware-rank1-v3" : "baseline-uncertainty-aware-rank1-v2",
                analysis.PackageSha256,
                rankOne.CandidateId,
                rankOne.EastingM,
                rankOne.NorthingM,
                rankOne.NeighborEvidenceIds,
                ToQuantiles(rankOne),
                configured ? Binding(analysis, analysis) : null)
        ];
    }

    private static BaselineSnapshot Build(
        Guid scenarioId,
        BaselineKind kind,
        string modelVersion,
        string packageSha256,
        string candidateId,
        double targetEastingM,
        double targetNorthingM,
        IReadOnlyList<string> contributingEvidenceIds,
        QuantileValues expectedPaydirt,
        BaselineAnalysisBinding? analysisBinding = null)
    {
        var baseline = new BaselineSnapshot(
            Guid.Empty,
            string.Empty,
            scenarioId,
            kind,
            modelVersion,
            packageSha256,
            candidateId,
            targetEastingM,
            targetNorthingM,
            contributingEvidenceIds.ToArray(),
            null,
            null,
            expectedPaydirt,
            null,
            null,
            null,
            UnsupportedMetrics,
            analysisBinding);
        return BaselineIntegrity.Finalize(baseline);
    }

    private static QuantileValues ToQuantiles(RankedCandidate candidate) =>
        new(candidate.P90NetPayM, candidate.P50NetPayM, candidate.P10NetPayM);

    private static BaselineAnalysisBinding Binding(AnalysisResult source, AnalysisResult prediction) =>
        new(ConfiguredPredictionBinding.BaselineVersion, source.ModelVersion, source.Configuration,
            source.ConfigurationSha256, source.AnalysisSha256, prediction.AnalysisSha256);

    private static double Distance(double x1, double y1, double x2, double y2) =>
        Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

    private static ScenarioApiException Unavailable(string detail) =>
        new(StatusCodes.Status409Conflict, "Prediction baseline unavailable", detail);
}
