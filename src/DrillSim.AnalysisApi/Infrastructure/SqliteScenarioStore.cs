using System.Globalization;
using DrillSim.AnalysisApi.Models;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed partial class SqliteScenarioStore
{
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS scenarios (
            scenario_id TEXT PRIMARY KEY,
            source_field_id TEXT NOT NULL,
            cloned_field_id TEXT NULL,
            reservoir_name TEXT NOT NULL,
            initial_as_of_utc TEXT NOT NULL,
            as_of_utc TEXT NOT NULL,
            seed_label TEXT NOT NULL,
            world_model_version TEXT NOT NULL,
            observation_model_version TEXT NOT NULL,
            scoring_model_version TEXT NOT NULL,
            status TEXT NOT NULL,
            assumptions_sha256 TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            modified_utc TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS evidence_visibility (
            scenario_id TEXT NOT NULL,
            evidence_id TEXT NOT NULL,
            record_kind TEXT NOT NULL,
            visible_from_utc TEXT NOT NULL,
            visible_until_utc TEXT NULL,
            reveal_id TEXT NULL,
            PRIMARY KEY (scenario_id, evidence_id),
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS scenario_package_snapshots (
            scenario_id TEXT PRIMARY KEY,
            field_id TEXT NOT NULL,
            as_of_utc TEXT NOT NULL,
            package_json TEXT NOT NULL,
            canonical_package_sha256 TEXT NOT NULL,
            origin TEXT NOT NULL CHECK (origin IN ('ScenarioCreation', 'LegacyBackfill')),
            created_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id)
        );

        CREATE TRIGGER IF NOT EXISTS tr_scenario_package_snapshots_no_update
        BEFORE UPDATE ON scenario_package_snapshots
        BEGIN SELECT RAISE(ABORT, 'Scenario package snapshot is immutable'); END;
        CREATE TRIGGER IF NOT EXISTS tr_scenario_package_snapshots_no_delete
        BEFORE DELETE ON scenario_package_snapshots
        BEGIN SELECT RAISE(ABORT, 'Scenario package snapshot is immutable'); END;

        CREATE TABLE IF NOT EXISTS scenario_clone_package_snapshots (
            scenario_id TEXT PRIMARY KEY,
            reveal_id TEXT NOT NULL UNIQUE,
            cloned_field_id TEXT NOT NULL,
            manifest_sha256 TEXT NOT NULL,
            valid_time_utc TEXT NOT NULL,
            package_json TEXT NOT NULL,
            canonical_package_sha256 TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id)
        );

        CREATE TRIGGER IF NOT EXISTS tr_scenario_clone_package_snapshots_no_update
        BEFORE UPDATE ON scenario_clone_package_snapshots
        BEGIN SELECT RAISE(ABORT, 'Scenario clone package snapshot is immutable'); END;
        CREATE TRIGGER IF NOT EXISTS tr_scenario_clone_package_snapshots_no_delete
        BEFORE DELETE ON scenario_clone_package_snapshots
        BEGIN SELECT RAISE(ABORT, 'Scenario clone package snapshot is immutable'); END;

        CREATE TABLE IF NOT EXISTS prediction_drafts (
            scenario_id TEXT PRIMARY KEY,
            body_json TEXT NOT NULL,
            revision INTEGER NOT NULL,
            created_utc TEXT NOT NULL,
            modified_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS prediction_seals (
            scenario_id TEXT PRIMARY KEY,
            body_json TEXT NOT NULL,
            sealed_sha256 TEXT NOT NULL,
            baselines_sha256 TEXT NOT NULL,
            sealed_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS prediction_approvals (
            scenario_id TEXT PRIMARY KEY,
            actor TEXT NOT NULL,
            approved_utc TEXT NOT NULL,
            sealed_sha256 TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES prediction_seals(scenario_id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS prediction_baselines (
            scenario_id TEXT NOT NULL,
            baseline_kind TEXT NOT NULL,
            baseline_id TEXT NOT NULL,
            content_json TEXT NOT NULL,
            content_sha256 TEXT NOT NULL,
            PRIMARY KEY (scenario_id, baseline_kind),
            UNIQUE (baseline_id),
            FOREIGN KEY (scenario_id) REFERENCES prediction_seals(scenario_id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS reveal_manifests (
            reveal_id TEXT PRIMARY KEY,
            scenario_id TEXT NOT NULL UNIQUE,
            run_id TEXT NOT NULL,
            cloned_field_id TEXT NOT NULL,
            valid_time_utc TEXT NOT NULL,
            observation_model_version TEXT NOT NULL,
            manifest_sha256 TEXT NOT NULL,
            evidence_json TEXT NOT NULL,
            evidence_count INTEGER NOT NULL,
            production_series_id TEXT NOT NULL UNIQUE,
            canonical_request_json TEXT NOT NULL,
            canonical_request_sha256 TEXT NOT NULL,
            receipt_status TEXT NOT NULL CHECK (receipt_status IN ('Prepared', 'Revealed')),
            created_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id)
        );

        CREATE TABLE IF NOT EXISTS public_production_series (
            series_id TEXT PRIMARY KEY,
            scenario_id TEXT NOT NULL UNIQUE,
            reveal_id TEXT NOT NULL UNIQUE,
            model_version TEXT NOT NULL,
            content_sha256 TEXT NOT NULL,
            month_count INTEGER NOT NULL,
            checkpoint_years_json TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id),
            FOREIGN KEY (reveal_id) REFERENCES reveal_manifests(reveal_id)
        );

        DROP TRIGGER IF EXISTS tr_reveal_manifests_no_update;
        CREATE TRIGGER tr_reveal_manifests_no_update
        BEFORE UPDATE ON reveal_manifests
        WHEN OLD.receipt_status <> 'Prepared'
          OR NEW.receipt_status <> 'Revealed'
          OR NEW.reveal_id <> OLD.reveal_id
          OR NEW.scenario_id <> OLD.scenario_id
          OR NEW.run_id <> OLD.run_id
          OR NEW.cloned_field_id <> OLD.cloned_field_id
          OR NEW.valid_time_utc <> OLD.valid_time_utc
          OR NEW.observation_model_version <> OLD.observation_model_version
          OR NEW.manifest_sha256 <> OLD.manifest_sha256
          OR NEW.evidence_json <> OLD.evidence_json
          OR NEW.evidence_count <> OLD.evidence_count
          OR NEW.production_series_id <> OLD.production_series_id
          OR NEW.canonical_request_json <> OLD.canonical_request_json
          OR NEW.canonical_request_sha256 <> OLD.canonical_request_sha256
          OR NEW.created_utc <> OLD.created_utc
        BEGIN SELECT RAISE(ABORT, 'Reveal manifest content is immutable'); END;
        CREATE TRIGGER IF NOT EXISTS tr_reveal_manifests_no_delete
        BEFORE DELETE ON reveal_manifests
        BEGIN SELECT RAISE(ABORT, 'Reveal manifest is immutable'); END;
        CREATE TRIGGER IF NOT EXISTS tr_public_production_series_no_update
        BEFORE UPDATE ON public_production_series
        BEGIN SELECT RAISE(ABORT, 'Public production series receipt is immutable'); END;
        CREATE TRIGGER IF NOT EXISTS tr_public_production_series_no_delete
        BEFORE DELETE ON public_production_series
        BEGIN SELECT RAISE(ABORT, 'Public production series receipt is immutable'); END;

        CREATE TABLE IF NOT EXISTS public_scorecards (
            scorecard_id TEXT PRIMARY KEY,
            scenario_id TEXT NOT NULL UNIQUE,
            run_id TEXT NOT NULL,
            reveal_id TEXT NOT NULL UNIQUE,
            scoring_model_version TEXT NOT NULL,
            input_sha256 TEXT NOT NULL,
            headline_metric TEXT NULL,
            metrics_json TEXT NOT NULL,
            created_valid_time_utc TEXT NOT NULL,
            limitation TEXT NULL,
            canonical_body_json TEXT NOT NULL,
            canonical_body_sha256 TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            FOREIGN KEY (scenario_id) REFERENCES scenarios(scenario_id),
            FOREIGN KEY (reveal_id) REFERENCES reveal_manifests(reveal_id)
        );

        CREATE TRIGGER IF NOT EXISTS tr_public_scorecards_no_update
        BEFORE UPDATE ON public_scorecards
        BEGIN SELECT RAISE(ABORT, 'Public scorecard is immutable'); END;
        CREATE TRIGGER IF NOT EXISTS tr_public_scorecards_no_delete
        BEFORE DELETE ON public_scorecards
        BEGIN SELECT RAISE(ABORT, 'Public scorecard is immutable'); END;

        CREATE INDEX IF NOT EXISTS ix_scenarios_status
            ON scenarios(status);
        CREATE INDEX IF NOT EXISTS ix_evidence_visibility_scenario_kind
            ON evidence_visibility(scenario_id, record_kind);
        """;

    private readonly string _connectionString;
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private bool _initialized;
    private readonly Action? _beforeRevealPrepareCommit;
    private readonly Action? _beforeRevealFinalizeCommit;
    private readonly Action? _beforeScorecardCommit;
    private readonly Action? _beforeScenarioCreateCommit;

    public SqliteScenarioStore(string connectionString)
        : this(connectionString, null, null, null, null)
    {
    }

    internal SqliteScenarioStore(string connectionString, Action beforeScenarioCreateCommit)
        : this(connectionString, null, null, null, beforeScenarioCreateCommit)
    {
    }

    internal SqliteScenarioStore(
        string connectionString,
        Action? beforeRevealPrepareCommit,
        Action? beforeRevealFinalizeCommit)
        : this(connectionString, beforeRevealPrepareCommit, beforeRevealFinalizeCommit, null, null)
    {
    }

    internal SqliteScenarioStore(
        string connectionString,
        Action? beforeRevealPrepareCommit,
        Action? beforeRevealFinalizeCommit,
        Action? beforeScorecardCommit)
        : this(connectionString, beforeRevealPrepareCommit, beforeRevealFinalizeCommit, beforeScorecardCommit, null)
    {
    }

    private SqliteScenarioStore(
        string connectionString,
        Action? beforeRevealPrepareCommit,
        Action? beforeRevealFinalizeCommit,
        Action? beforeScorecardCommit,
        Action? beforeScenarioCreateCommit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
        _beforeRevealPrepareCommit = beforeRevealPrepareCommit;
        _beforeRevealFinalizeCommit = beforeRevealFinalizeCommit;
        _beforeScorecardCommit = beforeScorecardCommit;
        _beforeScenarioCreateCommit = beforeScenarioCreateCommit;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        await _initializationGate.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
                return;

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await EnableForeignKeysAsync(connection, cancellationToken);
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = Schema;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            await EnsureInitialAsOfColumnAsync(connection, cancellationToken);
            await EnsurePredictionLedgerColumnsAsync(connection, cancellationToken);
            await ValidateScenarioTimesAsync(connection, cancellationToken);
            await ValidateScenarioPackageSnapshotsAsync(connection, cancellationToken);
            await ValidateClonePackageSnapshotsAsync(connection, cancellationToken);
            await ValidatePredictionRevisionsAsync(connection, cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initializationGate.Release();
        }
    }

    internal async Task<Scenario> CreateLegacyScenarioAsync(
        Scenario scenario,
        IReadOnlyCollection<EvidenceVisibility> visibility,
        CancellationToken cancellationToken = default) =>
        await CreateCoreAsync(scenario, visibility, null, null, cancellationToken);

    internal async Task<Scenario> CreateAsync(
        Scenario scenario,
        IReadOnlyCollection<EvidenceVisibility> visibility,
        AnalysisPackage sourcePackage,
        DateTimeOffset snapshotCreatedUtc,
        CancellationToken cancellationToken = default) =>
        await CreateCoreAsync(scenario, visibility, sourcePackage, snapshotCreatedUtc, cancellationToken);

    private async Task<Scenario> CreateCoreAsync(
        Scenario scenario,
        IReadOnlyCollection<EvidenceVisibility> visibility,
        AnalysisPackage? sourcePackage,
        DateTimeOffset? snapshotCreatedUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(visibility);
        if (visibility.Any(item => item.ScenarioId != scenario.ScenarioId))
            throw new ArgumentException("Every visibility row must belong to the scenario.", nameof(visibility));
        if (scenario.InitialAsOfUtc > scenario.AsOfUtc)
            throw new ArgumentException("InitialAsOfUtc cannot be later than AsOfUtc.", nameof(scenario));
        PreparedScenarioPackageSnapshot? preparedSnapshot = sourcePackage is null
            ? null
            : PrepareScenarioPackageSnapshot(
                scenario,
                sourcePackage,
                ScenarioPackageSnapshotOrigin.ScenarioCreation,
                snapshotCreatedUtc ?? throw new ArgumentNullException(nameof(snapshotCreatedUtc)));

        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);

        await using var scenarioCommand = connection.CreateCommand();
        scenarioCommand.Transaction = transaction;
        scenarioCommand.CommandText = """
            INSERT OR IGNORE INTO scenarios (
                scenario_id, source_field_id, cloned_field_id, reservoir_name, initial_as_of_utc, as_of_utc,
                seed_label, world_model_version, observation_model_version,
                scoring_model_version, status, assumptions_sha256, created_utc, modified_utc)
            VALUES (
                $scenario_id, $source_field_id, $cloned_field_id, $reservoir_name, $initial_as_of_utc, $as_of_utc,
                $seed_label, $world_model_version, $observation_model_version,
                $scoring_model_version, $status, $assumptions_sha256, $created_utc, $modified_utc);
            """;
        AddScenarioParameters(scenarioCommand, scenario);
        bool created = await scenarioCommand.ExecuteNonQueryAsync(cancellationToken) == 1;

        if (created)
        {
            foreach (EvidenceVisibility item in visibility
                .DistinctBy(item => item.EvidenceId, StringComparer.Ordinal)
                .OrderBy(item => item.RecordKind, StringComparer.Ordinal)
                .ThenBy(item => item.EvidenceId, StringComparer.Ordinal))
            {
                await InsertVisibilityAsync(connection, transaction, item, cancellationToken);
            }
            if (preparedSnapshot is not null)
                await InsertScenarioPackageSnapshotAsync(connection, transaction, preparedSnapshot, cancellationToken);
        }
        else if (preparedSnapshot is not null)
        {
            ScenarioPackageSnapshot? existingSnapshot = await FindScenarioPackageSnapshotAsync(
                connection,
                transaction,
                scenario,
                cancellationToken);
            if (existingSnapshot is not null)
                EnsureEquivalentSnapshot(existingSnapshot, preparedSnapshot);
        }

        if (created)
            _beforeScenarioCreateCommit?.Invoke();
        transaction.Commit();
        Scenario stored = await GetRequiredAsync(connection, scenario.ScenarioId, cancellationToken);
        return stored;
    }

    public async Task<IReadOnlyList<Scenario>> ListAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT scenario_id, source_field_id, cloned_field_id, reservoir_name, initial_as_of_utc, as_of_utc,
                   seed_label, world_model_version, observation_model_version,
                   scoring_model_version, status, assumptions_sha256, created_utc, modified_utc
            FROM scenarios
            ORDER BY created_utc, scenario_id;
            """;

        var scenarios = new List<Scenario>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            scenarios.Add(ReadScenario(reader));
        return scenarios;
    }

    public async Task<Scenario?> FindAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        return await FindAsync(connection, scenarioId, cancellationToken);
    }

    public async Task<IReadOnlyList<EvidenceVisibility>> GetEvidenceVisibilityAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        return await ReadEvidenceVisibilityAsync(connection, null, scenarioId, cancellationToken);
    }

    private static async Task<IReadOnlyList<EvidenceVisibility>> ReadEvidenceVisibilityAsync(
        SqliteConnection connection, SqliteTransaction? transaction, Guid scenarioId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, evidence_id, record_kind, visible_from_utc, visible_until_utc, reveal_id
            FROM evidence_visibility
            WHERE scenario_id = $scenario_id
            ORDER BY record_kind, evidence_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));

        var items = new List<EvidenceVisibility>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EvidenceVisibility(
                ParseGuid(reader.GetString(0)),
                reader.GetString(1),
                reader.GetString(2),
                ParseInstant(reader.GetString(3)),
                reader.IsDBNull(4) ? null : ParseInstant(reader.GetString(4)),
                reader.IsDBNull(5) ? null : ParseGuid(reader.GetString(5))));
        }
        return items;
    }

    public async Task<PredictionRecord?> FindPredictionAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        PredictionRecord? prediction = await FindPredictionAsync(connection, transaction, scenarioId, cancellationToken);
        transaction.Commit();
        return prediction;
    }

    public async Task<PredictionRecord> SavePredictionDraftAsync(
        Guid scenarioId,
        string bodyJson,
        int? expectedRevision,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        ScenarioStatus status = await GetScenarioStatusAsync(connection, transaction, scenarioId, cancellationToken);
        if (status is not (ScenarioStatus.Draft or ScenarioStatus.Armed or ScenarioStatus.PredictionDrafted))
            throw Conflict("Prediction is sealed", "Prediction drafts cannot be changed after sealing.");
        PredictionBody submitted = PredictionJson.Deserialize<PredictionBody>(bodyJson, "prediction body");
        Services.PredictionValidator.EnsureSealable(submitted);
        if (submitted.AnalysisBinding is not null)
            await ValidateConfiguredPredictionAsync(connection, transaction, scenarioId, submitted, null, cancellationToken);

        (string BodyJson, int Revision)? existing =
            await GetDraftStateAsync(connection, transaction, scenarioId, cancellationToken);
        if (existing is not null && string.Equals(existing.Value.BodyJson, bodyJson, StringComparison.Ordinal))
        {
            if (status is not ScenarioStatus.PredictionDrafted)
            {
                await UpdateScenarioStatusAsync(
                    connection,
                    transaction,
                    scenarioId,
                    ScenarioStatus.PredictionDrafted,
                    now,
                    [ScenarioStatus.Draft, ScenarioStatus.Armed],
                    cancellationToken);
            }
            transaction.Commit();
            return await GetRequiredPredictionSnapshotAsync(connection, scenarioId, cancellationToken);
        }

        if (existing is null)
        {
            if (expectedRevision is not null)
                throw Conflict("Prediction revision conflict", "A new prediction draft cannot match an existing revision.");
            await using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO prediction_drafts (scenario_id, body_json, revision, created_utc, modified_utc)
                VALUES ($scenario_id, $body_json, 1, $created_utc, $modified_utc);
                """;
            insertCommand.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
            insertCommand.Parameters.AddWithValue("$body_json", bodyJson);
            insertCommand.Parameters.AddWithValue("$created_utc", FormatInstant(now));
            insertCommand.Parameters.AddWithValue("$modified_utc", FormatInstant(now));
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            if (expectedRevision is null || expectedRevision.Value != existing.Value.Revision)
            {
                throw Conflict(
                    "Prediction revision conflict",
                    $"Changed draft requires expected revision {existing.Value.Revision}.");
            }
            await using var updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = """
                UPDATE prediction_drafts
                SET body_json = $body_json,
                    revision = revision + 1,
                    modified_utc = $modified_utc
                WHERE scenario_id = $scenario_id AND revision = $expected_revision;
                """;
            updateCommand.Parameters.AddWithValue("$body_json", bodyJson);
            updateCommand.Parameters.AddWithValue("$modified_utc", FormatInstant(now));
            updateCommand.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
            updateCommand.Parameters.AddWithValue("$expected_revision", expectedRevision.Value);
            if (await updateCommand.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw Conflict("Prediction revision conflict", "The prediction draft was updated concurrently.");
        }

        await UpdateScenarioStatusAsync(
            connection,
            transaction,
            scenarioId,
            ScenarioStatus.PredictionDrafted,
            now,
            [ScenarioStatus.Draft, ScenarioStatus.Armed, ScenarioStatus.PredictionDrafted],
            cancellationToken);
        transaction.Commit();
        return await GetRequiredPredictionSnapshotAsync(connection, scenarioId, cancellationToken);
    }
    public async Task<PredictionRecord> SealPredictionAsync(
        Guid scenarioId,
        string bodyJson,
        string sealedSha256,
        IReadOnlyList<BaselineSnapshot> baselines,
        DateTimeOffset now,
        CancellationToken cancellationToken = default,
        int? expectedRevision = null)
    {
        Services.PredictionValidator.EnsureSealable(
            PredictionJson.Deserialize<PredictionBody>(bodyJson, "prediction body"));
        if (baselines.Count != 4 || baselines.Select(item => item.Kind).Distinct().Count() != 4 ||
            baselines.Select(item => item.BaselineId).Distinct().Count() != 4 ||
            baselines.Any(item => item.ScenarioId != scenarioId || !Enum.IsDefined(item.Kind)))
        {
            throw new ArgumentException("Exactly four unique baselines belonging to the scenario are required.", nameof(baselines));
        }
        foreach (BaselineSnapshot baseline in baselines)
            BaselineIntegrity.Validate(baseline);
        string baselinesSha256 = BaselineIntegrity.ComputeBundleSha256(baselines);

        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        ScenarioStatus status = await GetScenarioStatusAsync(connection, transaction, scenarioId, cancellationToken);
        (string BodyJson, int Revision)? currentDraft =
            await GetDraftStateAsync(connection, transaction, scenarioId, cancellationToken);
        if (expectedRevision is not null && expectedRevision != currentDraft?.Revision)
            throw Conflict("Prediction revision conflict", "The reviewed draft revision changed before sealing.");
        (string BodyJson, string Sha256, string BaselinesSha256)? existingSeal =
            await GetSealIdentityAsync(connection, transaction, scenarioId, cancellationToken);
        if (existingSeal is not null)
        {
            if (!string.Equals(existingSeal.Value.BodyJson, bodyJson, StringComparison.Ordinal) ||
                !string.Equals(existingSeal.Value.Sha256, sealedSha256, StringComparison.Ordinal) ||
                !string.Equals(existingSeal.Value.BaselinesSha256, baselinesSha256, StringComparison.Ordinal))
            {
                throw Conflict("Prediction seal conflict", "The prediction is already sealed with different content.");
            }
            transaction.Commit();
            return await GetRequiredPredictionSnapshotAsync(connection, scenarioId, cancellationToken);
        }
        if (status is not ScenarioStatus.PredictionDrafted)
            throw Conflict("Invalid scenario status", $"Scenario status {status} cannot transition to PredictionSealed.");

        if (currentDraft is null)
            throw Conflict("Prediction not found", "A prediction draft is required before sealing.");
        if (!string.Equals(currentDraft.Value.BodyJson, bodyJson, StringComparison.Ordinal))
            throw Conflict("Prediction draft changed", "The prediction draft changed while it was being sealed; retry with the current draft.");

        PredictionBody submitted = PredictionJson.Deserialize<PredictionBody>(bodyJson, "prediction body");
        if (submitted.AnalysisBinding is not null)
        {
            if (PredictionJson.Canonicalize(submitted) != bodyJson || PredictionJson.ComputeSha256(submitted) != sealedSha256)
                throw Conflict("Prediction seal mismatch", "The submitted configured seal does not match its canonical body.");
            await ValidateConfiguredPredictionAsync(connection, transaction, scenarioId, submitted, baselines, cancellationToken);
        }

        await using (var sealCommand = connection.CreateCommand())
        {
            sealCommand.Transaction = transaction;
            sealCommand.CommandText = """
                INSERT INTO prediction_seals (scenario_id, body_json, sealed_sha256, baselines_sha256, sealed_utc)
                VALUES ($scenario_id, $body_json, $sealed_sha256, $baselines_sha256, $sealed_utc);
                """;
            sealCommand.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
            sealCommand.Parameters.AddWithValue("$body_json", bodyJson);
            sealCommand.Parameters.AddWithValue("$sealed_sha256", sealedSha256);
            sealCommand.Parameters.AddWithValue("$baselines_sha256", baselinesSha256);
            sealCommand.Parameters.AddWithValue("$sealed_utc", FormatInstant(now));
            await sealCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        foreach (BaselineSnapshot baseline in baselines.OrderBy(item => item.Kind))
        {
            await using var baselineCommand = connection.CreateCommand();
            baselineCommand.Transaction = transaction;
            baselineCommand.CommandText = """
                INSERT INTO prediction_baselines (scenario_id, baseline_kind, baseline_id, content_json, content_sha256)
                VALUES ($scenario_id, $baseline_kind, $baseline_id, $content_json, $content_sha256);
                """;
            baselineCommand.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
            baselineCommand.Parameters.AddWithValue("$baseline_kind", baseline.Kind.ToString());
            baselineCommand.Parameters.AddWithValue("$baseline_id", FormatGuid(baseline.BaselineId));
            baselineCommand.Parameters.AddWithValue("$content_json", PredictionJson.Canonicalize(baseline));
            baselineCommand.Parameters.AddWithValue("$content_sha256", baseline.ContentSha256);
            await baselineCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        await UpdateScenarioStatusAsync(
            connection,
            transaction,
            scenarioId,
            ScenarioStatus.PredictionSealed,
            now,
            [ScenarioStatus.PredictionDrafted],
            cancellationToken);
        transaction.Commit();
        return await GetRequiredPredictionSnapshotAsync(connection, scenarioId, cancellationToken);
    }

    public async Task<PredictionRecord> ApprovePredictionAsync(
        Guid scenarioId,
        string actor,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
        await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        ScenarioStatus status = await GetScenarioStatusAsync(connection, transaction, scenarioId, cancellationToken);
        (string Actor, string SealedSha256)? existingApproval =
            await GetApprovalIdentityAsync(connection, transaction, scenarioId, cancellationToken);
        (string BodyJson, string Sha256, string BaselinesSha256)? seal =
            await GetSealIdentityAsync(connection, transaction, scenarioId, cancellationToken);
        if (seal is null)
            throw Conflict("Prediction is not sealed", "A sealed prediction is required before approval.");
        PredictionBody sealedBody = PredictionJson.Deserialize<PredictionBody>(seal.Value.BodyJson, "sealed prediction body");
        if (sealedBody.AnalysisBinding is not null)
            _ = await FindPredictionAsync(connection, transaction, scenarioId, cancellationToken)
                ?? throw Conflict("Prediction not found", "The configured prediction seal could not be verified.");
        if (existingApproval is not null)
        {
            if (!string.Equals(existingApproval.Value.Actor, actor, StringComparison.Ordinal) ||
                !string.Equals(existingApproval.Value.SealedSha256, seal.Value.Sha256, StringComparison.Ordinal))
            {
                throw Conflict("Prediction approval conflict", "The prediction was already approved with different actor semantics.");
            }
            transaction.Commit();
            return await GetRequiredPredictionSnapshotAsync(connection, scenarioId, cancellationToken);
        }
        if (status is not ScenarioStatus.PredictionSealed)
            throw Conflict("Invalid scenario status", $"Scenario status {status} cannot transition to HumanApproved.");

        await using (var approvalCommand = connection.CreateCommand())
        {
            approvalCommand.Transaction = transaction;
            approvalCommand.CommandText = """
                INSERT INTO prediction_approvals (scenario_id, actor, approved_utc, sealed_sha256)
                VALUES ($scenario_id, $actor, $approved_utc, $sealed_sha256);
                """;
            approvalCommand.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
            approvalCommand.Parameters.AddWithValue("$actor", actor);
            approvalCommand.Parameters.AddWithValue("$approved_utc", FormatInstant(now));
            approvalCommand.Parameters.AddWithValue("$sealed_sha256", seal.Value.Sha256);
            await approvalCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        await UpdateScenarioStatusAsync(
            connection,
            transaction,
            scenarioId,
            ScenarioStatus.HumanApproved,
            now,
            [ScenarioStatus.PredictionSealed],
            cancellationToken);
        transaction.Commit();
        return await GetRequiredPredictionSnapshotAsync(connection, scenarioId, cancellationToken);
    }

    private static async Task<ScenarioStatus> GetScenarioStatusAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT status FROM scenarios WHERE scenario_id = $scenario_id;";
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null)
        {
            throw new ScenarioApiException(
                StatusCodes.Status404NotFound,
                "Scenario not found",
                $"Scenario {scenarioId:D} does not exist.");
        }
        string statusText = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        if (!Enum.TryParse(statusText, ignoreCase: false, out ScenarioStatus status))
            throw new InvalidDataException($"Scenario {scenarioId:D} has unsupported status '{statusText}'.");
        return status;
    }

    private static async Task UpdateScenarioStatusAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        ScenarioStatus target,
        DateTimeOffset now,
        IReadOnlyList<ScenarioStatus> allowedCurrentStatuses,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        string allowedParameters = string.Join(", ", allowedCurrentStatuses.Select((_, index) => $"$allowed_{index}"));
        command.CommandText = $"""
            UPDATE scenarios
            SET status = $target_status, modified_utc = $modified_utc
            WHERE scenario_id = $scenario_id AND status IN ({allowedParameters});
            """;
        command.Parameters.AddWithValue("$target_status", target.ToString());
        command.Parameters.AddWithValue("$modified_utc", FormatInstant(now));
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        for (int index = 0; index < allowedCurrentStatuses.Count; index++)
            command.Parameters.AddWithValue($"$allowed_{index}", allowedCurrentStatuses[index].ToString());
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw Conflict("Scenario status conflict", $"Scenario {scenarioId:D} could not transition to {target}.");
    }

    private static async Task<(string BodyJson, int Revision)?> GetDraftStateAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT body_json, revision FROM prediction_drafts WHERE scenario_id = $scenario_id;";
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? (reader.GetString(0), reader.GetInt32(1))
            : null;
    }

    private static async Task<(string BodyJson, string Sha256, string BaselinesSha256)?> GetSealIdentityAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT body_json, sealed_sha256, baselines_sha256
            FROM prediction_seals
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? (reader.GetString(0), reader.GetString(1), reader.GetString(2))
            : null;
    }

    private static async Task<(string Actor, string SealedSha256)?> GetApprovalIdentityAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT actor, sealed_sha256
            FROM prediction_approvals
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? (reader.GetString(0), reader.GetString(1))
            : null;
    }

    private static async Task<PredictionRecord?> FindPredictionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        PredictionBody body;
        int revision;
        DateTimeOffset createdUtc;
        DateTimeOffset modifiedUtc;
        PredictionSeal? seal = null;
        PredictionApproval? approval = null;
        string scenarioIdText = FormatGuid(scenarioId);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                SELECT d.body_json, d.revision, d.created_utc, d.modified_utc,
                       s.body_json, s.sealed_sha256, s.baselines_sha256, s.sealed_utc,
                       a.actor, a.approved_utc, a.sealed_sha256
                FROM prediction_drafts d
                LEFT JOIN prediction_seals s ON s.scenario_id = d.scenario_id
                LEFT JOIN prediction_approvals a ON a.scenario_id = d.scenario_id
                WHERE d.scenario_id = $scenario_id;
                """;
            command.Parameters.AddWithValue("$scenario_id", scenarioIdText);
            await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;

            string bodyJson = reader.GetString(0);
            body = PredictionJson.Deserialize<PredictionBody>(bodyJson, "prediction body");
            revision = reader.GetInt32(1);
            if (revision < 1)
                throw new InvalidDataException($"Scenario {scenarioId:D} has an invalid prediction revision.");
            createdUtc = ReadStoredInstant(reader, 2, "prediction created_utc", scenarioIdText);
            modifiedUtc = ReadStoredInstant(reader, 3, "prediction modified_utc", scenarioIdText);
            if (!reader.IsDBNull(4))
            {
                string sealedBodyJson = reader.GetString(4);
                string sealedSha256 = reader.GetString(5);
                string baselinesSha256 = reader.GetString(6);
                if (!string.Equals(bodyJson, sealedBodyJson, StringComparison.Ordinal) ||
                    !string.Equals(PredictionJson.ComputeSha256(body), sealedSha256, StringComparison.Ordinal))
                {
                    throw new InvalidDataException($"Scenario {scenarioId:D} has inconsistent immutable prediction seal data.");
                }
                seal = new PredictionSeal(
                    sealedSha256,
                    baselinesSha256,
                    ReadStoredInstant(reader, 7, "sealed_utc", scenarioIdText));
            }
            if (!reader.IsDBNull(8))
            {
                if (seal is null || !string.Equals(reader.GetString(10), seal.Sha256, StringComparison.Ordinal))
                    throw new InvalidDataException($"Scenario {scenarioId:D} has an approval for a different prediction seal.");
                approval = new PredictionApproval(
                    reader.GetString(8),
                    ReadStoredInstant(reader, 9, "approved_utc", scenarioIdText),
                    reader.GetString(10));
            }
        }

        IReadOnlyList<BaselineSnapshot> baselines =
            await ReadBaselinesAsync(connection, transaction, scenarioId, cancellationToken);
        if ((seal is null && baselines.Count != 0) || (seal is not null && baselines.Count != 4))
            throw new InvalidDataException($"Scenario {scenarioId:D} has an inconsistent baseline snapshot count.");
        if (seal is not null && body.AnalysisBinding is null &&
            (body.CandidateId.StartsWith(Services.PetrophysicsAnalysisService.ConfiguredCandidatePrefix, StringComparison.Ordinal) ||
             baselines.Any(item => item.AnalysisBinding is not null)))
            throw new InvalidDataException("A configured seal cannot omit its prediction analysis binding.");
        if (seal is not null &&
            !string.Equals(BaselineIntegrity.ComputeBundleSha256(baselines), seal.BaselinesSha256, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Scenario {scenarioId:D} failed prediction baseline bundle integrity validation.");
        }
        if (seal is not null && body.AnalysisBinding is not null)
        {
            try
            {
                await ValidateConfiguredPredictionAsync(connection, transaction, scenarioId, body, baselines, cancellationToken);
            }
            catch (ScenarioApiException exception)
            {
                throw new InvalidDataException("The configured prediction seal failed source/configuration/baseline validation.", exception);
            }
        }
        return new PredictionRecord(scenarioId, body, revision, createdUtc, modifiedUtc, seal, approval, baselines);
    }

    private static async Task<IReadOnlyList<BaselineSnapshot>> ReadBaselinesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT baseline_kind, baseline_id, content_json, content_sha256
            FROM prediction_baselines
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        var baselines = new List<BaselineSnapshot>();
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string kindText = reader.GetString(0);
            if (!Enum.TryParse(kindText, ignoreCase: false, out BaselineKind kind))
                throw new InvalidDataException($"Scenario {scenarioId:D} has unsupported baseline kind '{kindText}'.");
            Guid baselineId = ParseGuid(reader.GetString(1));
            BaselineSnapshot baseline = PredictionJson.Deserialize<BaselineSnapshot>(reader.GetString(2), "prediction baseline");
            string storedContentSha256 = reader.GetString(3);
            if (baseline.ScenarioId != scenarioId || baseline.BaselineId != baselineId || baseline.Kind != kind ||
                !string.Equals(baseline.ContentSha256, storedContentSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Scenario {scenarioId:D} has inconsistent baseline identity data.");
            }
            BaselineIntegrity.Validate(baseline);
            baselines.Add(baseline);
        }
        return baselines.OrderBy(item => item.Kind).ToArray();
    }

    private static async Task<PredictionRecord> GetRequiredPredictionSnapshotAsync(
        SqliteConnection connection,
        Guid scenarioId,
        CancellationToken cancellationToken)
    {
        using SqliteTransaction transaction = connection.BeginTransaction(deferred: true);
        PredictionRecord prediction = await FindPredictionAsync(connection, transaction, scenarioId, cancellationToken)
            ?? throw new InvalidOperationException($"Prediction for scenario {scenarioId:D} disappeared during persistence.");
        transaction.Commit();
        return prediction;
    }
    private static ScenarioApiException Conflict(string title, string detail) =>
        new(StatusCodes.Status409Conflict, title, detail);

    private static async Task EnsurePredictionLedgerColumnsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        if (!await ColumnExistsAsync(connection, "prediction_drafts", "revision", cancellationToken))
        {
            using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
            await using (var alterCommand = connection.CreateCommand())
            {
                alterCommand.Transaction = transaction;
                alterCommand.CommandText = "ALTER TABLE prediction_drafts ADD COLUMN revision INTEGER NULL;";
                await alterCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            await using (var backfillCommand = connection.CreateCommand())
            {
                backfillCommand.Transaction = transaction;
                backfillCommand.CommandText = "UPDATE prediction_drafts SET revision = 1 WHERE revision IS NULL;";
                await backfillCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            transaction.Commit();
        }

        bool sealBundleColumnExists = await ColumnExistsAsync(
            connection,
            "prediction_seals",
            "baselines_sha256",
            cancellationToken);
        bool baselineContentColumnExists = await ColumnExistsAsync(
            connection,
            "prediction_baselines",
            "content_sha256",
            cancellationToken);
        if (sealBundleColumnExists && baselineContentColumnExists)
            return;

        await using (var countCommand = connection.CreateCommand())
        {
            countCommand.CommandText = """
                SELECT
                    (SELECT COUNT(*) FROM prediction_seals) +
                    (SELECT COUNT(*) FROM prediction_baselines);
                """;
            long sealedDataCount = (long)(await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0L);
            if (sealedDataCount != 0)
            {
                throw new InvalidDataException(
                    "Existing sealed prediction baselines predate content integrity metadata and cannot be safely reinterpreted.");
            }
        }

        using SqliteTransaction integrityTransaction = connection.BeginTransaction(deferred: false);
        if (!sealBundleColumnExists)
        {
            await using var alterSealCommand = connection.CreateCommand();
            alterSealCommand.Transaction = integrityTransaction;
            alterSealCommand.CommandText = "ALTER TABLE prediction_seals ADD COLUMN baselines_sha256 TEXT NULL;";
            await alterSealCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        if (!baselineContentColumnExists)
        {
            await using var alterBaselineCommand = connection.CreateCommand();
            alterBaselineCommand.Transaction = integrityTransaction;
            alterBaselineCommand.CommandText = "ALTER TABLE prediction_baselines ADD COLUMN content_sha256 TEXT NULL;";
            await alterBaselineCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        integrityTransaction.Commit();
    }

    private static async Task<bool> ColumnExistsAsync(
        SqliteConnection connection,
        string tableName,
        string columnName,
        CancellationToken cancellationToken)
    {
        string table = tableName switch
        {
            "prediction_drafts" => "prediction_drafts",
            "prediction_seals" => "prediction_seals",
            "prediction_baselines" => "prediction_baselines",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName))
        };
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT 1
            FROM pragma_table_info('{table}')
            WHERE name = $column_name
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$column_name", columnName);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    private static async Task ValidatePredictionRevisionsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                (SELECT COUNT(*) FROM prediction_drafts
                    WHERE revision IS NULL OR typeof(revision) <> 'integer' OR revision < 1) +
                (SELECT COUNT(*) FROM prediction_seals WHERE baselines_sha256 IS NULL) +
                (SELECT COUNT(*) FROM prediction_baselines WHERE content_sha256 IS NULL);
            """;
        long invalidCount = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        if (invalidCount != 0)
            throw new InvalidDataException("Prediction ledger contains invalid revision or integrity metadata.");
    }

    private static async Task EnsureInitialAsOfColumnAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        bool columnExists;
        await using (var columnCommand = connection.CreateCommand())
        {
            columnCommand.CommandText = """
                SELECT 1
                FROM pragma_table_info('scenarios')
                WHERE name = $column_name
                LIMIT 1;
                """;
            columnCommand.Parameters.AddWithValue("$column_name", "initial_as_of_utc");
            columnExists = await columnCommand.ExecuteScalarAsync(cancellationToken) is not null;
        }
        if (columnExists)
            return;

        using SqliteTransaction transaction = connection.BeginTransaction(deferred: false);
        await ValidateLegacyScenarioTimesAsync(connection, transaction, cancellationToken);
        await using (var alterCommand = connection.CreateCommand())
        {
            alterCommand.Transaction = transaction;
            alterCommand.CommandText = "ALTER TABLE scenarios ADD COLUMN initial_as_of_utc TEXT NULL;";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (var backfillCommand = connection.CreateCommand())
        {
            backfillCommand.Transaction = transaction;
            backfillCommand.CommandText = """
                UPDATE scenarios
                SET initial_as_of_utc = as_of_utc
                WHERE initial_as_of_utc IS NULL;
                """;
            await backfillCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        transaction.Commit();
    }

    private static async Task ValidateLegacyScenarioTimesAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT scenario_id, as_of_utc FROM scenarios;";
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string scenarioId = reader.GetString(0);
            _ = ReadStoredInstant(reader, 1, "as_of_utc", scenarioId);
        }
    }

    private static async Task ValidateScenarioTimesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT scenario_id, initial_as_of_utc, as_of_utc FROM scenarios;";
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string scenarioId = reader.GetString(0);
            DateTimeOffset initial = ReadStoredInstant(reader, 1, "initial_as_of_utc", scenarioId);
            DateTimeOffset current = ReadStoredInstant(reader, 2, "as_of_utc", scenarioId);
            if (initial > current)
            {
                throw new InvalidDataException(
                    $"Scenario '{scenarioId}' has initial_as_of_utc later than as_of_utc.");
            }
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnableForeignKeysAsync(connection, cancellationToken);
        return connection;
    }

    private static async Task EnableForeignKeysAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertVisibilityAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        EvidenceVisibility item,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT OR IGNORE INTO evidence_visibility (
                scenario_id, evidence_id, record_kind, visible_from_utc, visible_until_utc, reveal_id)
            VALUES (
                $scenario_id, $evidence_id, $record_kind, $visible_from_utc, $visible_until_utc, $reveal_id);
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(item.ScenarioId));
        command.Parameters.AddWithValue("$evidence_id", item.EvidenceId);
        command.Parameters.AddWithValue("$record_kind", item.RecordKind);
        command.Parameters.AddWithValue("$visible_from_utc", FormatInstant(item.VisibleFromUtc));
        command.Parameters.AddWithValue("$visible_until_utc", DbValue(item.VisibleUntilUtc is null ? null : FormatInstant(item.VisibleUntilUtc.Value)));
        command.Parameters.AddWithValue("$reveal_id", DbValue(item.RevealId is null ? null : FormatGuid(item.RevealId.Value)));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddScenarioParameters(SqliteCommand command, Scenario scenario)
    {
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenario.ScenarioId));
        command.Parameters.AddWithValue("$source_field_id", FormatGuid(scenario.SourceFieldId));
        command.Parameters.AddWithValue("$cloned_field_id", DbValue(scenario.ClonedFieldId is null ? null : FormatGuid(scenario.ClonedFieldId.Value)));
        command.Parameters.AddWithValue("$reservoir_name", scenario.ReservoirName);
        command.Parameters.AddWithValue("$initial_as_of_utc", FormatInstant(scenario.InitialAsOfUtc));
        command.Parameters.AddWithValue("$as_of_utc", FormatInstant(scenario.AsOfUtc));
        command.Parameters.AddWithValue("$seed_label", scenario.SeedLabel);
        command.Parameters.AddWithValue("$world_model_version", scenario.WorldModelVersion);
        command.Parameters.AddWithValue("$observation_model_version", scenario.ObservationModelVersion);
        command.Parameters.AddWithValue("$scoring_model_version", scenario.ScoringModelVersion);
        command.Parameters.AddWithValue("$status", scenario.Status.ToString());
        command.Parameters.AddWithValue("$assumptions_sha256", scenario.AssumptionsSha256);
        command.Parameters.AddWithValue("$created_utc", FormatInstant(scenario.CreatedUtc));
        command.Parameters.AddWithValue("$modified_utc", FormatInstant(scenario.ModifiedUtc));
    }

    private static async Task<Scenario?> FindAsync(
        SqliteConnection connection,
        Guid scenarioId,
        CancellationToken cancellationToken,
        SqliteTransaction? transaction = null)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT scenario_id, source_field_id, cloned_field_id, reservoir_name, initial_as_of_utc, as_of_utc,
                   seed_label, world_model_version, observation_model_version,
                   scoring_model_version, status, assumptions_sha256, created_utc, modified_utc
            FROM scenarios
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$scenario_id", FormatGuid(scenarioId));
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadScenario(reader) : null;
    }

    private static async Task<Scenario> GetRequiredAsync(
        SqliteConnection connection,
        Guid scenarioId,
        CancellationToken cancellationToken) =>
        await FindAsync(connection, scenarioId, cancellationToken)
        ?? throw new InvalidOperationException($"Scenario {scenarioId:D} disappeared during creation.");

    private static Scenario ReadScenario(SqliteDataReader reader)
    {
        string scenarioIdText = reader.GetString(0);
        DateTimeOffset initialAsOfUtc = ReadStoredInstant(reader, 4, "initial_as_of_utc", scenarioIdText);
        DateTimeOffset asOfUtc = ReadStoredInstant(reader, 5, "as_of_utc", scenarioIdText);
        if (initialAsOfUtc > asOfUtc)
        {
            throw new InvalidDataException(
                $"Scenario '{scenarioIdText}' has initial_as_of_utc later than as_of_utc.");
        }

        string statusText = reader.GetString(10);
        if (!Enum.TryParse(statusText, ignoreCase: false, out ScenarioStatus status))
            throw new InvalidDataException($"Scenario status '{statusText}' is not supported.");

        return new Scenario(
            ParseGuid(scenarioIdText),
            ParseGuid(reader.GetString(1)),
            reader.IsDBNull(2) ? null : ParseGuid(reader.GetString(2)),
            reader.GetString(3),
            initialAsOfUtc,
            asOfUtc,
            reader.GetString(6),
            reader.GetString(7),
            reader.GetString(8),
            reader.GetString(9),
            status,
            reader.GetString(11),
            ReadStoredInstant(reader, 12, "created_utc", scenarioIdText),
            ReadStoredInstant(reader, 13, "modified_utc", scenarioIdText));
    }

    private static DateTimeOffset ReadStoredInstant(
        SqliteDataReader reader,
        int ordinal,
        string columnName,
        string scenarioId)
    {
        if (reader.IsDBNull(ordinal))
            throw new InvalidDataException($"Scenario '{scenarioId}' has a null {columnName} value.");

        try
        {
            return ParseInstant(reader.GetString(ordinal));
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException)
        {
            throw new InvalidDataException(
                $"Scenario '{scenarioId}' has an invalid {columnName} value.",
                exception);
        }
    }

    private static object DbValue(string? value) => value is null ? DBNull.Value : value;

    private static string FormatGuid(Guid value) => value.ToString("D", CultureInfo.InvariantCulture);

    private static Guid ParseGuid(string value) => Guid.ParseExact(value, "D");

    private static string FormatInstant(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseInstant(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
