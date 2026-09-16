using Microsoft.Data.Sqlite;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class RestrictedTruthSamplingTests
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
    public async Task Sample_SameApprovedBindingAndRequest_IsDeterministicAndAudited()
    {
        (ReservoirWorld world, RestrictedTruthSamplingService sampler, SqliteTruthSamplingRepository repository) =
            await CreateHarnessAsync();
        ApprovedPathBindingMetadata binding = await sampler.RegisterAsync(world, BindingRequest());
        TruthSamplingRequest request = SamplingRequest(binding.BindingId);

        SamplingExecution first = await sampler.SampleAsync(world, request);
        SamplingExecution second = await sampler.SampleAsync(world, request);
        PersistedSamplingAudit audit = (await sampler.LoadVerifiedAuditAsync(first.AuditId))!;

        Assert.Multiple(() =>
        {
            Assert.That(second.ResponseHash, Is.EqualTo(first.ResponseHash));
            Assert.That(second.AuditId, Is.EqualTo(first.AuditId));
            Assert.That(second.Result.SampleCount, Is.EqualTo(first.Result.SampleCount));
            Assert.That(second.Result.Samples, Is.EqualTo(first.Result.Samples));
            Assert.That(audit.SampledCount, Is.EqualTo(first.Result.SampleCount));
            Assert.That(audit.ResponseHash, Is.EqualTo(first.ResponseHash));
            Assert.That(audit.CallerLabel, Is.EqualTo("drilling-operations-stage-b"));
        });

        await using var connection = new SqliteConnection(ConnectionString());
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE SamplingAudit SET SampledCount = $count WHERE AuditId = $auditId;";
        command.Parameters.AddWithValue("$count", audit.SampledCount + 1);
        command.Parameters.AddWithValue("$auditId", audit.AuditId);
        SqliteException? exception = Assert.ThrowsAsync<SqliteException>(async () =>
            await command.ExecuteNonQueryAsync());
        Assert.That(exception!.SqliteErrorCode, Is.Not.Zero);
    }

    [Test]
    public async Task Sample_SmallKnownGrid_InterpolatesPathAndReturnsOnlyNearestActiveCells()
    {
        (ReservoirWorld world, RestrictedTruthSamplingService sampler, _) = await CreateHarnessAsync();
        ApprovedPathBindingMetadata binding = await sampler.RegisterAsync(world, BindingRequest());

        SamplingExecution execution = await sampler.SampleAsync(world, SamplingRequest(binding.BindingId));

        Assert.Multiple(() =>
        {
            Assert.That(execution.Result.SampleCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(execution.Result.SampleCount, Is.LessThanOrEqualTo(world.Grid.CellCount));
            Assert.That(execution.Result.Samples.Select(sample => sample.MeasuredDepthM), Is.Ordered);
            Assert.That(execution.Result.Samples.All(sample =>
                Math.Abs(sample.OilSaturation + sample.WaterSaturation + sample.GasSaturation - 1) < 1e-10), Is.True);
            Assert.That(execution.Result.Samples.All(sample => double.IsFinite(sample.Porosity) &&
                double.IsFinite(sample.PermeabilityM2) && double.IsFinite(sample.PressurePa)), Is.True);
            Assert.That(execution.Result.Samples[0].MeasuredDepthM, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task Sample_DensityAndCapViolations_AreRejected()
    {
        (ReservoirWorld world, RestrictedTruthSamplingService sampler, _) = await CreateHarnessAsync();
        ApprovedPathBindingMetadata binding = await sampler.RegisterAsync(world, BindingRequest());

        Assert.ThrowsAsync<ReservoirValidationException>(async () => await sampler.SampleAsync(world,
            SamplingRequest(binding.BindingId) with { MaximumSpacingM = 0.1 }));
        Assert.ThrowsAsync<ReservoirValidationException>(async () => await sampler.SampleAsync(world,
            SamplingRequest(binding.BindingId) with { MaximumSamples = 10_001 }));
        Assert.ThrowsAsync<ReservoirValidationException>(async () => await sampler.SampleAsync(world,
            SamplingRequest(binding.BindingId) with { MaximumSpacingM = 10, MaximumSamples = 10 }));
    }

    [Test]
    public async Task Binding_WorldMismatchAndOutOfGridPath_AreRejected()
    {
        (ReservoirWorld world, RestrictedTruthSamplingService sampler, SqliteTruthSamplingRepository repository) =
            await CreateHarnessAsync();
        ApprovedPathBindingMetadata binding = await sampler.RegisterAsync(world, BindingRequest());
        var worldRepository = new SqliteReservoirRepository(ConnectionString());
        var manager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(4), worldRepository, TimeProvider.System);
        ReservoirWorld otherWorld = await manager.CreateAsync(
            TestData.UniformWorldRequest() with { FieldId = Guid.Parse("c64ebef5-e820-4194-8810-d6a5d08d1138") });

        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await sampler.SampleAsync(otherWorld, SamplingRequest(binding.BindingId)));
        ApprovedPathBindingRequest outside = BindingRequest() with
        {
            Stations =
            [
                Station(0, -10_000, 0, 1_005),
                Station(100, -9_000, 0, 1_010)
            ]
        };
        Assert.ThrowsAsync<ReservoirValidationException>(async () =>
            await sampler.RegisterAsync(world, outside));
    }

    [Test]
    public async Task Binding_PersistsAcrossRestart_AndConflictingIdentityFails()
    {
        (ReservoirWorld world, RestrictedTruthSamplingService sampler, SqliteTruthSamplingRepository repository) =
            await CreateHarnessAsync();
        ApprovedPathBindingMetadata metadata = await sampler.RegisterAsync(world, BindingRequest());

        var restartedRepository = new SqliteTruthSamplingRepository(ConnectionString());
        await restartedRepository.InitializeAsync();
        var restarted = new RestrictedTruthSamplingService(restartedRepository, TimeProvider.System);
        RestoredPathBinding restored = (await restarted.LoadBindingAsync(world, metadata.BindingId))!;
        PersistedPathBinding persisted = (await repository.LoadBindingAsync(metadata.BindingId))!;
        PersistedPathBinding conflict = persisted with { CanonicalHash = new string('f', 64) };

        Assert.Multiple(() =>
        {
            Assert.That(restored.Metadata, Is.EqualTo(metadata));
            Assert.That(restored.Stations, Is.EqualTo(BindingRequest().Stations));
        });
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () =>
            await repository.SaveBindingAsync(conflict));
    }

    [Test]
    public async Task Binding_SurfaceCollarCanDescendToReservoir_WhileAllAbovePathSamplesNothing()
    {
        (ReservoirWorld world, RestrictedTruthSamplingService sampler, _) = await CreateHarnessAsync();
        ApprovedPathBindingRequest surfaceToReservoir = BindingRequest() with
        {
            Stations =
            [
                Station(0, 0, 0, 0),
                Station(1_100, 350, 0, 1_010),
                Station(1_200, 700, 0, 1_025)
            ]
        };
        ApprovedPathBindingMetadata intersecting = await sampler.RegisterAsync(world, surfaceToReservoir);
        SamplingExecution sampled = await sampler.SampleAsync(world, SamplingRequest(intersecting.BindingId) with
        {
            MaximumSpacingM = 10,
            MaximumSamples = 1_000
        });

        ApprovedPathBindingRequest aboveReservoir = BindingRequest() with
        {
            RunId = Guid.Parse("569151a3-c97d-4fb9-9a34-2ed3b03f8330"),
            Stations =
            [
                Station(0, 0, 0, 0),
                Station(800, 700, 0, 500)
            ]
        };
        ApprovedPathBindingMetadata above = await sampler.RegisterAsync(world, aboveReservoir);

        Assert.That(sampled.Result.SampleCount, Is.GreaterThan(0));
        ReservoirValidationException? exception = Assert.ThrowsAsync<ReservoirValidationException>(
            async () => await sampler.SampleAsync(world, SamplingRequest(above.BindingId) with
            {
                MaximumSamples = 1_000
            }));
        Assert.That(exception!.Errors.Values.SelectMany(messages => messages),
            Has.Some.Contains("does not intersect"));
    }


    private async Task<(ReservoirWorld World, RestrictedTruthSamplingService Sampler,
        SqliteTruthSamplingRepository Repository)> CreateHarnessAsync()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-truth-{Guid.NewGuid():N}.db");
        var worldRepository = new SqliteReservoirRepository(ConnectionString());
        await worldRepository.InitializeAsync();
        var truthRepository = new SqliteTruthSamplingRepository(ConnectionString());
        await truthRepository.InitializeAsync();
        var manager = new PersistentWorldManager(
            new ReservoirWorldFactory(), new InMemoryWorldStore(4), worldRepository, TimeProvider.System);
        ReservoirWorld world = await manager.CreateAsync(TestData.UniformWorldRequest(4, 3, 2));
        return (world, new RestrictedTruthSamplingService(truthRepository, TimeProvider.System), truthRepository);
    }

    private string ConnectionString() => $"Data Source={_databasePath}";

    private static ApprovedPathBindingRequest BindingRequest() => new()
    {
        ScenarioId = Guid.Parse("250738f1-e044-4751-a0ca-cda12022479a"),
        RunId = Guid.Parse("f495e5ec-054d-4cac-b46b-6cbdcf19d488"),
        PathKind = ApprovedPathKind.Planned,
        ApprovedSealedPredictionSha256 =
            "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
        Stations =
        [
            Station(0, 0, 0, 1_002),
            Station(700, 700, 0, 1_028)
        ]
    };

    private static TruthSamplingRequest SamplingRequest(string bindingId) => new()
    {
        BindingId = bindingId,
        MaximumSpacingM = 25,
        MaximumSamples = 100,
        PropertySetVersion = "stage-b-truth-v1",
        CallerLabel = "drilling-operations-stage-b"
    };

    private static ApprovedPathStation Station(
        double md, double easting, double northing, double tvd) => new()
        {
            MeasuredDepthM = md,
            EastingM = easting,
            NorthingM = northing,
            TrueVerticalDepthM = tvd
        };
}
