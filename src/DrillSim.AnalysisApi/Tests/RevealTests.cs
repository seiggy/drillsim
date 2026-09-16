using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Primitives;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class RevealTests
{
    private static readonly DateTimeOffset T0 = new(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T1 = T0.AddDays(30);
    private string _databasePath = null!;
    private string _connectionString = null!;

    [SetUp]
    public void SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-reveal-{Guid.NewGuid():N}.db");
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
    public void InternalCallbackKey_UsesOneRequiredHeaderWithoutLeakingFailureReason()
    {
        const string key = "analysis-callback-secret";
        var validator = new InternalCallbackKeyValidator(key);
        var missing = new HeaderDictionary();
        var wrong = new HeaderDictionary { [InternalCallbackKeyValidator.HeaderName] = "wrong" };
        var multiple = new HeaderDictionary
        {
            [InternalCallbackKeyValidator.HeaderName] = new StringValues([key, key])
        };
        var correct = new HeaderDictionary { [InternalCallbackKeyValidator.HeaderName] = key };

        Assert.Multiple(() =>
        {
            Assert.That(validator.IsAuthorized(missing), Is.False);
            Assert.That(validator.IsAuthorized(wrong), Is.False);
            Assert.That(validator.IsAuthorized(multiple), Is.False);
            Assert.That(validator.IsAuthorized(correct), Is.True);
        });
    }

    [Test]
    public void Validation_RejectsMalformedIdsDuplicatesHashesKindsAndProductionShape()
    {
        Guid scenarioId = Guid.NewGuid();
        Guid cloneId = Guid.NewGuid();
        RevealRequest valid = CreateRequest(cloneId);
        RevealEvidenceRequest field = valid.Evidence[0];

        Assert.Multiple(() =>
        {
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with { RevealId = valid.RevealId.ToUpperInvariant() }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with { ManifestSha256 = new string('A', 64) }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with { Evidence = [field with { ContentSha256 = new string('D', 64) }] }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with
                {
                    ProductionSeries = valid.ProductionSeries with { ContentSha256 = new string('C', 64) }
                }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with { Evidence = [field, field] }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with { Evidence = Enumerable.Repeat(field, 5_001).ToArray() }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with
                {
                    Evidence = [field with { RecordKind = "World", EvidenceId = Guid.NewGuid().ToString("D") }]
                }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with { ProductionSeries = valid.ProductionSeries with { MonthCount = 59 } }));
            Assert.Throws<ScenarioApiException>(() => RevealService.Validate(
                scenarioId,
                valid with
                {
                    ProductionSeries = valid.ProductionSeries with { CheckpointYears = [1, 5, 3] }
                }));
        });
    }

    [Test]
    public void Validation_NormalizesDrillingOperationsEvidenceKindsToPackageIds()
    {
        Guid scenarioId = Guid.NewGuid();
        Guid cloneId = Guid.NewGuid();
        Guid architectureId = Guid.NewGuid();
        Guid geologyId = Guid.NewGuid();
        RevealRequest request = CreateRequest(
            cloneId,
            [
                Evidence(cloneId, EvidenceCatalog.Field),
                Evidence(architectureId, EvidenceCatalog.Architecture),
                Evidence(geologyId, EvidenceCatalog.Geology)
            ]);

        ValidatedReveal validated = RevealService.Validate(scenarioId, request);

        Assert.That(validated.Evidence.Select(item => (item.RecordKind, item.EvidenceId)), Is.EqualTo(new[]
        {
            (EvidenceCatalog.Field, $"field:{cloneId:D}"),
            (EvidenceCatalog.Architecture, $"architecture:{architectureId:D}"),
            (EvidenceCatalog.Geology, $"geology:{geologyId:D}")
        }));
    }

    [Test]
    public async Task Prepare_AcceptsUniqueCommittedSixtyMonthProductionRepresentation()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);

        RevealReceipt receipt = await fixture.Reveals.PrepareAsync(
            fixture.Scenario.ScenarioId,
            request);

        Assert.Multiple(() =>
        {
            Assert.That(receipt.Status, Is.EqualTo(RevealReceiptStatus.Prepared));
            Assert.That(request.ClonePackage!.Wells, Has.Count.EqualTo(1));
            Assert.That(
                request.ClonePackage.Wells[0]["Dataset"]!["MonthlyProduction"]!.AsArray(),
                Has.Count.EqualTo(60));
        });
    }

    [Test]
    public async Task Prepare_IgnoresExplicitNullProductionOnNonProducingWells()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest valid = CreateRequest(fixture.CloneId);
        AnalysisPackage seed = valid.ClonePackage!;
        Guid idleWellId = Guid.NewGuid();
        Guid clusterId = JsonAccess.MetaId(seed.Clusters.Single())!.Value;
        var idleWell = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = idleWellId },
            ["ClusterID"] = clusterId,
            ["Dataset"] = new JsonObject { ["MonthlyProduction"] = null }
        };
        AnalysisPackage package = Rehash(seed with
        {
            Wells = [.. seed.Wells, idleWell],
            SourceCounts = seed.SourceCounts with { Wells = 2 }
        });
        RevealRequest request = valid with
        {
            ClonePackage = package,
            Evidence =
            [
                .. valid.Evidence,
                new RevealEvidenceRequest(
                    idleWellId.ToString("D"),
                    "Well",
                    RevealPackageValidator.BusinessContentHash(idleWell))
            ]
        };

        RevealReceipt receipt = await fixture.Reveals.PrepareAsync(
            fixture.Scenario.ScenarioId,
            request);

        Assert.That(receipt.Status, Is.EqualTo(RevealReceiptStatus.Prepared));
    }

    [Test]
    public async Task Prepare_AcceptsPackageHashedByDrillingOperations()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        AnalysisPackage seed = RevealPackageTestData.Create(fixture.CloneId, T1);
        JsonElement field = JsonSerializer.SerializeToElement(seed.Field);
        JsonElement[] clusters = seed.Clusters.Select(item => JsonSerializer.SerializeToElement(item)).ToArray();
        JsonElement[] wells = seed.Wells.Select(item => JsonSerializer.SerializeToElement(item)).ToArray();
        var counts = new DrillingOperations.AnalysisPackageCounts(1, 1, 1, 0, 0, 0, 0);
        string hash = DrillingOperations.PublicationJson.ComputeAnalysisPackageHash(
            seed.FieldId,
            field,
            clusters,
            wells,
            [],
            [],
            [],
            [],
            counts,
            seed.DataGaps);
        var drillingPackage = new DrillingOperations.AnalysisPackageDocument(
            seed.GeneratedAt,
            seed.FieldId,
            field,
            clusters,
            wells,
            [],
            [],
            [],
            [],
            counts,
            seed.DataGaps,
            hash);
        AnalysisPackage package = JsonSerializer.Deserialize<AnalysisPackage>(
            JsonSerializer.Serialize(drillingPackage, DrillingOperations.CanonicalJson.SerializerOptions),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        RevealRequest template = CreateRequest(fixture.CloneId);
        RevealRequest request = template with
        {
            ClonePackage = package,
            Evidence = RevealPackageTestData.Evidence(package),
            ProductionSeries = template.ProductionSeries with
            {
                ContentSha256 = RevealPackageTestData.ProductionHash(package)
            }
        };

        RevealReceipt receipt = await fixture.Reveals.PrepareAsync(
            fixture.Scenario.ScenarioId,
            request);

        Assert.Multiple(() =>
        {
            Assert.That(receipt.Status, Is.EqualTo(RevealReceiptStatus.Prepared));
            Assert.That(package.Sha256, Is.EqualTo(seed.Sha256));
        });
    }

    [TestCase("month")]
    [TestCase("value")]
    [TestCase("unit")]
    [TestCase("order")]
    public async Task Prepare_RejectsAlteredProductionRepresentation(string mutation)
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest valid = CreateRequest(fixture.CloneId);
        AnalysisPackage package = ClonePackage(valid.ClonePackage!);
        JsonArray months = package.Wells.Single()["Dataset"]!["MonthlyProduction"]!.AsArray();
        switch (mutation)
        {
            case "month":
                months[0]!["Month"] = 12;
                break;
            case "value":
                months[0]!["Oil"]!["Value"] = 999_999d;
                break;
            case "unit":
                months[0]!["Oil"]!["Unit"] = "bbl";
                break;
            case "order":
                JsonNode first = months[0]!.DeepClone();
                JsonNode second = months[1]!.DeepClone();
                months[0] = second;
                months[1] = first;
                break;
        }
        package = Rehash(package);
        RevealRequest tampered = WithPackageAndMatchingEvidence(valid, package);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, tampered))!;

        Assert.That(exception.Message, Does.Contain("productionSeries.contentSha256"));
    }

    [Test]
    public async Task Prepare_RejectsMissingMultipleOrMismatchedProductionRepresentation()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest valid = CreateRequest(fixture.CloneId);

        AnalysisPackage missing = ClonePackage(valid.ClonePackage!);
        missing.Wells.Single()["Dataset"]!.AsObject().Remove("MonthlyProduction");
        missing = Rehash(missing);
        ScenarioApiException missingError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                WithPackageAndMatchingEvidence(valid, missing)))!;

        AnalysisPackage multiple = ClonePackage(valid.ClonePackage!);
        JsonNode secondWell = multiple.Wells.Single().DeepClone();
        secondWell["MetaInfo"]!["ID"] = Guid.NewGuid();
        JsonNode[] wells = multiple.Wells.Append(secondWell).ToArray();
        multiple = Rehash(multiple with
        {
            Wells = wells,
            SourceCounts = multiple.SourceCounts with { Wells = wells.Length }
        });
        IReadOnlyList<RevealEvidenceRequest> multipleEvidence =
        [
            .. RevealPackageTestData.Evidence(multiple with { Wells = [wells[0]] }),
            new(
                JsonAccess.MetaId(secondWell)!.Value.ToString("D"),
                "Well",
                RevealPackageValidator.BusinessContentHash(secondWell))
        ];
        ScenarioApiException multipleError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                valid with { ClonePackage = multiple, Evidence = multipleEvidence }))!;

        ScenarioApiException hashError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                valid with
                {
                    ProductionSeries = valid.ProductionSeries with
                    {
                        ContentSha256 = new string('f', 64)
                    }
                }))!;

        Assert.Multiple(() =>
        {
            Assert.That(missingError.Message, Does.Contain("exactly one monthly production representation"));
            Assert.That(multipleError.Message, Does.Contain("exactly one monthly production representation"));
            Assert.That(hashError.Message, Does.Contain("productionSeries.contentSha256"));
        });
    }

    [Test]
    public async Task Prepare_RejectsInvalidStatusTimeModelAndClone()
    {
        RevealFixture draft = await CreateFixtureAsync(ScenarioStatus.Draft);
        ScenarioApiException status = Assert.ThrowsAsync<ScenarioApiException>(() =>
            draft.Reveals.PrepareAsync(draft.Scenario.ScenarioId, CreateRequest(draft.CloneId)))!;

        RevealFixture approved = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(approved.CloneId);
        ScenarioApiException time = Assert.ThrowsAsync<ScenarioApiException>(() =>
            approved.Reveals.PrepareAsync(
                approved.Scenario.ScenarioId,
                request with { ValidTimeUtc = T0 }))!;
        ScenarioApiException model = Assert.ThrowsAsync<ScenarioApiException>(() =>
            approved.Reveals.PrepareAsync(
                approved.Scenario.ScenarioId,
                request with { ObservationModelVersion = "observation-model-v999" }))!;
        ScenarioApiException clone = Assert.ThrowsAsync<ScenarioApiException>(() =>
            approved.Reveals.PrepareAsync(
                approved.Scenario.ScenarioId,
                CreateRequest(approved.SourceId)))!;

        Assert.Multiple(() =>
        {
            Assert.That(status.StatusCode, Is.EqualTo(409));
            Assert.That(time.StatusCode, Is.EqualTo(400));
            Assert.That(model.StatusCode, Is.EqualTo(409));
            Assert.That(clone.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task Prepare_RollsBackManifestAndProductionWhenCommitFails()
    {
        Guid sourceId = Guid.NewGuid();
        Guid cloneId = Guid.NewGuid();
        var store = new SqliteScenarioStore(
            _connectionString,
            () => throw new InjectedRevealFailureException(),
            null);
        Scenario scenario = await SeedScenarioAsync(store, sourceId, ScenarioStatus.HumanApproved);
        var service = new RevealService(store, new FixedTimeProvider(T1.AddMinutes(1)));
        RevealRequest request = CreateRequest(cloneId);

        Assert.ThrowsAsync<InjectedRevealFailureException>(() =>
            service.PrepareAsync(scenario.ScenarioId, request));

        Scenario? unchanged = await store.FindAsync(scenario.ScenarioId);
        IReadOnlyList<EvidenceVisibility> visibility = await store.GetEvidenceVisibilityAsync(scenario.ScenarioId);
        RevealReceipt? internalStatus = await store.FindRevealStatusAsync(
            scenario.ScenarioId,
            Guid.Parse(request.RevealId));
        PublicProductionSeriesMetadata? production = await store.FindProductionSeriesAsync(scenario.ScenarioId);
        long manifestRows = await CountRowsAsync("reveal_manifests");
        long productionRows = await CountRowsAsync("public_production_series");
        long cloneSnapshotRows = await CountRowsAsync("scenario_clone_package_snapshots");
        Assert.Multiple(() =>
        {
            Assert.That(unchanged!.Status, Is.EqualTo(ScenarioStatus.HumanApproved));
            Assert.That(unchanged.ClonedFieldId, Is.Null);
            Assert.That(unchanged.AsOfUtc, Is.EqualTo(T0));
            Assert.That(visibility, Has.Count.EqualTo(1));
            Assert.That(internalStatus, Is.Null);
            Assert.That(production, Is.Null);
            Assert.That(manifestRows, Is.Zero);
            Assert.That(productionRows, Is.Zero);
            Assert.That(cloneSnapshotRows, Is.Zero);
        });
    }

    [Test]
    public async Task Finalize_RollsBackVisibilityClockAndStatusWhenCommitFails()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        RevealReceipt prepared = await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        var faultStore = new SqliteScenarioStore(
            _connectionString,
            null,
            () => throw new InjectedRevealFailureException());
        var faultService = new RevealService(faultStore, new FixedTimeProvider(T1.AddMinutes(2)));

        Assert.ThrowsAsync<InjectedRevealFailureException>(() =>
            faultService.FinalizeAsync(fixture.Scenario.ScenarioId, Finalize(request)));

        Scenario? unchanged = await faultStore.FindAsync(fixture.Scenario.ScenarioId);
        IReadOnlyList<EvidenceVisibility> visibility =
            await faultStore.GetEvidenceVisibilityAsync(fixture.Scenario.ScenarioId);
        RevealReceipt internalStatus = await faultService.GetInternalStatusAsync(
            fixture.Scenario.ScenarioId,
            prepared.RevealId);
        Assert.Multiple(() =>
        {
            Assert.That(unchanged!.Status, Is.EqualTo(ScenarioStatus.HumanApproved));
            Assert.That(unchanged.ClonedFieldId, Is.Null);
            Assert.That(unchanged.AsOfUtc, Is.EqualTo(T0));
            Assert.That(visibility, Has.Count.EqualTo(1));
            Assert.That(internalStatus.Status, Is.EqualTo(RevealReceiptStatus.Prepared));
            Assert.ThrowsAsync<ScenarioApiException>(() =>
                faultService.GetRevealAsync(fixture.Scenario.ScenarioId));
        });
    }

    [Test]
    public async Task Prepare_IsConcurrentIdempotentAndRejectsConflictingIdentity()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<RevealReceipt>[] attempts = Enumerable.Range(0, 2).Select(async _ =>
        {
            await start.Task;
            return await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        }).ToArray();
        start.SetResult();

        RevealReceipt[] receipts = await Task.WhenAll(attempts);
        RevealReceipt retry = await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        ScenarioApiException conflictingBody = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                request with { ManifestSha256 = new string('e', 64) }))!;
        ScenarioApiException conflictingReveal = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                request with { RevealId = Guid.NewGuid().ToString("D") }))!;
        Scenario secondScenario = await SeedScenarioAsync(fixture.Store, Guid.NewGuid(), ScenarioStatus.HumanApproved);
        ScenarioApiException conflictingScenario = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(secondScenario.ScenarioId, request))!;

        Assert.Multiple(() =>
        {
            Assert.That(receipts[1], Is.EqualTo(receipts[0]));
            Assert.That(retry, Is.EqualTo(receipts[0]));
            Assert.That(receipts[0].Status, Is.EqualTo(RevealReceiptStatus.Prepared));
            Assert.That(conflictingBody.StatusCode, Is.EqualTo(409));
            Assert.That(conflictingReveal.StatusCode, Is.EqualTo(409));
            Assert.That(conflictingScenario.StatusCode, Is.EqualTo(409));
            Assert.That(JsonSerializer.Serialize(receipts[0]), Does.Not.Contain(request.RunId));
        });
    }

    [Test]
    public async Task ConcurrentConflictingPrepares_CommitOneAndRejectOne()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest firstRequest = CreateRequest(fixture.CloneId);
        RevealRequest secondRequest = CreateRequest(fixture.CloneId);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<object> PrepareAsync(RevealRequest request)
        {
            await start.Task;
            try
            {
                return await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
            }
            catch (ScenarioApiException exception)
            {
                return exception;
            }
        }

        Task<object> first = PrepareAsync(firstRequest);
        Task<object> second = PrepareAsync(secondRequest);
        start.SetResult();
        object[] results = await Task.WhenAll(first, second);

        Assert.Multiple(() =>
        {
            Assert.That(results.OfType<RevealReceipt>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>().Single().StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task PreparedState_IsPrivateAndSurvivesRestartForFinalize()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        RevealReceipt prepared = await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        Scenario afterPrepare = (await fixture.Store.FindAsync(fixture.Scenario.ScenarioId))!;
        IReadOnlyList<EvidenceVisibility> preparedVisibility =
            await fixture.Store.GetEvidenceVisibilityAsync(fixture.Scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(prepared.Status, Is.EqualTo(RevealReceiptStatus.Prepared));
            Assert.That(SerializeApi(prepared), Does.Contain("\"status\":\"Prepared\""));
            Assert.That(afterPrepare.Status, Is.EqualTo(ScenarioStatus.HumanApproved));
            Assert.That(afterPrepare.ClonedFieldId, Is.Null);
            Assert.That(afterPrepare.AsOfUtc, Is.EqualTo(T0));
            Assert.That(preparedVisibility, Has.Count.EqualTo(1));
            Assert.ThrowsAsync<ScenarioApiException>(() =>
                fixture.Reveals.GetRevealAsync(fixture.Scenario.ScenarioId));
            Assert.ThrowsAsync<ScenarioApiException>(() =>
                fixture.Reveals.GetProductionAsync(fixture.Scenario.ScenarioId));
        });

        var restartedStore = new SqliteScenarioStore(_connectionString);
        var restarted = new RevealService(restartedStore, new FixedTimeProvider(T1.AddDays(1)));
        RevealReceipt internalPrepared = await restarted.GetInternalStatusAsync(
            fixture.Scenario.ScenarioId,
            prepared.RevealId);
        RevealReceipt finalized = await restarted.FinalizeAsync(
            fixture.Scenario.ScenarioId,
            Finalize(request));
        RevealReceipt publicReceipt = await restarted.GetRevealAsync(fixture.Scenario.ScenarioId);
        PublicProductionSeriesMetadata production =
            await restarted.GetProductionAsync(fixture.Scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(internalPrepared.Status, Is.EqualTo(RevealReceiptStatus.Prepared));
            Assert.That(finalized.Status, Is.EqualTo(RevealReceiptStatus.Revealed));
            Assert.That(SerializeApi(finalized), Does.Contain("\"status\":\"Revealed\""));
            Assert.That(publicReceipt, Is.EqualTo(finalized));
            Assert.That(production.SeriesId, Is.EqualTo(finalized.ProductionSeriesId));
            Assert.That(production.MonthCount, Is.EqualTo(60));
            Assert.That(production.CheckpointYears, Is.EqualTo(new[] { 1, 3, 5 }));
            Assert.That(
                JsonSerializer.Serialize(production),
                Does.Not.Contain("MonthlyProduction")
                    .And.Not.Contain("\"months\":")
                    .And.Not.Contain("\"oil\"")
                    .And.Not.Contain("\"gas\"")
                    .And.Not.Contain("\"water\""));
        });
    }

    [Test]
    public async Task SchemaMigration_AcceptsExistingRevealedReceiptAndReplacesLegacyUpdateTrigger()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        RevealReceipt finalized = await fixture.Reveals.FinalizeAsync(
            fixture.Scenario.ScenarioId,
            Finalize(request));

        SqliteConnection.ClearAllPools();
        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                DROP TRIGGER IF EXISTS tr_reveal_manifests_no_update;
                CREATE TRIGGER tr_reveal_manifests_no_update
                BEFORE UPDATE ON reveal_manifests
                BEGIN SELECT RAISE(ABORT, 'Reveal manifest is immutable'); END;
                """;
            await command.ExecuteNonQueryAsync();
        }

        var restartedStore = new SqliteScenarioStore(_connectionString);
        await restartedStore.InitializeAsync();
        RevealReceipt? migrated = await restartedStore.FindRevealAsync(fixture.Scenario.ScenarioId);

        Assert.That(migrated, Is.EqualTo(finalized));
    }

    [Test]
    public async Task Finalize_IsConcurrentIdempotentAndRejectsMismatchOrMissingPrepare()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        FinalizeRevealRequest finalize = Finalize(request);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<RevealReceipt>[] attempts = Enumerable.Range(0, 2).Select(async _ =>
        {
            await start.Task;
            return await fixture.Reveals.FinalizeAsync(fixture.Scenario.ScenarioId, finalize);
        }).ToArray();
        start.SetResult();

        RevealReceipt[] receipts = await Task.WhenAll(attempts);
        RevealReceipt retry = await fixture.Reveals.FinalizeAsync(fixture.Scenario.ScenarioId, finalize);
        ScenarioApiException mismatch = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.FinalizeAsync(
                fixture.Scenario.ScenarioId,
                finalize with { ManifestSha256 = new string('f', 64) }))!;
        RevealFixture missingFixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        ScenarioApiException missing = Assert.ThrowsAsync<ScenarioApiException>(() =>
            missingFixture.Reveals.FinalizeAsync(
                missingFixture.Scenario.ScenarioId,
                new FinalizeRevealRequest(Guid.NewGuid().ToString("D"), new string('b', 64))))!;

        Assert.Multiple(() =>
        {
            Assert.That(receipts[1], Is.EqualTo(receipts[0]));
            Assert.That(retry, Is.EqualTo(receipts[0]));
            Assert.That(receipts[0].Status, Is.EqualTo(RevealReceiptStatus.Revealed));
            Assert.That(mismatch.StatusCode, Is.EqualTo(409));
            Assert.That(missing.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task ConcurrentConflictingFinalizes_CommitValidIdentityAndRejectConflict()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        FinalizeRevealRequest valid = Finalize(request);
        FinalizeRevealRequest conflicting = valid with { ManifestSha256 = new string('f', 64) };
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<object> FinalizeAsync(FinalizeRevealRequest candidate)
        {
            await start.Task;
            try
            {
                return await fixture.Reveals.FinalizeAsync(fixture.Scenario.ScenarioId, candidate);
            }
            catch (ScenarioApiException exception)
            {
                return exception;
            }
        }

        Task<object> first = FinalizeAsync(valid);
        Task<object> second = FinalizeAsync(conflicting);
        start.SetResult();
        object[] results = await Task.WhenAll(first, second);

        Assert.Multiple(() =>
        {
            Assert.That(results.OfType<RevealReceipt>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<RevealReceipt>().Single().Status, Is.EqualTo(RevealReceiptStatus.Revealed));
            Assert.That(results.OfType<ScenarioApiException>().Single().StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task Reveal_PreservesT0SourceBytesAndFiltersT1CloneWithHiddenIdsSuppressed()
    {
        Guid sourceId = Guid.NewGuid();
        Guid cloneId = Guid.NewGuid();
        Guid sourceWellId = Guid.NewGuid();
        Guid hiddenCloneWellId = Guid.NewGuid();
        AnalysisPackage sourcePackage = CreatePackage(sourceId, sourceWellId);
        AnalysisPackage snapshotClonePackage = RevealPackageTestData.Create(cloneId, T1);
        snapshotClonePackage.Field["observable"] = "committed";
        snapshotClonePackage = Rehash(snapshotClonePackage);
        AnalysisPackage liveClonePackage = CreatePackage(cloneId, hiddenCloneWellId);
        liveClonePackage.Field["observable"] = "future-live-mutation";
        liveClonePackage = Rehash(liveClonePackage);
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = await SeedScenarioAsync(
            store,
            sourceId,
            ScenarioStatus.HumanApproved,
            $"well:{sourceWellId:D}");
        var packages = new MapPackageService(sourcePackage, liveClonePackage);
        await store.BackfillScenarioPackageSnapshotAsync(scenario, sourcePackage, T0);
        var scenarios = new ScenarioService(store, packages, new CanonicalJsonHasher(), new FixedTimeProvider(T1));
        var reveals = new RevealService(store, new FixedTimeProvider(T1.AddMinutes(1)));

        AnalysisPackage before = await scenarios.GetPackageAsync(sourceId, scenario.ScenarioId, T0);
        string beforeJson = JsonSerializer.Serialize(before);
        RevealRequest request = CreateRequest(cloneId, clonePackage: snapshotClonePackage);
        await reveals.PrepareAsync(scenario.ScenarioId, request);
        AnalysisPackage preparedSource = await scenarios.GetPackageAsync(sourceId, scenario.ScenarioId, T0);
        await reveals.FinalizeAsync(scenario.ScenarioId, Finalize(request));
        Scenario revealedScenario = await scenarios.GetAsync(scenario.ScenarioId);
        AnalysisPackage after = await scenarios.GetPackageAsync(sourceId, scenario.ScenarioId, T0);
        AnalysisPackage current = await scenarios.GetPackageAsync(cloneId, scenario.ScenarioId, T1);
        EvidenceVisibilitySummary summary = await scenarios.GetVisibilitySummaryAsync(scenario.ScenarioId, T1);
        string summaryJson = JsonSerializer.Serialize(summary);

        Assert.Multiple(() =>
        {
            Assert.That(JsonSerializer.Serialize(preparedSource), Is.EqualTo(beforeJson));
            Assert.That(revealedScenario.Status, Is.EqualTo(ScenarioStatus.Revealed));
            Assert.That(revealedScenario.ClonedFieldId, Is.EqualTo(cloneId));
            Assert.That(revealedScenario.AsOfUtc, Is.EqualTo(T1));
            Assert.That(JsonSerializer.Serialize(after), Is.EqualTo(beforeJson));
            Assert.That(after.Sha256, Is.EqualTo(before.Sha256));
            Assert.That(current.FieldId, Is.EqualTo(cloneId));
            Assert.That(current.Field["observable"]!.GetValue<string>(), Is.EqualTo("committed"));
            Assert.That(current.Wells, Has.Count.EqualTo(1));
            Assert.That(current.Wells.Select(MetaId), Does.Not.Contain(hiddenCloneWellId));
            Assert.That(summaryJson, Does.Not.Contain(hiddenCloneWellId.ToString("D")));
            Assert.That(summary.Counts.Any(item =>
                item.RecordKind == EvidenceCatalog.Well &&
                item.Status == EvidenceVisibilityStatus.Hidden), Is.False);
        });
    }

    [Test]
    public async Task Prepare_RejectsCloneSnapshotExtrasHashMismatchAndForbiddenData()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest valid = CreateRequest(fixture.CloneId);

        AnalysisPackage extra = CreatePackage(fixture.CloneId, Guid.NewGuid()) with { GeneratedAt = T1 };
        ScenarioApiException extraError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                valid with { ClonePackage = extra }))!;

        ScenarioApiException hashError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                valid with
                {
                    Evidence =
                    [
                        valid.Evidence[0] with { ContentSha256 = new string('e', 64) }
                    ]
                }))!;
        ScenarioApiException omissionError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(
                fixture.Scenario.ScenarioId,
                valid with { Evidence = [] }))!;

        AnalysisPackage forbidden = valid.ClonePackage!;
        forbidden.Field["StageAPath"] = "hidden";
        forbidden = Rehash(forbidden);
        RevealRequest forbiddenRequest = valid with
        {
            ClonePackage = forbidden,
            Evidence =
            [
                valid.Evidence[0] with
                {
                    ContentSha256 = RevealPackageValidator.BusinessContentHash(forbidden.Field)
                }
            ]
        };
        ScenarioApiException forbiddenError = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, forbiddenRequest))!;

        Assert.Multiple(() =>
        {
            Assert.That(extraError.StatusCode, Is.EqualTo(400));
            Assert.That(hashError.StatusCode, Is.EqualTo(400));
            Assert.That(omissionError.StatusCode, Is.EqualTo(400));
            Assert.That(forbiddenError.StatusCode, Is.EqualTo(400));
        });
    }

    [Test]
    public async Task CloneSnapshot_IsImmutableAndLegacyRevealedScenarioCanBeBackfilled()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);
        await fixture.Reveals.FinalizeAsync(fixture.Scenario.ScenarioId, Finalize(request));

        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var mutation = connection.CreateCommand();
            mutation.CommandText = """
                UPDATE scenario_clone_package_snapshots
                SET canonical_package_sha256 = $hash
                WHERE scenario_id = $scenario_id;
                """;
            mutation.Parameters.AddWithValue("$hash", new string('0', 64));
            mutation.Parameters.AddWithValue("$scenario_id", fixture.Scenario.ScenarioId.ToString("D"));
            Assert.That(async () => await mutation.ExecuteNonQueryAsync(), Throws.TypeOf<SqliteException>());

            await using var remove = connection.CreateCommand();
            remove.CommandText = """
                DROP TRIGGER tr_scenario_clone_package_snapshots_no_delete;
                DELETE FROM scenario_clone_package_snapshots WHERE scenario_id = $scenario_id;
                """;
            remove.Parameters.AddWithValue("$scenario_id", fixture.Scenario.ScenarioId.ToString("D"));
            await remove.ExecuteNonQueryAsync();
        }

        var restartedStore = new SqliteScenarioStore(_connectionString);
        var restartedReveals = new RevealService(restartedStore, new FixedTimeProvider(T1.AddMinutes(2)));
        var scenarios = new ScenarioService(
            restartedStore,
            new MapPackageService(CreatePackage(fixture.SourceId), CreatePackage(fixture.CloneId)),
            new CanonicalJsonHasher(),
            new FixedTimeProvider(T1));
        Scenario revealed = await scenarios.GetAsync(fixture.Scenario.ScenarioId);
        ScenarioApiException unavailable = Assert.ThrowsAsync<ScenarioApiException>(() =>
            scenarios.GetPackageAsync(fixture.CloneId, revealed.ScenarioId, T1))!;

        RevealReceipt backfilled = await restartedReveals.BackfillCloneSnapshotAsync(
            fixture.Scenario.ScenarioId,
            request);
        AnalysisPackage restored = await scenarios.GetPackageAsync(
            fixture.CloneId,
            fixture.Scenario.ScenarioId,
            T1);
        RevealReceipt retry = await restartedReveals.BackfillCloneSnapshotAsync(
            fixture.Scenario.ScenarioId,
            request);

        Assert.Multiple(() =>
        {
            Assert.That(unavailable.StatusCode, Is.EqualTo(409));
            Assert.That(backfilled.Status, Is.EqualTo(RevealReceiptStatus.Revealed));
            Assert.That(retry, Is.EqualTo(backfilled));
            Assert.That(restored.Sha256, Is.EqualTo(request.ClonePackage!.Sha256));
        });
    }

    [Test]
    public async Task CloneSnapshot_TamperingIsDetectedOnRestart()
    {
        RevealFixture fixture = await CreateFixtureAsync(ScenarioStatus.HumanApproved);
        RevealRequest request = CreateRequest(fixture.CloneId);
        await fixture.Reveals.PrepareAsync(fixture.Scenario.ScenarioId, request);

        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var tamper = connection.CreateCommand();
            tamper.CommandText = """
                DROP TRIGGER tr_scenario_clone_package_snapshots_no_update;
                UPDATE scenario_clone_package_snapshots
                SET canonical_package_sha256 = $hash
                WHERE scenario_id = $scenario_id;
                """;
            tamper.Parameters.AddWithValue("$hash", new string('0', 64));
            tamper.Parameters.AddWithValue("$scenario_id", fixture.Scenario.ScenarioId.ToString("D"));
            await tamper.ExecuteNonQueryAsync();
        }

        var restarted = new SqliteScenarioStore(_connectionString);
        Assert.That(
            async () => await restarted.InitializeAsync(),
            Throws.TypeOf<InvalidDataException>());
    }

    private async Task<RevealFixture> CreateFixtureAsync(ScenarioStatus status)
    {
        Guid sourceId = Guid.NewGuid();
        Guid cloneId = Guid.NewGuid();
        var store = new SqliteScenarioStore(_connectionString);
        Scenario scenario = await SeedScenarioAsync(store, sourceId, status);
        return new RevealFixture(
            store,
            new RevealService(store, new FixedTimeProvider(T1.AddMinutes(1))),
            scenario,
            sourceId,
            cloneId);
    }

    private static async Task<Scenario> SeedScenarioAsync(
        SqliteScenarioStore store,
        Guid sourceId,
        ScenarioStatus status,
        params string[] additionalVisibleEvidence)
    {
        Guid scenarioId = Guid.NewGuid();
        var scenario = new Scenario(
            scenarioId,
            sourceId,
            null,
            "Target",
            T0,
            T0,
            "reveal-tests",
            ScenarioModelVersions.World,
            ScenarioModelVersions.Observation,
            ScenarioModelVersions.Scoring,
            status,
            new string('a', 64),
            T0,
            T0);
        var visibility = new List<EvidenceVisibility>
        {
            new(scenarioId, $"field:{sourceId:D}", EvidenceCatalog.Field, T0, null, null)
        };
        visibility.AddRange(additionalVisibleEvidence.Select(id => new EvidenceVisibility(
            scenarioId,
            id,
            id[..id.IndexOf(':')],
            T0,
            null,
            null)));
        return await store.CreateLegacyScenarioAsync(scenario, visibility);
    }

    private static RevealRequest CreateRequest(
        Guid cloneId,
        IReadOnlyList<RevealEvidenceRequest>? evidence = null,
        AnalysisPackage? clonePackage = null)
    {
        AnalysisPackage package = clonePackage ?? RevealPackageTestData.Create(cloneId, T1);
        IReadOnlyList<RevealEvidenceRequest> commitments = evidence ??
            RevealPackageTestData.Evidence(package);
        return new(
            Guid.NewGuid().ToString("D"),
            Guid.NewGuid().ToString("D"),
            cloneId.ToString("D"),
            T1,
            ScenarioModelVersions.Observation,
            new string('b', 64),
            commitments,
            new ProductionSeriesRequest(
                Guid.NewGuid().ToString("D"),
                "production-meter-v1",
                RevealPackageTestData.ProductionHash(package),
                60,
                [1, 3, 5]),
            package);
    }

    private async Task<long> CountRowsAsync(string table)
    {
        string safeTable = table switch
        {
            "reveal_manifests" => "reveal_manifests",
            "public_production_series" => "public_production_series",
            "scenario_clone_package_snapshots" => "scenario_clone_package_snapshots",
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {safeTable};";
        return (long)(await command.ExecuteScalarAsync() ?? 0L);
    }

    private static string SerializeApi<T>(T value)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Serialize(value, options);
    }

    private static FinalizeRevealRequest Finalize(RevealRequest request) =>
        new(request.RevealId, request.ManifestSha256);

    private static RevealEvidenceRequest Evidence(Guid id, string kind) =>
        new(id.ToString("D"), PublicationKind(kind), new string('d', 64));

    private static string PublicationKind(string kind) => kind switch
    {
        EvidenceCatalog.Field => "Field",
        EvidenceCatalog.Cluster => "Cluster",
        EvidenceCatalog.Well => "Well",
        EvidenceCatalog.WellBore => "WellBore",
        EvidenceCatalog.Architecture => "WellBoreArchitecture",
        EvidenceCatalog.Trajectory => "Trajectory",
        EvidenceCatalog.Geology => "GeologicalProperties",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static AnalysisPackage CreatePackage(Guid fieldId, params Guid[] wellIds)
    {
        JsonNode field = Entity(fieldId);
        JsonNode[] wells = wellIds.Select(id => (JsonNode)Entity(id)).ToArray();
        var counts = new SourceCounts(1, 0, wells.Length, 0, 0, 0, 0);
        var gaps = new List<string>
        {
            "No clusters were returned for the field.",
            "No trajectories were returned for the field."
        };
        if (wells.Length > 0)
            gaps.Add("No wellbores were returned for the field wells.");
        string[] orderedGaps = gaps.Order(StringComparer.Ordinal).ToArray();
        string hash = new CanonicalJsonHasher().Compute(
            fieldId, field, [], wells, [], [], [], [], counts, orderedGaps);
        return new AnalysisPackage(T0, fieldId, field, [], wells, [], [], [], [], counts, orderedGaps, hash);
    }

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

    private static AnalysisPackage ClonePackage(AnalysisPackage package) =>
        PredictionJson.Deserialize<AnalysisPackage>(
            PredictionJson.Canonicalize(package),
            "test clone package");

    private static RevealRequest WithPackageAndMatchingEvidence(
        RevealRequest request,
        AnalysisPackage package) =>
        request with
        {
            ClonePackage = package,
            Evidence = RevealPackageTestData.Evidence(package)
        };

    private static JsonObject Entity(Guid id) => new()
    {
        ["MetaInfo"] = new JsonObject { ["ID"] = id }
    };

    private static Guid MetaId(JsonNode item) => item["MetaInfo"]!["ID"]!.GetValue<Guid>();

    private sealed record RevealFixture(
        SqliteScenarioStore Store,
        RevealService Reveals,
        Scenario Scenario,
        Guid SourceId,
        Guid CloneId);

    private sealed class MapPackageService(params AnalysisPackage[] packages) : IFieldPackageService
    {
        private readonly IReadOnlyDictionary<Guid, AnalysisPackage> _packages =
            packages.ToDictionary(package => package.FieldId);

        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<JsonNode>(new JsonArray());

        public Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken) =>
            Task.FromResult(_packages[fieldId]);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InjectedRevealFailureException : Exception;
}
