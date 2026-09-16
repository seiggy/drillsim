using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

internal static class ConfiguredPredictionBinding
{
    public const string Version = "prediction-analysis-binding-v1";
    public const string BaselineVersion = "baseline-analysis-binding-v1";
    public const string ModelVersion = "petrophysics-screening-v1";

    public static void ValidateContract(PredictionBody body)
    {
        if (body.AnalysisBinding is not PredictionAnalysisBinding binding)
        {
            if (body.CandidateId.StartsWith(PetrophysicsAnalysisService.ConfiguredCandidatePrefix, StringComparison.Ordinal))
                throw Mismatch("A configured candidate requires an explicit versioned analysisBinding; legacy defaults cannot be substituted.");
            return;
        }
        if (binding.Version != Version || binding.ModelVersion != ModelVersion)
            throw Mismatch("Unsupported or missing prediction analysis binding/model version. Rebuild the draft from a supported analysis.");
        AnalysisConfiguration.Validate(binding.Configuration);
        if (binding.ConfigurationSha256 != binding.Configuration.ComputeSha256() ||
            binding.ScenarioId is null || binding.ScenarioId == Guid.Empty || binding.FieldId is null || binding.FieldId == Guid.Empty ||
            string.IsNullOrWhiteSpace(binding.ReservoirName) || binding.ReservoirName.Length > 200 ||
            binding.AsOfUtc is null || binding.AsOfUtc == default(DateTimeOffset) || binding.AsOfUtc == DateTimeOffset.MaxValue ||
            binding.AsOfUtc.Value.Offset != TimeSpan.Zero || binding.Target is not PredictionAnalysisTarget target ||
            target.CandidateId != body.CandidateId || !double.IsFinite(target.EastingM) || !double.IsFinite(target.NorthingM) ||
            Math.Abs(target.EastingM) > 1e9 || Math.Abs(target.NorthingM) > 1e9 || target.ExpectedPaydirtM != body.ExpectedPaydirtM)
            throw Mismatch("The analysis binding must identify the exact scope, configuration, candidate and output quantiles.");
        // v1 is a point-screening result, not an integrated directional-well forecast.
        if (body.ProposedWellPath.Any(station =>
                station.EastingM != binding.Target.EastingM || station.NorthingM != binding.Target.NorthingM))
            throw Mismatch("Point-screening binding v1 requires every planned station at the selected candidate's exact easting/northing.");
        if (!body.Formations.Any(formation => formation.FormationName == binding.ReservoirName))
            throw Mismatch("The prediction must include the bound reservoir formation.");
    }

    public static AnalysisResult Validate(
        Scenario scenario, PredictionBody body, AnalysisPackage package, IPetrophysicsAnalysisService analyzer)
    {
        PredictionValidator.Validate(body);
        PredictionAnalysisBinding binding = body.AnalysisBinding
            ?? throw Mismatch("A configured analysis binding is required.");
        if (binding.ScenarioId != scenario.ScenarioId || binding.FieldId != scenario.SourceFieldId ||
            binding.ReservoirName != scenario.ReservoirName || binding.AsOfUtc != scenario.InitialAsOfUtc ||
            package.FieldId != scenario.SourceFieldId || package.GeneratedAt != scenario.InitialAsOfUtc ||
            package.Sha256 != body.FieldPackageSha256)
            throw Mismatch("The configured prediction must use this scenario's frozen initial source package, reservoir and clock.");
        AnalysisResult result = analyzer.Analyze(package, scenario.ReservoirName, binding.Configuration);
        ValidateResult(body, result);
        HashSet<string> visible = EvidenceCatalog.Enumerate(package).Select(item => item.EvidenceId).ToHashSet(StringComparer.Ordinal);
        if (body.CitedEvidenceIds.Any(id => !visible.Contains(id)))
            throw Mismatch("The configured prediction cites evidence outside its frozen visible source package.");
        return result;
    }

    public static RankedCandidate ValidateResult(PredictionBody body, AnalysisResult result)
    {
        ValidateContract(body);
        PredictionAnalysisBinding binding = body.AnalysisBinding!;
        RankedCandidate? candidate = result.CandidateGrid
            .FirstOrDefault(point => point.Status == "eligible" && point.CandidateId == body.CandidateId)?.Prediction;
        if (result.ModelVersion != ModelVersion || result.FieldId != binding.FieldId ||
            result.ReservoirName != binding.ReservoirName || result.PackageSha256 != body.FieldPackageSha256 ||
            result.Configuration != binding.Configuration || result.ConfigurationSha256 != binding.ConfigurationSha256 ||
            result.AnalysisSha256 != binding.AnalysisSha256 || candidate is null ||
            binding.Target != new PredictionAnalysisTarget(candidate.CandidateId, candidate.EastingM, candidate.NorthingM,
                new(candidate.P90NetPayM, candidate.P50NetPayM, candidate.P10NetPayM)))
            throw Mismatch("The candidate, geometry or analysis fingerprint differs from the recomputed configured result.");
        return candidate;
    }

    public static object Capabilities() => new
    {
        version = "prediction-handoff-capabilities-v1",
        configuredSaveSupported = true,
        configuredSealSupported = true,
        bindingVersion = Version,
        analysisModelVersion = ModelVersion,
        configurationVersion = AnalysisConfiguration.CurrentVersion,
        baselineBindingVersion = BaselineVersion,
        baselineModelVersions = new[]
        {
            "baseline-nearest-well-v3", "baseline-field-mean-v3",
            "baseline-four-neighbor-idw-v3", "baseline-uncertainty-aware-rank1-v3"
        },
        sourceScope = "Scenario source field and initialAsOfUtc only; immutable visible source snapshot required.",
        targetRule = "Any eligible full-grid candidate. Every planned station must use its exact eastingM/northingM; expectedPaydirtM must equal its P90/P50/P10.",
        minimumLocatedControls = 4,
        fourNeighborBaselineRule = "Exactly four nearest located controls with configured cutoffs, grid and exclusion; independent of the main idwNeighborCount.",
        limitation = "Qualifying-rock point-screening proxy, not hydrocarbon presence, reserves or a path-integrated forecast. Formation, fluid/contact and production forecasts remain explicit human inputs.",
        legacyNullBindingSupported = true
    };

    private static ScenarioApiException Mismatch(string detail) =>
        new(StatusCodes.Status409Conflict, "Prediction analysis binding mismatch", detail);
}
