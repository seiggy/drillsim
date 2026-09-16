using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ScenarioServiceTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
    private string _databasePath = null!;
    private string _connectionString = null!;

    [SetUp]
    public void SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-analysis-{Guid.NewGuid():N}.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = _databasePath }.ToString();
    }

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    [Test]
    public async Task Create_IsDeterministicAndIdempotent()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService service = CreateService(store, testPackage.Package);
        var request = new CreateScenarioRequest(
            testPackage.FieldId,
            "Target",
            T0,
            "public-seed-label",
            new string('a', 64));

        Scenario first = await service.CreateAsync(request);
        Scenario retry = await service.CreateAsync(request with
        {
            ReservoirName = " Target ",
            AsOfUtc = T0.ToOffset(TimeSpan.FromHours(2)),
            SeedLabel = " public-seed-label ",
            AssumptionsSha256 = new string('A', 64)
        });
        IReadOnlyList<Scenario> scenarios = await store.ListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(retry, Is.EqualTo(first));
            Assert.That(scenarios, Has.Count.EqualTo(1));
            Assert.That(first.ScenarioId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(first.InitialAsOfUtc, Is.EqualTo(T0));
            Assert.That(first.AsOfUtc, Is.EqualTo(first.InitialAsOfUtc));
            Assert.That(first.Status, Is.EqualTo(ScenarioStatus.Draft));
        });
    }

    [Test]
    public async Task Store_PersistsAcrossRestart()
    {
        TestPackage testPackage = CreateCompletePackage();
        var firstStore = new SqliteScenarioStore(_connectionString);
        Scenario created = await CreateService(firstStore, testPackage.Package).CreateAsync(StandardRequest(testPackage.FieldId));

        var restartedStore = new SqliteScenarioStore(_connectionString);
        Scenario? reloaded = await restartedStore.FindAsync(created.ScenarioId);
        IReadOnlyList<EvidenceVisibility> visibility =
            await restartedStore.GetEvidenceVisibilityAsync(created.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(reloaded, Is.EqualTo(created));
            Assert.That(visibility, Has.Count.EqualTo(7));
        });
    }

    [Test]
    public async Task Package_RejectsAsOfAfterCurrentScenarioClock()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = await CreateService(store, testPackage.Package)
            .CreateAsync(StandardRequest(testPackage.FieldId));
        ScenarioService service = CreateService(store, testPackage.Package);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.GetPackageAsync(testPackage.FieldId, scenario.ScenarioId, T0.AddTicks(1)))!;

        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.Title, Is.EqualTo("Invalid scenario time"));
    }

    [Test]
    public async Task Package_RejectsAsOfBeforeInitialScenarioTime()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(testPackage.FieldId, T0.AddDays(1));
        await store.CreateLegacyScenarioAsync(scenario, []);
        ScenarioService service = CreateService(store, testPackage.Package);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.GetPackageAsync(testPackage.FieldId, scenario.ScenarioId, T0.AddTicks(-1)))!;

        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.Title, Is.EqualTo("Invalid scenario time"));
    }

    [Test]
    public async Task Package_AcceptsHistoricalInitialTimeAfterClockAdvances()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(testPackage.FieldId, T0.AddDays(1));
        await store.CreateLegacyScenarioAsync(scenario,
        [
            Visibility(scenario, "field", testPackage.FieldId, T0),
            Visibility(scenario, "cluster", testPackage.ClusterId, T0)
        ]);
        await store.BackfillScenarioPackageSnapshotAsync(scenario, testPackage.Package, T0);
        ScenarioService service = CreateService(store, testPackage.Package);

        AnalysisPackage package = await service.GetPackageAsync(
            testPackage.FieldId,
            scenario.ScenarioId,
            T0);

        Assert.That(package.Clusters, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Store_PersistsDistinctInitialTimeAcrossRestart()
    {
        TestPackage testPackage = CreateCompletePackage();
        var firstStore = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(testPackage.FieldId, T0.AddDays(1));
        await firstStore.CreateLegacyScenarioAsync(scenario, []);

        var restartedStore = new SqliteScenarioStore(_connectionString);
        Scenario? reloaded = await restartedStore.FindAsync(scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.InitialAsOfUtc, Is.EqualTo(T0));
            Assert.That(reloaded.AsOfUtc, Is.EqualTo(T0.AddDays(1)));
        });
    }

    [Test]
    public async Task Store_MigratesLegacySchemaAndBackfillsInitialTime()
    {
        Guid scenarioId = Guid.NewGuid();
        Guid fieldId = Guid.NewGuid();
        await CreateLegacyScenarioDatabaseAsync(scenarioId, fieldId, T0.ToString("O"));

        var store = new SqliteScenarioStore(_connectionString);
        Scenario? migrated = await store.FindAsync(scenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(migrated, Is.Not.Null);
            Assert.That(migrated!.InitialAsOfUtc, Is.EqualTo(T0));
            Assert.That(migrated.AsOfUtc, Is.EqualTo(T0));
        });
    }

    [Test]
    public void Store_RejectsCorruptLegacyScenarioTime()
    {
        Guid scenarioId = Guid.NewGuid();
        Assert.That(async () =>
        {
            await CreateLegacyScenarioDatabaseAsync(scenarioId, Guid.NewGuid(), "not-a-timestamp");
            var store = new SqliteScenarioStore(_connectionString);
            await store.InitializeAsync();
        }, Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public void ScenarioModelVersions_UsesFrozenVersions()
    {
        Assert.That(ScenarioModelVersions.World, Is.EqualTo("reservoir-hidden-world-v3"));
        Assert.That(ScenarioModelVersions.Scoring, Is.EqualTo("scoring-model-v1"));
    }

    [Test]
    public async Task Create_CapturesEveryInitialEvidenceIdExactlyOnce()
    {
        TestPackage testPackage = CreateCompletePackage(duplicateCluster: true);
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService service = CreateService(store, testPackage.Package);

        Scenario scenario = await service.CreateAsync(StandardRequest(testPackage.FieldId));
        await service.CreateAsync(StandardRequest(testPackage.FieldId));
        IReadOnlyList<EvidenceVisibility> visibility =
            await store.GetEvidenceVisibilityAsync(scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(visibility, Has.Count.EqualTo(7));
            Assert.That(visibility.Select(item => item.EvidenceId), Is.Unique);
            Assert.That(visibility.Select(item => item.RecordKind), Is.EquivalentTo(new[]
            {
                "field", "cluster", "well", "wellbore", "architecture", "trajectory", "geology"
            }));
            Assert.That(visibility.All(item => item.VisibleFromUtc == T0), Is.True);
        });
    }

    [Test]
    public async Task Package_FiltersIntervalsAndTreatsMissingRowsAsHidden()
    {
        TestPackage testPackage = CreateCompletePackage(includeSecondCluster: true);
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(testPackage.FieldId, T0.AddHours(1));
        EvidenceVisibility[] rows =
        [
            Visibility(scenario, "field", testPackage.FieldId, T0),
            Visibility(scenario, "cluster", testPackage.ClusterId, T0, T0.AddHours(1)),
            Visibility(scenario, "cluster", testPackage.SecondClusterId!.Value, T0.AddHours(1)),
            Visibility(scenario, "well", testPackage.WellId, T0)
            // The other current package records intentionally have no visibility row.
        ];
        await store.CreateLegacyScenarioAsync(scenario, rows);
        await store.BackfillScenarioPackageSnapshotAsync(scenario, testPackage.Package, T0);
        ScenarioService service = CreateService(store, testPackage.Package);

        AnalysisPackage atT0 = await service.GetPackageAsync(testPackage.FieldId, scenario.ScenarioId, T0);
        AnalysisPackage atT1 = await service.GetPackageAsync(testPackage.FieldId, scenario.ScenarioId, T0.AddHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(atT0.Clusters.Select(MetaId), Is.EqualTo(new[] { testPackage.ClusterId }));
            Assert.That(atT1.Clusters.Select(MetaId), Is.EqualTo(new[] { testPackage.SecondClusterId!.Value }));
            Assert.That(atT0.Wells, Has.Count.EqualTo(1));
            Assert.That(atT0.WellBores, Is.Empty);
            Assert.That(atT0.GeologicalProperties, Is.Empty);
            Assert.That(atT0.SourceCounts, Is.EqualTo(new SourceCounts(1, 1, 1, 0, 0, 0, 0)));
            Assert.That(atT0.Sha256, Is.Not.EqualTo(testPackage.Package.Sha256));
        });
    }

    [Test]
    public async Task Package_DoesNotExposeFutureEvidenceAtT0()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(testPackage.FieldId);
        await store.CreateLegacyScenarioAsync(scenario,
        [
            Visibility(scenario, "field", testPackage.FieldId, T0),
            Visibility(scenario, "cluster", testPackage.ClusterId, T0.AddMinutes(1))
        ]);
        await store.BackfillScenarioPackageSnapshotAsync(scenario, testPackage.Package, T0);
        ScenarioService service = CreateService(store, testPackage.Package);

        AnalysisPackage package = await service.GetPackageAsync(testPackage.FieldId, scenario.ScenarioId, T0);

        Assert.That(package.Clusters, Is.Empty);
        Assert.That(package.SourceCounts.Clusters, Is.Zero);
    }

    [Test]
    public async Task Package_WithoutScenarioIsReturnedUnchanged()
    {
        TestPackage testPackage = CreateCompletePackage();
        ScenarioService service = CreateService(new SqliteScenarioStore(_connectionString), testPackage.Package);

        AnalysisPackage result = await service.GetPackageAsync(
            testPackage.FieldId,
            scenarioId: null,
            asOf: T0.AddYears(10));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(testPackage.Package));
            Assert.That(result.Sha256, Is.EqualTo(testPackage.Package.Sha256));
        });
    }

    [Test]
    public async Task Package_RejectsFieldThatDoesNotBelongToScenario()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = await CreateService(store, testPackage.Package).CreateAsync(StandardRequest(testPackage.FieldId));
        ScenarioService service = CreateService(store, testPackage.Package);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.GetPackageAsync(Guid.NewGuid(), scenario.ScenarioId, null))!;

        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.Title, Is.EqualTo("Scenario field mismatch"));
    }

    [Test]
    public async Task VisibilitySummary_ReportsHiddenCountsWithoutHiddenIds()
    {
        TestPackage testPackage = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(testPackage.FieldId);
        await store.CreateLegacyScenarioAsync(scenario,
        [
            Visibility(scenario, "field", testPackage.FieldId, T0),
            Visibility(scenario, "cluster", testPackage.ClusterId, T0.AddMinutes(1))
        ]);
        await store.BackfillScenarioPackageSnapshotAsync(scenario, testPackage.Package, T0);
        ScenarioService service = CreateService(store, testPackage.Package);

        EvidenceVisibilitySummary summary = await service.GetVisibilitySummaryAsync(scenario.ScenarioId, T0);
        string json = JsonSerializer.Serialize(summary);
        EvidenceVisibilityCount cluster = summary.Counts.Single(item => item.RecordKind == "cluster");

        Assert.Multiple(() =>
        {
            Assert.That(cluster.Status, Is.EqualTo(EvidenceVisibilityStatus.Hidden));
            Assert.That(cluster.Count, Is.EqualTo(1));
            Assert.That(cluster.EvidenceIds, Is.Null);
            Assert.That(json, Does.Not.Contain(testPackage.ClusterId.ToString("D")));
            Assert.That(json, Does.Not.Contain(testPackage.WellId.ToString("D")));
            Assert.That(json, Does.Contain(testPackage.FieldId.ToString("D")));
        });
    }

    [Test]
    public async Task ScenarioCreationSnapshot_PreventsSameIdMutationLeaksAcrossRestart()
    {
        TestPackage initial = CreateCompletePackage();
        var packages = new MutableFieldPackageService(initial.Package);
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService service = CreateService(store, packages);
        Scenario scenario = await service.CreateAsync(StandardRequest(initial.FieldId));
        AnalysisPackage before = await service.GetPackageAsync(
            initial.FieldId,
            scenario.ScenarioId,
            scenario.AsOfUtc);
        var analysis = new PetrophysicsAnalysisService(new FixedTimeProvider(T0));
        AnalysisResult analysisBefore = analysis.Analyze(before, scenario.ReservoirName);

        TestPackage changed = CreateCompletePackage(includeSecondCluster: true);
        changed.Package.Field["futureValue"] = "must-not-leak";
        changed.Package.Clusters[0]["futureValue"] = 42;
        packages.Current = Rehash(changed.Package);
        var restartedStore = new SqliteScenarioStore(_connectionString);
        ScenarioService restarted = CreateService(restartedStore, packages);

        AnalysisPackage after = await restarted.GetPackageAsync(
            initial.FieldId,
            scenario.ScenarioId,
            scenario.AsOfUtc);
        AnalysisResult analysisAfter = analysis.Analyze(after, scenario.ReservoirName);
        EvidenceVisibilitySummary visibility =
            await restarted.GetVisibilitySummaryAsync(scenario.ScenarioId);
        ScenarioPackageSnapshot snapshot =
            (await restartedStore.FindScenarioPackageSnapshotAsync(scenario))!;

        Assert.Multiple(() =>
        {
            Assert.That(JsonSerializer.Serialize(after), Is.EqualTo(JsonSerializer.Serialize(before)));
            Assert.That(after.Sha256, Is.EqualTo(before.Sha256));
            Assert.That(after.SourceCounts, Is.EqualTo(before.SourceCounts));
            Assert.That(JsonSerializer.Serialize(analysisAfter), Is.EqualTo(JsonSerializer.Serialize(analysisBefore)));
            Assert.That(visibility.Counts.Count(item =>
                item.RecordKind == EvidenceCatalog.Cluster &&
                item.Status == EvidenceVisibilityStatus.Hidden), Is.Zero);
            Assert.That(snapshot.Origin, Is.EqualTo(ScenarioPackageSnapshotOrigin.ScenarioCreation));
            Assert.That(snapshot.ScenarioId, Is.EqualTo(scenario.ScenarioId));
            Assert.That(snapshot.FieldId, Is.EqualTo(scenario.SourceFieldId));
            Assert.That(snapshot.AsOfUtc, Is.EqualTo(scenario.InitialAsOfUtc));
            Assert.That(snapshot.CreatedUtc, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(snapshot.Package.Field["futureValue"], Is.Null);
        });
    }

    [Test]
    public async Task LegacyScenario_UnchangedScoredReadBackfillsOnceAndThenRemainsImmutable()
    {
        TestPackage initial = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(initial.FieldId);
        await store.CreateLegacyScenarioAsync(
            scenario,
            EvidenceCatalog.Enumerate(initial.Package)
                .Select(item => new EvidenceVisibility(
                    scenario.ScenarioId,
                    item.EvidenceId,
                    item.RecordKind,
                    T0,
                    null,
                    null))
                .ToArray());
        await SealLegacyScenarioAsync(store, scenario, initial.Package.Sha256);
        await SetScenarioStatusAsync(scenario.ScenarioId, ScenarioStatus.Scored);
        var packages = new MutableFieldPackageService(initial.Package);
        ScenarioService service = CreateService(store, packages);

        AnalysisPackage first = await service.GetPackageAsync(
            initial.FieldId,
            scenario.ScenarioId,
            scenario.AsOfUtc);
        TestPackage changed = CreateCompletePackage(includeSecondCluster: true);
        changed.Package.Field["futureValue"] = "must-not-leak";
        packages.Current = Rehash(changed.Package);
        AnalysisPackage second = await service.GetPackageAsync(
            initial.FieldId,
            scenario.ScenarioId,
            scenario.AsOfUtc);
        ScenarioPackageSnapshot snapshot =
            (await store.FindScenarioPackageSnapshotAsync(scenario))!;

        Assert.Multiple(() =>
        {
            Assert.That(JsonSerializer.Serialize(second), Is.EqualTo(JsonSerializer.Serialize(first)));
            Assert.That(packages.BuildCount, Is.EqualTo(1));
            Assert.That(snapshot.Origin, Is.EqualTo(ScenarioPackageSnapshotOrigin.LegacyBackfill));
            Assert.That(snapshot.Package.Field["futureValue"], Is.Null);
        });
    }

    [Test]
    public async Task LegacyScenario_DraftWithoutSealFailsClosed()
    {
        TestPackage initial = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(initial.FieldId);
        await store.CreateLegacyScenarioAsync(scenario, []);
        ScenarioService service = CreateService(store, initial.Package);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.GetPackageAsync(initial.FieldId, scenario.ScenarioId, T0))!;
        ScenarioPackageSnapshot? snapshot = await store.FindScenarioPackageSnapshotAsync(scenario);

        Assert.Multiple(() =>
        {
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            Assert.That(exception.Title, Is.EqualTo("Legacy scenario snapshot unavailable"));
            Assert.That(snapshot, Is.Null);
        });
    }

    [Test]
    public async Task LegacyScenario_MutationBeforeFirstReadIsRejectedWithoutSnapshot()
    {
        TestPackage initial = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(initial.FieldId);
        await store.CreateLegacyScenarioAsync(scenario, []);
        await SealLegacyScenarioAsync(store, scenario, initial.Package.Sha256);
        AnalysisPackage changed = initial.Package with { Field = initial.Package.Field.DeepClone() };
        changed.Field["futureValue"] = "must-not-be-blessed";
        changed = Rehash(changed);
        ScenarioService service = CreateService(store, changed);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.GetPackageAsync(initial.FieldId, scenario.ScenarioId, T0))!;
        ScenarioPackageSnapshot? snapshot = await store.FindScenarioPackageSnapshotAsync(scenario);

        Assert.Multiple(() =>
        {
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            Assert.That(exception.Title, Is.EqualTo("Legacy scenario snapshot unavailable"));
            Assert.That(snapshot, Is.Null);
        });
    }

    [Test]
    public async Task LegacyScenario_ConcurrentIdenticalBackfillsConverge()
    {
        TestPackage initial = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(initial.FieldId);
        await store.CreateLegacyScenarioAsync(
            scenario,
            EvidenceCatalog.Enumerate(initial.Package)
                .Select(item => new EvidenceVisibility(
                    scenario.ScenarioId,
                    item.EvidenceId,
                    item.RecordKind,
                    T0,
                    null,
                    null))
                .ToArray());
        await SealLegacyScenarioAsync(store, scenario, initial.Package.Sha256);
        var packages = new BarrierPackageService(initial.Package, initial.Package);
        ScenarioService firstService = CreateService(store, packages);
        ScenarioService secondService = CreateService(store, packages);

        AnalysisPackage[] results = await Task.WhenAll(
            firstService.GetPackageAsync(initial.FieldId, scenario.ScenarioId, T0),
            secondService.GetPackageAsync(initial.FieldId, scenario.ScenarioId, T0));
        ScenarioPackageSnapshot snapshot =
            (await store.FindScenarioPackageSnapshotAsync(scenario))!;

        Assert.Multiple(() =>
        {
            Assert.That(results.Select(item => item.Sha256), Is.All.EqualTo(initial.Package.Sha256));
            Assert.That(packages.BuildCount, Is.EqualTo(2));
            Assert.That(snapshot.Origin, Is.EqualTo(ScenarioPackageSnapshotOrigin.LegacyBackfill));
        });
    }

    [Test]
    public async Task LegacyScenario_ConcurrentDifferentBackfillsRejectConflict()
    {
        TestPackage initial = CreateCompletePackage();
        TestPackage changed = CreateCompletePackage(includeSecondCluster: true);
        changed.Package.Field["futureValue"] = "different";
        AnalysisPackage changedPackage = Rehash(changed.Package);
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = CreateStoredScenario(initial.FieldId);
        await store.CreateLegacyScenarioAsync(
            scenario,
            EvidenceCatalog.Enumerate(initial.Package)
                .Select(item => new EvidenceVisibility(
                    scenario.ScenarioId,
                    item.EvidenceId,
                    item.RecordKind,
                    T0,
                    null,
                    null))
                .ToArray());
        await SealLegacyScenarioAsync(store, scenario, initial.Package.Sha256);
        var packages = new BarrierPackageService(initial.Package, changedPackage);
        ScenarioService firstService = CreateService(store, packages);
        ScenarioService secondService = CreateService(store, packages);

        object[] outcomes = await Task.WhenAll(
            Observe(firstService.GetPackageAsync(initial.FieldId, scenario.ScenarioId, T0)),
            Observe(secondService.GetPackageAsync(initial.FieldId, scenario.ScenarioId, T0)));

        Assert.Multiple(() =>
        {
            Assert.That(outcomes.OfType<AnalysisPackage>(), Has.Exactly(1).Items);
            Assert.That(outcomes.OfType<ScenarioApiException>(), Has.Exactly(1).Items);
            Assert.That(
                outcomes.OfType<ScenarioApiException>().Single().Title,
                Is.EqualTo("Legacy scenario snapshot unavailable"));
        });
    }

    [Test]
    public async Task Snapshot_TriggersRejectMutationAndReadDetectsTampering()
    {
        TestPackage initial = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = await CreateService(store, initial.Package)
            .CreateAsync(StandardRequest(initial.FieldId));

        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var update = connection.CreateCommand();
            update.CommandText = """
                UPDATE scenario_package_snapshots
                SET canonical_package_sha256 = $hash
                WHERE scenario_id = $scenario_id;
                """;
            update.Parameters.AddWithValue("$hash", new string('0', 64));
            update.Parameters.AddWithValue("$scenario_id", scenario.ScenarioId.ToString("D"));
            Assert.That(
                async () => await update.ExecuteNonQueryAsync(),
                Throws.TypeOf<SqliteException>());

            await using var delete = connection.CreateCommand();
            delete.CommandText = """
                DELETE FROM scenario_package_snapshots
                WHERE scenario_id = $scenario_id;
                """;
            delete.Parameters.AddWithValue("$scenario_id", scenario.ScenarioId.ToString("D"));
            Assert.That(
                async () => await delete.ExecuteNonQueryAsync(),
                Throws.TypeOf<SqliteException>());

            await using var tamper = connection.CreateCommand();
            tamper.CommandText = """
                DROP TRIGGER tr_scenario_package_snapshots_no_update;
                UPDATE scenario_package_snapshots
                SET canonical_package_sha256 = $hash
                WHERE scenario_id = $scenario_id;
                """;
            tamper.Parameters.AddWithValue("$hash", new string('0', 64));
            tamper.Parameters.AddWithValue("$scenario_id", scenario.ScenarioId.ToString("D"));
            await tamper.ExecuteNonQueryAsync();
        }

        var restarted = new SqliteScenarioStore(_connectionString);
        Assert.That(
            async () => await restarted.FindScenarioPackageSnapshotAsync(scenario),
            Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public async Task Snapshot_ReadDetectsSemanticIdentityTampering()
    {
        TestPackage initial = CreateCompletePackage();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = await CreateService(store, initial.Package)
            .CreateAsync(StandardRequest(initial.FieldId));
        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var tamper = connection.CreateCommand();
            tamper.CommandText = """
                DROP TRIGGER tr_scenario_package_snapshots_no_update;
                UPDATE scenario_package_snapshots
                SET field_id = $field_id
                WHERE scenario_id = $scenario_id;
                """;
            tamper.Parameters.AddWithValue("$field_id", Guid.NewGuid().ToString("D"));
            tamper.Parameters.AddWithValue("$scenario_id", scenario.ScenarioId.ToString("D"));
            await tamper.ExecuteNonQueryAsync();
        }

        var restarted = new SqliteScenarioStore(_connectionString);
        Assert.That(
            async () => await restarted.FindScenarioPackageSnapshotAsync(scenario),
            Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public async Task ScenarioCreation_InvalidSnapshotRollsBackAllRows()
    {
        TestPackage initial = CreateCompletePackage();
        AnalysisPackage invalid = initial.Package with { Sha256 = new string('0', 64) };
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService service = CreateService(store, invalid);

        Assert.That(
            async () => await service.CreateAsync(StandardRequest(initial.FieldId)),
            Throws.TypeOf<InvalidDataException>());
        IReadOnlyList<Scenario> scenarios = await store.ListAsync();
        long snapshotCount;
        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM scenario_package_snapshots;";
            snapshotCount = (long)(await command.ExecuteScalarAsync() ?? -1L);
        }

        Assert.Multiple(() =>
        {
            Assert.That(scenarios, Is.Empty);
            Assert.That(snapshotCount, Is.Zero);
        });
    }

    [Test]
    public async Task ScenarioCreation_TransactionFailureRollsBackScenarioVisibilityAndSnapshot()
    {
        TestPackage initial = CreateCompletePackage();
        var faultStore = new SqliteScenarioStore(
            _connectionString,
            () => throw new InvalidOperationException("Injected scenario creation failure."));
        ScenarioService service = CreateService(faultStore, initial.Package);

        Assert.That(
            async () => await service.CreateAsync(StandardRequest(initial.FieldId)),
            Throws.TypeOf<InvalidOperationException>());
        var restarted = new SqliteScenarioStore(_connectionString);
        IReadOnlyList<Scenario> scenarios = await restarted.ListAsync();
        long snapshotCount;
        long visibilityCount;
        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT
                    (SELECT COUNT(*) FROM scenario_package_snapshots),
                    (SELECT COUNT(*) FROM evidence_visibility);
                """;
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            Assert.That(await reader.ReadAsync(), Is.True);
            snapshotCount = reader.GetInt64(0);
            visibilityCount = reader.GetInt64(1);
        }

        Assert.Multiple(() =>
        {
            Assert.That(scenarios, Is.Empty);
            Assert.That(snapshotCount, Is.Zero);
            Assert.That(visibilityCount, Is.Zero);
        });
    }

    private static ScenarioService CreateService(SqliteScenarioStore store, AnalysisPackage package) =>
        new(store, new FakeFieldPackageService(package), new CanonicalJsonHasher(), new FixedTimeProvider(T0.AddDays(1)));

    private static ScenarioService CreateService(SqliteScenarioStore store, IFieldPackageService packages) =>
        new(store, packages, new CanonicalJsonHasher(), new FixedTimeProvider(T0.AddDays(1)));

    private static CreateScenarioRequest StandardRequest(Guid fieldId) =>
        new(fieldId, "Target", T0, "public-seed-label", new string('b', 64));

    private static async Task SealLegacyScenarioAsync(
        SqliteScenarioStore store,
        Scenario scenario,
        string packageSha256)
    {
        var body = new PredictionBody(
            "candidate",
            [],
            [],
            new QuantileValues(1, 2, 3),
            [],
            [],
            [],
            [],
            [],
            packageSha256,
            "legacy migration trust anchor");
        string bodyJson = PredictionJson.Canonicalize(body);
        await store.SavePredictionDraftAsync(scenario.ScenarioId, bodyJson, null, T0);
        BaselineSnapshot[] baselines = Enum.GetValues<BaselineKind>()
            .Select(kind => BaselineIntegrity.Finalize(new BaselineSnapshot(
                Guid.Empty,
                string.Empty,
                scenario.ScenarioId,
                kind,
                "legacy-baseline-v1",
                packageSha256,
                "candidate",
                0,
                0,
                [],
                null,
                null,
                null,
                null,
                null,
                null,
                "legacy migration baseline")))
            .ToArray();
        await store.SealPredictionAsync(
            scenario.ScenarioId,
            bodyJson,
            PredictionJson.ComputeSha256(body),
            baselines,
            T0);
    }

    private async Task SetScenarioStatusAsync(Guid scenarioId, ScenarioStatus status)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE scenarios
            SET status = $status
            WHERE scenario_id = $scenario_id;
            """;
        command.Parameters.AddWithValue("$status", status.ToString());
        command.Parameters.AddWithValue("$scenario_id", scenarioId.ToString("D"));
        await command.ExecuteNonQueryAsync();
    }

    private static Scenario CreateStoredScenario(Guid fieldId, DateTimeOffset? currentAsOfUtc = null) => new(
        Guid.NewGuid(),
        fieldId,
        null,
        "Target",
        T0,
        currentAsOfUtc ?? T0,
        "public-seed-label",
        ScenarioModelVersions.World,
        ScenarioModelVersions.Observation,
        ScenarioModelVersions.Scoring,
        ScenarioStatus.Draft,
        new string('c', 64),
        T0,
        T0);

    private async Task CreateLegacyScenarioDatabaseAsync(Guid scenarioId, Guid fieldId, string asOfUtc)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE scenarios (
                scenario_id TEXT PRIMARY KEY,
                source_field_id TEXT NOT NULL,
                cloned_field_id TEXT NULL,
                reservoir_name TEXT NOT NULL,
                as_of_utc TEXT NOT NULL,
                seed_label TEXT NOT NULL,
                world_model_version TEXT NOT NULL,
                observation_model_version TEXT NOT NULL,
                scoring_model_version TEXT NOT NULL,
                status TEXT NOT NULL,
                assumptions_sha256 TEXT NOT NULL,
                created_utc TEXT NOT NULL,
                modified_utc TEXT NOT NULL
            );
            INSERT INTO scenarios (
                scenario_id, source_field_id, cloned_field_id, reservoir_name, as_of_utc,
                seed_label, world_model_version, observation_model_version,
                scoring_model_version, status, assumptions_sha256, created_utc, modified_utc)
            VALUES (
                $scenario_id, $source_field_id, NULL, $reservoir_name, $as_of_utc,
                $seed_label, $world_model_version, $observation_model_version,
                $scoring_model_version, $status, $assumptions_sha256, $created_utc, $modified_utc);
            """;
        command.Parameters.AddWithValue("$scenario_id", scenarioId.ToString("D"));
        command.Parameters.AddWithValue("$source_field_id", fieldId.ToString("D"));
        command.Parameters.AddWithValue("$reservoir_name", "Target");
        command.Parameters.AddWithValue("$as_of_utc", asOfUtc);
        command.Parameters.AddWithValue("$seed_label", "legacy-label");
        command.Parameters.AddWithValue("$world_model_version", ScenarioModelVersions.World);
        command.Parameters.AddWithValue("$observation_model_version", ScenarioModelVersions.Observation);
        command.Parameters.AddWithValue("$scoring_model_version", ScenarioModelVersions.Scoring);
        command.Parameters.AddWithValue("$status", ScenarioStatus.Draft.ToString());
        command.Parameters.AddWithValue("$assumptions_sha256", new string('d', 64));
        command.Parameters.AddWithValue("$created_utc", T0.ToString("O"));
        command.Parameters.AddWithValue("$modified_utc", T0.ToString("O"));
        await command.ExecuteNonQueryAsync();
    }

    private static EvidenceVisibility Visibility(
        Scenario scenario,
        string kind,
        Guid id,
        DateTimeOffset visibleFrom,
        DateTimeOffset? visibleUntil = null) =>
        new(scenario.ScenarioId, $"{kind}:{id:D}", kind, visibleFrom, visibleUntil, null);

    private static TestPackage CreateCompletePackage(
        bool duplicateCluster = false,
        bool includeSecondCluster = false)
    {
        Guid fieldId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        Guid clusterId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        Guid secondClusterId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        Guid wellId = Guid.Parse("30000000-0000-0000-0000-000000000003");
        Guid wellBoreId = Guid.Parse("40000000-0000-0000-0000-000000000004");
        Guid architectureId = Guid.Parse("50000000-0000-0000-0000-000000000005");
        Guid trajectoryId = Guid.Parse("60000000-0000-0000-0000-000000000006");
        Guid geologyId = Guid.Parse("70000000-0000-0000-0000-000000000007");

        JsonNode cluster = Entity(clusterId);
        var clusters = new List<JsonNode> { cluster };
        if (duplicateCluster)
            clusters.Add(cluster.DeepClone());
        if (includeSecondCluster)
            clusters.Add(Entity(secondClusterId));
        JsonNode field = Entity(fieldId);
        JsonNode well = Entity(wellId, ("ClusterID", clusterId));
        JsonNode wellBore = Entity(wellBoreId, ("WellID", wellId));
        JsonNode architecture = Entity(architectureId, ("WellBoreID", wellBoreId));
        JsonNode trajectory = Entity(trajectoryId, ("WellBoreID", wellBoreId));
        JsonNode geology = Entity(geologyId, ("WellBoreID", wellBoreId));
        var counts = new SourceCounts(1, clusters.Count, 1, 1, 1, 1, 1);
        var hasher = new CanonicalJsonHasher();
        string hash = hasher.Compute(
            fieldId, field, clusters, [well], [wellBore], [architecture], [trajectory], [geology], counts, []);
        var package = new AnalysisPackage(
            T0,
            fieldId,
            field,
            clusters,
            [well],
            [wellBore],
            [architecture],
            [trajectory],
            [geology],
            counts,
            [],
            hash);
        return new TestPackage(
            package,
            fieldId,
            clusterId,
            includeSecondCluster ? secondClusterId : null,
            wellId);
    }

    private static JsonObject Entity(Guid id, params (string Name, Guid Value)[] references)
    {
        var entity = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = id }
        };
        foreach ((string name, Guid value) in references)
            entity[name] = value;
        return entity;
    }

    private static Guid MetaId(JsonNode item) =>
        item["MetaInfo"]!["ID"]!.GetValue<Guid>();

    private static AnalysisPackage Rehash(AnalysisPackage package) =>
        package with
        {
            Sha256 = new CanonicalJsonHasher().Compute(
                package.FieldId,
                package.Field,
                package.Clusters,
                package.Wells,
                package.WellBores,
                package.WellBoreArchitectures,
                package.Trajectories,
                package.GeologicalProperties,
                package.SourceCounts,
                package.DataGaps)
        };

    private static async Task<object> Observe(Task<AnalysisPackage> task)
    {
        try
        {
            return await task;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private sealed record TestPackage(
        AnalysisPackage Package,
        Guid FieldId,
        Guid ClusterId,
        Guid? SecondClusterId,
        Guid WellId);

    private sealed class FakeFieldPackageService(AnalysisPackage package) : IFieldPackageService
    {
        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<JsonNode>(new JsonArray());

        public Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken) =>
            Task.FromResult(package);
    }

    private sealed class MutableFieldPackageService(AnalysisPackage package) : IFieldPackageService
    {
        internal AnalysisPackage Current { get; set; } = package;
        internal int BuildCount { get; private set; }

        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<JsonNode>(new JsonArray());

        public Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken)
        {
            BuildCount++;
            return Task.FromResult(Current);
        }
    }

    private sealed class BarrierPackageService(params AnalysisPackage[] packages) : IFieldPackageService
    {
        private readonly TaskCompletionSource _bothCalled =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _buildCount;

        internal int BuildCount => _buildCount;

        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<JsonNode>(new JsonArray());

        public async Task<AnalysisPackage> BuildPackageAsync(
            Guid fieldId,
            CancellationToken cancellationToken)
        {
            int index = Interlocked.Increment(ref _buildCount) - 1;
            if (index == 1)
                _bothCalled.SetResult();
            await _bothCalled.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            return packages[index];
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
