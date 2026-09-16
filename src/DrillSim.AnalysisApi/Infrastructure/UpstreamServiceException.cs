using System.Net;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed class UpstreamServiceException : Exception
{
    public UpstreamServiceException(string service, Uri requestUri, HttpStatusCode? statusCode, string detail, Exception? innerException = null)
        : base($"{service} request to {requestUri} failed: {detail}", innerException)
    {
        Service = service;
        RequestUri = requestUri;
        StatusCode = statusCode;
    }

    public string Service { get; }
    public Uri RequestUri { get; }
    public HttpStatusCode? StatusCode { get; }
}
