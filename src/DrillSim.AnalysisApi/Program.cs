using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Identity;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using System.ClientModel.Primitives;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddAGUIServer();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPackageHasher, CanonicalJsonHasher>();
builder.Services.AddSingleton<IFieldPackageService, FieldPackageService>();
builder.Services.AddSingleton<IPetrophysicsAnalysisService, PetrophysicsAnalysisService>();
builder.Services.AddSingleton<ConfiguredAnalysisService>();
builder.Services.AddSingleton(_ => new SqliteScenarioStore(
    builder.Configuration.GetConnectionString("Sqlite")
    ?? throw new InvalidOperationException("ConnectionStrings:Sqlite is required.")));
builder.Services.AddSingleton<ScenarioService>();
builder.Services.AddSingleton<PredictionLedgerService>();
builder.Services.AddSingleton<RevealService>();
builder.Services.AddSingleton<ScorecardService>();
builder.Services.AddLocalOperatorWorkflow(builder.Configuration);
builder.Services.AddHypotheses(builder.Configuration);
string callbackKey = builder.Configuration["DRILLING_OPERATIONS_CALLBACK_KEY"]
    ?? throw new InvalidOperationException("DRILLING_OPERATIONS_CALLBACK_KEY is required.");
builder.Services.AddSingleton(new InternalCallbackKeyValidator(callbackKey));

AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.FieldClient, "FieldServiceUrl");
AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.ClusterClient, "ClusterServiceUrl");
AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.WellClient, "WellServiceUrl");
AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.WellBoreClient, "WellBoreServiceUrl");
AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.ArchitectureClient, "WellBoreArchitectureServiceUrl");
AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.TrajectoryClient, "TrajectoryServiceUrl");
AddUpstreamClient(builder.Services, builder.Configuration, FieldPackageService.GeologyClient, "GeologicalPropertiesServiceUrl");

string? endpointValue = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
string? deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];
string? openAiSubscriptionId = builder.Configuration["AZURE_OPENAI_SUBSCRIPTION_ID"];
bool agentConfigured = !string.IsNullOrWhiteSpace(endpointValue) &&
    !string.IsNullOrWhiteSpace(deploymentName) &&
    !string.IsNullOrWhiteSpace(openAiSubscriptionId);
string? mapsClientId = builder.Configuration["AZURE_MAPS_CLIENT_ID"];
string? mapsTenantId = builder.Configuration["AZURE_MAPS_TENANT_ID"];
string? mapsSubscriptionId = builder.Configuration["AZURE_MAPS_SUBSCRIPTION_ID"];
bool mapsConfigured = !string.IsNullOrWhiteSpace(mapsClientId) &&
    !string.IsNullOrWhiteSpace(mapsTenantId) &&
    !string.IsNullOrWhiteSpace(mapsSubscriptionId);
AzureCliCredential? agentCredential = agentConfigured
    ? new AzureCliCredential(new AzureCliCredentialOptions
    {
        Subscription = openAiSubscriptionId
    })
    : null;
AzureCliCredential? mapsCredential = mapsConfigured
    ? new AzureCliCredential(new AzureCliCredentialOptions
    {
        Subscription = mapsSubscriptionId
    })
    : null;

string scenarioInstructions =
    "You are a blind scenario petrophysics analyst. You can access only the scenario clock, its public visibility summary, " +
    "and field evidence filtered at the current scenario clock. You cannot access hidden or future evidence, infer hidden IDs " +
    "or values, or use unrestricted field packages or analyses. Always use the provided scenario tools for factual claims. " +
    "Analyze only the reservoir selected by the scenario and account for every visible intersecting wellbore. Distinguish " +
    "logged controls from interval-only geometry and do not confuse ranked target outputs with the evidence population. " +
    "Cite factual claims with returned visible evidence IDs and package SHA-256 IDs. Explicitly label each statement as " +
    "observed, human-interpreted, derived, model-estimated, or synthetic. Call net pay expected paydirt, never reserves, " +
    "and explain missing visible evidence and deterministic assumptions without inventing values. After scoring, the public " +
    "scorecard may be read only as aggregate statistics; a HiddenTruth basis never grants access to raw truth values.";

#pragma warning disable OPENAI001
ResponsesClient? responsesClient = agentConfigured
    ? new OpenAIClient(
        new BearerTokenPolicy(agentCredential!, "https://cognitiveservices.azure.com/.default"),
        new OpenAIClientOptions { Endpoint = new Uri(endpointValue!, UriKind.Absolute) })
        .GetResponsesClient()
    : null;
ResponsesClient? formationResponsesClient = agentConfigured
    ? new OpenAIClient(
        new BearerTokenPolicy(agentCredential!, "https://cognitiveservices.azure.com/.default"),
        new OpenAIClientOptions { Endpoint = new Uri(endpointValue!, UriKind.Absolute), RetryPolicy = new ClientRetryPolicy(0) })
        .GetResponsesClient()
    : null;
#pragma warning restore OPENAI001

builder.Services.AddFormationInterpretation(
    formationResponsesClient, deploymentName, enableSensitiveData: builder.Environment.IsDevelopment());

WebApplication app = builder.Build();
await app.Services.GetRequiredService<SqliteScenarioStore>()
    .InitializeAsync(app.Lifetime.ApplicationStopping);
app.UseExceptionHandler();
app.MapLocalOperatorWorkflow();
app.MapHypotheses();
app.MapFormationInterpretation();
app.UseWhen(
    context => ScenarioAgentScopeBinding.IsScenarioEndpoint(context.Request.Path),
    branch => branch.Use(ScenarioAgentScopeBinding.InvokeAsync));
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/internal"),
    branch => branch.Use(async (context, next) =>
    {
        InternalCallbackKeyValidator validator =
            context.RequestServices.GetRequiredService<InternalCallbackKeyValidator>();
        if (!validator.IsAuthorized(context.Request.Headers))
        {
            await Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized").ExecuteAsync(context);
            return;
        }
        await next(context);
    }));

app.MapGet("/api/fields", async (IFieldPackageService packages, CancellationToken cancellationToken) =>
    Results.Json(await packages.GetFieldsAsync(cancellationToken)));

app.MapGet("/api/fields/{fieldId:guid}/package", async (
    Guid fieldId,
    Guid? scenarioId,
    DateTimeOffset? asOf,
    ScenarioService scenarios,
    CancellationToken cancellationToken) =>
    Results.Ok(await scenarios.GetPackageAsync(fieldId, scenarioId, asOf, cancellationToken)));

app.MapGet("/api/fields/{fieldId:guid}/analysis", async (
    Guid fieldId,
    string? reservoir,
    Guid? scenarioId,
    DateTimeOffset? asOf,
    ScenarioService scenarios,
    IPetrophysicsAnalysisService analysis,
    CancellationToken cancellationToken) =>
{
    AnalysisPackage package = await scenarios.GetPackageAsync(fieldId, scenarioId, asOf, cancellationToken);
    string? selectedReservoir = reservoir;
    if (scenarioId is Guid id)
    {
        Scenario scenario = await scenarios.GetAsync(id, cancellationToken);
        if (!string.IsNullOrWhiteSpace(reservoir) &&
            !string.Equals(reservoir.Trim(), scenario.ReservoirName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Scenario reservoir mismatch",
                $"Scenario {id:D} is bound to reservoir '{scenario.ReservoirName}'.");
        }
        selectedReservoir = scenario.ReservoirName;
    }
    return Results.Ok(analysis.Analyze(package, selectedReservoir));
});

app.MapGet("/api/analysis/configuration/default", () => Results.Ok(AnalysisConfiguration.Default));
app.MapGet("/api/analysis/prediction-capabilities", () => Results.Ok(ConfiguredPredictionBinding.Capabilities()));

app.MapPost("/api/fields/{fieldId:guid}/analysis", async (
    Guid fieldId,
    ConfiguredAnalysisRequest request,
    ConfiguredAnalysisService analysis,
    CancellationToken cancellationToken) =>
    Results.Ok(await analysis.AnalyzeAsync(fieldId, request, cancellationToken)));

app.MapPost("/api/scenarios", async (
    CreateScenarioRequest request,
    ScenarioService scenarios,
    CancellationToken cancellationToken) =>
{
    Scenario scenario = await scenarios.CreateAsync(request, cancellationToken);
    return Results.Created($"/api/scenarios/{scenario.ScenarioId:D}", scenario);
});

app.MapGet("/api/scenarios", async (ScenarioService scenarios, CancellationToken cancellationToken) =>
    Results.Ok(await scenarios.ListAsync(cancellationToken)));

app.MapGet("/api/scenarios/{scenarioId:guid}", async (
    Guid scenarioId,
    ScenarioService scenarios,
    CancellationToken cancellationToken) =>
    Results.Ok(await scenarios.GetAsync(scenarioId, cancellationToken)));

app.MapGet("/api/scenarios/{scenarioId:guid}/evidence-visibility", async (
    Guid scenarioId,
    DateTimeOffset? asOf,
    ScenarioService scenarios,
    CancellationToken cancellationToken) =>
    Results.Ok(await scenarios.GetVisibilitySummaryAsync(scenarioId, asOf, cancellationToken)));

app.MapPut("/api/scenarios/{scenarioId:guid}/prediction", async (
    Guid scenarioId,
    PredictionBody body,
    HttpRequest request,
    HttpResponse response,
    PredictionLedgerService predictions,
    CancellationToken cancellationToken) =>
{
    string? ifMatch = request.Headers.TryGetValue("If-Match", out var values)
        ? values.Count == 1 ? values[0] : string.Empty
        : null;
    int? expectedRevision = PredictionLedgerService.ParseIfMatchRevision(ifMatch);
    PredictionRecord prediction = await predictions.PutDraftAsync(
        scenarioId,
        body,
        expectedRevision,
        cancellationToken);
    response.Headers.ETag = PredictionLedgerService.FormatRevisionEtag(prediction.Revision);
    return Results.Ok(prediction);
});

app.MapGet("/api/scenarios/{scenarioId:guid}/prediction", async (
    Guid scenarioId,
    HttpResponse response,
    PredictionLedgerService predictions,
    CancellationToken cancellationToken) =>
{
    PredictionRecord prediction = await predictions.GetAsync(scenarioId, cancellationToken);
    response.Headers.ETag = PredictionLedgerService.FormatRevisionEtag(prediction.Revision);
    return Results.Ok(prediction);
});

app.MapPost("/api/scenarios/{scenarioId:guid}/prediction/seal", async (
    Guid scenarioId,
    HttpRequest request,
    HttpResponse response,
    PredictionLedgerService predictions,
    CancellationToken cancellationToken) =>
{
    string? ifMatch = request.Headers.TryGetValue("If-Match", out var values)
        ? values.Count == 1 ? values[0] : string.Empty
        : null;
    int? expectedRevision = PredictionLedgerService.ParseIfMatchRevision(ifMatch);
    PredictionRecord prediction = await predictions.SealAsync(scenarioId, cancellationToken, expectedRevision);
    response.Headers.ETag = PredictionLedgerService.FormatRevisionEtag(prediction.Revision);
    return Results.Ok(prediction);
}).RequireLocalOperatorMutation();

app.MapPost("/api/scenarios/{scenarioId:guid}/prediction/approve", () =>
{
    return Results.Problem(statusCode: StatusCodes.Status410Gone,
        title: "Use the local operator workflow",
        detail: "Prediction approval requires the operator session, antiforgery token, actor label and reviewed seal hash at /api/operator/scenarios/{scenarioId}/approve-prediction.");
});

app.MapPost("/internal/scenarios/{scenarioId:guid}/reveal/prepare", async (
    Guid scenarioId,
    RevealRequest request,
    RevealService reveals,
    CancellationToken cancellationToken) =>
    Results.Ok(await reveals.PrepareAsync(scenarioId, request, cancellationToken)));

app.MapPost("/internal/scenarios/{scenarioId:guid}/reveal/finalize", async (
    Guid scenarioId,
    FinalizeRevealRequest request,
    RevealService reveals,
    CancellationToken cancellationToken) =>
    Results.Ok(await reveals.FinalizeAsync(scenarioId, request, cancellationToken)));

app.MapPost("/internal/scenarios/{scenarioId:guid}/reveal/snapshot/backfill", async (
    Guid scenarioId,
    RevealRequest request,
    RevealService reveals,
    CancellationToken cancellationToken) =>
    Results.Ok(await reveals.BackfillCloneSnapshotAsync(scenarioId, request, cancellationToken)));

app.MapGet("/internal/scenarios/{scenarioId:guid}/reveal/{revealId:guid}/status", async (
    Guid scenarioId,
    Guid revealId,
    RevealService reveals,
    CancellationToken cancellationToken) =>
    Results.Ok(await reveals.GetInternalStatusAsync(scenarioId, revealId, cancellationToken)));

app.MapGet("/api/scenarios/{scenarioId:guid}/reveal", async (
    Guid scenarioId,
    RevealService reveals,
    CancellationToken cancellationToken) =>
    Results.Ok(await reveals.GetRevealAsync(scenarioId, cancellationToken)));

app.MapGet("/api/scenarios/{scenarioId:guid}/production", async (
    Guid scenarioId,
    RevealService reveals,
    CancellationToken cancellationToken) =>
    Results.Ok(await reveals.GetProductionAsync(scenarioId, cancellationToken)));

app.MapPost("/internal/scenarios/{scenarioId:guid}/scorecard", async (
    Guid scenarioId,
    ScorecardRequest request,
    ScorecardService scorecards,
    CancellationToken cancellationToken) =>
    Results.Ok(await scorecards.PublishAsync(scenarioId, request, cancellationToken)));

app.MapGet("/api/scenarios/{scenarioId:guid}/scorecard", async (
    Guid scenarioId,
    ScorecardService scorecards,
    CancellationToken cancellationToken) =>
    Results.Ok(await scorecards.GetAsync(scenarioId, cancellationToken)));

app.MapGet("/api/agent/status", () => Results.Ok(new
{
    configured = agentConfigured,
    provider = agentConfigured ? "Azure OpenAI" : null,
    deployment = agentConfigured ? deploymentName : null
}));
app.MapGet("/api/maps/status", () => Results.Ok(new
{
    configured = mapsConfigured,
    clientId = mapsConfigured ? mapsClientId : null
}));
app.MapGet("/api/maps/token", async (CancellationToken cancellationToken) =>
{
    if (!mapsConfigured || mapsCredential is null)
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Azure Maps is not configured",
            detail: "Set AZURE_MAPS_CLIENT_ID to enable Azure Maps.");

    AccessToken token = await mapsCredential.GetTokenAsync(
        new TokenRequestContext(["https://atlas.microsoft.com/.default"]),
        cancellationToken);
    return Results.Text(token.Token, "text/plain");
});

// Authentication and claims-based tenant/user isolation are required here before multi-user deployment.
if (agentConfigured)
{
    IFieldPackageService packages = app.Services.GetRequiredService<IFieldPackageService>();
    IPetrophysicsAnalysisService analysis = app.Services.GetRequiredService<IPetrophysicsAnalysisService>();

    AIFunction packageTool = AIFunctionFactory.Create(
        (Guid fieldId, CancellationToken cancellationToken) => packages.BuildPackageAsync(fieldId, cancellationToken),
        "get_field_package",
        "Gets the evidence package and stable source IDs for one field.");
    AIFunction analysisTool = AIFunctionFactory.Create(
        async (Guid fieldId, string? reservoir, CancellationToken cancellationToken) =>
        {
            AnalysisPackage package = await packages.BuildPackageAsync(fieldId, cancellationToken);
            return analysis.Analyze(package, reservoir);
        },
        "analyze_field",
        "Runs deterministic expected paydirt screening for one field.");
    string legacyInstructions =
        "You are a non-scenario petrophysics analysis workstation assistant. Always call the provided tools for field evidence. " +
        "Analyze the selected reservoir interval and account for every intersecting wellbore; distinguish logged controls " +
        "from interval-only geometry and do not confuse the five ranked target outputs with the evidence population. " +
        "Cite every factual claim with returned evidence IDs such as field, cluster, well, wellbore, geology, trajectory, " +
        "candidate, and package SHA-256 IDs. Explicitly label each statement as observed, human-interpreted, derived, " +
        "model-estimated, or synthetic. Call net pay expected paydirt. Never describe expected paydirt as reserves and " +
        "never make a reserves claim. Explain missing evidence and deterministic assumptions without inventing values.";
#pragma warning disable OPENAI001
    var legacyAgent = responsesClient!.AsAIAgent(
        new ChatClientAgentOptions
        {
            Name = "DrillSimPetrophysicsAnalyst",
            ChatOptions = AgentResponsesOptions.Create(legacyInstructions, [packageTool, analysisTool])
        },
        model: deploymentName)
        .AsBuilder()
        .UseOpenTelemetry()
        .Build();
    AIFunction[] scenarioTools = ScenarioAgentTools.Create(
        app.Services.GetRequiredService<IHttpContextAccessor>(),
        app.Services.GetRequiredService<ScenarioService>(),
        app.Services.GetRequiredService<IPetrophysicsAnalysisService>());
    var scenarioAgent = responsesClient!.AsAIAgent(
        new ChatClientAgentOptions
        {
            Name = ScenarioAgentScope.AgentName,
            ChatOptions = AgentResponsesOptions.Create(scenarioInstructions, [.. scenarioTools])
        },
        model: deploymentName)
        .AsBuilder()
        .UseOpenTelemetry()
        .Build();
#pragma warning restore OPENAI001
    app.MapAGUIServer("/agui", legacyAgent);
    app.MapAGUIServer("/agui/scenario", scenarioAgent);
}
else
{
    static IResult AgentNotConfigured() => Results.Problem(
        statusCode: StatusCodes.Status503ServiceUnavailable,
        title: "Agent is not configured",
        detail: "Set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_DEPLOYMENT_NAME to enable AG-UI.");

    app.MapPost("/agui", AgentNotConfigured);
    app.MapPost("/agui/scenario", AgentNotConfigured);
}

app.MapDefaultEndpoints();
app.Run();

static void AddUpstreamClient(IServiceCollection services, IConfiguration configuration, string clientName, string configurationKey)
{
    services.AddHttpClient(clientName, client =>
    {
        string value = configuration[configurationKey]
            ?? throw new InvalidOperationException($"Configuration key {configurationKey} is required.");
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
            throw new InvalidOperationException($"Configuration key {configurationKey} must be an absolute URL.");
        client.BaseAddress = uri;
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}

public partial class Program;
