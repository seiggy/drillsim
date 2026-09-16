using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace DrillingOperations.Tests;

[TestFixture, NonParallelizable]
public sealed class PublicationRecoveryTests
{
    [Test]
    public async Task RecoveryPersistsAcrossRestart_ReplaysAfterPublish_AndPreservesEveryStagedCheckpoint()
    {
        await using var f = await Fixture.CreateAsync();
        string immutableBefore = await f.CommitmentsAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        Assert.That(review.RecoveryEnabled, Is.True, review.Reason);
        Assert.That(review.VerifiedOperationCount, Is.EqualTo(9));
        Assert.That(review.PendingOperationCount, Is.EqualTo(review.OperationCount - 9));
        AssertSafe(JsonSerializer.Serialize(review));
        int writes = f.Upstream.WriteCount;
        using var response = await f.RecoverAsync(review, "recover-one");
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        Assert.That(await f.CommitmentsAsync(), Is.EqualTo(immutableBefore));
        Assert.That(f.Upstream.WriteCount, Is.EqualTo(writes));
        Assert.That(f.Upstream.PrepareCount + f.Upstream.ActivationCount + f.Upstream.FinalizeCount, Is.Zero);
        Assert.That((await f.Store.GetRunAsync(f.RunId))!.Status, Is.EqualTo(RunStatus.PublishFailed));
        Assert.That(await f.Store.GetSideEffectCountsAsync(f.RunId), Is.EqualTo((0, 0)));
        string audit = await f.ScalarAsync("SELECT DataJson FROM AuditEntries WHERE Action='publication.recovery-approved';");
        Assert.That(audit, Does.Contain("\"actor\":\"Local owner\"").And.Contain("\"reason\":\"Geology validation corrected\"")
            .And.Contain(review.ReviewedPublicationHash!).And.Contain("\"previousStatus\":\"Failed\"").And.Contain("\"status\":\"PublishFailed\""));
        Assert.That(await f.Store.VerifyAuditChainAsync(TestData.ScenarioId), Is.True);
        var restart = new DrillingOperationsStore(f.Store.ConnectionString, TimeProvider.System);
        await restart.InitializeAsync();
        Assert.That((await restart.GetRunAsync(f.RunId))!.Status, Is.EqualTo(RunStatus.PublishFailed));
        var coordinator = f.Coordinator(restart);
        ApiOutcome replay = await coordinator.RecoverAsync(f.RecoverRoute, "recover-one", TestData.ScenarioId, f.RunId,
            Request(review), CancellationToken.None);
        Assert.That(replay.Replayed, Is.True);
        Assert.That(replay.Body, Is.EqualTo(body));
        await f.Factory.Services.GetRequiredService<RunOrchestrator>().ResumeAllAsync();
        Assert.That(f.Upstream.WriteCount, Is.EqualTo(writes), "Recovery must not queue automatic publication.");
        PublicationStaging original = (await f.Store.GetPublicationAsync(f.RunId))!;
        f.Upstream.FailWriteSequence = null;
        using var published = await f.PublishAsync("explicit-new-publish");
        Assert.That(published.StatusCode, Is.EqualTo(HttpStatusCode.OK), await published.Content.ReadAsStringAsync());
        Assert.That((await f.Store.GetRunAsync(f.RunId))!.Status, Is.EqualTo(RunStatus.Revealed));
        PublicationStaging final = (await restart.GetPublicationAsync(f.RunId))!;
        Assert.That(final.PublicationPlanId, Is.EqualTo(original.PublicationPlanId));
        Assert.That(CanonicalJson.Serialize(final.Operations), Is.EqualTo(CanonicalJson.Serialize(original.Operations)));
        Assert.That(f.Upstream.WriteCount, Is.EqualTo(original.Operations.Count + 1));
        Assert.That(await restart.GetSideEffectCountsAsync(f.RunId), Is.EqualTo((1, 1)));
        Assert.That((await coordinator.RecoverAsync(f.RecoverRoute, "recover-one", TestData.ScenarioId, f.RunId,
            Request(review), CancellationToken.None)).Body, Is.EqualTo(body));
        using var oldPublishReplay = await f.PublishAsync("initial-rejected-write");
        Assert.That(oldPublishReplay.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), "Existing failed publish endpoint bytes must remain replayable.");
        Assert.That(f.Upstream.FinalizeCount, Is.EqualTo(1));
    }

    [Test]
    public async Task DuplicateKeysReplay_ChangedBodyAndStaleReviewRejectWithoutMutations()
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        using var stale = await f.RecoverAsync(review with { ReviewedPublicationHash = TestData.HashD }, "stale");
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var consumed = await f.RecoverAsync(review, "stale");
        Assert.That(consumed.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), "A rejected key cannot be repurposed.");
        using var recovered = await f.RecoverAsync(review, "accepted");
        string body = await recovered.Content.ReadAsStringAsync();
        using var replay = await f.RecoverAsync(review, "accepted");
        Assert.That(await replay.Content.ReadAsStringAsync(), Is.EqualTo(body));
        using var changed = await f.PostAsync(f.RecoverRoute, Request(review) with { Reason = "Different reason" }, "accepted");
        Assert.That(changed.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var newAttempt = await f.RecoverAsync(review, "new-after-recovery");
        Assert.That(newAttempt.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM AuditEntries WHERE Action='publication.recovery-approved';"), Is.EqualTo("1"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ConcurrentRecoveryIsAtomic(bool sameKey)
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        var results = await Task.WhenAll(Enumerable.Range(0, 2).Select(i => Task.Run(async () =>
        {
            using var response = await f.RecoverAsync(review, sameKey ? "concurrent" : $"concurrent-{i}");
            return (response.StatusCode, Body: await response.Content.ReadAsStringAsync());
        })));
        Assert.That(results.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(sameKey ? 2 : 1));
        if (sameKey) Assert.That(results[0].Body, Is.EqualTo(results[1].Body));
        else Assert.That(results.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM AuditEntries WHERE Action='publication.recovery-approved';"), Is.EqualTo("1"));
        Assert.That(await f.Store.GetSideEffectCountsAsync(f.RunId), Is.EqualTo((0, 0)));
    }

    [TestCase("UPDATE Runs SET CurrentStage=7;")]
    [TestCase("UPDATE Runs SET Status='Cancelled';")]
    [TestCase("UPDATE Runs SET DiagnosticCode='RevealCallbackMismatch';")]
    [TestCase("UPDATE Runs SET PublicationCount=1;")]
    [TestCase("UPDATE Runs SET ClockAdvanceCount=1;")]
    [TestCase("UPDATE Runs SET BindingId='changed-binding';")]
    [TestCase("UPDATE RunStages SET Status='Pending' WHERE Stage=2;")]
    [TestCase("UPDATE RunStages SET OutputHash='corrupt' WHERE Stage=5;")]
    [TestCase("UPDATE RunStages SET InputHash='corrupt' WHERE Stage=7;")]
    [TestCase("UPDATE RunStages SET InputHash='corrupt' WHERE Stage=8;")]
    [TestCase("UPDATE RunStages SET Status='Completed' WHERE Stage=9;")]
    [TestCase("UPDATE PublicationStates SET PreparedReceiptJson='{}',PreparedReceiptHash='corrupt';")]
    [TestCase("UPDATE PublicationStates SET FinalReceiptJson='{}',FinalReceiptHash='corrupt';")]
    [TestCase("UPDATE PublicationStates SET FinalManifestJson='{}',FinalManifestHash='corrupt';")]
    [TestCase("UPDATE PublicationStates SET VerifiedOperationCount=0;")]
    [TestCase("UPDATE PublicationOperationStates SET ActivationStatus='Activated',ActivationAttemptCount=1 WHERE Status='Verified';")]
    [TestCase("UPDATE PublicationOperationStates SET ActivationAttemptCount=1 WHERE Status='Verified';")]
    [TestCase("UPDATE PublicationOperationStates SET ResultHash='corrupt' WHERE Status='Verified';")]
    [TestCase("DROP TRIGGER TR_PublicationOperations_NoUpdate; UPDATE PublicationOperations SET PayloadHash='corrupt' WHERE Sequence=1;")]
    [TestCase("DROP TRIGGER TR_PublicationOperations_NoUpdate; UPDATE PublicationOperations SET ReadRoute='/foreign/route' WHERE Sequence=1;")]
    [TestCase("DROP TRIGGER TR_PublicationPlans_NoUpdate; UPDATE PublicationPlans SET ManifestHash='corrupt';")]
    [TestCase("DROP TRIGGER TR_MaterializedPlans_NoUpdate; UPDATE MaterializedPlans SET PathHash='corrupt';")]
    [TestCase("DROP TRIGGER TR_ProductionSeries_NoUpdate; UPDATE ProductionSeries SET OutputHash='corrupt';")]
    public async Task InvalidPersistedGuardFailsClosed(string corruption)
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        Assert.That(review.RecoveryEnabled, Is.True, review.Reason);
        await f.ExecuteAsync(corruption);
        string before = await f.CommitmentsAsync();
        using var response = await f.RecoverAsync(review, "guard-corrupted");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await response.Content.ReadAsStringAsync());
        Assert.That(await f.CommitmentsAsync(), Is.EqualTo(before));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM AuditEntries WHERE Action='publication.recovery-approved';"), Is.EqualTo("0"));
        Assert.That(f.Upstream.PrepareCount + f.Upstream.ActivationCount + f.Upstream.FinalizeCount, Is.Zero);
    }

    [Test]
    public async Task GuardRechecksConcurrentChangesAfterExternalReads()
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        bool changed = false;
        f.Upstream.RecoveryReadOverride = request =>
        {
            if (!changed && request.RequestUri!.AbsolutePath.Contains("/internal/publication/records/", StringComparison.Ordinal))
            {
                changed = true;
                f.ExecuteAsync("UPDATE PublicationOperationStates SET AttemptCount=AttemptCount+1 WHERE Status='Failed';").GetAwaiter().GetResult();
            }

            return null;
        };
        using var response = await f.RecoverAsync(review, "guard-race");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("PublicationRecoveryReviewStale"));
        Assert.That((await f.Store.GetRunAsync(f.RunId))!.Status, Is.EqualTo(RunStatus.Failed));
    }

    [Test]
    public async Task FailedOperationWithMatchingStagedEntity_IsIntentionallyRecoverable()
    {
        await using var f = await Fixture.CreateAsync();
        PublicationStaging plan = (await f.Store.GetPublicationAsync(f.RunId))!;
        PublicationWriteOperation failed = (await f.Store.GetIncompletePublicationOperationsAsync(f.RunId))[0];
        f.Upstream.FailWriteSequence = null;
        _ = await f.Factory.Services.GetRequiredService<OntologyPublicationClient>()
            .WriteAndVerifyAsync(failed, plan.ScenarioId, plan.RevealId, plan.ProductionEntityId, CancellationToken.None);
        string before = await f.CommitmentsAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        Assert.That(review.RecoveryEnabled, Is.True, review.Reason);
        using var response = await f.RecoverAsync(review, "partially-written");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await f.CommitmentsAsync(), Is.EqualTo(before));
        Assert.That(f.Upstream.ActivationCount, Is.Zero);
        using var publish = await f.PublishAsync("explicit-after-partial-recovery");
        Assert.That(publish.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(f.Upstream.ExistingWriteConflictCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ExplicitPublishWaitsForRecoveryCommit_AndDoesNotRaceExternalChecks()
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int checks = 0;
        f.Upstream.RecoveryReadOverride = request =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("/internal/publication/records/", StringComparison.Ordinal) &&
                Interlocked.Increment(ref checks) == 1)
            {
                entered.SetResult();
                release.Task.GetAwaiter().GetResult();
            }
            return null;
        };
        f.Upstream.FailWriteSequence = null;
        Task<HttpResponseMessage> recovery = Task.Run(() => f.RecoverAsync(review, "serialized-recovery"));
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Task<HttpResponseMessage> publish = Task.Run(() => f.PublishAsync("separate-explicit-concurrent-publish"));
        try
        {
            await Task.Delay(100);
            Assert.That(publish.IsCompleted, Is.False);
            Assert.That(f.Upstream.WriteCount, Is.EqualTo(10));
        }
        finally { release.TrySetResult(); }
        using var recovered = await recovery;
        using var published = await publish;
        Assert.That(recovered.StatusCode, Is.EqualTo(HttpStatusCode.OK), await recovered.Content.ReadAsStringAsync());
        Assert.That(published.StatusCode, Is.EqualTo(HttpStatusCode.OK), await published.Content.ReadAsStringAsync());
        Assert.That(await f.Store.GetSideEffectCountsAsync(f.RunId), Is.EqualTo((1, 1)));
    }

    [Test]
    public async Task MissingVerifiedRecordIsBlockedEligibility_NotATransientDependencyFailure()
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview before = await f.ReviewAsync();
        string commitments = await f.CommitmentsAsync();
        f.Upstream.RemovePublishedField();
        PublicationRecoveryReview review = await f.ReviewAsync();
        Assert.That(review.RecoveryEnabled, Is.False);
        Assert.That(review.Reason, Is.EqualTo("PublicationRecoveryVerifiedRecordMissing"));
        Assert.That(review.ReviewedPublicationHash, Is.Null);
        using var response = await f.RecoverAsync(before, "missing-verified-record");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("PublicationRecoveryVerifiedRecordMissing"));
        Assert.That(await f.CommitmentsAsync(), Is.EqualTo(commitments));
        Assert.That((await f.Store.GetRunAsync(f.RunId))!.Status, Is.EqualTo(RunStatus.Failed));
        Assert.That(f.Upstream.WriteCount, Is.EqualTo(10));
        Assert.That(f.Upstream.ActivationCount, Is.Zero);
    }

    [TestCase("prepared")]
    [TestCase("clock")]
    [TestCase("activated")]
    [TestCase("foreign-marker")]
    [TestCase("unavailable")]
    [TestCase("changed-content")]
    public async Task ExternalReceiptsOwnershipAndClockAreChecked(string invalid)
    {
        await using var f = await Fixture.CreateAsync();
        PublicationRecoveryReview review = await f.ReviewAsync();
        f.Upstream.RecoveryReadOverride = request =>
        {
            string path = request.RequestUri!.AbsolutePath;
            if (invalid == "prepared" && path.EndsWith("/status", StringComparison.Ordinal))
                return TestData.Json(HttpStatusCode.OK, new { status = "Prepared" });
            if (invalid == "clock" && path == $"/api/scenarios/{TestData.ScenarioId}")
                return TestData.Json(HttpStatusCode.OK, new { scenarioId = TestData.ScenarioId, status = "HumanApproved",
                    initialAsOfUtc = "2025-01-01T00:00:00Z", asOfUtc = "2025-01-02T00:00:00Z", clonedFieldId = (Guid?)null });
            if (path.Contains("/internal/publication/records/", StringComparison.Ordinal))
            {
                if (invalid == "unavailable") return new(HttpStatusCode.ServiceUnavailable);
                if (invalid is "activated" or "foreign-marker")
                    return TestData.Json(HttpStatusCode.OK, new { entityId = Guid.Parse(path.Split('/').Last()),
                        scenarioId = invalid == "activated" ? Guid.Parse(TestData.ScenarioId) : Guid.NewGuid(),
                        revealId = (f.Store.GetPublicationAsync(f.RunId).GetAwaiter().GetResult())!.RevealId,
                        state = invalid == "activated" ? "Activated" : "Staged" });
            }
            if (invalid == "changed-content" && path.StartsWith("/field/api/Field/", StringComparison.Ordinal))
                return TestData.Json(HttpStatusCode.OK, new { });
            return null;
        };
        using var response = await f.RecoverAsync(review, "external-guard");
        Assert.That(response.StatusCode, Is.EqualTo(invalid == "unavailable" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Conflict));
        Assert.That((await f.Store.GetRunAsync(f.RunId))!.Status, Is.EqualTo(RunStatus.Failed));
        AssertSafe(await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task ActualRoutesRequireInternalKeyBoundedBodyAndOwnedRun()
    {
        await using var f = await Fixture.CreateAsync();
        using var anonymous = f.Factory.CreateClient();
        using var get = await anonymous.GetAsync(f.ReviewRoute);
        using var post = await anonymous.PostAsJsonAsync(f.RecoverRoute, new { });
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(post.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        PublicationRecoveryReview review = await f.ReviewAsync();
        using var wrongScenario = await f.Client.GetAsync(f.ReviewRoute.Replace(TestData.ScenarioId, Guid.NewGuid().ToString("D")));
        Assert.That(wrongScenario.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        using var extra = await f.PostAsync(f.RecoverRoute, new { actor = "Owner", reason = "Fix verified", reviewedPublicationHash = review.ReviewedPublicationHash, worldId = "forbidden" }, "extra");
        Assert.That(extra.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using var longActor = await f.PostAsync(f.RecoverRoute, Request(review) with { Actor = new string('a', 101) }, "actor");
        using var longReason = await f.PostAsync(f.RecoverRoute, Request(review) with { Reason = new string('a', 501) }, "reason");
        using var large = await f.PostAsync(f.RecoverRoute, Request(review) with { Reason = new string('a', 5000) }, "large");
        using var missingKey = await f.PostAsync(f.RecoverRoute, Request(review), "");
        Assert.That(new[] { longActor.StatusCode, longReason.StatusCode, large.StatusCode, missingKey.StatusCode },
            Is.All.EqualTo(HttpStatusCode.BadRequest));
    }

    private static RecoverPublicationRequest Request(PublicationRecoveryReview review) =>
        new("Local owner", "Geology validation corrected", review.ReviewedPublicationHash!);

    private static void AssertSafe(string json)
    {
        foreach (string forbidden in new[] { "worldId", "world-opaque", "canonicalPayloadJson", "truthSamples", "Internal-Key",
            "calibration-opaque", "productionTruth", "completionBindingId", "sourceFieldId", "clonedFieldId" })
            Assert.That(json, Does.Not.Contain(forbidden).IgnoreCase);
    }

    private sealed class Fixture(ApiFactory factory, HttpClient client, PublicationFakeHandler upstream, string runId) : IAsyncDisposable
    {
        internal ApiFactory Factory => factory;
        internal HttpClient Client => client;
        internal PublicationFakeHandler Upstream => upstream;
        internal string RunId => runId;
        internal DrillingOperationsStore Store => factory.Services.GetRequiredService<DrillingOperationsStore>();
        internal string ReviewRoute => $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/runs/{runId}/publication-recovery";
        internal string RecoverRoute => $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/runs/{runId}/recover-publication";
        internal static async Task<Fixture> CreateAsync()
        {
            var upstream = new PublicationFakeHandler { FailWriteSequence = 10, WriteFailureStatus = HttpStatusCode.BadRequest };
            var factory = new ApiFactory(publication: upstream);
            HttpClient client = factory.CreateInternalClient();
            RunResponse run = await PublicationWorkflowTests.ReadyRun(client);
            var f = new Fixture(factory, client, upstream, run.RunId);
            using var response = await f.PublishAsync("initial-rejected-write");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That((await f.Store.GetRunAsync(run.RunId))!.Status, Is.EqualTo(RunStatus.Failed));
            return f;
        }
        internal async Task<PublicationRecoveryReview> ReviewAsync()
        {
            using var response = await client.GetAsync(ReviewRoute);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            return (await response.Content.ReadFromJsonAsync<PublicationRecoveryReview>())!;
        }
        internal Task<HttpResponseMessage> RecoverAsync(PublicationRecoveryReview review, string key) =>
            PostAsync(RecoverRoute, Request(review), key);
        internal Task<HttpResponseMessage> PublishAsync(string key) => PostAsync($"/drillingoperations/api/runs/{runId}/publish", null, key);
        internal async Task<HttpResponseMessage> PostAsync(string route, object? body, string key)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, route);
            if (key != "") request.Headers.Add("Idempotency-Key", key);
            if (body is not null) request.Content = JsonContent.Create(body);
            return await client.SendAsync(request);
        }
        internal PublicationRecoveryCoordinator Coordinator(DrillingOperationsStore store) => new(store,
            factory.Services.GetRequiredService<AnalysisVerificationClient>(),
            factory.Services.GetRequiredService<AnalysisRevealClient>(),
            factory.Services.GetRequiredService<OntologyPublicationClient>(),
            NullLogger<PublicationRecoveryCoordinator>.Instance);
        internal async Task ExecuteAsync(string sql)
        {
            await using var c = new SqliteConnection(Store.ConnectionString); await c.OpenAsync();
            await using var q = c.CreateCommand(); q.CommandText = "PRAGMA foreign_keys=OFF;" + sql; await q.ExecuteNonQueryAsync();
        }
        internal async Task<string> ScalarAsync(string sql)
        {
            await using var c = new SqliteConnection(Store.ConnectionString); await c.OpenAsync();
            await using var q = c.CreateCommand(); q.CommandText = sql;
            return Convert.ToString(await q.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture)!;
        }
        internal async Task<string> CommitmentsAsync()
        {
            var tables = new[] { "RunStages", "PublicationPlans", "PublicationOperations", "PublicationOperationStates",
                "PublicationStates", "MaterializedPlans", "DrillingExecutions", "SurveyArtifacts", "TruthBindings",
                "TruthSampleBatches", "ObservationBatches", "CompletionDesigns", "CompletionApprovals", "ProductionSeries" };
            var rows = new List<object?[]>();
            await using var c = new SqliteConnection(Store.ConnectionString); await c.OpenAsync();
            foreach (string table in tables)
            {
                await using var q = c.CreateCommand(); q.CommandText = $"SELECT * FROM {table} ORDER BY rowid;";
                await using var r = await q.ExecuteReaderAsync();
                while (await r.ReadAsync()) rows.Add(Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? null : r.GetValue(i)).ToArray());
            }
            return DeterministicIdentity.Sha256(JsonSerializer.Serialize(rows));
        }
        public async ValueTask DisposeAsync() { client.Dispose(); await factory.DisposeAsync(); }
    }
}
