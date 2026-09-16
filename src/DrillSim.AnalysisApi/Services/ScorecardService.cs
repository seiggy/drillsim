using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Services;

internal sealed record ValidatedScorecard(
    Guid ScenarioId,
    Guid ScorecardId,
    Guid RunId,
    Guid RevealId,
    string ScoringModelVersion,
    string InputSha256,
    string? HeadlineMetric,
    IReadOnlyList<ScorecardMetric> Metrics,
    DateTimeOffset CreatedValidTimeUtc,
    string? Limitation)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScorecardCorrectionProvenance? Correction { get; init; }
}

public sealed class ScorecardService
{
    private const int MaximumMetrics = 200;
    private const int MaximumNameLength = 128;
    private const int MaximumUnitLength = 64;
    private const int MaximumLimitationLength = 2_000;
    private const double MaximumAbsoluteMetric = 1e15;
    private readonly SqliteScenarioStore _store;
    private readonly TimeProvider _timeProvider;

    public ScorecardService(SqliteScenarioStore store, TimeProvider timeProvider)
    {
        _store = store;
        _timeProvider = timeProvider;
    }

    public async Task<PublicScorecard> PublishAsync(
        Guid scenarioId,
        ScorecardRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatedScorecard scorecard = Validate(scenarioId, request);
        string canonicalBody = PredictionJson.Canonicalize(scorecard);
        string contentSha256 = PredictionJson.ComputeSha256(scorecard);
        return await _store.PublishScorecardAsync(
            scorecard,
            canonicalBody,
            contentSha256,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
    }

    public async Task<PublicScorecard> GetAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default) =>
        await _store.FindScorecardAsync(scenarioId, cancellationToken)
        ?? throw new ScenarioApiException(
            StatusCodes.Status404NotFound,
            "Scorecard not found",
            $"Scenario {scenarioId:D} has no public scorecard.");

    internal static ValidatedScorecard Validate(Guid scenarioId, ScorecardRequest request)
    {
        if (scenarioId == Guid.Empty)
            throw Invalid("scenarioId", "must be a non-empty GUID.");
        Guid scorecardId = CanonicalGuid(request.ScorecardId, "scorecardId");
        Guid runId = CanonicalGuid(request.RunId, "runId");
        Guid revealId = CanonicalGuid(request.RevealId, "revealId");
        string scoringModelVersion = Token(request.ScoringModelVersion, "scoringModelVersion", 128);
        string inputSha256 = Sha256(request.InputSha256, "inputSha256");
        if (request.CreatedValidTimeUtc == default || request.CreatedValidTimeUtc == DateTimeOffset.MinValue ||
            request.CreatedValidTimeUtc == DateTimeOffset.MaxValue)
        {
            throw Invalid("createdValidTimeUtc", "must be a valid timestamp.");
        }
        if (request.Metrics is null || request.Metrics.Count is < 1 or > MaximumMetrics)
            throw Invalid("metrics", $"must contain between 1 and {MaximumMetrics} aggregate metrics.");

        var unique = new HashSet<(string Name, ScoreMetricBasis Basis)>();
        var metrics = new List<ScorecardMetric>(request.Metrics.Count);
        for (int index = 0; index < request.Metrics.Count; index++)
        {
            ScorecardMetric? metric = request.Metrics[index];
            if (metric is null)
                throw Invalid($"metrics[{index}]", "is required.");
            string name = Token(metric.Name, $"metrics[{index}].name", MaximumNameLength);
            if (!Enum.IsDefined(metric.Basis))
                throw Invalid($"metrics[{index}].basis", "is unsupported.");
            if (!Enum.IsDefined(metric.Status))
                throw Invalid($"metrics[{index}].status", "is unsupported.");
            if (!unique.Add((name, metric.Basis)))
                throw Invalid("metrics", $"contains duplicate metric ({name}, {metric.Basis}).");

            string? unit = metric.Unit;
            if (unit is not null)
                unit = BoundedText(unit, $"metrics[{index}].unit", MaximumUnitLength, requireNonblank: true);
            string? limitation = metric.Limitation;
            if (limitation is not null)
                limitation = BoundedText(
                    limitation,
                    $"metrics[{index}].limitation",
                    MaximumLimitationLength,
                    requireNonblank: true);
            ValidateNumber(metric.Value, $"metrics[{index}].value");
            ValidateNumber(metric.LowerBound, $"metrics[{index}].lowerBound");
            ValidateNumber(metric.UpperBound, $"metrics[{index}].upperBound");
            if (metric.LowerBound.HasValue != metric.UpperBound.HasValue)
                throw Invalid($"metrics[{index}]", "lowerBound and upperBound must both be supplied or both be null.");
            if (metric.LowerBound > metric.UpperBound)
                throw Invalid($"metrics[{index}]", "lowerBound cannot exceed upperBound.");

            if (metric.Status == ScoreMetricStatus.Scored)
            {
                if (metric.Value is null || unit is null || limitation is not null)
                    throw Invalid($"metrics[{index}]", "Scored requires value and unit with no limitation.");
                if (metric.LowerBound is double lower && metric.Value < lower ||
                    metric.UpperBound is double upper && metric.Value > upper)
                {
                    throw Invalid($"metrics[{index}].value", "must fall within supplied bounds.");
                }
            }
            else
            {
                if (metric.Value is not null || limitation is null ||
                    metric.LowerBound is not null || metric.UpperBound is not null)
                {
                    throw Invalid(
                        $"metrics[{index}]",
                        "Unavailable requires null value and bounds plus a nonblank limitation.");
                }
            }
            metrics.Add(metric with { Name = name, Unit = unit, Limitation = limitation });
        }

        string? headline = request.HeadlineMetric;
        string? topLevelLimitation = request.Limitation;
        if (headline is null)
        {
            topLevelLimitation = BoundedText(
                topLevelLimitation,
                "limitation",
                MaximumLimitationLength,
                requireNonblank: true);
        }
        else
        {
            headline = Token(headline, "headlineMetric", MaximumNameLength);
            int matchingScoredMetrics = metrics.Count(metric =>
                metric.Status == ScoreMetricStatus.Scored &&
                string.Equals(metric.Name, headline, StringComparison.Ordinal));
            if (matchingScoredMetrics != 1)
                throw Invalid("headlineMetric", "must reference exactly one Scored metric.");
            if (topLevelLimitation is not null)
                throw Invalid("limitation", "must be null when headlineMetric is present.");
        }

        return new ValidatedScorecard(
            scenarioId,
            scorecardId,
            runId,
            revealId,
            scoringModelVersion,
            inputSha256,
            headline,
            metrics,
            request.CreatedValidTimeUtc.ToUniversalTime(),
            topLevelLimitation)
        {
            Correction = ValidateCorrection(scenarioId, request, scorecardId, runId, revealId)
        };
    }

    private static ScorecardCorrectionProvenance? ValidateCorrection(
        Guid scenarioId, ScorecardRequest request, Guid scorecardId, Guid runId, Guid revealId)
    {
        ScorecardCorrectionProvenance? correction = request.Correction;
        if (correction is null)
        {
            if (request.ScoringModelVersion == ScoringCorrectionVersions.ScoringModel)
                throw Invalid("correction", "is required for the corrected scoring model.");
            return null;
        }
        if (correction.CorrectionVersion != ScoringCorrectionVersions.Correction ||
            correction.OriginalScoringModelVersion != ScenarioModelVersions.Scoring ||
            request.ScoringModelVersion != ScoringCorrectionVersions.ScoringModel)
            throw Invalid("correction", "must identify the supported original model and exact correction version.");
        Guid originalId = CanonicalGuid(correction.SupersedesScorecardId, "correction.supersedesScorecardId");
        _ = Sha256(correction.SupersedesOutputSha256, "correction.supersedesOutputSha256");
        _ = Sha256(correction.OriginalInputSha256, "correction.originalInputSha256");
        if (originalId == scorecardId) throw Invalid("scorecardId", "must differ from the preserved rejected artifact.");
        string input = PredictionJson.Canonicalize(new
        {
            runId = runId.ToString("D"), scenarioId = scenarioId.ToString("D"), revealId = revealId.ToString("D"),
            scoringModelVersion = ScoringCorrectionVersions.ScoringModel, correction
        });
        string inputHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
        if (request.InputSha256 != inputHash)
            throw Invalid("inputSha256", "must commit the exact correction provenance and original input hash.");
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", "scorecard-correction-v1", runId.ToString("D"),
            correction.SupersedesScorecardId, correction.SupersedesOutputSha256, correction.CorrectionVersion, inputHash)))[..16];
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        if (new Guid(bytes, bigEndian: true) != scorecardId)
            throw Invalid("scorecardId", "must match the deterministic correction identity.");
        return correction;
    }

    private static void ValidateNumber(double? value, string field)
    {
        if (value is double number && (!double.IsFinite(number) || Math.Abs(number) > MaximumAbsoluteMetric))
            throw Invalid(field, $"must be finite and within +/-{MaximumAbsoluteMetric:G}.");
    }

    private static Guid CanonicalGuid(string? value, string field)
    {
        if (value is null || value.Length != 36 || !Guid.TryParseExact(value, "D", out Guid parsed) ||
            parsed == Guid.Empty || !string.Equals(value, parsed.ToString("D"), StringComparison.Ordinal))
        {
            throw Invalid(field, "must be a canonical lowercase non-empty GUID.");
        }
        return parsed;
    }

    private static string Sha256(string? value, string field)
    {
        if (value is null || value.Length != 64 || value.Any(character =>
            character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            throw Invalid(field, "must be exactly 64 lowercase hexadecimal characters.");
        }
        return value;
    }

    private static string Token(string? value, string field, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength ||
            value.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.' or ':')))
        {
            throw Invalid(field, $"must be a nonblank token of at most {maximumLength} characters.");
        }
        return value;
    }

    private static string BoundedText(string? value, string field, int maximumLength, bool requireNonblank)
    {
        if (value is null || requireNonblank && string.IsNullOrWhiteSpace(value) || value.Length > maximumLength ||
            value.Any(char.IsControl))
        {
            throw Invalid(field, $"must be nonblank text of at most {maximumLength} characters.");
        }
        return value;
    }

    private static ScenarioApiException Invalid(string field, string detail) =>
        new(StatusCodes.Status400BadRequest, "Invalid scorecard", $"{field}: {detail}");
}
