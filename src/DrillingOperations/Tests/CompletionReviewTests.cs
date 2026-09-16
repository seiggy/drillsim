using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DrillingOperations.Tests;

[TestFixture, NonParallelizable]
public sealed class CompletionReviewTests
{
    [Test]
    public async Task ReviewRoute_RequiresInternalKey_ProjectsVerifiedOpenings_AndHasNoSideEffects()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        RunResponse run = await CreatePausedRunAsync(client);
        using HttpClient unauthenticated = factory.CreateClient();
        using HttpResponseMessage denied = await unauthenticated.GetAsync(ReviewRoute(run.RunId));
        Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        DrillingOperationsStore store = factory.Services.GetRequiredService<DrillingOperationsStore>();
        CompletionDesign design = (await store.GetCompletionDesignAsync(run.RunId))!;
        string beforeAudit = CanonicalJson.Serialize(await store.GetAuditAsync(run.ScenarioId));
        string beforeStages = CanonicalJson.Serialize(await store.GetStagesAsync(run.RunId));
        using HttpResponseMessage response = await client.GetAsync(ReviewRoute(run.RunId));
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync();
        CompletionReview review = JsonSerializer.Deserialize<CompletionReview>(json, CanonicalJson.SerializerOptions)!;
        Assert.Multiple(() =>
        {
            Assert.That(review.RunId, Is.EqualTo(Guid.Parse(run.RunId)));
            Assert.That(review.ScenarioId, Is.EqualTo(Guid.Parse(run.ScenarioId)));
            Assert.That(review.Status, Is.EqualTo("Draft"));
            Assert.That(review.OpeningsHash, Is.EqualTo(design.OpeningsHash));
            Assert.That(review.Openings, Is.EqualTo(CompletionReview.FromDesign(run, design).Openings));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
        });
        using JsonDocument document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.EnumerateObject().Select(x => x.Name),
            Is.EquivalentTo(new[] { "runId", "scenarioId", "status", "openingsHash", "openings" }));
        foreach (JsonElement opening in document.RootElement.GetProperty("openings").EnumerateArray())
            Assert.That(opening.EnumerateObject().Select(x => x.Name), Is.EquivalentTo(new[]
            {
                "reservoirName", "type", "topMdM", "baseMdM", "wellboreRadiusM", "skin", "efficiency", "uncertaintyM"
            }));
        foreach (string forbidden in new[]
        {
            "bindingMetadata", "worldId", "connection", "openingId", "pathBinding", "completionBinding",
            "grApi", "pressurePa", "truth", "logObservation", "canonical"
        })
            Assert.That(json, Does.Not.Contain(forbidden).IgnoreCase);
        Assert.That(CanonicalJson.Serialize(await store.GetAuditAsync(run.ScenarioId)), Is.EqualTo(beforeAudit));
        Assert.That(CanonicalJson.Serialize(await store.GetStagesAsync(run.RunId)), Is.EqualTo(beforeStages));
        Assert.That(await store.GetSideEffectCountsAsync(run.RunId), Is.EqualTo((0, 0)));
        Assert.Throws<PersistenceIntegrityException>(() =>
            CompletionReview.FromDesign(run with { ScenarioId = Guid.NewGuid().ToString("D") }, design));
    }

    [Test]
    public async Task ReviewedHash_IsAtomicAndIdempotent_AndApprovalResumesS6IntoS7()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        RunResponse run = await CreatePausedRunAsync(client);
        var store = factory.Services.GetRequiredService<DrillingOperationsStore>();
        CompletionReview review = (await client.GetFromJsonAsync<CompletionReview>(ReviewRoute(run.RunId), CanonicalJson.SerializerOptions))!;

        using HttpResponseMessage stale = await ApproveAsync(client, run.RunId, "review-wrong", TestData.HashA);
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That((await store.GetCompletionDesignAsync(run.RunId))!.Status, Is.EqualTo("Draft"));
        Assert.That((await store.GetRunAsync(run.RunId))!.Status, Is.EqualTo(RunStatus.AwaitingApproval));
        Assert.That(await store.GetProductionSeriesAsync(run.RunId), Is.Null);
        using HttpResponseMessage changedRequest = await ApproveAsync(client, run.RunId, "review-wrong", review.OpeningsHash);
        Assert.That(changedRequest.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        using HttpResponseMessage approved = await ApproveAsync(client, run.RunId, "review-exact", review.OpeningsHash);
        Assert.That(approved.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        string approvalBody = await approved.Content.ReadAsStringAsync();
        await WaitAsync(client, run.RunId, x => x.Status == RunStatus.ReadyToReveal && x.CurrentStage == RunStageKind.S7RunProduction);
        using HttpResponseMessage replay = await ApproveAsync(client, run.RunId, "review-exact", review.OpeningsHash);
        Assert.That(replay.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        Assert.That(await replay.Content.ReadAsStringAsync(), Is.EqualTo(approvalBody));
        using HttpResponseMessage changedReplay = await ApproveAsync(client, run.RunId, "review-exact", TestData.HashB);
        Assert.That(changedReplay.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using HttpResponseMessage wrongHashAfterApproval = await ApproveAsync(client, run.RunId, "review-other-hash", TestData.HashB);
        Assert.That(wrongHashAfterApproval.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using HttpResponseMessage otherActor = await ApproveAsync(client, run.RunId, "review-other-actor", review.OpeningsHash, "another-operator");
        Assert.That(otherActor.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using HttpResponseMessage sameActor = await ApproveAsync(client, run.RunId, "review-new-key", review.OpeningsHash);
        Assert.That(sameActor.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        CompletionDesign completed = (await store.GetCompletionDesignAsync(run.RunId))!;
        Assert.That(completed.Status, Is.EqualTo("Approved"));
        Assert.That(completed.OpeningsHash, Is.EqualTo(review.OpeningsHash));
        Assert.That(completed.ApprovedActor, Is.EqualTo("reviewing-operator"));
        Assert.That(await store.GetProductionSeriesAsync(run.RunId), Is.Not.Null);
        Assert.That((await store.GetAuditAsync(run.ScenarioId)).Count(x => x.Action == "completion.approved"), Is.EqualTo(1));
        Assert.That(await store.GetSideEffectCountsAsync(run.RunId), Is.EqualTo((0, 0)));
    }

    [Test]
    public async Task LegacyCanonicalApprovalRecords_ReplayWithoutAddingHashField()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        string runId = Guid.NewGuid().ToString("D");
        string route = $"{ReviewRoute(runId)}/approve";
        string canonical = CanonicalJson.Serialize(new { runId, actor = "legacy-operator" });
        var prior = new ApiOutcome(202, "{\"status\":\"Approved\"}");
        await fixture.Store.ExecuteIdempotentAsync(route, "old-approval", canonical, (_, _, _) => Task.FromResult(prior));
        ApiOutcome replay = await fixture.Store.ApproveCompletionAsync(route, "old-approval", runId, "legacy-operator");
        Assert.That(replay.Replayed, Is.True);
        Assert.That(replay.Body, Is.EqualTo(prior.Body));
        ApiOutcome changed = await fixture.Store.ApproveCompletionAsync(
            route, "old-approval", runId, "legacy-operator", reviewedOpeningsHash: TestData.HashA);
        Assert.That(changed.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task IntegrityFailureAndWrongCheckpoint_DoNotCreateApproval()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        RunResponse run = await CreatePausedRunAsync(client);
        var store = factory.Services.GetRequiredService<DrillingOperationsStore>();
        CompletionDesign design = (await store.GetCompletionDesignAsync(run.RunId))!;
        await using var db = new SqliteConnection($"Data Source={factory.DatabasePath}");
        await db.OpenAsync();
        await using (SqliteCommand command = db.CreateCommand())
        {
            command.CommandText = "UPDATE RunStages SET Status='Pending' WHERE RunId=$run AND Stage=$stage;";
            command.Parameters.AddWithValue("$run", run.RunId);
            command.Parameters.AddWithValue("$stage", (int)RunStageKind.S6DesignCompletion);
            await command.ExecuteNonQueryAsync();
        }
        using HttpResponseMessage wrongStage = await ApproveAsync(client, run.RunId, "wrong-stage", design.OpeningsHash);
        Assert.That(wrongStage.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That((await store.GetCompletionDesignAsync(run.RunId))!.ApprovedActor, Is.Null);
        await using (SqliteCommand command = db.CreateCommand())
        {
            command.CommandText = "DROP TRIGGER TR_CompletionDesigns_NoUpdate; UPDATE CompletionDesigns SET OpeningsHash=$hash WHERE RunId=$run;";
            command.Parameters.AddWithValue("$hash", TestData.HashA);
            command.Parameters.AddWithValue("$run", run.RunId);
            await command.ExecuteNonQueryAsync();
        }
        using HttpResponseMessage tampered = await client.GetAsync(ReviewRoute(run.RunId));
        Assert.That(tampered.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        await using SqliteCommand count = db.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM CompletionApprovals;";
        Assert.That(Convert.ToInt32(await count.ExecuteScalarAsync()), Is.Zero);
    }

    [TestCase("bad")]
    [TestCase("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public async Task MalformedReviewedHeader_IsRejected(string hash)
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        using HttpResponseMessage response = await ApproveAsync(client, Guid.NewGuid().ToString("D"), "invalid-hash", hash);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static string ReviewRoute(string runId) => $"/drillingoperations/api/runs/{runId}/completion";

    private static async Task<HttpResponseMessage> ApproveAsync(
        HttpClient client, string runId, string key, string hash, string actor = "reviewing-operator")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ReviewRoute(runId)}/approve");
        request.Headers.Add("Idempotency-Key", key);
        request.Headers.Add("X-DrillSim-Human-Actor", actor);
        request.Headers.TryAddWithoutValidation(CompletionReview.ReviewedHashHeaderName, hash);
        return await client.SendAsync(request);
    }

    private static async Task<RunResponse> CreatePausedRunAsync(HttpClient client)
    {
        using var bind = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/bind")
        { Content = JsonContent.Create(TestData.Binding()) };
        bind.Headers.Add("Idempotency-Key", "completion-review-bind");
        using HttpResponseMessage bound = await client.SendAsync(bind);
        bound.EnsureSuccessStatusCode();
        using var create = new HttpRequestMessage(HttpMethod.Post, "/drillingoperations/api/runs")
        { Content = JsonContent.Create(TestData.Run()) };
        create.Headers.Add("Idempotency-Key", "completion-review-run");
        using HttpResponseMessage response = await client.SendAsync(create);
        response.EnsureSuccessStatusCode();
        RunResponse run = (await response.Content.ReadFromJsonAsync<RunResponse>(CanonicalJson.SerializerOptions))!;
        return await WaitAsync(client, run.RunId, x => x.Status == RunStatus.AwaitingApproval && x.CurrentStage == RunStageKind.S6DesignCompletion);
    }

    private static async Task<RunResponse> WaitAsync(HttpClient client, string runId, Func<RunResponse, bool> predicate)
    {
        for (int attempt = 0; attempt < 300; attempt++)
        {
            RunResponse run = (await client.GetFromJsonAsync<RunResponse>($"/drillingoperations/api/runs/{runId}", CanonicalJson.SerializerOptions))!;
            if (predicate(run)) return run;
            await Task.Delay(25);
        }
        throw new AssertionException("Completion review run did not reach the expected checkpoint.");
    }
}
