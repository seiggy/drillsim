using DrillSim.AnalysisApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DrillSim.AnalysisApi.Tests;

public sealed class ApiExceptionHandlerTests
{
    [TestCase(400)]
    [TestCase(413)]
    public async Task InvalidRequest_PreservesClientErrorWithoutEchoingRequestContent(int status)
    {
        using ServiceProvider services = new ServiceCollection().AddOptions().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var error = new BadHttpRequestException("request-content-must-not-be-echoed", status);

        bool handled = await new ApiExceptionHandler().TryHandleAsync(context, error, CancellationToken.None);
        body.Position = 0;
        string json = await new StreamReader(body).ReadToEndAsync();

        Assert.Multiple(() =>
        {
            Assert.That(handled, Is.True);
            Assert.That(context.Response.StatusCode, Is.EqualTo(status));
            Assert.That(json, Does.Contain("Invalid HTTP request"));
            Assert.That(json, Does.Not.Contain("request-content-must-not-be-echoed"));
        });
    }
}
