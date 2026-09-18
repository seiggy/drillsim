using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Persistence;

namespace ReservoirSimulation.Tests;

[TestFixture, NonParallelizable]
public sealed class RequestDiagnosticsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [TestCase(ApprovedPathKind.Planned)]
    [TestCase(ApprovedPathKind.AsDrilled)]
    public async Task RejectedPath_LogsSpecificFieldsOnTheRequestTraceWithoutRegisteringABinding(ApprovedPathKind kind)
    {
        var logs = new CapturingLogger();
        await using var factory = new ApiSecurityTests.ReservoirApiFactory();
        await using var configured = factory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.AddProvider(logs)));
        using HttpClient client = configured.CreateClient();
        client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", ApiSecurityTests.OperatorKey);
        WorldGenerationRequest definition = TestData.UniformWorldRequest(4, 3, 2);
        ReservoirWorld local = new ReservoirWorldFactory().Create(definition);
        using HttpResponseMessage created = await client.PostAsJsonAsync("/reservoirsimulation/api/worlds", definition, JsonOptions);
        created.EnsureSuccessStatusCode();
        WorldSummary world = (await created.Content.ReadFromJsonAsync<WorldSummary>())!;
        var request = new ApprovedPathBindingRequest
        {
            ScenarioId = Guid.NewGuid(), RunId = Guid.NewGuid(), PathKind = kind,
            ApprovedSealedPredictionSha256 = new string('a', 64),
            Stations =
            [
                new() { MeasuredDepthM = 0, TrueVerticalDepthM = 0,
                    EastingM = local.Grid.OriginEastingM + 5, NorthingM = local.Grid.OriginNorthingM - 760 },
                new() { MeasuredDepthM = 2_200, TrueVerticalDepthM = 1_025,
                    EastingM = local.Grid.OriginEastingM + 5, NorthingM = local.Grid.OriginNorthingM - 760 }
            ]
        };
        string traceId = ActivityTraceId.CreateRandom().ToString();
        using var post = new HttpRequestMessage(HttpMethod.Post, $"/reservoirsimulation/api/worlds/{world.WorldId}/path-bindings")
        { Content = JsonContent.Create(request, options: JsonOptions) };
        post.Headers.Add("traceparent", $"00-{traceId}-{ActivitySpanId.CreateRandom()}-01");
        using HttpResponseMessage response = await client.SendAsync(post);
        using JsonDocument problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        CapturingLogger.Entry log = logs.Entries.Single(entry => entry.Event.Id == 8400);
        var fields = log.Properties.ToDictionary(item => item.Key, item => item.Value);
        using JsonDocument errors = JsonDocument.Parse((string)fields["ValidationErrors"]!);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(problem.RootElement.GetProperty("diagnosticCode").GetString(), Is.EqualTo("PathOutsideModelCoverage"));
            Assert.That(errors.RootElement.GetProperty("stations[0].northingM")[0].GetString(),
                Is.EqualTo("Path station is outside the hidden world northing extent."));
            Assert.That(fields["DiagnosticCode"], Is.EqualTo("PathOutsideModelCoverage"));
            Assert.That(log.Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(log.Context?.TraceId.ToString(), Is.EqualTo(traceId));
            Assert.That(log.Message, Does.Not.Contain(ApiSecurityTests.OperatorKey).And.Not.Contain("X-DrillSim-Operator-Key"));
        });
        Assert.That(await configured.Services.GetRequiredService<SqliteTruthSamplingRepository>()
            .WorldHasBindingsAsync(world.WorldId), Is.False);
    }

    [Test]
    public async Task MalformedBody_LogsTheParseExceptionButReturnsTheExistingSafeProblem()
    {
        var logs = new CapturingLogger();
        await using var factory = new ApiSecurityTests.ReservoirApiFactory();
        await using var configured = factory.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.AddProvider(logs)));
        using HttpClient client = configured.CreateClient();
        client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", ApiSecurityTests.OperatorKey);
        using var body = new StringContent("""{"pathKind":12345}""", Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync(
            "/reservoirsimulation/api/worlds/unknown/path-bindings", body);
        CapturingLogger.Entry log = logs.Entries.Single(entry => entry.Event.Id == 8401);
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(log.Exception, Is.Not.Null);
            Assert.That(log.Level, Is.EqualTo(LogLevel.Warning));
        });
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("could not be parsed").And.Not.Contain("12345"));
    }

    private sealed class CapturingLogger : ILoggerProvider, ILogger
    {
        internal sealed record Entry(LogLevel Level, EventId Event, Exception? Exception, string Message,
            KeyValuePair<string, object?>[] Properties, ActivityContext? Context);
        internal ConcurrentQueue<Entry> Entries { get; } = [];
        public ILogger CreateLogger(string categoryName) => this;
        public void Dispose() { }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (id.Id is 8400 or 8401)
                Entries.Enqueue(new(level, id, exception, formatter(state, exception),
                    state is IEnumerable<KeyValuePair<string, object?>> fields ? fields.ToArray() : [],
                    Activity.Current?.Context));
        }
    }
}
