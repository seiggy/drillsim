using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DrillingOperations;

public sealed partial class DrillingOperationsStore
{
    private static async Task InitializeSimulationSetupSchemaAsync(SqliteConnection connection, CancellationToken ct)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SimulationSetups (
                ScenarioId TEXT NOT NULL PRIMARY KEY REFERENCES TruthBindings(ScenarioId),
                RequestJson TEXT NOT NULL,
                RequestHash TEXT NOT NULL,
                ResultJson TEXT NOT NULL,
                ResultHash TEXT NOT NULL
            );
            CREATE TRIGGER IF NOT EXISTS TR_SimulationSetups_NoUpdate
                BEFORE UPDATE ON SimulationSetups BEGIN SELECT RAISE(ABORT, 'Simulation setups are immutable'); END;
            CREATE TRIGGER IF NOT EXISTS TR_SimulationSetups_NoDelete
                BEFORE DELETE ON SimulationSetups BEGIN SELECT RAISE(ABORT, 'Simulation setups are immutable'); END;
            """;
        await command.ExecuteNonQueryAsync(ct);
    }

    public Task<ApiOutcome> PrepareSimulationAsync(
        string route, string key, PrepareSimulationRequest request, string profileName,
        BindWorldRequest binding, CancellationToken ct = default) =>
        ExecuteIdempotentAsync(route, key, CanonicalJson.Serialize(request), async (connection, transaction, token) =>
        {
            SimulationSetupCoordinator.Validate(request);
            if (request.ReviewedSealHash != binding.ApprovedSealedPredictionHash ||
                !SimulationSetupCoordinator.IsLabel(profileName, 300))
                return Problem(409, "The prepared model does not match the reviewed prediction.");
            SimulationSetupResult? existing = await ReadSimulationSetupAsync(connection, transaction, binding.ScenarioId, token);
            if (existing is not null)
                return existing.Configuration.ProfileId == request.ProfileId &&
                    existing.Configuration.Resolution == request.Resolution &&
                    existing.Configuration.RealizationSeed == request.RealizationSeed &&
                    existing.Configuration.PreparedBy == request.Actor &&
                    existing.ReviewedSealHash == request.ReviewedSealHash
                        ? Json(200, existing) : Problem(409, "Simulation setup is immutable.");
            ApiOutcome bound = await BindWorldCoreAsync(connection, transaction, binding.ScenarioId, binding, token);
            if (bound.StatusCode is not (200 or 201))
                return Problem(409, "The scenario could not be bound to the prepared simulation model.");
            var configuration = new SimulationConfiguration(request.ProfileId, profileName, request.Resolution,
                request.RealizationSeed, request.Actor, timeProvider.GetUtcNow());
            var result = new SimulationSetupResult(Guid.Parse(binding.ScenarioId), "simulation-prepared", configuration, request.ReviewedSealHash);
            string requestJson = CanonicalJson.Serialize(request);
            string resultJson = CanonicalJson.Serialize(result);
            await using SqliteCommand insert = Command(connection, transaction, """
                INSERT INTO SimulationSetups(ScenarioId,RequestJson,RequestHash,ResultJson,ResultHash)
                VALUES($scenario,$request,$requestHash,$result,$resultHash);
                """);
            Add(insert, "$scenario", binding.ScenarioId);
            Add(insert, "$request", requestJson);
            Add(insert, "$requestHash", DeterministicIdentity.Sha256(requestJson));
            Add(insert, "$result", resultJson);
            Add(insert, "$resultHash", DeterministicIdentity.Sha256(resultJson));
            await insert.ExecuteNonQueryAsync(token);
            await AppendAuditAsync(connection, transaction, binding.ScenarioId, "simulation.prepared", binding.ScenarioId,
                CanonicalJson.Serialize(new { request.Actor, request.ProfileId, request.Resolution, request.RealizationSeed, request.ReviewedSealHash }), token);
            return Json(201, result);
        }, ct);

    public async Task<SimulationSetupResult?> GetSimulationSetupAsync(string scenarioId, CancellationToken ct = default)
    {
        await using SqliteConnection connection = await OpenAsync(ct);
        return await ReadSimulationSetupAsync(connection, null, scenarioId, ct);
    }

    private static async Task<SimulationSetupResult?> ReadSimulationSetupAsync(
        SqliteConnection connection, SqliteTransaction? transaction, string scenarioId, CancellationToken ct)
    {
        await using SqliteCommand query = Command(connection, transaction, """
            SELECT RequestJson,RequestHash,ResultJson,ResultHash FROM SimulationSetups WHERE ScenarioId=$scenario;
            """);
        Add(query, "$scenario", scenarioId);
        await using SqliteDataReader reader = await query.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        string requestJson = reader.GetString(0), resultJson = reader.GetString(2);
        if (DeterministicIdentity.Sha256(requestJson) != reader.GetString(1) ||
            DeterministicIdentity.Sha256(resultJson) != reader.GetString(3))
            throw new PersistenceIntegrityException("Simulation setup hashes do not match.");
        PrepareSimulationRequest request = JsonSerializer.Deserialize<PrepareSimulationRequest>(requestJson, CanonicalJson.SerializerOptions)
            ?? throw new PersistenceIntegrityException("Simulation setup request is missing.");
        SimulationSetupResult result = JsonSerializer.Deserialize<SimulationSetupResult>(resultJson, CanonicalJson.SerializerOptions)
            ?? throw new PersistenceIntegrityException("Simulation setup result is missing.");
        SimulationSetupCoordinator.Validate(request);
        if (result.ScenarioId.ToString("D") != scenarioId || result.Outcome != "simulation-prepared" ||
            result.ReviewedSealHash != request.ReviewedSealHash ||
            result.Configuration.ProfileId != request.ProfileId || result.Configuration.Resolution != request.Resolution ||
            result.Configuration.RealizationSeed != request.RealizationSeed || result.Configuration.PreparedBy != request.Actor)
            throw new PersistenceIntegrityException("Simulation setup does not match its immutable request.");
        return result;
    }
}
