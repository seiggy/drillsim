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
        return services;
    }
#pragma warning restore OPENAI001

    public static IEndpointRouteBuilder MapFormationInterpretation(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/formation-interpretation");
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
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("DrillSim.AnalysisApi.FormationInterpretation")
                    .LogError(new EventId(8300, "FormationInterpretationFailure"), diagnosticException,
                        "Formation interpretation failed with {ErrorType} (provider status {ProviderStatusCode}).",
                        diagnosticException.GetType().Name, (diagnosticException as ClientResultException)?.Status);
                if (exception is FormationInterpretationModelException { RateLimited: true })
                    return Results.Problem(statusCode: 429, title: "AI capacity limit reached",
                        detail: "The AI service cannot accept this request right now. Try again after its quota or capacity is available. Your notes are unchanged; no automatic retry was made.");
                bool evidenceUnavailable = exception is InvalidDataException or SqliteException;
                return Results.Problem(statusCode: evidenceUnavailable ? 503 : 502,
                    title: evidenceUnavailable ? "Formation interpretation evidence unavailable" : "Formation interpretation AI unavailable",
                    detail: "The evidence or model response could not be verified. No interpretation was saved; no automatic inference retry was attempted.");
            }
        });
        group.MapGet("/status", (FormationInterpretationService service) => Results.Ok(service.Status));
        group.MapPost("", async (HttpContext context, FormationInterpretationService service) =>
            Results.Ok(await service.DraftAsync(await ReadBodyAsync(context), context.RequestAborted)))
            .RequireLocalOperatorMutation();
        return endpoints;
    }

    private static async Task<FormationInterpretationRequest> ReadBodyAsync(HttpContext context)
    {
        const int maximum = 128 * 1024;
        if (!context.Request.HasJsonContentType() || context.Request.ContentLength > maximum)
            throw HypothesisValidation.Invalid("A JSON formation interpretation request of at most 128 KiB is required.");
        byte[] bytes = new byte[maximum + 1];
        int count = 0;
        while (count < bytes.Length)
        {
            int read = await context.Request.Body.ReadAsync(bytes.AsMemory(count), context.RequestAborted);
            if (read == 0) break;
            count += read;
        }
        if (count > maximum) throw HypothesisValidation.Invalid("The formation interpretation request exceeds 128 KiB.");
        try
        {
            return JsonSerializer.Deserialize<FormationInterpretationRequest>(bytes.AsSpan(0, count), FormationInterpretationAgent.JsonOptions)
                ?? throw HypothesisValidation.Invalid("A formation interpretation request is required.");
        }
        catch (JsonException)
        {
            throw HypothesisValidation.Invalid("Supply only the documented formation interpretation JSON fields and value types.");
        }
    }
}
