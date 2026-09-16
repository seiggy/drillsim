using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrillingOperations;

public sealed record ScoreCorrectionProvenance(
    string CorrectionVersion, string OriginalScoringModelVersion,
    string SupersedesScorecardId, string SupersedesOutputSha256, string OriginalInputSha256);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CorrectScoreRequest(string Actor, string Reason, string ReviewedCorrectionHash);

public sealed record ScoringCorrectionReview(
    string ScenarioId, string RunId, bool CorrectionEnabled, string Reason,
    string? RejectedScorecardId = null, string? RejectedScorecardSha256 = null,
    string? ScoringInputSha256 = null, string? CorrectionVersion = null,
    int MetricCount = 0, int InvalidMetricCount = 0, int ChangedMetricCount = 0,
    string? ReviewedCorrectionHash = null, string? CorrectedScorecardId = null,
    string? CorrectedScorecardSha256 = null, string? CorrectedInputSha256 = null);

public sealed record ScoringCorrectionResult(
    string ScenarioId, string RunId, string Outcome, string Status,
    string RejectedScorecardId, string RejectedScorecardSha256, string ScoringInputSha256,
    string CorrectedScorecardId, string CorrectedScorecardSha256, string CorrectedInputSha256,
    string CorrectionVersion, string ReviewedCorrectionHash, string AuditId);

public sealed class ScoringCorrectionException(int statusCode, string code) : Exception(code)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}

internal sealed record ScoringCorrectionSnapshot(
    RunResponse Run, ScorecardDraft Rejected, PublicationStaging Publication,
    string FinalReceiptJson, string GuardHash);
internal sealed record ScoringCorrectionCandidate(
    ScoringCorrectionSnapshot Snapshot, ScorecardDraft Corrected, ScoringCorrectionReview Review);

public static class ScoreCorrection
{
    public const string Version = "absolute-error-bounds-v2";
    public const string ModelVersion = "scoring-model-v1-absolute-error-bounds-v2";

    internal static ScorecardDraft Create(ScorecardDraft rejected, ScorecardDraft historical, ScorecardDraft recalculated)
    {
        Require(rejected.ScoringModelVersion == ScoreArtifactIntegrity.ModelVersion &&
            rejected.CanonicalInputJson == historical.CanonicalInputJson &&
            rejected.CanonicalOutputJson == historical.CanonicalOutputJson &&
            rejected.InputSha256 == historical.InputSha256 && rejected.OutputSha256 == historical.OutputSha256 &&
            rejected.CanonicalInputJson == recalculated.CanonicalInputJson &&
            rejected.InputSha256 == recalculated.InputSha256 &&
            rejected.Metrics.Count == recalculated.Metrics.Count, "ScoringCorrectionOriginalNotReproduced");
        Require(InvalidMetricCount(rejected.Metrics) > 0, "ScoringCorrectionNoKnownBoundsFailure");
        int changed = 0;
        for (int i = 0; i < rejected.Metrics.Count; i++)
        {
            ScorecardMetric old = rejected.Metrics[i], current = recalculated.Metrics[i];
            if (CanonicalJson.Serialize(old) == CanonicalJson.Serialize(current)) continue;
            Require(old.Status == ScoreMetricStatus.Scored && old.Unit == "m" &&
                old.Name.Contains("P50AbsoluteError.", StringComparison.Ordinal) &&
                old.LowerBound > 0 && current.LowerBound == 0 &&
                CanonicalJson.Serialize(old with { LowerBound = current.LowerBound }) == CanonicalJson.Serialize(current),
                "ScoringCorrectionUnexpectedMetricChange");
            changed++;
        }
        Require(changed > 0, "ScoringCorrectionNoKnownBoundsFailure");
        ScoreMetricContract.ValidateForPublication(recalculated.Metrics);
        var provenance = new ScoreCorrectionProvenance(Version, rejected.ScoringModelVersion,
            rejected.ScorecardId, rejected.OutputSha256, rejected.InputSha256);
        string input = InputJson(rejected.RunId, rejected.ScenarioId, rejected.RevealId, provenance);
        string inputHash = DeterministicIdentity.Sha256(input);
        string id = Identity(rejected.RunId, provenance, inputHash);
        var request = new AnalysisScorecardRequest(id, rejected.RunId, rejected.RevealId, ModelVersion,
            inputHash, rejected.HeadlineMetric, recalculated.Metrics, rejected.CreatedValidTimeUtc) { Correction = provenance };
        string output = CanonicalJson.Serialize(request);
        return new(id, rejected.RunId, rejected.ScenarioId, rejected.RevealId, ModelVersion, inputHash, input,
            output, DeterministicIdentity.Sha256(output), rejected.HeadlineMetric, recalculated.Metrics, rejected.CreatedValidTimeUtc);
    }

    internal static string InputJson(string runId, string scenarioId, string revealId, ScoreCorrectionProvenance provenance) =>
        CanonicalJson.Serialize(new { runId, scenarioId, revealId, scoringModelVersion = ModelVersion, correction = provenance });
    internal static string Identity(string runId, ScoreCorrectionProvenance provenance, string inputHash) =>
        DeterministicIdentity.Create("scorecard-correction-v1", runId, provenance.SupersedesScorecardId,
            provenance.SupersedesOutputSha256, Version, inputHash);
    internal static int InvalidMetricCount(IReadOnlyList<ScorecardMetric> metrics) =>
        metrics.Count(x => x.Status == ScoreMetricStatus.Scored && (x.Value < x.LowerBound || x.Value > x.UpperBound));
    internal static void Require(bool condition, string code)
    {
        if (!condition) throw new ScoringCorrectionException(409, code);
    }
}

public sealed class ScoringCorrectionCoordinator(
    DrillingOperationsStore store, ScoreCoordinator scoring, AnalysisScorecardClient analysis,
    ILogger<ScoringCorrectionCoordinator> logger)
{
    // S9 delivery and correction share this owner-local lock; persisted guards still arbitrate other connections.
    internal static readonly SemaphoreSlim MutationGate = new(1, 1);

    public async Task<ScoringCorrectionReview> ReviewAsync(string scenarioId, string runId, CancellationToken ct)
    {
        ValidateIds(scenarioId, runId);
        try
        {
            ScoringCorrectionCandidate candidate = await CheckAsync(scenarioId, runId, ct);
            await store.RecheckScoringCorrectionAsync(candidate.Snapshot, ct);
            return candidate.Review;
        }
        catch (ScoringCorrectionException e) when (e.StatusCode == 409)
        {
            Log(e);
            return new(scenarioId, runId, false, e.Code);
        }
    }

    public async Task<ApiOutcome> CorrectAsync(
        string route, string key, string scenarioId, string runId, CorrectScoreRequest request, CancellationToken ct)
    {
        ValidateIds(scenarioId, runId);
        if (!Label(request.Actor, 100) || !Label(request.Reason, 500) ||
            !AnalysisScorecardClient.LowerHash(request.ReviewedCorrectionHash))
            throw new ScoringCorrectionException(400, "ScoringCorrectionRequestInvalid");
        string canonical = CanonicalJson.Serialize(new { scenarioId, runId, request });
        await MutationGate.WaitAsync(ct);
        try
        {
            ApiOutcome? replay = await store.TryReplayAsync(route, key, canonical, ct);
            if (replay is not null) return replay;
            try
            {
                ScoringCorrectionCandidate candidate = await CheckAsync(scenarioId, runId, ct);
                ScoreCorrection.Require(candidate.Review.ReviewedCorrectionHash == request.ReviewedCorrectionHash,
                    "ScoringCorrectionReviewStale");
                return await store.CorrectScoreAsync(route, key, canonical, request, candidate, ct);
            }
            catch (ScoringCorrectionException e)
            {
                Log(e);
                if (e.StatusCode != 409) throw;
                return await store.ExecuteIdempotentAsync(route, key, canonical, (_, _, _) =>
                    Task.FromResult(new ApiOutcome(409, CanonicalJson.Serialize(new { status = 409, title = e.Code }))), ct);
            }
        }
        finally { MutationGate.Release(); }
    }

    private async Task<ScoringCorrectionCandidate> CheckAsync(string scenarioId, string runId, CancellationToken ct)
    {
        try
        {
            ScoringCorrectionSnapshot snapshot = await store.CaptureScoringCorrectionAsync(scenarioId, runId, ct);
            await analysis.VerifyUnscoredRevealAsync(snapshot.Publication, snapshot.FinalReceiptJson, ct);
            ScorecardDraft historical = await scoring.CalculateForCorrectionAsync(snapshot.Run, legacyBounds: true, ct);
            ScorecardDraft recalculated = await scoring.CalculateForCorrectionAsync(snapshot.Run, legacyBounds: false, ct);
            ScorecardDraft corrected = ScoreCorrection.Create(snapshot.Rejected, historical, recalculated);
            await analysis.VerifyUnscoredRevealAsync(snapshot.Publication, snapshot.FinalReceiptJson, ct);
            string reviewedHash = DeterministicIdentity.Sha256(CanonicalJson.Serialize(new
            {
                contract = "scoring-correction-review-v1", snapshot.GuardHash,
                corrected.InputSha256, corrected.OutputSha256, correctionVersion = ScoreCorrection.Version
            }));
            int changed = snapshot.Rejected.Metrics.Zip(corrected.Metrics)
                .Count(x => CanonicalJson.Serialize(x.First) != CanonicalJson.Serialize(x.Second));
            var review = new ScoringCorrectionReview(scenarioId, runId, true,
                "Review the preserved rejected scorecard and the versioned bounds-only correction. Scoring publication remains a separate explicit action.",
                snapshot.Rejected.ScorecardId, snapshot.Rejected.OutputSha256, snapshot.Rejected.InputSha256,
                ScoreCorrection.Version, corrected.Metrics.Count, ScoreCorrection.InvalidMetricCount(snapshot.Rejected.Metrics),
                changed, reviewedHash, corrected.ScorecardId, corrected.OutputSha256, corrected.InputSha256);
            return new(snapshot, corrected, review);
        }
        catch (ScoringCorrectionException) { throw; }
        catch (ScoringException e)
        {
            throw new ScoringCorrectionException(e.StatusCode >= 500 ? 503 : 409, "ScoringCorrectionInputsUnverified");
        }
        catch (Exception e) when (e is PersistenceIntegrityException or JsonException or FormatException or
            InvalidOperationException or ArgumentException or KeyNotFoundException or OverflowException or
            Microsoft.Extensions.Options.OptionsValidationException)
        {
            throw new ScoringCorrectionException(409, "ScoringCorrectionIntegrityMismatch");
        }
        catch (Exception e) when (e is HttpRequestException || e is OperationCanceledException && !ct.IsCancellationRequested)
        {
            throw new ScoringCorrectionException(503, "ScoringCorrectionDependencyUnavailable");
        }
    }

    private void Log(ScoringCorrectionException e) =>
        logger.LogWarning(new EventId(8300, "ScoringCorrectionDenied"), "Scoring correction denied: {DiagnosticCode}.", e.Code);
    internal static void ValidateIds(string scenarioId, string runId)
    {
        if (!RequestValidation.IsCanonicalGuid(scenarioId, out _) || !RequestValidation.IsCanonicalGuid(runId, out _))
            throw new ScoringCorrectionException(400, "ScoringCorrectionIdentityInvalid");
    }
    private static bool Label(string? value, int maximum) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && value == value.Trim() &&
        value.All(c => c is >= ' ' and <= '~');
}

public sealed partial class AnalysisScorecardClient
{
    internal async Task VerifyUnscoredRevealAsync(PublicationStaging plan, string finalReceiptJson, CancellationToken ct)
    {
        using HttpResponseMessage scenarioResponse = await client.GetAsync($"api/scenarios/{plan.ScenarioId}", ct);
        RequireCorrectionRead(scenarioResponse);
        await scenarioResponse.Content.LoadIntoBufferAsync(1024 * 1024, ct);
        using JsonDocument scenarioDocument = JsonDocument.Parse(await scenarioResponse.Content.ReadAsStringAsync(ct));
        JsonElement scenario = scenarioDocument.RootElement;
        ScoreCorrection.Require(scenario.GetProperty("scenarioId").GetString() == plan.ScenarioId &&
            scenario.GetProperty("status").GetString() == "Revealed" &&
            scenario.GetProperty("clonedFieldId").GetString() == plan.ClonedFieldId &&
            scenario.GetProperty("asOfUtc").GetDateTimeOffset() == plan.ValidTimeUtc &&
            scenario.GetProperty("initialAsOfUtc").GetDateTimeOffset().AddDays(1) == plan.ValidTimeUtc &&
            scenario.GetProperty("scoringModelVersion").GetString() == ScoreArtifactIntegrity.ModelVersion,
            "ScoringCorrectionRevealChanged");
        using HttpResponseMessage receiptResponse = await client.GetAsync(
            $"internal/scenarios/{plan.ScenarioId}/reveal/{plan.RevealId}/status", ct);
        RequireCorrectionRead(receiptResponse);
        await receiptResponse.Content.LoadIntoBufferAsync(65536, ct);
        AnalysisRevealReceipt expected = JsonSerializer.Deserialize<AnalysisRevealReceipt>(finalReceiptJson, CanonicalJson.SerializerOptions)!;
        AnalysisRevealReceipt? receipt = await receiptResponse.Content.ReadFromJsonAsync<AnalysisRevealReceipt>(CanonicalJson.SerializerOptions, ct);
        ScoreCorrection.Require(receipt is not null && receipt == expected && receipt.Status == "Revealed",
            "ScoringCorrectionRevealReceiptChanged");
        using HttpResponseMessage score = await client.GetAsync($"api/scenarios/{plan.ScenarioId}/scorecard", ct);
        if (score.StatusCode == HttpStatusCode.NotFound) return;
        RequireCorrectionRead(score);
        throw new ScoringCorrectionException(409, "ScoringCorrectionAlreadyPublished");
    }
    private static void RequireCorrectionRead(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new ScoringCorrectionException(503, "ScoringCorrectionDependencyUnverified");
    }
}
