using System.Net;
using System.Net.Http.Json;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class HypothesisEndpointTests
{
    private const string Id = "11111111-1111-1111-1111-111111111111";

    [TestCase("POST", "/api/hypotheses")]
    [TestCase("POST", "/api/hypotheses/branches")]
    [TestCase("POST", $"/api/hypotheses/{Id}/revisions")]
    [TestCase("POST", $"/api/hypotheses/{Id}/revisions/1/challenges")]
    [TestCase("PUT", $"/api/hypotheses/{Id}/revisions/1/challenges/{Id}/dispositions")]
    public async Task EveryMutation_RequiresNativeAntiforgeryBeforeReadingTheBody(string method, string route)
    {
        await using var f = await HypothesisHttpFixture.StartAsync();
        using HttpResponseMessage response = await f.SendAsync(new(method), route, new { }, token: false);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Antiforgery token required"));
        Assert.That(await f.Data.Service.ListAsync(f.Data.LiveScope, null), Is.Empty);
    }

    [TestCase("http://example.invalid", null, HttpStatusCode.Forbidden)]
    [TestCase("", null, HttpStatusCode.Forbidden)]
    [TestCase(null, "forged", HttpStatusCode.BadRequest)]
    public async Task BrowserMutations_RejectForeignMissingOriginsAndForgedTokens(string? origin, string? token, HttpStatusCode expected)
    {
        await using var f = await HypothesisHttpFixture.StartAsync();
        using HttpResponseMessage response = await f.SendAsync(HttpMethod.Post, "/api/hypotheses", await f.Data.RequestAsync(),
            origin: origin, overrideToken: token);
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(await f.Data.Service.ListAsync(f.Data.LiveScope, null), Is.Empty);
    }

    [Test]
    public async Task DisabledOperator_RejectsSavesWithoutChangingExistingReadAccess()
    {
        await using var f = await HypothesisHttpFixture.StartAsync(configured: false);
        using HttpResponseMessage response = await f.SendAsync(HttpMethod.Post, "/api/hypotheses", await f.Data.RequestAsync(),
            token: false);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        using HttpResponseMessage list = await f.Client.GetAsync("/api/hypotheses" + f.ScopeQuery);
        Assert.That(list.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task HttpWorkflow_UsesExactEtagsScopedLocationsAndImmutableEarlierRevisions()
    {
        await using var f = await HypothesisHttpFixture.StartAsync();
        HypothesisCreateRequest request = await f.Data.RequestAsync();
        using HttpResponseMessage created = await f.SendAsync(HttpMethod.Post, "/api/hypotheses", request, key: "create");
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(created.Headers.ETag!.Tag, Is.EqualTo("\"1\""));
        HypothesisRevision first = (await created.Content.ReadFromJsonAsync<HypothesisRevision>())!;
        using HttpResponseMessage fetched = await f.Client.GetAsync(created.Headers.Location);
        Assert.That((await fetched.Content.ReadFromJsonAsync<HypothesisRevision>())!.SnapshotSha256, Is.EqualTo(first.SnapshotSha256));
        Assert.That(fetched.Headers.CacheControl!.NoStore, Is.True);
        var revisionRequest = new HypothesisReviseRequest(request.Scope, request.Input with { Rationale = "Reviewed second interpretation." });
        string append = $"/api/hypotheses/{first.HypothesisId:D}/revisions";
        using HttpResponseMessage missing = await f.SendAsync(HttpMethod.Post, append, revisionRequest, key: "missing");
        Assert.That((int)missing.StatusCode, Is.EqualTo(428));
        using HttpResponseMessage revised = await f.SendAsync(HttpMethod.Post, append, revisionRequest, match: "\"1\"", key: "append");
        Assert.That(revised.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(revised.Headers.ETag!.Tag, Is.EqualTo("\"2\""));
        using HttpResponseMessage stale = await f.SendAsync(HttpMethod.Post, append, revisionRequest, match: "\"1\"", key: "stale");
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        using HttpResponseMessage old = await f.Client.GetAsync(created.Headers.Location);
        Assert.That(old.Headers.ETag!.Tag, Is.EqualTo("\"1\""));
        Assert.That((await old.Content.ReadFromJsonAsync<HypothesisRevision>())!.SnapshotSha256, Is.EqualTo(first.SnapshotSha256));
        using HttpResponseMessage unscoped = await f.Client.GetAsync($"/api/hypotheses/{first.HypothesisId:D}");
        Assert.That(unscoped.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using HttpResponseMessage wrong = await f.Client.GetAsync($"/api/hypotheses/{first.HypothesisId:D}?fieldId={Guid.NewGuid():D}&reservoir=Target");
        Assert.That(wrong.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task HttpChallenges_RequireScopeCitationsAndVersionedHumanDispositions()
    {
        await using var f = await HypothesisHttpFixture.StartAsync();
        HypothesisRevision source = await f.Data.Service.CreateAsync(await f.Data.RequestAsync(), "create");
        string route = $"/api/hypotheses/{source.HypothesisId:D}/revisions/1/challenges";
        var request = new HypothesisChallengeRequest(source.Scope, source.Input.AnalysisSha256, "A manual challenge", "Reviewer",
            [new("Is distance support adequate?", [$"field:{source.Scope.FieldId:D}"])]);
        using HttpResponseMessage created = await f.SendAsync(HttpMethod.Post, route, request, key: "challenge");
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        HypothesisChallenge challenge = (await created.Content.ReadFromJsonAsync<HypothesisChallenge>())!;
        using HttpResponseMessage update = await f.SendAsync(HttpMethod.Put, $"{route}/{challenge.ChallengeId:D}/dispositions",
            new HypothesisDispositionRequest(source.Scope, "Reviewer", [new(challenge.Objections[0].ObjectionId, "deferred", "Acquire a nearby control.")]),
            match: "\"1\"", key: "disposition");
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(update.Headers.ETag!.Tag, Is.EqualTo("\"2\""));
        using HttpResponseMessage earlier = await f.Client.GetAsync($"{route}/{challenge.ChallengeId:D}{f.ScopeQuery}&version=1");
        Assert.That((await earlier.Content.ReadFromJsonAsync<HypothesisChallenge>())!.Objections[0].Disposition, Is.EqualTo("open"));
    }

    [Test]
    public async Task ComparisonAndSnapshotAnalysis_AreExplicitScopedReadOnlyComputations()
    {
        await using var f = await HypothesisHttpFixture.StartAsync();
        HypothesisRevision first = await f.Data.Service.CreateAsync(await f.Data.RequestAsync(), "first");
        HypothesisRevision second = await f.Data.Service.CreateAsync(await f.Data.RequestAsync(name: "Second"), "second");
        var reference = new HypothesisReference(first.HypothesisId, 1);
        using HttpResponseMessage analysis = await f.Client.PostAsJsonAsync(
            $"/api/hypotheses/{first.HypothesisId:D}/revisions/1/analysis",
            new HypothesisAnalysisRequest(first.Scope, AnalysisConfiguration.Default with { GridPointsPerAxis = 9 }));
        Assert.That(analysis.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await analysis.Content.ReadFromJsonAsync<AnalysisResult>())!.CandidateGrid, Has.Count.EqualTo(81));
        using HttpResponseMessage comparison = await f.Client.PostAsJsonAsync("/api/hypotheses/compare",
            new HypothesisCompareRequest([new(first.Scope, reference), new(second.Scope, new(second.HypothesisId, 1))]));
        Assert.That(comparison.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await comparison.Content.ReadFromJsonAsync<HypothesisComparison>())!.Entries, Has.Count.EqualTo(2));
        Assert.That(await f.Data.Service.ListAsync(first.Scope, first.HypothesisId), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task RequestShapeAndLength_AreBoundedAndCorruptionReturnsOnlySafeErrors()
    {
        await using var f = await HypothesisHttpFixture.StartAsync();
        HypothesisCreateRequest request = await f.Data.RequestAsync();
        using HttpResponseMessage extra = await f.SendAsync(HttpMethod.Post, "/api/hypotheses",
            new { request.Name, request.Scope, request.Input, stageAUrl = "not-a-supported-field" });
        Assert.That(extra.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using HttpResponseMessage huge = await f.SendAsync(HttpMethod.Post, "/api/hypotheses",
            request with { Name = new string('x', 513 * 1024) });
        Assert.That(huge.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using HttpResponseMessage noKey = await f.SendAsync(HttpMethod.Post, "/api/hypotheses", request, key: "");
        Assert.That(noKey.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        HypothesisRevision saved = await f.Data.Service.CreateAsync(request, "valid");
        await f.Data.ExecuteAsync("DROP TRIGGER hypothesis_revisions_no_update; UPDATE hypothesis_revisions SET content_json='broken-private-source';");
        using HttpResponseMessage corrupt = await f.Client.GetAsync($"/api/hypotheses/{saved.HypothesisId:D}{f.ScopeQuery}");
        Assert.That(corrupt.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        string body = await corrupt.Content.ReadAsStringAsync();
        Assert.That(body, Does.Not.Contain("broken-private-source").And.Not.Contain(f.Data.DatabasePath).And.Not.Contain("Sqlite"));
    }
}

internal sealed class HypothesisHttpFixture(WebApplication app, HttpClient client, HypothesisFixture data) : IAsyncDisposable
{
    public HttpClient Client { get; } = client;
    public HypothesisFixture Data { get; } = data;
    private OperatorSession? _session;
    public string ScopeQuery => $"?fieldId={Data.LiveScope.FieldId:D}&reservoir=Target";

    public static async Task<HypothesisHttpFixture> StartAsync(bool configured = true)
    {
        var data = new HypothesisFixture();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development",
            ContentRootPath = Directory.GetCurrentDirectory()
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Sqlite"] = data.ConnectionString,
            ["LocalOperator:BackendUrl"] = configured ? "http://127.0.0.1:43001" : null,
            ["LocalOperator:InternalKey"] = configured ? "test-only-key" : null,
            ["LocalOperator:BrowserOrigin"] = "http://localhost:5173"
        });
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton(data.Scenarios);
        builder.Services.AddSingleton(new SqliteScenarioStore(data.ConnectionString));
        builder.Services.AddSingleton<IPetrophysicsAnalysisService>(data.Analysis);
        builder.Services.AddSingleton<PredictionLedgerService>();
        builder.Services.AddLocalOperatorWorkflow(builder.Configuration);
        builder.Services.AddHypotheses(builder.Configuration);
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        app.MapLocalOperatorWorkflow();
        app.MapHypotheses();
        await app.StartAsync();
        var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), UseProxy = false })
        {
            BaseAddress = new Uri(app.Urls.Single()),
            Timeout = TimeSpan.FromSeconds(30)
        };
        return new(app, client, data);
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string path, object body, string key = "request", string? match = null,
        bool token = true, string? origin = null, string? overrideToken = null)
    {
        using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
        if (origin != "") request.Headers.Add("Origin", origin ?? Client.BaseAddress!.GetLeftPart(UriPartial.Authority));
        if (key.Length > 0) request.Headers.Add("Idempotency-Key", key);
        if (match is not null) request.Headers.TryAddWithoutValidation("If-Match", match);
        if (token)
        {
            _session ??= (await Client.GetFromJsonAsync<OperatorSession>("/api/operator/session"))!;
            request.Headers.Add(_session.CsrfHeaderName, overrideToken ?? _session.CsrfRequestToken);
        }
        return await Client.SendAsync(request);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.StopAsync();
        await app.DisposeAsync();
        Data.Dispose();
    }
}
