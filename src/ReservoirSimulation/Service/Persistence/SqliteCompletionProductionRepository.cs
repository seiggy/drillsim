using System.Globalization;
using Microsoft.Data.Sqlite;

namespace ReservoirSimulation.Persistence;

internal sealed record PersistedCompletionProductionRun(
    string ProductionRunId,
    string WorldId,
    string CompletionBindingId,
    string ModelVersion,
    string CanonicalRequestJson,
    string CanonicalRequestHash,
    string ResponseJson,
    string ResponseHash,
    string CheckpointStateIdsJson,
    DateTimeOffset CreatedUtc);

internal sealed record PersistedCompletionProductionAudit(
    string AuditId,
    string ProductionRunId,
    string CompletionBindingId,
    string WorldId,
    string Action,
    string RequestHash,
    int CheckpointCount,
    DateTimeOffset CreatedUtc);

internal sealed class SqliteCompletionProductionRepository(string connectionString)
{
    private readonly string _connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("SQLite connection string is required.", nameof(connectionString))
        : connectionString;

    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS CompletionProductionRuns (
                ProductionRunId TEXT NOT NULL PRIMARY KEY,
                WorldId TEXT NOT NULL,
                CompletionBindingId TEXT NOT NULL,
                ModelVersion TEXT NOT NULL,
                CanonicalRequestJson TEXT NOT NULL,
                CanonicalRequestHash TEXT NOT NULL,
                ResponseJson TEXT NOT NULL,
                ResponseHash TEXT NOT NULL,
                CheckpointStateIdsJson TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (WorldId) REFERENCES ReservoirWorldSpecs(WorldId) ON DELETE RESTRICT,
                FOREIGN KEY (CompletionBindingId)
                    REFERENCES ApprovedCompletionBindings(CompletionBindingId) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS CompletionProductionAudit (
                AuditId TEXT NOT NULL PRIMARY KEY,
                ProductionRunId TEXT NOT NULL,
                CompletionBindingId TEXT NOT NULL,
                WorldId TEXT NOT NULL,
                Action TEXT NOT NULL,
                RequestHash TEXT NOT NULL,
                CheckpointCount INTEGER NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (ProductionRunId)
                    REFERENCES CompletionProductionRuns(ProductionRunId) ON DELETE RESTRICT
            );
            CREATE TRIGGER IF NOT EXISTS TR_CompletionProductionRuns_NoUpdate
            BEFORE UPDATE ON CompletionProductionRuns
            BEGIN SELECT RAISE(ABORT, 'Completion production runs are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_CompletionProductionRuns_NoDelete
            BEFORE DELETE ON CompletionProductionRuns
            BEGIN SELECT RAISE(ABORT, 'Completion production runs are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_CompletionProductionAudit_NoUpdate
            BEFORE UPDATE ON CompletionProductionAudit
            BEGIN SELECT RAISE(ABORT, 'Completion production audit is immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_CompletionProductionAudit_NoDelete
            BEFORE DELETE ON CompletionProductionAudit
            BEGIN SELECT RAISE(ABORT, 'Completion production audit is immutable'); END;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    internal async Task SaveRunAsync(
        PersistedCompletionProductionRun run, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO CompletionProductionRuns
                (ProductionRunId, WorldId, CompletionBindingId, ModelVersion, CanonicalRequestJson,
                 CanonicalRequestHash, ResponseJson, ResponseHash, CheckpointStateIdsJson, CreatedUtc)
            VALUES
                ($runId, $worldId, $bindingId, $modelVersion, $requestJson,
                 $requestHash, $responseJson, $responseHash, $checkpointIds, $createdUtc);
            """;
        AddRunParameters(command, run);
        await command.ExecuteNonQueryAsync(cancellationToken);
        PersistedCompletionProductionRun stored = await LoadRunAsync(
            run.CompletionBindingId, run.ProductionRunId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Completion production run disappeared.");
        if (stored.WorldId != run.WorldId || stored.ModelVersion != run.ModelVersion ||
            stored.CanonicalRequestJson != run.CanonicalRequestJson ||
            stored.CanonicalRequestHash != run.CanonicalRequestHash ||
            stored.ResponseJson != run.ResponseJson || stored.ResponseHash != run.ResponseHash ||
            stored.CheckpointStateIdsJson != run.CheckpointStateIdsJson)
            throw new PersistenceIntegrityException(
                $"Completion production run {run.ProductionRunId} conflicts with deterministic identity.");
    }

    internal async Task<PersistedCompletionProductionRun?> LoadRunAsync(
        string completionBindingId, string productionRunId,
        CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ProductionRunId, WorldId, CompletionBindingId, ModelVersion,
                   CanonicalRequestJson, CanonicalRequestHash, ResponseJson, ResponseHash,
                   CheckpointStateIdsJson, CreatedUtc
            FROM CompletionProductionRuns
            WHERE CompletionBindingId = $bindingId AND ProductionRunId = $runId;
            """;
        command.Parameters.AddWithValue("$bindingId", completionBindingId);
        command.Parameters.AddWithValue("$runId", productionRunId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedCompletionProductionRun(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7),
            reader.GetString(8), ParseUtc(reader.GetString(9)));
    }

    internal async Task SaveAuditAsync(
        PersistedCompletionProductionAudit audit, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO CompletionProductionAudit
                (AuditId, ProductionRunId, CompletionBindingId, WorldId, Action,
                 RequestHash, CheckpointCount, CreatedUtc)
            VALUES
                ($auditId, $runId, $bindingId, $worldId, $action,
                 $requestHash, $checkpointCount, $createdUtc);
            """;
        command.Parameters.AddWithValue("$auditId", audit.AuditId);
        command.Parameters.AddWithValue("$runId", audit.ProductionRunId);
        command.Parameters.AddWithValue("$bindingId", audit.CompletionBindingId);
        command.Parameters.AddWithValue("$worldId", audit.WorldId);
        command.Parameters.AddWithValue("$action", audit.Action);
        command.Parameters.AddWithValue("$requestHash", audit.RequestHash);
        command.Parameters.AddWithValue("$checkpointCount", audit.CheckpointCount);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(audit.CreatedUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
        PersistedCompletionProductionAudit stored = await LoadAuditAsync(audit.AuditId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Completion production audit disappeared.");
        if (stored.ProductionRunId != audit.ProductionRunId ||
            stored.CompletionBindingId != audit.CompletionBindingId || stored.WorldId != audit.WorldId ||
            stored.Action != audit.Action || stored.RequestHash != audit.RequestHash ||
            stored.CheckpointCount != audit.CheckpointCount)
            throw new PersistenceIntegrityException(
                $"Completion production audit {audit.AuditId} conflicts with deterministic identity.");
    }

    internal async Task<PersistedCompletionProductionAudit?> LoadAuditAsync(
        string auditId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT AuditId, ProductionRunId, CompletionBindingId, WorldId, Action,
                   RequestHash, CheckpointCount, CreatedUtc
            FROM CompletionProductionAudit
            WHERE AuditId = $auditId;
            """;
        command.Parameters.AddWithValue("$auditId", auditId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedCompletionProductionAudit(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetString(4), reader.GetString(5), reader.GetInt32(6), ParseUtc(reader.GetString(7)));
    }


    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }

    private static void AddRunParameters(
        SqliteCommand command, PersistedCompletionProductionRun run)
    {
        command.Parameters.AddWithValue("$runId", run.ProductionRunId);
        command.Parameters.AddWithValue("$worldId", run.WorldId);
        command.Parameters.AddWithValue("$bindingId", run.CompletionBindingId);
        command.Parameters.AddWithValue("$modelVersion", run.ModelVersion);
        command.Parameters.AddWithValue("$requestJson", run.CanonicalRequestJson);
        command.Parameters.AddWithValue("$requestHash", run.CanonicalRequestHash);
        command.Parameters.AddWithValue("$responseJson", run.ResponseJson);
        command.Parameters.AddWithValue("$responseHash", run.ResponseHash);
        command.Parameters.AddWithValue("$checkpointIds", run.CheckpointStateIdsJson);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(run.CreatedUtc));
    }

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseUtc(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
