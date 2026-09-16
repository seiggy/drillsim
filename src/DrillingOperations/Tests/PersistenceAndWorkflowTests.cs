using Microsoft.Data.Sqlite;

namespace DrillingOperations.Tests;

[TestFixture]
[NonParallelizable]
public sealed class PersistenceAndWorkflowTests
{
    [Test]
    public async Task Schema_RoundTripsAcrossFreshStore_AndContainsAllP3Records()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        RunResponse created = await StoreFixture.BindAndCreateAsync(fixture.Store);
        var restarted = new DrillingOperationsStore(fixture.ConnectionString, TimeProvider.System);
        await restarted.InitializeAsync();

        RunResponse? loaded = await restarted.GetRunAsync(created.RunId);
        TruthBindingResponse? binding = await restarted.GetBindingAsync(created.ScenarioId);
        await using var connection = new SqliteConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table';";
        var names = new HashSet<string>(StringComparer.Ordinal);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) names.Add(reader.GetString(0));

        Assert.Multiple(() =>
        {
            Assert.That(loaded, Is.EqualTo(created));
            Assert.That(binding, Is.Not.Null);
            Assert.That(names, Does.Contain("Runs"));
            Assert.That(names, Does.Contain("RunStages"));
            Assert.That(names, Does.Contain("TruthBindings"));
            Assert.That(names, Does.Contain("ObservationBatches"));
            Assert.That(names, Does.Contain("RevealManifests"));
            Assert.That(names, Does.Contain("AuditEntries"));
            Assert.That(names, Does.Contain("IdempotencyRecords"));
        });
    }

    [Test]
    public async Task Binding_IsImmutable_AndDomainIdempotent()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        BindWorldRequest request = TestData.Binding();
        ApiOutcome first = await fixture.Store.BindWorldAsync("/bind/scenario-1", "a", TestData.ScenarioId, request);
        ApiOutcome same = await fixture.Store.BindWorldAsync("/bind/scenario-1", "b", TestData.ScenarioId, request);
        ApiOutcome mutation = await fixture.Store.BindWorldAsync("/bind/scenario-1", "c", TestData.ScenarioId,
            request with { WorldId = "different-world" });

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(201));
            Assert.That(same.StatusCode, Is.EqualTo(200));
            Assert.That(mutation.StatusCode, Is.EqualTo(409));
            Assert.That(mutation.Body, Does.Not.Contain("world-opaque-17"));
        });
    }

    [Test]
    public async Task Idempotency_ReplaysExactResponse_AndConflictingUseReturns409()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        BindWorldRequest request = TestData.Binding();
        ApiOutcome first = await fixture.Store.BindWorldAsync("/bind/scenario-1", "same-key", TestData.ScenarioId, request);
        ApiOutcome replay = await fixture.Store.BindWorldAsync("/bind/scenario-1", "same-key", TestData.ScenarioId, request);
        ApiOutcome conflict = await fixture.Store.CreateRunAsync("/runs", "same-key", TestData.Run());

        Assert.Multiple(() =>
        {
            Assert.That(replay.Replayed, Is.True);
            Assert.That(replay.StatusCode, Is.EqualTo(first.StatusCode));
            Assert.That(replay.Body, Is.EqualTo(first.Body));
            Assert.That(replay.Location, Is.EqualTo(first.Location));
            Assert.That(conflict.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task Database_EnforcesOneActiveRunPerScenario()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        await fixture.Store.BindWorldAsync("/bind", "bind", TestData.ScenarioId, TestData.Binding());
        ApiOutcome first = await fixture.Store.CreateRunAsync("/runs", "run-1", TestData.Run());
        ApiOutcome second = await fixture.Store.CreateRunAsync("/runs", "run-2",
            TestData.Run() with { PlanArtifactId = "another-plan", PlanArtifactSha256 = TestData.HashC });

        Assert.That(first.StatusCode, Is.EqualTo(202));
        Assert.That(second.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public void StateMachine_AllowsOnlyLegalOrderedProgression()
    {
        Dictionary<RunStageKind, StageStatus> stages = Enum.GetValues<RunStageKind>()
            .ToDictionary(static x => x, static _ => StageStatus.Pending);
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S0BindWorld, stages), Is.True);
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S1MaterializePlan, stages), Is.False);
        stages[RunStageKind.S0BindWorld] = StageStatus.Completed;
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S1MaterializePlan, stages), Is.True);
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S8PublishReveal, stages), Is.False);
        stages[RunStageKind.S7RunProduction] = StageStatus.Completed;
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S8PublishReveal, stages), Is.True);
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S9Score, stages), Is.False);
        stages[RunStageKind.S8PublishReveal] = StageStatus.Completed;
        Assert.That(RunStateMachine.CanExecute(RunStageKind.S9Score, stages), Is.True);
    }

    [Test]
    public async Task FreshStore_ResumesS1AndS2FromPersistedS0()
    {
        await using var fixture = new StoreFixture(); await fixture.InitializeAsync();
        RunResponse run = await StoreFixture.BindAndCreateAsync(fixture.Store);
        Assert.That(await fixture.Store.CompleteS0Async(run.RunId), Is.True);
        var restarted = new DrillingOperationsStore(fixture.ConnectionString, TimeProvider.System); await restarted.InitializeAsync();
        Assert.That(await restarted.MaterializePlanAsync(run.RunId, TestData.ScenarioSnapshot(), TestData.PredictionSnapshot()), Is.True);
        Assert.That(await restarted.ExecuteDeterministicDrillingAsync(run.RunId, new DrillingExecutionOptions()), Is.True);
        Assert.That(await restarted.SetAwaitingDependencyAsync(run.RunId, RunStageKind.S3GenerateSurvey, "SurveyObservationAdapterUnavailable"), Is.True);
        IReadOnlyList<StageResponse> stages = await restarted.GetStagesAsync(run.RunId);
        Assert.Multiple(() =>
        {
            Assert.That(stages.Take(3).Select(x => x.Status), Has.All.EqualTo(StageStatus.Completed));
            Assert.That(stages.Single(x => x.Stage == RunStageKind.S3GenerateSurvey).Status, Is.EqualTo(StageStatus.AwaitingDependency));
            Assert.That((restarted.GetRunAsync(run.RunId).Result)!.Status, Is.EqualTo(RunStatus.AwaitingDependency));
        });
    }

    [Test]
    public async Task Cancellation_HasNoPublicationOrClockSideEffects()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        RunResponse run = await StoreFixture.BindAndCreateAsync(fixture.Store);
        ApiOutcome cancelled = await fixture.Store.CancelAsync($"/runs/{run.RunId}/cancel", "cancel", run.RunId);
        (int publications, int clockAdvances) = await fixture.Store.GetSideEffectCountsAsync(run.RunId);

        Assert.Multiple(() =>
        {
            Assert.That(cancelled.StatusCode, Is.EqualTo(202));
            Assert.That(publications, Is.Zero);
            Assert.That(clockAdvances, Is.Zero);
        });
    }

    [Test]
    public async Task AuditChain_Verifies_AndRejectsAppendedTampering()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        await StoreFixture.BindAndCreateAsync(fixture.Store);
        Assert.That(await fixture.Store.VerifyAuditChainAsync(TestData.ScenarioId), Is.True);

        await using var connection = new SqliteConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using SqliteCommand insert = connection.CreateCommand();
        insert.CommandText = """
            INSERT INTO AuditEntries
                (AuditId, ScenarioId, Sequence, Action, SubjectId, DataJson, DataHash, PreviousHash, EntryHash, CreatedUtc)
            VALUES ($id, $scenario, 99, $action, $subject, $data, $hash, $previous, $entry, $created);
            """;
        insert.Parameters.AddWithValue("$id", "tampered-id");
        insert.Parameters.AddWithValue("$scenario", TestData.ScenarioId);
        insert.Parameters.AddWithValue("$action", "tampered");
        insert.Parameters.AddWithValue("$subject", "tampered");
        insert.Parameters.AddWithValue("$data", "{}");
        insert.Parameters.AddWithValue("$hash", TestData.HashA);
        insert.Parameters.AddWithValue("$previous", TestData.HashB);
        insert.Parameters.AddWithValue("$entry", TestData.HashC);
        insert.Parameters.AddWithValue("$created", DateTimeOffset.UtcNow.ToString("O"));
        await insert.ExecuteNonQueryAsync();

        Assert.That(await fixture.Store.VerifyAuditChainAsync(TestData.ScenarioId), Is.False);
    }
}
