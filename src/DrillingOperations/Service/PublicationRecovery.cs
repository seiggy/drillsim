using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrillingOperations;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RecoverPublicationRequest(string Actor, string Reason, string ReviewedPublicationHash);
public sealed record PublicationRecoveryReview(
    string ScenarioId, string RunId, bool RecoveryEnabled, string Reason,
    string? ReviewedPublicationHash = null, string? StagedManifestSha256 = null,
    string? PublicationPlanSha256 = null, int OperationCount = 0,
    int VerifiedOperationCount = 0, int PendingOperationCount = 0, int CompletedStageCount = 0);
public sealed record PublicationRecoveryResult(
    string ScenarioId, string RunId, string Outcome, string PreviousStatus, string Status,
    string ReviewedPublicationHash, string AuditId);

internal sealed record PublicationRecoverySnapshot(
    PublicationRecoveryReview Review, PublicationStaging Plan, TruthBindingResponse Binding,
    MaterializedPlan MaterializedPlan, string GuardHash,
    IReadOnlyList<PublicationRecoveryOperation> Operations);
internal sealed record PublicationRecoveryOperation(
    PublicationWriteOperation Operation, string Status, string? ResultHash, string? ResultBusinessJson);

public sealed class PublicationRecoveryException(int statusCode, string code) : Exception(code)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}

public sealed class PublicationRecoveryCoordinator(
    DrillingOperationsStore store, AnalysisVerificationClient verification,
    AnalysisRevealClient reveal, OntologyPublicationClient ontology,
    ILogger<PublicationRecoveryCoordinator> logger)
{
    // The owner-local backend has one publisher. Serialize its outbound publication effects with
    // recovery so an already admitted publish cannot activate records between review and commit.
    internal static readonly SemaphoreSlim PublicationMutationGate = new(1, 1);

    public async Task<PublicationRecoveryReview> ReviewAsync(string scenarioId, string runId, CancellationToken ct)
    {
        ValidateIds(scenarioId, runId);
        try
        {
            PublicationRecoverySnapshot snapshot = await CheckAsync(scenarioId, runId, ct);
            await store.RecheckPublicationRecoveryAsync(snapshot, ct);
            return snapshot.Review;
        }
        catch (PublicationRecoveryException e) when (e.StatusCode == 409)
        {
            Log(e);
            return new(scenarioId, runId, false, e.Code);
        }
    }

    public async Task<ApiOutcome> RecoverAsync(
        string route, string key, string scenarioId, string runId, RecoverPublicationRequest request, CancellationToken ct)
    {
        await PublicationMutationGate.WaitAsync(ct);
        try { return await RecoverCoreAsync(route, key, scenarioId, runId, request, ct); }
        finally { PublicationMutationGate.Release(); }
    }

    private async Task<ApiOutcome> RecoverCoreAsync(
        string route, string key, string scenarioId, string runId, RecoverPublicationRequest request, CancellationToken ct)
    {
        ValidateIds(scenarioId, runId);
        ValidateRequest(request);
        // Replay precedes live-state checks: a successful recovery remains replayable after publication/restart.
        string canonical = CanonicalJson.Serialize(new { scenarioId, runId, request });
        ApiOutcome? replay = await store.TryReplayAsync(route, key, canonical, ct);
        if (replay is not null) return replay;
        try
        {
            PublicationRecoverySnapshot snapshot = await CheckAsync(scenarioId, runId, ct);
            if (snapshot.Review.ReviewedPublicationHash != request.ReviewedPublicationHash)
                throw new PublicationRecoveryException(409, "PublicationRecoveryReviewStale");
            return await store.RecoverPublicationAsync(route, key, canonical, request, snapshot, ct);
        }
        catch (PublicationRecoveryException e)
        {
            Log(e);
            // A concurrent identical recovery may have committed while external reads were in flight.
            ApiOutcome? committed = await store.TryReplayAsync(route, key, canonical, ct);
            if (committed is not null) return committed;
            if (e.StatusCode == 409)
                return await store.ExecuteIdempotentAsync(route, key, canonical, (_, _, _) =>
                    Task.FromResult(new ApiOutcome(409, CanonicalJson.Serialize(new { status = 409, title = e.Code }))), ct);
            throw;
        }
    }

    private async Task<PublicationRecoverySnapshot> CheckAsync(string scenarioId, string runId, CancellationToken ct)
    {
        try
        {
            PublicationRecoverySnapshot snapshot = await store.CapturePublicationRecoveryAsync(scenarioId, runId, ct);
            await store.ValidatePublicationRecoveryArtifactsAsync(snapshot, ct);
            var (scenario, prediction) = await verification.GetAsync(Guid.Parse(scenarioId), ct);
            MaterializedPlan plan = snapshot.MaterializedPlan;
            if (scenario.ScenarioId.ToString("D") != scenarioId || scenario.Status != "HumanApproved" ||
                scenario.WorldModelVersion != snapshot.Binding.WorldModelVersion ||
                scenario.ObservationModelVersion != snapshot.Plan.ObservationModelVersion ||
                scenario.InitialAsOfUtc?.AddDays(1) != snapshot.Plan.ValidTimeUtc ||
                prediction.ScenarioId != scenario.ScenarioId || prediction.Seal is null || prediction.Approval is null ||
                prediction.Seal.Sha256 != prediction.Approval.SealedSha256 ||
                prediction.Seal.Sha256 != plan.SourcePredictionSealSha256 ||
                prediction.Body?.FieldPackageSha256 != plan.SourcePackageSha256 ||
                prediction.Revision != plan.SourcePredictionRevision ||
                prediction.Body.CandidateId != plan.CandidateId ||
                CanonicalJson.Serialize(prediction.Body.ProposedWellPath) != CanonicalJson.Serialize(plan.Stations) ||
                snapshot.Plan.ClonedFieldId != DeterministicIdentity.Create("published-field-v1", scenarioId, scenario.SourceFieldId.ToString("D")))
                throw new PublicationRecoveryException(409, "PublicationRecoveryBindingChanged");

            await reveal.VerifyRecoveryUnpublishedAsync(snapshot.Plan, ct);
            foreach (PublicationRecoveryOperation operation in snapshot.Operations)
                await ontology.VerifyRecoveryStagingAsync(snapshot.Plan, operation, ct);
            await reveal.VerifyRecoveryUnpublishedAsync(snapshot.Plan, ct);
            return snapshot;
        }
        catch (PublicationRecoveryException) { throw; }
        catch (BindingVerificationException e)
        {
            throw new PublicationRecoveryException(e.StatusCode >= 500 ? 503 : 409, "PublicationRecoveryAuthorityUnverified");
        }
        catch (Exception e) when (e is PersistenceIntegrityException or JsonException or FormatException or
            InvalidOperationException or KeyNotFoundException or ArgumentException or PublicationUpstreamException or
            OverflowException or Microsoft.Extensions.Options.OptionsValidationException)
        {
            throw new PublicationRecoveryException(409, "PublicationRecoveryIntegrityMismatch");
        }
        catch (Exception e) when (e is HttpRequestException || e is OperationCanceledException && !ct.IsCancellationRequested)
        {
            throw new PublicationRecoveryException(503, "PublicationRecoveryDependencyUnavailable");
        }
    }

    private void Log(PublicationRecoveryException e) =>
        logger.LogWarning(new EventId(8200, "PublicationRecoveryDenied"),
            "Publication recovery denied: {DiagnosticCode}.", e.Code);

    internal static void ValidateIds(string scenarioId, string runId)
    {
        if (!RequestValidation.IsCanonicalGuid(scenarioId, out _) || !RequestValidation.IsCanonicalGuid(runId, out _))
            throw new PublicationRecoveryException(400, "PublicationRecoveryIdentityInvalid");
    }

    internal static void ValidateRequest(RecoverPublicationRequest request)
    {
        if (!Label(request.Actor, 100) || !Label(request.Reason, 500) ||
            request.ReviewedPublicationHash is not { Length: 64 } ||
            request.ReviewedPublicationHash.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'f')))
            throw new PublicationRecoveryException(400, "PublicationRecoveryRequestInvalid");
    }

    private static bool Label(string? value, int max) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= max && value == value.Trim() &&
        value.All(c => c is >= ' ' and <= '~');
}

public sealed partial class AnalysisRevealClient
{
    internal async Task VerifyRecoveryUnpublishedAsync(PublicationStaging plan, CancellationToken ct)
    {
        using (HttpResponseMessage scenarioResponse = await client.GetAsync($"api/scenarios/{plan.ScenarioId}", ct))
        {
            RequireRecoveryRead(scenarioResponse);
            await scenarioResponse.Content.LoadIntoBufferAsync(1024 * 1024, ct);
            using JsonDocument document = JsonDocument.Parse(await scenarioResponse.Content.ReadAsStringAsync(ct));
            JsonElement scenario = document.RootElement;
            if (scenario.GetProperty("scenarioId").GetGuid().ToString("D") != plan.ScenarioId ||
                scenario.GetProperty("status").GetString() != "HumanApproved" ||
                scenario.GetProperty("asOfUtc").GetDateTimeOffset() != plan.ValidTimeUtc.AddDays(-1) ||
                scenario.GetProperty("initialAsOfUtc").GetDateTimeOffset() != plan.ValidTimeUtc.AddDays(-1) ||
                !scenario.TryGetProperty("clonedFieldId", out JsonElement clone) || clone.ValueKind != JsonValueKind.Null)
                throw new PublicationRecoveryException(409, "PublicationRecoveryRevealOrClockAdvanced");
        }
        using HttpResponseMessage receipt = await client.GetAsync(
            $"internal/scenarios/{plan.ScenarioId}/reveal/{plan.RevealId}/status", ct);
        if (receipt.StatusCode == HttpStatusCode.NotFound) return;
        RequireRecoveryRead(receipt);
        throw new PublicationRecoveryException(409, "PublicationRecoveryRevealAlreadyPrepared");
    }

    internal static void RequireRecoveryRead(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new PublicationRecoveryException(503, "PublicationRecoveryDependencyUnverified");
    }
}

public sealed partial class OntologyPublicationClient
{
    private sealed record RecoveryMarker(Guid EntityId, Guid ScenarioId, Guid RevealId, string State);

    internal async Task VerifyRecoveryStagingAsync(
        PublicationStaging plan, PublicationRecoveryOperation item, CancellationToken ct)
    {
        PublicationWriteOperation operation = item.Operation;
        HttpClient client = clients.CreateClient(operation.TargetService);
        string prefix = operation.Route[..(operation.Route.IndexOf("/api/", StringComparison.Ordinal) + 4)];
        using HttpResponseMessage marker = await client.GetAsync(
            $"{prefix}/internal/publication/records/{operation.EntityId}", ct);
        bool absent = marker.StatusCode == HttpStatusCode.NotFound;
        if (!absent)
        {
            AnalysisRevealClient.RequireRecoveryRead(marker);
            await marker.Content.LoadIntoBufferAsync(4096, ct);
            RecoveryMarker value = await marker.Content.ReadFromJsonAsync<RecoveryMarker>(CanonicalJson.SerializerOptions, ct)
                ?? throw new PublicationRecoveryException(409, "PublicationRecoveryMarkerInvalid");
            if (value.EntityId.ToString("D") != operation.EntityId || value.ScenarioId.ToString("D") != plan.ScenarioId ||
                value.RevealId.ToString("D") != plan.RevealId || value.State != "Staged")
                throw new PublicationRecoveryException(409, "PublicationRecoveryActivationOrOwnershipChanged");
        }
        else if (item.Status != "Pending")
            throw new PublicationRecoveryException(409, "PublicationRecoveryMarkerMissing");

        // Partially written entities are allowed, but only behind an owned, unactivated marker.
        using HttpResponseMessage read = await client.GetAsync(operation.ReadRoute, ct);
        if (read.StatusCode == HttpStatusCode.NotFound)
        {
            if (item.Status != "Verified") return;
            throw new PublicationRecoveryException(409, "PublicationRecoveryVerifiedRecordMissing");
        }
        AnalysisRevealClient.RequireRecoveryRead(read);
        if (absent) throw new PublicationRecoveryException(409, "PublicationRecoveryUnstagedEntity");
        await read.Content.LoadIntoBufferAsync(16 * 1024 * 1024, ct);
        using JsonDocument actual = JsonDocument.Parse(await read.Content.ReadAsStringAsync(ct));
        using JsonDocument expected = JsonDocument.Parse(operation.CanonicalPayloadJson);
        string business = PublicationJson.CanonicalBusinessContent(actual.RootElement);
        if (!PublicationJson.BusinessContentMatches(expected.RootElement, actual.RootElement) ||
            item.Status == "Verified" && (business != item.ResultBusinessJson || DeterministicIdentity.Sha256(business) != item.ResultHash))
            throw new PublicationRecoveryException(409, "PublicationRecoveryPersistedContentChanged");
    }
}
