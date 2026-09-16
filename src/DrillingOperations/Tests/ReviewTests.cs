using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
namespace DrillingOperations.Tests;

[TestFixture]
public sealed class AuthoritativeBindingVerificationTests
{
    [Test]
    public async Task ApprovedMatchingAuthorities_VerifySuccessfully_AndStageAReceivesOperatorKey()
    {
        bool stageKeySeen = false;
        BindingVerificationService verifier = Build(request =>
        {
            if (request.RequestUri!.Host == "reservoir.test") stageKeySeen = request.Headers.TryGetValues("X-DrillSim-Operator-Key", out IEnumerable<string>? values) && values.Single() == "operator-key";
            return TestData.Upstream(request);
        });
        await verifier.VerifyAsync(TestData.Binding(), CancellationToken.None);
        Assert.That(stageKeySeen, Is.True);
    }

    [TestCase("unapproved")]
    [TestCase("seal")]
    [TestCase("submitted-seal")]
    [TestCase("package")]
    [TestCase("world")]
    [TestCase("calibration")]
    [TestCase("model")]
    [TestCase("field")]
    [TestCase("world-model")]
    [TestCase("reservoir")]
    public void AuthorityMismatch_IsRejected(string mismatch)
    {
        BindingVerificationService verifier = Build(request => MismatchedResponse(request, mismatch));
        BindingVerificationException exception = Assert.ThrowsAsync<BindingVerificationException>(async () => await verifier.VerifyAsync(TestData.Binding(), CancellationToken.None))!;
        Assert.That(exception.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public void UpstreamUnavailable_FailsClosed()
    {
        BindingVerificationService verifier = Build(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        BindingVerificationException exception = Assert.ThrowsAsync<BindingVerificationException>(async () => await verifier.VerifyAsync(TestData.Binding(), CancellationToken.None))!;
        Assert.That(exception.StatusCode, Is.AnyOf(502, 503));
    }

    [Test]
    public async Task OriginalBindingIdempotencyRetry_ReplaysWithoutRequiringUpstreamsAgain()
    {
        bool unavailable = false; int calls = 0;
        await using var factory = new ApiFactory(request => { calls++; return unavailable ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) : TestData.Upstream(request); });
        using HttpClient client = factory.CreateInternalClient();
        HttpRequestMessage Create() { var message = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/bind") { Content = System.Net.Http.Json.JsonContent.Create(TestData.Binding()) }; message.Headers.Add("Idempotency-Key", "binding-replay"); return message; }
        using HttpResponseMessage first = await client.SendAsync(Create()); int authoritativeCalls = calls; unavailable = true;
        using HttpResponseMessage replay = await client.SendAsync(Create());
        Assert.Multiple(() => { Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created)); Assert.That(replay.StatusCode, Is.EqualTo(HttpStatusCode.Created)); Assert.That(calls, Is.EqualTo(authoritativeCalls)); });
    }

    private static BindingVerificationService Build(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var analysis = new HttpClient(new DelegateHandler(responder)) { BaseAddress = new Uri("http://analysis.test/") };
        var stage = new HttpClient(new DelegateHandler(responder)) { BaseAddress = new Uri("http://reservoir.test/") };
        stage.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", "operator-key");
        return new BindingVerificationService(new AnalysisVerificationClient(analysis), new ReservoirVerificationClient(stage));
    }

    private static HttpResponseMessage MismatchedResponse(HttpRequestMessage request, string mismatch)
    {
        string path = request.RequestUri!.AbsolutePath;
        if (path.EndsWith("/prediction", StringComparison.Ordinal))
            return TestData.Json(HttpStatusCode.OK, new { scenarioId = TestData.ScenarioId, body = new { candidateId = "candidate-1", proposedWellPath = TestData.ApprovedPath(), fieldPackageSha256 = mismatch == "package" ? TestData.HashD : TestData.HashB }, revision = 3, seal = new { sha256 = mismatch == "submitted-seal" ? TestData.HashD : TestData.HashA }, approval = new { sealedSha256 = mismatch is "seal" or "submitted-seal" ? TestData.HashD : TestData.HashA } });
        if (path.StartsWith("/api/scenarios/", StringComparison.Ordinal))
            return TestData.Json(HttpStatusCode.OK, new { scenarioId = TestData.ScenarioId, sourceFieldId = TestData.FieldId, reservoirName = "SOGNEFJORD FM", worldModelVersion = mismatch == "model" ? "another-model" : "reservoir-hidden-world-v3", status = mismatch == "unapproved" ? "PredictionSealed" : "HumanApproved", observationModelVersion = "observation-model-v1" });
        return TestData.Json(HttpStatusCode.OK, new { worldId = mismatch == "world" ? "different-world" : "world-opaque-17", fieldId = mismatch == "field" ? Guid.Parse("33333333-3333-4333-8333-333333333333") : TestData.FieldId, reservoirName = mismatch == "reservoir" ? "OTHER FM" : "SOGNEFJORD FM", modelVersion = mismatch == "world-model" ? "another-model" : "reservoir-hidden-world-v3", calibrationArtifact = new { id = mismatch == "calibration" ? "different-calibration" : "calibration-opaque-9", sha256 = TestData.HashC } });
    }
}

[TestFixture]
[NonParallelizable]
public sealed class IntegrityRecoveryAndResumeTests
{
    [Test]
    public async Task TruthBinding_ReadDetectsTampering_AndUpdateDeleteTriggersRejectMutation()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync();
        await fixture.Store.BindWorldAsync("/bind", "bind", TestData.ScenarioId, TestData.Binding());
        await using var connection = new SqliteConnection(fixture.ConnectionString); await connection.OpenAsync();
        await using (SqliteCommand update = connection.CreateCommand())
        {
            update.CommandText = "UPDATE TruthBindings SET WorldId = $world WHERE ScenarioId = $scenario;"; update.Parameters.AddWithValue("$world", "tampered-world"); update.Parameters.AddWithValue("$scenario", TestData.ScenarioId);
            Assert.ThrowsAsync<SqliteException>(async () => await update.ExecuteNonQueryAsync());
        }
        await using (SqliteCommand delete = connection.CreateCommand())
        {
            delete.CommandText = "DELETE FROM TruthBindings WHERE ScenarioId = $scenario;"; delete.Parameters.AddWithValue("$scenario", TestData.ScenarioId);
            Assert.ThrowsAsync<SqliteException>(async () => await delete.ExecuteNonQueryAsync());
        }
        await using (SqliteCommand tamper = connection.CreateCommand())
        {
            tamper.CommandText = "DROP TRIGGER TR_TruthBindings_NoUpdate; UPDATE TruthBindings SET InputHash = $hash WHERE ScenarioId = $scenario;"; tamper.Parameters.AddWithValue("$hash", TestData.HashD); tamper.Parameters.AddWithValue("$scenario", TestData.ScenarioId); await tamper.ExecuteNonQueryAsync();
        }
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () => await fixture.Store.GetBindingAsync(TestData.ScenarioId));
    }

    [Test]
    public async Task IdempotencyReplay_DetectsResponseTampering()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync();
        await fixture.Store.BindWorldAsync("/bind", "bind", TestData.ScenarioId, TestData.Binding());
        await using var connection = new SqliteConnection(fixture.ConnectionString); await connection.OpenAsync();
        await using SqliteCommand tamper = connection.CreateCommand();
        tamper.CommandText = "UPDATE IdempotencyRecords SET ResponseBody = $body WHERE IdempotencyKey = $key;"; tamper.Parameters.AddWithValue("$body", "{}"); tamper.Parameters.AddWithValue("$key", "bind"); await tamper.ExecuteNonQueryAsync();
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () => await fixture.Store.BindWorldAsync("/bind", "bind", TestData.ScenarioId, TestData.Binding()));
    }

    [Test]
    public async Task Resume_RemainsBlockedUntilCapabilityIsAvailable_ThenQueues()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync(); RunResponse run = await StoreFixture.BindAndCreateAsync(fixture.Store);
        await fixture.Store.CompleteS0Async(run.RunId);
        await using (var connection = new SqliteConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync(); await using SqliteCommand legacy = connection.CreateCommand();
            legacy.CommandText = "UPDATE RunStages SET Status = $stageStatus, Diagnostics = $code WHERE RunId = $run AND Stage = 1; UPDATE Runs SET Status = $runStatus, CurrentStage = 1 WHERE RunId = $run;";
            legacy.Parameters.AddWithValue("$stageStatus", StageStatus.AwaitingDependency.ToString()); legacy.Parameters.AddWithValue("$code", "PlanMaterializationAdapterUnavailable"); legacy.Parameters.AddWithValue("$run", run.RunId); legacy.Parameters.AddWithValue("$runStatus", RunStatus.AwaitingDependency.ToString()); await legacy.ExecuteNonQueryAsync();
        }
        ApiOutcome unavailable = await fixture.Store.ResumeAsync("/resume", "resume-1", run.RunId, false);
        Assert.That(unavailable.StatusCode, Is.EqualTo(503)); Assert.That((await fixture.Store.GetRunAsync(run.RunId))!.Status, Is.EqualTo(RunStatus.AwaitingDependency));
        ApiOutcome available = await fixture.Store.ResumeAsync("/resume", "resume-2", run.RunId, true);
        Assert.That(available.StatusCode, Is.EqualTo(202)); Assert.That((await fixture.Store.GetRunAsync(run.RunId))!.Status, Is.EqualTo(RunStatus.Queued));
        Assert.That((await fixture.Store.GetStagesAsync(run.RunId)).Single(x => x.Stage == RunStageKind.S1MaterializePlan).Status, Is.EqualTo(StageStatus.Pending));
    }

    [Test]
    public async Task PoisonRun_DoesNotStarveFollowingRun()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync();
        RunResponse poison = await StoreFixture.BindAndCreateAsync(fixture.Store, TestData.ScenarioId, "poison");
        const string secondScenario = "44444444-4444-4444-8444-444444444444";
        RunResponse healthy = await StoreFixture.BindAndCreateAsync(fixture.Store, secondScenario, "healthy");
        var orchestrator = new RunOrchestrator(fixture.Store, new PoisonExecutor(fixture.Store, poison.RunId), NullLogger<RunOrchestrator>.Instance);
        await orchestrator.ResumeAllAsync();
        RunResponse poisonResult = (await fixture.Store.GetRunAsync(poison.RunId))!; RunResponse healthyResult = (await fixture.Store.GetRunAsync(healthy.RunId))!;
        Assert.Multiple(() => { Assert.That(poisonResult.Status, Is.EqualTo(RunStatus.Failed)); Assert.That(healthyResult.Status, Is.EqualTo(RunStatus.Running)); });
    }

    [Test]
    public async Task BindingIdentityAndS0Audit_CommitToFullCanonicalBindingHash()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync();
        BindWorldRequest request = TestData.Binding();
        await fixture.Store.BindWorldAsync("/bind", "bind", TestData.ScenarioId, request);
        TruthBindingResponse binding = (await fixture.Store.GetBindingAsync(TestData.ScenarioId))!;
        string expectedHash = DeterministicIdentity.Sha256(CanonicalJson.Serialize(request));
        Assert.That(binding.BindingId, Is.EqualTo(DeterministicIdentity.Create("truth-binding-v1", CanonicalJson.Serialize(request))));
        ApiOutcome created = await fixture.Store.CreateRunAsync("/runs", "run", TestData.Run());
        RunResponse run = System.Text.Json.JsonSerializer.Deserialize<RunResponse>(created.Body, CanonicalJson.SerializerOptions)!;
        await fixture.Store.AdvanceOneCheckpointAsync(run.RunId);
        await using var connection = new SqliteConnection(fixture.ConnectionString); await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT DataJson FROM AuditEntries WHERE ScenarioId = $scenario AND (Action = $bindingAction OR Action = $stageAction) ORDER BY Sequence;";
        command.Parameters.AddWithValue("$scenario", TestData.ScenarioId); command.Parameters.AddWithValue("$bindingAction", "binding.created"); command.Parameters.AddWithValue("$stageAction", "stage.completed");
        var evidence = new List<string>(); await using SqliteDataReader reader = await command.ExecuteReaderAsync(); while (await reader.ReadAsync()) evidence.Add(reader.GetString(0));
        Assert.That(evidence, Has.Count.EqualTo(2)); Assert.That(evidence, Has.All.Contains(expectedHash));
    }

    [Test]
    public async Task TerminalDeterministicRun_NewKeyConflicts_OriginalKeyStillReplays()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync();
        await fixture.Store.BindWorldAsync("/bind", "bind", TestData.ScenarioId, TestData.Binding());
        ApiOutcome created = await fixture.Store.CreateRunAsync("/runs", "original-run-key", TestData.Run());
        RunResponse run = System.Text.Json.JsonSerializer.Deserialize<RunResponse>(created.Body, CanonicalJson.SerializerOptions)!;
        await fixture.Store.CancelAsync("/cancel", "cancel", run.RunId);
        ApiOutcome replay = await fixture.Store.CreateRunAsync("/runs", "original-run-key", TestData.Run()); ApiOutcome newKey = await fixture.Store.CreateRunAsync("/runs", "new-run-key", TestData.Run());
        Assert.Multiple(() => { Assert.That(replay.StatusCode, Is.EqualTo(202)); Assert.That(replay.Replayed, Is.True); Assert.That(newKey.StatusCode, Is.EqualTo(409)); });
    }

    [Test]
    public void Validation_RejectsUppercaseHashesAndNonCanonicalGuids()
    {
        Assert.That(RequestValidation.Validate(TestData.Binding() with { ApprovedSealedPredictionHash = TestData.HashA.ToUpperInvariant() }), Is.Not.Empty);
        Assert.That(RequestValidation.Validate(TestData.Binding() with { ScenarioId = "AAAAAAAA-AAAA-4AAA-8AAA-AAAAAAAAAAAA" }), Is.Not.Empty);
        Assert.That(RequestValidation.Validate(TestData.Binding() with { WorldId = new string((char)120, 129) }), Is.Not.Empty);
    }

    private sealed class PoisonExecutor(DrillingOperationsStore store, string poisonRunId) : IRunCheckpointExecutor
    {
        public Task<bool> AdvanceOneAsync(string runId, CancellationToken cancellationToken) => runId == poisonRunId ? throw new InvalidOperationException("poison") : store.AdvanceOneCheckpointAsync(runId, cancellationToken);
    }
}
