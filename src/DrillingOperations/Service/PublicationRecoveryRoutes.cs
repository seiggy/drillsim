using System.Text;
using System.Text.Json;

namespace DrillingOperations;

public static class PublicationRecoveryRoutes
{
    public static void MapPublicationRecovery(this RouteGroupBuilder api)
    {
        RouteGroupBuilder recovery = api.MapGroup("/scenarios/{scenarioId}/runs/{runId}");
        recovery.AddEndpointFilter(async (invocation, next) =>
        {
            HttpContext context = invocation.HttpContext;
            context.Response.Headers.CacheControl = "no-store";
            try { return await next(invocation); }
            catch (PublicationRecoveryException e)
            {
                context.RequestServices.GetRequiredService<ILogger<PublicationRecoveryCoordinator>>()
                    .LogWarning(new EventId(8201, "PublicationRecoveryRequestDenied"),
                        "Publication recovery request denied: {DiagnosticCode}.", e.Code);
                return Results.Problem(statusCode: e.StatusCode, title: e.Code);
            }
            catch (Exception e) when (e is PersistenceIntegrityException or JsonException or FormatException or InvalidOperationException)
            {
                context.RequestServices.GetRequiredService<ILogger<PublicationRecoveryCoordinator>>()
                    .LogWarning(new EventId(8202, "PublicationRecoveryIntegrityFailure"),
                        "Publication recovery integrity check failed.");
                return Results.Problem(statusCode: 409, title: "PublicationRecoveryIntegrityMismatch");
            }
            catch (Exception e) when (e is HttpRequestException or Microsoft.Data.Sqlite.SqliteException ||
                e is OperationCanceledException && !context.RequestAborted.IsCancellationRequested)
            {
                context.RequestServices.GetRequiredService<ILogger<PublicationRecoveryCoordinator>>()
                    .LogWarning(new EventId(8203, "PublicationRecoveryUnavailable"),
                        "Publication recovery dependency or persistence check unavailable.");
                return Results.Problem(statusCode: 503, title: "PublicationRecoveryDependencyUnavailable");
            }
            catch (Exception e) when (!context.RequestAborted.IsCancellationRequested)
            {
                string errorType = new(e.GetType().Name.Where(char.IsAsciiLetterOrDigit).Take(80).ToArray());
                context.RequestServices.GetRequiredService<ILogger<PublicationRecoveryCoordinator>>()
                    .LogError(new EventId(8204, "PublicationRecoveryUnhandledFailure"),
                        "Publication recovery unavailable with error type {ErrorType}.", errorType);
                return Results.Problem(statusCode: 503, title: "PublicationRecoveryUnavailable");
            }
        });
        recovery.MapGet("/publication-recovery", async (
            string scenarioId, string runId, PublicationRecoveryCoordinator coordinator, CancellationToken ct) =>
            Results.Ok(await coordinator.ReviewAsync(scenarioId, runId, ct)));
        recovery.MapPost("/recover-publication", async (
            string scenarioId, string runId, HttpContext context, PublicationRecoveryCoordinator coordinator) =>
        {
            if (!context.Request.HasJsonContentType() || context.Request.ContentLength > 4096)
                throw new PublicationRecoveryException(400, "PublicationRecoveryRequestInvalid");
            byte[] bytes = new byte[4097];
            int count = 0;
            while (count < bytes.Length)
            {
                int read = await context.Request.Body.ReadAsync(bytes.AsMemory(count), context.RequestAborted);
                if (read == 0) break;
                count += read;
            }
            if (count > 4096) throw new PublicationRecoveryException(400, "PublicationRecoveryRequestInvalid");
            RecoverPublicationRequest body;
            try
            {
                body = JsonSerializer.Deserialize<RecoverPublicationRequest>(bytes.AsSpan(0, count), CanonicalJson.SerializerOptions)
                    ?? throw new JsonException();
            }
            catch (JsonException) { throw new PublicationRecoveryException(400, "PublicationRecoveryRequestInvalid"); }
            string key = context.Request.Headers.TryGetValue("Idempotency-Key", out var values) && values.Count == 1
                ? values[0] ?? "" : "";
            ApiOutcome outcome = await coordinator.RecoverAsync(
                $"/drillingoperations/api/scenarios/{scenarioId}/runs/{runId}/recover-publication",
                key, scenarioId, runId, body, context.RequestAborted);
            return Results.Content(outcome.Body, "application/json", Encoding.UTF8, outcome.StatusCode);
        });
    }
}
