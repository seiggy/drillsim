using System.Net;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed partial class OperatorWorkflowService
{
    public async Task<OperatorScoringCorrectionReview> GetScoringCorrectionAsync(Guid scenarioId, Guid runId, CancellationToken ct)
    {
        _ = await GetScenarioAsync(scenarioId, ct);
        _ = await backend.GetOwnedRunAsync(scenarioId, runId, ct);
        return await backend.GetScoringCorrectionAsync(scenarioId, runId, ct);
    }

    public async Task<OperatorScoringCorrectionResult> CorrectScoreAsync(
        Guid scenarioId, Guid runId, OperatorScoringCorrectionRequest request, string key, CancellationToken ct)
    {
        if (!RecoveryLabel(request.Actor, 100) || !RecoveryLabel(request.Reason, 500))
            throw new OperatorWorkflowException(400, "Correction requires an actor (1-100) and reason (1-500), using visible ASCII without surrounding whitespace.");
        ValidateHash(request.ReviewedCorrectionHash);
        _ = await GetScenarioAsync(scenarioId, ct);
        _ = await backend.GetOwnedRunAsync(scenarioId, runId, ct);
        return await backend.CorrectScoreAsync(scenarioId, runId, request, key, ct);
    }
}

public sealed partial class OperatorBackendClient
{
    internal async Task<OperatorScoringCorrectionReview> GetScoringCorrectionAsync(Guid scenarioId, Guid runId, CancellationToken ct)
    {
        var review = await GetAsync<OperatorScoringCorrectionReview>(
            $"scenarios/{scenarioId:D}/runs/{runId:D}/scoring-correction", false, ct) ?? throw InvalidResponse();
        if (review.ScenarioId != scenarioId || review.RunId != runId) throw InvalidResponse();
        if (!review.CorrectionEnabled)
            return new(scenarioId, runId, false, "The rejected evaluation could not be verified as the unchanged, unpublished S9 absolute-error-bound failure. Correction is unavailable.");
        if (review.RejectedScorecardId is null || review.RejectedScorecardId == Guid.Empty ||
            review.CorrectedScorecardId is null || review.CorrectedScorecardId == Guid.Empty ||
            review.RejectedScorecardId == review.CorrectedScorecardId ||
            !RecoveryHash(review.RejectedScorecardSha256) || !RecoveryHash(review.ScoringInputSha256) ||
            !RecoveryHash(review.CorrectedScorecardSha256) || !RecoveryHash(review.CorrectedInputSha256) ||
            !RecoveryHash(review.ReviewedCorrectionHash) || review.CorrectionVersion != ScoringCorrectionVersions.Correction ||
            review.MetricCount is < 1 or > 200 || review.InvalidMetricCount < 1 ||
            review.ChangedMetricCount < review.InvalidMetricCount || review.ChangedMetricCount > review.MetricCount)
            throw InvalidResponse();
        return new(scenarioId, runId, true,
            "Review the rejected evaluation and versioned bounds-only correction. Original inputs and point scores are preserved; publication requires a separate evaluation action.",
            review.RejectedScorecardId, review.RejectedScorecardSha256, review.ScoringInputSha256, review.CorrectionVersion,
            review.MetricCount, review.InvalidMetricCount, review.ChangedMetricCount, review.ReviewedCorrectionHash,
            review.CorrectedScorecardId, review.CorrectedScorecardSha256, review.CorrectedInputSha256);
    }

    internal async Task<OperatorScoringCorrectionResult> CorrectScoreAsync(
        Guid scenarioId, Guid runId, OperatorScoringCorrectionRequest body, string key, CancellationToken ct)
    {
        using var request = Request(HttpMethod.Post, $"scenarios/{scenarioId:D}/runs/{runId:D}/correct-score", key);
        request.Content = JsonContent.Create(body);
        using var response = await clients.CreateClient(ClientName).SendAsync(request, ct);
        RequireSuccess(response);
        if (response.StatusCode != HttpStatusCode.OK) throw InvalidResponse();
        var result = await ReadAsync<OperatorScoringCorrectionResult>(response, ct);
        if (result.ScenarioId != scenarioId || result.RunId != runId || result.Outcome != "score-corrected" ||
            result.Status != "AwaitingDependency" || result.CorrectionVersion != ScoringCorrectionVersions.Correction ||
            result.RejectedScorecardId == Guid.Empty || result.CorrectedScorecardId == Guid.Empty ||
            result.CorrectedScorecardId == result.RejectedScorecardId || result.AuditId == Guid.Empty ||
            !RecoveryHash(result.RejectedScorecardSha256) || !RecoveryHash(result.ScoringInputSha256) ||
            !RecoveryHash(result.CorrectedScorecardSha256) || !RecoveryHash(result.CorrectedInputSha256) ||
            result.ReviewedCorrectionHash != body.ReviewedCorrectionHash)
            throw InvalidResponse();
        return new(scenarioId, runId, "score-corrected", "AwaitingDependency", result.RejectedScorecardId,
            result.RejectedScorecardSha256, result.ScoringInputSha256, result.CorrectedScorecardId,
            result.CorrectedScorecardSha256, result.CorrectedInputSha256, result.CorrectionVersion,
            result.ReviewedCorrectionHash, result.AuditId);
    }
}
