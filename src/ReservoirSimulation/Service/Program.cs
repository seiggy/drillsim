using System.Globalization;
using System.Text.Json.Serialization;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;
using ReservoirSimulation.Simulation;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

string sqliteConnectionString = builder.Configuration.GetConnectionString("Sqlite")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Sqlite is required for reservoir persistence.");
if (string.IsNullOrWhiteSpace(sqliteConnectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:Sqlite must not be empty for reservoir persistence.");
string operatorKey = builder.Configuration["RESERVOIR_SIMULATION_OPERATOR_KEY"]
    ?? throw new InvalidOperationException(
        "Configuration RESERVOIR_SIMULATION_OPERATOR_KEY is required for reservoir world access.");
if (operatorKey.Length == 0)
    throw new InvalidOperationException(
        "Configuration RESERVOIR_SIMULATION_OPERATOR_KEY must not be empty.");

int worldCapacity = 4;
string? configuredCapacity = builder.Configuration["RESERVOIR_SIMULATION_WORLD_CAPACITY"];
if (configuredCapacity is not null &&
    (!int.TryParse(configuredCapacity, NumberStyles.None, CultureInfo.InvariantCulture, out worldCapacity) ||
     worldCapacity is < 1 or > 64))
    throw new InvalidOperationException(
        "Configuration RESERVOIR_SIMULATION_WORLD_CAPACITY must be an integer between 1 and 64.");

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(
        namingPolicy: null, allowIntegerValues: false)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new OperatorKeyValidator(operatorKey));
builder.Services.AddSingleton<IWorldStore>(new InMemoryWorldStore(worldCapacity));
builder.Services.AddSingleton<IReservoirWorldFactory, ReservoirWorldFactory>();
builder.Services.AddSingleton<IReservoirSimulator, ReservoirSimulator>();
builder.Services.AddSingleton(new SqliteReservoirRepository(sqliteConnectionString));
builder.Services.AddSingleton(new SqliteTruthSamplingRepository(sqliteConnectionString));
builder.Services.AddSingleton(new SqliteCompletionBindingRepository(sqliteConnectionString));
builder.Services.AddSingleton(new SqliteCompletionProductionRepository(sqliteConnectionString));
builder.Services.AddSingleton<CompletionProductionService>();
builder.Services.AddSingleton<ApprovedCompletionBindingService>();
builder.Services.AddSingleton<RestrictedTruthSamplingService>();
builder.Services.AddSingleton<PersistentWorldManager>();
builder.Services.AddSingleton<PersistentSimulationStateStore>();
builder.Services.AddHostedService<ReservoirPersistenceInitializer>();

WebApplication app = builder.Build();
app.UseExceptionHandler();
OperatorKeyValidator operatorAuthorization = app.Services.GetRequiredService<OperatorKeyValidator>();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/reservoirsimulation/api/worlds") &&
        !operatorAuthorization.IsAuthorized(context.Request.Headers))
    {
        await Results.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Operator authorization required").ExecuteAsync(context);
        return;
    }
    await next(context);
});

RouteGroupBuilder api = app.MapGroup("/reservoirsimulation/api");
api.MapGet("/status", () => Results.Ok(new ServiceStatus(
    "ReservoirSimulation",
    KernelMetadata.Name,
    KernelMetadata.Limitation,
    "Conditioning controls and cell truth remain private; Stage B internal orchestration is the sole intended world-route recipient.")));

RouteGroupBuilder worlds = api.MapGroup("/worlds")
    .AddEndpointFilter<ReservoirProblemLoggingFilter>();

worlds.MapGet("/setup-profiles", async Task<IResult> (
    string fieldId,
    string reservoirName,
    PersistentWorldManager manager,
    CancellationToken cancellationToken) =>
{
    try
    {
        WorldSetupProfileCatalog catalog = await manager.GetSetupProfilesAsync(
            WorldSetupRequestValidator.ParseFieldId(fieldId), reservoirName, cancellationToken);
        return Results.Ok(catalog);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException)
    {
        return SetupIntegrityProblem();
    }
});

worlds.MapPost("/setup", async Task<IResult> (
    WorldSetupRequest request,
    PersistentWorldManager manager,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await manager.PrepareRealizationAsync(request, cancellationToken);
        return world is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Setup model profile not found")
            : Results.Ok(world.Summary);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException)
    {
        return SetupIntegrityProblem();
    }
});

worlds.MapPost("", async Task<IResult> (
    WorldGenerationRequest request,
    PersistentWorldManager manager,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld world = await manager.CreateAsync(request, cancellationToken);
        return Results.Ok(world.Summary);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapGet("/{worldId}", async Task<IResult> (
    string worldId,
    PersistentWorldManager manager,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await manager.GetAsync(worldId, cancellationToken);
        return world is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found")
            : Results.Ok(world.Summary);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapPost("/{worldId}/runs", async Task<IResult> (
    string worldId,
    SimulationRequest request,
    PersistentWorldManager worldsManager,
    PersistentSimulationStateStore states,
    IReservoirSimulator simulator,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await worldsManager.GetAsync(worldId, cancellationToken);
        if (world is null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");

        SimulationRequestValidator.Validate(request, world);
        RestoredSimulationState? parent = null;
        if (request.ContinueFromStateId is not null)
        {
            parent = await states.LoadAsync(world, request.ContinueFromStateId, cancellationToken);
            if (parent is null)
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Continuation state not found");
        }

        SimulationExecution execution = parent is null
            ? simulator.Run(world, request, cancellationToken)
            : simulator.Run(world, request, parent.State, parent.SimulatedTimeSeconds, cancellationToken);
        string finalStateId = await states.SaveAsync(
            world, parent?.StateId, request, execution, cancellationToken);
        return Results.Ok(new SimulationRunEnvelope(execution.Result, finalStateId));
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (SimulationFailureException exception)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Reservoir simulation failed",
            detail: exception.Message);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapGet("/{worldId}/states/{stateId}", async Task<IResult> (
    string worldId,
    string stateId,
    PersistentSimulationStateStore states,
    CancellationToken cancellationToken) =>
{
    try
    {
        SimulationStateSummary? summary = await states.GetMetadataAsync(worldId, stateId, cancellationToken);
        return summary is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Simulation state not found")
            : Results.Ok(summary);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapPost("/{worldId}/completion-bindings", async Task<IResult> (
    string worldId,
    ApprovedCompletionBindingRequest request,
    PersistentWorldManager worldManager,
    ApprovedCompletionBindingService completionBindings,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
        if (world is null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
        ApprovedCompletionBindingMetadata metadata = await completionBindings.RegisterAsync(
            world, request, cancellationToken);
        return Results.Ok(metadata);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapGet("/{worldId}/completion-bindings/{completionBindingId}", async Task<IResult> (
    string worldId,
    string completionBindingId,
    PersistentWorldManager worldManager,
    ApprovedCompletionBindingService completionBindings,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
        if (world is null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
        ApprovedCompletionBindingMetadata? metadata = await completionBindings.GetMetadataAsync(
            world, completionBindingId, cancellationToken);
        return metadata is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Completion binding not found")
            : Results.Ok(metadata);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});


worlds.MapPost(
    "/{worldId}/completion-bindings/{completionBindingId}/production-runs",
    async Task<IResult> (
        string worldId,
        string completionBindingId,
        CompletionProductionRequest request,
        PersistentWorldManager worldManager,
        CompletionProductionService production,
        CancellationToken cancellationToken) =>
    {
        try
        {
            ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
            if (world is null)
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
            CompletionProductionResult result = await production.RunAsync(
                world, completionBindingId, request, cancellationToken);
            return Results.Ok(result);
        }
        catch (ReservoirValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors);
        }
        catch (SimulationFailureException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Completion production failed",
                detail: exception.Message);
        }
        catch (PersistenceIntegrityException exception)
        {
            return IntegrityProblem(exception);
        }
    });

worlds.MapGet(
    "/{worldId}/completion-bindings/{completionBindingId}/production-runs/{productionRunId}",
    async Task<IResult> (
        string worldId,
        string completionBindingId,
        string productionRunId,
        PersistentWorldManager worldManager,
        CompletionProductionService production,
        CancellationToken cancellationToken) =>
    {
        try
        {
            ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
            if (world is null)
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
            CompletionProductionResult? result = await production.GetAsync(
                world, completionBindingId, productionRunId, cancellationToken);
            return result is null
                ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Completion production run not found")
                : Results.Ok(result);
        }
        catch (ReservoirValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors);
        }
        catch (PersistenceIntegrityException exception)
        {
            return IntegrityProblem(exception);
        }
    });


worlds.MapPost("/{worldId}/path-bindings", async Task<IResult> (
    string worldId,
    ApprovedPathBindingRequest request,
    PersistentWorldManager worldManager,
    RestrictedTruthSamplingService sampler,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
        if (world is null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
        ApprovedPathBindingMetadata metadata = await sampler.RegisterAsync(world, request, cancellationToken);
        return Results.Ok(metadata);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors, extensions:
            exception.DiagnosticCode is null ? null : new Dictionary<string, object?>
            {
                ["diagnosticCode"] = exception.DiagnosticCode
            });
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapGet("/{worldId}/path-bindings/{bindingId}", async Task<IResult> (
    string worldId,
    string bindingId,
    PersistentWorldManager worldManager,
    RestrictedTruthSamplingService sampler,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
        if (world is null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
        RestoredPathBinding? binding = await sampler.LoadBindingAsync(world, bindingId, cancellationToken);
        return binding is null
            ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Approved path binding not found")
            : Results.Ok(binding.Metadata);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});

worlds.MapPost("/{worldId}/samples", async Task<IResult> (
    string worldId,
    TruthSamplingRequest request,
    PersistentWorldManager worldManager,
    RestrictedTruthSamplingService sampler,
    CancellationToken cancellationToken) =>
{
    try
    {
        ReservoirWorld? world = await worldManager.GetAsync(worldId, cancellationToken);
        if (world is null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Reservoir world not found");
        SamplingExecution execution = await sampler.SampleAsync(world, request, cancellationToken);
        return Results.Ok(execution.Result);
    }
    catch (ReservoirValidationException exception)
    {
        return Results.ValidationProblem(exception.Errors);
    }
    catch (PersistenceIntegrityException exception)
    {
        return IntegrityProblem(exception);
    }
});


worlds.MapDelete("/{worldId}", async Task<IResult> (
    string worldId,
    PersistentWorldManager manager,
    RestrictedTruthSamplingService sampler,
    CancellationToken cancellationToken) =>
{
    if (await sampler.WorldHasBindingsAsync(worldId, cancellationToken))
        return Results.Conflict(new
        {
            title = "World deletion blocked",
            detail = "Approved path bindings and sampling audit are immutable."
        });
    await manager.DeleteAsync(worldId, cancellationToken);
    return Results.Ok(new WorldDeletionResult(worldId, "deleted"));
});

app.MapDefaultEndpoints();
app.Run();

static IResult IntegrityProblem(PersistenceIntegrityException exception) => Results.Problem(
    statusCode: StatusCodes.Status500InternalServerError,
    title: "Reservoir persistence integrity failure",
    detail: exception.Message);

static IResult SetupIntegrityProblem() => Results.Problem(
    statusCode: StatusCodes.Status500InternalServerError,
    title: "Reservoir persistence integrity failure",
    detail: "A prepared model failed persistence integrity verification.");

public partial class Program;
