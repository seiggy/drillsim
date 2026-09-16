using Microsoft.Data.Sqlite;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;

namespace ReservoirSimulation.Tests;

internal sealed class WorldSetupTestStore : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new(new SqliteConnectionStringBuilder
    {
        DataSource = $"reservoir-setup-{Guid.NewGuid():N}",
        Mode = SqliteOpenMode.Memory,
        Cache = SqliteCacheMode.Shared,
        Pooling = false
    }.ToString());

    internal string ConnectionString => _connection.ConnectionString;
    internal SqliteReservoirRepository Repository { get; }
    internal PersistentWorldManager Manager { get; }

    internal WorldSetupTestStore()
    {
        Repository = new SqliteReservoirRepository(ConnectionString);
        Manager = NewManager();
    }

    internal async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        await Repository.InitializeAsync();
    }

    internal PersistentWorldManager NewManager() => new(
        new ReservoirWorldFactory(), new InMemoryWorldStore(4), Repository, TimeProvider.System);

    internal async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = sql;
        foreach ((string name, object value) in parameters)
            command.Parameters.AddWithValue(name, value);
        await command.ExecuteNonQueryAsync();
    }

    internal async Task CorruptAsync(string worldId, string corruption)
    {
        PersistedWorldSpec spec = (await Repository.LoadWorldSpecAsync(worldId))!;
        WorldGenerationRequest request = ReservoirWorldFactory.RequestFromCanonicalJson(spec.CanonicalRequestJson);
        string value = corruption switch
        {
            "checksum" => new string('0', 64),
            "canonical" => spec.CanonicalRequestJson + " ",
            "identity" => ReservoirWorldFactory.CanonicalRequestJson(request with { Seed = request.Seed + 1 }),
            "invalid-request" => spec.CanonicalRequestJson.Replace("\"countX\":8", "\"countX\":0",
                StringComparison.Ordinal),
            "invalid-json" => spec.CanonicalRequestJson + "{",
            _ => throw new ArgumentOutOfRangeException(nameof(corruption))
        };
        string column = corruption == "checksum" ? "TruthChecksum" : "CanonicalRequestJson";
        await ExecuteAsync($"UPDATE ReservoirWorldSpecs SET {column} = $value WHERE WorldId = $worldId;",
            ("$value", value), ("$worldId", worldId));
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
