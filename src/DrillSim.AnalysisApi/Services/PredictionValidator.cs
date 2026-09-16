using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

internal static class PredictionValidator
{
    private const int MaximumCandidateIdLength = 200;
    private const int MaximumPathStations = 2_000;
    private const int MaximumFormations = 64;
    private const int MaximumContacts = 16;
    private const int MaximumAssumptions = 64;
    private const int MaximumEvidenceIds = 512;
    private const int MaximumTextItemLength = 1_000;
    private const int MaximumRationaleLength = 10_000;
    private const int MaximumEvidenceIdLength = 200;

    public static void Validate(PredictionBody body)
    {
        ArgumentNullException.ThrowIfNull(body);
        RequireText(body.CandidateId, "candidateId", MaximumCandidateIdLength);
        ValidatePath(body.ProposedWellPath);
        ValidateFormations(body.Formations);
        ValidateQuantiles(body.ExpectedPaydirtM, "expectedPaydirtM", requireNonnegative: true);
        ValidateFluids(body.FluidClasses);
        ValidateContacts(body.ContactPredictions);
        ValidateForecasts(body.ProductionForecasts);
        ValidateTextList(body.UncertaintyAssumptions, "uncertaintyAssumptions", 1, MaximumAssumptions);
        ValidateEvidenceIds(body.CitedEvidenceIds);
        ValidateSha256(body.FieldPackageSha256, "fieldPackageSha256");
        RequireText(body.Rationale, "rationale", MaximumRationaleLength);
        if (body.AnalysisBinding is PredictionAnalysisBinding binding)
        {
            AnalysisConfiguration.Validate(binding.Configuration);
            ValidateSha256(binding.ConfigurationSha256, "analysisBinding.configurationSha256");
            ValidateSha256(binding.AnalysisSha256, "analysisBinding.analysisSha256");
            if (binding.ConfigurationSha256 != binding.Configuration.ComputeSha256())
                throw Invalid("analysisBinding.configurationSha256", "does not match the supplied configuration.");
        }
        ConfiguredPredictionBinding.ValidateContract(body);
    }

    public static void EnsureSealable(PredictionBody body)
    {
        ConfiguredPredictionBinding.ValidateContract(body);
    }

    public static string ValidateActor(string? actor)
    {
        string normalized = actor?.Trim() ?? string.Empty;
        RequireText(normalized, "X-DrillSim-Human-Actor", 200);
        return normalized;
    }

    private static void ValidatePath(IReadOnlyList<ProposedWellPathStation>? path)
    {
        if (path is null || path.Count < 2 || path.Count > MaximumPathStations)
            throw Invalid("proposedWellPath", $"must contain between 2 and {MaximumPathStations} stations.");

        double previousMeasuredDepth = double.NegativeInfinity;
        for (int index = 0; index < path.Count; index++)
        {
            ProposedWellPathStation? station = path[index];
            if (station is null)
                throw Invalid($"proposedWellPath[{index}]", "is required.");
            RequireFinite(station.MeasuredDepthM, $"proposedWellPath[{index}].measuredDepthM");
            RequireFinite(station.TrueVerticalDepthM, $"proposedWellPath[{index}].trueVerticalDepthM");
            RequireFinite(station.EastingM, $"proposedWellPath[{index}].eastingM");
            RequireFinite(station.NorthingM, $"proposedWellPath[{index}].northingM");
            if (station.MeasuredDepthM < 0)
                throw Invalid($"proposedWellPath[{index}].measuredDepthM", "must be nonnegative.");
            if (station.TrueVerticalDepthM < 0)
                throw Invalid($"proposedWellPath[{index}].trueVerticalDepthM", "must be nonnegative.");
            if (station.MeasuredDepthM <= previousMeasuredDepth)
                throw Invalid("proposedWellPath", "measured depth must be strictly increasing.");
            previousMeasuredDepth = station.MeasuredDepthM;
        }
    }

    private static void ValidateFormations(IReadOnlyList<FormationPrediction>? formations)
    {
        if (formations is null || formations.Count == 0 || formations.Count > MaximumFormations)
            throw Invalid("formations", $"must contain between 1 and {MaximumFormations} predictions.");

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < formations.Count; index++)
        {
            FormationPrediction? formation = formations[index];
            if (formation is null)
                throw Invalid($"formations[{index}]", "is required.");
            string name = RequireText(formation.FormationName, $"formations[{index}].formationName", 200);
            if (!names.Add(name))
                throw Invalid("formations", $"contains duplicate formation '{name}'.");
            ValidateQuantiles(formation.TopTrueVerticalDepthM, $"formations[{index}].topTrueVerticalDepthM", true);
            ValidateQuantiles(formation.BaseTrueVerticalDepthM, $"formations[{index}].baseTrueVerticalDepthM", true);
            if (formation.TopTrueVerticalDepthM.P90 > formation.BaseTrueVerticalDepthM.P90 ||
                formation.TopTrueVerticalDepthM.P50 > formation.BaseTrueVerticalDepthM.P50 ||
                formation.TopTrueVerticalDepthM.P10 > formation.BaseTrueVerticalDepthM.P10)
            {
                throw Invalid($"formations[{index}]", "top depth cannot exceed base depth at any quantile.");
            }
        }
    }

    private static void ValidateFluids(IReadOnlyList<PredictedFluidClass>? fluids)
    {
        if (fluids is null || fluids.Count == 0 || fluids.Count > 3)
            throw Invalid("fluidClasses", "must contain between 1 and 3 allowed fluid classes.");
        if (fluids.Any(fluid => !Enum.IsDefined(fluid)))
            throw Invalid("fluidClasses", "contains an unsupported fluid class.");
        if (fluids.Distinct().Count() != fluids.Count)
            throw Invalid("fluidClasses", "must contain unique values.");
    }

    private static void ValidateContacts(IReadOnlyList<FluidContactPrediction>? contacts)
    {
        if (contacts is null || contacts.Count > MaximumContacts)
            throw Invalid("contactPredictions", $"must contain no more than {MaximumContacts} contacts.");

        var types = new HashSet<FluidContactType>();
        for (int index = 0; index < contacts.Count; index++)
        {
            FluidContactPrediction? contact = contacts[index];
            if (contact is null)
                throw Invalid($"contactPredictions[{index}]", "is required.");
            if (!Enum.IsDefined(contact.ContactType))
                throw Invalid($"contactPredictions[{index}].contactType", "is unsupported.");
            if (!types.Add(contact.ContactType))
                throw Invalid("contactPredictions", $"contains duplicate contact type {contact.ContactType}.");
            ValidateQuantiles(contact.TrueVerticalDepthM, $"contactPredictions[{index}].trueVerticalDepthM", true);
        }
    }

    private static void ValidateForecasts(IReadOnlyList<ProductionForecast>? forecasts)
    {
        if (forecasts is null || forecasts.Count != 3 ||
            !forecasts.Select(forecast => forecast?.Year).Order().SequenceEqual(new int?[] { 1, 3, 5 }))
        {
            throw Invalid("productionForecasts", "must contain exactly one checkpoint for years 1, 3, and 5.");
        }

        for (int index = 0; index < forecasts.Count; index++)
        {
            ProductionForecast forecast = forecasts[index];
            RequireFiniteNonnegative(forecast.OilM3, $"productionForecasts[{index}].oilM3");
            RequireFiniteNonnegative(forecast.GasM3, $"productionForecasts[{index}].gasM3");
            RequireFiniteNonnegative(forecast.WaterM3, $"productionForecasts[{index}].waterM3");
        }
    }

    private static void ValidateTextList(
        IReadOnlyList<string>? values,
        string field,
        int minimumCount,
        int maximumCount)
    {
        if (values is null || values.Count < minimumCount || values.Count > maximumCount)
            throw Invalid(field, $"must contain between {minimumCount} and {maximumCount} items.");
        for (int index = 0; index < values.Count; index++)
            RequireText(values[index], $"{field}[{index}]", MaximumTextItemLength);
    }

    private static void ValidateEvidenceIds(IReadOnlyList<string>? evidenceIds)
    {
        if (evidenceIds is null || evidenceIds.Count == 0 || evidenceIds.Count > MaximumEvidenceIds)
            throw Invalid("citedEvidenceIds", $"must contain between 1 and {MaximumEvidenceIds} IDs.");

        var unique = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < evidenceIds.Count; index++)
        {
            string value = RequireText(evidenceIds[index], $"citedEvidenceIds[{index}]", MaximumEvidenceIdLength);
            int separator = value.IndexOf(':');
            string kind = separator > 0 ? value[..separator] : string.Empty;
            string idText = separator > 0 ? value[(separator + 1)..] : string.Empty;
            if (separator <= 0 || separator != value.LastIndexOf(':') ||
                kind.Any(character => character is not (>= 'a' and <= 'z') and not (>= '0' and <= '9') and not '-') ||
                !Guid.TryParseExact(idText, "D", out Guid id) ||
                !string.Equals(value, $"{kind}:{id:D}", StringComparison.Ordinal))
            {
                throw Invalid($"citedEvidenceIds[{index}]", "must use canonical lowercase kind:uuid form.");
            }
            if (!unique.Add(value))
                throw Invalid("citedEvidenceIds", $"contains duplicate ID '{value}'.");
        }
    }

    private static void ValidateQuantiles(QuantileValues? values, string field, bool requireNonnegative)
    {
        if (values is null)
            throw Invalid(field, "is required.");
        RequireFinite(values.P90, $"{field}.p90");
        RequireFinite(values.P50, $"{field}.p50");
        RequireFinite(values.P10, $"{field}.p10");
        if (values.P90 > values.P50 || values.P50 > values.P10)
            throw Invalid(field, "must be ordered P90 <= P50 <= P10.");
        if (requireNonnegative && values.P90 < 0)
            throw Invalid(field, "must be nonnegative.");
    }

    private static void ValidateSha256(string? value, string field)
    {
        if (value is null || value.Length != 64 || value.Any(character =>
            character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            throw Invalid(field, "must be exactly 64 lowercase hexadecimal characters.");
        }
    }

    private static string RequireText(string? value, string field, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw Invalid(field, "is required.");
        if (value.Length > maximumLength)
            throw Invalid(field, $"cannot exceed {maximumLength} characters.");
        return value;
    }

    private static void RequireFinite(double value, string field)
    {
        if (!double.IsFinite(value))
            throw Invalid(field, "must be finite.");
    }

    private static void RequireFiniteNonnegative(double value, string field)
    {
        RequireFinite(value, field);
        if (value < 0)
            throw Invalid(field, "must be nonnegative.");
    }

    private static ScenarioApiException Invalid(string field, string message) =>
        new(StatusCodes.Status400BadRequest, "Invalid prediction", $"{field}: {message}");
}
