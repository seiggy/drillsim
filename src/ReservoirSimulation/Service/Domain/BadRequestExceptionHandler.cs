using Microsoft.AspNetCore.Diagnostics;

namespace ReservoirSimulation.Domain;

internal sealed class BadRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException)
            return false;
        await Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid request",
            detail: "The request body or route values could not be parsed.")
            .ExecuteAsync(httpContext);
        return true;
    }
}
