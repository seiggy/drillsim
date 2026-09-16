using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Persistence;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class ApiSecurityTests
{
    internal const string OperatorKey = "integration-test-operator-key";
    private static readonly JsonSerializerOptions ApiJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false) }
    };

    [Test]
    public async Task WorldRoutes_RequireOperatorKey_AndResponsesDoNotLeakTruth()
    {
        await using var factory = new ReservoirApiFactory();
        using HttpClient publicClient = factory.CreateClient();
        WorldGenerationRequest apiWorldRequest = TestData.UniformWorldRequest(8, 3, 1);
        HttpResponseMessage statusResponse = await publicClient.GetAsync("/reservoirsimulation/api/status");
        string statusJson = await statusResponse.Content.ReadAsStringAsync();
        HttpResponseMessage missingKeyResponse = await publicClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds", apiWorldRequest);
        HttpResponseMessage wrongKeyResponse = await SendWithKey(
            publicClient, HttpMethod.Get, "/reservoirsimulation/api/worlds/not-present", "wrong-key");
        HttpResponseMessage missingRunKeyResponse = await publicClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds/not-present/runs", new SimulationRequest());
        HttpResponseMessage missingBindingKeyResponse = await publicClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds/not-present/path-bindings", new ApprovedPathBindingRequest());
        HttpResponseMessage missingSamplingKeyResponse = await publicClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds/not-present/samples", new TruthSamplingRequest());
        HttpResponseMessage missingCompletionKeyResponse = await publicClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds/not-present/completion-bindings",
            new ApprovedCompletionBindingRequest());
        HttpResponseMessage missingProductionKeyResponse = await publicClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds/not-present/completion-bindings/not-present/production-runs",
            new CompletionProductionRequest());

        using HttpClient operatorClient = factory.CreateClient();
        operatorClient.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", OperatorKey);
        HttpResponseMessage createResponse = await operatorClient.PostAsJsonAsync(
            "/reservoirsimulation/api/worlds", apiWorldRequest);
        string createJson = await createResponse.Content.ReadAsStringAsync();
        WorldSummary? summary = await createResponse.Content.ReadFromJsonAsync<WorldSummary>();
        HttpResponseMessage readResponse = await operatorClient.GetAsync(
            $"/reservoirsimulation/api/worlds/{summary!.WorldId}");
        var pathRequest = new ApprovedPathBindingRequest
        {
            ScenarioId = Guid.Parse("250738f1-e044-4751-a0ca-cda12022479a"),
            RunId = Guid.Parse("f495e5ec-054d-4cac-b46b-6cbdcf19d488"),
            PathKind = ApprovedPathKind.Planned,
            ApprovedSealedPredictionSha256 =
                "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
            Stations =
            [
                new ApprovedPathStation
                {
                    MeasuredDepthM = 0, EastingM = 0, NorthingM = 0, TrueVerticalDepthM = 1_005
                },
                new ApprovedPathStation
                {
                    MeasuredDepthM = 1_000, EastingM = 700, NorthingM = 0, TrueVerticalDepthM = 1_025
                }
            ]
        };
        HttpResponseMessage bindingResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/path-bindings", pathRequest, ApiJsonOptions);
        string bindingJson = await bindingResponse.Content.ReadAsStringAsync();
        ApprovedPathBindingMetadata? bindingMetadata =
            await bindingResponse.Content.ReadFromJsonAsync<ApprovedPathBindingMetadata>(ApiJsonOptions);
        ApprovedPathBindingRequest asDrilledRequest = pathRequest with
        {
            RunId = Guid.Parse("569151a3-c97d-4fb9-9a34-2ed3b03f8330"),
            PathKind = ApprovedPathKind.AsDrilled
        };
        HttpResponseMessage asDrilledResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/path-bindings",
            asDrilledRequest, ApiJsonOptions);
        string asDrilledJson = await asDrilledResponse.Content.ReadAsStringAsync();
        ApprovedPathBindingMetadata? asDrilledMetadata =
            await asDrilledResponse.Content.ReadFromJsonAsync<ApprovedPathBindingMetadata>(ApiJsonOptions);
        var completionRequest = new ApprovedCompletionBindingRequest
        {
            PathBindingId = asDrilledMetadata!.BindingId,
            CompletionModelVersion = "observed-log-completion-v1",
            Openings =
            [
                new ApprovedCompletionOpening
                {
                    OpeningId = "main-perf",
                    ReservoirName = "Uniform Sand",
                    Type = CompletionOpeningType.Perforated,
                    TopMeasuredDepthM = 0,
                    BaseMeasuredDepthM = 1_000,
                    WellboreRadiusM = 0.12,
                    Skin = 2,
                    Efficiency = 0.75,
                    UncertaintyM = 1
                }
            ]
        };
        HttpResponseMessage completionResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings",
            completionRequest, ApiJsonOptions);
        string completionJson = await completionResponse.Content.ReadAsStringAsync();
        ApprovedCompletionBindingMetadata? completionMetadata =
            await completionResponse.Content.ReadFromJsonAsync<ApprovedCompletionBindingMetadata>(ApiJsonOptions);
        HttpResponseMessage completionReadResponse = await operatorClient.GetAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings/" +
            completionMetadata!.CompletionBindingId);
        string completionReadJson = await completionReadResponse.Content.ReadAsStringAsync();
        double productionYear = CompletionProductionService.YearSeconds;
        var productionRequest = new CompletionProductionRequest
        {
            ProductionModelVersion = "completion-production-v1",
            InitialTimeStepSeconds = 90 * 24 * 60 * 60,
            Solver = new SolverOptions
            {
                MinimumTimeStepSeconds = 60,
                MaximumTimeStepSeconds = 90 * 24 * 60 * 60,
                MaximumSaturationChange = 0.05,
                GrowthSaturationChange = 0.005,
                TimeStepGrowthFactor = 1.5,
                TimeStepShrinkFactor = 0.5,
                CgRelativeTolerance = 1e-8,
                CgMaximumIterations = 1_000,
                MaximumStepAttempts = 10_000,
                MaximumSamplesPerWell = 120
            },
            Schedule =
            [
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 0, DurationSeconds = productionYear,
                    ControlMode = WellControlMode.Rate, TargetRateM3PerSecond = -8e-9
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = productionYear, DurationSeconds = 0.25 * productionYear, ShutIn = true
                },
                new CompletionProductionScheduleSegment
                {
                    StartTimeSeconds = 1.25 * productionYear, DurationSeconds = 3.75 * productionYear,
                    ControlMode = WellControlMode.Rate, TargetRateM3PerSecond = -8e-9
                }
            ]
        };
        HttpResponseMessage productionResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings/" +
            $"{completionMetadata.CompletionBindingId}/production-runs", productionRequest, ApiJsonOptions);
        string productionJson = await productionResponse.Content.ReadAsStringAsync();
        CompletionProductionResult? productionResult =
            await productionResponse.Content.ReadFromJsonAsync<CompletionProductionResult>(ApiJsonOptions);
        HttpResponseMessage productionReadResponse = await operatorClient.GetAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings/" +
            $"{completionMetadata.CompletionBindingId}/production-runs/{productionResult!.ProductionRunId}");
        string numericProductionJson = JsonSerializer.Serialize(productionRequest, ApiJsonOptions)
            .Replace("\"Rate\"", "0", StringComparison.Ordinal);
        using var numericProductionContent = new StringContent(
            numericProductionJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage numericProductionResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings/" +
            $"{completionMetadata.CompletionBindingId}/production-runs", numericProductionContent);
        string arbitraryProductionJson = JsonSerializer.Serialize(productionRequest, ApiJsonOptions)
            .Replace("\"shutIn\":false", "\"shutIn\":false,\"connections\":[],\"i\":0",
                StringComparison.Ordinal);
        using var arbitraryProductionContent = new StringContent(
            arbitraryProductionJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage arbitraryProductionResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings/" +
            $"{completionMetadata.CompletionBindingId}/production-runs", arbitraryProductionContent);
        string numericCompletionJson = JsonSerializer.Serialize(completionRequest, ApiJsonOptions)
            .Replace("\"Perforated\"", "0", StringComparison.Ordinal);
        using var numericCompletionContent = new StringContent(
            numericCompletionJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage numericCompletionResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings",
            numericCompletionContent);
        string oldMdJson = JsonSerializer.Serialize(completionRequest, ApiJsonOptions)
            .Replace("\"topMdM\":0", "\"topMeasuredDepthM\":0", StringComparison.Ordinal);
        using var oldMdContent = new StringContent(
            oldMdJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage oldMdResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings", oldMdContent);
        string arbitraryConnectionJson = JsonSerializer.Serialize(completionRequest, ApiJsonOptions)
            .Replace("\"uncertaintyM\":1",
                "\"uncertaintyM\":1,\"i\":0,\"j\":0,\"k\":0,\"eastingM\":1",
                StringComparison.Ordinal);
        using var arbitraryConnectionContent = new StringContent(
            arbitraryConnectionJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage arbitraryConnectionResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/completion-bindings",
            arbitraryConnectionContent);
        string numericEnumJson = JsonSerializer.Serialize(pathRequest, ApiJsonOptions)
            .Replace("\"Planned\"", "0", StringComparison.Ordinal);
        using var numericEnumContent = new StringContent(
            numericEnumJson, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage numericEnumResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/path-bindings", numericEnumContent);
        string numericEnumProblem = await numericEnumResponse.Content.ReadAsStringAsync();
        ApprovedPathBindingRequest outsideRequest = pathRequest with
        {
            RunId = Guid.Parse("ad44e33d-4134-42b7-9004-8bf80e60c51d"),
            Stations =
            [
                pathRequest.Stations[0] with { EastingM = 100_000 },
                pathRequest.Stations[1]
            ]
        };
        HttpResponseMessage outsideResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/path-bindings",
            outsideRequest, ApiJsonOptions);
        string outsideProblem = await outsideResponse.Content.ReadAsStringAsync();
        HttpResponseMessage bindingReadResponse = await operatorClient.GetAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/path-bindings/{bindingMetadata!.BindingId}");
        string bindingReadJson = await bindingReadResponse.Content.ReadAsStringAsync();
        var samplingRequest = new TruthSamplingRequest
        {
            BindingId = bindingMetadata.BindingId,
            MaximumSpacingM = 50,
            MaximumSamples = 1_000,
            PropertySetVersion = "stage-b-truth-v1",
            CallerLabel = "drilling-operations-stage-b"
        };
        HttpResponseMessage samplingResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/samples", samplingRequest);
        string samplingJson = await samplingResponse.Content.ReadAsStringAsync();
        using var arbitraryCoordinates = new StringContent(
            $$"""
            {
              "bindingId": "{{bindingMetadata.BindingId}}",
              "maximumSpacingM": 50,
              "maximumSamples": 1000,
              "propertySetVersion": "stage-b-truth-v1",
              "callerLabel": "drilling-operations-stage-b",
              "coordinates": [{ "eastingM": 1, "northingM": 2, "trueVerticalDepthM": 3 }]
            }
            """, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage arbitraryCoordinatesResponse = await operatorClient.PostAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/samples", arbitraryCoordinates);
        HttpResponseMessage blockedDeleteResponse = await operatorClient.DeleteAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}");
        string readJson = await readResponse.Content.ReadAsStringAsync();
        HttpResponseMessage runResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/runs",
            new SimulationRequest
            {
                DurationSeconds = 10,
                InitialTimeStepSeconds = 10,
                Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
                Wells = []
            });
        string runJson = await runResponse.Content.ReadAsStringAsync();
        SimulationRunEnvelope? envelope = await runResponse.Content.ReadFromJsonAsync<SimulationRunEnvelope>();
        HttpResponseMessage continuationResponse = await operatorClient.PostAsJsonAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/runs",
            new SimulationRequest
            {
                DurationSeconds = 10,
                InitialTimeStepSeconds = 10,
                Fluids = new FluidModelOptions { GravityMPerS2 = 0 },
                Wells = [],
                ContinueFromStateId = envelope!.FinalStateId
            });
        SimulationRunEnvelope? continuationEnvelope =
            await continuationResponse.Content.ReadFromJsonAsync<SimulationRunEnvelope>();
        HttpResponseMessage stateResponse = await operatorClient.GetAsync(
            $"/reservoirsimulation/api/worlds/{summary.WorldId}/states/{envelope!.FinalStateId}");
        string stateJson = await stateResponse.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(statusJson, Does.Not.Contain("worldId"));
            Assert.That(statusJson, Does.Not.Contain("worldsInMemory"));
            Assert.That(missingKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(wrongKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(missingRunKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(missingBindingKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(missingSamplingKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(missingCompletionKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(missingProductionKeyResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(readResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(bindingResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(asDrilledResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(completionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(completionReadResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(productionResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(productionReadResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(numericProductionResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(arbitraryProductionResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(productionResult.MonthlyTruth, Has.Count.EqualTo(60));
            Assert.That(productionJson, Does.Not.Contain("connections"));
            Assert.That(productionJson, Does.Not.Contain("\"i\""));
            Assert.That(productionJson, Does.Not.Contain("oilSaturation"));
            Assert.That(productionJson, Does.Not.Contain("pressurePa"));
            Assert.That(productionJson, Does.Not.Contain("stateRanges"));
            Assert.That(numericCompletionResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(arbitraryConnectionResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(oldMdResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(completionMetadata.ModelVersion, Is.EqualTo("observed-log-completion-v1"));
            Assert.That(completionMetadata.OpeningCount, Is.EqualTo(1));
            Assert.That(completionMetadata.ProducingConnectionCount, Is.GreaterThan(1));
            Assert.That(completionJson, Does.Contain("\"modelVersion\":\"observed-log-completion-v1\""));
            Assert.That(completionJson, Does.Not.Contain("completionModelVersion"));
            Assert.That(completionJson, Does.Not.Contain("\"type\""));
            Assert.That(completionJson, Does.Not.Contain("openingId"));
            Assert.That(completionJson, Does.Not.Contain("mappedConnection"));
            Assert.That(completionJson, Does.Not.Contain("connectionHash"));
            Assert.That(completionJson, Does.Not.Contain("\"i\""));
            Assert.That(completionReadJson, Does.Not.Contain("openings"));
            Assert.That(bindingJson, Does.Contain("\"pathKind\":\"Planned\""));
            Assert.That(asDrilledJson, Does.Contain("\"pathKind\":\"AsDrilled\""));
            Assert.That(numericEnumResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(numericEnumResponse.Content.Headers.ContentType?.MediaType,
                Is.EqualTo("application/problem+json"));
            Assert.That(numericEnumProblem, Does.Contain("\"title\""));
            Assert.That(outsideResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(outsideResponse.Content.Headers.ContentType?.MediaType,
                Is.EqualTo("application/problem+json"));
            Assert.That(outsideProblem, Does.Contain("stations[0].eastingM"));
            Assert.That(bindingReadResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(samplingResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(arbitraryCoordinatesResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(blockedDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(bindingJson, Does.Not.Contain("stations"));
            Assert.That(bindingReadJson, Does.Not.Contain("stations"));
            Assert.That(bindingReadJson, Does.Not.Contain("eastingM"));
            Assert.That(samplingJson, Does.Not.Contain("cellIndex"));
            Assert.That(samplingJson, Does.Not.Contain("responseHash"));
            Assert.That(samplingJson, Does.Not.Contain("auditId"));
            Assert.That(samplingJson, Does.Not.Contain("truthChecksum"));
            Assert.That(samplingJson, Does.Not.Contain("\"pressurePa\":["));
            Assert.That(runResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(stateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(continuationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(continuationEnvelope!.Result.SimulatedTimeSeconds, Is.EqualTo(20));
            Assert.That(summary.ModelVersion, Is.EqualTo("reservoir-hidden-world-v3"));
            Assert.That(summary.CalibrationArtifact.Id, Is.EqualTo("test-calibration-v1"));
            Assert.That(summary.CalibrationArtifact.Sha256, Has.Length.EqualTo(64));
            AssertPublicWorldSummary(createJson);
            AssertPublicWorldSummary(readJson);
            Assert.That(runJson, Does.Not.Contain("initialInPlaceM3"));
            Assert.That(runJson, Does.Not.Contain("finalInPlaceM3"));
            Assert.That(runJson, Does.Not.Contain("compressibilityStorageM3"));
            Assert.That(runJson, Does.Contain("inPlaceNormalizedBalanceErrorFraction"));
            Assert.That(runJson, Does.Contain("throughputNormalizedBalanceErrorFraction"));
            Assert.That(runJson, Does.Not.Contain("\"pressurePa\":["));
            Assert.That(runJson, Does.Not.Contain("\"oilSaturation\":["));
            Assert.That(runJson, Does.Not.Contain("\"waterSaturation\":["));
            Assert.That(runJson, Does.Not.Contain("\"gasSaturation\":["));
            Assert.That(runJson, Does.Not.Contain("pressureBlob"));
            Assert.That(runJson, Does.Not.Contain("stateChecksum"));
            Assert.That(runJson, Does.Not.Contain("truthChecksum"));
            Assert.That(stateJson, Does.Not.Contain("checksum"));
            Assert.That(stateJson, Does.Not.Contain("pressure"));
            Assert.That(stateJson, Does.Not.Contain("saturation"));
        });
    }

    private static async Task<HttpResponseMessage> SendWithKey(
        HttpClient client,
        HttpMethod method,
        string path,
        string key)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-DrillSim-Operator-Key", key);
        return await client.SendAsync(request);
    }

    private static void AssertPublicWorldSummary(string json)
    {
        Assert.That(json, Does.Contain("\"modelVersion\":\"reservoir-hidden-world-v3\""));
        Assert.That(json, Does.Not.Contain("propertyRanges"));
        Assert.That(json, Does.Not.Contain("originEastingM"));
        Assert.That(json, Does.Not.Contain("originNorthingM"));
        Assert.That(json, Does.Not.Contain("cellSizeXM"));
        Assert.That(json, Does.Not.Contain("cellSizeYM"));
        Assert.That(json, Does.Not.Contain("topDepthM"));
        Assert.That(json, Does.Not.Contain("baseDepthM"));
    }

    internal sealed class ReservoirApiFactory(string? connectionString = null) : WebApplicationFactory<global::Program>
    {
        private readonly string? _databasePath = connectionString is null
            ? Path.Combine(Path.GetTempPath(), $"drillsim-reservoir-api-{Guid.NewGuid():N}.db")
            : null;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Sqlite", connectionString ?? $"Data Source={_databasePath}");
            builder.UseSetting("RESERVOIR_SIMULATION_OPERATOR_KEY", OperatorKey);
            builder.UseSetting("RESERVOIR_SIMULATION_WORLD_CAPACITY", "2");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SqliteConnection.ClearAllPools();
            if (disposing && _databasePath is not null && File.Exists(_databasePath))
                File.Delete(_databasePath);
        }
    }
}
