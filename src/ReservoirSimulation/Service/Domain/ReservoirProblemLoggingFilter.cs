using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ReservoirSimulation.Domain;

internal sealed class ReservoirProblemLoggingFilter(ILogger<ReservoirProblemLoggingFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        object? result = await next(context);
        if (result is IValueHttpResult { Value: ProblemDetails { Status: >= 400 } problem })
        {
            problem.Extensions.TryGetValue("diagnosticCode", out object? diagnosticCode);
            string? errors = problem is HttpValidationProblemDetails validation
                ? JsonSerializer.Serialize(validation.Errors) : null;
            logger.Log(problem.Status >= 500 ? LogLevel.Error : LogLevel.Warning,
                new EventId(8400, "ReservoirRequestRejected"),
                "Reservoir request {Method} {RequestPath} rejected with HTTP {StatusCode}: {ProblemTitle}. " +
                "{ProblemDetail} Diagnostic: {DiagnosticCode}. Validation errors: {ValidationErrors}",
                context.HttpContext.Request.Method, context.HttpContext.Request.Path,
                problem.Status, problem.Title, problem.Detail, diagnosticCode, errors);
        }
        return result;
    }
}
