using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ScenarioAgentScopeTests
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
    private string _databasePath = null!;

    [SetUp]
    public void SetUp() =>
        _databasePath = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"drillsim-agent-scope-{Guid.NewGuid():N}.db");

    [TearDown]
    public void TearDown()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    [TestCase(ScenarioStatus.Draft, false)]
    [TestCase(ScenarioStatus.HumanApproved, false)]
    [TestCase(ScenarioStatus.Revealed, true)]
    [TestCase(ScenarioStatus.Scored, true)]
    public void Create_SelectsFieldFromScenarioStatus(ScenarioStatus status, bool expectsClone)
    {
        Guid sourceFieldId = Guid.NewGuid();
        Guid clonedFieldId = Guid.NewGuid();
        Scenario scenario = CreateScenario(Guid.NewGuid(), sourceFieldId, status, clonedFieldId);

        ScenarioAgentScope scope = ScenarioAgentScope.Create(scenario);

        Assert.That(scope.FieldId, Is.EqualTo(expectsClone ? clonedFieldId : sourceFieldId));
        Assert.That(scope.AsOfUtc, Is.EqualTo(scenario.AsOfUtc));
        Assert.That(scope.ReservoirName, Is.EqualTo(scenario.ReservoirName));
    }

    [Test]
    public void Create_RevealedScenarioWithoutCloneIsRejected()
    {
        Scenario scenario = CreateScenario(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ScenarioStatus.Revealed);

        ScenarioApiException exception =
            Assert.Throws<ScenarioApiException>(() => ScenarioAgentScope.Create(scenario))!;

        Assert.That(exception.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [TestCase("/agui/scenario", true)]
    [TestCase("/agui/scenario/", true)]
    [TestCase("/agui/scenario/other", false)]
    [TestCase("/agui/scenarios", false)]
    public void IsScenarioEndpoint_MatchesOnlyRouteEquivalentForms(string path, bool expected) =>
        Assert.That(ScenarioAgentScopeBinding.IsScenarioEndpoint(path), Is.EqualTo(expected));

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not-a-guid")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public async Task Binding_MissingOrMalformedHeaderIsRejected(string? header)
    {
        await using ServiceProvider services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Response.Body = new MemoryStream();
        if (header is not null)
            context.Request.Headers[ScenarioAgentScope.HeaderName] = header;
        bool nextCalled = false;

        await ScenarioAgentScopeBinding.InvokeAsync(
            context,
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(nextCalled, Is.False);
            Assert.That(context.Features.Get<ScenarioAgentScope>(), Is.Null);
        });
    }

    [Test]
    public async Task Binding_ParallelRequestsDoNotBleedScenarioScope()
    {
        var store = new SqliteScenarioStore(ConnectionString());
        Scenario first = CreateScenario(Guid.NewGuid(), Guid.NewGuid(), ScenarioStatus.Draft);
        Scenario second = CreateScenario(Guid.NewGuid(), Guid.NewGuid(), ScenarioStatus.Draft);
        await store.CreateLegacyScenarioAsync(first, []);
        await store.CreateLegacyScenarioAsync(second, []);
        ScenarioService scenarioService = CreateScenarioService(store, new RecordingPackageService());
        await using ServiceProvider services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(scenarioService)
            .BuildServiceProvider();
        var scopes = new ConcurrentDictionary<Guid, ScenarioAgentScope>();
        var bothEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int entered = 0;
        RequestDelegate next = async context =>
        {
            ScenarioAgentScope scope = ScenarioAgentScopeBinding.GetRequired(context);
            scopes[scope.Scenario.ScenarioId] = scope;
            if (Interlocked.Increment(ref entered) == 2)
                bothEntered.SetResult();
            await bothEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        };
        DefaultHttpContext firstContext = ContextFor(first.ScenarioId, services);
        DefaultHttpContext secondContext = ContextFor(second.ScenarioId, services);

        await Task.WhenAll(
            ScenarioAgentScopeBinding.InvokeAsync(firstContext, next),
            ScenarioAgentScopeBinding.InvokeAsync(secondContext, next));

        Assert.Multiple(() =>
        {
            Assert.That(scopes[first.ScenarioId].FieldId, Is.EqualTo(first.SourceFieldId));
            Assert.That(scopes[second.ScenarioId].FieldId, Is.EqualTo(second.SourceFieldId));
            Assert.That(firstContext.Features.Get<ScenarioAgentScope>(), Is.Not.SameAs(
                secondContext.Features.Get<ScenarioAgentScope>()));
        });
    }

    [Test]
    public async Task Tools_IgnoreForeignScopeArgumentsAndReadOnlyBoundField()
    {
        Guid boundFieldId = Guid.NewGuid();
        Guid foreignFieldId = Guid.NewGuid();
        Scenario boundScenario = CreateScenario(Guid.NewGuid(), boundFieldId, ScenarioStatus.Draft);
        Scenario foreignScenario = CreateScenario(Guid.NewGuid(), foreignFieldId, ScenarioStatus.Draft);
        var store = new SqliteScenarioStore(ConnectionString());
        await store.CreateLegacyScenarioAsync(boundScenario,
        [
            new EvidenceVisibility(
                boundScenario.ScenarioId,
                $"field:{boundFieldId:D}",
                EvidenceCatalog.Field,
                T0,
                null,
                null)
        ]);
        await store.CreateLegacyScenarioAsync(foreignScenario,
        [
            new EvidenceVisibility(
                foreignScenario.ScenarioId,
                $"field:{foreignFieldId:D}",
                EvidenceCatalog.Field,
                T0,
                null,
                null)
        ]);
        AnalysisPackage boundPackage = CreatePackage(boundFieldId);
        await store.BackfillScenarioPackageSnapshotAsync(boundScenario, boundPackage, T0);
        var packages = new RecordingPackageService(
            boundPackage,
            CreatePackage(foreignFieldId));
        ScenarioService scenarios = CreateScenarioService(store, packages);
        AIFunction[] tools = ScenarioAgentTools.Create(
            ScenarioAgentScope.Create(boundScenario),
            scenarios,
            new PetrophysicsAnalysisService(new FixedTimeProvider(T0)));
        AIFunction packageTool = tools.Single(tool => tool.Name == "get_asof_field_package");
        var maliciousArguments = new AIFunctionArguments
        {
            ["scenarioId"] = foreignScenario.ScenarioId,
            ["fieldId"] = foreignFieldId,
            ["reservoir"] = "Foreign",
            ["asOf"] = T0.AddYears(10)
        };

        object? result = await packageTool.InvokeAsync(maliciousArguments);
        JsonElement packageJson = (JsonElement)result!;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<JsonElement>());
            Assert.That(packageJson.GetProperty("fieldId").GetGuid(), Is.EqualTo(boundFieldId));
            Assert.That(packageJson.ToString(), Does.Not.Contain(foreignFieldId.ToString("D")));
            Assert.That(packages.RequestedFieldIds, Is.Empty);
        });
    }

    [Test]
    public void Tools_DefinitionsOmitAllScopeArguments()
    {
        Scenario scenario = CreateScenario(Guid.NewGuid(), Guid.NewGuid(), ScenarioStatus.Draft);
        var store = new SqliteScenarioStore(ConnectionString());
        var packages = new RecordingPackageService(CreatePackage(scenario.SourceFieldId));
        ScenarioService scenarios = CreateScenarioService(store, packages);

        AIFunction[] tools = ScenarioAgentTools.Create(
            ScenarioAgentScope.Create(scenario),
            scenarios,
            new PetrophysicsAnalysisService(new FixedTimeProvider(T0)));

        Assert.Multiple(() =>
        {
            Assert.That(tools.Select(tool => tool.Name), Is.EquivalentTo(new[]
            {
                "get_scenario_clock",
                "get_asof_field_package",
                "analyze_asof_field",
                "get_scenario_scorecard"
            }));
            foreach (AIFunction tool in tools)
            {
                Assert.That(
                    tool.JsonSchema.GetProperty("properties").EnumerateObject().Count(),
                    Is.Zero,
                    $"{tool.Name} exposed a model-controlled scope parameter.");
            }
        });
    }

    [Test]
    public async Task Scorecard_BoundSnapshotCannotObserveLaterPublication()
    {
        DateTimeOffset revealTime = T0.AddHours(1);
        Guid scenarioId = Guid.NewGuid();
        Guid sourceFieldId = Guid.NewGuid();
        Guid clonedFieldId = Guid.NewGuid();
        Guid revealId = Guid.NewGuid();
        Guid runId = Guid.NewGuid();
        var store = new SqliteScenarioStore(ConnectionString());
        Scenario initial = CreateScenario(
            scenarioId,
            sourceFieldId,
            ScenarioStatus.HumanApproved);
        await store.CreateLegacyScenarioAsync(initial,
        [
            new EvidenceVisibility(
                scenarioId,
                $"field:{sourceFieldId:D}",
                EvidenceCatalog.Field,
                T0,
                null,
                null)
        ]);
        var time = new FixedTimeProvider(revealTime.AddMinutes(1));
        var reveals = new RevealService(store, time);
        AnalysisPackage clonePackage = RevealPackageTestData.Create(clonedFieldId, revealTime);
        var revealRequest = new RevealRequest(
            revealId.ToString("D"),
            runId.ToString("D"),
            clonedFieldId.ToString("D"),
            revealTime,
            ScenarioModelVersions.Observation,
            new string('c', 64),
            RevealPackageTestData.Evidence(clonePackage),
            new ProductionSeriesRequest(
                Guid.NewGuid().ToString("D"),
                "production-meter-v1",
                RevealPackageTestData.ProductionHash(clonePackage),
                60,
                [1, 3, 5]),
            clonePackage);
        RevealReceipt prepared = await reveals.PrepareAsync(scenarioId, revealRequest);
        await reveals.FinalizeAsync(
            scenarioId,
            new FinalizeRevealRequest(prepared.RevealId.ToString("D"), prepared.ManifestSha256));
        ScenarioService scenarios = CreateScenarioService(store, new RecordingPackageService());
        Scenario boundRevealed = await scenarios.GetAsync(scenarioId);
        var scorecards = new ScorecardService(store, time);
        await scorecards.PublishAsync(
            scenarioId,
            new ScorecardRequest(
                Guid.NewGuid().ToString("D"),
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
                        1,
                        "m",
                        0,
                        10,
                        null)
                ],
                revealTime,
                null));
        Scenario liveScored = await scenarios.GetAsync(scenarioId);

        ScenarioApiException unavailable = Assert.ThrowsAsync<ScenarioApiException>(() =>
            scenarios.GetCurrentScorecardAsync(boundRevealed))!;
        InvalidDataException futureScorecard = Assert.ThrowsAsync<InvalidDataException>(() =>
            scenarios.GetCurrentScorecardAsync(liveScored with { AsOfUtc = T0 }))!;
        InvalidDataException mismatchedReveal = Assert.ThrowsAsync<InvalidDataException>(() =>
            scenarios.GetCurrentScorecardAsync(liveScored with { ClonedFieldId = Guid.NewGuid() }))!;
        PublicScorecard visible = await scenarios.GetCurrentScorecardAsync(liveScored);

        Assert.Multiple(() =>
        {
            Assert.That(boundRevealed.Status, Is.EqualTo(ScenarioStatus.Revealed));
            Assert.That(liveScored.Status, Is.EqualTo(ScenarioStatus.Scored));
            Assert.That(unavailable.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(futureScorecard.Message, Does.Contain("bound scenario snapshot"));
            Assert.That(mismatchedReveal.Message, Does.Contain("bound scenario snapshot"));
            Assert.That(visible.ScenarioId, Is.EqualTo(scenarioId));
            Assert.That(visible.RevealId, Is.EqualTo(revealId));
            Assert.That(visible.CreatedValidTimeUtc, Is.LessThanOrEqualTo(liveScored.AsOfUtc));
        });
    }

    private string ConnectionString() => $"Data Source={_databasePath}";

    private static DefaultHttpContext ContextFor(Guid scenarioId, IServiceProvider services)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };
        context.Request.Headers[ScenarioAgentScope.HeaderName] = scenarioId.ToString("D");
        return context;
    }

    private static ScenarioService CreateScenarioService(
        SqliteScenarioStore store,
        IFieldPackageService packages) =>
        new(store, packages, new CanonicalJsonHasher(), new FixedTimeProvider(T0));

    private static Scenario CreateScenario(
        Guid scenarioId,
        Guid sourceFieldId,
        ScenarioStatus status,
        Guid? clonedFieldId = null) =>
        new(
            scenarioId,
            sourceFieldId,
            clonedFieldId,
            "Bound Reservoir",
            T0,
            T0,
            "agent-scope-test",
            ScenarioModelVersions.World,
            ScenarioModelVersions.Observation,
            ScenarioModelVersions.Scoring,
            status,
            new string('a', 64),
            T0,
            T0);

    private static AnalysisPackage CreatePackage(Guid fieldId)
    {
        JsonNode field = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = fieldId }
        };
        var counts = new SourceCounts(1, 0, 0, 0, 0, 0, 0);
        string[] gaps =
        [
            "No clusters were returned for the field.",
            "No trajectories were returned for the field."
        ];
        string hash = new CanonicalJsonHasher().Compute(
            fieldId,
            field,
            [],
            [],
            [],
            [],
            [],
            [],
            counts,
            gaps);
        return new AnalysisPackage(
            T0,
            fieldId,
            field,
            [],
            [],
            [],
            [],
            [],
            [],
            counts,
            gaps,
            hash);
    }

    private sealed class RecordingPackageService(params AnalysisPackage[] packages) : IFieldPackageService
    {
        private readonly IReadOnlyDictionary<Guid, AnalysisPackage> _packages =
            packages.ToDictionary(package => package.FieldId);

        internal List<Guid> RequestedFieldIds { get; } = [];

        public Task<JsonNode> GetFieldsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<JsonNode>(new JsonArray());

        public Task<AnalysisPackage> BuildPackageAsync(
            Guid fieldId,
            CancellationToken cancellationToken)
        {
            RequestedFieldIds.Add(fieldId);
            return Task.FromResult(_packages[fieldId]);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
