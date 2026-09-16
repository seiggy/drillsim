using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Services;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace DrillSim.AnalysisApi.Tests;

public sealed class FieldPackageFailureTests
{
    [TestCase("timeout")]
    [TestCase("circuit")]
    [TestCase("http-timeout")]
    public void ResilienceFailures_PreserveUpstreamContextInsteadOfUnhandled500(string kind)
    {
        Exception failure = kind switch
        {
            "timeout" => new TimeoutRejectedException("attempt timed out"),
            "circuit" => new BrokenCircuitException("open circuit"),
            _ => new TaskCanceledException("HTTP timeout")
        };
        using var client = new HttpClient(new FailureHandler(failure)) { BaseAddress = new Uri("http://localhost:12345") };
        var service = new FieldPackageService(new Factory(client), new CanonicalJsonHasher(), TimeProvider.System);

        var error = Assert.ThrowsAsync<UpstreamServiceException>(() => service.GetFieldsAsync(CancellationToken.None))!;
        Assert.Multiple(() =>
        {
            Assert.That(error.Service, Is.EqualTo(FieldPackageService.FieldClient));
            Assert.That(error.InnerException, Is.SameAs(failure));
            Assert.That(error.RequestUri.AbsolutePath, Is.EqualTo("/field/api/Field/LightData"));
            Assert.That((int?)error.StatusCode, Is.AnyOf(503, 504));
        });
    }

    [Test]
    public void CallerCancellation_IsNotReportedAsAnUpstreamFailure()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var client = new HttpClient(new FailureHandler(new TaskCanceledException())) { BaseAddress = new Uri("http://localhost:12345") };
        var service = new FieldPackageService(new Factory(client), new CanonicalJsonHasher(), TimeProvider.System);
        Assert.CatchAsync<OperationCanceledException>(() => service.GetFieldsAsync(cancellation.Token));
    }

    private sealed class Factory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class FailureHandler(Exception failure) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromException<HttpResponseMessage>(failure);
    }
}
