using System.Text;
using System.Text.Json;

namespace DrillingOperations;

public static class ScoringCorrectionRoutes
{
    public static void MapScoringCorrection(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/scenarios/{scenarioId}/runs/{runId}");
        group.AddEndpointFilter(async (invocation, next) =>
        {
            var context = invocation.HttpContext;
            context.Response.Headers.CacheControl = "no-store";
            try { return await next(invocation); }
            catch (ScoringCorrectionException e)
            {
                context.RequestServices.GetRequiredService<ILogger<ScoringCorrectionCoordinator>>()
                    .LogWarning(new EventId(8301, "ScoringCorrectionRequestDenied"), "Scoring correction rejected: {DiagnosticCode}.", e.Code);
                return Results.Problem(statusCode: e.StatusCode, title: e.Code);
            }
            catch (Exception e) when (!context.RequestAborted.IsCancellationRequested)
            {
                string errorType = new(e.GetType().Name.Where(char.IsAsciiLetterOrDigit).Take(80).ToArray());
                context.RequestServices.GetRequiredService<ILogger<ScoringCorrectionCoordinator>>()
                    .LogError(new EventId(8302, "ScoringCorrectionUnavailable"), "Scoring correction unavailable with error type {ErrorType}.", errorType);
                return Results.Problem(statusCode: 503, title: "ScoringCorrectionUnavailable");
            }
        });
        group.MapGet("/scoring-correction", async (
            string scenarioId, string runId, ScoringCorrectionCoordinator coordinator, CancellationToken ct) =>
            Results.Ok(await coordinator.ReviewAsync(scenarioId, runId, ct)));
        group.MapPost("/correct-score", async (
            string scenarioId, string runId, HttpContext context, ScoringCorrectionCoordinator coordinator) =>
        {
            if (!context.Request.HasJsonContentType() || context.Request.ContentLength > 4096)
                throw new ScoringCorrectionException(400, "ScoringCorrectionRequestInvalid");
            byte[] bytes = new byte[4097]; int count = 0;
            while (count < bytes.Length)
            {
                int read = await context.Request.Body.ReadAsync(bytes.AsMemory(count), context.RequestAborted);
                if (read == 0) break;
                count += read;
            }
            if (count > 4096) throw new ScoringCorrectionException(400, "ScoringCorrectionRequestInvalid");
            CorrectScoreRequest body;
            try { body = JsonSerializer.Deserialize<CorrectScoreRequest>(bytes.AsSpan(0, count), CanonicalJson.SerializerOptions) ?? throw new JsonException(); }
            catch (JsonException) { throw new ScoringCorrectionException(400, "ScoringCorrectionRequestInvalid"); }
            string key = context.Request.Headers.TryGetValue("Idempotency-Key", out var values) && values.Count == 1 ? values[0] ?? "" : "";
            ApiOutcome outcome = await coordinator.CorrectAsync(
                $"/drillingoperations/api/scenarios/{scenarioId}/runs/{runId}/correct-score",
                key, scenarioId, runId, body, context.RequestAborted);
            return Results.Content(outcome.Body, "application/json", Encoding.UTF8, outcome.StatusCode);
        });
        api.MapGet("/runs/{runId}/scorecards/{scorecardId}", async Task<IResult> (
            string runId, string scorecardId, HttpResponse response, DrillingOperationsStore store, CancellationToken ct) =>
        {
            response.Headers.CacheControl = "no-store";
            if (!RequestValidation.IsCanonicalGuid(runId, out _) || !RequestValidation.IsCanonicalGuid(scorecardId, out _))
                return Results.Problem(statusCode: 400, title: "Canonical scorecard and run identifiers are required");
            ScorecardDraft? artifact = await store.GetScorecardArtifactAsync(runId, scorecardId, ct);
            return artifact is null ? Results.NotFound() : Results.Content(CanonicalJson.Serialize(artifact), "application/json", Encoding.UTF8);
        });
    }
}
