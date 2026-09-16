using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace DrillingOperations.Tests;

[TestFixture, NonParallelizable]
public sealed class ScoringCorrectionTests
{
    [Test]
    public async Task CorrectionIsAppendOnly_Restartable_AndPublishesOnlyOnSeparateScoreAction()
    {
        await using var f = await Fixture.CreateAsync();
        string before = await f.PreservedHashAsync();
        using var oldResponse = await f.Client.GetAsync(f.HistoryRoute(f.Original.ScorecardId));
        string oldBody = await oldResponse.Content.ReadAsStringAsync();
        var review = await f.ReviewAsync();
        Assert.That(review.CorrectionEnabled, Is.True, review.Reason);
        Assert.That(review.InvalidMetricCount, Is.GreaterThan(0));
        Assert.That(review.CorrectedScorecardId, Is.Not.EqualTo(f.Original.ScorecardId));
        AssertSafe(CanonicalJson.Serialize(review));
        using var corrected = await f.CorrectAsync(review, "correction-approved");
        string body = await corrected.Content.ReadAsStringAsync();
        Assert.That(corrected.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        var result = JsonSerializer.Deserialize<ScoringCorrectionResult>(body, CanonicalJson.SerializerOptions)!;
        Assert.That(result.CorrectedScorecardSha256, Is.EqualTo(review.CorrectedScorecardSha256));
        Assert.That(result.ScoringInputSha256, Is.EqualTo(f.Original.InputSha256));
        Assert.That(await f.PreservedHashAsync(), Is.EqualTo(before));
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(1));
        Assert.That((await f.Store.GetRunAsync(f.Run.RunId))!.Status, Is.EqualTo(RunStatus.AwaitingDependency));
        Assert.That((await f.Store.GetStagesAsync(f.Run.RunId)).Single(x => x.Stage == RunStageKind.S9Score).Status,
            Is.EqualTo(StageStatus.AwaitingDependency));
        var restart = new DrillingOperationsStore(f.Store.ConnectionString, TimeProvider.System);
        await restart.InitializeAsync();
        ScorecardDraft active = (await restart.GetScoringCheckpointAsync(f.Run.RunId))!;
        ScorecardDraft rejected = (await restart.GetScorecardArtifactAsync(f.Run.RunId, f.Original.ScorecardId))!;
        Assert.That(CanonicalJson.Serialize(rejected), Is.EqualTo(CanonicalJson.Serialize(f.Original)));
        Assert.That(active.ScoringModelVersion, Is.EqualTo(ScoreCorrection.ModelVersion));
        Assert.That(active.InputSha256, Is.EqualTo(review.CorrectedInputSha256));
        var request = JsonSerializer.Deserialize<AnalysisScorecardRequest>(active.CanonicalOutputJson, CanonicalJson.SerializerOptions)!;
        Assert.That(request.Correction, Is.EqualTo(new ScoreCorrectionProvenance(ScoreCorrection.Version,
            f.Original.ScoringModelVersion, f.Original.ScorecardId, f.Original.OutputSha256, f.Original.InputSha256)));
        for (int i = 0; i < active.Metrics.Count; i++)
            Assert.That(CanonicalJson.Serialize(active.Metrics[i] with { LowerBound = rejected.Metrics[i].LowerBound }),
                Is.EqualTo(CanonicalJson.Serialize(rejected.Metrics[i])));
        Assert.DoesNotThrow(() => ScoreMetricContract.ValidateForPublication(active.Metrics));
        var restartedCoordinator = f.Coordinator(restart);
        ApiOutcome replay = await restartedCoordinator.CorrectAsync(f.CorrectRoute, "correction-approved",
            TestData.ScenarioId, f.Run.RunId, Request(review), CancellationToken.None);
        Assert.That(replay.Replayed, Is.True);
        Assert.That(replay.Body, Is.EqualTo(body));
        await f.Factory.Services.GetRequiredService<RunOrchestrator>().ResumeAllAsync();
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(1));
        f.Upstream.ScorecardMismatch = false;
        f.Prediction.CorruptPrediction = true;
        f.Prediction.CorruptBaseline = true;
        using var score = await f.ScoreAsync("explicit-corrected-score");
        string scoreBody = await score.Content.ReadAsStringAsync();
        Assert.That(score.StatusCode, Is.EqualTo(HttpStatusCode.OK), scoreBody);
        var scored = JsonSerializer.Deserialize<ScorecardSummary>(scoreBody, CanonicalJson.SerializerOptions)!;
        Assert.That(scored.ScorecardId, Is.EqualTo(active.ScorecardId));
        Assert.That(scored.Correction, Is.EqualTo(request.Correction));
        Assert.That((await restart.GetRunAsync(f.Run.RunId))!.Status, Is.EqualTo(RunStatus.Scored));
        Assert.That(await restart.GetSideEffectCountsAsync(f.Run.RunId), Is.EqualTo((1, 1)));
        Assert.That(await f.PreservedHashAsync(), Is.EqualTo(before));
        Assert.That(await f.Client.GetStringAsync(f.HistoryRoute(rejected.ScorecardId)), Is.EqualTo(oldBody));
        Assert.That(f.Upstream.PrepareCount, Is.EqualTo(1));
        Assert.That(f.Upstream.FinalizeCount, Is.EqualTo(1));
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(2));
        using var scoreReplay = await f.ScoreAsync("explicit-corrected-score");
        Assert.That(await scoreReplay.Content.ReadAsStringAsync(), Is.EqualTo(scoreBody));
        using var correctionReplay = await f.CorrectAsync(review, "correction-approved");
        Assert.That(await correctionReplay.Content.ReadAsStringAsync(), Is.EqualTo(body));
        Assert.That(await f.Store.VerifyAuditChainAsync(TestData.ScenarioId), Is.True);
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM AuditEntries WHERE Action='scorecard.correction-approved';"), Is.EqualTo("1"));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardReceipts;"), Is.EqualTo("0"));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrectionReceipts;"), Is.EqualTo("1"));
        Assert.ThrowsAsync<SqliteException>(() => f.MutateAsync("UPDATE ScorecardCorrections SET OutputSha256='changed';"));
        Assert.ThrowsAsync<SqliteException>(() => f.MutateAsync("DELETE FROM ScorecardCorrections;"));
        Assert.ThrowsAsync<SqliteException>(() => f.MutateAsync("UPDATE ScorecardCorrectionReceipts SET ReceiptHash='changed';"));
    }

    [Test]
    public async Task CoherentButDifferentPointScoreIsNotSilentlyCorrected()
    {
        await using var f = await Fixture.CreateAsync();
        var metrics = f.Original.Metrics.Select((x, i) => i == 0 ? x with { Value = x.Value + 1 } : x).ToArray();
        var body = JsonSerializer.Deserialize<AnalysisScorecardRequest>(f.Original.CanonicalOutputJson, CanonicalJson.SerializerOptions)! with { Metrics = metrics };
        string output = CanonicalJson.Serialize(body);
        await SeedHistoricalOutputAsync(f.Factory.DatabasePath, f.Original with
        {
            Metrics = metrics, CanonicalOutputJson = output, OutputSha256 = DeterministicIdentity.Sha256(output)
        });
        var review = await f.ReviewAsync();
        Assert.That(review.CorrectionEnabled, Is.False);
        Assert.That(review.Reason, Is.EqualTo("ScoringCorrectionOriginalNotReproduced"));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrections;"), Is.EqualTo("0"));
    }

    [Test]
    public async Task RehashedCorrectionTamperingIsRejectedByItsImmutableApprovalCommitment()
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        using var corrected = await f.CorrectAsync(review, "approve-tamper-test");
        Assert.That(corrected.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ScorecardDraft card = (await f.Store.GetScoringCheckpointAsync(f.Run.RunId))!;
        var request = JsonSerializer.Deserialize<AnalysisScorecardRequest>(card.CanonicalOutputJson, CanonicalJson.SerializerOptions)!;
        int index = request.Metrics.Select((x, i) => (Metric: x, Index: i))
            .First(x => x.Metric.LowerBound == 0 && f.Original.Metrics[x.Index].LowerBound > 0 &&
                f.Original.Metrics[x.Index].Value >= f.Original.Metrics[x.Index].LowerBound).Index;
        var metrics = request.Metrics.Select((x, i) => i == index ? x with { LowerBound = f.Original.Metrics[i].LowerBound } : x).ToArray();
        string json = CanonicalJson.Serialize(request with { Metrics = metrics });
        await using var c = new SqliteConnection(f.Store.ConnectionString); await c.OpenAsync();
        await using var q = c.CreateCommand();
        q.CommandText = "DROP TRIGGER TR_ScorecardCorrections_NoUpdate;UPDATE ScorecardCorrections SET CanonicalOutputJson=$json,OutputSha256=$hash;";
        q.Parameters.AddWithValue("$json", json); q.Parameters.AddWithValue("$hash", DeterministicIdentity.Sha256(json));
        await q.ExecuteNonQueryAsync();
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () => { _ = await f.Store.GetScoringCheckpointAsync(f.Run.RunId); });
        Assert.That((await f.Store.GetScorecardArtifactAsync(f.Run.RunId, f.Original.ScorecardId))!.CanonicalOutputJson,
            Is.EqualTo(f.Original.CanonicalOutputJson));
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ConcurrentCorrectionCommitsOneArtifactAndAudit(bool sameKey)
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        var results = await Task.WhenAll(Enumerable.Range(0, 2).Select(i => Task.Run(async () =>
        {
            using var response = await f.CorrectAsync(review, sameKey ? "race" : $"race-{i}");
            return (response.StatusCode, Body: await response.Content.ReadAsStringAsync());
        })));
        Assert.That(results.Count(x => x.StatusCode == HttpStatusCode.OK), Is.EqualTo(sameKey ? 2 : 1));
        if (sameKey) Assert.That(results[0].Body, Is.EqualTo(results[1].Body));
        else Assert.That(results.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrections;"), Is.EqualTo("1"));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM AuditEntries WHERE Action='scorecard.correction-approved';"), Is.EqualTo("1"));
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(1));
    }

    [Test]
    public async Task StaleAndMismatchedRequestKeysDoNotChangeArtifacts()
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        using var stale = await f.CorrectAsync(review with { ReviewedCorrectionHash = TestData.HashD }, "stale");
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var reuse = await f.CorrectAsync(review, "stale");
        Assert.That(reuse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var approved = await f.CorrectAsync(review, "approved");
        Assert.That(approved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var changed = await f.PostAsync(f.CorrectRoute, Request(review) with { Reason = "Changed reason" }, "approved");
        Assert.That(changed.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var second = await f.CorrectAsync(review, "different-correction");
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using var oldScoreKey = await f.ScoreAsync("initial-rejected-score");
        Assert.That(oldScoreKey.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(1));
    }

    [TestCase("UPDATE Runs SET Status='Revealed';")]
    [TestCase("UPDATE Runs SET CurrentStage=8;")]
    [TestCase("UPDATE Runs SET DiagnosticCode='ScorecardReceiptMismatch';")]
    [TestCase("UPDATE Runs SET PublicationCount=2;")]
    [TestCase("UPDATE Runs SET ClockAdvanceCount=0;")]
    [TestCase("UPDATE RunStages SET OutputHash='corrupt' WHERE Stage=2;")]
    [TestCase("UPDATE RunStages SET Status='Pending' WHERE Stage=8;")]
    [TestCase("UPDATE RunStages SET InputHash='corrupt' WHERE Stage=9;")]
    [TestCase("UPDATE ScorecardDeliveries SET Diagnostic='OtherFailure';")]
    [TestCase("DROP TRIGGER TR_Scorecards_NoUpdate; UPDATE Scorecards SET InputSha256='corrupt';")]
    [TestCase("DROP TRIGGER TR_Scorecards_NoUpdate; UPDATE Scorecards SET OutputSha256='corrupt';")]
    [TestCase("DROP TRIGGER TR_ScorecardMetrics_NoUpdate; UPDATE ScorecardMetrics SET Value=123456 WHERE Sequence=0;")]
    [TestCase("UPDATE PublicationStates SET FinalReceiptHash='corrupt';")]
    [TestCase("UPDATE PublicationOperationStates SET ResultHash='corrupt' WHERE OperationId=(SELECT OperationId FROM PublicationOperations ORDER BY Sequence LIMIT 1);")]
    [TestCase("DROP TRIGGER TR_ProductionSeries_NoUpdate; UPDATE ProductionSeries SET OutputHash='corrupt';")]
    [TestCase("DROP TRIGGER TR_TruthBindings_NoUpdate; UPDATE TruthBindings SET InputHash='corrupt';")]
    [TestCase("INSERT INTO ScorecardReceipts SELECT ScorecardId,'{}','corrupt','2026-01-01T00:00:00Z' FROM Scorecards;")]
    public async Task InvalidGuardFailsClosed(string sql)
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        Assert.That(review.CorrectionEnabled, Is.True, review.Reason);
        await f.MutateAsync(sql);
        string before = await f.PreservedHashAsync();
        using var response = await f.CorrectAsync(review, "invalid-guard");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict), await response.Content.ReadAsStringAsync());
        Assert.That(await f.PreservedHashAsync(), Is.EqualTo(before));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrections;"), Is.EqualTo("0"));
        Assert.That(f.Upstream.ScorecardCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ChangedImmutableBaselineIsNotRepaired()
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        f.Prediction.CorruptBaseline = true;
        using var response = await f.CorrectAsync(review, "baseline-changed");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrections;"), Is.EqualTo("0"));
    }

    [TestCase("score-published")]
    [TestCase("receipt-changed")]
    [TestCase("unavailable")]
    public async Task ExternalPublicationStateIsRechecked(string invalid)
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        f.Upstream.RecoveryReadOverride = request =>
        {
            string path = request.RequestUri!.AbsolutePath;
            if (invalid == "score-published" && path.EndsWith("/scorecard", StringComparison.Ordinal))
                return TestData.Json(HttpStatusCode.OK, new { status = "Scored" });
            if (invalid == "receipt-changed" && path.EndsWith("/status", StringComparison.Ordinal))
                return TestData.Json(HttpStatusCode.OK, new AnalysisRevealReceipt(Guid.Parse(TestData.ScenarioId),
                    Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "Revealed", TestData.HashA, 1, Guid.NewGuid()));
            if (invalid == "unavailable") return new(HttpStatusCode.ServiceUnavailable);
            return null;
        };
        using var response = await f.CorrectAsync(review, "external-state");
        Assert.That(response.StatusCode, Is.EqualTo(invalid == "unavailable" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Conflict));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrections;"), Is.EqualTo("0"));
    }

    [Test]
    public async Task ConcurrentGuardChangeBetweenExternalChecksAndCommitRejects()
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        bool changed = false;
        f.Upstream.RecoveryReadOverride = request =>
        {
            if (!changed)
            {
                changed = true;
                f.MutateAsync("UPDATE ScorecardDeliveries SET AttemptCount=AttemptCount+1;").GetAwaiter().GetResult();
            }
            return null;
        };
        using var response = await f.CorrectAsync(review, "guard-race");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("ScoringCorrectionReviewStale"));
        Assert.That(await f.ScalarAsync("SELECT COUNT(*) FROM ScorecardCorrections;"), Is.EqualTo("0"));
    }

    [Test]
    public async Task CorrectedDeliveryOutageRetriesOnlyTheSameCorrection()
    {
        await using var f = await Fixture.CreateAsync();
        var review = await f.ReviewAsync();
        using var correction = await f.CorrectAsync(review, "approve");
        Assert.That(correction.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        f.Upstream.ScorecardUnavailable = true;
        using var failed = await f.ScoreAsync("unavailable");
        Assert.That(failed.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That((await f.Store.GetRunAsync(f.Run.RunId))!.Status, Is.EqualTo(RunStatus.AwaitingDependency));
        string prior = CanonicalJson.Serialize((await f.Store.GetScoringCheckpointAsync(f.Run.RunId))!);
        f.Upstream.ScorecardUnavailable = false;
        using var scored = await f.ScoreAsync("explicit-retry");
        Assert.That(scored.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(CanonicalJson.Serialize((await f.Store.GetScoringCheckpointAsync(f.Run.RunId))!), Is.EqualTo(prior));
        Assert.That(await f.Store.GetSideEffectCountsAsync(f.Run.RunId), Is.EqualTo((1, 1)));
    }

    [Test]
    public async Task RoutesRequirePrivateKeyOwnedScopeAndStrictReviewedBody()
    {
        await using var f = await Fixture.CreateAsync();
        using var anonymous = f.Factory.CreateClient();
        using var read = await anonymous.GetAsync(f.ReviewRoute);
        using var history = await anonymous.GetAsync(f.HistoryRoute(f.Original.ScorecardId));
        using var mutation = await anonymous.PostAsJsonAsync(f.CorrectRoute, new { });
        Assert.That(new[] { read.StatusCode, history.StatusCode, mutation.StatusCode }, Is.All.EqualTo(HttpStatusCode.Unauthorized));
        var review = await f.ReviewAsync();
        using var foreign = await f.Client.GetAsync(f.ReviewRoute.Replace(TestData.ScenarioId, Guid.NewGuid().ToString("D")));
        Assert.That(foreign.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        using var wrongBody = await f.PostAsync(f.CorrectRoute, new { actor = "Owner", reason = "Reviewed", reviewedCorrectionHash = review.ReviewedCorrectionHash, version = "invented" }, "wrong-body");
        using var longActor = await f.PostAsync(f.CorrectRoute, Request(review) with { Actor = new string('a', 101) }, "actor");
        using var longReason = await f.PostAsync(f.CorrectRoute, Request(review) with { Reason = new string('a', 501) }, "reason");
        using var oversized = await f.PostAsync(f.CorrectRoute, Request(review) with { Reason = new string('a', 5000) }, "size");
        using var keyless = await f.PostAsync(f.CorrectRoute, Request(review), "");
        Assert.That(new[] { wrongBody.StatusCode, longActor.StatusCode, longReason.StatusCode, oversized.StatusCode, keyless.StatusCode },
            Is.All.EqualTo(HttpStatusCode.BadRequest));
    }

    private static CorrectScoreRequest Request(ScoringCorrectionReview review) => new("Local owner", "Preserve rejection and correct error bounds", review.ReviewedCorrectionHash!);
    private static void AssertSafe(string json)
    {
        foreach (string value in new[] { "worldId", "world-opaque", "canonicalInputJson", "canonicalOutputJson",
            "truthSamples", "bindingId", "publicationKey", "internalKey", "monthlyTruth", "clonedFieldId" })
            Assert.That(json, Does.Not.Contain(value).IgnoreCase);
    }

    private sealed class Fixture(ApiFactory factory, HttpClient client, PublicationFakeHandler upstream,
        ScoringWorkflowTests.PredictionFixture prediction, RunResponse run, ScorecardDraft original) : IAsyncDisposable
    {
        internal ApiFactory Factory => factory;
        internal HttpClient Client => client;
        internal PublicationFakeHandler Upstream => upstream;
        internal ScoringWorkflowTests.PredictionFixture Prediction => prediction;
        internal RunResponse Run => run;
        internal ScorecardDraft Original => original;
        internal DrillingOperationsStore Store => factory.Services.GetRequiredService<DrillingOperationsStore>();
        internal string ReviewRoute => $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/runs/{run.RunId}/scoring-correction";
        internal string CorrectRoute => $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/runs/{run.RunId}/correct-score";
        internal string HistoryRoute(string id) => $"/drillingoperations/api/runs/{run.RunId}/scorecards/{id}";
        internal static async Task<Fixture> CreateAsync()
        {
            var prediction = ScoringWorkflowTests.PredictionFixture.Create();
            var upstream = new PublicationFakeHandler { ScorecardRejectStatus = HttpStatusCode.BadRequest };
            var factory = new ApiFactory(prediction.Respond, publication: upstream);
            var client = factory.CreateInternalClient();
            var run = await ScoringWorkflowTests.ReadyRevealed(client, prediction.SealHash);
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/runs/{run.RunId}/score"))
            {
                request.Headers.Add("Idempotency-Key", "initial-rejected-score");
                Assert.That((await client.SendAsync(request)).StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            }
            var coordinator = factory.Services.GetRequiredService<ScoreCoordinator>();
            var historical = await coordinator.CalculateForCorrectionAsync(run, true, CancellationToken.None);
            Assert.That(ScoreCorrection.InvalidMetricCount(historical.Metrics), Is.GreaterThan(0));
            await SeedHistoricalOutputAsync(factory.DatabasePath, historical);
            return new(factory, client, upstream, prediction, run, historical);
        }
        internal ScoringCorrectionCoordinator Coordinator(DrillingOperationsStore store) => new(store,
            new ScoreCoordinator(store, factory.Services.GetRequiredService<AnalysisVerificationClient>(),
                factory.Services.GetRequiredService<AnalysisScorecardClient>()),
            factory.Services.GetRequiredService<AnalysisScorecardClient>(), NullLogger<ScoringCorrectionCoordinator>.Instance);
        internal async Task<ScoringCorrectionReview> ReviewAsync()
        {
            using var response = await client.GetAsync(ReviewRoute);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            return (await response.Content.ReadFromJsonAsync<ScoringCorrectionReview>())!;
        }
        internal Task<HttpResponseMessage> CorrectAsync(ScoringCorrectionReview review, string key) => PostAsync(CorrectRoute, Request(review), key);
        internal Task<HttpResponseMessage> ScoreAsync(string key) => PostAsync($"/drillingoperations/api/runs/{run.RunId}/score", null, key);
        internal async Task<HttpResponseMessage> PostAsync(string path, object? body, string key)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            if (key.Length > 0) request.Headers.Add("Idempotency-Key", key);
            if (body is not null) request.Content = JsonContent.Create(body);
            return await client.SendAsync(request);
        }
        internal async Task MutateAsync(string sql)
        {
            await using var c = new SqliteConnection(Store.ConnectionString); await c.OpenAsync();
            await using var q = c.CreateCommand(); q.CommandText = sql; await q.ExecuteNonQueryAsync();
        }
        internal async Task<string> ScalarAsync(string sql)
        {
            await using var c = new SqliteConnection(Store.ConnectionString); await c.OpenAsync();
            await using var q = c.CreateCommand(); q.CommandText = sql;
            return Convert.ToString(await q.ExecuteScalarAsync(), CultureInfo.InvariantCulture)!;
        }
        internal async Task<string> PreservedHashAsync()
        {
            await using var c = new SqliteConnection(Store.ConnectionString); await c.OpenAsync();
            var rows = new List<object?[]>();
            foreach (string table in new[] { "Scorecards", "ScorecardMetrics", "ScorecardDeliveries", "ScorecardReceipts",
                "PublicationPlans", "PublicationOperations", "PublicationOperationStates", "PublicationStates", "RevealManifests",
                "TruthBindings", "MaterializedPlans", "DrillingExecutions", "SurveyArtifacts", "TruthSampleBatches",
                "ObservationBatches", "CompletionDesigns", "CompletionApprovals", "ProductionTruthArtifacts", "ProductionSeries", "RunStages" })
            {
                await using var q = c.CreateCommand(); q.CommandText = $"SELECT * FROM {table}" + (table == "RunStages" ? " WHERE Stage<9" : "") + " ORDER BY rowid;";
                await using var r = await q.ExecuteReaderAsync();
                while (await r.ReadAsync()) rows.Add(Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? null : r.GetValue(i)).ToArray());
            }
            return DeterministicIdentity.Sha256(JsonSerializer.Serialize(rows));
        }
        public async ValueTask DisposeAsync() { client.Dispose(); await factory.DisposeAsync(); }
    }

    private static async Task SeedHistoricalOutputAsync(string path, ScorecardDraft old)
    {
        await using var c = new SqliteConnection($"Data Source={path}"); await c.OpenAsync();
        await using var tx = c.BeginTransaction(false);
        async Task Execute(string sql, params (string Key, object Value)[] parameters)
        {
            await using var q = c.CreateCommand(); q.Transaction = tx; q.CommandText = sql;
            foreach (var p in parameters) q.Parameters.AddWithValue(p.Key, p.Value);
            await q.ExecuteNonQueryAsync();
        }
        await Execute("DROP TRIGGER TR_Scorecards_NoUpdate;DROP TRIGGER TR_ScorecardMetrics_NoUpdate;DROP TRIGGER TR_AuditEntries_NoUpdate;");
        await Execute("UPDATE Scorecards SET CanonicalOutputJson=$json,OutputSha256=$hash WHERE ScorecardId=$id;UPDATE RunStages SET OutputHash=$hash WHERE RunId=$run AND Stage=9;",
            ("$json", old.CanonicalOutputJson), ("$hash", old.OutputSha256), ("$id", old.ScorecardId), ("$run", old.RunId));
        for (int i = 0; i < old.Metrics.Count; i++)
        {
            var metric = old.Metrics[i]; string json = CanonicalJson.Serialize(metric);
            await Execute("UPDATE ScorecardMetrics SET Value=$value,LowerBound=$lower,UpperBound=$upper,CanonicalJson=$json,ContentSha256=$hash WHERE ScorecardId=$id AND Sequence=$sequence;",
                ("$value", metric.Value ?? (object)DBNull.Value),
                ("$lower", metric.LowerBound ?? (object)DBNull.Value), ("$upper", metric.UpperBound ?? (object)DBNull.Value),
                ("$json", json), ("$hash", DeterministicIdentity.Sha256(json)), ("$id", old.ScorecardId), ("$sequence", i));
        }
        var audits = new List<(long Sequence, string Action, string Subject, string Data, string Created)>();
        await using (var q = c.CreateCommand())
        {
            q.Transaction = tx; q.CommandText = "SELECT Sequence,Action,SubjectId,DataJson,CreatedUtc FROM AuditEntries ORDER BY Sequence;";
            await using var r = await q.ExecuteReaderAsync();
            while (await r.ReadAsync()) audits.Add((r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4)));
        }
        string previous = new('0', 64);
        foreach (var audit in audits)
        {
            string data = audit.Data;
            if (audit.Action == "scorecard.calculated")
            {
                var node = JsonNode.Parse(data)!; node["outputSha256"] = old.OutputSha256;
                using var document = JsonDocument.Parse(node.ToJsonString()); data = CanonicalJson.Canonicalize(document.RootElement);
            }
            string dataHash = DeterministicIdentity.Sha256(data);
            string entryHash = DeterministicIdentity.Sha256(string.Join("\n", "audit-v1", old.ScenarioId,
                audit.Sequence.ToString(CultureInfo.InvariantCulture), audit.Action, audit.Subject, dataHash, previous, audit.Created));
            string id = DeterministicIdentity.Create("audit", old.ScenarioId, audit.Sequence.ToString(CultureInfo.InvariantCulture), entryHash);
            await Execute("UPDATE AuditEntries SET AuditId=$id,DataJson=$data,DataHash=$dataHash,PreviousHash=$previous,EntryHash=$hash WHERE Sequence=$sequence;",
                ("$id", id), ("$data", data), ("$dataHash", dataHash), ("$previous", previous), ("$hash", entryHash), ("$sequence", audit.Sequence));
            previous = entryHash;
        }
        await Execute("""
            CREATE TRIGGER TR_Scorecards_NoUpdate BEFORE UPDATE ON Scorecards BEGIN SELECT RAISE(ABORT,'Scorecards are immutable');END;
            CREATE TRIGGER TR_ScorecardMetrics_NoUpdate BEFORE UPDATE ON ScorecardMetrics BEGIN SELECT RAISE(ABORT,'Scorecard metrics are immutable');END;
            CREATE TRIGGER TR_AuditEntries_NoUpdate BEFORE UPDATE ON AuditEntries BEGIN SELECT RAISE(ABORT,'Audit is immutable');END;
            """);
        await tx.CommitAsync();
    }
}
