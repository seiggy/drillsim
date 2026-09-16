using System.Text;
using System.Text.Json;

namespace DrillingOperations;

public static class SimulationSetupRoutes
{
    public static void MapSimulationSetup(this RouteGroupBuilder api)
    {
        RouteGroupBuilder group = api.MapGroup("/scenarios/{scenarioId:guid}/setup");
        group.AddEndpointFilter(async (invocation, next) =>
        {
            HttpContext context = invocation.HttpContext;
            context.Response.Headers.CacheControl = "no-store";
            try { return await next(invocation); }
            catch (SimulationSetupException exception)
            {
                context.RequestServices.GetRequiredService<ILogger<SimulationSetupCoordinator>>()
                    .LogWarning("Simulation setup rejected: {Reason}.", exception.Message);
                return Results.Problem(statusCode: exception.StatusCode, title: exception.Message);
            }
            catch (BindingVerificationException exception)
            {
                return Results.Problem(statusCode: exception.StatusCode, title: "The approved prediction or prepared model could not be verified.");
            }
            catch (Exception exception) when (exception is PersistenceIntegrityException or JsonException or FormatException)
            {
                context.RequestServices.GetRequiredService<ILogger<SimulationSetupCoordinator>>()
                    .LogError(exception, "Simulation setup integrity check failed.");
                return Results.Problem(statusCode: 409, title: "Simulation setup integrity check failed.");
            }
            catch (Exception exception) when (exception is HttpRequestException or Microsoft.Data.Sqlite.SqliteException ||
                exception is OperationCanceledException && !context.RequestAborted.IsCancellationRequested)
            {
                context.RequestServices.GetRequiredService<ILogger<SimulationSetupCoordinator>>()
                    .LogError(exception, "Simulation setup dependency unavailable.");
                return Results.Problem(statusCode: 503, title: "Simulation setup dependency unavailable. Retry the same attempt after checking status.");
            }
        });
        group.MapGet("", async (Guid scenarioId, SimulationSetupCoordinator coordinator, CancellationToken ct) =>
            Results.Ok(await coordinator.GetAsync(scenarioId, ct)));
        group.MapPost("", async (Guid scenarioId, HttpContext context, SimulationSetupCoordinator coordinator) =>
        {
            if (!context.Request.HasJsonContentType() || context.Request.ContentLength > 4096)
                throw new SimulationSetupException(400, "A bounded JSON simulation setup is required.");
            byte[] bytes = new byte[4097];
            int count = 0;
            while (count < bytes.Length)
            {
                int read = await context.Request.Body.ReadAsync(bytes.AsMemory(count), context.RequestAborted);
                if (read == 0) break;
                count += read;
            }
            if (count > 4096) throw new SimulationSetupException(400, "Simulation setup exceeds 4 KiB.");
            PrepareSimulationRequest request;
            try
            {
                request = JsonSerializer.Deserialize<PrepareSimulationRequest>(bytes.AsSpan(0, count), CanonicalJson.SerializerOptions)
                    ?? throw new JsonException();
            }
            catch (JsonException) { throw new SimulationSetupException(400, "Supply only the documented simulation setup fields."); }
            string key = context.Request.Headers.TryGetValue("Idempotency-Key", out var values) && values.Count == 1 ? values[0] ?? "" : "";
            ApiOutcome result = await coordinator.PrepareAsync($"/drillingoperations/api/scenarios/{scenarioId:D}/setup",
                key, scenarioId, request, context.RequestAborted);
            return Results.Content(result.Body, "application/json", Encoding.UTF8, result.StatusCode);
        });
    }
}
