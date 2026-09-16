using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ConfiguredAnalysisScopeTests
{
    private string _database = null!;
    private static readonly DateTimeOffset T0 = DateTimeOffset.Parse("2026-01-01T00:00:00Z");

    [SetUp]
    public void SetUp() => _database = Path.Combine(AppContext.BaseDirectory, $"configured-analysis-{Guid.NewGuid():N}.db");

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_database))
            File.Delete(_database);
    }

    [Test]
    public async Task ConfiguredRequest_RejectsCrossFieldReservoirAndTimeScope()
    {
        AnalysisPackage source = AnalysisConfigurationTests.CreatePackage();
        ScenarioService scenarios = CreateScenarios(source);
        Scenario scenario = await scenarios.CreateAsync(new(source.FieldId, "Target", T0, "config-scope", new string('a', 64)));
        var service = new ConfiguredAnalysisService(scenarios, new PetrophysicsAnalysisService(TimeProvider.System));
        var request = new ConfiguredAnalysisRequest(AnalysisConfiguration.Default, scenario.ScenarioId);
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(Guid.NewGuid(), request))!.StatusCode,
            Is.EqualTo(409));
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId,
            request with { Reservoir = "Other" }))!.Title, Is.EqualTo("Scenario reservoir mismatch"));
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId,
            request with { AsOf = T0.AddTicks(1) }))!.Title, Is.EqualTo("Invalid scenario time"));
        Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId, request with { AsOf = T0.AddTicks(-1) }));
        Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId, request with { ScenarioId = null, AsOf = T0 }));
        Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId, request with { ScenarioId = Guid.Empty }));
        Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId, request with { Reservoir = " " }));
        Assert.ThrowsAsync<ScenarioApiException>(() => service.AnalyzeAsync(source.FieldId, request with { Configuration = null! }));
    }

    [Test]
    public async Task ConfiguredRequest_UsesImmutableScenarioSnapshotAndCanonicalReservoir()
    {
        AnalysisPackage source = AnalysisConfigurationTests.CreatePackage();
        ScenarioService scenarios = CreateScenarios(source);
        Scenario scenario = await scenarios.CreateAsync(new(source.FieldId, "Target", T0, "config-snapshot", new string('a', 64)));
        var service = new ConfiguredAnalysisService(scenarios, new PetrophysicsAnalysisService(TimeProvider.System));
        var request = new ConfiguredAnalysisRequest(AnalysisConfiguration.Default with { GridPointsPerAxis = 9 },
            scenario.ScenarioId, T0, " target ");
        AnalysisResult first = await service.AnalyzeAsync(source.FieldId, request);
        source.Field["futureUnpublishedEvidence"] = 987;
        source.GeologicalProperties[0]["GeologicalPropertyTable"] = new JsonArray();
        AnalysisResult afterMutation = await service.AnalyzeAsync(source.FieldId, request);
        Assert.Multiple(() =>
        {
            Assert.That(first.ReservoirName, Is.EqualTo("Target"));
            Assert.That(first.CandidateGrid, Has.Count.EqualTo(81));
            Assert.That(afterMutation.PackageSha256, Is.EqualTo(first.PackageSha256));
            Assert.That(afterMutation.AnalysisSha256, Is.EqualTo(first.AnalysisSha256));
            Assert.That(first.Configuration, Is.EqualTo(request.Configuration));
        });
    }

    private ScenarioService CreateScenarios(AnalysisPackage source) => new(
        new SqliteScenarioStore(new SqliteConnectionStringBuilder { DataSource = _database }.ToString()),
        new PackageSource(source), new CanonicalJsonHasher(), TimeProvider.System);

    private sealed class PackageSource(AnalysisPackage package) : IFieldPackageService
    {
        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) => Task.FromResult<JsonNode>(new JsonArray());
        public Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken) => Task.FromResult(package);
    }
}
