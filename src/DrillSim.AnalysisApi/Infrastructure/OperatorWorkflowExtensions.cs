using System.Net;
using System.Text.Json;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Primitives;

namespace DrillSim.AnalysisApi.Infrastructure;

public static class OperatorWorkflowExtensions
{
    public const string CsrfHeaderName = "X-DrillSim-CSRF";
    public const string AuditLabelLimitation =
        "Single-user local operator only. The actor is a human-supplied audit label, not an authenticated identity. " +
        "Prediction and completion approvals, simulator setup, guarded publication recovery, and score correction store this label; other simulator actions retain their existing run audit records without actor attribution.";

    public static IServiceCollection AddLocalOperatorWorkflow(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(provider => OperatorConfiguration.Read(
            configuration, provider.GetRequiredService<IHostEnvironment>()));
        services.AddAntiforgery(options =>
        {
            options.HeaderName = CsrfHeaderName;
            options.Cookie.Name = "DrillSim.LocalOperator.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            // Local Aspire/Vite may use HTTP. SameAsRequest also protects the HTTPS deployment.
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.Path = "/";
        });
#pragma warning disable EXTEXP0001
        services.AddHttpClient(OperatorBackendClient.ClientName, (provider, client) =>
        {
            OperatorConfiguration settings = provider.GetRequiredService<OperatorConfiguration>();
            client.BaseAddress = settings.BackendUri;
            client.Timeout = TimeSpan.FromMinutes(2);
            client.MaxResponseContentBufferSize = 1024 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            UseCookies = false
        }).RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001
        services.TryAddSingleton<IOperatorLedger, OperatorLedger>();
        services.AddSingleton<OperatorBackendClient>();
        services.AddSingleton<OperatorWorkflowService>();
        return services;
    }

    public static IEndpointRouteBuilder MapLocalOperatorWorkflow(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/operator");
        group.AddEndpointFilter(GuardAsync);
        group.MapGet("/session", (HttpContext context, OperatorConfiguration settings, IAntiforgery antiforgery) =>
        {
            string? token = settings.Enabled ? antiforgery.GetAndStoreTokens(context).RequestToken : null;
            return Results.Ok(new OperatorSession(settings.Enabled, settings.Reason, token, CsrfHeaderName, AuditLabelLimitation));
        });
        group.MapGet("/scenarios/{scenarioId:guid}", async (Guid scenarioId, HttpContext context) =>
        {
            OperatorConfiguration settings = context.RequestServices.GetRequiredService<OperatorConfiguration>();
            if (!settings.Enabled)
                return Results.Ok(OperatorWorkflowService.Disabled(scenarioId, settings.Reason!));
            return Results.Ok(await Workflow(context).GetAsync(scenarioId, context.RequestAborted));
        });
        group.MapPost("/scenarios/{scenarioId:guid}/approve-prediction", async (Guid scenarioId, HttpContext context) =>
        {
            var request = await ReadBodyAsync<OperatorPredictionApprovalRequest>(context);
            return Results.Ok(await Workflow(context).ApprovePredictionAsync(
                scenarioId, request.Actor, request.ReviewedSealHash, context.RequestAborted));
        });
        group.MapPost("/scenarios/{scenarioId:guid}/runs", async (Guid scenarioId, HttpContext context) =>
        {
            var request = await ReadBodyAsync<OperatorActorRequest>(context);
            return Results.Ok(await Workflow(context).StartAsync(
                scenarioId, request.Actor, IdempotencyKey(context), context.RequestAborted));
        });
        group.MapGet("/scenarios/{scenarioId:guid}/setup", async (Guid scenarioId, HttpContext context) =>
        {
            OperatorConfiguration settings = context.RequestServices.GetRequiredService<OperatorConfiguration>();
            if (!settings.Enabled)
                return Results.Ok(new OperatorSimulationSetup(scenarioId, false, settings.Reason, false, null,
                    [], new(null, "Preview", 0), null));
            return Results.Ok(await Workflow(context).GetSimulationSetupAsync(scenarioId, context.RequestAborted));
        });
        group.MapPost("/scenarios/{scenarioId:guid}/setup", async (Guid scenarioId, HttpContext context) =>
        {
            var request = await ReadBodyAsync<OperatorSimulationSetupRequest>(context);
            return Results.Ok(await Workflow(context).PrepareSimulationAsync(
                scenarioId, request, IdempotencyKey(context), context.RequestAborted));
        });
        foreach (string action in new[] { "cancel", "resume", "publish", "score" })
        {
            string capturedAction = action;
            group.MapPost($"/scenarios/{{scenarioId:guid}}/runs/{{runId:guid}}/{action}",
                async (Guid scenarioId, Guid runId, HttpContext context) =>
                {
                    var request = await ReadBodyAsync<OperatorActorRequest>(context);
                    return Results.Ok(await Workflow(context).ActAsync(
                        scenarioId, runId, capturedAction, request.Actor,
                        IdempotencyKey(context), context.RequestAborted));
                });
        }
        group.MapPost("/scenarios/{scenarioId:guid}/runs/{runId:guid}/approve-completion",
            async (Guid scenarioId, Guid runId, HttpContext context) =>
            {
                var request = await ReadBodyAsync<OperatorCompletionApprovalRequest>(context);
                return Results.Ok(await Workflow(context).ApproveCompletionAsync(
                    scenarioId, runId, request.Actor, request.ReviewedOpeningHash,
                    IdempotencyKey(context), context.RequestAborted));
            });
        group.MapGet("/scenarios/{scenarioId:guid}/runs/{runId:guid}/publication-recovery",
            async (Guid scenarioId, Guid runId, HttpContext context) =>
                Results.Ok(await Workflow(context).GetPublicationRecoveryAsync(scenarioId, runId, context.RequestAborted)));
        group.MapPost("/scenarios/{scenarioId:guid}/runs/{runId:guid}/recover-publication",
            async (Guid scenarioId, Guid runId, HttpContext context) =>
            {
                var request = await ReadBodyAsync<OperatorPublicationRecoveryRequest>(context);
                return Results.Ok(await Workflow(context).RecoverPublicationAsync(
                    scenarioId, runId, request, IdempotencyKey(context), context.RequestAborted));
            });
        group.MapGet("/scenarios/{scenarioId:guid}/runs/{runId:guid}/scoring-correction",
            async (Guid scenarioId, Guid runId, HttpContext context) =>
                Results.Ok(await Workflow(context).GetScoringCorrectionAsync(scenarioId, runId, context.RequestAborted)));
        group.MapPost("/scenarios/{scenarioId:guid}/runs/{runId:guid}/correct-score",
            async (Guid scenarioId, Guid runId, HttpContext context) =>
            {
                var request = await ReadBodyAsync<OperatorScoringCorrectionRequest>(context);
                return Results.Ok(await Workflow(context).CorrectScoreAsync(scenarioId, runId, request,
                    IdempotencyKey(context), context.RequestAborted));
            });
        return endpoints;
    }

    public static RouteHandlerBuilder RequireLocalOperatorMutation(this RouteHandlerBuilder endpoint) =>
        endpoint.AddEndpointFilter(async (invocation, next) =>
        {
            IResult? denial = await ValidateLocalRequestAsync(invocation.HttpContext, mutation: true);
            // Preserve the attached handler's existing results and application exception handling.
            return denial is not null ? denial : await next(invocation);
        });

    private static OperatorWorkflowService Workflow(HttpContext context) =>
        context.RequestServices.GetRequiredService<OperatorWorkflowService>();

    private static async Task<IResult?> ValidateLocalRequestAsync(HttpContext context, bool mutation)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        OperatorConfiguration settings = context.RequestServices.GetRequiredService<OperatorConfiguration>();
        if (!IsLocalRequest(context, settings, mutation))
            return Results.Problem(statusCode: 403, title: "Local operator request denied",
                detail: "Use the configured loopback browser origin on the owner-local development machine.");
        if (mutation && !settings.Enabled)
            return Results.Problem(statusCode: 503, title: "Local operator unavailable", detail: settings.Reason);
        try
        {
            if (mutation)
            {
                if (!context.Request.Headers.TryGetValue(CsrfHeaderName, out StringValues token) ||
                    token.Count != 1 || string.IsNullOrWhiteSpace(token[0]))
                    return Results.Problem(statusCode: 400, title: "Antiforgery token required");
                await context.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(context);
                _ = IdempotencyKey(context);
            }
            return null;
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Problem(statusCode: 400, title: "Antiforgery validation failed");
        }
        catch (OperatorWorkflowException exception)
        {
            return Results.Problem(statusCode: exception.StatusCode, title: exception.Message);
        }
    }

    private static async ValueTask<object?> GuardAsync(
        EndpointFilterInvocationContext invocation, EndpointFilterDelegate next)
    {
        HttpContext context = invocation.HttpContext;
        try
        {
            IResult? denial = await ValidateLocalRequestAsync(context, !HttpMethods.IsGet(context.Request.Method));
            return denial is not null ? denial : await next(invocation);
        }
        catch (OperatorWorkflowException exception)
        {
            return Results.Problem(statusCode: exception.StatusCode, title: exception.Message);
        }
        catch (BadHttpRequestException)
        {
            return Results.Problem(statusCode: 400, title: "Invalid operator request");
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception exception)
        {
            string errorType = new(exception.GetType().Name.Where(char.IsAsciiLetterOrDigit).Take(80).ToArray());
            context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DrillSim.AnalysisApi.LocalOperator")
                .LogError(new EventId(8100, "LocalOperatorUnhandledFailure"),
                    "Local operator operation failed with error type {ErrorType}.", errorType);
            return Results.Problem(statusCode: 503, title: "Local operator operation unavailable",
                detail: "Reload the operator view before retrying with the same action key. No rollback is implied.");
        }
    }

    internal static bool IsLocalRequest(HttpContext context, OperatorConfiguration settings, bool mutation)
    {
        IPAddress? remote = context.Connection.RemoteIpAddress;
        if (remote is null || !IPAddress.IsLoopback(remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote) ||
            !OperatorConfiguration.IsLoopbackHost(context.Request.Host.Host))
            return false;
        if (context.Request.Headers.TryGetValue("Sec-Fetch-Site", out var fetchSite) &&
            (fetchSite.Count != 1 || fetchSite[0] is not ("same-origin" or "same-site" or "none")))
            return false;
        if (!context.Request.Headers.TryGetValue("Origin", out var origins))
            return !mutation;
        if (origins.Count != 1 || !OperatorConfiguration.TryOrigin(origins[0], out Uri? origin))
            return false;
        string actualOrigin = $"{context.Request.Scheme}://{context.Request.Host}";
        return string.Equals(origin!.GetLeftPart(UriPartial.Authority), actualOrigin, StringComparison.OrdinalIgnoreCase) ||
            (settings.BrowserOrigin is not null && origin == settings.BrowserOrigin);
    }

    private static string IdempotencyKey(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var values) ||
            values.Count != 1 || values[0] is not string key ||
            key.Length is < 1 or > 128 || key.Any(c => c < 33 || c > 126))
            throw new OperatorWorkflowException(400, "A bounded stable Idempotency-Key is required for each action.");
        return key;
    }

    private static async Task<T> ReadBodyAsync<T>(HttpContext context)
    {
        if (!context.Request.HasJsonContentType() || context.Request.ContentLength > 4096)
            throw new OperatorWorkflowException(400, "A JSON operator request of at most 4096 bytes is required.");
        byte[] bytes = new byte[4097];
        int count = 0;
        while (count < bytes.Length)
        {
            int read = await context.Request.Body.ReadAsync(bytes.AsMemory(count), context.RequestAborted);
            if (read == 0) break;
            count += read;
        }
        if (count > 4096)
            throw new OperatorWorkflowException(400, "Operator request exceeds 4096 bytes.");
        try
        {
            return JsonSerializer.Deserialize<T>(bytes.AsSpan(0, count), new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? throw new OperatorWorkflowException(400, "An operator request is required.");
        }
        catch (JsonException)
        {
            throw new OperatorWorkflowException(400, "Invalid operator request. Supply only the documented JSON fields.");
        }
    }
}

public sealed record OperatorConfiguration(bool Enabled, string? Reason, Uri? BackendUri, string? Key, Uri? BrowserOrigin)
{
    internal static OperatorConfiguration Read(IConfiguration configuration, IHostEnvironment environment)
    {
        string? value = configuration["LocalOperator:BackendUrl"];
        string? key = configuration["LocalOperator:InternalKey"];
        string? browser = configuration["LocalOperator:BrowserOrigin"];
        string? reason = !environment.IsDevelopment() ? "Local operator workflow is available only in Development." : null;
        bool validUri = TryOrigin(value, out Uri? uri);
        if (!validUri || string.IsNullOrWhiteSpace(key) || key.Length > 1024 || key.Any(c => c < 33 || c > 126))
            reason ??= "Operator backend URL/key are not configured. Configure the local Drilling Operations connection.";
        Uri? browserOrigin = null;
        if (!string.IsNullOrEmpty(browser) && !TryOrigin(browser, out browserOrigin))
            reason ??= "The configured operator browser origin must be a loopback HTTP(S) origin.";
        return new(reason is null, reason, validUri ? uri : null, key, browserOrigin);
    }

    internal static bool TryOrigin(string? value, out Uri? uri)
    {
        uri = null;
        return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme is "http" or "https" &&
            IsLoopbackHost(uri.Host) && uri.UserInfo.Length == 0 && uri.Query.Length == 0 &&
            uri.Fragment.Length == 0 && uri.AbsolutePath == "/";
    }

    internal static bool IsLoopbackHost(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        (IPAddress.TryParse(host.Trim('[', ']'), out IPAddress? address) &&
         IPAddress.IsLoopback(address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address));
}

internal sealed class OperatorWorkflowException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
