using Microsoft.AspNetCore.Diagnostics;

namespace ReservoirSimulation.Domain;

internal sealed class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException)
            return false;
        logger.LogWarning(new EventId(8401, "ReservoirRequestMalformed"), exception,
            "Reservoir request {Method} {RequestPath} could not be parsed.",
            httpContext.Request.Method, httpContext.Request.Path);
        await Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid request",
            detail: "The request body or route values could not be parsed.")
            .ExecuteAsync(httpContext);
        return true;
    }
}
