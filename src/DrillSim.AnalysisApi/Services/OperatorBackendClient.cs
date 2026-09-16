using System.Net;
using System.Text.Json;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed partial class OperatorBackendClient(IHttpClientFactory clients, OperatorConfiguration configuration)
{
    public const string ClientName = "LocalOperatorDrillingOperations";
    private const string Prefix = "drillingoperations/api/";
    private const long MaximumResponseBytes = 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static readonly string[] StageNames =
    [
        "S0BindWorld", "S1MaterializePlan", "S2ExecuteDrilling", "S3GenerateSurvey", "S4SampleGeology",
        "S5GenerateLogs", "S6DesignCompletion", "S7RunProduction", "S8PublishReveal", "S9Score"
    ];
    private static readonly string[] StageLabels =
    [
        "Verify existing world binding", "Materialize approved plan", "Execute drilling", "Generate survey",
        "Sample geology", "Generate observable logs", "Design completion", "Run production",
        "Publish new evidence", "Evaluate prediction"
    ];
    private static readonly HashSet<string> RunStatuses =
    [
        "Queued", "Running", "Blocked", "AwaitingDependency", "AwaitingApproval", "ReadyToReveal",
        "PublishFailed", "Revealed", "Scored", "Cancelled", "Failed"
    ];
    private static readonly HashSet<string> StageStatuses =
    [
        "Pending", "Running", "Completed", "Blocked", "AwaitingDependency", "AwaitingApproval", "Failed", "Cancelled"
    ];

    internal async Task<Binding?> GetBindingAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        Binding? binding = await GetAsync<Binding>($"scenarios/{scenarioId:D}/binding", true, cancellationToken);
        if (binding is not null && binding.ScenarioId != scenarioId)
            throw InvalidResponse();
        return binding;
    }

    internal async Task<Run?> GetCurrentRunAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        AuditEntry[] entries = await GetAsync<AuditEntry[]>($"audit?scenarioId={scenarioId:D}", false, cancellationToken)
            ?? throw InvalidResponse();
        if (entries.Length > 4096 || entries.Any(x => x.ScenarioId != scenarioId || x.Sequence < 1) ||
            entries.Select(x => x.Sequence).Distinct().Count() != entries.Length)
            throw InvalidResponse();
        AuditEntry? latest = entries.Where(x => x.Action == "run.created").MaxBy(x => x.Sequence);
        if (latest is null) return null;
        if (!Guid.TryParseExact(latest.SubjectId, "D", out Guid id) || id == Guid.Empty)
            throw InvalidResponse();
        return await GetOwnedRunAsync(scenarioId, id, cancellationToken);
    }

    internal async Task<Run> GetOwnedRunAsync(Guid scenarioId, Guid runId, CancellationToken cancellationToken)
    {
        Run? run = await GetAsync<Run>($"runs/{runId:D}", true, cancellationToken);
        if (run is null || run.ScenarioId != scenarioId || run.RunId != runId)
            throw new OperatorWorkflowException(404, "Run not found in the requested scenario.");
        ValidateRun(run);
        return run;
    }

    internal async Task<Completion?> GetCompletionAsync(Guid scenarioId, Guid runId, CancellationToken cancellationToken)
    {
        Completion? review = await GetAsync<Completion>($"runs/{runId:D}/completion", true, cancellationToken);
        if (review is null) return null;
        if (review.RunId != runId || review.ScenarioId != scenarioId)
            throw new OperatorWorkflowException(404, "Completion review not found in the requested scenario and run.");
        if (review.Status is not ("Draft" or "Approved") ||
            review.OpeningsHash is not { Length: 64 } || review.OpeningsHash.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'f')) ||
            review.Openings is not { Count: >= 1 and <= 256 } || review.Openings.Any(x =>
                x is null || string.IsNullOrWhiteSpace(x.ReservoirName) || x.ReservoirName.Length > 200 ||
                x.ReservoirName.Any(char.IsControl) || x.Type is not ("Perforated" or "OpenHole" or "Isolated") ||
                !double.IsFinite(x.TopMdM) || !double.IsFinite(x.BaseMdM) || x.TopMdM < 0 || x.BaseMdM <= x.TopMdM ||
                !double.IsFinite(x.WellboreRadiusM) || x.WellboreRadiusM is < .02 or > .5 ||
                !double.IsFinite(x.Skin) || x.Skin is < -20 or > 100 ||
                !double.IsFinite(x.Efficiency) || x.Efficiency is < 0 or > 1 ||
                !double.IsFinite(x.UncertaintyM) || x.UncertaintyM is < 0 or > 100))
            throw InvalidResponse();
        return review;
    }

    internal async Task ApproveCompletionAsync(
        Guid runId, string actor, string reviewedHash, string key, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Post, $"runs/{runId:D}/completion/approve", key);
        request.Headers.Add("X-DrillSim-Human-Actor", actor);
        request.Headers.Add("X-DrillSim-Reviewed-Openings-Hash", reviewedHash);
        using HttpResponseMessage response = await clients.CreateClient(ClientName).SendAsync(request, cancellationToken);
        RequireSuccess(response);
        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Accepted))
            throw new OperatorWorkflowException(503, "The operator backend has not confirmed completion approval.");
    }

    internal async Task<IReadOnlyList<OperatorStage>> GetStagesAsync(Guid runId, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        using var request = Request(HttpMethod.Get, $"runs/{runId:D}/events");
        using HttpResponseMessage response = await clients.CreateClient(ClientName)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        RequireSuccess(response);
        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(timeout.Token));
        char[] buffer = new char[4096];
        string remaining = string.Empty;
        int total = 0;
        var stages = new Dictionary<string, OperatorStage>(StringComparer.Ordinal);
        while (stages.Count < 10)
        {
            int read = await reader.ReadAsync(buffer.AsMemory(), timeout.Token);
            if (read == 0) break;
            total += read;
            if (total > 128 * 1024) throw InvalidResponse();
            remaining += new string(buffer, 0, read);
            int newline;
            while ((newline = remaining.IndexOf('\n')) >= 0)
            {
                string line = remaining[..newline].TrimEnd('\r');
                remaining = remaining[(newline + 1)..];
                if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;
                StageEvent stage = JsonSerializer.Deserialize<StageEvent>(line.AsSpan(6), JsonOptions) ?? throw InvalidResponse();
                int index = Array.IndexOf(StageNames, stage.Stage);
                if (index < 0 || stage.Status is null || !StageStatuses.Contains(stage.Status) ||
                    stage.AttemptCount is < 0 or > 1_000_000)
                    throw InvalidResponse();
                stages[stage.Stage] = new(stage.Stage, StageLabels[index], stage.Status, stage.AttemptCount);
            }
        }
        if (stages.Count != 10) throw InvalidResponse();
        return StageNames.Select(x => stages[x]).ToArray();
    }

    internal async Task<Run> StartAsync(
        Guid scenarioId, string sealHash, string planArtifactId, string planArtifactSha256,
        string key, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Post, "runs", key);
        request.Content = JsonContent.Create(new
        {
            scenarioId, approvedSealedPredictionHash = sealHash, planArtifactId, planArtifactSha256
        });
        using HttpResponseMessage response = await clients.CreateClient(ClientName).SendAsync(request, cancellationToken);
        RequireSuccess(response);
        Run run = await ReadAsync<Run>(response, cancellationToken);
        if (run.ScenarioId != scenarioId || run.RunId == Guid.Empty) throw InvalidResponse();
        ValidateRun(run);
        return run;
    }

    internal async Task PostRunActionAsync(Guid runId, string action, string key, CancellationToken cancellationToken)
    {
        if (action is not ("cancel" or "resume" or "publish" or "score"))
            throw new ArgumentOutOfRangeException(nameof(action));
        using var request = Request(HttpMethod.Post, $"runs/{runId:D}/{action}", key);
        using HttpResponseMessage response = await clients.CreateClient(ClientName).SendAsync(request, cancellationToken);
        RequireSuccess(response);
        if (action is "publish" or "score" && response.StatusCode != HttpStatusCode.OK)
            throw new OperatorWorkflowException(503, "The operator backend has not confirmed completion of this action.");
        // Successful command bodies can contain internal publication/scoring metadata. Never deserialize or return them.
    }

    internal async Task<OperatorPublicationRecoveryReview> GetPublicationRecoveryAsync(
        Guid scenarioId, Guid runId, CancellationToken cancellationToken)
    {
        var review = await GetAsync<OperatorPublicationRecoveryReview>(
            $"scenarios/{scenarioId:D}/runs/{runId:D}/publication-recovery", false, cancellationToken) ?? throw InvalidResponse();
        if (review.ScenarioId != scenarioId || review.RunId != runId) throw InvalidResponse();
        if (!review.RecoveryEnabled)
            return new(scenarioId, runId, false, review.Reason == "PublicationRecoveryVerifiedRecordMissing"
                ? "A previously verified staged record is missing from an ontology service. Recovery is blocked; no record was restored or changed."
                : "Guarded publication recovery is unavailable. The backend could not verify an unchanged, unactivated S8 write-rejection checkpoint.");
        if (!RecoveryHash(review.ReviewedPublicationHash) || !RecoveryHash(review.StagedManifestSha256) ||
            !RecoveryHash(review.PublicationPlanSha256) || review.OperationCount is < 1 or > 20000 ||
            review.VerifiedOperationCount < 0 || review.PendingOperationCount < 1 ||
            review.PendingOperationCount + review.VerifiedOperationCount != review.OperationCount ||
            review.CompletedStageCount != 8)
            throw InvalidResponse();
        // Do not forward backend text, operation bodies, world identifiers or extension fields.
        return new(scenarioId, runId, true,
            "Review the staged manifest and plan hashes. Recovery preserves this run and all staged evidence; publication requires a separate explicit action.",
            review.ReviewedPublicationHash, review.StagedManifestSha256, review.PublicationPlanSha256,
            review.OperationCount, review.VerifiedOperationCount, review.PendingOperationCount, 8);
    }

    internal async Task<OperatorPublicationRecoveryResult> RecoverPublicationAsync(
        Guid scenarioId, Guid runId, OperatorPublicationRecoveryRequest body, string key, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Post, $"scenarios/{scenarioId:D}/runs/{runId:D}/recover-publication", key);
        request.Content = JsonContent.Create(body);
        using HttpResponseMessage response = await clients.CreateClient(ClientName).SendAsync(request, cancellationToken);
        RequireSuccess(response);
        if (response.StatusCode != HttpStatusCode.OK) throw InvalidResponse();
        var result = await ReadAsync<OperatorPublicationRecoveryResult>(response, cancellationToken);
        if (result.ScenarioId != scenarioId || result.RunId != runId || result.Outcome != "publication-recovered" ||
            result.PreviousStatus != "Failed" || result.Status != "PublishFailed" ||
            result.ReviewedPublicationHash != body.ReviewedPublicationHash || result.AuditId == Guid.Empty)
            throw InvalidResponse();
        return new(scenarioId, runId, "publication-recovered", "Failed", "PublishFailed", result.ReviewedPublicationHash, result.AuditId);
    }

    private static bool RecoveryHash(string? value) =>
        value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    private HttpRequestMessage Request(HttpMethod method, string relativeRoute, string? key = null)
    {
        if (!configuration.Enabled)
            throw new OperatorWorkflowException(503, "Local operator backend is not configured.");
        var request = new HttpRequestMessage(method, Prefix + relativeRoute);
        request.Headers.Add("X-DrillSim-Internal-Key", configuration.Key);
        if (key is not null) request.Headers.Add("Idempotency-Key", key);
        return request;
    }

    private async Task<T?> GetAsync<T>(string route, bool allowMissing, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Get, route);
        using HttpResponseMessage response = await clients.CreateClient(ClientName).SendAsync(request, cancellationToken);
        if (allowMissing && response.StatusCode == HttpStatusCode.NotFound) return default;
        RequireSuccess(response);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await response.Content.LoadIntoBufferAsync(MaximumResponseBytes, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken) ?? throw InvalidResponse();
    }

    private static void RequireSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        int status = response.StatusCode switch
        {
            HttpStatusCode.NotFound => 404,
            HttpStatusCode.Conflict or HttpStatusCode.BadRequest => 409,
            _ => 503
        };
        throw new OperatorWorkflowException(status, status == 409
            ? "The operator backend rejected the action prerequisites. Refresh before a new attempt."
            : "The operator backend is unavailable or could not verify the requested operation.");
    }

    private static void ValidateRun(Run run)
    {
        if (run.Status is null || !RunStatuses.Contains(run.Status) ||
            (run.CurrentStage is not null && !StageNames.Contains(run.CurrentStage)))
            throw InvalidResponse();
    }

    private static OperatorWorkflowException InvalidResponse() =>
        new(503, "The operator backend returned an unverifiable progress document.");

    internal sealed record Binding(
        Guid ScenarioId, string ApprovedSealedPredictionHash, string SourcePackageSha256, string WorldModelVersion);
    internal sealed record Run(Guid RunId, Guid ScenarioId, string Status, string? CurrentStage);
    internal sealed record Completion(
        Guid RunId, Guid ScenarioId, string Status, string OpeningsHash, IReadOnlyList<OperatorOpening> Openings);
    private sealed record AuditEntry(Guid ScenarioId, long Sequence, string Action, string SubjectId);
    private sealed record StageEvent(string Stage, string Status, int AttemptCount);
}
