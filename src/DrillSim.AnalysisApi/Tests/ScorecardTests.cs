using System.Text.Json;
using System.Text.Json.Serialization;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Core = DrillingOperations;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ScorecardTests
{
    private static readonly DateTimeOffset T0 = new(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T1 = T0.AddDays(1);
    private string _databasePath = null!;
    private string _connectionString = null!;

    [SetUp]
    public void SetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"drillsim-scorecard-{Guid.NewGuid():N}.db");
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
    public void InternalScorecard_UsesExistingCallbackAuthentication()
    {
        const string key = "scorecard-callback-key";
        var validator = new InternalCallbackKeyValidator(key);
        var missing = new HeaderDictionary();
        var wrong = new HeaderDictionary { [InternalCallbackKeyValidator.HeaderName] = "wrong" };
        var correct = new HeaderDictionary { [InternalCallbackKeyValidator.HeaderName] = key };

        Assert.Multiple(() =>
        {
            Assert.That(validator.IsAuthorized(missing), Is.False);
            Assert.That(validator.IsAuthorized(wrong), Is.False);
            Assert.That(validator.IsAuthorized(correct), Is.True);
        });
    }

    [Test]
    public void Validation_RejectsMalformedMetricsHeadlineAndUnsafeNumbers()
    {
        Guid scenarioId = Guid.NewGuid();
        ScorecardRequest valid = CreateScorecard(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        ScorecardMetric scored = valid.Metrics[0];
        ScorecardMetric unavailable = valid.Metrics[1];

        Assert.Multiple(() =>
        {
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { ScorecardId = valid.ScorecardId.ToUpperInvariant() }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { InputSha256 = new string('A', 64) }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = [scored, scored] }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = [scored with { Value = double.NaN }, unavailable] }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = [scored with { Unit = null }, unavailable] }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = [scored with { Limitation = "not allowed" }, unavailable] }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = [scored, unavailable with { Value = 1 }] }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = [scored, unavailable with { Limitation = " " }] }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { HeadlineMetric = "missing-metric" }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { HeadlineMetric = null, Limitation = null }));
            Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(
                scenarioId,
                valid with { Metrics = Enumerable.Repeat(scored, 201).ToArray() }));
        });
    }

    [Test]
    public void Validation_AcceptsUnavailableAggregateWithExplicitLimitations()
    {
        Guid scenarioId = Guid.NewGuid();
        ScorecardRequest request = CreateScorecard(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()) with
        {
            HeadlineMetric = null,
            Limitation = "No metric was eligible for a headline score.",
            Metrics =
            [
                new ScorecardMetric(
                    "production-error",
                    ScoreMetricBasis.ObservationGap,
                    ScoreMetricStatus.Unavailable,
                    null,
                    "fraction",
                    null,
                    null,
                    "Public production observations were incomplete.")
            ]
        };

        ValidatedScorecard validated = ScorecardService.Validate(scenarioId, request);

        Assert.That(validated.HeadlineMetric, Is.Null);
        Assert.That(validated.Limitation, Is.Not.Empty);
        Assert.That(validated.Metrics[0].Status, Is.EqualTo(ScoreMetricStatus.Unavailable));
    }

    [Test]
    public void AbsoluteErrorRangeMustIncludeInteriorZero_NotOnlyQuantileEndpointErrors()
    {
        Guid scenarioId = Guid.NewGuid();
        const string name = "formationTopP50AbsoluteError.HiddenTruth";
        var metric = new ScorecardMetric(name, ScoreMetricBasis.HiddenTruth, ScoreMetricStatus.Scored,
            13.136667380992094, "m", 113.1366673809921, 136.8633326190079, null);
        var request = CreateScorecard(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()) with
        {
            HeadlineMetric = name,
            Metrics = [metric]
        };
        var rejected = Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(scenarioId, request))!;
        Assert.That(rejected.StatusCode, Is.EqualTo(400));
        Assert.That(rejected.Message, Is.EqualTo("metrics[0].value: must fall within supplied bounds."));
        var accepted = ScorecardService.Validate(scenarioId, request with { Metrics = [metric with { LowerBound = 0 }] });
        Assert.That(accepted.Metrics.Single().Value, Is.EqualTo(metric.Value));
        Assert.That(accepted.Metrics.Single().UpperBound, Is.EqualTo(metric.UpperBound));
        Assert.That(request.Metrics.Single().LowerBound, Is.EqualTo(metric.LowerBound));
    }

    [Test]
    public async Task Scorecard_IsRejectedBeforeCommittedRevealAndPublicGetReturns404()
    {
        var store = new SqliteScenarioStore(_connectionString);
        Guid sourceId = Guid.NewGuid();
        Scenario scenario = await SeedScenarioAsync(store, sourceId);
        var service = new ScorecardService(store, new FixedTimeProvider(T1.AddMinutes(1)));
        ScorecardRequest request = CreateScorecard(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        ScenarioApiException publish = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.PublishAsync(scenario.ScenarioId, request))!;
        ScenarioApiException read = Assert.ThrowsAsync<ScenarioApiException>(() =>
            service.GetAsync(scenario.ScenarioId))!;

        Assert.Multiple(() =>
        {
            Assert.That(publish.StatusCode, Is.EqualTo(409));
            Assert.That(read.StatusCode, Is.EqualTo(404));
        });
    }

    [Test]
    public async Task Scorecard_RejectsRevealRunVersionAndValidTimeMismatches()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        ScorecardRequest valid = CreateScorecard(
            Guid.NewGuid(),
            fixture.RunId,
            fixture.RevealId);

        ScenarioApiException reveal = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.PublishAsync(
                fixture.ScenarioId,
                valid with { RevealId = Guid.NewGuid().ToString("D") }))!;
        ScenarioApiException run = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.PublishAsync(
                fixture.ScenarioId,
                valid with { RunId = Guid.NewGuid().ToString("D") }))!;
        ScenarioApiException version = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.PublishAsync(
                fixture.ScenarioId,
                valid with { ScoringModelVersion = "scoring-model-v999" }))!;
        ScenarioApiException time = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.PublishAsync(
                fixture.ScenarioId,
                valid with { CreatedValidTimeUtc = T1.AddMinutes(1) }))!;

        Assert.Multiple(() =>
        {
            Assert.That(reveal.StatusCode, Is.EqualTo(409));
            Assert.That(run.StatusCode, Is.EqualTo(409));
            Assert.That(version.StatusCode, Is.EqualTo(409));
            Assert.That(time.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task Scorecard_PublicationIsAtomicAndAdvancesScenarioToScored()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        ScorecardRequest request = CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId);
        ScenarioApiException beforeScore = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.GetAsync(fixture.ScenarioId))!;

        PublicScorecard scorecard = await fixture.Scorecards.PublishAsync(fixture.ScenarioId, request);
        Scenario scenario = (await fixture.Store.FindAsync(fixture.ScenarioId))!;
        PublicScorecard loaded = await fixture.Scorecards.GetAsync(fixture.ScenarioId);

        Assert.Multiple(() =>
        {
            Assert.That(beforeScore.StatusCode, Is.EqualTo(404));
            Assert.That(scenario.Status, Is.EqualTo(ScenarioStatus.Scored));
            Assert.That(PredictionJson.Canonicalize(scorecard), Is.EqualTo(PredictionJson.Canonicalize(loaded)));
            Assert.That(scorecard.ContentSha256, Does.Match("^[0-9a-f]{64}$"));
            Assert.That(scorecard.HeadlineMetric, Is.EqualTo("expected-paydirt-error"));
        });
    }

    [Test]
    public async Task Scorecard_TransactionFailureRollsBackRecordAndScenarioStatus()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        var faultStore = new SqliteScenarioStore(
            _connectionString,
            null,
            null,
            () => throw new InjectedScorecardFailureException());
        var service = new ScorecardService(faultStore, new FixedTimeProvider(T1.AddMinutes(2)));
        ScorecardRequest request = CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId);

        Assert.ThrowsAsync<InjectedScorecardFailureException>(() =>
            service.PublishAsync(fixture.ScenarioId, request));

        Scenario scenario = (await faultStore.FindAsync(fixture.ScenarioId))!;
        PublicScorecard? stored = await faultStore.FindScorecardAsync(fixture.ScenarioId);
        Assert.Multiple(() =>
        {
            Assert.That(scenario.Status, Is.EqualTo(ScenarioStatus.Revealed));
            Assert.That(stored, Is.Null);
        });
    }

    [Test]
    public async Task Scorecard_IsConcurrentIdempotentAndConflictsExplicitly()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        ScorecardRequest request = CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<PublicScorecard>[] attempts = Enumerable.Range(0, 2).Select(async _ =>
        {
            await start.Task;
            return await fixture.Scorecards.PublishAsync(fixture.ScenarioId, request);
        }).ToArray();
        start.SetResult();

        PublicScorecard[] results = await Task.WhenAll(attempts);
        PublicScorecard retry = await fixture.Scorecards.PublishAsync(fixture.ScenarioId, request);
        ScenarioApiException changed = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.PublishAsync(
                fixture.ScenarioId,
                request with { InputSha256 = new string('e', 64) }))!;
        ScenarioApiException newId = Assert.ThrowsAsync<ScenarioApiException>(() =>
            fixture.Scorecards.PublishAsync(
                fixture.ScenarioId,
                request with { ScorecardId = Guid.NewGuid().ToString("D") }))!;

        Assert.Multiple(() =>
        {
            Assert.That(PredictionJson.Canonicalize(results[1]), Is.EqualTo(PredictionJson.Canonicalize(results[0])));
            Assert.That(PredictionJson.Canonicalize(retry), Is.EqualTo(PredictionJson.Canonicalize(results[0])));
            Assert.That(changed.StatusCode, Is.EqualTo(409));
            Assert.That(newId.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task ConcurrentConflictingScorecards_CommitOneAndRejectOne()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        ScorecardRequest firstRequest = CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId);
        ScorecardRequest secondRequest = CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId) with
        {
            InputSha256 = new string('e', 64)
        };
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<object> PublishAsync(ScorecardRequest request)
        {
            await start.Task;
            try
            {
                return await fixture.Scorecards.PublishAsync(fixture.ScenarioId, request);
            }
            catch (ScenarioApiException exception)
            {
                return exception;
            }
        }

        Task<object> first = PublishAsync(firstRequest);
        Task<object> second = PublishAsync(secondRequest);
        start.SetResult();
        object[] results = await Task.WhenAll(first, second);

        Assert.Multiple(() =>
        {
            Assert.That(results.OfType<PublicScorecard>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>(), Has.Exactly(1).Items);
            Assert.That(results.OfType<ScenarioApiException>().Single().StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task Scorecard_PersistsAcrossRestartAndDetectsTampering()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        ScorecardRequest request = CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId);
        PublicScorecard published = await fixture.Scorecards.PublishAsync(fixture.ScenarioId, request);

        var restartedStore = new SqliteScenarioStore(_connectionString);
        var restarted = new ScorecardService(restartedStore, new FixedTimeProvider(T1.AddDays(1)));
        PublicScorecard reloaded = await restarted.GetAsync(fixture.ScenarioId);
        Assert.That(PredictionJson.Canonicalize(reloaded), Is.EqualTo(PredictionJson.Canonicalize(published)));

        SqliteConnection.ClearAllPools();
        await using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                DROP TRIGGER IF EXISTS tr_public_scorecards_no_update;
                UPDATE public_scorecards
                SET canonical_body_sha256 = $tampered
                WHERE scenario_id = $scenario_id;
                """;
            command.Parameters.AddWithValue("$tampered", new string('0', 64));
            command.Parameters.AddWithValue("$scenario_id", fixture.ScenarioId.ToString("D"));
            await command.ExecuteNonQueryAsync();
        }

        var tamperedStore = new SqliteScenarioStore(_connectionString);
        var tampered = new ScorecardService(tamperedStore, new FixedTimeProvider(T1.AddDays(2)));
        Assert.ThrowsAsync<InvalidDataException>(() => tampered.GetAsync(fixture.ScenarioId));
    }

    [Test]
    public async Task PublicScorecardSerializationContainsOnlySafeAggregateFields()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        PublicScorecard scorecard = await fixture.Scorecards.PublishAsync(
            fixture.ScenarioId,
            CreateScorecard(Guid.NewGuid(), fixture.RunId, fixture.RevealId));
        string json = SerializeApi(scorecard);
        string lowercaseJson = json.ToLowerInvariant();

        Assert.Multiple(() =>
        {
            Assert.That(lowercaseJson, Does.Not.Contain("runid"));
            Assert.That(lowercaseJson, Does.Not.Contain("worldid"));
            Assert.That(lowercaseJson, Does.Not.Contain("path"));
            Assert.That(lowercaseJson, Does.Not.Contain("production"));
            Assert.That(lowercaseJson, Does.Not.Contain("evidence"));
            Assert.That(json, Does.Contain("\"basis\":\"HiddenTruth\""));
            Assert.That(json, Does.Contain("\"status\":\"Scored\""));
        });
    }

    [Test]
    public async Task VersionedCorrectionPublishesThroughAuthenticatedCallbackAndSurvivesRestart()
    {
        ScorecardFixture fixture = await CreateRevealedFixtureAsync();
        var (original, corrected) = CorrectionArtifacts(fixture.ScenarioId, fixture.RunId, fixture.RevealId);
        const string secret = "correction-callback-key";
        var validator = new InternalCallbackKeyValidator(secret);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.ConfigureHttpJsonOptions(x => x.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        await using WebApplication app = builder.Build();
        app.MapPost("/internal/scenarios/{scenarioId:guid}/scorecard",
            async Task<IResult> (Guid scenarioId, ScorecardRequest body, HttpRequest request) =>
            {
                if (!validator.IsAuthorized(request.Headers)) return Results.Unauthorized();
                try { return Results.Ok(await fixture.Scorecards.PublishAsync(scenarioId, body)); }
                catch (ScenarioApiException e) { return Results.Problem(statusCode: e.StatusCode, title: e.Title, detail: e.Message); }
            });
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        var core = new Core.AnalysisScorecardClient(client);
        var correctedRequest = JsonSerializer.Deserialize<Core.AnalysisScorecardRequest>(corrected.CanonicalOutputJson, Core.CanonicalJson.SerializerOptions)!;
        using var unauthorized = await client.PostAsync($"internal/scenarios/{fixture.ScenarioId:D}/scorecard",
            new StringContent(corrected.CanonicalOutputJson, System.Text.Encoding.UTF8, "application/json"));
        Assert.That(unauthorized.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        client.DefaultRequestHeaders.Add(InternalCallbackKeyValidator.HeaderName, secret);
        var oldRequest = JsonSerializer.Deserialize<Core.AnalysisScorecardRequest>(original.CanonicalOutputJson, Core.CanonicalJson.SerializerOptions)!;
        Core.ScoringException rejected = Assert.ThrowsAsync<Core.ScoringException>(() =>
            core.PublishAsync(fixture.ScenarioId.ToString("D"), oldRequest, CancellationToken.None))!;
        Assert.That(rejected.DiagnosticCode, Is.EqualTo("ScorecardCallbackMismatch"));
        string receiptJson = await core.PublishAsync(fixture.ScenarioId.ToString("D"), correctedRequest, CancellationToken.None);
        var receipt = JsonSerializer.Deserialize<Core.AnalysisScorecardReceipt>(receiptJson, Core.CanonicalJson.SerializerOptions)!;
        Assert.That(Core.AnalysisScorecardClient.ReceiptMatches(receipt, fixture.ScenarioId.ToString("D"), correctedRequest), Is.True);
        Assert.That(receipt.ScorecardId.ToString("D"), Is.EqualTo(corrected.ScorecardId));
        Assert.That(receipt.Correction!.SupersedesOutputSha256, Is.EqualTo(original.OutputSha256));
        Assert.That(receipt.Correction.OriginalInputSha256, Is.EqualTo(original.InputSha256));
        Assert.That(receipt.ScoringModelVersion, Is.EqualTo(ScoringCorrectionVersions.ScoringModel));
        Assert.That(await core.PublishAsync(fixture.ScenarioId.ToString("D"), correctedRequest, CancellationToken.None), Is.EqualTo(receiptJson));
        var restarted = new ScorecardService(new SqliteScenarioStore(_connectionString), TimeProvider.System);
        PublicScorecard stored = await restarted.GetAsync(fixture.ScenarioId);
        Assert.That(stored.ScorecardId.ToString("D"), Is.EqualTo(corrected.ScorecardId));
        Assert.That(stored.Correction!.SupersedesScorecardId, Is.EqualTo(original.ScorecardId));
        Assert.That(SerializeApi(stored), Does.Not.Contain("worldId").And.Not.Contain("canonicalInputJson"));
        await app.StopAsync();
    }

    [TestCase("version")]
    [TestCase("missing-provenance")]
    [TestCase("old-id")]
    [TestCase("original-input")]
    [TestCase("original-output")]
    public void CorrectionProvenanceMustBindVersionIdentityAndOriginalHashes(string invalid)
    {
        Guid scenario = Guid.NewGuid(), run = Guid.NewGuid(), reveal = Guid.NewGuid();
        var (original, corrected) = CorrectionArtifacts(scenario, run, reveal);
        ScorecardRequest body = JsonSerializer.Deserialize<ScorecardRequest>(corrected.CanonicalOutputJson, Core.CanonicalJson.SerializerOptions)!;
        body = invalid switch
        {
            "version" => body with { ScoringModelVersion = ScenarioModelVersions.Scoring },
            "missing-provenance" => body with { Correction = null },
            "old-id" => body with { ScorecardId = original.ScorecardId },
            "original-input" => body with { Correction = body.Correction! with { OriginalInputSha256 = new string('b', 64) } },
            "original-output" => body with { Correction = body.Correction! with { SupersedesOutputSha256 = new string('b', 64) } },
            _ => throw new AssertionException("Unknown variant")
        };
        Assert.That(Assert.Throws<ScenarioApiException>(() => ScorecardService.Validate(scenario, body))!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void LegacyScorecardCanonicalBytesOmitAllCorrectionMetadata()
    {
        var (old, _) = CorrectionArtifacts(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var request = JsonSerializer.Deserialize<Core.AnalysisScorecardRequest>(old.CanonicalOutputJson, Core.CanonicalJson.SerializerOptions)!;
        string expected = Core.CanonicalJson.Serialize(new
        {
            request.ScorecardId, request.RunId, request.RevealId, request.ScoringModelVersion,
            request.InputSha256, request.HeadlineMetric, request.Metrics, request.CreatedValidTimeUtc
        });
        Assert.That(Core.CanonicalJson.Serialize(request), Is.EqualTo(expected));
        Assert.That(expected, Does.Not.Contain("correction").And.Not.Contain("supersedes"));
        var publicCard = new PublicScorecard(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ScenarioModelVersions.Scoring,
            new string('a', 64), null, [], T1, "Unavailable", new string('b', 64));
        string oldApiBytes = SerializeApi(new
        {
            publicCard.ScenarioId, publicCard.ScorecardId, publicCard.RevealId, publicCard.ScoringModelVersion,
            publicCard.InputSha256, publicCard.HeadlineMetric, publicCard.Metrics, publicCard.CreatedValidTimeUtc,
            publicCard.Limitation, publicCard.ContentSha256
        });
        Assert.That(SerializeApi(publicCard), Is.EqualTo(oldApiBytes));
    }

    private static (Core.ScorecardDraft Original, Core.ScorecardDraft Corrected) CorrectionArtifacts(Guid scenarioId, Guid runId, Guid revealId)
    {
        const string name = "expectedPaydirtP50AbsoluteError.HiddenTruth";
        string input = Core.CanonicalJson.Serialize(new { scoringModelVersion = Core.ScoreArtifactIntegrity.ModelVersion,
            runId = runId.ToString("D"), scenarioId = scenarioId.ToString("D"), revealId = revealId.ToString("D"), artifacts = Array.Empty<object>() });
        string inputHash = Core.DeterministicIdentity.Sha256(input);
        string originalId = Core.DeterministicIdentity.Create("scorecard-v1", runId.ToString("D"), revealId.ToString("D"), Core.ScoreArtifactIntegrity.ModelVersion, inputHash);
        Core.ScorecardMetric[] metrics = [new(name, Core.ScoreMetricBasis.HiddenTruth, Core.ScoreMetricStatus.Scored, 0, "m", 10, 20, null)];
        var body = new Core.AnalysisScorecardRequest(originalId, runId.ToString("D"), revealId.ToString("D"),
            Core.ScoreArtifactIntegrity.ModelVersion, inputHash, name, metrics, T1);
        string output = Core.CanonicalJson.Serialize(body);
        var original = new Core.ScorecardDraft(originalId, runId.ToString("D"), scenarioId.ToString("D"), revealId.ToString("D"),
            Core.ScoreArtifactIntegrity.ModelVersion, inputHash, input, output, Core.DeterministicIdentity.Sha256(output), name, metrics, T1);
        Core.ScorecardMetric[] corrected = [metrics[0] with { LowerBound = 0 }];
        string correctedOutput = Core.CanonicalJson.Serialize(body with { Metrics = corrected });
        var recalculated = original with { Metrics = corrected, CanonicalOutputJson = correctedOutput, OutputSha256 = Core.DeterministicIdentity.Sha256(correctedOutput) };
        return (original, Core.ScoreCorrection.Create(original, original, recalculated));
    }

    private async Task<ScorecardFixture> CreateRevealedFixtureAsync()
    {
        var store = new SqliteScenarioStore(_connectionString);
        Guid sourceId = Guid.NewGuid();
        Guid cloneId = Guid.NewGuid();
        Guid scenarioId = Guid.NewGuid();
        Guid runId = Guid.NewGuid();
        Guid revealId = Guid.NewGuid();
        var scenario = new Scenario(
            scenarioId,
            sourceId,
            null,
            "Target",
            T0,
            T0,
            "scorecard-tests",
            ScenarioModelVersions.World,
            ScenarioModelVersions.Observation,
            ScenarioModelVersions.Scoring,
            ScenarioStatus.HumanApproved,
            new string('a', 64),
            T0,
            T0);
        await store.CreateLegacyScenarioAsync(
            scenario,
            [new EvidenceVisibility(scenarioId, $"field:{sourceId:D}", EvidenceCatalog.Field, T0, null, null)]);
        var time = new FixedTimeProvider(T1.AddMinutes(1));
        var reveals = new RevealService(store, time);
        AnalysisPackage clonePackage = RevealPackageTestData.Create(cloneId, T1);
        var reveal = new RevealRequest(
            revealId.ToString("D"),
            runId.ToString("D"),
            cloneId.ToString("D"),
            T1,
            ScenarioModelVersions.Observation,
            new string('b', 64),
            RevealPackageTestData.Evidence(clonePackage),
            new ProductionSeriesRequest(
                Guid.NewGuid().ToString("D"),
                "production-meter-v1",
                RevealPackageTestData.ProductionHash(clonePackage),
                60,
                [1, 3, 5]),
            clonePackage);
        await reveals.PrepareAsync(scenarioId, reveal);
        await reveals.FinalizeAsync(
            scenarioId,
            new FinalizeRevealRequest(reveal.RevealId, reveal.ManifestSha256));
        return new ScorecardFixture(
            store,
            new ScorecardService(store, time),
            scenarioId,
            runId,
            revealId);
    }

    private static async Task<Scenario> SeedScenarioAsync(SqliteScenarioStore store, Guid sourceId)
    {
        Guid scenarioId = Guid.NewGuid();
        var scenario = new Scenario(
            scenarioId,
            sourceId,
            null,
            "Target",
            T0,
            T0,
            "scorecard-tests",
            ScenarioModelVersions.World,
            ScenarioModelVersions.Observation,
            ScenarioModelVersions.Scoring,
            ScenarioStatus.HumanApproved,
            new string('a', 64),
            T0,
            T0);
        return await store.CreateLegacyScenarioAsync(
            scenario,
            [new EvidenceVisibility(scenarioId, $"field:{sourceId:D}", EvidenceCatalog.Field, T0, null, null)]);
    }

    private static ScorecardRequest CreateScorecard(Guid scorecardId, Guid runId, Guid revealId) => new(
        scorecardId.ToString("D"),
        runId.ToString("D"),
        revealId.ToString("D"),
        ScenarioModelVersions.Scoring,
        new string('f', 64),
        "expected-paydirt-error",
        [
            new ScorecardMetric(
                "expected-paydirt-error",
                ScoreMetricBasis.HiddenTruth,
                ScoreMetricStatus.Scored,
                2.5,
                "m",
                0,
                100,
                null),
            new ScorecardMetric(
                "contact-depth-error",
                ScoreMetricBasis.ObservationGap,
                ScoreMetricStatus.Unavailable,
                null,
                "m",
                null,
                null,
                "No public contact observation was available.")
        ],
        T1,
        null);

    private static string SerializeApi<T>(T value)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Serialize(value, options);
    }

    private sealed record ScorecardFixture(
        SqliteScenarioStore Store,
        ScorecardService Scorecards,
        Guid ScenarioId,
        Guid RunId,
        Guid RevealId);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InjectedScorecardFailureException : Exception;
}
