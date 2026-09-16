using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http.Resilience;
using DrillingOperations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

string connectionString = builder.Configuration.GetConnectionString("Sqlite")
    ?? throw new InvalidOperationException("ConnectionStrings:Sqlite is required for drilling operations persistence.");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:Sqlite must not be empty.");
string internalKey = builder.Configuration["DRILLING_OPERATIONS_INTERNAL_KEY"]
    ?? throw new InvalidOperationException("DRILLING_OPERATIONS_INTERNAL_KEY is required.");
if (internalKey.Length == 0 || internalKey.Length > 1024)
    throw new InvalidOperationException("DRILLING_OPERATIONS_INTERNAL_KEY must contain between 1 and 1024 characters.");
string analysisCallbackKey = builder.Configuration["DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY"] ?? throw new InvalidOperationException("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY is required.");
if (analysisCallbackKey.Length == 0 || analysisCallbackKey.Length > 1024) throw new InvalidOperationException("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY must contain between 1 and 1024 characters.");
string publicationImportKey = builder.Configuration["DRILLSIM_PUBLICATION_IMPORT_KEY"] ?? throw new InvalidOperationException("DRILLSIM_PUBLICATION_IMPORT_KEY is required.");
if (publicationImportKey.Length == 0 || publicationImportKey.Length > 1024) throw new InvalidOperationException("DRILLSIM_PUBLICATION_IMPORT_KEY must contain between 1 and 1024 characters.");
string reservoirOperatorKey = builder.Configuration["RESERVOIR_SIMULATION_OPERATOR_KEY"]
    ?? throw new InvalidOperationException("RESERVOIR_SIMULATION_OPERATOR_KEY is required.");
if (reservoirOperatorKey.Length == 0 || reservoirOperatorKey.Length > 1024)
    throw new InvalidOperationException("RESERVOIR_SIMULATION_OPERATOR_KEY must contain between 1 and 1024 characters.");
Uri analysisApiUri = RequiredBaseUri(builder.Configuration, "AnalysisApiUrl");
Uri reservoirSimulationUri = RequiredBaseUri(builder.Configuration, "ReservoirSimulationUrl");

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new InternalKeyValidator(internalKey));
builder.Services.AddHttpClient<AnalysisVerificationClient>(client =>
{
    client.BaseAddress = analysisApiUri; client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<ReservoirVerificationClient>(client =>
{
    client.BaseAddress = reservoirSimulationUri; client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", reservoirOperatorKey);
});
builder.Services.AddTransient<BindingVerificationService>();
builder.Services.AddHttpClient<ReservoirSetupClient>(client =>
{
    client.BaseAddress = reservoirSimulationUri;
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", reservoirOperatorKey);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    UseCookies = false,
    UseProxy = false
}).RemoveAllResilienceHandlers();
builder.Services.AddTransient<SimulationSetupCoordinator>();
builder.Services.AddHttpClient<AnalysisPublicationClient>(client => { client.BaseAddress = analysisApiUri; client.Timeout = TimeSpan.FromMinutes(2); });
builder.Services.AddHttpClient<AnalysisRevealClient>(client => { client.BaseAddress = analysisApiUri; client.Timeout = TimeSpan.FromMinutes(2); client.DefaultRequestHeaders.Add("X-DrillSim-Internal-Key", analysisCallbackKey); });
builder.Services.AddHttpClient<AnalysisScorecardClient>(client => { client.BaseAddress = analysisApiUri; client.Timeout = TimeSpan.FromMinutes(2); client.DefaultRequestHeaders.Add("X-DrillSim-Internal-Key", analysisCallbackKey); });
builder.Services.AddSingleton<ScoreCoordinator>();
builder.Services.AddSingleton<ScoringCorrectionCoordinator>();
builder.Services.AddHttpClient("FieldService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "FieldServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddHttpClient("ClusterService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "ClusterServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddHttpClient("WellService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "WellServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddHttpClient("WellBoreService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "WellBoreServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddHttpClient("WellBoreArchitectureService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "WellBoreArchitectureServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddHttpClient("TrajectoryService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "TrajectoryServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddHttpClient("GeologicalPropertiesService", client => { client.BaseAddress = RequiredBaseUri(builder.Configuration, "GeologicalPropertiesServiceUrl"); client.DefaultRequestHeaders.Add("X-DrillSim-Publication-Key", publicationImportKey); });
builder.Services.AddSingleton<OntologyPublicationClient>();
builder.Services.AddSingleton<PublicationCoordinator>();
builder.Services.AddSingleton<PublicationRecoveryCoordinator>();
DrillingExecutionOptions drillingOptions = builder.Configuration.GetSection(DrillingExecutionOptions.SectionName).Get<DrillingExecutionOptions>() ?? new();
drillingOptions.Validate();
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(drillingOptions));
SurveyObservationOptions surveyOptions = builder.Configuration.GetSection(SurveyObservationOptions.SectionName).Get<SurveyObservationOptions>() ?? new();
surveyOptions.Validate();
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(surveyOptions));
TruthSamplingOptions truthOptions = builder.Configuration.GetSection(TruthSamplingOptions.SectionName).Get<TruthSamplingOptions>() ?? new();
truthOptions.Validate();
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(truthOptions));
LogObservationOptions logOptions = builder.Configuration.GetSection(LogObservationOptions.SectionName).Get<LogObservationOptions>() ?? new();
logOptions.Validate();
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(logOptions));
DrillingObservationOptions drillingObservationOptions = builder.Configuration.GetSection(DrillingObservationOptions.SectionName).Get<DrillingObservationOptions>() ?? new();
drillingObservationOptions.Validate();
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(drillingObservationOptions));
CompletionDesignOptions completionOptions = builder.Configuration.GetSection(CompletionDesignOptions.SectionName).Get<CompletionDesignOptions>() ?? new();
completionOptions.Validate();
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(completionOptions));
ProductionExecutionOptions productionOptions = builder.Configuration.GetSection(ProductionExecutionOptions.SectionName).Get<ProductionExecutionOptions>() ?? new(); productionOptions.Validate(); builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(productionOptions));
ProductionMeterOptions meterOptions = builder.Configuration.GetSection(ProductionMeterOptions.SectionName).Get<ProductionMeterOptions>() ?? new(); meterOptions.Validate(); builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(meterOptions));
builder.Services.AddHttpClient<ReservoirProductionClient>(client => { client.BaseAddress = reservoirSimulationUri; client.Timeout = Timeout.InfiniteTimeSpan; client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", reservoirOperatorKey); })
    .RemoveAllResilienceHandlers()
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
        options.Retry.MaxRetryAttempts = 1;
    });
builder.Services.AddHttpClient<ReservoirCompletionClient>(client => { client.BaseAddress = reservoirSimulationUri; client.Timeout = TimeSpan.FromSeconds(30); client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", reservoirOperatorKey); });
builder.Services.AddHttpClient<ReservoirSamplingClient>(client =>
{
    client.BaseAddress = reservoirSimulationUri; client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", reservoirOperatorKey);
});
builder.Services.AddSingleton<IDependencyCapabilityProbe, BuiltInDependencyCapabilityProbe>();
builder.Services.AddSingleton(serviceProvider => new DrillingOperationsStore(
    connectionString, serviceProvider.GetRequiredService<TimeProvider>()));
builder.Services.AddSingleton<IRunCheckpointExecutor, RunStageExecutor>();
builder.Services.AddSingleton<RunOrchestrator>();
builder.Services.AddHostedService<RunWorker>();

WebApplication app = builder.Build();
await app.Services.GetRequiredService<DrillingOperationsStore>().InitializeAsync();
app.UseExceptionHandler();
InternalKeyValidator authorization = app.Services.GetRequiredService<InternalKeyValidator>();
app.Use(async (context, next) =>
{
    bool isOperatorApi = context.Request.Path.StartsWithSegments("/drillingoperations/api");
    bool isStatus = context.Request.Path.Equals("/drillingoperations/api/status");
    if (isOperatorApi && !isStatus && !authorization.IsAuthorized(context.Request.Headers))
    {
        await Results.Problem(statusCode: StatusCodes.Status401Unauthorized,
            title: "Internal authorization required").ExecuteAsync(context);
        return;
    }
    await next(context);
});

RouteGroupBuilder api = app.MapGroup("/drillingoperations/api");
api.MapPublicationRecovery();
api.MapScoringCorrection();
api.MapSimulationSetup();
api.MapGet("/status", () => Results.Ok(new ServiceStatus(
    "DrillingOperations", "Ready", "Internal checkpoint orchestration; no identifiers or hidden values are exposed.")));

api.MapPost("/scenarios/{scenarioId}/bind", async Task<IResult> (
    string scenarioId,
    BindWorldRequest request,
    HttpRequest httpRequest,
    DrillingOperationsStore store,
    BindingVerificationService verifier,
    CancellationToken cancellationToken) =>
{
    string route = $"/drillingoperations/api/scenarios/{scenarioId}/bind";
    string key = IdempotencyKey(httpRequest);
    if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key.Any(static c => c < 33 || c > 126))
        return Results.Problem(statusCode: 400, title: "A bounded Idempotency-Key header is required");
    try
    {
        IReadOnlyDictionary<string, string[]> errors = RequestValidation.Validate(request);
        if (!StringComparer.Ordinal.Equals(scenarioId, request.ScenarioId))
            return Results.ValidationProblem(new Dictionary<string, string[]> { [nameof(request.ScenarioId)] = ["ScenarioId must match the route."] });
        if (errors.Count > 0) return Results.ValidationProblem(errors);
        ApiOutcome? replay = await store.TryReplayAsync(route, key, CanonicalJson.Serialize(request), cancellationToken);
        if (replay is not null) return Outcome(replay);
        await verifier.VerifyAsync(request, cancellationToken);
        ApiOutcome outcome = await store.BindWorldAsync(route, key, scenarioId, request, cancellationToken);
        return Outcome(outcome);
    }
    catch (BindingVerificationException exception)
    {
        return Results.Problem(statusCode: exception.StatusCode, title: "Authoritative binding verification failed", detail: exception.Message);
    }
});

api.MapGet("/scenarios/{scenarioId}/binding", async Task<IResult> (
    string scenarioId, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{
    if (!RequestValidation.IsCanonicalGuid(scenarioId, out _)) return Results.BadRequest(new { title = "scenarioId must be a canonical nonempty GUID", status = 400 });
    TruthBindingResponse? binding = await store.GetBindingAsync(scenarioId, cancellationToken);
    return binding is null ? Results.NotFound() : Results.Content(CanonicalJson.Serialize(binding), "application/json", Encoding.UTF8);
});

api.MapPost("/runs", async Task<IResult> (
    CreateRunRequest request,
    HttpRequest httpRequest,
    DrillingOperationsStore store,
    RunOrchestrator orchestrator,
    CancellationToken cancellationToken) =>
{
    ApiOutcome outcome = await store.CreateRunAsync("/drillingoperations/api/runs",
        IdempotencyKey(httpRequest), request, cancellationToken);
    if (outcome.StatusCode == StatusCodes.Status202Accepted && !outcome.Replayed) orchestrator.Signal();
    return Outcome(outcome);
});

api.MapGet("/runs/{runId}", async Task<IResult> (
    string runId, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{
    RunResponse? run = await store.GetRunAsync(runId, cancellationToken);
    return run is null ? Results.NotFound() : Results.Content(CanonicalJson.Serialize(run), "application/json", Encoding.UTF8);
});

api.MapGet("/runs/{runId}/events", async Task (
    string runId, HttpContext context, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{
    if (await store.GetRunAsync(runId, cancellationToken) is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    context.Response.StatusCode = StatusCodes.Status200OK;
    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-store";
    using var bounded = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    bounded.CancelAfter(TimeSpan.FromSeconds(5));
    var sent = new Dictionary<RunStageKind, string>();
    try
    {
        for (int poll = 0; poll < 20; poll++)
        {
            IReadOnlyList<StageResponse> stages = await store.GetStagesAsync(runId, bounded.Token);
            foreach (StageResponse stage in stages)
            {
                string payload = CanonicalJson.Serialize(new
                {
                    stage = stage.Stage.ToString(),
                    stage.Name,
                    status = stage.Status.ToString(),
                    stage.AttemptCount,
                    stage.StartedUtc,
                    stage.EndedUtc
                });
                if (sent.TryGetValue(stage.Stage, out string? prior) && prior == payload) continue;
                sent[stage.Stage] = payload;
                await context.Response.WriteAsync($"event: stage\ndata: {payload}\n\n", bounded.Token);
                await context.Response.Body.FlushAsync(bounded.Token);
            }
            RunResponse? run = await store.GetRunAsync(runId, bounded.Token);
            if (run is null || RunStateMachine.IsTerminal(run.Status) ||
                run.Status is RunStatus.Blocked or RunStatus.AwaitingDependency or RunStatus.AwaitingApproval) break;
            await Task.Delay(250, bounded.Token);
        }
    }
    catch (OperationCanceledException) when (bounded.IsCancellationRequested) { }
});

api.MapPost("/runs/{runId}/resume", async Task<IResult> (
    string runId, HttpRequest request, DrillingOperationsStore store, IDependencyCapabilityProbe capabilities,
    RunOrchestrator orchestrator, CancellationToken cancellationToken) =>
{
    RunResponse? blockedRun = await store.GetRunAsync(runId, cancellationToken);
    bool capabilityAvailable = blockedRun?.CurrentStage is RunStageKind blockedStage && capabilities.IsAvailable(blockedStage);
    ApiOutcome outcome = await store.ResumeAsync($"/drillingoperations/api/runs/{runId}/resume",
        IdempotencyKey(request), runId, capabilityAvailable, cancellationToken);
    if (outcome.StatusCode == StatusCodes.Status202Accepted && !outcome.Replayed) orchestrator.Signal();
    return Outcome(outcome);
});
api.MapGet("/runs/{runId}/completion", async Task<IResult> (
    string runId, HttpResponse response, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{
    response.Headers.CacheControl = "no-store";
    if (!RequestValidation.IsCanonicalGuid(runId, out _))
        return Results.Problem(statusCode: 400, title: "A canonical nonempty run ID is required");
    RunResponse? run = await store.GetRunAsync(runId, cancellationToken);
    if (run is null) return Results.NotFound();
    CompletionDesign? design = await store.GetCompletionDesignAsync(runId, cancellationToken);
    return design is null ? Results.NotFound() : Results.Ok(CompletionReview.FromDesign(run, design));
});
api.MapPost("/runs/{runId}/completion/approve", async Task<IResult> (
    string runId, HttpRequest request, DrillingOperationsStore store, RunOrchestrator orchestrator, CancellationToken cancellationToken) =>
{
    string actor = request.Headers.TryGetValue("X-DrillSim-Human-Actor", out var values) && values.Count == 1 ? values[0] ?? string.Empty : string.Empty;
    string? reviewedHash = null;
    if (request.Headers.TryGetValue(CompletionReview.ReviewedHashHeaderName, out var hashes))
    {
        if (hashes.Count != 1 || !CompletionReview.IsValidHash(hashes[0]))
            return Results.Problem(statusCode: 400, title: "A single reviewed lowercase openings SHA-256 hash is required");
        reviewedHash = hashes[0];
    }
    ApiOutcome outcome = await store.ApproveCompletionAsync(
        $"/drillingoperations/api/runs/{runId}/completion/approve", IdempotencyKey(request),
        runId, actor, cancellationToken, reviewedHash);
    if (outcome.StatusCode == StatusCodes.Status202Accepted && !outcome.Replayed) orchestrator.Signal();
    return Outcome(outcome);
});api.MapPost("/runs/{runId}/cancel", async Task<IResult> (
    string runId, HttpRequest request, DrillingOperationsStore store, CancellationToken cancellationToken) =>
    Outcome(await store.CancelAsync($"/drillingoperations/api/runs/{runId}/cancel",
        IdempotencyKey(request), runId, cancellationToken)));
api.MapPost("/runs/{runId}/publish", async Task<IResult> (
    string runId, HttpRequest request, PublicationCoordinator publication, CancellationToken cancellationToken) =>
    Outcome(await publication.PublishAsync($"/drillingoperations/api/runs/{runId}/publish",
        IdempotencyKey(request), runId, cancellationToken)));
api.MapPost("/runs/{runId}/reconcile-analysis-snapshot", async Task<IResult> (
    string runId, HttpRequest request, PublicationCoordinator publication, CancellationToken cancellationToken) =>
    Outcome(await publication.BackfillAnalysisSnapshotAsync(
        $"/drillingoperations/api/runs/{runId}/reconcile-analysis-snapshot",
        IdempotencyKey(request), runId, cancellationToken)));
api.MapGet("/runs/{runId}/reveal", async Task<IResult> (string runId, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{ RevealSummary? value = await store.GetRevealSummaryAsync(runId, cancellationToken); return value is null ? Results.NotFound() : Results.Content(CanonicalJson.Serialize(value), "application/json"); });
api.MapPost("/runs/{runId}/score", async Task<IResult> (
    string runId, HttpRequest request, ScoreCoordinator scoring, CancellationToken cancellationToken) =>
    Outcome(await scoring.ScoreAsync($"/drillingoperations/api/runs/{runId}/score",
        IdempotencyKey(request), runId, cancellationToken)));
api.MapGet("/runs/{runId}/scorecard", async Task<IResult> (
    string runId, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{
    ScorecardSummary? value = await store.GetScorecardAsync(runId, cancellationToken);
    return value is null ? Results.NotFound() : Results.Content(CanonicalJson.Serialize(value), "application/json", Encoding.UTF8);
});

api.MapGet("/audit", async Task<IResult> (
    string? scenarioId, DrillingOperationsStore store, CancellationToken cancellationToken) =>
{
    if (!RequestValidation.IsCanonicalGuid(scenarioId, out _))
        return Results.BadRequest(new { title = "scenarioId must be a canonical nonempty GUID", status = 400 });
    if (!await store.VerifyAuditChainAsync(scenarioId!, cancellationToken))
        return Results.Problem(statusCode: 500, title: "Audit chain integrity failure");
    IReadOnlyList<AuditResponse> entries = await store.GetAuditAsync(scenarioId!, cancellationToken);
    return Results.Content(CanonicalJson.Serialize(entries), "application/json", Encoding.UTF8);
});

app.MapDefaultEndpoints();
app.Run();

static Uri RequiredBaseUri(IConfiguration configuration, string key)
{
    string value = configuration[key] ?? throw new InvalidOperationException($"Configuration {key} is required.");
    if (value.Length > 2048 || !Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) || uri.Scheme is not ("http" or "https"))
        throw new InvalidOperationException($"Configuration {key} must be an absolute HTTP(S) URI.");
    return new Uri(uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal) ? uri.AbsoluteUri : uri.AbsoluteUri + "/");
}
static string IdempotencyKey(HttpRequest request) =>
    request.Headers.TryGetValue("Idempotency-Key", out var values) && values.Count == 1 ? values[0] ?? string.Empty : string.Empty;
static IResult Outcome(ApiOutcome outcome)
{
    if (outcome.Location is not null) return new StoredOutcomeResult(outcome);
    return Results.Content(outcome.Body, "application/json", Encoding.UTF8, outcome.StatusCode);
}

public sealed class StoredOutcomeResult(ApiOutcome outcome) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = outcome.StatusCode;
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        if (outcome.Location is not null) httpContext.Response.Headers.Location = outcome.Location;
        await httpContext.Response.WriteAsync(outcome.Body, httpContext.RequestAborted);
    }
}

public partial class Program;



