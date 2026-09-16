using System.Globalization;
using Microsoft.Data.Sqlite;

namespace ReservoirSimulation.Persistence;

internal sealed record PersistedCompletionBinding(
    string CompletionBindingId,
    string WorldId,
    string PathBindingId,
    Guid ScenarioId,
    Guid RunId,
    string ApprovedPredictionSha256,
    string ModelVersion,
    string CanonicalRequestJson,
    string CanonicalRequestHash,
    string MappedConnectionJson,
    string MappedConnectionHash,
    int OpeningCount,
    int ProducingConnectionCount,
    DateTimeOffset CreatedUtc);

internal sealed record PersistedCompletionBindingAudit(
    string AuditId,
    string CompletionBindingId,
    string WorldId,
    string PathBindingId,
    Guid ScenarioId,
    Guid RunId,
    string Action,
    string RequestHash,
    int ConnectionCount,
    DateTimeOffset CreatedUtc);

internal sealed class SqliteCompletionBindingRepository(string connectionString)
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
            CREATE TABLE IF NOT EXISTS ApprovedCompletionBindings (
                CompletionBindingId TEXT NOT NULL PRIMARY KEY,
                WorldId TEXT NOT NULL,
                PathBindingId TEXT NOT NULL,
                ScenarioId TEXT NOT NULL,
                RunId TEXT NOT NULL,
                ApprovedPredictionSha256 TEXT NOT NULL,
                ModelVersion TEXT NOT NULL,
                CanonicalRequestJson TEXT NOT NULL,
                CanonicalRequestHash TEXT NOT NULL,
                MappedConnectionJson TEXT NOT NULL,
                MappedConnectionHash TEXT NOT NULL,
                OpeningCount INTEGER NOT NULL,
                ProducingConnectionCount INTEGER NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (WorldId) REFERENCES ReservoirWorldSpecs(WorldId) ON DELETE RESTRICT,
                FOREIGN KEY (PathBindingId) REFERENCES ApprovedPathBindings(BindingId) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS CompletionBindingAudit (
                AuditId TEXT NOT NULL PRIMARY KEY,
                CompletionBindingId TEXT NOT NULL,
                WorldId TEXT NOT NULL,
                PathBindingId TEXT NOT NULL,
                ScenarioId TEXT NOT NULL,
                RunId TEXT NOT NULL,
                Action TEXT NOT NULL,
                RequestHash TEXT NOT NULL,
                ConnectionCount INTEGER NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (CompletionBindingId)
                    REFERENCES ApprovedCompletionBindings(CompletionBindingId) ON DELETE RESTRICT
            );
            CREATE TRIGGER IF NOT EXISTS TR_ApprovedCompletionBindings_NoUpdate
            BEFORE UPDATE ON ApprovedCompletionBindings
            BEGIN SELECT RAISE(ABORT, 'Approved completion bindings are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_ApprovedCompletionBindings_NoDelete
            BEFORE DELETE ON ApprovedCompletionBindings
            BEGIN SELECT RAISE(ABORT, 'Approved completion bindings are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_CompletionBindingAudit_NoUpdate
            BEFORE UPDATE ON CompletionBindingAudit
            BEGIN SELECT RAISE(ABORT, 'Completion binding audit is immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_CompletionBindingAudit_NoDelete
            BEFORE DELETE ON CompletionBindingAudit
            BEGIN SELECT RAISE(ABORT, 'Completion binding audit is immutable'); END;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    internal async Task SaveBindingAsync(
        PersistedCompletionBinding binding, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO ApprovedCompletionBindings
                (CompletionBindingId, WorldId, PathBindingId, ScenarioId, RunId,
                 ApprovedPredictionSha256, ModelVersion, CanonicalRequestJson,
                 CanonicalRequestHash, MappedConnectionJson, MappedConnectionHash,
                 OpeningCount, ProducingConnectionCount, CreatedUtc)
            VALUES
                ($bindingId, $worldId, $pathBindingId, $scenarioId, $runId,
                 $predictionHash, $modelVersion, $requestJson, $requestHash,
                 $connectionJson, $connectionHash, $openingCount, $connectionCount, $createdUtc);
            """;
        AddBindingParameters(command, binding);
        await command.ExecuteNonQueryAsync(cancellationToken);
        PersistedCompletionBinding stored = await LoadBindingAsync(
            binding.CompletionBindingId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Completion binding disappeared during persistence.");
        if (stored.WorldId != binding.WorldId || stored.PathBindingId != binding.PathBindingId ||
            stored.ScenarioId != binding.ScenarioId || stored.RunId != binding.RunId ||
            stored.ApprovedPredictionSha256 != binding.ApprovedPredictionSha256 ||
            stored.ModelVersion != binding.ModelVersion ||
            stored.CanonicalRequestJson != binding.CanonicalRequestJson ||
            stored.CanonicalRequestHash != binding.CanonicalRequestHash ||
            stored.MappedConnectionJson != binding.MappedConnectionJson ||
            stored.MappedConnectionHash != binding.MappedConnectionHash ||
            stored.OpeningCount != binding.OpeningCount ||
            stored.ProducingConnectionCount != binding.ProducingConnectionCount)
            throw new PersistenceIntegrityException(
                $"Completion binding {binding.CompletionBindingId} conflicts with its deterministic identity.");
    }

    internal async Task<PersistedCompletionBinding?> LoadBindingAsync(
        string completionBindingId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT CompletionBindingId, WorldId, PathBindingId, ScenarioId, RunId,
                   ApprovedPredictionSha256, ModelVersion, CanonicalRequestJson,
                   CanonicalRequestHash, MappedConnectionJson, MappedConnectionHash,
                   OpeningCount, ProducingConnectionCount, CreatedUtc
            FROM ApprovedCompletionBindings
            WHERE CompletionBindingId = $bindingId;
            """;
        command.Parameters.AddWithValue("$bindingId", completionBindingId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedCompletionBinding(
            reader.GetString(0), reader.GetString(1), reader.GetString(2),
            Guid.ParseExact(reader.GetString(3), "D"), Guid.ParseExact(reader.GetString(4), "D"),
            reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetString(8),
            reader.GetString(9), reader.GetString(10), reader.GetInt32(11), reader.GetInt32(12),
            ParseUtc(reader.GetString(13)));
    }

    internal async Task SaveAuditAsync(
        PersistedCompletionBindingAudit audit, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO CompletionBindingAudit
                (AuditId, CompletionBindingId, WorldId, PathBindingId, ScenarioId, RunId,
                 Action, RequestHash, ConnectionCount, CreatedUtc)
            VALUES
                ($auditId, $bindingId, $worldId, $pathBindingId, $scenarioId, $runId,
                 $action, $requestHash, $connectionCount, $createdUtc);
            """;
        command.Parameters.AddWithValue("$auditId", audit.AuditId);
        command.Parameters.AddWithValue("$bindingId", audit.CompletionBindingId);
        command.Parameters.AddWithValue("$worldId", audit.WorldId);
        command.Parameters.AddWithValue("$pathBindingId", audit.PathBindingId);
        command.Parameters.AddWithValue("$scenarioId", audit.ScenarioId.ToString("D"));
        command.Parameters.AddWithValue("$runId", audit.RunId.ToString("D"));
        command.Parameters.AddWithValue("$action", audit.Action);
        command.Parameters.AddWithValue("$requestHash", audit.RequestHash);
        command.Parameters.AddWithValue("$connectionCount", audit.ConnectionCount);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(audit.CreatedUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
        PersistedCompletionBindingAudit stored = await LoadAuditAsync(audit.AuditId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Completion binding audit disappeared.");
        if (stored.CompletionBindingId != audit.CompletionBindingId || stored.WorldId != audit.WorldId ||
            stored.PathBindingId != audit.PathBindingId || stored.ScenarioId != audit.ScenarioId ||
            stored.RunId != audit.RunId || stored.Action != audit.Action ||
            stored.RequestHash != audit.RequestHash || stored.ConnectionCount != audit.ConnectionCount)
            throw new PersistenceIntegrityException(
                $"Completion binding audit {audit.AuditId} conflicts with its deterministic identity.");
    }

    internal async Task<PersistedCompletionBindingAudit?> LoadAuditAsync(
        string auditId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT AuditId, CompletionBindingId, WorldId, PathBindingId, ScenarioId, RunId,
                   Action, RequestHash, ConnectionCount, CreatedUtc
            FROM CompletionBindingAudit
            WHERE AuditId = $auditId;
            """;
        command.Parameters.AddWithValue("$auditId", auditId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedCompletionBindingAudit(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            Guid.ParseExact(reader.GetString(4), "D"), Guid.ParseExact(reader.GetString(5), "D"),
            reader.GetString(6), reader.GetString(7), reader.GetInt32(8), ParseUtc(reader.GetString(9)));
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

    private static void AddBindingParameters(
        SqliteCommand command, PersistedCompletionBinding binding)
    {
        command.Parameters.AddWithValue("$bindingId", binding.CompletionBindingId);
        command.Parameters.AddWithValue("$worldId", binding.WorldId);
        command.Parameters.AddWithValue("$pathBindingId", binding.PathBindingId);
        command.Parameters.AddWithValue("$scenarioId", binding.ScenarioId.ToString("D"));
        command.Parameters.AddWithValue("$runId", binding.RunId.ToString("D"));
        command.Parameters.AddWithValue("$predictionHash", binding.ApprovedPredictionSha256);
        command.Parameters.AddWithValue("$modelVersion", binding.ModelVersion);
        command.Parameters.AddWithValue("$requestJson", binding.CanonicalRequestJson);
        command.Parameters.AddWithValue("$requestHash", binding.CanonicalRequestHash);
        command.Parameters.AddWithValue("$connectionJson", binding.MappedConnectionJson);
        command.Parameters.AddWithValue("$connectionHash", binding.MappedConnectionHash);
        command.Parameters.AddWithValue("$openingCount", binding.OpeningCount);
        command.Parameters.AddWithValue("$connectionCount", binding.ProducingConnectionCount);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(binding.CreatedUtc));
    }

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseUtc(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
