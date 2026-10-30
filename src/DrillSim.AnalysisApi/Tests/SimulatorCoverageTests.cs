using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class SimulatorCoverageTests
{
    [Test]
    public void Coverage_PadsStructuralControlBoundsAndAppliesSimulatorDepthEnvelope()
    {
        SimulatorCoverage coverage = PetrophysicsAnalysisService.EstimateSimulatorCoverage(
            AnalysisConfigurationTests.CreatePackage(), "Target")!;
        AssertFixtureCoverage(coverage);
        Assert.That(coverage.Contains(5500, -1500), Is.True);
        Assert.That(coverage.Contains(5501, 0), Is.False);

        AnalysisPackage surveyed = AnalysisConfigurationTests.CreatePackage(withTrajectories: true);
        foreach (JsonNode trajectory in surveyed.Trajectories)
            trajectory["SurveyStationList"] = new JsonArray(Station(0, 0), Station(2000, 2000));
        surveyed.Trajectories[0]["SurveyStationList"] = new JsonArray(Station(0, 0), Station(300, 1500));
        // Deepest base 1500 m TVD, shallowest top 100 m: 1500 + max(1000, 1400) - 100.
        Assert.That(PetrophysicsAnalysisService.EstimateSimulatorCoverage(surveyed, "Target")!.MaxTrueVerticalDepthM,
            Is.EqualTo(2800).Within(1e-9));
        Assert.That(PetrophysicsAnalysisService.EstimateSimulatorCoverage(surveyed, "Missing"), Is.Null);

        static JsonObject Station(double md, double tvd) => new() { ["MD"] = md, ["TVD"] = tvd };
    }

    [Test]
    public async Task Endpoint_ReturnsCoverageForLiveAndScenarioScopes()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        Guid fieldId = f.Data.Data.LiveScope.FieldId;
        AssertFixtureCoverage((await f.Client.GetFromJsonAsync<SimulatorCoverage>(
            $"/api/fields/{fieldId}/simulator-coverage?reservoir=Target"))!);

        HypothesisScope scope = await f.Data.Data.ScenarioScopeAsync();
        AssertFixtureCoverage((await f.Client.GetFromJsonAsync<SimulatorCoverage>(
            $"/api/fields/{fieldId}/simulator-coverage?scenarioId={scope.ScenarioId}"))!);

        (string Query, HttpStatusCode Status)[] failures =
        [
            ("", HttpStatusCode.BadRequest),
            ("?reservoir=Missing", HttpStatusCode.NotFound),
            ($"?reservoir=Other&scenarioId={scope.ScenarioId}", HttpStatusCode.Conflict),
            ($"?reservoir=Target&scenarioId={Guid.NewGuid()}", HttpStatusCode.NotFound),
        ];
        foreach ((string query, HttpStatusCode status) in failures)
        {
            using HttpResponseMessage response = await f.Client.GetAsync($"/api/fields/{fieldId}/simulator-coverage{query}");
            Assert.That(response.StatusCode, Is.EqualTo(status), query);
        }
    }

    [Test]
    public async Task ForecastDraft_RejectsTargetOutsideSimulatorCoverageBeforeCallingTheModel()
    {
        using var f = new FormationInterpretationFixture();
        f.Data.Source.Package.Field["DelineationLines"] = new JsonArray(new JsonObject
        {
            ["Points"] = new JsonArray(Point(-12000, -12000), Point(16000, 16000))
        });
        f.Rehash();
        HypothesisScope scope = await f.Data.ScenarioScopeAsync();
        AnalysisPackage package = await f.Data.Scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc);
        AnalysisResult result = f.Data.Analysis.Analyze(package, scope.ReservoirName, AnalysisConfiguration.Default);
        SimulatorCoverage coverage = PetrophysicsAnalysisService.EstimateSimulatorCoverage(package, scope.ReservoirName)!;
        CandidateGridPoint outside = result.CandidateGrid.First(point => point.Status == "eligible" &&
            !coverage.Contains(point.EastingM, point.NorthingM));
        var service = new ForecastDraftService(f.Data.Scenarios, new SqliteScenarioStore(f.Data.ConnectionString),
            f.Data.Analysis, f.Agent, TimeProvider.System);

        ScenarioApiException rejected = Assert.ThrowsAsync<ScenarioApiException>(() => service.DraftAsync(new(
            scope.ScenarioId!.Value, outside.CandidateId, AnalysisConfiguration.Default, package.Sha256, result.AnalysisSha256)))!;

        Assert.That(rejected.StatusCode, Is.EqualTo(409));
        Assert.That(rejected.Message, Does.Contain("outside the blind simulator"));
        Assert.That(f.Client.Calls, Is.Zero);

        static JsonObject Point(double east, double north) => new() { ["RiemannianEast"] = east, ["RiemannianNorth"] = north };
    }

    private static void AssertFixtureCoverage(SimulatorCoverage coverage) => Assert.Multiple(() =>
    {
        Assert.That(coverage.Method, Is.EqualTo("simulator-coverage-v1"));
        Assert.That(coverage.ControlCount, Is.EqualTo(5));
        Assert.That(coverage.MarginM, Is.EqualTo(1500));
        Assert.That(coverage.MinEastingM, Is.EqualTo(-1500).Within(1e-9));
        Assert.That(coverage.MaxEastingM, Is.EqualTo(5500).Within(1e-9));
        Assert.That(coverage.MinNorthingM, Is.EqualTo(-1500).Within(1e-9));
        Assert.That(coverage.MaxNorthingM, Is.EqualTo(5500).Within(1e-9));
        Assert.That(coverage.MaxTrueVerticalDepthM, Is.EqualTo(1200).Within(1e-9));
        Assert.That(coverage.Limitation, Does.Contain("Not a geological boundary"));
    });
}
