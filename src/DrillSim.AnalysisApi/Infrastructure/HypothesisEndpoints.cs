using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

public static class HypothesisEndpoints
{
    public static IServiceCollection AddHypotheses(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new SqliteHypothesisStore(configuration.GetConnectionString("Sqlite")
            ?? throw new InvalidOperationException("ConnectionStrings:Sqlite is required.")));
        services.AddSingleton<HypothesisService>();
        return services;
    }

    public static IEndpointRouteBuilder MapHypotheses(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/hypotheses");
        group.AddEndpointFilter(async (invocation, next) =>
        {
            HttpContext context = invocation.HttpContext;
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            try { return await next(invocation); }
            catch (ScenarioApiException exception)
            {
                return Results.Problem(statusCode: exception.StatusCode, title: exception.Title, detail: exception.Message);
            }
            catch (Exception exception) when (exception is InvalidDataException or SqliteException or UpstreamServiceException)
            {
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("DrillSim.AnalysisApi.Hypotheses")
                    .LogError(new EventId(8200, "HypothesisStorageFailure"), "Hypothesis operation failed with {ErrorType}.", exception.GetType().Name);
                return Results.Problem(statusCode: exception is UpstreamServiceException ? 502 : 503,
                    title: "Hypothesis artifact unavailable",
                    detail: "The evidence source or saved artifact could not be verified. No replacement snapshot or successful mutation is implied.");
            }
        });
        group.MapGet("", async (HttpContext context, HypothesisService service) =>
            Results.Ok(await service.ListAsync(Scope(context.Request), null, Page(context, "limit", 20), Page(context, "offset", 0), context.RequestAborted)));
        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, HypothesisService service) =>
            RevisionResponse(context, await service.GetLatestAsync(Scope(context.Request), id, context.RequestAborted)));
        group.MapGet("/{id:guid}/revisions", async (Guid id, HttpContext context, HypothesisService service) =>
            Results.Ok(await service.ListAsync(Scope(context.Request), id, Page(context, "limit", 20), Page(context, "offset", 0), context.RequestAborted)));
        group.MapGet("/{id:guid}/revisions/{revision:int}", async (Guid id, int revision, HttpContext context, HypothesisService service) =>
            RevisionResponse(context, await service.GetAsync(Scope(context.Request), new(id, revision), context.RequestAborted)));
        group.MapPost("", async (HttpContext context, HypothesisService service) =>
            RevisionResponse(context, await service.CreateAsync(await Body<HypothesisCreateRequest>(context), Key(context), context.RequestAborted), true))
            .RequireLocalOperatorMutation();
        group.MapPost("/{id:guid}/revisions", async (Guid id, HttpContext context, HypothesisService service) =>
            RevisionResponse(context, await service.ReviseAsync(id, await Body<HypothesisReviseRequest>(context), Match(context),
                Key(context), context.RequestAborted), true)).RequireLocalOperatorMutation();
        group.MapPost("/branches", async (HttpContext context, HypothesisService service) =>
            RevisionResponse(context, await service.BranchAsync(await Body<HypothesisBranchRequest>(context), Key(context), context.RequestAborted), true))
            .RequireLocalOperatorMutation();
        group.MapPost("/{id:guid}/revisions/{revision:int}/analysis", async (Guid id, int revision, HttpContext context, HypothesisService service) =>
            Results.Ok(await service.AnalyzeAsync(new(id, revision), await Body<HypothesisAnalysisRequest>(context), context.RequestAborted)));
        group.MapPost("/compare", async (HttpContext context, HypothesisService service) =>
            Results.Ok(await service.CompareAsync(await Body<HypothesisCompareRequest>(context), context.RequestAborted)));
        group.MapPost("/{id:guid}/revisions/{revision:int}/challenges", async (Guid id, int revision, HttpContext context, HypothesisService service) =>
        {
            HypothesisChallengeRequest request = await Body<HypothesisChallengeRequest>(context);
            return ChallengeResponse(context, request.Scope,
                await service.CreateChallengeAsync(new(id, revision), request, Key(context), context.RequestAborted), true);
        }).RequireLocalOperatorMutation();
        group.MapGet("/{id:guid}/revisions/{revision:int}/challenges", async (Guid id, int revision, HttpContext context, HypothesisService service) =>
            Results.Ok(await service.ListChallengesAsync(Scope(context.Request), new(id, revision), Page(context, "limit", 20),
                Page(context, "offset", 0), context.RequestAborted)));
        group.MapGet("/{id:guid}/revisions/{revision:int}/challenges/{challengeId:guid}", async (
            Guid id, int revision, Guid challengeId, HttpContext context, HypothesisService service) =>
        {
            HypothesisScope scope = Scope(context.Request);
            int? version = Query(context.Request, "version") is not null ? Page(context, "version", 1) : null;
            return ChallengeResponse(context, scope,
                await service.GetChallengeAsync(scope, new(id, revision), challengeId, version, context.RequestAborted));
        });
        group.MapPut("/{id:guid}/revisions/{revision:int}/challenges/{challengeId:guid}/dispositions", async (
            Guid id, int revision, Guid challengeId, HttpContext context, HypothesisService service) =>
        {
            HypothesisDispositionRequest request = await Body<HypothesisDispositionRequest>(context);
            return ChallengeResponse(context, request.Scope,
                await service.DispositionAsync(new(id, revision), challengeId, request, Match(context), Key(context), context.RequestAborted));
        }).RequireLocalOperatorMutation();
        return endpoints;
    }

    private static IResult RevisionResponse(HttpContext context, HypothesisRevision value, bool created = false)
    {
        context.Response.Headers.ETag = PredictionLedgerService.FormatRevisionEtag(value.Revision);
        return created
            ? Results.Created($"/api/hypotheses/{value.HypothesisId:D}/revisions/{value.Revision}{ScopeQuery(value.Scope)}", value)
            : Results.Ok(value);
    }

    private static IResult ChallengeResponse(HttpContext context, HypothesisScope scope, HypothesisChallenge value, bool created = false)
    {
        context.Response.Headers.ETag = PredictionLedgerService.FormatRevisionEtag(value.Version);
        return created
            ? Results.Created($"/api/hypotheses/{value.Hypothesis.HypothesisId:D}/revisions/{value.Hypothesis.Revision}/challenges/{value.ChallengeId:D}{ScopeQuery(scope)}", value)
            : Results.Ok(value);
    }

    private static string ScopeQuery(HypothesisScope scope) =>
        $"?fieldId={scope.FieldId:D}&reservoir={Uri.EscapeDataString(scope.ReservoirName)}" +
        (scope.ScenarioId is Guid scenario
            ? $"&scenarioId={scenario:D}&asOf={Uri.EscapeDataString(scope.AsOfUtc!.Value.ToString("O", CultureInfo.InvariantCulture))}"
            : string.Empty);

    private static HypothesisScope Scope(HttpRequest request)
    {
        if (!Guid.TryParseExact(Query(request, "fieldId"), "D", out Guid fieldId))
            throw HypothesisValidation.Invalid("fieldId is required as a canonical UUID query parameter.");
        string? scenarioText = Query(request, "scenarioId");
        Guid? scenarioId = scenarioText is null ? null :
            Guid.TryParseExact(scenarioText, "D", out Guid id) ? id : throw HypothesisValidation.Invalid("Invalid scenarioId.");
        string? asOfText = Query(request, "asOf");
        DateTimeOffset? asOf = asOfText is null ? null :
            DateTimeOffset.TryParse(asOfText, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset instant)
                ? instant : throw HypothesisValidation.Invalid("Invalid asOf.");
        return HypothesisValidation.Scope(new(fieldId, Query(request, "reservoir")!, scenarioId, asOf));
    }

    private static string? Query(HttpRequest request, string name)
    {
        if (!request.Query.TryGetValue(name, out var values)) return null;
        if (values.Count != 1 || string.IsNullOrWhiteSpace(values[0]))
            throw HypothesisValidation.Invalid($"Supply one nonempty {name} query parameter.");
        return values[0];
    }

    private static int Page(HttpContext context, string name, int fallback) =>
        Query(context.Request, name) is not string text ? fallback :
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) ? value :
        throw HypothesisValidation.Invalid($"{name} must be an integer.");

    private static int? Match(HttpContext context)
    {
        var values = context.Request.Headers.IfMatch;
        return values.Count == 0 ? null : PredictionLedgerService.ParseIfMatchRevision(values.Count == 1 ? values[0] : string.Empty);
    }

    private static string Key(HttpContext context) => context.Request.Headers["Idempotency-Key"].ToString();

    private static async Task<T> Body<T>(HttpContext context)
    {
        const int maximum = 512 * 1024;
        if (!context.Request.HasJsonContentType() || context.Request.ContentLength > maximum)
            throw HypothesisValidation.Invalid("A JSON request of at most 512 KiB is required.");
        using var stream = new MemoryStream();
        byte[] buffer = new byte[8192];
        int read;
        while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) > 0)
        {
            if (stream.Length + read > maximum) throw HypothesisValidation.Invalid("The request exceeds 512 KiB.");
            stream.Write(buffer, 0, read);
        }
        try
        {
            return JsonSerializer.Deserialize<T>(stream.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web)
                { NumberHandling = JsonNumberHandling.Strict })
                ?? throw HypothesisValidation.Invalid("A request body is required.");
        }
        catch (JsonException)
        {
            throw HypothesisValidation.Invalid("Supply only the documented required JSON fields and value types.");
        }
    }
}
