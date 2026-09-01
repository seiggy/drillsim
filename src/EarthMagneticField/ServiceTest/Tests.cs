using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OSDC.Drilling.EarthMagneticField.ModelShared;
using OSDC.Drilling.EarthMagneticField.Service.Mcp;

namespace OSDC.Drilling.EarthMagneticField.ServiceTest;

public class Tests
{
    private WebApplicationFactory<Program> factory_ = null!;
    private HttpClient httpClient_ = null!;
    private Client generatedClient_ = null!;

    [SetUp]
    public void Setup()
    {
        factory_ = new WebApplicationFactory<Program>();
        httpClient_ = factory_.CreateClient();
        generatedClient_ = new Client("http://localhost/EarthMagneticField/api/", httpClient_);
    }

    [TearDown]
    public void TearDown()
    {
        httpClient_.Dispose();
        factory_.Dispose();
    }

    [TestCase(EarthMagneticFieldModel.WMM2025, 2026)]
    [TestCase(EarthMagneticFieldModel.IGRF14, 2020)]
    public async Task GeneratedClientEvaluatesBothModels(EarthMagneticFieldModel model, int year)
    {
        EvaluateEarthMagneticFieldRequest request = PseudoConstructors.ConstructEvaluateEarthMagneticFieldRequest();
        request.Model = model;
        EarthMagneticFieldEvaluationPoint input = request.Samples.First();
        input.Latitude = 0.5;
        input.Longitude = 1.0;
        input.Depth = 500;
        input.DateTimeUtc = new DateTimeOffset(year, 6, 1, 0, 0, 0, TimeSpan.Zero);

        EvaluateEarthMagneticFieldResponse response = await generatedClient_.EvaluateEarthMagneticFieldAsync(request);
        Assert.Multiple(() =>
        {
            Assert.That(response.Samples, Has.Count.EqualTo(1));
            Assert.That(response.Samples.First().TotalIntensity, Is.GreaterThan(1e-6));
            Assert.That(response.Samples.First().TotalIntensity, Is.LessThan(1e-3));
            Assert.That(response.Samples.First().Input.DateTimeUtc.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(response.Model.Model, Is.EqualTo(model));
        });
    }

    [Test]
    public void InvalidRequestReturnsUnprocessableEntity()
    {
        EvaluateEarthMagneticFieldRequest request = PseudoConstructors.ConstructEvaluateEarthMagneticFieldRequest();
        request.Samples.First().DateTimeUtc = new DateTimeOffset(2026, 1, 1, 1, 0, 0, TimeSpan.FromHours(1));
        ApiException exception = Assert.CatchAsync<ApiException>(async () =>
            await generatedClient_.EvaluateEarthMagneticFieldAsync(request))!;
        Assert.That(exception.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task OffsetlessTimestampIsRejectedIndependentlyOfServerTimeZone()
    {
        const string body = """
            {"Model":"WMM2025","Samples":[{"Latitude":0.5,"Longitude":1.0,"Depth":500.0,"DateTimeUtc":"2026-06-01T00:00:00"}]}
            """;
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient_.PostAsync(
            "/EarthMagneticField/api/EarthMagneticField/Evaluate", content);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [TestCase("/EarthMagneticField/api/EarthMagneticField")]
    [TestCase("/earthmagneticfield/api/earthmagneticfield")]
    public async Task EntryEndpointReturnsBothModelsAndConventions(string path)
    {
        HttpResponseMessage response = await httpClient_.GetAsync(path);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(document.RootElement.GetProperty("CoordinateFrame").GetString(), Is.EqualTo("north-east-down"));
            Assert.That(document.RootElement.GetProperty("TimeConvention").GetString(), Is.EqualTo("UTC"));
            Assert.That(document.RootElement.GetProperty("Models").GetArrayLength(), Is.EqualTo(2));
        });
    }

    [Test]
    public void UsageStatisticsAreNotRegisteredAsMcpTools()
    {
        string[] names = factory_.Services.GetServices<IMcpTool>().Select(tool => tool.Name).Order().ToArray();
        Assert.That(names, Is.EqualTo(new[]
        {
            "earth_magnetic_field_evaluate",
            "earth_magnetic_field_get_model_info",
            "ping"
        }));
    }

    [Test]
    public async Task McpToolListPublishesPreciseSchemasWithoutUsageStatistics()
    {
        using var request = McpRequest("""{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}""");
        HttpResponseMessage response = await httpClient_.SendAsync(request);
        using JsonDocument document = ParseSse(await response.Content.ReadAsStringAsync());
        JsonElement tools = document.RootElement.GetProperty("result").GetProperty("tools");
        JsonElement evaluate = tools.EnumerateArray().Single(tool =>
            tool.GetProperty("name").GetString() == "earth_magnetic_field_evaluate");
        string description = evaluate.GetProperty("description").GetString()!;

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(tools.GetArrayLength(), Is.EqualTo(3));
            Assert.That(tools.EnumerateArray().All(tool => tool.TryGetProperty("inputSchema", out _)), Is.True);
            Assert.That(tools.EnumerateArray().All(tool => tool.TryGetProperty("outputSchema", out _)), Is.True);
            Assert.That(tools.ToString(), Does.Not.Contain("usage_statistics").IgnoreCase);
            Assert.That(description, Does.Contain("north-east-down"));
            Assert.That(description, Does.Contain("UTC"));
            Assert.That(description, Does.Contain("no GUID"));
            Assert.That(evaluate.GetProperty("inputSchema").GetProperty("properties")
                .GetProperty("Samples").GetProperty("items").GetProperty("properties")
                .TryGetProperty("DateTimeUtc", out _), Is.True);
        });
    }

    [Test]
    public async Task McpEvaluationReturnsStructuredNedResult()
    {
        const string payload = """
            {"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"earth_magnetic_field_evaluate","arguments":{"Model":"WMM2025","Samples":[{"Latitude":0.5,"Longitude":1.0,"Depth":500.0,"DateTimeUtc":"2026-06-01T00:00:00Z"}]}}}
            """;
        using var request = McpRequest(payload);
        HttpResponseMessage response = await httpClient_.SendAsync(request);
        using JsonDocument document = ParseSse(await response.Content.ReadAsStringAsync());
        JsonElement result = document.RootElement.GetProperty("result");
        JsonElement structured = result.GetProperty("structuredContent");
        JsonElement sample = structured.GetProperty("Samples")[0];

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TryGetProperty("isError", out JsonElement isError) && isError.GetBoolean(), Is.False);
            Assert.That(structured.GetProperty("Model").GetProperty("Model").GetString(), Is.EqualTo("WMM2025"));
            Assert.That(sample.TryGetProperty("North", out _), Is.True);
            Assert.That(sample.TryGetProperty("East", out _), Is.True);
            Assert.That(sample.TryGetProperty("Down", out _), Is.True);
            Assert.That(sample.GetProperty("TotalIntensity").GetDouble(), Is.GreaterThan(1e-6));
        });
    }

    [Test]
    public async Task McpOffsetlessTimestampReturnsStructuredValidationError()
    {
        const string payload = """
            {"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"earth_magnetic_field_evaluate","arguments":{"Model":"WMM2025","Samples":[{"Latitude":0.5,"Longitude":1.0,"Depth":500.0,"DateTimeUtc":"2026-06-01T00:00:00"}]}}}
            """;
        using var request = McpRequest(payload);
        HttpResponseMessage response = await httpClient_.SendAsync(request);
        using JsonDocument document = ParseSse(await response.Content.ReadAsStringAsync());
        JsonElement result = document.RootElement.GetProperty("result");
        JsonElement structured = result.GetProperty("structuredContent");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.GetProperty("isError").GetBoolean(), Is.True);
            Assert.That(structured.GetProperty("Error").GetString(), Is.EqualTo("invalid_request"));
            Assert.That(structured.GetProperty("Errors")[0].GetProperty("Property").GetString(), Is.EqualTo("DateTimeUtc"));
        });
    }

    [TestCase("/EarthMagneticField/api/health/live")]
    [TestCase("/EarthMagneticField/api/health/ready")]
    [TestCase("/EarthMagneticField/api/metrics")]
    [TestCase("/EarthMagneticField/api/swagger/merged/swagger.json")]
    public async Task OperationalEndpointsAreAvailable(string path) =>
        Assert.That((await httpClient_.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.OK));

    private static HttpRequestMessage McpRequest(string payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/EarthMagneticField/api/mcp");
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        return request;
    }

    private static JsonDocument ParseSse(string content)
    {
        string dataLine = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Single(line => line.StartsWith("data:", StringComparison.Ordinal));
        return JsonDocument.Parse(dataLine["data:".Length..].Trim());
    }
}
