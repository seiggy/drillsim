using System.Globalization;
using Microsoft.Data.Sqlite;

namespace ReservoirSimulation.Persistence;

internal sealed class PersistenceIntegrityException : InvalidOperationException
{
    internal PersistenceIntegrityException(string message) : base(message) { }
    internal PersistenceIntegrityException(string message, Exception innerException) : base(message, innerException) { }
}

internal sealed record PersistedWorldSpec(
    string WorldId,
    string ModelVersion,
    string CanonicalRequestJson,
    DateTimeOffset CreatedUtc,
    string TruthChecksum);

internal sealed record PersistedState(
    string StateId,
    string WorldId,
    string? ParentStateId,
    string ModelVersion,
    string CanonicalRunRequestJson,
    string RunRequestHash,
    double SimulatedTimeSeconds,
    byte[] PressureBlob,
    byte[] OilBlob,
    byte[] WaterBlob,
    byte[] GasBlob,
    string StateChecksum,
    DateTimeOffset CreatedUtc);

internal sealed record PersistedStateMetadata(
    string StateId,
    string WorldId,
    string? ParentStateId,
    string ModelVersion,
    double SimulatedTimeSeconds,
    DateTimeOffset CreatedUtc);

internal sealed record PersistedStatePin(
    string StateId,
    string ReferenceKind,
    string ReferenceId,
    DateTimeOffset CreatedUtc);

internal sealed class SqliteReservoirRepository(string connectionString)
{
    private const int StateRetentionPerWorld = 20;
    private readonly string _connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("SQLite connection string is required.", nameof(connectionString))
        : connectionString;

    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS ReservoirWorldSpecs (
                WorldId TEXT NOT NULL PRIMARY KEY,
                ModelVersion TEXT NOT NULL,
                CanonicalRequestJson TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL,
                TruthChecksum TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ReservoirSimulationStates (
                StateId TEXT NOT NULL PRIMARY KEY,
                WorldId TEXT NOT NULL,
                ParentStateId TEXT NULL,
                ModelVersion TEXT NOT NULL,
                CanonicalRunRequestJson TEXT NOT NULL,
                RunRequestHash TEXT NOT NULL,
                SimulatedTimeSeconds REAL NOT NULL,
                PressureBlob BLOB NOT NULL,
                OilBlob BLOB NOT NULL,
                WaterBlob BLOB NOT NULL,
                GasBlob BLOB NOT NULL,
                StateChecksum TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL,
                FOREIGN KEY (WorldId) REFERENCES ReservoirWorldSpecs(WorldId) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS StatePins (
                StateId TEXT NOT NULL,
                ReferenceKind TEXT NOT NULL,
                ReferenceId TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL,
                PRIMARY KEY (StateId, ReferenceKind, ReferenceId),
                FOREIGN KEY (StateId) REFERENCES ReservoirSimulationStates(StateId) ON DELETE RESTRICT
            );
            CREATE TRIGGER IF NOT EXISTS TR_StatePins_NoUpdate
            BEFORE UPDATE ON StatePins
            BEGIN SELECT RAISE(ABORT, 'State pins are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_StatePins_NoDelete
            BEFORE DELETE ON StatePins
            BEGIN SELECT RAISE(ABORT, 'State pins are immutable'); END;
            CREATE INDEX IF NOT EXISTS IX_ReservoirSimulationStates_WorldCreated
                ON ReservoirSimulationStates(WorldId, CreatedUtc DESC, StateId DESC);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    internal async Task SaveWorldSpecAsync(PersistedWorldSpec spec, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using (SqliteCommand insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText =
                """
                INSERT OR IGNORE INTO ReservoirWorldSpecs
                    (WorldId, ModelVersion, CanonicalRequestJson, CreatedUtc, TruthChecksum)
                VALUES ($worldId, $modelVersion, $requestJson, $createdUtc, $truthChecksum);
                """;
            insert.Parameters.AddWithValue("$worldId", spec.WorldId);
            insert.Parameters.AddWithValue("$modelVersion", spec.ModelVersion);
            insert.Parameters.AddWithValue("$requestJson", spec.CanonicalRequestJson);
            insert.Parameters.AddWithValue("$createdUtc", FormatUtc(spec.CreatedUtc));
            insert.Parameters.AddWithValue("$truthChecksum", spec.TruthChecksum);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        PersistedWorldSpec stored = await ReadWorldSpecAsync(connection, transaction, spec.WorldId, cancellationToken)
            ?? throw new PersistenceIntegrityException("World spec disappeared during idempotent persistence.");
        if (stored.ModelVersion != spec.ModelVersion ||
            stored.CanonicalRequestJson != spec.CanonicalRequestJson ||
            stored.TruthChecksum != spec.TruthChecksum)
            throw new PersistenceIntegrityException(
                $"Persisted world spec {spec.WorldId} conflicts with its deterministic identity.");
        await transaction.CommitAsync(cancellationToken);
    }

    internal async Task<PersistedWorldSpec?> LoadWorldSpecAsync(
        string worldId,
        CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        return await ReadWorldSpecAsync(connection, null, worldId, cancellationToken);
    }

    internal async Task<IReadOnlyList<PersistedWorldSpec>> ListWorldSpecsAsync(
        Guid fieldId,
        string reservoirName,
        string modelVersion,
        PersistedWorldSpec? after = null,
        CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT WorldId, ModelVersion, CanonicalRequestJson, CreatedUtc, TruthChecksum
            FROM ReservoirWorldSpecs
            WHERE ModelVersion = $modelVersion
              AND CASE WHEN ModelVersion = $modelVersion
                  THEN json_extract(CanonicalRequestJson, '$.fieldId') END = $fieldId
              AND CASE WHEN ModelVersion = $modelVersion
                  THEN json_extract(CanonicalRequestJson, '$.reservoirName') END = $reservoirName
              AND ($afterCreatedUtc IS NULL OR CreatedUtc > $afterCreatedUtc
                   OR (CreatedUtc = $afterCreatedUtc AND WorldId > $afterWorldId))
            ORDER BY CreatedUtc, WorldId
            LIMIT 64;
            """;
        command.Parameters.AddWithValue("$modelVersion", modelVersion);
        command.Parameters.AddWithValue("$fieldId", fieldId.ToString("D"));
        command.Parameters.AddWithValue("$reservoirName", reservoirName);
        command.Parameters.AddWithValue("$afterCreatedUtc",
            after is null ? DBNull.Value : FormatUtc(after.CreatedUtc));
        command.Parameters.AddWithValue("$afterWorldId", (object?)after?.WorldId ?? DBNull.Value);
        try
        {
            await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            var specifications = new List<PersistedWorldSpec>();
            while (await reader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                specifications.Add(new PersistedWorldSpec(
                    reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    ParseUtc(reader.GetString(3)), reader.GetString(4)));
            }
            return specifications;
        }
        catch (Exception exception) when (exception is SqliteException or FormatException)
        {
            throw new PersistenceIntegrityException(
                "Persisted setup model specifications could not be read.", exception);
        }
    }

    internal async Task SaveStateAsync(PersistedState state, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using (SqliteCommand insert = connection.CreateCommand())
        {
            insert.Transaction = transaction;
            insert.CommandText =
                """
                INSERT OR IGNORE INTO ReservoirSimulationStates
                    (StateId, WorldId, ParentStateId, ModelVersion, CanonicalRunRequestJson, RunRequestHash,
                     SimulatedTimeSeconds, PressureBlob, OilBlob, WaterBlob, GasBlob, StateChecksum, CreatedUtc)
                VALUES
                    ($stateId, $worldId, $parentStateId, $modelVersion, $runJson, $runHash,
                     $simulatedTime, $pressure, $oil, $water, $gas, $stateChecksum, $createdUtc);
                """;
            AddStateParameters(insert, state);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        PersistedState stored = await ReadStateAsync(connection, transaction, state.WorldId, state.StateId, cancellationToken)
            ?? throw new PersistenceIntegrityException("Simulation state disappeared during idempotent persistence.");
        if (stored.ParentStateId != state.ParentStateId ||
            stored.ModelVersion != state.ModelVersion ||
            stored.CanonicalRunRequestJson != state.CanonicalRunRequestJson ||
            stored.RunRequestHash != state.RunRequestHash ||
            stored.SimulatedTimeSeconds != state.SimulatedTimeSeconds ||
            stored.StateChecksum != state.StateChecksum)
            throw new PersistenceIntegrityException(
                $"Persisted simulation state {state.StateId} conflicts with its deterministic identity.");

        await using (SqliteCommand retention = connection.CreateCommand())
        {
            retention.Transaction = transaction;
            retention.CommandText =
                """
                DELETE FROM ReservoirSimulationStates
                WHERE WorldId = $worldId
                  AND StateId IN (
                    SELECT StateId
                    FROM ReservoirSimulationStates
                    WHERE WorldId = $worldId
                      AND StateId NOT IN (SELECT StateId FROM StatePins)
                    ORDER BY CreatedUtc DESC, StateId DESC
                    LIMIT -1 OFFSET $retention
                  );
                """;
            retention.Parameters.AddWithValue("$worldId", state.WorldId);
            retention.Parameters.AddWithValue("$retention", StateRetentionPerWorld);
            await retention.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    internal async Task<PersistedState?> LoadStateAsync(
        string worldId,
        string stateId,
        CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        return await ReadStateAsync(connection, null, worldId, stateId, cancellationToken);
    }

    internal async Task<PersistedStateMetadata?> LoadStateMetadataAsync(
        string worldId,
        string stateId,
        CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT StateId, WorldId, ParentStateId, ModelVersion, SimulatedTimeSeconds, CreatedUtc
            FROM ReservoirSimulationStates
            WHERE WorldId = $worldId AND StateId = $stateId;
            """;
        command.Parameters.AddWithValue("$worldId", worldId);
        command.Parameters.AddWithValue("$stateId", stateId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedStateMetadata(
            reader.GetString(0), reader.GetString(1), NullableString(reader, 2), reader.GetString(3),
            reader.GetDouble(4), ParseUtc(reader.GetString(5)));
    }

    internal async Task PinStateAsync(
        PersistedStatePin pin, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR IGNORE INTO StatePins (StateId, ReferenceKind, ReferenceId, CreatedUtc)
            VALUES ($stateId, $referenceKind, $referenceId, $createdUtc);
            """;
        command.Parameters.AddWithValue("$stateId", pin.StateId);
        command.Parameters.AddWithValue("$referenceKind", pin.ReferenceKind);
        command.Parameters.AddWithValue("$referenceId", pin.ReferenceId);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(pin.CreatedUtc));
        await command.ExecuteNonQueryAsync(cancellationToken);
        await using SqliteCommand verify = connection.CreateCommand();
        verify.CommandText =
            """
            SELECT CreatedUtc FROM StatePins
            WHERE StateId = $stateId AND ReferenceKind = $referenceKind AND ReferenceId = $referenceId;
            """;
        verify.Parameters.AddWithValue("$stateId", pin.StateId);
        verify.Parameters.AddWithValue("$referenceKind", pin.ReferenceKind);
        verify.Parameters.AddWithValue("$referenceId", pin.ReferenceId);
        if (await verify.ExecuteScalarAsync(cancellationToken) is not string)
            throw new PersistenceIntegrityException("State pin disappeared during persistence.");
    }

    internal async Task<int> CountStatePinsAsync(
        string stateId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM StatePins WHERE StateId = $stateId;";
        command.Parameters.AddWithValue("$stateId", stateId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }


    internal async Task DeleteWorldAsync(string worldId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteTransaction transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using (SqliteCommand states = connection.CreateCommand())
        {
            states.Transaction = transaction;
            states.CommandText = "DELETE FROM ReservoirSimulationStates WHERE WorldId = $worldId;";
            states.Parameters.AddWithValue("$worldId", worldId);
            await states.ExecuteNonQueryAsync(cancellationToken);
        }
        await using (SqliteCommand world = connection.CreateCommand())
        {
            world.Transaction = transaction;
            world.CommandText = "DELETE FROM ReservoirWorldSpecs WHERE WorldId = $worldId;";
            world.Parameters.AddWithValue("$worldId", worldId);
            await world.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    internal async Task<int> CountStatesAsync(string worldId, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ReservoirSimulationStates WHERE WorldId = $worldId;";
        command.Parameters.AddWithValue("$worldId", worldId);
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
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

    private static async Task<PersistedWorldSpec?> ReadWorldSpecAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string worldId,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT WorldId, ModelVersion, CanonicalRequestJson, CreatedUtc, TruthChecksum
            FROM ReservoirWorldSpecs
            WHERE WorldId = $worldId;
            """;
        command.Parameters.AddWithValue("$worldId", worldId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedWorldSpec(
            reader.GetString(0), reader.GetString(1), reader.GetString(2),
            ParseUtc(reader.GetString(3)), reader.GetString(4));
    }

    private static async Task<PersistedState?> ReadStateAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string worldId,
        string stateId,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT StateId, WorldId, ParentStateId, ModelVersion, CanonicalRunRequestJson, RunRequestHash,
                   SimulatedTimeSeconds, PressureBlob, OilBlob, WaterBlob, GasBlob, StateChecksum, CreatedUtc
            FROM ReservoirSimulationStates
            WHERE WorldId = $worldId AND StateId = $stateId;
            """;
        command.Parameters.AddWithValue("$worldId", worldId);
        command.Parameters.AddWithValue("$stateId", stateId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;
        return new PersistedState(
            reader.GetString(0), reader.GetString(1), NullableString(reader, 2), reader.GetString(3),
            reader.GetString(4), reader.GetString(5), reader.GetDouble(6),
            (byte[])reader[7], (byte[])reader[8], (byte[])reader[9], (byte[])reader[10],
            reader.GetString(11), ParseUtc(reader.GetString(12)));
    }

    private static void AddStateParameters(SqliteCommand command, PersistedState state)
    {
        command.Parameters.AddWithValue("$stateId", state.StateId);
        command.Parameters.AddWithValue("$worldId", state.WorldId);
        command.Parameters.AddWithValue("$parentStateId", (object?)state.ParentStateId ?? DBNull.Value);
        command.Parameters.AddWithValue("$modelVersion", state.ModelVersion);
        command.Parameters.AddWithValue("$runJson", state.CanonicalRunRequestJson);
        command.Parameters.AddWithValue("$runHash", state.RunRequestHash);
        command.Parameters.AddWithValue("$simulatedTime", state.SimulatedTimeSeconds);
        command.Parameters.AddWithValue("$pressure", state.PressureBlob);
        command.Parameters.AddWithValue("$oil", state.OilBlob);
        command.Parameters.AddWithValue("$water", state.WaterBlob);
        command.Parameters.AddWithValue("$gas", state.GasBlob);
        command.Parameters.AddWithValue("$stateChecksum", state.StateChecksum);
        command.Parameters.AddWithValue("$createdUtc", FormatUtc(state.CreatedUtc));
    }

    private static string? NullableString(SqliteDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    private static string FormatUtc(DateTimeOffset value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ParseUtc(string value) =>
        DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
