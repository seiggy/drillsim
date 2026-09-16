using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DrillingOperations.Tests;

[TestFixture]
[NonParallelizable]
public sealed class ApiSecurityAndContractTests
{
    [Test]
    public async Task EveryOperatorRouteRequiresInternalKey_StatusIsSafe()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateClient();
        HttpResponseMessage status = await client.GetAsync("/drillingoperations/api/status");
        string statusBody = await status.Content.ReadAsStringAsync();
        string[] paths =
        [
            "/drillingoperations/api/scenarios/s/binding",
            "/drillingoperations/api/runs/r",
            "/drillingoperations/api/runs/r/events",
            "/drillingoperations/api/audit?scenarioId=s"
        ];
        var unauthorized = new List<HttpStatusCode>();
        foreach (string path in paths) unauthorized.Add((await client.GetAsync(path)).StatusCode);
        unauthorized.Add((await client.PostAsJsonAsync("/drillingoperations/api/scenarios/s/bind", TestData.Binding("s"))).StatusCode);
        unauthorized.Add((await client.PostAsync("/drillingoperations/api/runs/r/cancel", null)).StatusCode);
        unauthorized.Add((await client.GetAsync("/drillingoperations/api/not-a-route")).StatusCode);

        Assert.Multiple(() =>
        {
            Assert.That(status.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(unauthorized, Has.All.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(statusBody, Does.Not.Contain("world-opaque"));
            Assert.That(statusBody, Does.Not.Contain(ApiFactory.InternalKey));
            Assert.That(statusBody, Does.Not.Contain("runId"));
            Assert.That(statusBody, Does.Not.Contain("scenarioId"));
        });
    }

    [Test]
    public async Task BindingReturnsOnlyOpaqueProvenance_AndCannotLeakTruthGeometry()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/bind")
        {
            Content = JsonContent.Create(TestData.Binding())
        };
        request.Headers.Add("Idempotency-Key", "bind-api");
        HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        HttpResponseMessage read = await client.GetAsync($"/drillingoperations/api/scenarios/{TestData.ScenarioId}/binding");
        string readBody = await read.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Is.EqualTo(readBody));
            Assert.That(body, Does.Contain("world-opaque-17"));
            Assert.That(body, Does.Not.Contain("pressure"));
            Assert.That(body, Does.Not.Contain("saturation"));
            Assert.That(body, Does.Not.Contain("geometry"));
            Assert.That(body, Does.Not.Contain("trajectory"));
            Assert.That(body, Does.Not.Contain("samples"));
        });
    }

    [Test]
    public async Task HttpIdempotency_ReplaysOriginalStatusBodyAndLocation_AndConflicts()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        static HttpRequestMessage Bind(string world)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/bind")
            {
                Content = JsonContent.Create(TestData.Binding() with { WorldId = world })
            };
            message.Headers.Add("Idempotency-Key", "same-http-key");
            return message;
        }
        using HttpResponseMessage first = await client.SendAsync(Bind("world-opaque-17"));
        string firstBody = await first.Content.ReadAsStringAsync();
        using HttpResponseMessage replay = await client.SendAsync(Bind("world-opaque-17"));
        string replayBody = await replay.Content.ReadAsStringAsync();
        using HttpResponseMessage conflict = await client.SendAsync(Bind("other-world"));

        Assert.Multiple(() =>
        {
            Assert.That(replay.StatusCode, Is.EqualTo(first.StatusCode));
            Assert.That(replayBody, Is.EqualTo(firstBody));
            Assert.That(replay.Headers.Location, Is.EqualTo(first.Headers.Location));
            Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        });
    }

    [Test]
    public async Task SseIsBoundedAndContainsStageOnlyPayloads()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        await BindAsync(client, TestData.ScenarioId, "bind-sse");
        using var create = new HttpRequestMessage(HttpMethod.Post, "/drillingoperations/api/runs")
        {
            Content = JsonContent.Create(TestData.Run())
        };
        create.Headers.Add("Idempotency-Key", "run-sse");
        HttpResponseMessage createResponse = await client.SendAsync(create);
        RunResponse run = JsonSerializer.Deserialize<RunResponse>(
            await createResponse.Content.ReadAsStringAsync(), CanonicalJson.SerializerOptions)!;
        HttpResponseMessage events = await client.GetAsync($"/drillingoperations/api/runs/{run.RunId}/events");
        string body = await events.Content.ReadAsStringAsync();
        string audit = await client.GetStringAsync($"/drillingoperations/api/audit?scenarioId={TestData.ScenarioId}");

        Assert.Multiple(() =>
        {
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(events.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(events.Content.Headers.ContentType!.MediaType, Is.EqualTo("text/event-stream"));
            Assert.That(body, Does.Contain("event: stage"));
            Assert.That(body, Does.Contain("S0BindWorld"));
            Assert.That(body, Does.Contain("S2ExecuteDrilling"));
            Assert.That(body, Does.Contain("S6DesignCompletion"));
            Assert.That(body, Does.Not.Contain(run.RunId));
            Assert.That(body, Does.Not.Contain("world-opaque-17"));
            Assert.That(body, Does.Not.Contain("outputHash"));
            Assert.That(body, Does.Not.Contain("diagnosticCode"));
            Assert.That(body, Does.Not.Contain("eastingM"));
            Assert.That(body, Does.Not.Contain("trueVerticalDepthM"));
            Assert.That(audit, Does.Not.Contain("eastingM"));
            Assert.That(audit, Does.Not.Contain("canonicalPath"));
            Assert.That(body.Length, Is.LessThan(20_000));
        });
    }

    [Test]
    public async Task ApiEnumFields_AreStableNames_NotNumbers()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateInternalClient();
        string serviceStatus = await client.GetStringAsync("/drillingoperations/api/status");
        await BindAsync(client, TestData.ScenarioId, "bind-enum-api");
        string binding = await client.GetStringAsync($"/drillingoperations/api/scenarios/{TestData.ScenarioId}/binding");
        using var create = new HttpRequestMessage(HttpMethod.Post, "/drillingoperations/api/runs") { Content = JsonContent.Create(TestData.Run()) };
        create.Headers.Add("Idempotency-Key", "run-enum-api");
        HttpResponseMessage created = await client.SendAsync(create);
        string createdBody = await created.Content.ReadAsStringAsync();
        RunResponse run = JsonSerializer.Deserialize<RunResponse>(createdBody, CanonicalJson.SerializerOptions)!;
        string getBody = string.Empty;
        for (int attempt = 0; attempt < 200; attempt++)
        {
            getBody = await client.GetStringAsync($"/drillingoperations/api/runs/{run.RunId}");
            using JsonDocument document = JsonDocument.Parse(getBody);
            if (document.RootElement.GetProperty("status").GetString() == "AwaitingApproval") break;
            await Task.Delay(25);
        }
        Assert.Multiple(() =>
        {
            Assert.That(serviceStatus, Does.Contain("\"status\":\"Ready\""));
            Assert.That(binding, Does.Not.Match("\"[^\"]+\":-?[0-9]+(?=[,}])"));
            Assert.That(createdBody, Does.Contain("\"status\":\"Queued\""));
            Assert.That(getBody, Does.Contain("\"status\":\"AwaitingApproval\""));
            Assert.That(getBody, Does.Contain("\"currentStage\":\"S6DesignCompletion\""));
            Assert.That(getBody, Does.Not.Match("\"status\":-?[0-9]+"));
            Assert.That(getBody, Does.Not.Match("\"currentStage\":-?[0-9]+"));
        });
    }

    [Test]
    public async Task ResumeEndpoint_RequiresIdempotencyKey_AndRemainsBlockedWithoutCapability()
    {
        await using var factory = new ApiFactory(); using HttpClient client = factory.CreateInternalClient();
        await BindAsync(client, TestData.ScenarioId, "bind-resume-api");
        using var create = new HttpRequestMessage(HttpMethod.Post, "/drillingoperations/api/runs") { Content = JsonContent.Create(TestData.Run()) };
        create.Headers.Add("Idempotency-Key", "run-resume-api");
        HttpResponseMessage created = await client.SendAsync(create);
        RunResponse run = JsonSerializer.Deserialize<RunResponse>(await created.Content.ReadAsStringAsync(), CanonicalJson.SerializerOptions)!;
        for (int attempt = 0; attempt < 20 && (await client.GetFromJsonAsync<RunResponse>($"/drillingoperations/api/runs/{run.RunId}", CanonicalJson.SerializerOptions))!.Status != RunStatus.AwaitingDependency; attempt++) await Task.Delay(25);
        HttpResponseMessage missingKey = await client.PostAsync($"/drillingoperations/api/runs/{run.RunId}/resume", null);
        using var resume = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/runs/{run.RunId}/resume"); resume.Headers.Add("Idempotency-Key", "resume-api");
        HttpResponseMessage unavailable = await client.SendAsync(resume);
        Assert.Multiple(() => { Assert.That(missingKey.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest)); Assert.That(unavailable.StatusCode, Is.EqualTo(HttpStatusCode.Conflict)); });
    }

    private static async Task BindAsync(HttpClient client, string scenarioId, string key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/drillingoperations/api/scenarios/{scenarioId}/bind")
        {
            Content = JsonContent.Create(TestData.Binding(scenarioId))
        };
        request.Headers.Add("Idempotency-Key", key);
        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}

[TestFixture]
public sealed class AppHostTopologyTests
{
    [Test]
    public void DrillingOperationsIsInternal_AndSecretsNeverReachAnalysisResources()
    {
        string source = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Topology", "AppHost.cs"));
        string drilling = Slice(source, ".AddProject(\"drilling-operations\"", "cartographicProjection.WithEnvironment");
        string analysisApi = Slice(source, ".AddProject(\"analysis-api\"", "var analysisWeb");
        string analysisWeb = Slice(source, ".AddViteApp(\"analysis-web\"", "var clusterUi");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("drilling-operations-db"));
            Assert.That(drilling, Does.Contain("DRILLING_OPERATIONS_INTERNAL_KEY"));
            Assert.That(drilling, Does.Contain("RESERVOIR_SIMULATION_OPERATOR_KEY"));
            Assert.That(drilling, Does.Contain("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY"));
            Assert.That(drilling, Does.Contain("DRILLSIM_PUBLICATION_IMPORT_KEY"));
            Assert.That(source, Does.Contain("publication-import-key"));
            Assert.That(drilling, Does.Contain("ReservoirSimulationUrl"));
            Assert.That(drilling, Does.Not.Contain("WithExternalHttpEndpoints"));
            Assert.That(analysisApi, Does.Not.Contain("drilling-operations"));
            Assert.That(analysisApi, Does.Not.Contain("RESERVOIR_SIMULATION_OPERATOR_KEY"));
            Assert.That(analysisApi, Does.Not.Contain("DRILLING_OPERATIONS_INTERNAL_KEY"));
            Assert.That(analysisWeb, Does.Not.Contain("drilling-operations"));
            Assert.That(analysisWeb, Does.Not.Contain("RESERVOIR_SIMULATION_OPERATOR_KEY"));
            Assert.That(analysisWeb, Does.Not.Contain("DRILLING_OPERATIONS_INTERNAL_KEY"));
            Assert.That(analysisWeb, Does.Not.Contain("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY"));
            Assert.That(analysisApi, Does.Not.Contain("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY"));
            Assert.That(analysisApi, Does.Not.Contain("DRILLSIM_PUBLICATION_IMPORT_KEY"));
            Assert.That(analysisWeb, Does.Not.Contain("DRILLSIM_PUBLICATION_IMPORT_KEY"));
        });
    }

    private static string Slice(string source, string start, string end)
    {
        int startIndex = source.IndexOf(start, StringComparison.Ordinal);
        int endIndex = source.IndexOf(end, startIndex, StringComparison.Ordinal);
        Assert.That(startIndex, Is.GreaterThanOrEqualTo(0), $"Missing topology marker: {start}");
        Assert.That(endIndex, Is.GreaterThan(startIndex), $"Missing topology marker: {end}");
        return source[startIndex..endIndex];
    }
}







