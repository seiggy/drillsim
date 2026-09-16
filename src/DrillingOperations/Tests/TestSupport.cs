using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DrillingOperations.Tests;

internal static class TestData
{
    internal const string ScenarioId = "11111111-1111-4111-8111-111111111111";
    internal static readonly Guid FieldId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    internal const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    internal const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    internal const string HashC = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";
    internal const string HashD = "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";
    internal static BindWorldRequest Binding(string scenarioId = ScenarioId) => new(scenarioId, HashA, HashB,
        "world-opaque-17", "reservoir-hidden-world-v3", "calibration-opaque-9", HashC);
    internal static CreateRunRequest Run(string scenarioId = ScenarioId, string planHash = HashD) =>
        new(scenarioId, HashA, "sealed-plan-opaque-3", planHash);

    internal static HttpResponseMessage Upstream(HttpRequestMessage request)
    {
        string path = request.RequestUri!.AbsolutePath;
        object body = path.EndsWith("/prediction", StringComparison.Ordinal)
            ? new { scenarioId = ScenarioId, body = new { candidateId = "candidate-1", proposedWellPath = ApprovedPath(), fieldPackageSha256 = HashB }, revision = 3, seal = new { sha256 = HashA }, approval = new { sealedSha256 = HashA } }
            : path.StartsWith("/api/scenarios/", StringComparison.Ordinal)
                ? new { scenarioId = ScenarioId, sourceFieldId = FieldId, reservoirName = "SOGNEFJORD FM", worldModelVersion = "reservoir-hidden-world-v3", status = "HumanApproved", initialAsOfUtc = DateTimeOffset.Parse("2025-01-01T00:00:00Z"), observationModelVersion = "observation-model-v1" }
                : new { worldId = "world-opaque-17", fieldId = FieldId, reservoirName = "SOGNEFJORD FM", modelVersion = "reservoir-hidden-world-v3", calibrationArtifact = new { id = "calibration-opaque-9", sha256 = HashC } };
        return Json(HttpStatusCode.OK, body);
    }

    internal static AnalysisScenarioDto ScenarioSnapshot(string scenarioId = ScenarioId) => new(Guid.Parse(scenarioId), FieldId, "SOGNEFJORD FM", "reservoir-hidden-world-v3", "HumanApproved", DateTimeOffset.Parse("2025-01-01T00:00:00Z"), "observation-model-v1");
    internal static AnalysisPredictionDto PredictionSnapshot(string scenarioId = ScenarioId) => new(Guid.Parse(scenarioId),
        new AnalysisPredictionBodyDto("candidate-1",
        [
            new(0, 100, 500000, 6700000), new(500, 500, 500100, 6700020),
            new(1000, 900, 500200, 6700040), new(1500, 1300, 500300, 6700060)
        ], HashB), 3, new AnalysisSealDto(HashA), new AnalysisApprovalDto(HashA));

    internal static object[] ApprovedPath() =>
    [
        new { measuredDepthM = 0d, trueVerticalDepthM = 100d, eastingM = 500000d, northingM = 6700000d },
        new { measuredDepthM = 500d, trueVerticalDepthM = 500d, eastingM = 500100d, northingM = 6700020d },
        new { measuredDepthM = 1000d, trueVerticalDepthM = 900d, eastingM = 500200d, northingM = 6700040d },
        new { measuredDepthM = 1500d, trueVerticalDepthM = 1300d, eastingM = 500300d, northingM = 6700060d }
    ];

    internal static HttpResponseMessage Json(HttpStatusCode status, object body) => new(status)
    { Content = new StringContent(CanonicalJson.Serialize(body), Encoding.UTF8, "application/json") };
}

internal sealed class StageASamplingHandler(Func<HttpRequestMessage, HttpResponseMessage> fallback, Func<HttpRequestMessage, HttpResponseMessage>? completionResponder = null, Func<HttpRequestMessage, HttpResponseMessage>? productionResponder = null) : HttpMessageHandler
{
    private JsonElement[] stations = []; private Guid runId; private string bindingId = string.Empty;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string path = request.RequestUri!.AbsolutePath;
        if (path.EndsWith("/path-bindings", StringComparison.Ordinal))
        {
            using JsonDocument d = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            JsonElement root = d.RootElement; runId=root.GetProperty("runId").GetGuid(); stations = root.GetProperty("stations").EnumerateArray().Select(x => x.Clone()).ToArray();
            var canonicalStations=stations.Select(x=>new{measuredDepthM=x.GetProperty("measuredDepthM").GetDouble(),eastingM=x.GetProperty("eastingM").GetDouble(),northingM=x.GetProperty("northingM").GetDouble(),trueVerticalDepthM=x.GetProperty("trueVerticalDepthM").GetDouble()});string approvedHash=root.GetProperty("approvedSealedPredictionSha256").GetString()!;string canonical=JsonSerializer.Serialize(new{worldId="world-opaque-17",scenarioId=root.GetProperty("scenarioId").GetGuid(),runId,pathKind=1,approvedSealedPredictionSha256=approvedHash,stations=canonicalStations},new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});string hash=DeterministicIdentity.Sha256(canonical);bindingId="rpb_"+DeterministicIdentity.Sha256("approved-path-binding-v1\n"+canonical);return TestData.Json(HttpStatusCode.OK, new { bindingId, worldId="world-opaque-17", scenarioId=root.GetProperty("scenarioId").GetGuid(), runId, pathKind="AsDrilled", approvedSealedPredictionSha256=approvedHash, canonicalHash=hash, stationCount=stations.Length });
        }
        if (path.EndsWith("/production-runs", StringComparison.Ordinal))
        {
            if (productionResponder is not null) return productionResponder(request);
            var checkpoints = new[] { new { year=1, simulatedTimeSeconds=ProductionExecutionOptions.YearSeconds, stateId="state-1", parentStateId=(string?)null, cumulativeProducedM3=new{oil=1200d,water=240d,gas=60d},maximumBalanceErrorFraction=1e-8 }, new { year=3, simulatedTimeSeconds=3*ProductionExecutionOptions.YearSeconds, stateId="state-3", parentStateId=(string?)"state-1", cumulativeProducedM3=new{oil=3600d,water=720d,gas=180d},maximumBalanceErrorFraction=1e-8 }, new { year=5, simulatedTimeSeconds=5*ProductionExecutionOptions.YearSeconds, stateId="state-5", parentStateId=(string?)"state-3", cumulativeProducedM3=new{oil=6000d,water=1200d,gas=300d},maximumBalanceErrorFraction=1e-8 } };
            var monthly = Enumerable.Range(1,60).Select(month => new { month, simulatedTimeSeconds=month*ProductionExecutionOptions.YearSeconds/12, oilRateM3PerSecond=.001, waterRateM3PerSecond=.0002, gasRateM3PerSecond=.00005, cumulativeOilM3=month*100d, cumulativeWaterM3=month*20d, cumulativeGasM3=month*5d, bottomHolePressurePa=15_000_000d, effectiveControlMode="Rate", switchReason="None" }).ToArray();
            return TestData.Json(HttpStatusCode.OK,new { productionRunId="production-run-1",worldId="world-opaque-17",completionBindingId="completion-binding-1",modelVersion="completion-production-v1",checkpoints,monthlyTruth=monthly });
        }        if (path.EndsWith("/completion-bindings", StringComparison.Ordinal))
        {
            if (completionResponder is not null) return completionResponder(request);
            string json = await request.Content!.ReadAsStringAsync(cancellationToken); using JsonDocument d = JsonDocument.Parse(json); JsonElement root = d.RootElement;
            JsonElement[] openings = root.GetProperty("openings").EnumerateArray().Select(x => x.Clone()).ToArray(); CompletionOpening[] typedOpenings = JsonSerializer.Deserialize<CompletionOpening[]>(root.GetProperty("openings"), CanonicalJson.SerializerOptions)!;
            return TestData.Json(HttpStatusCode.OK, new { completionBindingId = "completion-binding-1", worldId = "world-opaque-17", pathBindingId = root.GetProperty("pathBindingId").GetString(), scenarioId = Guid.Parse(TestData.ScenarioId), runId, modelVersion = root.GetProperty("completionModelVersion").GetString(), openingCount = openings.Length, producingConnectionCount = openings.Count(x => x.GetProperty("type").GetString() != "Isolated") + 7, canonicalRequestHash = CompletionCanonicalization.CanonicalRequestHash(root.GetProperty("pathBindingId").GetString()!, root.GetProperty("completionModelVersion").GetString()!, typedOpenings), createdUtc = DateTimeOffset.UtcNow });
        }        if (path.EndsWith("/samples", StringComparison.Ordinal))
        {
            using JsonDocument d = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            JsonElement root=d.RootElement; var samples=stations.Select(x=>new { measuredDepthM=x.GetProperty("measuredDepthM").GetDouble(),eastingM=x.GetProperty("eastingM").GetDouble(),northingM=x.GetProperty("northingM").GetDouble(),trueVerticalDepthM=x.GetProperty("trueVerticalDepthM").GetDouble(),reservoirTopDepthM=0d,reservoirBaseDepthM=2000d,porosity=.2,permeabilityM2=1e-13,netToGross=.7,isReservoirQuality=true,pressurePa=20_000_000d,oilSaturation=.7,waterSaturation=.3,gasSaturation=0d}).ToArray();
            return TestData.Json(HttpStatusCode.OK,new {bindingId,worldId="world-opaque-17",scenarioId=Guid.Parse(TestData.ScenarioId),runId,propertySetVersion="stage-b-truth-v1",sampleCount=samples.Length,samples});
        }
        return fallback(request);
    }
    private string LastRunId { get; set; } = TestData.ScenarioId;
}
internal sealed class DelegateHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(responder(request));
}

internal sealed class ApiFactory(Func<HttpRequestMessage, HttpResponseMessage>? responder = null, IDependencyCapabilityProbe? capability = null, Func<HttpRequestMessage, HttpResponseMessage>? completionResponder = null, Func<HttpRequestMessage, HttpResponseMessage>? productionResponder = null, PublicationFakeHandler? publication = null) : WebApplicationFactory<global::Program>
{
    internal const string InternalKey = "drilling-integration-internal-key";
    internal string DatabasePath { get; } = Path.Combine(Path.GetTempPath(), $"drillsim-drilling-{Guid.NewGuid():N}.db");
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder = responder ?? TestData.Upstream;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Sqlite", $"Data Source={DatabasePath}");
        builder.UseSetting("DRILLING_OPERATIONS_INTERNAL_KEY", InternalKey);
        builder.UseSetting("RESERVOIR_SIMULATION_OPERATOR_KEY", "stage-a-test-key");
        builder.UseSetting("AnalysisApiUrl", "http://analysis.test/");        builder.UseSetting("DRILLING_OPERATIONS_ANALYSIS_CALLBACK_KEY", "analysis-callback-test-key");
        builder.UseSetting("DRILLSIM_PUBLICATION_IMPORT_KEY", "publication-import-test-key");
        builder.UseSetting("FieldServiceUrl", "http://field.test/");
        builder.UseSetting("ClusterServiceUrl", "http://cluster.test/");
        builder.UseSetting("WellServiceUrl", "http://well.test/");
        builder.UseSetting("WellBoreServiceUrl", "http://wellbore.test/");
        builder.UseSetting("WellBoreArchitectureServiceUrl", "http://architecture.test/");
        builder.UseSetting("TrajectoryServiceUrl", "http://trajectory.test/");
        builder.UseSetting("GeologicalPropertiesServiceUrl", "http://geology.test/");
        builder.UseSetting("ReservoirSimulationUrl", "http://reservoir.test/");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(new AnalysisVerificationClient(new HttpClient(new DelegateHandler(_responder)) { BaseAddress = new Uri("http://analysis.test/") }));
            var stageClient = new HttpClient(new DelegateHandler(_responder)) { BaseAddress = new Uri("http://reservoir.test/") };
            stageClient.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", "stage-a-test-key");
            services.AddSingleton(new ReservoirVerificationClient(stageClient));
            services.AddSingleton(new ReservoirSetupClient(stageClient));
            var stageHandler = new StageASamplingHandler(_responder, completionResponder, productionResponder);
            services.AddSingleton(new ReservoirSamplingClient(new HttpClient(stageHandler) { BaseAddress = new Uri("http://reservoir.test/") }));
            services.AddSingleton(new ReservoirCompletionClient(new HttpClient(stageHandler) { BaseAddress = new Uri("http://reservoir.test/") }));
            services.AddSingleton(new ReservoirProductionClient(new HttpClient(stageHandler) { BaseAddress = new Uri("http://reservoir.test/") }));            if (publication is not null)
            {
                services.RemoveAll<AnalysisPublicationClient>(); services.RemoveAll<AnalysisRevealClient>(); services.RemoveAll<AnalysisScorecardClient>();
                var analysisPublication = new HttpClient(publication) { BaseAddress = new Uri("http://analysis.test/") };
                var reveal = new HttpClient(publication) { BaseAddress = new Uri("http://analysis.test/") }; reveal.DefaultRequestHeaders.Add("X-DrillSim-Internal-Key", "analysis-callback-test-key");
                services.AddSingleton(new AnalysisPublicationClient(analysisPublication)); services.AddSingleton(new AnalysisRevealClient(reveal)); services.AddSingleton(new AnalysisScorecardClient(reveal));
                foreach (string name in new[] { "FieldService", "ClusterService", "WellService", "WellBoreService", "WellBoreArchitectureService", "TrajectoryService", "GeologicalPropertiesService" }) services.AddHttpClient(name).ConfigurePrimaryHttpMessageHandler(() => publication);
            }
            if (capability is not null) services.AddSingleton(capability);
        });
    }
    internal HttpClient CreateInternalClient()
    {
        HttpClient client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(InternalKeyValidator.HeaderName, InternalKey); return client;
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); SqliteConnection.ClearAllPools(); if (disposing && File.Exists(DatabasePath)) File.Delete(DatabasePath);
    }
}

internal sealed class StoreFixture : IAsyncDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"drillsim-drilling-store-{Guid.NewGuid():N}.db");
    internal DrillingOperationsStore Store { get; }
    internal string ConnectionString => $"Data Source={_path}";
    internal StoreFixture(TimeProvider? timeProvider = null) => Store = new DrillingOperationsStore(ConnectionString, timeProvider ?? TimeProvider.System);
    internal Task InitializeAsync() => Store.InitializeAsync();
    public ValueTask DisposeAsync() { SqliteConnection.ClearAllPools(); if (File.Exists(_path)) File.Delete(_path); return ValueTask.CompletedTask; }
    internal static async Task<RunResponse> BindAndCreateAsync(DrillingOperationsStore store, string scenarioId = TestData.ScenarioId, string suffix = "x")
    {
        ApiOutcome binding = await store.BindWorldAsync($"/bind/{scenarioId}", $"bind-{suffix}", scenarioId, TestData.Binding(scenarioId));
        Assert.That(binding.StatusCode, Is.EqualTo(201));
        ApiOutcome run = await store.CreateRunAsync("/runs", $"run-{suffix}", TestData.Run(scenarioId));
        Assert.That(run.StatusCode, Is.EqualTo(202));
        return System.Text.Json.JsonSerializer.Deserialize<RunResponse>(run.Body, CanonicalJson.SerializerOptions)!;
    }
}

internal sealed class ToggleCapability(bool available = false) : IDependencyCapabilityProbe
{
    internal bool Available { get; set; } = available;
    public bool IsAvailable(RunStageKind stage) => Available && stage is RunStageKind.S1MaterializePlan;
}

















