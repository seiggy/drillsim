using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            BadHttpRequestException request => new ProblemDetails
            {
                Status = request.StatusCode,
                Title = "Invalid HTTP request",
                Detail = "The request body or parameters do not match the endpoint contract or exceed its size limit."
            },
            UpstreamServiceException upstream => new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Upstream service request failed",
                Detail = upstream.Message,
                Extensions =
                {
                    ["service"] = upstream.Service,
                    ["url"] = upstream.RequestUri.ToString(),
                    ["upstreamStatus"] = upstream.StatusCode is null ? null : (int)upstream.StatusCode.Value
                }
            },
            ScenarioApiException scenario => new ProblemDetails
            {
                Status = scenario.StatusCode,
                Title = scenario.Title,
                Detail = scenario.Message
            },
            _ => null
        };
        if (problem is null)
            return false;

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
