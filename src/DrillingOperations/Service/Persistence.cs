using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace DrillingOperations;

public sealed partial class DrillingOperationsStore(string connectionString, TimeProvider timeProvider)
{
    private const string EmptyHash = "0000000000000000000000000000000000000000000000000000000000000000";
    public string ConnectionString { get; } = !string.IsNullOrWhiteSpace(connectionString)
        ? connectionString : throw new ArgumentException("SQLite connection string is required.", nameof(connectionString));

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using (SqliteCommand journal = connection.CreateCommand())
        {
            journal.CommandText = "PRAGMA journal_mode=WAL;";
            await journal.ExecuteScalarAsync(cancellationToken);
        }
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = Schema;
        await command.ExecuteNonQueryAsync(cancellationToken);
        await InitializeSimulationSetupSchemaAsync(connection, cancellationToken);
        await InitializeExecutionSchemaAsync(connection, cancellationToken);
        await InitializeObservationSchemaAsync(connection, cancellationToken);
        await InitializeLogSchemaAsync(connection, cancellationToken);
        await InitializeDrillingObservationSchemaAsync(connection, cancellationToken);
        await InitializeCompletionSchemaAsync(connection, cancellationToken);
        await InitializeProductionSchemaAsync(connection, cancellationToken);
        await InitializePublicationSchemaAsync(connection, cancellationToken);
        await InitializeScoringSchemaAsync(connection, cancellationToken);
        await MigrateLegacyScorecardContractAsync(connection, cancellationToken);
        await MigrateLegacyPublicationAdapterStateAsync(connection, cancellationToken);
        await MigrateLegacyPublicationNormalizationFailureAsync(connection, cancellationToken);
        await MigrateLegacyPreRepairRevealCallbackMismatchAsync(connection, cancellationToken);
        await MigrateLegacyRevealCallbackMismatchAsync(connection, cancellationToken);
    }

    public async Task<ApiOutcome?> TryReplayAsync(string route, string key, string canonicalRequest, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key.Any(static c => c < 33 || c > 126))
            return Problem(400, "A bounded Idempotency-Key header is required");
        string requestHash = DeterministicIdentity.Sha256(canonicalRequest);
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = connection.BeginTransaction();
        ApiOutcome? result = await ReadIdempotencyAsync(connection, transaction, key, route, requestHash, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<ApiOutcome> ExecuteIdempotentAsync(
        string route,
        string key,
        string canonicalRequest,
        Func<SqliteConnection, SqliteTransaction, CancellationToken, Task<ApiOutcome>> operation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Problem(400, "Idempotency-Key header is required");
        if (key.Length > 128 || key.Any(static c => c < 33 || c > 126))
            return Problem(400, "Idempotency-Key must contain at most 128 visible ASCII characters");
        if (route.Length > 500 || canonicalRequest.Length > 1_048_576)
            return Problem(400, "Idempotency route or request exceeds its bound");

        string requestHash = DeterministicIdentity.Sha256(canonicalRequest);
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        ApiOutcome? stored = await ReadIdempotencyAsync(connection, transaction, key, route, requestHash, cancellationToken);
        if (stored is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return stored;
        }

        ApiOutcome outcome = await operation(connection, transaction, cancellationToken);
        await using SqliteCommand insert = Command(connection, transaction, """
            INSERT INTO IdempotencyRecords
                (IdempotencyId, IdempotencyKey, Route, RequestHash, CanonicalRequestJson,
                 ResponseStatus, ResponseBody, ResponseHash, Location, CreatedUtc)
            VALUES ($id, $key, $route, $requestHash, $requestJson,
                    $status, $body, $responseHash, $location, $createdUtc);
            """);
        Add(insert, "$id", DeterministicIdentity.Create("idempotency", key));
        Add(insert, "$key", key);
        Add(insert, "$route", route);
        Add(insert, "$requestHash", requestHash);
        Add(insert, "$requestJson", canonicalRequest);
        Add(insert, "$status", outcome.StatusCode);
        Add(insert, "$body", outcome.Body);
        Add(insert, "$responseHash", DeterministicIdentity.Sha256(outcome.Body));
        Add(insert, "$location", outcome.Location);
        Add(insert, "$createdUtc", NowText());
        await insert.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return outcome;
    }

    public Task<ApiOutcome> BindWorldAsync(string route, string key, string scenarioId, BindWorldRequest request, CancellationToken cancellationToken = default)
    {
        string canonical = CanonicalJson.Serialize(request);
        return ExecuteIdempotentAsync(route, key, canonical,
            (connection, transaction, ct) => BindWorldCoreAsync(connection, transaction, scenarioId, request, ct), cancellationToken);
    }

    private async Task<ApiOutcome> BindWorldCoreAsync(
        SqliteConnection connection, SqliteTransaction transaction, string scenarioId,
        BindWorldRequest request, CancellationToken ct)
    {
            string canonical = CanonicalJson.Serialize(request);
            IReadOnlyDictionary<string, string[]> errors = RequestValidation.Validate(request);
            if (!StringComparer.Ordinal.Equals(scenarioId, request.ScenarioId))
                return Validation(new Dictionary<string, string[]> { [nameof(request.ScenarioId)] = ["ScenarioId must match the route."] });
            if (errors.Count > 0) return Validation(errors);

            TruthBindingResponse? existing = await ReadBindingAsync(connection, transaction, request.ScenarioId, ct);
            if (existing is not null)
            {
                string existingCanonical = CanonicalJson.Serialize(ToRequest(existing));
                return existingCanonical == canonical
                    ? Json(200, existing)
                    : Problem(409, "Scenario truth binding is immutable");
            }

            DateTimeOffset now = timeProvider.GetUtcNow();
            string normalizedCanonical = CanonicalJson.Serialize(request);
            string bindingInputHash = DeterministicIdentity.Sha256(normalizedCanonical);
            var response = new TruthBindingResponse(
                DeterministicIdentity.Create("truth-binding-v1", normalizedCanonical),
                request.ScenarioId,
                NormalizeHash(request.ApprovedSealedPredictionHash),
                NormalizeHash(request.SourcePackageSha256),
                request.WorldId,
                request.WorldModelVersion,
                request.CalibrationArtifactId,
                NormalizeHash(request.CalibrationArtifactSha256),
                now);
            await using SqliteCommand insert = Command(connection, transaction, """
                INSERT INTO TruthBindings
                    (BindingId, ScenarioId, ApprovedPredictionHash, SourcePackageSha256, WorldId,
                     WorldModelVersion, CalibrationArtifactId, CalibrationArtifactSha256,
                     CanonicalInputJson, InputHash, CreatedUtc)
                VALUES ($bindingId, $scenarioId, $predictionHash, $sourceHash, $worldId,
                        $worldVersion, $calibrationId, $calibrationHash, $input, $inputHash, $createdUtc);
                """);
            Add(insert, "$bindingId", response.BindingId);
            Add(insert, "$scenarioId", response.ScenarioId);
            Add(insert, "$predictionHash", response.ApprovedSealedPredictionHash);
            Add(insert, "$sourceHash", response.SourcePackageSha256);
            Add(insert, "$worldId", response.WorldId);
            Add(insert, "$worldVersion", response.WorldModelVersion);
            Add(insert, "$calibrationId", response.CalibrationArtifactId);
            Add(insert, "$calibrationHash", response.CalibrationArtifactSha256);
            Add(insert, "$input", normalizedCanonical);
            Add(insert, "$inputHash", bindingInputHash);
            Add(insert, "$createdUtc", Format(now));
            await insert.ExecuteNonQueryAsync(ct);
            await AppendAuditAsync(connection, transaction, request.ScenarioId, "binding.created", response.BindingId,
                CanonicalJson.Serialize(new { response.BindingId, bindingInputHash }), ct);
            return Json(201, response, $"/drillingoperations/api/scenarios/{Uri.EscapeDataString(request.ScenarioId)}/binding");
    }

    public async Task<TruthBindingResponse?> GetBindingAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        return await ReadBindingAsync(connection, null, scenarioId, cancellationToken);
    }

    public Task<ApiOutcome> CreateRunAsync(string route, string key, CreateRunRequest request, CancellationToken cancellationToken = default)
    {
        string canonical = CanonicalJson.Serialize(request);
        return ExecuteIdempotentAsync(route, key, canonical, async (connection, transaction, ct) =>
        {
            IReadOnlyDictionary<string, string[]> errors = RequestValidation.Validate(request);
            if (errors.Count > 0) return Validation(errors);
            TruthBindingResponse? binding = await ReadBindingAsync(connection, transaction, request.ScenarioId, ct);
            if (binding is null) return Problem(409, "Scenario must have an immutable truth binding before execution");
            if (!StringComparer.OrdinalIgnoreCase.Equals(binding.ApprovedSealedPredictionHash, request.ApprovedSealedPredictionHash))
                return Problem(409, "Run prediction hash does not match the immutable binding");

            string runId = DeterministicIdentity.Create("drilling-run", request.ScenarioId,
                request.ApprovedSealedPredictionHash.ToLowerInvariant(), request.PlanArtifactSha256.ToLowerInvariant());
            RunResponse? sameRun = await ReadRunAsync(connection, transaction, runId, ct);
            if (sameRun is not null)
                return RunStateMachine.IsTerminal(sameRun.Status)
                    ? Problem(409, "A terminal deterministic run cannot be executed again; reset requires a new scenario")
                    : Json(202, sameRun, $"/drillingoperations/api/runs/{runId}");

            if (await HasActiveRunAsync(connection, transaction, request.ScenarioId, ct))
                return Problem(409, "Only one active run is allowed per scenario");

            DateTimeOffset now = timeProvider.GetUtcNow();
            await using SqliteCommand insert = Command(connection, transaction, """
                INSERT INTO Runs
                    (RunId, ScenarioId, BindingId, ApprovedPredictionHash, PlanArtifactId, PlanArtifactSha256,
                     CanonicalInputJson, InputHash, Status, CurrentStage, CreatedUtc, UpdatedUtc,
                     EndedUtc, DiagnosticCode, PublicationCount, ClockAdvanceCount)
                VALUES ($runId, $scenarioId, $bindingId, $predictionHash, $planId, $planHash,
                        $input, $inputHash, $status, NULL, $createdUtc, $updatedUtc,
                        NULL, NULL, 0, 0);
                """);
            Add(insert, "$runId", runId);
            Add(insert, "$scenarioId", request.ScenarioId);
            Add(insert, "$bindingId", binding.BindingId);
            Add(insert, "$predictionHash", NormalizeHash(request.ApprovedSealedPredictionHash));
            Add(insert, "$planId", request.PlanArtifactId);
            Add(insert, "$planHash", NormalizeHash(request.PlanArtifactSha256));
            Add(insert, "$input", canonical);
            Add(insert, "$inputHash", DeterministicIdentity.Sha256(canonical));
            Add(insert, "$status", RunStatus.Queued.ToString());
            Add(insert, "$createdUtc", Format(now));
            Add(insert, "$updatedUtc", Format(now));
            await insert.ExecuteNonQueryAsync(ct);

            foreach (RunStageKind stage in Enum.GetValues<RunStageKind>())
            {
                await using SqliteCommand stageInsert = Command(connection, transaction, """
                    INSERT INTO RunStages
                        (RunId, Stage, Name, InputJson, InputHash, OutputJson, OutputHash,
                         AttemptCount, Status, StartedUtc, EndedUtc, Diagnostics, PreviousAuditHash)
                    VALUES ($runId, $stage, $name, NULL, NULL, NULL, NULL,
                            0, $status, NULL, NULL, NULL, $previousAuditHash);
                    """);
                Add(stageInsert, "$runId", runId);
                Add(stageInsert, "$stage", (int)stage);
                Add(stageInsert, "$name", StageName(stage));
                Add(stageInsert, "$status", StageStatus.Pending.ToString());
                Add(stageInsert, "$previousAuditHash", EmptyHash);
                await stageInsert.ExecuteNonQueryAsync(ct);
            }
            await AppendAuditAsync(connection, transaction, request.ScenarioId, "run.created", runId,
                CanonicalJson.Serialize(new { runId, request.PlanArtifactId, planArtifactSha256 = NormalizeHash(request.PlanArtifactSha256) }), ct);
            return Json(202, new RunResponse(runId, request.ScenarioId, RunStatus.Queued, null, now, now, null),
                $"/drillingoperations/api/runs/{runId}");
        }, cancellationToken);
    }

    public async Task<RunResponse?> GetRunAsync(string runId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        return await ReadRunAsync(connection, null, runId, cancellationToken);
    }

    public async Task<IReadOnlyList<StageResponse>> GetStagesAsync(string runId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        return await ReadStagesAsync(connection, null, runId, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetResumableRunIdsAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT RunId FROM Runs
            WHERE Status IN ($queued, $running)
            ORDER BY CreatedUtc, RunId;
            """;
        Add(command, "$queued", RunStatus.Queued.ToString());
        Add(command, "$running", RunStatus.Running.ToString());
        var ids = new List<string>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) ids.Add(reader.GetString(0));
        return ids;
    }

    public Task<bool> AdvanceOneCheckpointAsync(string runId, CancellationToken cancellationToken = default) => CompleteS0Async(runId, cancellationToken);

    public async Task<bool> CompleteS0Async(string runId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        RunResponse? run = await ReadRunAsync(connection, transaction, runId, cancellationToken);
        if (run is null || RunStateMachine.IsTerminal(run.Status) || run.Status is RunStatus.AwaitingDependency or RunStatus.Blocked)
        { await transaction.CommitAsync(cancellationToken); return false; }
        IReadOnlyList<StageResponse> stages = await ReadStagesAsync(connection, transaction, runId, cancellationToken);
        StageResponse s0 = stages.Single(static stage => stage.Stage is RunStageKind.S0BindWorld);
        if (s0.Status is StageStatus.Completed) { await transaction.CommitAsync(cancellationToken); return false; }
        TruthBindingResponse? binding = await ReadBindingAsync(connection, transaction, run.ScenarioId, cancellationToken);
        if (binding is null)
        { await FailStageAsync(connection, transaction, run, RunStageKind.S0BindWorld, "BindingMissing", cancellationToken); await transaction.CommitAsync(cancellationToken); return true; }
        DateTimeOffset now = timeProvider.GetUtcNow();
        string canonicalBinding = CanonicalJson.Serialize(ToRequest(binding));
        string bindingInputHash = DeterministicIdentity.Sha256(canonicalBinding);
        string input = CanonicalJson.Serialize(new { binding.BindingId, bindingInputHash });
        string output = CanonicalJson.Serialize(new { binding.BindingId, bindingInputHash });
        await CompleteStageAsync(connection, transaction, run, RunStageKind.S0BindWorld, input, output, bindingInputHash, now, cancellationToken);
        await transaction.CommitAsync(cancellationToken); return true;
    }

    public Task<ApiOutcome> ResumeAsync(string route, string key, string runId, bool capabilityAvailable, CancellationToken cancellationToken = default) =>
        ExecuteIdempotentAsync(route, key, CanonicalJson.Serialize(new { runId }), async (connection, transaction, ct) =>
        {
            RunResponse? run = await ReadRunAsync(connection, transaction, runId, ct);
            if (run is null) return Problem(404, "Run not found");
            if (run.Status is not RunStatus.AwaitingDependency || run.CurrentStage is null)
                return Problem(409, "Run is not awaiting an available dependency");
            RunStageKind blockedStage = run.CurrentStage.Value;
            if (!capabilityAvailable) return Problem(503, $"{blockedStage} dependency is unavailable");
            DateTimeOffset now = timeProvider.GetUtcNow();
            await using SqliteCommand stage = Command(connection, transaction, """
                UPDATE RunStages SET Status = $pending, StartedUtc = NULL, EndedUtc = NULL, Diagnostics = NULL
                WHERE RunId = $runId AND Stage = $stage AND Status = $awaiting;
                """);
            Add(stage, "$pending", StageStatus.Pending.ToString()); Add(stage, "$runId", runId);
            Add(stage, "$stage", (int)blockedStage); Add(stage, "$awaiting", StageStatus.AwaitingDependency.ToString());
            if (await stage.ExecuteNonQueryAsync(ct) != 1) return Problem(409, "Run dependency checkpoint changed concurrently");
            RunStageKind? previousStage = blockedStage is RunStageKind.S0BindWorld ? null : (RunStageKind)((int)blockedStage - 1);
            await UpdateRunAsync(connection, transaction, runId, RunStatus.Queued, previousStage, now, null, null, ct);
            await AppendAuditAsync(connection, transaction, run.ScenarioId, "run.resumed", runId,
                CanonicalJson.Serialize(new { stage = blockedStage.ToString() }), ct);
            return Json(202, run with { Status = RunStatus.Queued, CurrentStage = previousStage, UpdatedUtc = now, DiagnosticCode = null },
                $"/drillingoperations/api/runs/{runId}");
        }, cancellationToken);

    public async Task MarkRunFailedAsync(string runId, string diagnosticCode, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        RunResponse? run = await ReadRunAsync(connection, transaction, runId, cancellationToken);
        if (run is not null && !RunStateMachine.IsTerminal(run.Status))
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            IReadOnlyList<StageResponse> stages = await ReadStagesAsync(connection, transaction, runId, cancellationToken);
            RunStageKind? attemptedStage = stages.Where(static stage => stage.Status != StageStatus.Completed)
                .OrderBy(static stage => stage.Stage).Select(static stage => (RunStageKind?)stage.Stage).FirstOrDefault();
            if (attemptedStage is RunStageKind stageKind)
            {
                await using SqliteCommand stage = Command(connection, transaction,
                    "UPDATE RunStages SET Status = $failed, EndedUtc = $ended, Diagnostics = $code WHERE RunId = $runId AND Stage = $stage AND Status <> $completed;");
                Add(stage, "$failed", StageStatus.Failed.ToString()); Add(stage, "$ended", Format(now)); Add(stage, "$code", diagnosticCode);
                Add(stage, "$runId", runId); Add(stage, "$stage", (int)stageKind); Add(stage, "$completed", StageStatus.Completed.ToString());
                await stage.ExecuteNonQueryAsync(cancellationToken);
            }
            RunStageKind? failedAt = attemptedStage ?? run.CurrentStage;
            await UpdateRunAsync(connection, transaction, runId, RunStatus.Failed, failedAt, now, diagnosticCode, now, cancellationToken);
            await AppendAuditAsync(connection, transaction, run.ScenarioId, "run.failed", runId, CanonicalJson.Serialize(new { diagnosticCode, stage = failedAt }), cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    public Task<ApiOutcome> CancelAsync(string route, string key, string runId, CancellationToken cancellationToken = default) =>
        ExecuteIdempotentAsync(route, key, CanonicalJson.Serialize(new { runId }), async (connection, transaction, ct) =>
        {
            RunResponse? run = await ReadRunAsync(connection, transaction, runId, ct);
            if (run is null) return Problem(404, "Run not found");
            if (run.Status is RunStatus.Revealed or RunStatus.Scored)
                return Problem(409, "A revealed run cannot be cancelled");
            if (run.Status is not RunStatus.Cancelled)
            {
                DateTimeOffset now = timeProvider.GetUtcNow();
                await using SqliteCommand stages = Command(connection, transaction, """
                    UPDATE RunStages SET Status = $cancelled, EndedUtc = $endedUtc, Diagnostics = $diagnostics
                    WHERE RunId = $runId AND Status <> $completed;
                    """);
                Add(stages, "$cancelled", StageStatus.Cancelled.ToString());
                Add(stages, "$endedUtc", Format(now));
                Add(stages, "$diagnostics", "OperatorCancelled");
                Add(stages, "$runId", runId);
                Add(stages, "$completed", StageStatus.Completed.ToString());
                await stages.ExecuteNonQueryAsync(ct);
                await UpdateRunAsync(connection, transaction, runId, RunStatus.Cancelled, run.CurrentStage,
                    now, "OperatorCancelled", now, ct);
                await AppendAuditAsync(connection, transaction, run.ScenarioId, "run.cancelled", runId,
                    CanonicalJson.Serialize(new { publicationCount = 0, clockAdvanceCount = 0 }), ct);
                run = run with { Status = RunStatus.Cancelled, UpdatedUtc = now, EndedUtc = now, DiagnosticCode = "OperatorCancelled" };
            }
            return Json(202, run, $"/drillingoperations/api/runs/{runId}");
        }, cancellationToken);

    public Task<ApiOutcome> PublishAsync(string route, string key, string runId, CancellationToken cancellationToken = default) =>
        ExecuteIdempotentAsync(route, key, CanonicalJson.Serialize(new { runId }), async (connection, transaction, ct) =>
        {
            RunResponse? run = await ReadRunAsync(connection, transaction, runId, ct);
            if (run is null) return Problem(404, "Run not found");
            IReadOnlyList<StageResponse> stages = await ReadStagesAsync(connection, transaction, runId, ct);
            if (stages.Single(x => x.Stage == RunStageKind.S7RunProduction).Status is not StageStatus.Completed)
            {
                await AppendAuditAsync(connection, transaction, run.ScenarioId, "publish.blocked", runId,
                    CanonicalJson.Serialize(new { code = "ObservationPipelineIncomplete" }), ct);
                return Problem(409, "Publication is unavailable until S7 completes");
            }
            await AppendAuditAsync(connection, transaction, run.ScenarioId, "publish.blocked", runId,
                CanonicalJson.Serialize(new { code = "PublicationAdapterUnavailable" }), ct);
            return Problem(503, "Publication adapter is awaiting a later phase");
        }, cancellationToken);

    public async Task<IReadOnlyList<AuditResponse>> GetAuditAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        return await ReadAuditAsync(connection, scenarioId, cancellationToken);
    }

    public async Task<bool> VerifyAuditChainAsync(string scenarioId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT AuditId, Sequence, Action, SubjectId, DataJson, DataHash, PreviousHash, EntryHash, CreatedUtc
            FROM AuditEntries WHERE ScenarioId = $scenarioId ORDER BY Sequence;
            """;
        Add(command, "$scenarioId", scenarioId);
        string expectedPrevious = EmptyHash;
        long expectedSequence = 1;
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long sequence = reader.GetInt64(1);
            string dataJson = reader.GetString(4);
            string dataHash = DeterministicIdentity.Sha256(dataJson);
            string createdUtc = reader.GetString(8);
            string entryHash = AuditHash(scenarioId, sequence, reader.GetString(2), reader.GetString(3),
                dataHash, expectedPrevious, createdUtc);
            if (sequence != expectedSequence || reader.GetString(5) != dataHash ||
                reader.GetString(6) != expectedPrevious || reader.GetString(7) != entryHash ||
                reader.GetString(0) != DeterministicIdentity.Create("audit", scenarioId,
                    sequence.ToString(CultureInfo.InvariantCulture), entryHash))
                return false;
            expectedPrevious = entryHash;
            expectedSequence++;
        }
        return true;
    }

    public async Task<(int PublicationCount, int ClockAdvanceCount)> GetSideEffectCountsAsync(
        string runId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT PublicationCount, ClockAdvanceCount FROM Runs WHERE RunId = $runId;";
        Add(command, "$runId", runId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Run not found.");
        return (reader.GetInt32(0), reader.GetInt32(1));
    }

    private async Task SetAwaitingDependencyAsync(SqliteConnection connection, SqliteTransaction transaction, RunResponse run,
        RunStageKind stage, string input, string code, DateTimeOffset now, CancellationToken cancellationToken)
    {
        (string previousHash, _) = await AppendAuditAsync(connection, transaction, run.ScenarioId,
            "stage.awaiting-dependency", run.RunId, CanonicalJson.Serialize(new { stage = stage.ToString(), code, inputHash = DeterministicIdentity.Sha256(input) }), cancellationToken);
        await using SqliteCommand update = Command(connection, transaction, """
            UPDATE RunStages SET InputJson = $input, InputHash = $inputHash, OutputJson = NULL, OutputHash = NULL,
                AttemptCount = AttemptCount + 1, Status = $status, StartedUtc = $startedUtc, EndedUtc = NULL,
                Diagnostics = $diagnostics, PreviousAuditHash = $previousHash
            WHERE RunId = $runId AND Stage = $stage;
            """);
        Add(update, "$input", input); Add(update, "$inputHash", DeterministicIdentity.Sha256(input));
        Add(update, "$status", StageStatus.AwaitingDependency.ToString()); Add(update, "$startedUtc", Format(now));
        Add(update, "$diagnostics", code); Add(update, "$previousHash", previousHash); Add(update, "$runId", run.RunId); Add(update, "$stage", (int)stage);
        await update.ExecuteNonQueryAsync(cancellationToken);
        await UpdateRunAsync(connection, transaction, run.RunId, RunStatus.AwaitingDependency, stage, now, code, null, cancellationToken);
    }

    private async Task CompleteStageAsync(SqliteConnection connection, SqliteTransaction transaction,
        RunResponse run, RunStageKind stage, string input, string output, string commitmentHash, DateTimeOffset now, CancellationToken cancellationToken)
    {
        (string previousHash, _) = await AppendAuditAsync(connection, transaction, run.ScenarioId,
            "stage.completed", run.RunId,
            CanonicalJson.Serialize(new { stage = stage.ToString(), commitmentHash, inputHash = DeterministicIdentity.Sha256(input), outputHash = DeterministicIdentity.Sha256(output) }),
            cancellationToken);
        await using SqliteCommand update = Command(connection, transaction, """
            UPDATE RunStages SET InputJson = $input, InputHash = $inputHash,
                OutputJson = $output, OutputHash = $outputHash, AttemptCount = AttemptCount + 1,
                Status = $status, StartedUtc = COALESCE(StartedUtc, $startedUtc), EndedUtc = $endedUtc,
                Diagnostics = NULL, PreviousAuditHash = $previousHash
            WHERE RunId = $runId AND Stage = $stage;
            """);
        Add(update, "$input", input);
        Add(update, "$inputHash", DeterministicIdentity.Sha256(input));
        Add(update, "$output", output);
        Add(update, "$outputHash", DeterministicIdentity.Sha256(output));
        Add(update, "$status", StageStatus.Completed.ToString());
        Add(update, "$startedUtc", Format(now));
        Add(update, "$endedUtc", Format(now));
        Add(update, "$previousHash", previousHash);
        Add(update, "$runId", run.RunId);
        Add(update, "$stage", (int)stage);
        await update.ExecuteNonQueryAsync(cancellationToken);
        await UpdateRunAsync(connection, transaction, run.RunId, RunStatus.Running, stage, now, null, null, cancellationToken);
    }

    private async Task FailStageAsync(SqliteConnection connection, SqliteTransaction transaction,
        RunResponse run, RunStageKind stage, string code, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        (string previousHash, _) = await AppendAuditAsync(connection, transaction, run.ScenarioId,
            "stage.failed", run.RunId, CanonicalJson.Serialize(new { stage = stage.ToString(), code }), cancellationToken);
        await using SqliteCommand update = Command(connection, transaction, """
            UPDATE RunStages SET AttemptCount = AttemptCount + 1, Status = $status,
                StartedUtc = COALESCE(StartedUtc, $time), EndedUtc = $time,
                Diagnostics = $diagnostics, PreviousAuditHash = $previousHash
            WHERE RunId = $runId AND Stage = $stage;
            """);
        Add(update, "$status", StageStatus.Failed.ToString());
        Add(update, "$time", Format(now));
        Add(update, "$diagnostics", code);
        Add(update, "$previousHash", previousHash);
        Add(update, "$runId", run.RunId);
        Add(update, "$stage", (int)stage);
        await update.ExecuteNonQueryAsync(cancellationToken);
        await UpdateRunAsync(connection, transaction, run.RunId, RunStatus.Failed, stage, now, code, now, cancellationToken);
    }

    private static async Task UpdateRunAsync(SqliteConnection connection, SqliteTransaction transaction,
        string runId, RunStatus status, RunStageKind? stage, DateTimeOffset updatedUtc,
        string? diagnosticCode, DateTimeOffset? endedUtc, CancellationToken cancellationToken)
    {
        await using SqliteCommand update = Command(connection, transaction, """
            UPDATE Runs SET Status = $status, CurrentStage = $currentStage, UpdatedUtc = $updatedUtc,
                DiagnosticCode = $diagnosticCode, EndedUtc = $endedUtc WHERE RunId = $runId;
            """);
        Add(update, "$status", status.ToString());
        Add(update, "$currentStage", stage is null ? null : (int)stage.Value);
        Add(update, "$updatedUtc", Format(updatedUtc));
        Add(update, "$diagnosticCode", diagnosticCode);
        Add(update, "$endedUtc", endedUtc is null ? null : Format(endedUtc.Value));
        Add(update, "$runId", runId);
        await update.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<(string PreviousHash, string EntryHash)> AppendAuditAsync(
        SqliteConnection connection, SqliteTransaction transaction, string scenarioId,
        string action, string subjectId, string dataJson, CancellationToken cancellationToken)
    {
        long sequence;
        string previousHash;
        await using (SqliteCommand prior = Command(connection, transaction, """
            SELECT Sequence, EntryHash FROM AuditEntries
            WHERE ScenarioId = $scenarioId ORDER BY Sequence DESC LIMIT 1;
            """))
        {
            Add(prior, "$scenarioId", scenarioId);
            await using SqliteDataReader reader = await prior.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                sequence = reader.GetInt64(0) + 1;
                previousHash = reader.GetString(1);
            }
            else
            {
                sequence = 1;
                previousHash = EmptyHash;
            }
        }
        string canonicalData = CanonicalizeStoredJson(dataJson);
        string dataHash = DeterministicIdentity.Sha256(canonicalData);
        string createdUtc = NowText();
        string entryHash = AuditHash(scenarioId, sequence, action, subjectId, dataHash, previousHash, createdUtc);
        string auditId = DeterministicIdentity.Create("audit", scenarioId,
            sequence.ToString(CultureInfo.InvariantCulture), entryHash);
        await using SqliteCommand insert = Command(connection, transaction, """
            INSERT INTO AuditEntries
                (AuditId, ScenarioId, Sequence, Action, SubjectId, DataJson, DataHash,
                 PreviousHash, EntryHash, CreatedUtc)
            VALUES ($auditId, $scenarioId, $sequence, $action, $subjectId, $dataJson, $dataHash,
                    $previousHash, $entryHash, $createdUtc);
            """);
        Add(insert, "$auditId", auditId);
        Add(insert, "$scenarioId", scenarioId);
        Add(insert, "$sequence", sequence);
        Add(insert, "$action", action);
        Add(insert, "$subjectId", subjectId);
        Add(insert, "$dataJson", canonicalData);
        Add(insert, "$dataHash", dataHash);
        Add(insert, "$previousHash", previousHash);
        Add(insert, "$entryHash", entryHash);
        Add(insert, "$createdUtc", createdUtc);
        await insert.ExecuteNonQueryAsync(cancellationToken);
        return (previousHash, entryHash);
    }

    private static string AuditHash(string scenarioId, long sequence, string action, string subjectId,
        string dataHash, string previousHash, string createdUtc) => DeterministicIdentity.Sha256(string.Join("\n",
            "audit-v1", scenarioId, sequence.ToString(CultureInfo.InvariantCulture), action, subjectId,
            dataHash, previousHash, createdUtc));

    private static async Task<ApiOutcome?> ReadIdempotencyAsync(SqliteConnection connection,
        SqliteTransaction transaction, string key, string route, string requestHash, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT Route, RequestHash, ResponseStatus, ResponseBody, ResponseHash, Location
            FROM IdempotencyRecords WHERE IdempotencyKey = $key;
            """);
        Add(command, "$key", key);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        if (!StringComparer.Ordinal.Equals(reader.GetString(0), route) ||
            !StringComparer.Ordinal.Equals(reader.GetString(1), requestHash))
            return Problem(409, "Idempotency-Key was already used for a different route or request");
        string responseBody = reader.GetString(3);
        if (!StringComparer.Ordinal.Equals(reader.GetString(4), DeterministicIdentity.Sha256(responseBody)))
            throw new PersistenceIntegrityException("Stored idempotency response hash verification failed.");
        return new ApiOutcome(reader.GetInt32(2), responseBody, reader.IsDBNull(5) ? null : reader.GetString(5), true);
    }

    private static async Task<TruthBindingResponse?> ReadBindingAsync(SqliteConnection connection,
        SqliteTransaction? transaction, string scenarioId, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT BindingId, ScenarioId, ApprovedPredictionHash, SourcePackageSha256, WorldId,
                   WorldModelVersion, CalibrationArtifactId, CalibrationArtifactSha256, CreatedUtc,
                   CanonicalInputJson, InputHash
            FROM TruthBindings WHERE ScenarioId = $scenarioId;
            """);
        Add(command, "$scenarioId", scenarioId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var binding = new TruthBindingResponse(reader.GetString(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6),
            reader.GetString(7), Parse(reader.GetString(8)));
        string canonical = CanonicalJson.Serialize(ToRequest(binding));
        string inputHash = DeterministicIdentity.Sha256(canonical);
        if (!StringComparer.Ordinal.Equals(reader.GetString(9), canonical) ||
            !StringComparer.Ordinal.Equals(reader.GetString(10), inputHash) ||
            !StringComparer.Ordinal.Equals(binding.BindingId, DeterministicIdentity.Create("truth-binding-v1", canonical)))
            throw new PersistenceIntegrityException("Stored truth binding integrity verification failed.");
        return binding;
    }

    private static async Task<RunResponse?> ReadRunAsync(SqliteConnection connection,
        SqliteTransaction? transaction, string runId, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT RunId, ScenarioId, Status, CurrentStage, CreatedUtc, UpdatedUtc, EndedUtc, DiagnosticCode
            FROM Runs WHERE RunId = $runId;
            """);
        Add(command, "$runId", runId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new RunResponse(reader.GetString(0), reader.GetString(1),
            Enum.Parse<RunStatus>(reader.GetString(2)),
            reader.IsDBNull(3) ? null : (RunStageKind)reader.GetInt32(3),
            Parse(reader.GetString(4)), Parse(reader.GetString(5)),
            reader.IsDBNull(6) ? null : Parse(reader.GetString(6)),
            reader.IsDBNull(7) ? null : reader.GetString(7));
    }

    private static async Task<IReadOnlyList<StageResponse>> ReadStagesAsync(SqliteConnection connection,
        SqliteTransaction? transaction, string runId, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT Stage, Name, Status, AttemptCount, StartedUtc, EndedUtc, Diagnostics
            FROM RunStages WHERE RunId = $runId ORDER BY Stage;
            """);
        Add(command, "$runId", runId);
        var stages = new List<StageResponse>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            stages.Add(new StageResponse((RunStageKind)reader.GetInt32(0), reader.GetString(1),
                Enum.Parse<StageStatus>(reader.GetString(2)), reader.GetInt32(3),
                reader.IsDBNull(4) ? null : Parse(reader.GetString(4)),
                reader.IsDBNull(5) ? null : Parse(reader.GetString(5)),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        return stages;
    }

    private static async Task<(string PlanId, string PlanHash)> ReadRunPlanAsync(SqliteConnection connection,
        SqliteTransaction transaction, string runId, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction,
            "SELECT PlanArtifactId, PlanArtifactSha256 FROM Runs WHERE RunId = $runId;");
        Add(command, "$runId", runId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Run disappeared during execution.");
        return (reader.GetString(0), reader.GetString(1));
    }

    private static async Task<bool> HasActiveRunAsync(SqliteConnection connection, SqliteTransaction transaction,
        string scenarioId, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = Command(connection, transaction, """
            SELECT 1 FROM Runs WHERE ScenarioId = $scenarioId
            AND Status NOT IN ($cancelled, $failed, $scored) LIMIT 1;
            """);
        Add(command, "$scenarioId", scenarioId);
        Add(command, "$cancelled", RunStatus.Cancelled.ToString());
        Add(command, "$failed", RunStatus.Failed.ToString());
        Add(command, "$scored", RunStatus.Scored.ToString());
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    private static async Task<IReadOnlyList<AuditResponse>> ReadAuditAsync(SqliteConnection connection,
        string scenarioId, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT AuditId, ScenarioId, Sequence, Action, SubjectId, DataHash, PreviousHash, EntryHash, CreatedUtc
            FROM AuditEntries WHERE ScenarioId = $scenarioId ORDER BY Sequence;
            """;
        Add(command, "$scenarioId", scenarioId);
        var entries = new List<AuditResponse>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            entries.Add(new AuditResponse(reader.GetString(0), reader.GetString(1), reader.GetInt64(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6),
                reader.GetString(7), Parse(reader.GetString(8))));
        return entries;
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqliteCommand pragmas = connection.CreateCommand();
        pragmas.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;";
        await pragmas.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? transaction, string text)
    {
        SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = text;
        return command;
    }

    private static void Add(SqliteCommand command, string name, object? value) =>
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);

    private string NowText() => Format(timeProvider.GetUtcNow());
    private static string Format(DateTimeOffset value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static DateTimeOffset Parse(string value) => DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    private static string NormalizeHash(string value) => value.ToLowerInvariant();
    private static string CanonicalizeStoredJson(string value)
    {
        using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(value);
        return CanonicalJson.Canonicalize(document.RootElement);
    }
    private static BindWorldRequest ToRequest(TruthBindingResponse value) => new(value.ScenarioId,
        value.ApprovedSealedPredictionHash, value.SourcePackageSha256, value.WorldId,
        value.WorldModelVersion, value.CalibrationArtifactId, value.CalibrationArtifactSha256);
    private static string StageName(RunStageKind stage) => stage switch
    {
        RunStageKind.S0BindWorld => "BindWorld",
        RunStageKind.S1MaterializePlan => "MaterializePlan",
        RunStageKind.S2ExecuteDrilling => "ExecuteDrilling",
        RunStageKind.S3GenerateSurvey => "GenerateSurvey",
        RunStageKind.S4SampleGeology => "SampleGeology",
        RunStageKind.S5GenerateLogs => "GenerateLogs",
        RunStageKind.S6DesignCompletion => "DesignCompletion",
        RunStageKind.S7RunProduction => "RunProduction",
        RunStageKind.S8PublishReveal => "PublishReveal",
        RunStageKind.S9Score => "Score",
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
    private static ApiOutcome Json<T>(int status, T value, string? location = null) =>
        new(status, CanonicalJson.Serialize(value), location);
    private static ApiOutcome Problem(int status, string title) => Json(status, new { status, title });
    private static ApiOutcome Validation(IReadOnlyDictionary<string, string[]> errors) =>
        Json(400, new { errors, status = 400, title = "Request validation failed" });

    private const string Schema = """
        CREATE TABLE IF NOT EXISTS TruthBindings (
            BindingId TEXT PRIMARY KEY,
            ScenarioId TEXT NOT NULL UNIQUE,
            ApprovedPredictionHash TEXT NOT NULL,
            SourcePackageSha256 TEXT NOT NULL,
            WorldId TEXT NOT NULL,
            WorldModelVersion TEXT NOT NULL,
            CalibrationArtifactId TEXT NOT NULL,
            CalibrationArtifactSha256 TEXT NOT NULL,
            CanonicalInputJson TEXT NOT NULL,
            InputHash TEXT NOT NULL,
            CreatedUtc TEXT NOT NULL
        );
        CREATE TRIGGER IF NOT EXISTS TR_TruthBindings_NoUpdate
            BEFORE UPDATE ON TruthBindings BEGIN
                SELECT RAISE(ABORT, 'Truth bindings are immutable');
            END;
        CREATE TRIGGER IF NOT EXISTS TR_TruthBindings_NoDelete
            BEFORE DELETE ON TruthBindings BEGIN
                SELECT RAISE(ABORT, 'Truth bindings are immutable');
            END;

        CREATE TABLE IF NOT EXISTS Runs (
            RunId TEXT PRIMARY KEY,
            ScenarioId TEXT NOT NULL,
            BindingId TEXT NOT NULL,
            ApprovedPredictionHash TEXT NOT NULL,
            PlanArtifactId TEXT NOT NULL,
            PlanArtifactSha256 TEXT NOT NULL,
            CanonicalInputJson TEXT NOT NULL,
            InputHash TEXT NOT NULL,
            Status TEXT NOT NULL,
            CurrentStage INTEGER NULL CHECK (CurrentStage IS NULL OR CurrentStage BETWEEN 0 AND 9),
            CreatedUtc TEXT NOT NULL,
            UpdatedUtc TEXT NOT NULL,
            EndedUtc TEXT NULL,
            DiagnosticCode TEXT NULL,
            PublicationCount INTEGER NOT NULL DEFAULT 0 CHECK (PublicationCount >= 0),
            ClockAdvanceCount INTEGER NOT NULL DEFAULT 0 CHECK (ClockAdvanceCount >= 0),
            FOREIGN KEY (BindingId) REFERENCES TruthBindings(BindingId)
        );
        CREATE UNIQUE INDEX IF NOT EXISTS UX_Runs_OneActiveScenario
            ON Runs(ScenarioId)
            WHERE Status NOT IN ('Cancelled', 'Failed', 'Scored');

        CREATE TABLE IF NOT EXISTS RunStages (
            RunId TEXT NOT NULL,
            Stage INTEGER NOT NULL CHECK (Stage BETWEEN 0 AND 9),
            Name TEXT NOT NULL,
            InputJson TEXT NULL,
            InputHash TEXT NULL,
            OutputJson TEXT NULL,
            OutputHash TEXT NULL,
            AttemptCount INTEGER NOT NULL DEFAULT 0 CHECK (AttemptCount >= 0),
            Status TEXT NOT NULL,
            StartedUtc TEXT NULL,
            EndedUtc TEXT NULL,
            Diagnostics TEXT NULL,
            PreviousAuditHash TEXT NOT NULL,
            PRIMARY KEY (RunId, Stage),
            FOREIGN KEY (RunId) REFERENCES Runs(RunId)
        );

        CREATE TABLE IF NOT EXISTS ObservationBatches (
            ObservationBatchId TEXT PRIMARY KEY,
            RunId TEXT NOT NULL,
            Stage INTEGER NOT NULL CHECK (Stage BETWEEN 0 AND 9),
            ObservationModelVersion TEXT NOT NULL,
            CanonicalInputJson TEXT NOT NULL,
            InputHash TEXT NOT NULL,
            CanonicalOutputJson TEXT NOT NULL,
            OutputHash TEXT NOT NULL,
            Status TEXT NOT NULL,
            CreatedUtc TEXT NOT NULL,
            CompletedUtc TEXT NULL,
            FOREIGN KEY (RunId) REFERENCES Runs(RunId)
        );

        CREATE TABLE IF NOT EXISTS RevealManifests (
            RevealManifestId TEXT PRIMARY KEY,
            RunId TEXT NOT NULL UNIQUE,
            ObservationBatchId TEXT NOT NULL,
            CanonicalManifestJson TEXT NOT NULL,
            ManifestHash TEXT NOT NULL,
            Status TEXT NOT NULL,
            AttemptCount INTEGER NOT NULL DEFAULT 0,
            CreatedUtc TEXT NOT NULL,
            PublishedUtc TEXT NULL,
            FOREIGN KEY (RunId) REFERENCES Runs(RunId),
            FOREIGN KEY (ObservationBatchId) REFERENCES ObservationBatches(ObservationBatchId)
        );

        CREATE TABLE IF NOT EXISTS AuditEntries (
            AuditId TEXT PRIMARY KEY,
            ScenarioId TEXT NOT NULL,
            Sequence INTEGER NOT NULL CHECK (Sequence > 0),
            Action TEXT NOT NULL,
            SubjectId TEXT NOT NULL,
            DataJson TEXT NOT NULL,
            DataHash TEXT NOT NULL,
            PreviousHash TEXT NOT NULL,
            EntryHash TEXT NOT NULL,
            CreatedUtc TEXT NOT NULL,
            UNIQUE (ScenarioId, Sequence),
            UNIQUE (ScenarioId, EntryHash)
        );
        CREATE TRIGGER IF NOT EXISTS TR_AuditEntries_NoUpdate
            BEFORE UPDATE ON AuditEntries BEGIN
                SELECT RAISE(ABORT, 'Audit entries are append-only');
            END;
        CREATE TRIGGER IF NOT EXISTS TR_AuditEntries_NoDelete
            BEFORE DELETE ON AuditEntries BEGIN
                SELECT RAISE(ABORT, 'Audit entries are append-only');
            END;

        CREATE TABLE IF NOT EXISTS IdempotencyRecords (
            IdempotencyId TEXT PRIMARY KEY,
            IdempotencyKey TEXT NOT NULL UNIQUE,
            Route TEXT NOT NULL,
            RequestHash TEXT NOT NULL,
            CanonicalRequestJson TEXT NOT NULL,
            ResponseStatus INTEGER NOT NULL,
            ResponseBody TEXT NOT NULL,
            ResponseHash TEXT NOT NULL,
            Location TEXT NULL,
            CreatedUtc TEXT NOT NULL
        );
        """;
}




