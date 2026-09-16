using Microsoft.Data.Sqlite;
using System.Text.Json;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class CompletionBindingTests
{
    private string? _databasePath;

    [TearDown]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();
        if (_databasePath is not null && File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    [Test]
    public void CanonicalRequest_FromStringEnumHttpContract_UsesNumericInternalTypeAndStableHash()
    {
        string pathBindingId = "rpb_" + new string('a', 64);
        ApprovedCompletionBindingRequest request = CompletionRequest(pathBindingId);
        string canonicalJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        const string expectedJson =
            "{\"pathBindingId\":\"rpb_aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"," +
            "\"completionModelVersion\":\"observed-log-completion-v1\",\"openings\":[{" +
            "\"openingId\":\"main-perf\",\"reservoirName\":\"Uniform Sand\",\"type\":0," +
            "\"topMdM\":0,\"baseMdM\":700,\"wellboreRadiusM\":0.12,\"skin\":2," +
            "\"efficiency\":0.75,\"uncertaintyM\":1}]}";

        Assert.Multiple(() =>
        {
            Assert.That(canonicalJson, Is.EqualTo(expectedJson));
            Assert.That(DeterministicEncoding.Sha256Hex(canonicalJson),
                Is.EqualTo("a55031ca5e8e3f597d78412eaa946f8c68d24424c57cca3deb3c5d57210d8d84"));
        });
    }


    [Test]
    public async Task RegisterAndResolve_MapsUniqueActivePeacemanConnectionsWithoutLeakingMetadata()
    {
        Harness harness = await CreateHarnessAsync(ApprovedPathKind.AsDrilled);
        ApprovedCompletionBindingRequest request = CompletionRequest(harness.Path.BindingId);

        ApprovedCompletionBindingMetadata metadata = await harness.Completions.RegisterAsync(
            harness.World, request);
        ResolvedCompletionBinding resolved = (await harness.Completions.ResolveAsync(
            harness.World, metadata.CompletionBindingId))!;
        WellConnection[] connections = resolved.Openings.SelectMany(opening => opening.Connections).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(metadata.OpeningCount, Is.EqualTo(1));
            Assert.That(metadata.ProducingConnectionCount, Is.GreaterThan(1));
            Assert.That(metadata.ProducingConnectionCount, Is.EqualTo(connections.Length));
            Assert.That(metadata.ProducingConnectionCount, Is.LessThanOrEqualTo(10_000));
            Assert.That(connections.Select(connection => (connection.I, connection.J, connection.K)).Distinct().Count(),
                Is.EqualTo(connections.Length));
            Assert.That(connections, Has.All.Property(nameof(WellConnection.WellboreRadiusM)).EqualTo(0.12));
            Assert.That(connections, Has.All.Property(nameof(WellConnection.Skin)).EqualTo(2));
            Assert.That(connections, Has.All.Property(nameof(WellConnection.OpenFraction)).EqualTo(0.75));
        });
    }

    [Test]
    public async Task Register_RequiresAsDrilledPathAndContainedIntervals()
    {
        Harness planned = await CreateHarnessAsync(ApprovedPathKind.Planned);
        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await planned.Completions.RegisterAsync(
                planned.World, CompletionRequest(planned.Path.BindingId)));

        ApprovedCompletionBindingRequest outside = CompletionRequest(planned.Path.BindingId) with
        {
            Openings =
            [
                Opening("outside", -1, 100, CompletionOpeningType.Perforated)
            ]
        };
        Harness asDrilled = await CreateHarnessAsync(ApprovedPathKind.AsDrilled, reuseDatabase: true);
        outside = outside with { PathBindingId = asDrilled.Path.BindingId };
        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await asDrilled.Completions.RegisterAsync(asDrilled.World, outside));
    }

    [Test]
    public async Task Register_RejectsProducingOverlapAndFullyIsolatedOpening()
    {
        Harness harness = await CreateHarnessAsync(ApprovedPathKind.AsDrilled);
        ApprovedCompletionBindingRequest overlap = CompletionRequest(harness.Path.BindingId) with
        {
            Openings =
            [
                Opening("p1", 0, 400, CompletionOpeningType.Perforated),
                Opening("p2", 300, 600, CompletionOpeningType.OpenHole)
            ]
        };
        ApprovedCompletionBindingRequest isolated = CompletionRequest(harness.Path.BindingId) with
        {
            Openings =
            [
                Opening("p1", 200, 300, CompletionOpeningType.Perforated),
                Opening("iso", 200, 300, CompletionOpeningType.Isolated) with { Efficiency = 0 }
            ]
        };

        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await harness.Completions.RegisterAsync(harness.World, overlap));
        ReservoirValidationException? isolationError = Assert.ThrowsAsync<ReservoirValidationException>(
            async () => await harness.Completions.RegisterAsync(harness.World, isolated));
        Assert.That(isolationError!.Errors.Values.SelectMany(messages => messages),
            Has.Some.Contains("after isolation exclusions"));
    }

    [Test]
    public async Task Register_AdjacentProducingIntervalsAroundNarrowIsolation_ExcludeIsolationCells()
    {
        Harness harness = await CreateHarnessAsync(ApprovedPathKind.AsDrilled);
        ApprovedCompletionBindingRequest isolatedDesign = CompletionRequest(harness.Path.BindingId) with
        {
            Openings =
            [
                Opening("upper", 0, 300, CompletionOpeningType.Perforated),
                Opening("narrow-isolation", 300, 306.46, CompletionOpeningType.Isolated) with
                {
                    Efficiency = 0
                },
                Opening("lower", 306.46, 700, CompletionOpeningType.OpenHole)
            ]
        };
        ApprovedCompletionBindingMetadata metadata = await harness.Completions.RegisterAsync(
            harness.World, isolatedDesign);
        ResolvedCompletionBinding resolved = (await harness.Completions.ResolveAsync(
            harness.World, metadata.CompletionBindingId))!;

        ApprovedCompletionBindingRequest isolationProbe = CompletionRequest(harness.Path.BindingId) with
        {
            Openings =
            [
                Opening("isolation-probe", 300, 306.46, CompletionOpeningType.Perforated)
            ]
        };
        ApprovedCompletionBindingMetadata probeMetadata = await harness.Completions.RegisterAsync(
            harness.World, isolationProbe);
        ResolvedCompletionBinding probe = (await harness.Completions.ResolveAsync(
            harness.World, probeMetadata.CompletionBindingId))!;
        HashSet<(int I, int J, int K)> isolatedCells = probe.Openings
            .SelectMany(opening => opening.Connections)
            .Select(connection => (connection.I, connection.J, connection.K))
            .ToHashSet();
        WellConnection[] producingConnections = resolved.Openings
            .SelectMany(opening => opening.Connections)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(resolved.Openings, Has.Count.EqualTo(2));
            Assert.That(resolved.Openings.All(opening => opening.Connections.Count > 0), Is.True);
            Assert.That(producingConnections.Any(connection =>
                isolatedCells.Contains((connection.I, connection.J, connection.K))), Is.False);
            Assert.That(producingConnections.Select(connection => (connection.I, connection.J, connection.K))
                .Distinct().Count(), Is.EqualTo(producingConnections.Length));
            Assert.That(metadata.ProducingConnectionCount, Is.EqualTo(producingConnections.Length));
        });
    }


    [Test]
    public async Task Binding_RestartIsDeterministicIdempotentAndImmutableWithAudits()
    {
        Harness harness = await CreateHarnessAsync(ApprovedPathKind.AsDrilled);
        ApprovedCompletionBindingRequest request = CompletionRequest(harness.Path.BindingId);
        ApprovedCompletionBindingMetadata first = await harness.Completions.RegisterAsync(harness.World, request);
        ApprovedCompletionBindingMetadata second = await harness.Completions.RegisterAsync(harness.World, request);

        var restartedRepository = new SqliteCompletionBindingRepository(ConnectionString());
        await restartedRepository.InitializeAsync();
        var restarted = new ApprovedCompletionBindingService(
            restartedRepository, harness.PathService, TimeProvider.System);
        ResolvedCompletionBinding restored = (await restarted.ResolveAsync(
            harness.World, first.CompletionBindingId))!;
        PersistedCompletionBinding persisted = (await restartedRepository.LoadBindingAsync(
            first.CompletionBindingId))!;

        Assert.Multiple(() =>
        {
            Assert.That(second.CompletionBindingId, Is.EqualTo(first.CompletionBindingId));
            Assert.That(restored.Metadata, Is.EqualTo(first));
            Assert.That(restored.Openings.Sum(opening => opening.Connections.Count),
                Is.EqualTo(first.ProducingConnectionCount));
        });
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () =>
            await restartedRepository.SaveBindingAsync(persisted with
            {
                ApprovedPredictionSha256 = new string('e', 64)
            }));

        await using var connection = new SqliteConnection(ConnectionString());
        await connection.OpenAsync();
        await using SqliteCommand tamper = connection.CreateCommand();
        tamper.CommandText =
            "UPDATE ApprovedCompletionBindings SET OpeningCount = $count WHERE CompletionBindingId = $id;";
        tamper.Parameters.AddWithValue("$count", 99);
        tamper.Parameters.AddWithValue("$id", first.CompletionBindingId);
        Assert.ThrowsAsync<SqliteException>(async () => await tamper.ExecuteNonQueryAsync());

        await using SqliteCommand audits = connection.CreateCommand();
        audits.CommandText =
            "SELECT COUNT(*), COUNT(DISTINCT Action) FROM CompletionBindingAudit WHERE CompletionBindingId = $id;";
        audits.Parameters.AddWithValue("$id", first.CompletionBindingId);
        await using SqliteDataReader reader = await audits.ExecuteReaderAsync();
        Assert.That(await reader.ReadAsync(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(reader.GetInt32(0), Is.EqualTo(2));
            Assert.That(reader.GetInt32(1), Is.EqualTo(2));
        });
        await reader.DisposeAsync();
        await using SqliteCommand auditTamper = connection.CreateCommand();
        auditTamper.CommandText =
            "UPDATE CompletionBindingAudit SET ConnectionCount = $count WHERE CompletionBindingId = $id;";
        auditTamper.Parameters.AddWithValue("$count", 99);
        auditTamper.Parameters.AddWithValue("$id", first.CompletionBindingId);
        Assert.ThrowsAsync<SqliteException>(async () => await auditTamper.ExecuteNonQueryAsync());
    }

    [Test]
    public async Task Resolve_WorldMismatchFailsAndWorldDeletionRemainsBlocked()
    {
        Harness harness = await CreateHarnessAsync(ApprovedPathKind.AsDrilled);
        ApprovedCompletionBindingMetadata metadata = await harness.Completions.RegisterAsync(
            harness.World, CompletionRequest(harness.Path.BindingId));
        var worldRepository = new SqliteReservoirRepository(ConnectionString());
        var worldManager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(4), worldRepository, TimeProvider.System);
        ReservoirWorld other = await worldManager.CreateAsync(TestData.UniformWorldRequest() with
        {
            FieldId = Guid.Parse("bcfd4d41-a207-4078-a187-eb9d389bc775")
        });

        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await harness.Completions.ResolveAsync(other, metadata.CompletionBindingId));
        Assert.That(await harness.PathService.WorldHasBindingsAsync(harness.World.Summary.WorldId), Is.True);
    }

    private async Task<Harness> CreateHarnessAsync(
        ApprovedPathKind pathKind, bool reuseDatabase = false)
    {
        if (!reuseDatabase || _databasePath is null)
            _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-completion-{Guid.NewGuid():N}.db");
        var worldRepository = new SqliteReservoirRepository(ConnectionString());
        await worldRepository.InitializeAsync();
        var pathRepository = new SqliteTruthSamplingRepository(ConnectionString());
        await pathRepository.InitializeAsync();
        var completionRepository = new SqliteCompletionBindingRepository(ConnectionString());
        await completionRepository.InitializeAsync();
        var worldManager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(4), worldRepository, TimeProvider.System);
        ReservoirWorld world = await worldManager.CreateAsync(TestData.UniformWorldRequest(4, 3, 2));
        var pathService = new RestrictedTruthSamplingService(pathRepository, TimeProvider.System);
        ApprovedPathBindingMetadata path = await pathService.RegisterAsync(world, PathRequest(pathKind));
        var completions = new ApprovedCompletionBindingService(
            completionRepository, pathService, TimeProvider.System);
        return new Harness(world, path, pathService, completions);
    }

    private string ConnectionString() => $"Data Source={_databasePath}";

    private static ApprovedPathBindingRequest PathRequest(ApprovedPathKind kind) => new()
    {
        ScenarioId = Guid.Parse("48310cd0-97b1-4aa6-af1b-ae46ec5f862b"),
        RunId = Guid.Parse("3b2a124d-600d-4ff2-9985-0463a5e42362"),
        PathKind = kind,
        ApprovedSealedPredictionSha256 =
            "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
        Stations =
        [
            new ApprovedPathStation
            {
                MeasuredDepthM = 0, EastingM = 0, NorthingM = 0, TrueVerticalDepthM = 1_002
            },
            new ApprovedPathStation
            {
                MeasuredDepthM = 700, EastingM = 700, NorthingM = 0, TrueVerticalDepthM = 1_028
            }
        ]
    };

    private static ApprovedCompletionBindingRequest CompletionRequest(string pathBindingId) => new()
    {
        PathBindingId = pathBindingId,
        CompletionModelVersion = "observed-log-completion-v1",
        Openings = [Opening("main-perf", 0, 700, CompletionOpeningType.Perforated)]
    };

    private static ApprovedCompletionOpening Opening(
        string id, double topMd, double baseMd, CompletionOpeningType type) => new()
        {
            OpeningId = id,
            ReservoirName = "Uniform Sand",
            Type = type,
            TopMeasuredDepthM = topMd,
            BaseMeasuredDepthM = baseMd,
            WellboreRadiusM = 0.12,
            Skin = 2,
            Efficiency = 0.75,
            UncertaintyM = 1
        };

    private sealed record Harness(
        ReservoirWorld World,
        ApprovedPathBindingMetadata Path,
        RestrictedTruthSamplingService PathService,
        ApprovedCompletionBindingService Completions);
}
