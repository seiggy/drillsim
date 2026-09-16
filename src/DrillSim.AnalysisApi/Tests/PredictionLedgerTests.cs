using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class PredictionLedgerTests
{
    private static readonly DateTimeOffset T0 = new(2026, 2, 3, 4, 5, 6, TimeSpan.Zero);
    private string _databasePath = null!;
    private string _connectionString = null!;

    [SetUp]
    public void SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-prediction-{Guid.NewGuid():N}.db");
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
    public void Validation_RejectsInvalidQuantilesPathForecastsEvidenceAndHash()
    {
        PredictionBody valid = CreateBody(new string('a', 64), "field:10000000-0000-0000-0000-000000000001");

        Assert.Multiple(() =>
        {
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with { ExpectedPaydirtM = new QuantileValues(20, 10, 30) }));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with
                {
                    ProposedWellPath =
                    [
                        new(100, 90, 0, 0),
                        new(90, 100, 0, 0)
                    ]
                }));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with { ProductionForecasts = [new(1, 1, 1, 1), new(3, 1, 1, 1)] }));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with { CitedEvidenceIds = [valid.CitedEvidenceIds[0], valid.CitedEvidenceIds[0]] }));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with { CitedEvidenceIds = ["not-an-evidence-id"] }));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with { FieldPackageSha256 = new string('A', 64) }));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ComputeCanonicalSha256(
                valid with { FluidClasses = [(PredictedFluidClass)999] }));
        });
    }

    [Test]
    public async Task Draft_CanBeMutatedBeforeSealAndAdvancesStatus()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId);

        PredictionRecord first = await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);
        context.Time.Advance(TimeSpan.FromMinutes(1));
        PredictionRecord updated = await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            body with { Rationale = "Updated human-reviewable rationale." },
            first.Revision);
        Scenario scenario = await context.Scenarios.GetAsync(context.Scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(first.Body.Rationale, Is.Not.EqualTo(updated.Body.Rationale));
            Assert.That(first.Revision, Is.EqualTo(1));
            Assert.That(updated.Revision, Is.EqualTo(2));
            Assert.That(updated.ModifiedUtc, Is.GreaterThan(first.ModifiedUtc));
            Assert.That(scenario.Status, Is.EqualTo(ScenarioStatus.PredictionDrafted));
        });
    }

    [Test]
    public void CanonicalHash_IsStableForEquivalentPredictionContent()
    {
        PredictionBody first = CreateBody(new string('a', 64), "field:10000000-0000-0000-0000-000000000001");
        PredictionBody equivalent = first with
        {
            ProposedWellPath = first.ProposedWellPath.ToArray(),
            Formations = first.Formations.ToArray(),
            FluidClasses = first.FluidClasses.ToArray(),
            ContactPredictions = first.ContactPredictions.ToArray(),
            ProductionForecasts = first.ProductionForecasts.ToArray(),
            UncertaintyAssumptions = first.UncertaintyAssumptions.ToArray(),
            CitedEvidenceIds = first.CitedEvidenceIds.ToArray()
        };

        string firstHash = PredictionLedgerService.ComputeCanonicalSha256(first);
        string equivalentHash = PredictionLedgerService.ComputeCanonicalSha256(equivalent);

        Assert.That(equivalentHash, Is.EqualTo(firstHash));
        Assert.That(firstHash, Does.Match("^[0-9a-f]{64}$"));
    }

    [Test]
    public void LegacyCanonicalBytes_AreUnchangedAndMatchSimulatorDto()
    {
        PredictionBody body = CreateBody(new string('a', 64), "field:10000000-0000-0000-0000-000000000001");
        string canonical = PredictionJson.Canonicalize(body);
        string legacy = PredictionJson.Canonicalize(new
        {
            body.CandidateId, body.ProposedWellPath, body.Formations, body.ExpectedPaydirtM,
            body.FluidClasses, body.ContactPredictions, body.ProductionForecasts, body.UncertaintyAssumptions,
            body.CitedEvidenceIds, body.FieldPackageSha256, body.Rationale
        });
        var simulator = System.Text.Json.JsonSerializer.Deserialize<DrillingOperations.AnalysisPredictionBodyDto>(
            canonical, DrillingOperations.CanonicalJson.SerializerOptions)!;
        Assert.Multiple(() =>
        {
            Assert.That(canonical, Is.EqualTo(legacy));
            Assert.That(canonical, Does.Not.Contain("analysisBinding"));
            Assert.That(DrillingOperations.CanonicalJson.Serialize(simulator), Is.EqualTo(legacy));
            Assert.That(PredictionJson.ComputeSha256(PredictionJson.Deserialize<PredictionBody>(legacy, "legacy")),
                Is.EqualTo(PredictionJson.ComputeSha256(body)));
        });
        JsonNode unknown = JsonNode.Parse(canonical)!;
        unknown["configuration"] = System.Text.Json.JsonSerializer.SerializeToNode(AnalysisConfiguration.Default);
        Assert.Throws<InvalidDataException>(() => PredictionJson.Deserialize<PredictionBody>(unknown.ToJsonString(), "unknown config"));
    }

    [Test]
    public async Task ConfiguredDraft_UnversionedBindingAndStrippedMetadataCannotUseLegacyBaselines()
    {
        AnalysisPackage source = AnalysisConfigurationTests.CreatePackage();
        var time = new SettableTimeProvider(T0);
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService scenarios = CreateScenarioService(store, source, time);
        Scenario scenario = await scenarios.CreateAsync(new CreateScenarioRequest(source.FieldId, "Target", T0, "configured", new string('b', 64)));
        AnalysisPackage package = await scenarios.GetPackageAsync(source.FieldId, scenario.ScenarioId, T0);
        var analyzer = new PetrophysicsAnalysisService(time);
        var ledger = new PredictionLedgerService(store, scenarios, analyzer, time);
        AnalysisResult configured = analyzer.Analyze(package, "Target",
            AnalysisConfiguration.Default with { GridPointsPerAxis = 9 });
        PredictionBody body = CreateBody(package.Sha256, $"field:{source.FieldId:D}") with
        {
            CandidateId = configured.Ranking[0].CandidateId,
            AnalysisBinding = new(configured.Configuration, configured.ConfigurationSha256, configured.AnalysisSha256)
        };
        Assert.ThrowsAsync<ScenarioApiException>(() => ledger.PutDraftAsync(scenario.ScenarioId, body));
        Assert.Throws<ScenarioApiException>(() => BaselineFactory.Create(scenario.ScenarioId, body, configured));
        Assert.ThrowsAsync<ScenarioApiException>(() => store.SealPredictionAsync(scenario.ScenarioId,
            PredictionJson.Canonicalize(body), PredictionJson.ComputeSha256(body), [], T0));
        Assert.ThrowsAsync<ScenarioApiException>(() => ledger.PutDraftAsync(scenario.ScenarioId, body with { AnalysisBinding = null }));
        AnalysisResult legacyAnalysis = analyzer.Analyze(package, "Target");
        PredictionBody legacyBody = body with { AnalysisBinding = null, CandidateId = legacyAnalysis.Ranking[0].CandidateId };
        Assert.Throws<ScenarioApiException>(() => BaselineFactory.Create(scenario.ScenarioId, legacyBody, configured));
        await ledger.PutDraftAsync(scenario.ScenarioId, legacyBody);
        PredictionRecord sealedLegacy = await ledger.SealAsync(scenario.ScenarioId);
        Assert.That(sealedLegacy.Seal!.Sha256, Is.EqualTo(PredictionJson.ComputeSha256(legacyBody)));
        Assert.That(sealedLegacy.Baselines, Has.Count.EqualTo(4));
    }

    [Test]
    public async Task ConfiguredDraft_RejectsWrongResultConfigurationAndReservoirBindings()
    {
        AnalysisPackage source = AnalysisConfigurationTests.CreatePackage();
        var time = new SettableTimeProvider(T0);
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService scenarios = CreateScenarioService(store, source, time);
        Scenario scenario = await scenarios.CreateAsync(new CreateScenarioRequest(source.FieldId, "Target", T0, "configured", new string('b', 64)));
        AnalysisPackage package = await scenarios.GetPackageAsync(source.FieldId, scenario.ScenarioId, T0);
        var analyzer = new PetrophysicsAnalysisService(time);
        var ledger = new PredictionLedgerService(store, scenarios, analyzer, time);
        AnalysisResult result = analyzer.Analyze(package, "Target", AnalysisConfiguration.Default with { GridPointsPerAxis = 9 });
        PredictionBody body = CreateBody(package.Sha256, $"field:{source.FieldId:D}") with
        {
            CandidateId = result.Ranking[0].CandidateId,
            AnalysisBinding = new(result.Configuration, result.ConfigurationSha256, result.AnalysisSha256)
        };
        Assert.ThrowsAsync<ScenarioApiException>(() => ledger.PutDraftAsync(scenario.ScenarioId,
            body with { AnalysisBinding = body.AnalysisBinding with { ConfigurationSha256 = new string('a', 64) } }));
        Assert.ThrowsAsync<ScenarioApiException>(() => ledger.PutDraftAsync(scenario.ScenarioId,
            body with { AnalysisBinding = body.AnalysisBinding with { AnalysisSha256 = new string('a', 64) } }));
        Assert.ThrowsAsync<ScenarioApiException>(() => ledger.PutDraftAsync(scenario.ScenarioId,
            body with { FieldPackageSha256 = new string('a', 64) }));
        AnalysisResult wrongReservoir = analyzer.Analyze(package, null, result.Configuration);
        Assert.ThrowsAsync<ScenarioApiException>(() => ledger.PutDraftAsync(scenario.ScenarioId,
            body with { AnalysisBinding = body.AnalysisBinding with { AnalysisSha256 = wrongReservoir.AnalysisSha256 } }));
        Assert.That(await store.FindPredictionAsync(scenario.ScenarioId), Is.Null);
    }

    [Test]
    public async Task Seal_RejectsPackageAndEvidenceMismatches()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody wrongPackage = CreateBody(new string('a', 64), context.FieldEvidenceId);
        PredictionRecord wrongPackageDraft = await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, wrongPackage);

        ScenarioApiException packageException = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.SealAsync(context.Scenario.ScenarioId))!;

        PredictionBody hiddenCitation = wrongPackage with
        {
            FieldPackageSha256 = context.Package.Sha256,
            CitedEvidenceIds = [$"well:{Guid.NewGuid():D}"]
        };
        await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, hiddenCitation, wrongPackageDraft.Revision);
        ScenarioApiException evidenceException = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.SealAsync(context.Scenario.ScenarioId))!;

        Assert.Multiple(() =>
        {
            Assert.That(packageException.StatusCode, Is.EqualTo(409));
            Assert.That(packageException.Title, Is.EqualTo("Field package hash mismatch"));
            Assert.That(evidenceException.StatusCode, Is.EqualTo(409));
            Assert.That(evidenceException.Title, Is.EqualTo("Prediction evidence mismatch"));
        });
    }

    [Test]
    public async Task Seal_RequiresReviewedRevisionAndChecksItInsideTheTransaction()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        Guid scenarioId = context.Scenario.ScenarioId;
        PredictionRecord first = await context.Ledger.PutDraftAsync(scenarioId, body);
        await context.Ledger.PutDraftAsync(scenarioId, body with { Rationale = "Concurrent edit." }, first.Revision);
        PredictionRecord restored = await context.Ledger.PutDraftAsync(scenarioId, body, 2);
        var serviceConflict = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.SealAsync(scenarioId, expectedRevision: first.Revision))!;
        IReadOnlyList<BaselineSnapshot> baselines = BaselineFactory.Create(
            scenarioId, body, context.Analysis.Analyze(context.Package, context.Scenario.ReservoirName));
        var transactionConflict = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Store.SealPredictionAsync(scenarioId, PredictionJson.Canonicalize(body),
                PredictionJson.ComputeSha256(body), baselines, context.Time.GetUtcNow(),
                expectedRevision: first.Revision))!;
        Assert.Multiple(() =>
        {
            Assert.That(serviceConflict.StatusCode, Is.EqualTo(409));
            Assert.That(transactionConflict.StatusCode, Is.EqualTo(409));
        });
        Assert.That((await context.Ledger.GetAsync(scenarioId)).Seal, Is.Null);
        PredictionRecord sealedRecord = await context.Ledger.SealAsync(scenarioId, expectedRevision: restored.Revision);
        PredictionRecord retry = await context.Ledger.SealAsync(scenarioId, expectedRevision: restored.Revision);
        Assert.That(retry.Seal, Is.EqualTo(sealedRecord.Seal));
        Assert.That(retry.Approval, Is.Null);
        Assert.That((await context.Scenarios.GetAsync(scenarioId)).Status, Is.EqualTo(ScenarioStatus.PredictionSealed));
        Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.SealAsync(scenarioId, expectedRevision: first.Revision));
    }

    [Test]
    public async Task Seal_PreventsFurtherDraftMutationAndCapturesExactlyFourBaselines()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);

        PredictionRecord sealedPrediction = await context.Ledger.SealAsync(context.Scenario.ScenarioId);
        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.PutDraftAsync(
                context.Scenario.ScenarioId,
                body with { Rationale = "Forbidden mutation." }))!;
        Scenario scenario = await context.Scenarios.GetAsync(context.Scenario.ScenarioId);
        BaselineSnapshot nearest = sealedPrediction.Baselines.Single(item => item.Kind == BaselineKind.NearestWell);
        BaselineSnapshot fieldMean = sealedPrediction.Baselines.Single(item => item.Kind == BaselineKind.FieldMean);
        BaselineSnapshot idw = sealedPrediction.Baselines.Single(item => item.Kind == BaselineKind.FourNeighborIdw);
        BaselineSnapshot rankOne = sealedPrediction.Baselines.Single(item => item.Kind == BaselineKind.UncertaintyAwareRank1);

        Assert.Multiple(() =>
        {
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            Assert.That(sealedPrediction.Seal, Is.Not.Null);
            Assert.That(sealedPrediction.Seal!.BaselinesSha256, Does.Match("^[0-9a-f]{64}$"));
            Assert.That(sealedPrediction.Baselines, Has.Count.EqualTo(4));
            Assert.That(sealedPrediction.Baselines.Select(item => item.Kind), Is.EquivalentTo(Enum.GetValues<BaselineKind>()));
            Assert.That(sealedPrediction.Baselines.Select(item => item.BaselineId), Is.Unique);
            Assert.That(sealedPrediction.Baselines.All(item => item.ContentSha256.Length == 64), Is.True);
            Assert.That(sealedPrediction.Baselines.All(item => item.ExpectedPaydirtM is not null), Is.True);
            Assert.That(nearest.CandidateId, Is.EqualTo(body.CandidateId));
            Assert.That(nearest.TargetEastingM, Is.EqualTo(1_000));
            Assert.That(nearest.TargetNorthingM, Is.EqualTo(2_000));
            Assert.That(nearest.ContributingEvidenceIds, Is.EqualTo(new[] { $"well:{WellId(1):D}" }));
            Assert.That(fieldMean.CandidateId, Is.EqualTo(body.CandidateId));
            Assert.That(fieldMean.TargetEastingM, Is.EqualTo(1_000));
            Assert.That(fieldMean.TargetNorthingM, Is.EqualTo(2_000));
            Assert.That(fieldMean.ContributingEvidenceIds,
                Is.EqualTo(Enumerable.Range(1, 5).Select(index => $"well:{WellId(index):D}")));
            Assert.That(idw.CandidateId, Is.EqualTo(body.CandidateId));
            Assert.That(idw.TargetEastingM, Is.EqualTo(1_100));
            Assert.That(idw.TargetNorthingM, Is.EqualTo(2_100));
            Assert.That(idw.ExpectedPaydirtM, Is.EqualTo(new QuantileValues(10, 20, 30)));
            Assert.That(idw.ContributingEvidenceIds,
                Is.EqualTo(Enumerable.Range(2, 4).Select(index => $"well:{WellId(index):D}")));
            Assert.That(rankOne.CandidateId, Is.EqualTo("candidate:rank-one"));
            Assert.That(rankOne.TargetEastingM, Is.EqualTo(9_000));
            Assert.That(rankOne.TargetNorthingM, Is.EqualTo(9_500));
            Assert.That(rankOne.ContributingEvidenceIds,
                Is.EqualTo(Enumerable.Range(1, 4).Select(index => $"well:{WellId(index):D}")));
            Assert.That(sealedPrediction.Baselines.All(item => item.FormationTopTrueVerticalDepthM is null), Is.True);
            Assert.That(sealedPrediction.Baselines.All(item => item.ContactPredictions is null), Is.True);
            Assert.That(sealedPrediction.Baselines.All(item => item.ProductionForecasts is null), Is.True);
            Assert.That(sealedPrediction.Baselines.All(item => item.Limitation.Length > 0), Is.True);
            Assert.That(scenario.Status, Is.EqualTo(ScenarioStatus.PredictionSealed));
        });
    }

    [Test]
    public async Task Seal_BaselinesUseScenarioCreationPackageAfterLiveSourceMutation()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);
        context.SourcePackage.Field["futureValue"] = "must-not-affect-baselines";
        context.SourcePackage.Wells[0]["futureValue"] = 42;

        PredictionRecord sealedPrediction =
            await context.Ledger.SealAsync(context.Scenario.ScenarioId);

        Assert.That(
            sealedPrediction.Baselines.Select(item => item.PackageSha256),
            Is.All.EqualTo(context.Package.Sha256));
    }

    [Test]
    public async Task Seal_RejectsCandidateOutsideCurrentRanking()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId) with
        {
            CandidateId = "candidate:not-currently-ranked"
        };
        await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);

        ScenarioApiException exception = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.SealAsync(context.Scenario.ScenarioId))!;

        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.Title, Is.EqualTo("Prediction baseline unavailable"));
    }

    [Test]
    public async Task DraftRevision_RejectsStaleAAfterBAndAllowsExactBRetry()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody bodyA = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        PredictionRecord revisionOne = await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, bodyA);
        PredictionBody bodyB = bodyA with { Rationale = "Revision B." };
        PredictionRecord revisionTwo = await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            bodyB,
            revisionOne.Revision);

        ScenarioApiException stale = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, bodyA, revisionOne.Revision))!;
        PredictionRecord exactRetry = await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            bodyB,
            revisionOne.Revision);
        ScenarioApiException missingRevision = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.PutDraftAsync(
                context.Scenario.ScenarioId,
                bodyB with { Rationale = "Revision C." }))!;

        Assert.Multiple(() =>
        {
            Assert.That(revisionOne.Revision, Is.EqualTo(1));
            Assert.That(revisionTwo.Revision, Is.EqualTo(2));
            Assert.That(stale.StatusCode, Is.EqualTo(409));
            Assert.That(exactRetry.Revision, Is.EqualTo(2));
            Assert.That(missingRevision.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task ConcurrentDraftUpdates_AllowOnlyOneExpectedRevisionWinner()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody original = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        PredictionRecord revisionOne = await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, original);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<object> UpdateAsync(string rationale)
        {
            await start.Task;
            try
            {
                return await context.Ledger.PutDraftAsync(
                    context.Scenario.ScenarioId,
                    original with { Rationale = rationale },
                    revisionOne.Revision);
            }
            catch (ScenarioApiException exception)
            {
                return exception;
            }
        }

        Task<object> first = UpdateAsync("Concurrent B.");
        Task<object> second = UpdateAsync("Concurrent C.");
        start.SetResult();
        object[] results = await Task.WhenAll(first, second);
        PredictionRecord current = await context.Ledger.GetAsync(context.Scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(results.OfType<PredictionRecord>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>().Single().StatusCode, Is.EqualTo(409));
            Assert.That(current.Revision, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task PredictionReadsRemainSnapshotConsistentDuringSeal()
    {
        LedgerContext context = await CreateContextAsync();
        await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            CreateBody(context.Package.Sha256, context.FieldEvidenceId));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<PredictionRecord>[] reads = Enumerable.Range(0, 32).Select(async _ =>
        {
            await start.Task;
            return await context.Ledger.GetAsync(context.Scenario.ScenarioId);
        }).ToArray();
        Task<PredictionRecord> seal = Task.Run(async () =>
        {
            await start.Task;
            return await context.Ledger.SealAsync(context.Scenario.ScenarioId);
        });

        start.SetResult();
        PredictionRecord[] observed = await Task.WhenAll(reads);
        PredictionRecord sealedPrediction = await seal;

        Assert.That(observed.Append(sealedPrediction).All(record =>
            record.Seal is null && record.Baselines.Count == 0 ||
            record.Seal is not null && record.Baselines.Count == 4), Is.True);
    }

    [Test]
    public async Task BaselineIntegrityTamperingFailsLoudly()
    {
        LedgerContext context = await CreateContextAsync();
        await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            CreateBody(context.Package.Sha256, context.FieldEvidenceId));
        await context.Ledger.SealAsync(context.Scenario.ScenarioId);

        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE prediction_baselines
                SET content_sha256 = $tampered
                WHERE scenario_id = $scenario_id AND baseline_kind = $baseline_kind;
                """;
            command.Parameters.AddWithValue("$tampered", new string('0', 64));
            command.Parameters.AddWithValue("$scenario_id", context.Scenario.ScenarioId.ToString("D"));
            command.Parameters.AddWithValue("$baseline_kind", BaselineKind.NearestWell.ToString());
            await command.ExecuteNonQueryAsync();
        }

        var restartedStore = new SqliteScenarioStore(_connectionString);
        ScenarioService restartedScenarios = CreateScenarioService(restartedStore, context.SourcePackage, context.Time);
        var restartedLedger = new PredictionLedgerService(
            restartedStore,
            restartedScenarios,
            context.Analysis,
            context.Time);

        Assert.ThrowsAsync<InvalidDataException>(() =>
            restartedLedger.GetAsync(context.Scenario.ScenarioId));
    }

    [Test]
    public async Task BaselineBundleTamperingFailsLoudly()
    {
        LedgerContext context = await CreateContextAsync();
        await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            CreateBody(context.Package.Sha256, context.FieldEvidenceId));
        await context.Ledger.SealAsync(context.Scenario.ScenarioId);

        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE prediction_seals
                SET baselines_sha256 = $tampered
                WHERE scenario_id = $scenario_id;
                """;
            command.Parameters.AddWithValue("$tampered", new string('0', 64));
            command.Parameters.AddWithValue("$scenario_id", context.Scenario.ScenarioId.ToString("D"));
            await command.ExecuteNonQueryAsync();
        }

        var restartedStore = new SqliteScenarioStore(_connectionString);
        ScenarioService restartedScenarios = CreateScenarioService(restartedStore, context.SourcePackage, context.Time);
        var restartedLedger = new PredictionLedgerService(
            restartedStore,
            restartedScenarios,
            context.Analysis,
            context.Time);

        Assert.ThrowsAsync<InvalidDataException>(() =>
            restartedLedger.GetAsync(context.Scenario.ScenarioId));
    }

    [Test]
    public void RevisionEtags_AreExactQuotedPositiveIntegers()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PredictionLedgerService.ParseIfMatchRevision("\"12\""), Is.EqualTo(12));
            Assert.That(PredictionLedgerService.FormatRevisionEtag(12), Is.EqualTo("\"12\""));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ParseIfMatchRevision("12"));
            Assert.Throws<ScenarioApiException>(() => PredictionLedgerService.ParseIfMatchRevision("W/\"12\""));
        });
    }

    [Test]
    public async Task Approval_RequiresHumanActorAndAdvancesStatusSeparately()
    {
        LedgerContext context = await CreateContextAsync();
        await context.Ledger.PutDraftAsync(
            context.Scenario.ScenarioId,
            CreateBody(context.Package.Sha256, context.FieldEvidenceId));
        PredictionRecord sealedPrediction = await context.Ledger.SealAsync(context.Scenario.ScenarioId);

        ScenarioApiException missingActor = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.ApproveAsync(context.Scenario.ScenarioId, "  "))!;
        PredictionRecord approved = await context.Ledger.ApproveAsync(context.Scenario.ScenarioId, " human@example.com ");
        Scenario scenario = await context.Scenarios.GetAsync(context.Scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(missingActor.StatusCode, Is.EqualTo(400));
            Assert.That(approved.Approval, Is.Not.Null);
            Assert.That(approved.Approval!.Actor, Is.EqualTo("human@example.com"));
            Assert.That(approved.Approval.SealedSha256, Is.EqualTo(sealedPrediction.Seal!.Sha256));
            Assert.That(scenario.Status, Is.EqualTo(ScenarioStatus.HumanApproved));
        });
    }

    [Test]
    public async Task IdenticalDraftSealAndApprovalRetriesAreIdempotent()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        PredictionRecord draft = await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);
        context.Time.Advance(TimeSpan.FromMinutes(1));
        PredictionRecord draftRetry = await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);

        PredictionRecord sealedPrediction = await context.Ledger.SealAsync(context.Scenario.ScenarioId);
        context.Time.Advance(TimeSpan.FromMinutes(1));
        PredictionRecord sealRetry = await context.Ledger.SealAsync(context.Scenario.ScenarioId);

        PredictionRecord approved = await context.Ledger.ApproveAsync(context.Scenario.ScenarioId, "reviewer");
        context.Time.Advance(TimeSpan.FromMinutes(1));
        PredictionRecord approvalRetry = await context.Ledger.ApproveAsync(context.Scenario.ScenarioId, " reviewer ");
        ScenarioApiException differentActor = Assert.ThrowsAsync<ScenarioApiException>(() =>
            context.Ledger.ApproveAsync(context.Scenario.ScenarioId, "other-reviewer"))!;

        Assert.Multiple(() =>
        {
            Assert.That(draftRetry.ModifiedUtc, Is.EqualTo(draft.ModifiedUtc));
            Assert.That(sealRetry.Seal, Is.EqualTo(sealedPrediction.Seal));
            Assert.That(sealRetry.Baselines.Select(item => item.BaselineId),
                Is.EqualTo(sealedPrediction.Baselines.Select(item => item.BaselineId)));
            Assert.That(approvalRetry.Approval, Is.EqualTo(approved.Approval));
            Assert.That(differentActor.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task ApprovedLedgerPersistsExactlyAcrossRestart()
    {
        LedgerContext context = await CreateContextAsync();
        PredictionBody body = CreateBody(context.Package.Sha256, context.FieldEvidenceId);
        await context.Ledger.PutDraftAsync(context.Scenario.ScenarioId, body);
        await context.Ledger.SealAsync(context.Scenario.ScenarioId);
        PredictionRecord approved = await context.Ledger.ApproveAsync(context.Scenario.ScenarioId, "restart-reviewer");

        var restartedStore = new SqliteScenarioStore(_connectionString);
        ScenarioService restartedScenarios = CreateScenarioService(restartedStore, context.SourcePackage, context.Time);
        var restartedLedger = new PredictionLedgerService(
            restartedStore,
            restartedScenarios,
            context.Analysis,
            context.Time);
        PredictionRecord reloaded = await restartedLedger.GetAsync(context.Scenario.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(PredictionLedgerService.ComputeCanonicalSha256(reloaded.Body),
                Is.EqualTo(PredictionLedgerService.ComputeCanonicalSha256(approved.Body)));
            Assert.That(reloaded.Seal, Is.EqualTo(approved.Seal));
            Assert.That(reloaded.Approval, Is.EqualTo(approved.Approval));
            Assert.That(reloaded.Baselines.Select(item => item.BaselineId),
                Is.EqualTo(approved.Baselines.Select(item => item.BaselineId)));
            Assert.That(reloaded.Baselines.Select(item => item.Limitation),
                Is.EqualTo(approved.Baselines.Select(item => item.Limitation)));
        });
    }

    private async Task<LedgerContext> CreateContextAsync()
    {
        AnalysisPackage sourcePackage = CreateSourcePackage();
        var time = new SettableTimeProvider(T0.AddDays(1));
        var store = new SqliteScenarioStore(_connectionString);
        ScenarioService scenarios = CreateScenarioService(store, sourcePackage, time);
        Scenario scenario = await scenarios.CreateAsync(new CreateScenarioRequest(
            sourcePackage.FieldId,
            "Target",
            T0,
            "prediction-tests",
            new string('b', 64)));
        AnalysisPackage package = await scenarios.GetPackageAsync(
            sourcePackage.FieldId,
            scenario.ScenarioId,
            scenario.AsOfUtc);
        var analysis = new FakePetrophysicsAnalysisService(time);
        var ledger = new PredictionLedgerService(
            store,
            scenarios,
            analysis,
            time);
        return new LedgerContext(
            store,
            scenarios,
            ledger,
            scenario,
            sourcePackage,
            package,
            $"field:{sourcePackage.FieldId:D}",
            analysis,
            time);
    }

    private static ScenarioService CreateScenarioService(
        SqliteScenarioStore store,
        AnalysisPackage package,
        TimeProvider time) =>
        new(store, new FakeFieldPackageService(package), new CanonicalJsonHasher(), time);

    private static AnalysisPackage CreateSourcePackage()
    {
        Guid fieldId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        JsonNode field = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = fieldId }
        };
        JsonNode[] wells = Enumerable.Range(1, 5)
            .Select(index => (JsonNode)new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = WellId(index) }
            })
            .ToArray();
        string[] gaps =
        [
            "No clusters were returned for the field.",
            "No trajectories were returned for the field.",
            "No wellbores were returned for the field wells."
        ];
        var counts = new SourceCounts(1, 0, wells.Length, 0, 0, 0, 0);
        string hash = new CanonicalJsonHasher().Compute(fieldId, field, [], wells, [], [], [], [], counts, gaps);
        return new AnalysisPackage(T0, fieldId, field, [], wells, [], [], [], [], counts, gaps, hash);
    }

    private static PredictionBody CreateBody(string packageSha256, string evidenceId) => new(
        "candidate:prediction-test",
        [
            new ProposedWellPathStation(0, 0, 1_000, 2_000),
            new ProposedWellPathStation(2_000, 1_800, 1_100, 2_100)
        ],
        [
            new FormationPrediction(
                "Target",
                new QuantileValues(1_400, 1_450, 1_500),
                new QuantileValues(1_700, 1_750, 1_800))
        ],
        new QuantileValues(10, 20, 30),
        [PredictedFluidClass.Oil],
        [],
        [
            new ProductionForecast(1, 100, 0, 10),
            new ProductionForecast(3, 250, 0, 30),
            new ProductionForecast(5, 350, 0, 50)
        ],
        ["Petrophysics uncertainty is represented by submitted P90/P50/P10 ranges."],
        [evidenceId],
        packageSha256,
        "Prediction rationale based only on cited visible evidence.");

    private sealed record LedgerContext(
        SqliteScenarioStore Store,
        ScenarioService Scenarios,
        PredictionLedgerService Ledger,
        Scenario Scenario,
        AnalysisPackage SourcePackage,
        AnalysisPackage Package,
        string FieldEvidenceId,
        IPetrophysicsAnalysisService Analysis,
        SettableTimeProvider Time);

    private static Guid WellId(int index) =>
        Guid.Parse($"30000000-0000-0000-0000-{index:D12}");

    private sealed class FakePetrophysicsAnalysisService(TimeProvider timeProvider) : IPetrophysicsAnalysisService
    {
        public AnalysisResult Analyze(AnalysisPackage package, string? reservoirName = null)
        {
            WellPaySummary[] wells = Enumerable.Range(1, 5)
                .Select(index => new WellPaySummary(
                    WellId(index),
                    $"well:{WellId(index):D}",
                    [],
                    [],
                    index * 10,
                    0.2,
                    2e-15,
                    index == 1 ? 1_010 : 2_000 + index * 100,
                    index == 1 ? 2_010 : 3_000 + index * 100))
                .ToArray();
            string[] firstNeighbors = Enumerable.Range(1, 4).Select(index => $"well:{WellId(index):D}").ToArray();
            string[] selectedNeighbors = Enumerable.Range(2, 4).Select(index => $"well:{WellId(index):D}").ToArray();
            RankedCandidate[] ranking =
            [
                new(
                    1,
                    "candidate:rank-one",
                    9_000,
                    9_500,
                    0,
                    0,
                    800,
                    30,
                    40,
                    50,
                    8,
                    0.2,
                    2,
                    0.2,
                    100,
                    firstNeighbors),
                new(
                    2,
                    "candidate:prediction-test",
                    1_100,
                    2_100,
                    0,
                    0,
                    100,
                    10,
                    20,
                    30,
                    8,
                    0.2,
                    2,
                    0.4,
                    80,
                    selectedNeighbors)
            ];
            var methodology = new AnalysisMethodology(
                "test four-neighbor IDW",
                0.12,
                1e-15,
                "positive",
                "adjacent",
                "test grid",
                4,
                500,
                1.2816,
                "well disagreement",
                "uncertainty-aware score");
            return new AnalysisResult(
                timeProvider.GetUtcNow(),
                package.FieldId,
                reservoirName,
                package.Sha256,
                methodology,
                wells,
                ranking,
                []);
        }

        public NetPayResult CalculateNetPay(string evidenceId, IEnumerable<PetrophysicsSample> samples) =>
            throw new NotSupportedException();
    }

    private sealed class FakeFieldPackageService(AnalysisPackage package) : IFieldPackageService
    {
        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<JsonNode>(new JsonArray());

        public Task<AnalysisPackage> BuildPackageAsync(Guid fieldId, CancellationToken cancellationToken) =>
            Task.FromResult(package);
    }

    private sealed class SettableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; private set; } = now;

        public override DateTimeOffset GetUtcNow() => UtcNow;

        public void Advance(TimeSpan duration) => UtcNow += duration;
    }
}
