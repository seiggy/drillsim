using System.Net;
using System.Net.Http.Json;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
public sealed class FormationInterpretationEndpointTests
{
    [TestCase("missing-token", HttpStatusCode.BadRequest)]
    [TestCase("forged-token", HttpStatusCode.BadRequest)]
    [TestCase("missing-key", HttpStatusCode.BadRequest)]
    [TestCase("duplicate-key", HttpStatusCode.BadRequest)]
    [TestCase("foreign-origin", HttpStatusCode.Forbidden)]
    [TestCase("missing-origin", HttpStatusCode.Forbidden)]
    [TestCase("foreign-host", HttpStatusCode.Forbidden)]
    [TestCase("cross-site", HttpStatusCode.Forbidden)]
    public async Task ActualHttpOperatorGuard_RejectsBeforeReadingRequestOrCallingAI(string invalid, HttpStatusCode status)
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        using HttpResponseMessage response = await f.PostAsync("not-json", invalid);
        Assert.That(response.StatusCode, Is.EqualTo(status));
        Assert.That(response.Headers.CacheControl!.NoStore, Is.True);
        Assert.That(f.Data.Client.Calls, Is.Zero);
        Assert.That(await f.Data.Data.Service.ListAsync(f.Data.Data.LiveScope, null), Is.Empty);
    }

    [Test]
    public async Task HttpDraftAndStatus_ReturnExactContractNoStoreAndDoNotSaveOrCreateScenarios()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        using HttpResponseMessage status = await f.Client.GetAsync("/api/formation-interpretation/status");
        JsonNode statusBody = JsonNode.Parse(await status.Content.ReadAsStringAsync())!;
        Assert.That(statusBody.AsObject().Select(pair => pair.Key), Is.EquivalentTo(new[] { "configured", "reason" }));
        Assert.That(statusBody["configured"]!.GetValue<bool>(), Is.True);
        Assert.That(statusBody["reason"], Is.Null);
        Assert.That(status.Headers.CacheControl!.NoStore, Is.True);
        FormationInterpretationRequest request = await f.Data.RequestAsync();
        using HttpResponseMessage response = await f.PostAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        Assert.That(response.Headers.CacheControl!.NoStore, Is.True);
        FormationInterpretationResponse result = JsonSerializer.Deserialize<FormationInterpretationResponse>(body, FormationInterpretationAgent.JsonOptions)!;
        Assert.Multiple(() =>
        {
            Assert.That(result.Scope, Is.EqualTo(request.Scope));
            Assert.That(result.PackageSha256, Is.EqualTo(request.PackageSha256));
            Assert.That(result.AnalysisSha256, Is.EqualTo(request.AnalysisSha256));
            Assert.That(result.ConfigurationSha256, Is.EqualTo(request.Configuration.ComputeSha256()));
            Assert.That(JsonNode.Parse(body)!.AsObject().Select(pair => pair.Key), Is.EquivalentTo(new[]
            {
                "version", "promptVersion", "scope", "packageSha256", "analysisSha256", "configurationSha256",
                "selectedCandidateId", "savedHypothesis", "snapshotSha256", "generatedAt", "draft"
            }));
            Assert.That(result.Draft.CitedEvidenceIds, Has.Count.EqualTo(1));
            Assert.That(f.Data.Client.Calls, Is.EqualTo(2));
        });
        Assert.That(await f.Data.Data.Service.ListAsync(request.Scope, null), Is.Empty);
        Assert.That(await f.Data.Data.Scenarios.ListAsync(), Is.Empty);
    }

    [TestCase("extra-field")]
    [TestCase("missing-field")]
    [TestCase("duplicate-field")]
    [TestCase("wrong-type")]
    [TestCase("null-configuration")]
    [TestCase("oversized")]
    public async Task HttpBody_IsStrictAndBoundedBeforeInference(string invalid)
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        JsonNode node = JsonSerializer.SerializeToNode(await f.Data.RequestAsync(), FormationInterpretationAgent.JsonOptions)!;
        switch (invalid)
        {
            case "extra-field": node["stageAUrl"] = "https://foreign.invalid"; break;
            case "missing-field": node.AsObject().Remove("snapshotSha256"); break;
            case "wrong-type": node["configuration"]!["gridPointsPerAxis"] = "15"; break;
            case "null-configuration": node["configuration"] = null; break;
            case "oversized": node["notes"]!["rationale"] = new string('x', 128 * 1024); break;
        }
        string json = node.ToJsonString();
        if (invalid == "duplicate-field") json = json.Insert(1, "\"selectedCandidateId\":null,");
        using HttpResponseMessage response = await f.PostRawAsync(json);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), await response.Content.ReadAsStringAsync());
        Assert.That(f.Data.Client.Calls, Is.Zero);
    }

    [Test]
    public async Task UnconfiguredStatus_IsHonestAndDraftReturns503WithoutModelCall()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync(aiConfigured: false);
        FormationInterpretationStatus status = (await f.Client.GetFromJsonAsync<FormationInterpretationStatus>("/api/formation-interpretation/status"))!;
        Assert.That(status.Configured, Is.False);
        Assert.That(status.Reason, Does.Contain("AZURE_OPENAI"));
        using HttpResponseMessage response = await f.PostAsync(await f.Data.RequestAsync());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain(status.Reason));
        Assert.That(f.Data.Client.Calls, Is.Zero);
    }

    [Test]
    public async Task DisabledOperator_BlocksInferenceEvenWhenAIIsConfigured()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync(operatorConfigured: false);
        using HttpResponseMessage response = await f.PostAsync(new { }, "missing-token");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(f.Data.Client.Calls, Is.Zero);
    }

    [TestCase("model")]
    [TestCase("malformed")]
    [TestCase("invented-citation")]
    [TestCase("uri")]
    public async Task ModelFailure_IsSafeProblemNotFakeDraftAndNeverRetried(string failure)
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        f.Data.Client.Respond = (payload, _) =>
        {
            if (failure == "model") throw new InvalidOperationException("PRIVATE-ERROR-AND-SECRET");
            if (failure == "malformed") return Task.FromResult("malformed PRIVATE-ERROR-AND-SECRET");
            JsonNode result = JsonNode.Parse(FormationInterpretationClient.ValidReply(payload))!;
            if (failure == "uri") result["rationale"] = "See https://foreign.invalid/model";
            else result["citedEvidenceIds"] = new JsonArray($"geology:{Guid.NewGuid():D}");
            return Task.FromResult(result.ToJsonString());
        };
        using HttpResponseMessage response = await f.PostAsync(await f.Data.RequestAsync());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/problem+json"));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("PRIVATE-ERROR-AND-SECRET").And.Not.Contain("\"draft\""));
        Assert.That(f.Data.Client.Calls, Is.EqualTo(2));
        Assert.That(await f.Data.Data.Service.ListAsync(f.Data.Data.LiveScope, null), Is.Empty);
    }

    [TestCase(429, HttpStatusCode.TooManyRequests)]
    [TestCase(503, HttpStatusCode.BadGateway)]
    public async Task ProviderFailure_LogsOriginalExceptionOnRequestTraceWithoutExposingItOrRetrying(
        int providerStatus, HttpStatusCode responseStatus)
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        using var activity = new Activity("formation-endpoint-test").Start();
        using var providerResponse = new ProviderResponse(providerStatus);
        var providerException = new ClientResultException(
            $"PRIVATE-PROVIDER-CONTENT: HTTP {providerStatus}; retry after 60 seconds.", providerResponse,
            new InvalidOperationException("PRIVATE-INNER-EXCEPTION"));
        f.Data.Client.InitialFailure = providerException;
        using HttpResponseMessage response = await f.PostAsync(await f.Data.RequestAsync());
        string content = await response.Content.ReadAsStringAsync();
        FormationInterpretationLogCapture.Entry[] entries = f.Logs.Entries.ToArray();
        Assert.That(entries, Has.Length.EqualTo(1));
        FormationInterpretationLogCapture.Entry log = entries.Single();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(responseStatus));
            Assert.That(content, Does.Not.Contain("PRIVATE-PROVIDER-CONTENT")
                .And.Not.Contain("PRIVATE-INNER-EXCEPTION").And.Not.Contain("\"draft\""));
            if (providerStatus == 429)
                Assert.That(content, Does.Contain("AI capacity limit reached").And.Contain("no automatic retry"));
            Assert.That(log.Level, Is.EqualTo(LogLevel.Error));
            Assert.That(log.Event.Name, Is.EqualTo("FormationInterpretationFailure"));
            Assert.That(log.Exception, Is.SameAs(providerException));
            Assert.That(log.Exception?.StackTrace, Is.Not.Empty);
            Assert.That(log.Exception?.ToString(), Does.Contain("retry after 60 seconds")
                .And.Contain("PRIVATE-INNER-EXCEPTION").And.Not.Contain("formation-interpretation-evidence-v1"));
            Assert.That(log.Properties.SingleOrDefault(item => item.Key == "ErrorType").Value, Is.EqualTo(nameof(ClientResultException)));
            Assert.That(log.Properties.SingleOrDefault(item => item.Key == "ProviderStatusCode").Value, Is.EqualTo(providerStatus));
            Assert.That(log.Context?.TraceId, Is.EqualTo(activity.TraceId));
            Assert.That(log.Context?.SpanId, Is.Not.EqualTo(activity.SpanId));
            Assert.That(f.Data.Client.Calls, Is.EqualTo(1));
        });
        Assert.That(await f.Data.Data.Service.ListAsync(f.Data.Data.LiveScope, null), Is.Empty);
    }

    [Test]
    public async Task CorruptSavedSnapshot_Is503NotAReplacementLiveDraft()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        HypothesisRevision saved = await f.Data.Data.Service.CreateAsync(await f.Data.Data.RequestAsync(), "create");
        FormationInterpretationRequest request = await f.Data.RequestAsync();
        request = request with { SavedHypothesis = new(saved.HypothesisId, saved.Revision), SnapshotSha256 = saved.SnapshotSha256 };
        await f.Data.Data.ExecuteAsync("DROP TRIGGER hypothesis_revisions_no_update; UPDATE hypothesis_revisions SET content_json='PRIVATE-ARTIFACT';");
        using HttpResponseMessage response = await f.PostAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("PRIVATE-ARTIFACT"));
        Assert.That(f.Data.Client.Calls, Is.Zero);
    }

    [Test]
    public void ProductionRegistration_UsesExistingClientWithoutInferenceRetriesAndNoBroadAgentTools()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DrillSim.AnalysisApi.csproj"))) directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        string program = File.ReadAllText(Path.Combine(directory!.FullName, "Program.cs"));
        string endpoints = File.ReadAllText(Path.Combine(directory.FullName, "Infrastructure", "FormationInterpretationEndpoints.cs"));
        Assert.Multiple(() =>
        {
            Assert.That(program, Does.Contain("RetryPolicy = new ClientRetryPolicy(0)"));
            Assert.That(program, Does.Contain("builder.Services.AddFormationInterpretation(")
                .And.Contain("formationResponsesClient, deploymentName, enableSensitiveData: builder.Environment.IsDevelopment()")
                .And.Contain(".GetResponsesClient()"));
            Assert.That(program[program.IndexOf("ChatClient? chatClient", StringComparison.Ordinal)
                ..program.IndexOf("ResponsesClient? formationResponsesClient", StringComparison.Ordinal)], Does.Not.Contain("RetryPolicy"));
            Assert.That(endpoints, Does.Contain("responseClient.AsAIAgent(options, model: deploymentName,")
                .And.Contain("clientFactory: client => client.AsBuilder()")
                .And.Contain("sourceName: FormationInterpretationAgent.TelemetrySourceName"));
            Assert.That(endpoints, Does.Contain(".RequireLocalOperatorMutation()"));
            Assert.That(endpoints, Does.Not.Contain("ScenarioAgentTools").And.Not.Contain("FieldPackageService"));
        });
    }

    private sealed class ProviderResponse(int status) : PipelineResponse
    {
        public override int Status => status;
        public override string ReasonPhrase => ((HttpStatusCode)status).ToString();
        public override BinaryData Content => BinaryData.FromString("{}");
        public override Stream? ContentStream { get; set; }
        protected override PipelineResponseHeaders HeadersCore => throw new NotSupportedException();
        public override BinaryData BufferContent(CancellationToken cancellationToken = default) => Content;
        public override ValueTask<BinaryData> BufferContentAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult(Content);
        public override void Dispose() => ContentStream?.Dispose();
    }
}

internal sealed class FormationInterpretationHttpFixture(
    WebApplication app, HttpClient client, FormationInterpretationFixture data, FormationInterpretationLogCapture logs) : IAsyncDisposable
{
    public HttpClient Client { get; } = client;
    public FormationInterpretationFixture Data { get; } = data;
    public FormationInterpretationLogCapture Logs { get; } = logs;
    private OperatorSession? _session;

    public static async Task<FormationInterpretationHttpFixture> StartAsync(bool aiConfigured = true, bool operatorConfigured = true)
    {
        var data = new FormationInterpretationFixture();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development", ContentRootPath = Directory.GetCurrentDirectory() });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        var logs = new FormationInterpretationLogCapture();
        builder.Logging.ClearProviders().AddProvider(logs);
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Sqlite"] = data.Data.ConnectionString,
            ["LocalOperator:BackendUrl"] = operatorConfigured ? "http://127.0.0.1:43001" : null,
            ["LocalOperator:InternalKey"] = operatorConfigured ? "test-only-key" : null,
            ["LocalOperator:BrowserOrigin"] = "http://localhost:5173"
        });
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton(data.Data.Scenarios);
        builder.Services.AddSingleton(new SqliteScenarioStore(data.Data.ConnectionString));
        builder.Services.AddSingleton<IPetrophysicsAnalysisService>(data.Data.Analysis);
        builder.Services.AddSingleton<PredictionLedgerService>();
        builder.Services.AddLocalOperatorWorkflow(builder.Configuration);
        builder.Services.AddHypotheses(builder.Configuration);
        builder.Services.AddSingleton(aiConfigured ? data.Agent : new FormationInterpretationAgent(null));
        builder.Services.AddSingleton<FormationInterpretationService>();
        builder.Services.AddProblemDetails();
        WebApplication app = builder.Build();
        app.MapLocalOperatorWorkflow();
        app.MapFormationInterpretation();
        await app.StartAsync();
        var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), UseProxy = false })
        {
            BaseAddress = new Uri(app.Urls.Single()), Timeout = TimeSpan.FromSeconds(20)
        };
        return new(app, client, data, logs);
    }

    public Task<HttpResponseMessage> PostAsync(object body, string? invalid = null) =>
        SendAsync(JsonContent.Create(body), invalid);
    public Task<HttpResponseMessage> PostRawAsync(string json) =>
        SendAsync(new StringContent(json, Encoding.UTF8, "application/json"), null);

    private async Task<HttpResponseMessage> SendAsync(HttpContent content, string? invalid)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/formation-interpretation") { Content = content };
        if (invalid != "missing-origin") request.Headers.Add("Origin",
            invalid == "foreign-origin" ? "http://foreign.invalid" : Client.BaseAddress!.GetLeftPart(UriPartial.Authority));
        if (invalid != "missing-key") request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));
        if (invalid == "duplicate-key") request.Headers.Add("Idempotency-Key", "second");
        if (invalid == "foreign-host") request.Headers.Host = "foreign.invalid";
        if (invalid == "cross-site") request.Headers.Add("Sec-Fetch-Site", "cross-site");
        if (invalid != "missing-token")
        {
            _session ??= (await Client.GetFromJsonAsync<OperatorSession>("/api/operator/session"))!;
            request.Headers.Add(_session.CsrfHeaderName, invalid == "forged-token" ? "forged" : _session.CsrfRequestToken);
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

internal sealed class FormationInterpretationLogCapture : ILoggerProvider, ILogger
{
    public sealed record Entry(LogLevel Level, EventId Event, Exception? Exception,
        KeyValuePair<string, object?>[] Properties, ActivityContext? Context);

    public ConcurrentQueue<Entry> Entries { get; } = new();
    public ILogger CreateLogger(string categoryName) => this;
    public void Dispose() { }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (eventId.Id == 8300)
            Entries.Enqueue(new(logLevel, eventId, exception,
                state is IEnumerable<KeyValuePair<string, object?>> properties ? properties.ToArray() : [],
                Activity.Current?.Context));
    }
}
