using System.ClientModel;
using System.Text.Json;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace DrillSim.AnalysisApi.Infrastructure;

public static class FormationInterpretationEndpoints
{
#pragma warning disable OPENAI001 // Responses supports reasoning with function tools.
    public static IServiceCollection AddFormationInterpretation(
        this IServiceCollection services, ResponsesClient? responseClient, string? deploymentName, bool enableSensitiveData = false)
    {
        services.AddSingleton(new FormationInterpretationAgent(
            responseClient is null ? null : options => responseClient.AsAIAgent(options, model: deploymentName,
                clientFactory: client => client.AsBuilder()
                    .UseFunctionInvocation(configure: invocation =>
                    {
                        invocation.MaximumIterationsPerRequest = FormationInterpretationTools.MaximumModelIterations;
                        invocation.MaximumConsecutiveErrorsPerRequest = 2;
                        invocation.AllowConcurrentInvocation = false;
                        invocation.TerminateOnUnknownCalls = true;
                    })
                    .UseOpenTelemetry(sourceName: FormationInterpretationAgent.TelemetrySourceName,
                        configure: telemetry => telemetry.EnableSensitiveData = enableSensitiveData)
                    .Build()),
            enableSensitiveData));
        services.AddSingleton<FormationInterpretationService>();
        services.AddSingleton<ForecastDraftService>();
        services.AddSingleton<ProspectBriefService>();
        return services;
    }
#pragma warning restore OPENAI001

    public static IEndpointRouteBuilder MapFormationInterpretation(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/formation-interpretation");
        group.WithAgentErrorHandling("Formation interpretation", new EventId(8300, "FormationInterpretationFailure"));
        group.MapGet("/status", (FormationInterpretationService service) => Results.Ok(service.Status));
        group.MapPost("", async (HttpContext context, FormationInterpretationService service) =>
            Results.Ok(await service.DraftAsync(
                await ReadBodyAsync<FormationInterpretationRequest>(context, "formation interpretation", FormationInterpretationAgent.JsonOptions), context.RequestAborted)))
            .RequireLocalOperatorMutation();
        return endpoints;
    }

    public static IEndpointRouteBuilder MapForecastDrafts(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/forecast-drafts");
        group.WithAgentErrorHandling("Forecast draft", new EventId(8310, "ForecastDraftFailure"));
        group.MapGet("/status", (ForecastDraftService service) => Results.Ok(service.Status));
        group.MapPost("", async (HttpContext context, ForecastDraftService service) =>
            Results.Ok(await service.DraftAsync(
                await ReadBodyAsync<ForecastDraftRequest>(context, "forecast draft", ForecastDraftAgent.JsonOptions), context.RequestAborted)))
            .RequireLocalOperatorMutation();
        return endpoints;
    }

    public static IEndpointRouteBuilder MapProspectBriefs(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/prospect-briefs");
        group.WithAgentErrorHandling("Prospect brief", new EventId(8320, "ProspectBriefFailure"));
        group.MapGet("/status", (ProspectBriefService service) => Results.Ok(service.Status));
        group.MapPost("", async (HttpContext context, ProspectBriefService service) =>
            Results.Ok(await service.ExplainAsync(
                await ReadBodyAsync<ProspectBriefRequest>(context, "prospect brief", ProspectBriefAgent.JsonOptions), context.RequestAborted)))
            .RequireLocalOperatorMutation();
        return endpoints;
    }

    private static void WithAgentErrorHandling(this RouteGroupBuilder group, string subject, EventId eventId)
    {
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
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                return Results.StatusCode(499);
            }
            catch (Exception exception)
            {
                Exception diagnosticException = exception is FormationInterpretationModelException model
                    ? model.OriginalException : exception;
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger($"DrillSim.AnalysisApi.{eventId.Name}")
                    .LogError(eventId, diagnosticException,
                        "{Subject} failed with {ErrorType} (provider status {ProviderStatusCode}).",
                        subject, diagnosticException.GetType().Name, (diagnosticException as ClientResultException)?.Status);
                if (exception is FormationInterpretationModelException { RateLimited: true })
                    return Results.Problem(statusCode: 429, title: "AI capacity limit reached",
                        detail: "The AI service cannot accept this request right now. Try again after its quota or capacity is available. Your notes are unchanged; no automatic retry was made.");
                bool evidenceUnavailable = exception is InvalidDataException or SqliteException;
                return Results.Problem(statusCode: evidenceUnavailable ? 503 : 502,
                    title: evidenceUnavailable ? $"{subject} evidence unavailable" : $"{subject} AI unavailable",
                    detail: "The evidence or model response could not be verified. Nothing was saved; no automatic inference retry was attempted.");
            }
        });
    }

    private static async Task<T> ReadBodyAsync<T>(HttpContext context, string subject, JsonSerializerOptions options) where T : class
    {
        const int maximum = 128 * 1024;
        if (!context.Request.HasJsonContentType() || context.Request.ContentLength > maximum)
            throw HypothesisValidation.Invalid($"A JSON {subject} request of at most 128 KiB is required.");
        byte[] bytes = new byte[maximum + 1];
        int count = 0;
        while (count < bytes.Length)
        {
            int read = await context.Request.Body.ReadAsync(bytes.AsMemory(count), context.RequestAborted);
            if (read == 0) break;
            count += read;
        }
        if (count > maximum) throw HypothesisValidation.Invalid($"The {subject} request exceeds 128 KiB.");
        try
        {
            return JsonSerializer.Deserialize<T>(bytes.AsSpan(0, count), options)
                ?? throw HypothesisValidation.Invalid($"A {subject} request is required.");
        }
        catch (JsonException)
        {
            throw HypothesisValidation.Invalid($"Supply only the documented {subject} JSON fields and value types.");
        }
    }
}
