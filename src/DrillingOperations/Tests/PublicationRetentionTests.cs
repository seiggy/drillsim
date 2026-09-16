using System.Net;
using DrillSim.PublicationGate;
using Microsoft.Data.Sqlite;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class PublicationRetentionTests
{
    [TestCase("WellBoreArchitectureTable")]
    [TestCase("GeologicalPropertiesTable")]
    public async Task Retirement_PreservesStagedActivatedAndVisibleEvidenceAcrossRestart(string table)
    {
        string servicePath = Path.Combine(Path.GetTempPath(), $"retention-{Guid.NewGuid():N}.db");
        string gatePath = servicePath + ".publication-gate.db";
        try
        {
            Guid scenario = Guid.NewGuid(), reveal = Guid.NewGuid();
            Guid staged = Guid.NewGuid(), activated = Guid.NewGuid(), visible = Guid.NewGuid(), expired = Guid.NewGuid(), recent = Guid.NewGuid();
            using var http = new HttpClient(new ReceiptHandler(scenario, reveal)) { BaseAddress = new Uri("http://analysis.test/") };
            var store = new ScenarioPublicationGateStore($"Data Source={gatePath}", http);
            await store.StageAsync(visible, new(scenario, reveal), default);
            await store.ActivateAsync(visible, default);
            Assert.That(await store.GetHiddenIdsAsync(default), Is.Empty);
            Assert.That((await store.GetAsync(visible, default))!.State, Is.EqualTo("Visible"));
            await store.StageAsync(staged, new(scenario, reveal), default);
            await store.StageAsync(activated, new(scenario, reveal), default);
            await store.ActivateAsync(activated, default);
            await using var service = new SqliteConnection($"Data Source={servicePath};Pooling=False");
            await service.OpenAsync();
            await using (var create = service.CreateCommand())
            {
                create.CommandText = $"CREATE TABLE {table}(ID TEXT PRIMARY KEY, LastModificationDate TEXT, Payload TEXT);";
                await create.ExecuteNonQueryAsync();
            }
            foreach (Guid id in new[] { staged, activated, visible, expired, recent })
            {
                await using var insert = service.CreateCommand();
                insert.CommandText = $"INSERT INTO {table} VALUES($id,$date,'immutable evidence');";
                insert.Parameters.AddWithValue("$id", id == staged ? id.ToString("D").ToUpperInvariant() : id.ToString("D"));
                insert.Parameters.AddWithValue("$date", id == recent ? "2026-09-14 00:00:00" : "2024-01-02 00:00:00");
                await insert.ExecuteNonQueryAsync();
            }
            var restarted = new ScenarioPublicationGateStore($"Data Source={gatePath}");
            var cutoff = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
            Assert.That(await restarted.DeleteExpiredUnregisteredAsync(service, table, cutoff, default), Is.EqualTo(1));
            Assert.That(await restarted.DeleteExpiredUnregisteredAsync(service, table, cutoff, default), Is.Zero);
            await using var read = service.CreateCommand();
            read.CommandText = $"SELECT ID,Payload FROM {table};";
            await using var reader = await read.ExecuteReaderAsync();
            var retained = new List<Guid>();
            while (await reader.ReadAsync())
            {
                retained.Add(Guid.Parse(reader.GetString(0)));
                Assert.That(reader.GetString(1), Is.EqualTo("immutable evidence"));
            }
            Assert.That(retained, Is.EquivalentTo(new[] { staged, activated, visible, recent }));
            Assert.That((await restarted.GetAsync(staged, default))!.State, Is.EqualTo("Staged"));
            Assert.That((await restarted.GetAsync(activated, default))!.State, Is.EqualTo("Activated"));
            Assert.That((await restarted.GetAsync(visible, default))!.State, Is.EqualTo("Visible"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(servicePath);
            File.Delete(gatePath);
        }
    }

    [Test]
    public async Task Retirement_RejectsUnknownTableAndUnavailableRegistryWithoutDeletingRecords()
    {
        string servicePath = Path.Combine(Path.GetTempPath(), $"retention-guard-{Guid.NewGuid():N}.db");
        string gatePath = servicePath + ".publication-gate.db";
        try
        {
            var store = new ScenarioPublicationGateStore($"Data Source={gatePath}");
            await store.StageAsync(Guid.NewGuid(), new(Guid.NewGuid(), Guid.NewGuid()), default);
            await using var service = new SqliteConnection($"Data Source={servicePath};Pooling=False");
            await service.OpenAsync();
            await using var command = service.CreateCommand();
            command.CommandText = "CREATE TABLE WellBoreArchitectureTable(ID TEXT PRIMARY KEY,LastModificationDate TEXT); INSERT INTO WellBoreArchitectureTable VALUES('old','2024-01-01 00:00:00');";
            await command.ExecuteNonQueryAsync();
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await store.DeleteExpiredUnregisteredAsync(service, "WellBoreArchitectureTable; DROP TABLE x", DateTimeOffset.UtcNow, default));
            await using (var registry = new SqliteConnection($"Data Source={gatePath};Pooling=False"))
            {
                await registry.OpenAsync();
                await using var corrupt = registry.CreateCommand();
                corrupt.CommandText = "DROP TABLE DrillSimPublicationVisibility;";
                await corrupt.ExecuteNonQueryAsync();
            }
            Assert.ThrowsAsync<SqliteException>(async () =>
                await store.DeleteExpiredUnregisteredAsync(service, "WellBoreArchitectureTable", DateTimeOffset.UtcNow, default));
            command.CommandText = "SELECT COUNT(*) FROM WellBoreArchitectureTable;";
            Assert.That(await command.ExecuteScalarAsync(), Is.EqualTo(1L));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(servicePath);
            File.Delete(gatePath);
        }
    }

    private sealed class ReceiptHandler(Guid scenario, Guid reveal) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(TestData.Json(HttpStatusCode.OK, new { scenarioId = scenario, revealId = reveal, status = "Revealed" }));
    }
}
