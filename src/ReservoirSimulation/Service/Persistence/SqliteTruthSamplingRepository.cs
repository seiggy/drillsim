using System.Globalization;
using Microsoft.Data.Sqlite;

namespace ReservoirSimulation.Persistence;

internal sealed record PersistedPathBinding(
    string BindingId,
    string WorldId,
    Guid ScenarioId,
    Guid RunId,
    int PathKind,
    string ApprovedPredictionSha256,
    string CanonicalJson,
    string CanonicalHash,
    int StationCount,
    DateTimeOffset CreatedUtc);

internal sealed record PersistedSamplingAudit(
    string AuditId,
    string BindingId,
    string WorldId,
    Guid ScenarioId,
    Guid RunId,
    string CallerLabel,
    string CanonicalRequestHash,
    int SampledCount,
    string ResponseHash,
    DateTimeOffset CreatedUtc);

internal sealed class SqliteTruthSamplingRepository(string connectionString)
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
            CREATE TABLE IF NOT EXISTS ApprovedPathBindings (
                BindingId TEXT NOT NULL PRIMARY KEY,
                WorldId TEXT NOT NULL,
                ScenarioId TEXT NOT NULL,
                RunId TEXT NOT NULL,
                PathKind INTEGER NOT NULL,
                ApprovedPredictionSha256 TEXT NOT NULL,
                CanonicalJson TEXT NOT NULL,
                CanonicalHash TEXT NOT NULL,
                StationCount INTEGER NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (WorldId) REFERENCES ReservoirWorldSpecs(WorldId) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS SamplingAudit (
                AuditId TEXT NOT NULL PRIMARY KEY,
                BindingId TEXT NOT NULL,
                WorldId TEXT NOT NULL,
                ScenarioId TEXT NOT NULL,
                RunId TEXT NOT NULL,
                CallerLabel TEXT NOT NULL,
                CanonicalRequestHash TEXT NOT NULL,
                SampledCount INTEGER NOT NULL,
                ResponseHash TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (BindingId) REFERENCES ApprovedPathBindings(BindingId) ON DELETE RESTRICT,
                FOREIGN KEY (WorldId) REFERENCES ReservoirWorldSpecs(WorldId) ON DELETE RESTRICT
            );
            CREATE TRIGGER IF NOT EXISTS TR_ApprovedPathBindings_NoUpdate
            BEFORE UPDATE ON ApprovedPathBindings
            BEGIN SELECT RAISE(ABORT, 'Approved path bindings are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_ApprovedPathBindings_NoDelete
            BEFORE DELETE ON ApprovedPathBindings
            BEGIN SELECT RAISE(ABORT, 'Approved path bindings are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_SamplingAudit_NoUpdate
            BEFORE UPDATE ON SamplingAudit
            BEGIN SELECT RAISE(ABORT, 'Sampling audit is immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_SamplingAudit_NoDelete
            BEFORE DELETE ON SamplingAudit
            BEGIN SELECT RAISE(ABORT, 'Sampling audit is immutable'); END;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    internal async Task SaveBindingAsync(PersistedPathBinding binding, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO ApprovedPathBindings
                (BindingId, WorldId, ScenarioId, RunId, PathKind, ApprovedPredictionSha256,
                 CanonicalJson, CanonicalHash, StationCount, CreatedUtc)
            VALUES
                ($bindingId, $worldId, $scenarioId, $runId, $pathKind, $predictionHash,
                 $canonicalJson, $canonicalHash, $stationCount, $createdUtc);
            """;
        AddBindingParameters(command, binding);
        await command.ExecuteNonQueryAsync(cancellationToken);
        PersistedPathBinding stored = await LoadBindingAsync(binding.BindingId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Approved path binding disappeared during persistence.");
        if (stored.WorldId != binding.WorldId || stored.ScenarioId != binding.ScenarioId ||
            stored.RunId != binding.RunId || stored.PathKind != binding.PathKind ||
            stored.ApprovedPredictionSha256 != binding.ApprovedPredictionSha256 ||
            stored.CanonicalJson != binding.CanonicalJson || stored.CanonicalHash != binding.CanonicalHash ||
            stored.StationCount != binding.StationCount)
            throw new PersistenceIntegrityException(
                $"Approved path binding {binding.BindingId} conflicts with its deterministic identity.");
    }

    internal async Task<PersistedPathBinding?> LoadBindingAsync(
        string bindingId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT BindingId, WorldId, ScenarioId, RunId, PathKind, ApprovedPredictionSha256,
                   CanonicalJson, CanonicalHash, StationCount, CreatedUtc
            FROM ApprovedPathBindings
            WHERE BindingId = $bindingId;
            """;
        command.Parameters.AddWithValue("$bindingId", bindingId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return ReadBinding(reader);
    }

    internal async Task<bool> WorldHasBindingsAsync(
        string worldId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM ApprovedPathBindings WHERE WorldId = $worldId LIMIT 1;";
        command.Parameters.AddWithValue("$worldId", worldId);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    internal async Task SaveAuditAsync(PersistedSamplingAudit audit, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO SamplingAudit
                (AuditId, BindingId, WorldId, ScenarioId, RunId, CallerLabel, CanonicalRequestHash,
                 SampledCount, ResponseHash, CreatedUtc)
            VALUES
                ($auditId, $bindingId, $worldId, $scenarioId, $runId, $callerLabel, $requestHash,
                 $sampledCount, $responseHash, $createdUtc);
            """;
        command.Parameters.AddWithValue("$auditId", audit.AuditId);
        command.Parameters.AddWithValue("$bindingId", audit.BindingId);
        command.Parameters.AddWithValue("$worldId", audit.WorldId);
        command.Parameters.AddWithValue("$scenarioId", audit.ScenarioId.ToString("D"));
        command.Parameters.AddWithValue("$runId", audit.RunId.ToString("D"));
        command.Parameters.AddWithValue("$callerLabel", audit.CallerLabel);
        command.Parameters.AddWithValue("$requestHash", audit.CanonicalRequestHash);
        command.Parameters.AddWithValue("$sampledCount", audit.SampledCount);
        command.Parameters.AddWithValue("$responseHash", audit.ResponseHash);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(audit.CreatedUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
        PersistedSamplingAudit stored = await LoadAuditAsync(audit.AuditId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Sampling audit disappeared during persistence.");
        if (stored.BindingId != audit.BindingId || stored.WorldId != audit.WorldId ||
            stored.ScenarioId != audit.ScenarioId || stored.RunId != audit.RunId ||
            stored.CallerLabel != audit.CallerLabel ||
            stored.CanonicalRequestHash != audit.CanonicalRequestHash ||
            stored.SampledCount != audit.SampledCount || stored.ResponseHash != audit.ResponseHash)
            throw new PersistenceIntegrityException(
                $"Sampling audit {audit.AuditId} conflicts with its deterministic identity.");
    }

    internal async Task<PersistedSamplingAudit?> LoadAuditAsync(
        string auditId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT AuditId, BindingId, WorldId, ScenarioId, RunId, CallerLabel,
                   CanonicalRequestHash, SampledCount, ResponseHash, CreatedUtc
            FROM SamplingAudit
            WHERE AuditId = $auditId;
            """;
        command.Parameters.AddWithValue("$auditId", auditId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedSamplingAudit(
            reader.GetString(0), reader.GetString(1), reader.GetString(2),
            Guid.ParseExact(reader.GetString(3), "D"), Guid.ParseExact(reader.GetString(4), "D"),
            reader.GetString(5), reader.GetString(6), reader.GetInt32(7), reader.GetString(8),
            ParseUtc(reader.GetString(9)));
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

    private static void AddBindingParameters(SqliteCommand command, PersistedPathBinding binding)
    {
        command.Parameters.AddWithValue("$bindingId", binding.BindingId);
        command.Parameters.AddWithValue("$worldId", binding.WorldId);
        command.Parameters.AddWithValue("$scenarioId", binding.ScenarioId.ToString("D"));
        command.Parameters.AddWithValue("$runId", binding.RunId.ToString("D"));
        command.Parameters.AddWithValue("$pathKind", binding.PathKind);
        command.Parameters.AddWithValue("$predictionHash", binding.ApprovedPredictionSha256);
        command.Parameters.AddWithValue("$canonicalJson", binding.CanonicalJson);
        command.Parameters.AddWithValue("$canonicalHash", binding.CanonicalHash);
        command.Parameters.AddWithValue("$stationCount", binding.StationCount);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(binding.CreatedUtc));
    }

    private static PersistedPathBinding ReadBinding(SqliteDataReader reader) => new(
        reader.GetString(0), reader.GetString(1),
        Guid.ParseExact(reader.GetString(2), "D"), Guid.ParseExact(reader.GetString(3), "D"),
        reader.GetInt32(4), reader.GetString(5), reader.GetString(6), reader.GetString(7),
        reader.GetInt32(8), ParseUtc(reader.GetString(9)));

    private static string FormatUtc(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseUtc(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
