using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class OperatorWorkflowTests
{
    private const string Secret = "operator-test-secret-never-public";
    private const string Hidden = "hidden-world-never-public";
    private static readonly Guid ScenarioId = Guid.Parse("33000000-0000-0000-0000-000000000001");
    private static readonly Guid RunId = Guid.Parse("33000000-0000-0000-0000-000000000002");
    private static readonly string SealHash = new('a', 64);
    private static readonly string PackageHash = new('b', 64);
    private static string ScenarioRoute => $"/api/operator/scenarios/{ScenarioId:D}";
    private static string RunRoute => $"{ScenarioRoute}/runs/{RunId:D}";
    private static string SealRoute => $"/api/scenarios/{ScenarioId:D}/prediction/seal";

    [Test]
    public async Task AttachedSealGuard_BlocksCrossSiteFormPostWithoutCallingExistingHandler()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, SealRoute)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };
        request.Headers.Add("Origin", "https://hostile.example");
        using HttpResponseMessage response = await fixture.Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(fixture.Ledger.SealCalls, Is.Zero);
    }

    [TestCase("missing-token", HttpStatusCode.BadRequest)]
    [TestCase("forged-token", HttpStatusCode.BadRequest)]
    [TestCase("missing-origin", HttpStatusCode.Forbidden)]
    [TestCase("missing-key", HttpStatusCode.BadRequest)]
    [TestCase("foreign-loopback-origin", HttpStatusCode.Forbidden)]
    public async Task AttachedSealGuard_RequiresNativeTokenAllowedOriginAndStableKey(string invalid, HttpStatusCode expected)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync(SealRoute, new { },
            token: invalid != "missing-token",
            overrideToken: invalid == "forged-token" ? "forged" : null,
            origin: invalid == "missing-origin" ? "" : invalid == "foreign-loopback-origin" ? "http://localhost:5174" : null,
            key: invalid == "missing-key" ? "" : "seal-attempt");
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(fixture.Ledger.SealCalls, Is.Zero);
    }

    [Test]
    public async Task AttachedSealGuard_PreservesBodylessHandlerIfMatchAndEtag()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        OperatorSession session = await fixture.SessionAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, SealRoute);
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add(session.CsrfHeaderName, session.CsrfRequestToken);
        request.Headers.Add("Idempotency-Key", "explicit-seal-attempt");
        request.Headers.Add("If-Match", "\"7\"");
        using HttpResponseMessage response = await fixture.Client.SendAsync(request);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Headers.ETag?.Tag, Is.EqualTo("\"7\""));
            Assert.That(fixture.Ledger.SealCalls, Is.EqualTo(1));
            Assert.That(fixture.Ledger.LastSealRevision, Is.EqualTo(7));
            Assert.That(fixture.Backend.Requests, Is.Empty);
        });
    }

    [Test]
    public async Task AttachedSealGuard_PreservesExistingScenarioConflictResponse()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Ledger.SealError = new ScenarioApiException(
            409, "Configured prediction sealing unavailable", "The existing sealing service rejected this configured draft.");
        using HttpResponseMessage response = await fixture.PostAsync(SealRoute, new { });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Configured prediction sealing unavailable"));
        Assert.That(fixture.Ledger.SealCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task AttachedSealGuard_DisabledConfigurationCannotExecuteHandler()
    {
        await using Fixture fixture = await Fixture.StartAsync(configured: false);
        using HttpResponseMessage response = await fixture.PostAsync(SealRoute, new { }, token: false);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(fixture.Ledger.SealCalls, Is.Zero);
    }

    [Test]
    public async Task DisabledConfiguration_IsExplicit_AndDoesNotContactBackend()
    {
        await using Fixture fixture = await Fixture.StartAsync(configured: false);
        using HttpResponseMessage response = await fixture.Client.GetAsync("/api/operator/session");
        OperatorSession session = (await response.Content.ReadFromJsonAsync<OperatorSession>())!;
        using HttpResponseMessage view = await fixture.Client.GetAsync(ScenarioRoute);
        using HttpResponseMessage action = await fixture.PostAsync($"{ScenarioRoute}/runs", new { actor = "Local owner" }, token: false);
        Assert.Multiple(() =>
        {
            Assert.That(session.Enabled, Is.False);
            Assert.That(session.Reason, Does.Contain("not configured"));
            Assert.That(session.CsrfRequestToken, Is.Null);
            Assert.That(view.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(action.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(fixture.Backend.Requests, Is.Empty);
        });
    }

    [TestCase("https://external.example")]
    [TestCase("http://localhost:5001/internal")]
    [TestCase("http://user:password@localhost:5001")]
    public async Task UnsafeBackendConfiguration_FailsClosedWithoutStartupFailure(string backendUrl)
    {
        await using Fixture fixture = await Fixture.StartAsync(backendUrl: backendUrl);
        OperatorSession session = (await fixture.Client.GetFromJsonAsync<OperatorSession>("/api/operator/session"))!;
        Assert.That(session.Enabled, Is.False);
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task ProductionEnvironment_DisablesOperator()
    {
        await using Fixture fixture = await Fixture.StartAsync(environment: Environments.Production);
        OperatorSession session = (await fixture.Client.GetFromJsonAsync<OperatorSession>("/api/operator/session"))!;
        Assert.That(session.Enabled, Is.False);
        Assert.That(session.Reason, Does.Contain("Development"));
    }

    [Test]
    public async Task Session_IssuesNativeHttpOnlyStrictCookie_AndOnlyRequestToken()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.Client.GetAsync("/api/operator/session");
        string json = await response.Content.ReadAsStringAsync();
        OperatorSession session = JsonSerializer.Deserialize<OperatorSession>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        string cookie = response.Headers.GetValues("Set-Cookie").Single();
        Assert.Multiple(() =>
        {
            Assert.That(session.Enabled, Is.True);
            Assert.That(session.CsrfHeaderName, Is.EqualTo("X-DrillSim-CSRF"));
            Assert.That(session.CsrfRequestToken, Is.Not.Null.And.Not.Empty);
            Assert.That(session.AuditLabelLimitation, Does.Contain("not an authenticated identity"));
            Assert.That(cookie, Does.Contain("httponly").IgnoreCase);
            Assert.That(cookie, Does.Contain("samesite=strict").IgnoreCase);
            Assert.That(json, Does.Not.Contain(Secret));
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
        });
    }

    [Test]
    public async Task MutationWithoutToken_IsRejectedByActualEndpointFilter()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync($"{ScenarioRoute}/runs", new { actor = "Local owner" }, token: false);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task ForgedTokenWithRealCookie_IsRejectedByNativeAntiforgery()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        await fixture.SessionAsync();
        using HttpResponseMessage response = await fixture.PostAsync(
            $"{ScenarioRoute}/runs", new { actor = "Local owner" }, overrideToken: "not-a-native-token");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Antiforgery validation failed"));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task RealRequestTokenWithoutItsCookie_IsRejected()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        OperatorSession session = await fixture.SessionAsync();
        using var client = new HttpClient(new HttpClientHandler { UseCookies = false }) { BaseAddress = fixture.Client.BaseAddress };
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{ScenarioRoute}/runs");
        request.Headers.Add("Origin", fixture.Client.BaseAddress!.GetLeftPart(UriPartial.Authority));
        request.Headers.Add("Idempotency-Key", "missing-cookie");
        request.Headers.Add(session.CsrfHeaderName, session.CsrfRequestToken);
        request.Content = JsonContent.Create(new { actor = "Local owner" });
        using HttpResponseMessage response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [TestCase("https://hostile.example")]
    [TestCase("http://localhost:5174")]
    [TestCase("null")]
    [TestCase("")]
    public async Task MissingOrUnapprovedOrigin_IsRejectedEvenWithValidCsrf(string origin)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync(
            $"{ScenarioRoute}/runs", new { actor = "Local owner" }, origin: origin);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task ConfiguredViteOrigin_WorksDespiteRewrittenHost_AndStartFieldsAreServerDerived()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync(
            $"{ScenarioRoute}/runs", new { actor = "Local owner" }, origin: "http://localhost:5173", key: "stable-start-attempt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        RequestCapture start = fixture.Backend.Requests.Single(x => x.Method == HttpMethod.Post);
        using JsonDocument body = JsonDocument.Parse(start.Body!);
        Assert.Multiple(() =>
        {
            Assert.That(start.Path, Is.EqualTo("/drillingoperations/api/runs"));
            Assert.That(start.Key, Is.EqualTo("stable-start-attempt"));
            Assert.That(start.InternalKey, Is.EqualTo(Secret));
            Assert.That(body.RootElement.GetProperty("scenarioId").GetGuid(), Is.EqualTo(ScenarioId));
            Assert.That(body.RootElement.GetProperty("approvedSealedPredictionHash").GetString(), Is.EqualTo(SealHash));
            Assert.That(body.RootElement.GetProperty("planArtifactId").GetString(), Does.StartWith("approved-plan:"));
            Assert.That(body.RootElement.GetProperty("planArtifactSha256").GetString(), Does.Match("^[0-9a-f]{64}$"));
            Assert.That(body.RootElement.TryGetProperty("worldId", out _), Is.False);
        });
        var realRequest = JsonSerializer.Deserialize<DrillingOperations.CreateRunRequest>(
            start.Body!, DrillingOperations.CanonicalJson.SerializerOptions)!;
        Assert.That(DrillingOperations.RequestValidation.Validate(realRequest), Is.Empty);
    }

    [Test]
    public async Task NonLoopbackHost_IsDeniedOnActualLocalSocket()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/operator/session");
        request.Headers.Host = "rebound.example";
        using HttpResponseMessage response = await fixture.Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [TestCase("192.0.2.5")]
    [TestCase("::ffff:192.0.2.5")]
    public void NonLoopbackRemoteAddress_IsDenied(string remote)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(remote);
        context.Request.Host = new HostString("localhost");
        context.Request.Scheme = "http";
        Assert.That(OperatorWorkflowExtensions.IsLocalRequest(context, new(true, null, null, null, null), false), Is.False);
    }

    [Test]
    public async Task CrossSiteFetchMetadata_IsRejected()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/operator/session");
        request.Headers.Add("Sec-Fetch-Site", "cross-site");
        using HttpResponseMessage response = await fixture.Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task ScenarioGet_ProjectsOnlySafeFields_WithNoActions()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = true;
        fixture.Backend.Status = "AwaitingApproval";
        fixture.Backend.Stage = "S6DesignCompletion";
        using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
        string json = await response.Content.ReadAsStringAsync();
        OperatorScenarioView view = JsonSerializer.Deserialize<OperatorScenarioView>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(view.Run!.RunId, Is.EqualTo(RunId));
            Assert.That(view.Run.Stages.Count, Is.EqualTo(10));
            Assert.That(view.Run.ProgressPercent, Is.EqualTo(60));
            Assert.That(view.Run.CurrentStage, Is.EqualTo("S6DesignCompletion"));
            Assert.That(view.Completion.Available, Is.False);
            Assert.That(view.Completion.ApprovalEnabled, Is.False);
            Assert.That(view.Actions.ApproveCompletion.Enabled, Is.False);
            Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
            Assert.That(fixture.Ledger.ApprovalCount, Is.Zero);
        });
        AssertSafeJson(json);
    }

    [Test]
    public async Task CrossScenarioAuditRun_IsRejectedBeforeReadingOtherRun()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = true;
        fixture.Backend.AuditScenario = Guid.NewGuid();
        using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(fixture.Backend.Requests.Any(x => x.Path.EndsWith($"/runs/{RunId:D}", StringComparison.Ordinal)), Is.False);
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [TestCase("cancel")]
    [TestCase("resume")]
    [TestCase("publish")]
    [TestCase("score")]
    public async Task CrossScenarioRunAction_IsRejectedWithoutMutation(string action)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.RunScenario = Guid.NewGuid();
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/{action}", new { actor = "Local owner" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task RunIdentityMismatchInScenarioView_IsRejected()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = true;
        fixture.Backend.RunScenario = Guid.NewGuid();
        using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase("worldId")]
    [TestCase("bindingId")]
    [TestCase("truthSamples")]
    [TestCase("planArtifactSha256")]
    public async Task BrowserCannotSupplyInternalStartFields(string forbiddenField)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync(
            $"{ScenarioRoute}/runs", new Dictionary<string, object> { ["actor"] = "Local owner", [forbiddenField] = Hidden });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [TestCase("")]
    [TestCase("bad\nactor")]
    public async Task InvalidActor_IsRejectedWithoutMutation(string actor)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync($"{ScenarioRoute}/runs", new { actor });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task MissingIdempotencyKey_IsRejectedWithoutMutation()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using HttpResponseMessage response = await fixture.PostAsync($"{ScenarioRoute}/runs", new { actor = "Local owner" }, key: "");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task PredictionApproval_RequiresExistingSeal_AndNeverAutoSeals()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Ledger.Prediction = fixture.Ledger.Prediction! with { Seal = null, Approval = null };
        using HttpResponseMessage response = await fixture.PostAsync(
            $"{ScenarioRoute}/approve-prediction", new { actor = "Local owner", reviewedSealHash = SealHash });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(fixture.Ledger.ApprovalCount, Is.Zero);
        Assert.That(fixture.Ledger.Prediction.Seal, Is.Null);
    }

    [Test]
    public async Task PredictionApproval_RequiresExactReviewedSeal_AndCallsLedgerOnlyWhenMatched()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Ledger.Scenario = fixture.Ledger.Scenario with { Status = ScenarioStatus.PredictionSealed };
        fixture.Ledger.Prediction = fixture.Ledger.Prediction! with { Approval = null };
        using HttpResponseMessage stale = await fixture.PostAsync(
            $"{ScenarioRoute}/approve-prediction", new { actor = "Local owner", reviewedSealHash = new string('c', 64) });
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(fixture.Ledger.ApprovalCount, Is.Zero);
        using HttpResponseMessage approved = await fixture.PostAsync(
            $"{ScenarioRoute}/approve-prediction", new { actor = " Local owner ", reviewedSealHash = SealHash });
        Assert.That(approved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(fixture.Ledger.ApprovalCount, Is.EqualTo(1));
        Assert.That(fixture.Ledger.LastActor, Is.EqualTo("Local owner"));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [TestCase("no-seal")]
    [TestCase("no-approval")]
    [TestCase("wrong-approval")]
    [TestCase("wrong-status")]
    [TestCase("no-binding")]
    [TestCase("wrong-binding")]
    public async Task Start_EnforcesApprovalAndAuthoritativeBinding(string missing)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        if (missing == "no-seal") fixture.Ledger.Prediction = fixture.Ledger.Prediction! with { Seal = null };
        if (missing == "no-approval") fixture.Ledger.Prediction = fixture.Ledger.Prediction! with { Approval = null };
        if (missing == "wrong-approval") fixture.Ledger.Prediction = fixture.Ledger.Prediction! with
        { Approval = new("Local owner", DateTimeOffset.UtcNow, new string('c', 64)) };
        if (missing == "wrong-status") fixture.Ledger.Scenario = fixture.Ledger.Scenario with { Status = ScenarioStatus.PredictionSealed };
        if (missing == "no-binding") fixture.Backend.HasBinding = false;
        if (missing == "wrong-binding") fixture.Backend.BindingSeal = new string('c', 64);
        using HttpResponseMessage response = await fixture.PostAsync($"{ScenarioRoute}/runs", new { actor = "Local owner" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
        if (missing == "no-binding")
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("prepare the simulator"));
    }

    [Test]
    public async Task CompletionApproval_IsDisabledWithoutSafeReviewedOpeningContract()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.Status = "AwaitingApproval";
        fixture.Backend.Stage = "S6DesignCompletion";
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/approve-completion",
            new { actor = "Local owner", reviewedOpeningHash = SealHash });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("No verified completion review"));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [Test]
    public async Task CompletionReview_IsSafeAndEnabledOnlyForVerifiedOwnedS6Pause()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = fixture.Backend.HasCompletion = true;
        fixture.Backend.Status = "AwaitingApproval";
        fixture.Backend.Stage = "S6DesignCompletion";
        using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
        string json = await response.Content.ReadAsStringAsync();
        OperatorScenarioView view = (await response.Content.ReadFromJsonAsync<OperatorScenarioView>())!;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(view.Completion.Available, Is.True);
            Assert.That(view.Completion.ApprovalEnabled, Is.True);
            Assert.That(view.Actions.ApproveCompletion.Enabled, Is.True);
            Assert.That(view.Completion.OpeningsHash, Is.EqualTo(SealHash));
            Assert.That(view.Completion.RunId, Is.EqualTo(RunId));
            Assert.That(view.Completion.ScenarioId, Is.EqualTo(ScenarioId));
            Assert.That(view.Completion.Status, Is.EqualTo("Draft"));
            Assert.That(view.Completion.Openings.Single().ReservoirName, Is.EqualTo("Reservoir"));
            Assert.That(view.Completion.Openings.Single().Skin, Is.EqualTo(2));
            Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
        });
        AssertSafeJson(json);
    }

    [TestCase("Queued", "S5GenerateLogs")]
    [TestCase("AwaitingDependency", "S6DesignCompletion")]
    [TestCase("Cancelled", "S6DesignCompletion")]
    public async Task CompletionReview_OtherRunStatesCannotApprove(string status, string stage)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = fixture.Backend.HasCompletion = true;
        fixture.Backend.Status = status;
        fixture.Backend.Stage = stage;
        OperatorScenarioView view = (await fixture.Client.GetFromJsonAsync<OperatorScenarioView>(ScenarioRoute))!;
        Assert.That(view.Completion.ApprovalEnabled, Is.False);
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/approve-completion",
            new { actor = "Local owner", reviewedOpeningHash = SealHash });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [Test]
    public async Task CompletionApproval_AlwaysForwardsReviewedHashAndActor_AndRejectsStaleReview()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasCompletion = true;
        fixture.Backend.Status = "AwaitingApproval";
        fixture.Backend.Stage = "S6DesignCompletion";
        using HttpResponseMessage stale = await fixture.PostAsync($"{RunRoute}/approve-completion",
            new { actor = "Local owner", reviewedOpeningHash = PackageHash });
        Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
        using HttpResponseMessage missing = await fixture.PostAsync($"{RunRoute}/approve-completion", new { actor = "Local owner" });
        Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using HttpResponseMessage approved = await fixture.PostAsync($"{RunRoute}/approve-completion",
            new { actor = " Local owner ", reviewedOpeningHash = SealHash }, key: "completion-reviewed-attempt");
        Assert.That(approved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        RequestCapture mutation = fixture.Backend.Requests.Single(x => x.Method == HttpMethod.Post);
        Assert.Multiple(() =>
        {
            Assert.That(mutation.Path, Is.EqualTo($"/drillingoperations/api/runs/{RunId:D}/completion/approve"));
            Assert.That(mutation.ReviewedHash, Is.EqualTo(SealHash));
            Assert.That(mutation.Actor, Is.EqualTo("Local owner"));
            Assert.That(mutation.Key, Is.EqualTo("completion-reviewed-attempt"));
        });
        AssertSafeJson(await approved.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task CompletionReview_CrossScenarioRejectedForReadAndApproval()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = fixture.Backend.HasCompletion = true;
        fixture.Backend.CompletionScenario = Guid.NewGuid();
        fixture.Backend.Status = "AwaitingApproval";
        fixture.Backend.Stage = "S6DesignCompletion";
        using HttpResponseMessage read = await fixture.Client.GetAsync(ScenarioRoute);
        Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        using HttpResponseMessage approval = await fixture.PostAsync($"{RunRoute}/approve-completion",
            new { actor = "Local owner", reviewedOpeningHash = SealHash });
        Assert.That(approval.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [Test]
    public async Task CompletionApproval_BackendAtomicMismatchIsNotReportedAsApproved()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasCompletion = true;
        fixture.Backend.Status = "AwaitingApproval";
        fixture.Backend.Stage = "S6DesignCompletion";
        fixture.Backend.CompletionApprovalResponse = HttpStatusCode.Conflict;
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/approve-completion",
            new { actor = "Local owner", reviewedOpeningHash = SealHash });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task UnexpectedFailure_LogsOnlyBoundedErrorTypeAndEvent_NotExceptionContent()
    {
        var logs = new CapturingLogger();
        await using Fixture fixture = await Fixture.StartAsync(logs: logs);
        fixture.Backend.ThrowUnexpected = true;
        using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        var failure = logs.Entries.Single(x => x.Event.Id == 8100);
        Assert.Multiple(() =>
        {
            Assert.That(failure.Event.Name, Is.EqualTo("LocalOperatorUnhandledFailure"));
            Assert.That(failure.Message, Does.Contain("InvalidOperationException"));
            Assert.That(failure.Message.Length, Is.LessThan(180));
            Assert.That(failure.Exception, Is.Null);
        });
        AssertSafeJson(failure.Message);
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task UnboundScenarioReadiness_ReportsOperatorSetup_NotWorldGeneration()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasBinding = false;
        OperatorScenarioView view = (await fixture.Client.GetFromJsonAsync<OperatorScenarioView>(ScenarioRoute))!;
        Assert.Multiple(() =>
        {
            Assert.That(view.Preflight.WorldBound, Is.False);
            Assert.That(view.Preflight.Ready, Is.False);
            Assert.That(view.Preflight.Reason, Does.Contain("prepare the simulator"));
            Assert.That(view.Actions.Start.Enabled, Is.False);
            Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
        });
    }

    [TestCase("cancel", "AwaitingApproval", "S6DesignCompletion")]
    [TestCase("resume", "AwaitingDependency", "S7RunProduction")]
    public async Task PermittedRunAction_ForwardsStableKeyToFixedOwnedRoute(string action, string status, string stage)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.Status = status;
        fixture.Backend.Stage = stage;
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/{action}", new { actor = "Local owner" }, key: "stable-action");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        RequestCapture mutation = fixture.Backend.Requests.Single(x => x.Method == HttpMethod.Post);
        Assert.That(mutation.Path, Is.EqualTo($"/drillingoperations/api/runs/{RunId:D}/{action}"));
        Assert.That(mutation.Key, Is.EqualTo("stable-action"));
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [TestCase("cancel")]
    [TestCase("resume")]
    public async Task ScoringCheckpoint_CannotBeCancelledOrResumedThroughWrongCommand(string action)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.Status = "AwaitingDependency";
        fixture.Backend.Stage = "S9Score";
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/{action}", new { actor = "Local owner" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [TestCase(HttpStatusCode.ServiceUnavailable, "pending")]
    [TestCase(HttpStatusCode.Conflict, "failed")]
    public async Task PublishSuccess_ScoringFailure_NeverImpliesRevealRollback(HttpStatusCode scoringFailure, string scoringStatus)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.Status = "ReadyToReveal";
        fixture.Backend.Stage = "S7RunProduction";
        fixture.Backend.ScoreResponse = scoringFailure;
        using HttpResponseMessage response = await fixture.PostAsync($"{RunRoute}/publish", new { actor = "Local owner" }, key: "publish-stable");
        OperatorActionResult result = (await response.Content.ReadFromJsonAsync<OperatorActionResult>())!;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.RevealSucceeded, Is.True);
            Assert.That(result.ScoringStatus, Is.EqualTo(scoringStatus));
            Assert.That(result.Reason, Does.Contain("remains published"));
            Assert.That(fixture.Backend.Requests.Where(x => x.Method == HttpMethod.Post).Select(x => x.Path),
                Is.EqualTo(new[] { $"/drillingoperations/api/runs/{RunId:D}/publish", $"/drillingoperations/api/runs/{RunId:D}/score" }));
            Assert.That(fixture.Backend.Requests.Last().Key, Does.StartWith("operator-score:"));
        });
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task PublishFailure_DoesNotStartScoring_AndExplicitScoreRetryUsesProvidedKey()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.Status = "ReadyToReveal";
        fixture.Backend.PublishResponse = HttpStatusCode.Conflict;
        using HttpResponseMessage failed = await fixture.PostAsync($"{RunRoute}/publish", new { actor = "Local owner" });
        OperatorActionResult result = (await failed.Content.ReadFromJsonAsync<OperatorActionResult>())!;
        Assert.That(result.RevealSucceeded, Is.False);
        Assert.That(result.Outcome, Is.EqualTo("publication-failed"));
        Assert.That(fixture.Backend.Requests.Any(x => x.Path.EndsWith("/score", StringComparison.Ordinal)), Is.False);
        fixture.Backend.Status = "Revealed";
        using HttpResponseMessage retry = await fixture.PostAsync($"{RunRoute}/score", new { actor = "Local owner" }, key: "score-retry-new-attempt");
        OperatorActionResult scored = (await retry.Content.ReadFromJsonAsync<OperatorActionResult>())!;
        Assert.That(scored.ScoringStatus, Is.EqualTo("scored"));
        Assert.That(scored.RevealSucceeded, Is.True);
        Assert.That(fixture.Backend.Requests.Last().Key, Is.EqualTo("score-retry-new-attempt"));
    }

    [Test]
    public async Task GetDuringReadyToReveal_NeverPublishesOrScores()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = true;
        fixture.Backend.Status = "ReadyToReveal";
        fixture.Backend.Stage = "S7RunProduction";
        for (int i = 0; i < 2; i++)
        {
            using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [Test]
    public async Task UpstreamErrors_AreSanitized_NotRawPassthrough()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.FailReads = true;
        using HttpResponseMessage response = await fixture.Client.GetAsync(ScenarioRoute);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    private static void AssertSafeJson(string json)
    {
        foreach (string forbidden in new[]
        {
            Secret, Hidden, "bindingMetadata", "worldId", "pathBindingId", "completionBindingId",
            "truthSamples", "truthArray", "internalKey", "publicationKey", "stageAUrl", "canonicalInputJson"
        })
            Assert.That(json, Does.Not.Contain(forbidden).IgnoreCase);
    }

    [Test]
    public async Task PublicationRecoveryReview_IsProjectedReadOnly_AndEnablesOnlyRecovery()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = true;
        fixture.Backend.Status = "Failed";
        fixture.Backend.Stage = "S8PublishReveal";
        for (int i = 0; i < 2; i++)
        {
            using var response = await fixture.Client.GetAsync($"{RunRoute}/publication-recovery");
            string json = await response.Content.ReadAsStringAsync();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), json);
            Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
            var review = JsonSerializer.Deserialize<OperatorPublicationRecoveryReview>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            Assert.That(review.RecoveryEnabled, Is.True);
            Assert.That(review.ReviewedPublicationHash, Is.EqualTo(SealHash));
            Assert.That(review.OperationCount, Is.EqualTo(223));
            Assert.That(review.VerifiedOperationCount, Is.EqualTo(9));
            AssertSafeJson(json);
            Assert.That(json, Does.Not.Contain("clonedFieldId").And.Not.Contain("canonicalPayloadJson"));
        }
        OperatorScenarioView view = (await fixture.Client.GetFromJsonAsync<OperatorScenarioView>(ScenarioRoute))!;
        Assert.That(view.PublicationRecovery!.RecoveryEnabled, Is.True);
        Assert.That(view.Actions.RecoverPublication.Enabled, Is.True);
        Assert.That(view.Actions.Publish.Enabled, Is.False);
        Assert.That(view.Actions.Resume.Enabled, Is.False);
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [TestCase("missing-token", HttpStatusCode.BadRequest)]
    [TestCase("forged-token", HttpStatusCode.BadRequest)]
    [TestCase("missing-origin", HttpStatusCode.Forbidden)]
    [TestCase("foreign-origin", HttpStatusCode.Forbidden)]
    [TestCase("missing-key", HttpStatusCode.BadRequest)]
    public async Task PublicationRecoveryAction_UsesActualLocalAntiforgeryGuard(string invalid, HttpStatusCode expected)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using var response = await fixture.PostAsync($"{RunRoute}/recover-publication",
            new { actor = "Local owner", reason = "Write adapter corrected", reviewedPublicationHash = SealHash },
            token: invalid != "missing-token", overrideToken: invalid == "forged-token" ? "forged" : null,
            origin: invalid == "missing-origin" ? "" : invalid == "foreign-origin" ? "http://localhost:5174" : null,
            key: invalid == "missing-key" ? "" : "recovery");
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task PublicationRecoveryAction_RequiresCookieEvenWithNativeRequestToken()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        OperatorSession session = await fixture.SessionAsync();
        using var client = new HttpClient(new HttpClientHandler { UseCookies = false }) { BaseAddress = fixture.Client.BaseAddress };
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{RunRoute}/recover-publication")
        {
            Content = JsonContent.Create(new { actor = "Owner", reason = "Adapter corrected", reviewedPublicationHash = SealHash })
        };
        request.Headers.Add("Origin", fixture.Client.BaseAddress!.GetLeftPart(UriPartial.Authority));
        request.Headers.Add("Idempotency-Key", "no-cookie");
        request.Headers.Add(session.CsrfHeaderName, session.CsrfRequestToken);
        using var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task PublicationRecoveryAction_ForwardsExactReviewedBodyAndKey_WithoutPublishingOrBlockingReplay()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        var body = new { actor = "Local owner", reason = "Write adapter corrected", reviewedPublicationHash = SealHash };
        fixture.Backend.Status = "Failed";
        fixture.Backend.Stage = "S8PublishReveal";
        using var first = await fixture.PostAsync($"{RunRoute}/recover-publication", body, key: "reviewed-recovery");
        string firstJson = await first.Content.ReadAsStringAsync();
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK), firstJson);
        fixture.Backend.Status = "Revealed";
        using var replay = await fixture.PostAsync($"{RunRoute}/recover-publication", body, key: "reviewed-recovery");
        Assert.That(await replay.Content.ReadAsStringAsync(), Is.EqualTo(firstJson));
        RequestCapture[] mutations = fixture.Backend.Requests.Where(x => x.Method == HttpMethod.Post).ToArray();
        Assert.That(mutations.Length, Is.EqualTo(2));
        foreach (var mutation in mutations)
        {
            Assert.That(mutation.Path, Is.EqualTo($"/drillingoperations/api/scenarios/{ScenarioId:D}/runs/{RunId:D}/recover-publication"));
            Assert.That(mutation.Key, Is.EqualTo("reviewed-recovery"));
            Assert.That(mutation.InternalKey, Is.EqualTo(Secret));
            using var json = JsonDocument.Parse(mutation.Body!);
            Assert.That(json.RootElement.GetProperty("actor").GetString(), Is.EqualTo(body.actor));
            Assert.That(json.RootElement.GetProperty("reason").GetString(), Is.EqualTo(body.reason));
            Assert.That(json.RootElement.GetProperty("reviewedPublicationHash").GetString(), Is.EqualTo(SealHash));
        }
        var result = JsonSerializer.Deserialize<OperatorPublicationRecoveryResult>(firstJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.That(result.Status, Is.EqualTo("PublishFailed"));
        Assert.That(result.PreviousStatus, Is.EqualTo("Failed"));
        Assert.That(result.Outcome, Is.EqualTo("publication-recovered"));
        AssertSafeJson(firstJson);
    }

    [TestCase("actor")]
    [TestCase("reason")]
    [TestCase("blank")]
    [TestCase("hash")]
    [TestCase("unknown-field")]
    [TestCase("oversized")]
    public async Task PublicationRecoveryAction_RejectsInvalidBodiesBeforeBackend(string invalid)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        object body = invalid == "unknown-field"
            ? new { actor = "Owner", reason = "Verified", reviewedPublicationHash = SealHash, planId = "client-supplied" }
            : new { actor = invalid == "actor" ? new string('a', 101) : "Owner",
                reason = invalid == "blank" ? " " : invalid == "reason" ? new string('a', 501) : invalid == "oversized" ? new string('a', 5000) : "Verified",
                reviewedPublicationHash = invalid == "hash" ? "stale" : SealHash };
        using var response = await fixture.PostAsync($"{RunRoute}/recover-publication", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task PublicationRecoveryRoutes_RejectForeignOwnershipAndUnsafeBackendProjection()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.RunScenario = Guid.NewGuid();
        using var foreign = await fixture.Client.GetAsync($"{RunRoute}/publication-recovery");
        using var foreignPost = await fixture.PostAsync($"{RunRoute}/recover-publication",
            new { actor = "Owner", reason = "Verified", reviewedPublicationHash = SealHash });
        Assert.That(foreign.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(foreignPost.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(fixture.Backend.Requests.Any(x => x.Method == HttpMethod.Post), Is.False);
        fixture.Backend.RunScenario = ScenarioId;
        fixture.Backend.RecoveryHash = Hidden;
        using var invalid = await fixture.Client.GetAsync($"{RunRoute}/publication-recovery");
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        AssertSafeJson(await invalid.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task PublicationRecoveryDisabledAndUpstreamConflicts_AreSafeAndNeverPublish()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.RecoveryEnabled = false;
        using var response = await fixture.Client.GetAsync($"{RunRoute}/publication-recovery");
        var review = (await response.Content.ReadFromJsonAsync<OperatorPublicationRecoveryReview>())!;
        Assert.That(review.RecoveryEnabled, Is.False);
        Assert.That(review.ReviewedPublicationHash, Is.Null);
        AssertSafeJson(await response.Content.ReadAsStringAsync());
        fixture.Backend.RecoveryResponse = HttpStatusCode.Conflict;
        using var conflict = await fixture.PostAsync($"{RunRoute}/recover-publication",
            new { actor = "Owner", reason = "Verified", reviewedPublicationHash = SealHash });
        Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        AssertSafeJson(await conflict.Content.ReadAsStringAsync());
        Assert.That(fixture.Backend.Requests.Any(x => x.Path.EndsWith("/publish", StringComparison.Ordinal)), Is.False);
    }

    [Test]
    public async Task MissingVerifiedPublicationRecord_IsProjectedAsExplicitBlockedReviewWithoutInternalDetails()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.RecoveryEnabled = false;
        fixture.Backend.RecoveryReason = "PublicationRecoveryVerifiedRecordMissing";
        using var response = await fixture.Client.GetAsync($"{RunRoute}/publication-recovery");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var review = (await response.Content.ReadFromJsonAsync<OperatorPublicationRecoveryReview>())!;
        Assert.That(review.RecoveryEnabled, Is.False);
        Assert.That(review.ReviewedPublicationHash, Is.Null);
        Assert.That(review.Reason, Does.Contain("previously verified staged record is missing").And.Contain("Recovery is blocked"));
        AssertSafeJson(await response.Content.ReadAsStringAsync());
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [Test]
    public async Task ScoreCorrectionReview_IsReadOnlySanitizedAndBoundToTheOperatorView()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasRun = true;
        fixture.Backend.Status = "Failed";
        fixture.Backend.Stage = "S9Score";
        fixture.Backend.ScoreCorrectionEnabled = true;
        using var response = await fixture.Client.GetAsync($"{RunRoute}/scoring-correction");
        string json = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), json);
        Assert.That(response.Headers.CacheControl?.NoStore, Is.True);
        var review = JsonSerializer.Deserialize<OperatorScoringCorrectionReview>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.That(review.CorrectionEnabled, Is.True);
        Assert.That(review.CorrectionVersion, Is.EqualTo(ScoringCorrectionVersions.Correction));
        Assert.That(review.InvalidMetricCount, Is.EqualTo(3));
        Assert.That(review.ReviewedCorrectionHash, Is.EqualTo(SealHash));
        AssertSafeJson(json);
        OperatorScenarioView view = (await fixture.Client.GetFromJsonAsync<OperatorScenarioView>(ScenarioRoute))!;
        Assert.That(view.ScoreCorrection!.CorrectionEnabled, Is.True);
        Assert.That(view.Actions.CorrectScore.Enabled, Is.True);
        Assert.That(view.Actions.Score.Enabled, Is.False);
        Assert.That(view.Actions.Publish.Enabled, Is.False);
        Assert.That(fixture.Backend.Requests.All(x => x.Method == HttpMethod.Get), Is.True);
    }

    [TestCase("missing-token", HttpStatusCode.BadRequest)]
    [TestCase("forged-token", HttpStatusCode.BadRequest)]
    [TestCase("missing-origin", HttpStatusCode.Forbidden)]
    [TestCase("foreign-origin", HttpStatusCode.Forbidden)]
    [TestCase("missing-key", HttpStatusCode.BadRequest)]
    public async Task CorrectScore_UsesTheActualOriginAndNativeAntiforgeryGuards(string invalid, HttpStatusCode expected)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        using var response = await fixture.PostAsync($"{RunRoute}/correct-score",
            new { actor = "Owner", reason = "Preserve rejected score", reviewedCorrectionHash = SealHash },
            token: invalid != "missing-token", overrideToken: invalid == "forged-token" ? "forged" : null,
            origin: invalid == "missing-origin" ? "" : invalid == "foreign-origin" ? "http://localhost:5174" : null,
            key: invalid == "missing-key" ? "" : "correct-score");
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task CorrectScore_RequiresTheNativeCookieAndBlocksDisabledConfiguration()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        OperatorSession session = await fixture.SessionAsync();
        using var client = new HttpClient(new HttpClientHandler { UseCookies = false }) { BaseAddress = fixture.Client.BaseAddress };
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{RunRoute}/correct-score")
        {
            Content = JsonContent.Create(new { actor = "Owner", reason = "Reviewed", reviewedCorrectionHash = SealHash })
        };
        request.Headers.Add("Origin", client.BaseAddress!.GetLeftPart(UriPartial.Authority));
        request.Headers.Add("Idempotency-Key", "cookie-required");
        request.Headers.Add(session.CsrfHeaderName, session.CsrfRequestToken);
        using var response = await client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
        await using Fixture disabled = await Fixture.StartAsync(configured: false);
        using var disabledResponse = await disabled.PostAsync($"{RunRoute}/correct-score",
            new { actor = "Owner", reason = "Reviewed", reviewedCorrectionHash = SealHash }, token: false);
        Assert.That(disabledResponse.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(disabled.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task CorrectScore_ForwardsOnlyExactReviewedRequest_AndNeverPublishes()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.Status = "Failed"; fixture.Backend.Stage = "S9Score";
        var body = new { actor = "Local owner", reason = "Keep the rejected evaluation", reviewedCorrectionHash = SealHash };
        using var response = await fixture.PostAsync($"{RunRoute}/correct-score", body, key: "reviewed-correction");
        string json = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), json);
        var result = JsonSerializer.Deserialize<OperatorScoringCorrectionResult>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.That(result.Outcome, Is.EqualTo("score-corrected"));
        Assert.That(result.Status, Is.EqualTo("AwaitingDependency"));
        AssertSafeJson(json);
        fixture.Backend.Status = "Scored";
        using var replay = await fixture.PostAsync($"{RunRoute}/correct-score", body, key: "reviewed-correction");
        Assert.That(await replay.Content.ReadAsStringAsync(), Is.EqualTo(json));
        var posts = fixture.Backend.Requests.Where(x => x.Method == HttpMethod.Post).ToArray();
        Assert.That(posts.Length, Is.EqualTo(2));
        foreach (var post in posts)
        {
            Assert.That(post.Path, Is.EqualTo($"/drillingoperations/api/scenarios/{ScenarioId:D}/runs/{RunId:D}/correct-score"));
            Assert.That(post.Key, Is.EqualTo("reviewed-correction"));
            Assert.That(post.InternalKey, Is.EqualTo(Secret));
            using var document = JsonDocument.Parse(post.Body!);
            Assert.That(document.RootElement.EnumerateObject().Select(x => x.Name), Is.EquivalentTo(new[] { "actor", "reason", "reviewedCorrectionHash" }));
            Assert.That(document.RootElement.GetProperty("reviewedCorrectionHash").GetString(), Is.EqualTo(SealHash));
        }
    }

    [TestCase("actor")]
    [TestCase("reason")]
    [TestCase("hash")]
    [TestCase("unknown-field")]
    public async Task CorrectScore_RejectsInvalidBodyBeforeCallingBackend(string invalid)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        object body = invalid == "unknown-field"
            ? new { actor = "Owner", reason = "Reviewed", reviewedCorrectionHash = SealHash, scoringModelVersion = "invented" }
            : new { actor = invalid == "actor" ? new string('a', 101) : "Owner",
                reason = invalid == "reason" ? new string('a', 501) : "Reviewed",
                reviewedCorrectionHash = invalid == "hash" ? "invalid" : SealHash };
        using var response = await fixture.PostAsync($"{RunRoute}/correct-score", body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(fixture.Backend.Requests, Is.Empty);
    }

    [Test]
    public async Task ScoreCorrection_RejectsForeignScopeAndMalformedProjection()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.RunScenario = Guid.NewGuid();
        using var wrongScope = await fixture.Client.GetAsync($"{RunRoute}/scoring-correction");
        Assert.That(wrongScope.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        fixture.Backend.RunScenario = ScenarioId;
        fixture.Backend.ScoreCorrectionEnabled = true;
        fixture.Backend.ScoreCorrectionVersion = "invented";
        using var invalid = await fixture.Client.GetAsync($"{RunRoute}/scoring-correction");
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        AssertSafeJson(await invalid.Content.ReadAsStringAsync());
        fixture.Backend.ScoreCorrectionEnabled = false;
        using var disabled = await fixture.Client.GetAsync($"{RunRoute}/scoring-correction");
        var review = (await disabled.Content.ReadFromJsonAsync<OperatorScoringCorrectionReview>())!;
        Assert.That(review.ReviewedCorrectionHash, Is.Null);
        Assert.That(review.RejectedScorecardId, Is.Null);
        AssertSafeJson(await disabled.Content.ReadAsStringAsync());
        fixture.Backend.ScoreCorrectionResponse = HttpStatusCode.Conflict;
        using var rejected = await fixture.PostAsync($"{RunRoute}/correct-score",
            new { actor = "Owner", reason = "Reviewed", reviewedCorrectionHash = SealHash });
        Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        AssertSafeJson(await rejected.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task SimulatorSetupProjectsOnlyPublicOptionsAndForwardsReviewedConfiguration()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Backend.HasBinding = false;
        fixture.Backend.HasRun = false;
        using HttpResponseMessage read = await fixture.Client.GetAsync($"{ScenarioRoute}/setup");
        string json = await read.Content.ReadAsStringAsync();
        AssertSafeJson(json);
        OperatorSimulationSetup setup = JsonSerializer.Deserialize<OperatorSimulationSetup>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.That(setup.Available, Is.True);
        Assert.That(setup.Profiles, Has.Count.EqualTo(1));
        using HttpResponseMessage response = await fixture.PostAsync($"{ScenarioRoute}/setup", new
        {
            actor = "Demo operator", profileId = "profile-one", resolution = "Preview", realizationSeed = 123,
            reviewedSealHash = SealHash
        }, key: "prepare-model");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), await response.Content.ReadAsStringAsync());
        AssertSafeJson(await response.Content.ReadAsStringAsync());
        OperatorActionResult result = (await response.Content.ReadFromJsonAsync<OperatorActionResult>())!;
        Assert.That(result.Outcome, Is.EqualTo("simulation-prepared"));
        RequestCapture sent = fixture.Backend.Requests.Single(item => item.Method == HttpMethod.Post);
        Assert.That(sent.Path, Is.EqualTo($"/drillingoperations/api/scenarios/{ScenarioId:D}/setup"));
        Assert.That(sent.Key, Is.EqualTo("prepare-model"));
        Assert.That(sent.InternalKey, Is.EqualTo(Secret));
        Assert.That(sent.Body, Does.Not.Contain(Hidden).And.Not.Contain(Secret));
        Assert.That(fixture.Backend.HasRun, Is.False);
    }

    [TestCase("missing-token", HttpStatusCode.BadRequest)]
    [TestCase("missing-key", HttpStatusCode.BadRequest)]
    [TestCase("foreign-origin", HttpStatusCode.Forbidden)]
    [TestCase("unapproved", HttpStatusCode.Conflict)]
    [TestCase("unknown-field", HttpStatusCode.BadRequest)]
    [TestCase("negative-seed", HttpStatusCode.BadRequest)]
    public async Task SimulatorSetupEnforcesBrowserGuardsAndPredictionApproval(string failure, HttpStatusCode expected)
    {
        await using Fixture fixture = await Fixture.StartAsync();
        if (failure == "unapproved") fixture.Ledger.Prediction = fixture.Ledger.Prediction! with { Approval = null };
        object body = failure == "unknown-field"
            ? new { actor = "Demo operator", profileId = "profile-one", resolution = "Preview", realizationSeed = 123, reviewedSealHash = SealHash, worldId = Hidden }
            : new { actor = "Demo operator", profileId = "profile-one", resolution = "Preview", realizationSeed = failure == "negative-seed" ? -1 : 123, reviewedSealHash = SealHash };
        using HttpResponseMessage response = await fixture.PostAsync($"{ScenarioRoute}/setup", body,
            token: failure != "missing-token", key: failure == "missing-key" ? "" : "prepare-model",
            origin: failure == "foreign-origin" ? "https://foreign.invalid" : null);
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        Assert.That(fixture.Backend.Requests, Is.Empty);
        AssertSafeJson(await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task CompletedScenarioDoesNotReportMissingHumanApprovalOrUnpublishedEvidence()
    {
        await using Fixture fixture = await Fixture.StartAsync();
        fixture.Ledger.Scenario = fixture.Ledger.Scenario with { Status = ScenarioStatus.Scored };
        fixture.Backend.Status = "Scored";
        fixture.Backend.Stage = "S9Score";
        OperatorScenarioView view = (await fixture.Client.GetFromJsonAsync<OperatorScenarioView>(ScenarioRoute))!;
        Assert.Multiple(() =>
        {
            Assert.That(view.Preflight.Reason, Does.Contain("finished").And.Not.Contain("HumanApproved"));
            Assert.That(view.Actions.Start.Enabled, Is.False);
            Assert.That(view.Actions.Publish.Enabled, Is.False);
            Assert.That(view.Actions.Publish.Reason, Does.Contain("already been published"));
            Assert.That(view.Actions.Score.Reason, Does.Contain("Evaluation is complete"));
        });
    }

    private sealed class Fixture(WebApplication app, HttpClient client, FakeBackend backend, FakeLedger ledger) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public FakeBackend Backend { get; } = backend;
        public FakeLedger Ledger { get; } = ledger;
        private OperatorSession? _session;

        public static async Task<Fixture> StartAsync(
            bool configured = true, string environment = "Development", string backendUrl = "http://localhost:43111",
            CapturingLogger? logs = null)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environment,
                ContentRootPath = Directory.GetCurrentDirectory()
            });
            builder.Logging.ClearProviders();
            if (logs is not null) builder.Logging.AddProvider(logs);
            builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
            builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LocalOperator:BackendUrl"] = configured ? backendUrl : null,
                ["LocalOperator:InternalKey"] = configured ? Secret : null,
                ["LocalOperator:BrowserOrigin"] = "http://localhost:5173"
            });
            var backend = new FakeBackend();
            var ledger = new FakeLedger();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<ApiExceptionHandler>();
            builder.Services.AddSingleton<IOperatorLedger>(ledger);
            builder.Services.AddLocalOperatorWorkflow(builder.Configuration);
            builder.Services.AddHttpClient(OperatorBackendClient.ClientName).ConfigurePrimaryHttpMessageHandler(() => backend);
            WebApplication app = builder.Build();
            app.UseExceptionHandler();
            app.MapLocalOperatorWorkflow();
            app.MapPost("/api/scenarios/{scenarioId:guid}/prediction/seal",
                (Guid scenarioId, HttpRequest request, HttpResponse response) =>
                {
                    ledger.SealCalls++;
                    if (ledger.SealError is not null) throw ledger.SealError;
                    int? revision = PredictionLedgerService.ParseIfMatchRevision(request.Headers.IfMatch.FirstOrDefault());
                    ledger.LastSealRevision = revision;
                    response.Headers.ETag = PredictionLedgerService.FormatRevisionEtag(revision ?? 1);
                    return Results.Ok(new { scenarioId, sealHash = SealHash });
                }).RequireLocalOperatorMutation();
            await app.StartAsync();
            var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() })
            {
                BaseAddress = new Uri(app.Urls.Single()), Timeout = TimeSpan.FromSeconds(15)
            };
            return new(app, client, backend, ledger);
        }

        public async Task<OperatorSession> SessionAsync() =>
            _session ??= (await Client.GetFromJsonAsync<OperatorSession>("/api/operator/session"))!;

        public async Task<HttpResponseMessage> PostAsync(
            string path, object body, bool token = true, string? overrideToken = null,
            string? origin = null, string key = "stable-attempt")
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
            if (origin != "") request.Headers.Add("Origin", origin ?? Client.BaseAddress!.GetLeftPart(UriPartial.Authority));
            if (key != "") request.Headers.Add("Idempotency-Key", key);
            if (token)
            {
                OperatorSession session = await SessionAsync();
                request.Headers.Add(session.CsrfHeaderName, overrideToken ?? session.CsrfRequestToken);
            }
            return await Client.SendAsync(request);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    private sealed class FakeLedger : IOperatorLedger
    {
        private static readonly DateTimeOffset Now = new(2026, 9, 11, 0, 0, 0, TimeSpan.Zero);
        public Scenario Scenario { get; set; } = new(ScenarioId, Guid.NewGuid(), null, "Reservoir", Now, Now,
            "seed", "world-v1", "observation-v1", "scoring-v1", ScenarioStatus.HumanApproved, PackageHash, Now, Now);
        public PredictionRecord? Prediction { get; set; } = new(ScenarioId,
            new("candidate", [new(0, 0, 0, 0), new(1000, 1000, 0, 0)], [], new(1, 2, 3), [], [], [],
                ["Synthetic"], [], PackageHash, "Reviewed"), 1, Now, Now,
            new(SealHash, PackageHash, Now), new("Local owner", Now, SealHash), []);
        public int ApprovalCount { get; private set; }
        public string? LastActor { get; private set; }
        public int SealCalls { get; set; }
        public int? LastSealRevision { get; set; }
        public ScenarioApiException? SealError { get; set; }
        public Task<Scenario> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken) => Task.FromResult(Scenario);
        public Task<PredictionRecord?> GetPredictionAsync(Guid scenarioId, CancellationToken cancellationToken) => Task.FromResult(Prediction);
        public Task ApproveAsync(Guid scenarioId, string actor, CancellationToken cancellationToken)
        {
            ApprovalCount++;
            LastActor = actor;
            return Task.CompletedTask;
        }
    }

    private sealed record RequestCapture(
        HttpMethod Method, string Path, string? Body, string? Key, string? InternalKey,
        string? Actor, string? ReviewedHash);

    private sealed class CapturingLogger : ILoggerProvider, ILogger
    {
        public List<(EventId Event, string Message, Exception? Exception)> Entries { get; } = [];
        public ILogger CreateLogger(string categoryName) => this;
        public void Dispose() { }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (eventId.Id == 8100) Entries.Add((eventId, formatter(state, exception), exception));
        }
    }

    private sealed class FakeBackend : HttpMessageHandler
    {
        public List<RequestCapture> Requests { get; } = [];
        public bool HasRun { get; set; }
        public bool HasBinding { get; set; } = true;
        public bool FailReads { get; set; }
        public bool ThrowUnexpected { get; set; }
        public bool HasCompletion { get; set; }
        public Guid CompletionScenario { get; set; } = ScenarioId;
        public string BindingSeal { get; set; } = SealHash;
        public Guid RunScenario { get; set; } = ScenarioId;
        public Guid AuditScenario { get; set; } = ScenarioId;
        public string Status { get; set; } = "Queued";
        public string? Stage { get; set; }
        public HttpStatusCode PublishResponse { get; set; } = HttpStatusCode.OK;
        public HttpStatusCode ScoreResponse { get; set; } = HttpStatusCode.OK;
        public HttpStatusCode CompletionApprovalResponse { get; set; } = HttpStatusCode.Accepted;
        public bool RecoveryEnabled { get; set; } = true;
        public string RecoveryReason { get; set; } = Hidden;
        public string RecoveryHash { get; set; } = SealHash;
        public HttpStatusCode RecoveryResponse { get; set; } = HttpStatusCode.OK;
        public bool ScoreCorrectionEnabled { get; set; }
        public string ScoreCorrectionVersion { get; set; } = ScoringCorrectionVersions.Correction;
        public HttpStatusCode ScoreCorrectionResponse { get; set; } = HttpStatusCode.OK;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string path = request.RequestUri!.AbsolutePath;
            string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new(request.Method, path, body,
                request.Headers.TryGetValues("Idempotency-Key", out var keys) ? keys.Single() : null,
                request.Headers.TryGetValues("X-DrillSim-Internal-Key", out var secrets) ? secrets.Single() : null,
                request.Headers.TryGetValues("X-DrillSim-Human-Actor", out var actors) ? actors.Single() : null,
                request.Headers.TryGetValues("X-DrillSim-Reviewed-Openings-Hash", out var reviewed) ? reviewed.Single() : null));
            if (ThrowUnexpected) throw new InvalidOperationException($"{Secret} {Hidden} http://localhost:43112");
            if (FailReads) return Json(HttpStatusCode.InternalServerError, new { detail = Secret, worldId = Hidden });
            if (path.EndsWith("/setup", StringComparison.Ordinal))
            {
                if (request.Method == HttpMethod.Get)
                    return Json(HttpStatusCode.OK, new
                    {
                        scenarioId = ScenarioId, available = !HasBinding, reason = HasBinding ? "Simulator already prepared." : null,
                        prepared = HasBinding, current = (object?)null,
                        profiles = new[] { new { profileId = "profile-one", name = "Demo reservoir", description = "Synthetic model.", worldModelVersion = "world-v1", worldId = Hidden } },
                        defaults = new { profileId = "profile-one", resolution = "Preview", realizationSeed = 123 },
                        reviewedSealHash = SealHash, worldId = Hidden, operatorKey = Secret
                    });
                using JsonDocument setup = JsonDocument.Parse(body!);
                JsonElement root = setup.RootElement;
                HasBinding = true;
                return Json(HttpStatusCode.Created, new
                {
                    scenarioId = ScenarioId, outcome = "simulation-prepared", reviewedSealHash = SealHash,
                    configuration = new
                    {
                        profileId = root.GetProperty("profileId").GetString(), profileName = "Demo reservoir",
                        resolution = root.GetProperty("resolution").GetString(), realizationSeed = root.GetProperty("realizationSeed").GetInt32(),
                        preparedBy = root.GetProperty("actor").GetString(), preparedUtc = DateTimeOffset.UtcNow, worldId = Hidden
                    },
                    worldId = Hidden, operatorKey = Secret
                });
            }
            if (request.Method == HttpMethod.Get && path.EndsWith("/scoring-correction", StringComparison.Ordinal))
                return Json(HttpStatusCode.OK, new
                {
                    scenarioId = ScenarioId, runId = RunId, correctionEnabled = ScoreCorrectionEnabled, reason = Hidden,
                    rejectedScorecardId = "44000000-0000-0000-0000-000000000001", rejectedScorecardSha256 = PackageHash,
                    scoringInputSha256 = SealHash, correctionVersion = ScoreCorrectionVersion,
                    metricCount = 94, invalidMetricCount = 3, changedMetricCount = 5, reviewedCorrectionHash = SealHash,
                    correctedScorecardId = "44000000-0000-0000-0000-000000000002",
                    correctedScorecardSha256 = new string('c', 64), correctedInputSha256 = new string('d', 64),
                    canonicalInputJson = Hidden, truthSamples = new[] { Secret }, internalKey = Secret
                });
            if (request.Method == HttpMethod.Post && path.EndsWith("/correct-score", StringComparison.Ordinal))
                return Json(ScoreCorrectionResponse, new
                {
                    scenarioId = ScenarioId, runId = RunId, outcome = "score-corrected", status = "AwaitingDependency",
                    rejectedScorecardId = "44000000-0000-0000-0000-000000000001", rejectedScorecardSha256 = PackageHash,
                    scoringInputSha256 = SealHash, correctedScorecardId = "44000000-0000-0000-0000-000000000002",
                    correctedScorecardSha256 = new string('c', 64), correctedInputSha256 = new string('d', 64),
                    correctionVersion = ScoreCorrectionVersion, reviewedCorrectionHash = SealHash,
                    auditId = "44000000-0000-0000-0000-000000000003", worldId = Hidden, detail = Secret
                });
            if (request.Method == HttpMethod.Get && path.EndsWith("/publication-recovery", StringComparison.Ordinal))
                return Json(HttpStatusCode.OK, new
                {
                    scenarioId = ScenarioId, runId = RunId, recoveryEnabled = RecoveryEnabled, reason = RecoveryReason,
                    reviewedPublicationHash = RecoveryHash, stagedManifestSha256 = PackageHash, publicationPlanSha256 = SealHash,
                    operationCount = 223, verifiedOperationCount = 9, pendingOperationCount = 214, completedStageCount = 8,
                    canonicalPayloadJson = Hidden, clonedFieldId = Hidden, worldId = Hidden, internalKey = Secret
                });
            if (request.Method == HttpMethod.Post && path.EndsWith("/recover-publication", StringComparison.Ordinal))
                return Json(RecoveryResponse, new
                {
                    scenarioId = ScenarioId, runId = RunId, outcome = "publication-recovered", previousStatus = "Failed", status = "PublishFailed",
                    reviewedPublicationHash = SealHash, auditId = Guid.Parse("33000000-0000-0000-0000-000000000003"),
                    worldId = Hidden, internalKey = Secret, detail = Hidden
                });
            if (request.Method == HttpMethod.Get && path.EndsWith("/binding", StringComparison.Ordinal))
                return HasBinding ? Json(HttpStatusCode.OK, new
                {
                    scenarioId = ScenarioId, approvedSealedPredictionHash = BindingSeal, sourcePackageSha256 = PackageHash,
                    worldModelVersion = "world-v1", worldId = Hidden, internalKey = Secret, bindingMetadata = new { completionBindingId = Hidden }
                }) : new(HttpStatusCode.NotFound);
            if (request.Method == HttpMethod.Get && path.EndsWith("/audit", StringComparison.Ordinal))
                return Json(HttpStatusCode.OK, HasRun ? new object[]
                {
                    new { scenarioId = AuditScenario, sequence = 1, action = "run.created", subjectId = RunId.ToString("D"), canonicalInputJson = Hidden }
                } : []);
            if (request.Method == HttpMethod.Get && path.EndsWith("/events", StringComparison.Ordinal))
            {
                string events = string.Join("", OperatorBackendClient.StageNames.Select((stage, index) =>
                    $"event: stage\ndata: {JsonSerializer.Serialize(new { stage, name = Hidden, status = index < 6 ? "Completed" : index == 6 && Status == "AwaitingApproval" ? "AwaitingApproval" : "Pending", attemptCount = 1, truthArray = new[] { Hidden } })}\n\n"));
                return new(HttpStatusCode.OK) { Content = new StringContent(events, Encoding.UTF8, "text/event-stream") };
            }
            if (request.Method == HttpMethod.Get && path.EndsWith("/completion", StringComparison.Ordinal))
                return HasCompletion ? Json(HttpStatusCode.OK, new
                {
                    runId = RunId, scenarioId = CompletionScenario, status = "Draft", openingsHash = SealHash,
                    openings = new[] { new { reservoirName = "Reservoir", type = "Perforated", topMdM = 10d, baseMdM = 20d,
                        wellboreRadiusM = .1, skin = 2d, efficiency = .85, uncertaintyM = 2d, openingId = Hidden } },
                    bindingMetadata = new { worldId = Hidden }, truthSamples = new[] { Secret }
                }) : new(HttpStatusCode.NotFound);
            if (request.Method == HttpMethod.Post && path.EndsWith("/completion/approve", StringComparison.Ordinal))
                return Json(CompletionApprovalResponse, new { status = "Approved", completionBindingId = Hidden });
            if (request.Method == HttpMethod.Get && path.EndsWith($"/runs/{RunId:D}", StringComparison.Ordinal))
                return RunResponse();
            if (request.Method == HttpMethod.Post && path == "/drillingoperations/api/runs")
            {
                HasRun = true;
                return RunResponse();
            }
            if (request.Method == HttpMethod.Post && path.EndsWith("/publish", StringComparison.Ordinal))
            {
                if (PublishResponse == HttpStatusCode.OK) Status = "Revealed";
                return Json(PublishResponse, new { runId = RunId, internalKey = Secret, worldId = Hidden });
            }
            if (request.Method == HttpMethod.Post && path.EndsWith("/score", StringComparison.Ordinal))
            {
                if (ScoreResponse == HttpStatusCode.OK) Status = "Scored";
                return Json(ScoreResponse, new { truthSamples = new[] { Hidden }, detail = Secret });
            }
            if (request.Method == HttpMethod.Post && (path.EndsWith("/cancel", StringComparison.Ordinal) || path.EndsWith("/resume", StringComparison.Ordinal)))
                return RunResponse();
            return Json(HttpStatusCode.NotFound, new { detail = Hidden });
        }

        private HttpResponseMessage RunResponse() => Json(HttpStatusCode.OK, new
        {
            runId = RunId, scenarioId = RunScenario, status = Status, currentStage = Stage,
            bindingMetadata = new { worldId = Hidden }, stageAUrl = "http://localhost:43112", publicationKey = Secret
        });
        private static HttpResponseMessage Json(HttpStatusCode status, object value) =>
            new(status) { Content = JsonContent.Create(value) };
    }
}
