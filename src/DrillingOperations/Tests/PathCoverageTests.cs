using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace DrillingOperations.Tests;

[TestFixture, NonParallelizable]
public sealed class PathCoverageTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task S2_ChecksTheApprovedPathBeforeAnyDrilling_AndDistinguishesRejectionFromOutage(bool unavailable)
    {
        var calls = new List<string>();
        await using var factory = new ApiFactory(pathBindingResponder: request =>
        {
            using JsonDocument body = JsonDocument.Parse(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
            calls.Add(body.RootElement.GetProperty("pathKind").GetString()!);
            Assert.That(body.RootElement.GetProperty("stations").GetArrayLength(), Is.EqualTo(4));
            return TestData.Json(unavailable ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.BadRequest,
                new { diagnosticCode = "PathOutsideModelCoverage", detail = "PRIVATE-WORLD-DETAIL",
                    errors = new Dictionary<string, string[]> { ["stations[0].northingM"] = ["Private model extent."] } });
        });
        using HttpClient client = factory.CreateInternalClient();
        using var bind = new HttpRequestMessage(HttpMethod.Post, $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/bind")
        { Content = JsonContent.Create(TestData.Binding()) };
        bind.Headers.Add("Idempotency-Key", "coverage-bind");
        (await client.SendAsync(bind)).EnsureSuccessStatusCode();
        using var start = new HttpRequestMessage(HttpMethod.Post, "/drillingoperations/api/runs")
        { Content = JsonContent.Create(TestData.Run()) };
        start.Headers.Add("Idempotency-Key", "coverage-run");
        using HttpResponseMessage started = await client.SendAsync(start);
        started.EnsureSuccessStatusCode();
        RunResponse run = (await started.Content.ReadFromJsonAsync<RunResponse>(CanonicalJson.SerializerOptions))!;
        var store = factory.Services.GetRequiredService<DrillingOperationsStore>();
        RunStatus expectedStatus = unavailable ? RunStatus.AwaitingDependency : RunStatus.Failed;
        for (int attempt = 0; attempt < 200; attempt++)
        {
            run = (await store.GetRunAsync(run.RunId))!;
            if (run.Status == expectedStatus) break;
            await Task.Delay(20);
        }
        IReadOnlyList<StageResponse> stages = await store.GetStagesAsync(run.RunId);
        var sideEffects = await store.GetSideEffectCountsAsync(run.RunId);
        Assert.Multiple(() =>
        {
            Assert.That(run.Status, Is.EqualTo(expectedStatus));
            Assert.That(run.CurrentStage, Is.EqualTo(RunStageKind.S2ExecuteDrilling));
            Assert.That(run.DiagnosticCode, Is.EqualTo(unavailable ? "StageAUnavailable" : "PlannedPathOutsideModelCoverage"));
            Assert.That(calls, Is.EqualTo(new[] { "Planned" }));
            Assert.That(stages.Take(2).Select(stage => stage.Status), Has.All.EqualTo(StageStatus.Completed));
            Assert.That(stages.Skip(3).Select(stage => stage.Status), Has.All.EqualTo(StageStatus.Pending));
            Assert.That(sideEffects.PublicationCount, Is.Zero);
            Assert.That(sideEffects.ClockAdvanceCount, Is.Zero);
        });
        Assert.That(await store.GetDrillingExecutionAsync(run.RunId), Is.Null);
        Assert.That(await store.GetSurveyArtifactAsync(run.RunId), Is.Null);
        Assert.That(await store.GetTruthSampleBatchAsync(run.RunId), Is.Null);
        Assert.That(CanonicalJson.Serialize(run), Does.Not.Contain("PRIVATE-WORLD-DETAIL").And.Not.Contain("northingM"));
    }

    [Test]
    public async Task AsDrilledCoverageRejection_PreservesSpecificCodeWithoutForwardingPrivateProblemText()
    {
        await using var fixture = new StoreFixture();
        await fixture.InitializeAsync();
        RunResponse run = await StoreFixture.BindAndCreateAsync(fixture.Store);
        await fixture.Store.CompleteS0Async(run.RunId);
        await fixture.Store.MaterializePlanAsync(run.RunId, TestData.ScenarioSnapshot(), TestData.PredictionSnapshot());
        await fixture.Store.ExecuteDeterministicDrillingAsync(run.RunId, new());
        TruthBindingResponse binding = (await fixture.Store.GetBindingAsync(run.ScenarioId))!;
        DrillingExecutionArtifact execution = (await fixture.Store.GetDrillingExecutionAsync(run.RunId))!;
        using var http = new HttpClient(new DelegateHandler(_ => TestData.Json(HttpStatusCode.BadRequest,
            new { diagnosticCode = "PathOutsideModelCoverage", detail = "PRIVATE-WORLD-DETAIL" })))
        { BaseAddress = new Uri("http://reservoir.test/") };
        var sampling = new ReservoirSamplingClient(http);
        StageASamplingException exception = Assert.ThrowsAsync<StageASamplingException>(
            async () => await sampling.SampleAsync(binding, execution, new(), CancellationToken.None))!;
        Assert.Multiple(() =>
        {
            Assert.That(exception.DiagnosticCode, Is.EqualTo("AsDrilledPathOutsideModelCoverage"));
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            Assert.That(exception.Message, Does.Contain("as-drilled").And.Not.Contain("PRIVATE-WORLD-DETAIL"));
        });
    }
}
