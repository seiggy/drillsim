using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ForecastDraftTests
{
    [Test]
    public void StructureEstimate_InterpolatesVisiblePicksAndConvertsMdWithSurveyStations()
    {
        AnalysisPackage package = AnalysisConfigurationTests.CreatePackage(withTrajectories: true);
        FormationStructureEstimate flat = PetrophysicsAnalysisService.EstimateFormationStructure(package, "Target", 1000, 1000, 4);
        Assert.Multiple(() =>
        {
            Assert.That(flat.ControlCount, Is.EqualTo(4));
            Assert.That(flat.TopTrueVerticalDepthM!.P50, Is.EqualTo(100).Within(1e-9));
            Assert.That(flat.BaseTrueVerticalDepthM!.P50, Is.EqualTo(300).Within(1e-9));
            Assert.That(flat.TopTrueVerticalDepthM.P90, Is.LessThan(100));
            Assert.That(flat.TopTrueVerticalDepthM.P10, Is.GreaterThan(100));
            Assert.That(flat.NearestControls[0].GeologyEvidenceId, Does.StartWith("geology:"));
            Assert.That(flat.NearestControls[0].DistanceM, Is.LessThanOrEqualTo(flat.NearestControls[^1].DistanceM));
            Assert.That(flat.Limitations, Has.Some.Contains("lack survey TVD"));
        });

        foreach (JsonNode trajectory in package.Trajectories)
            trajectory["SurveyStationList"] = new JsonArray(Station(0, 0), Station(400, 200));
        FormationStructureEstimate surveyed = PetrophysicsAnalysisService.EstimateFormationStructure(package, "Target", 1000, 1000, 4);
        Assert.Multiple(() =>
        {
            Assert.That(surveyed.TopTrueVerticalDepthM!.P50, Is.EqualTo(50).Within(1e-9));
            Assert.That(surveyed.GrossThicknessM, Is.EqualTo(100).Within(1e-9));
            Assert.That(surveyed.NearestControls.All(c => c.DepthBasis == "survey-interpolated TVD"));
            Assert.That(JsonSerializer.Serialize(surveyed),
                Is.EqualTo(JsonSerializer.Serialize(PetrophysicsAnalysisService.EstimateFormationStructure(package, "Target", 1000, 1000, 4))));
        });
        Assert.That(PetrophysicsAnalysisService.EstimateFormationStructure(package, "Missing", 0, 0, 4).ControlCount, Is.Zero);

        static JsonObject Station(double md, double tvd) => new() { ["MD"] = md, ["TVD"] = tvd };
    }

    [Test]
    public void StructureEstimate_ExcludesUnsurveyedSidetracksAndMdOutliers_WhenEnoughSurveyedPicksExist()
    {
        AnalysisPackage surveyed = AnalysisConfigurationTests.CreatePackage(withTrajectories: true);
        for (int i = 0; i < surveyed.Trajectories.Count; i++)
        {
            surveyed.Trajectories[i]["WellBoreID"] = surveyed.WellBores[i]["MetaInfo"]!["ID"]!.DeepClone();
            surveyed.Trajectories[i]["SurveyStationList"] = new JsonArray(Station(0, 0), Station(400, 400 - 20 * i));
        }
        Guid sidetrackBore = Guid.Parse("00000004-0000-0000-0000-000000000090");
        var bores = surveyed.WellBores.Append(new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = sidetrackBore },
            ["WellID"] = surveyed.Wells[4]["MetaInfo"]!["ID"]!.DeepClone()
        }).ToList();
        var trajectories = surveyed.Trajectories.ToList();
        foreach ((int i, double east) in new[] { (92, 2100.0), (93, 1900.0), (94, 2000.0) })
        {
            bores.Add(new JsonObject { ["MetaInfo"] = new JsonObject { ["ID"] = Bore(i) } });
            trajectories.Add(new JsonObject
            {
                ["MetaInfo"] = new JsonObject { ["ID"] = Guid.Parse($"00000006-0000-0000-0000-{i:D12}") },
                ["WellBoreID"] = Bore(i),
                ["TieInPoint"] = new JsonObject { ["RiemannianEast"] = east, ["RiemannianNorth"] = 2100.0 }
            });
        }
        var geology = surveyed.GeologicalProperties.Concat(
        [
            Pick(91, sidetrackBore, 100, 1082),
            Pick(92, Bore(92), 110, 1092),
            Pick(93, Bore(93), 2600, 2800),
            Pick(94, Bore(94), 120, 310)
        ]).ToList();
        AnalysisPackage package = surveyed with { WellBores = bores, Trajectories = trajectories, GeologicalProperties = geology };

        (IReadOnlyList<FormationStructureControl> controls, int unsurveyed, int excluded) =
            PetrophysicsAnalysisService.ReadStructuralControls(package, "Target");
        FormationStructureEstimate estimate = PetrophysicsAnalysisService.EstimateFormationStructure(package, "Target", 2050, 2050, 4);

        Assert.Multiple(() =>
        {
            Assert.That(controls.Select(c => c.GeologyEvidenceId), Does.Not.Contain(Geology(91)).And.Not.Contain(Geology(92)).And.Not.Contain(Geology(93)));
            Assert.That(controls.Select(c => c.GeologyEvidenceId), Does.Contain(Geology(94)));
            Assert.That((controls.Count, unsurveyed, excluded), Is.EqualTo((6, 1, 3)));
            Assert.That(controls.Single(c => c.GeologyEvidenceId == Geology(94)).DepthBasis, Does.Contain("assumed vertical"));
            Assert.That(estimate.GrossThicknessM, Is.InRange(160, 200));
            Assert.That(estimate.TopTrueVerticalDepthM!.P50, Is.InRange(80, 120));
            Assert.That(estimate.Limitations, Has.Some.Contains("3 unsurveyed control(s) excluded"));
        });

        static Guid Bore(int i) => Guid.Parse($"00000004-0000-0000-0000-{i:D12}");
        static string Geology(int i) => $"geology:00000005-0000-0000-0000-{i:D12}";
        static JsonObject Station(double md, double tvd) => new() { ["MD"] = md, ["TVD"] = tvd };
        static JsonObject Pick(int i, Guid bore, double top, double @base) => new()
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = Guid.Parse($"00000005-0000-0000-0000-{i:D12}") },
            ["WellBoreID"] = bore,
            ["Petrophysics"] = new JsonObject
            {
                ["FormationIntervals"] = new JsonArray(new JsonObject
                {
                    ["FormationName"] = "Target",
                    ["TopDepth"] = new JsonObject { ["Value"] = top },
                    ["BaseDepth"] = new JsonObject { ["Value"] = @base }
                })
            }
        };
    }

    [Test]
    public void Validation_ReportsEveryRuleTheAgentMustSatisfy()
    {
        const string id = "geology:00000005-0000-0000-0000-000000000000";
        var allow = new HashSet<string> { id };
        ForecastDraft valid = Draft(id);
        Assert.That(ForecastDraftAgent.Problems(valid, allow), Is.Empty);
        (ForecastDraft Draft, string Problem)[] cases =
        [
            (valid with { FormationBaseTrueVerticalDepthM = new(90, 95, 99) }, "deeper than the top"),
            (valid with { TotalDepthM = 900 }, "totalDepthM"),
            (valid with { ExpectedPaydirtM = new(60, 50, 70) }, "expectedPaydirtM"),
            (valid with { ExpectedPaydirtM = new(60, 100, 250) }, "gross thickness"),
            (valid with { FormationTopTrueVerticalDepthM = new(110, 100, 120) }, "ordered"),
            (valid with { FluidClasses = [] }, "fluidClasses"),
            (valid with { ContactPredictions = [new(FluidContactType.GWC, new(1, 2, 3)), new(FluidContactType.GWC, new(1, 2, 3))] }, "contactPredictions"),
            (valid with { ProductionForecasts = [new(1, 10, 0, 0), new(3, 5, 0, 0), new(5, 20, 0, 0)] }, "cumulative"),
            (valid with { ProductionForecasts = [new(1, 0, 0, 0), new(5, 0, 0, 0)] }, "years 1, 3 and 5"),
            (valid with { CitedEvidenceIds = ["geology:00000005-0000-0000-0000-000000000001"] }, "not delivered"),
            (valid with { Rationale = "No inline citation.", UncertaintyAssumptions = ["Uncalibrated."] }, "appear inline"),
            (valid with { Rationale = $"{id} and well:00000003-0000-0000-0000-000000000001." }, "listed in citedEvidenceIds"),
            (valid with { Rationale = $"{id} see https://example.com" }, "URI"),
        ];
        foreach ((ForecastDraft draft, string problem) in cases)
            Assert.That(ForecastDraftAgent.Problems(draft, allow), Has.Some.Contains(problem), problem);
        Assert.That(ForecastDraftAgent.Problems(valid with { Rationale = $"{id} at candidate:03:04." }, allow), Is.Empty);
        Assert.That(ForecastDraftAgent.Problems(valid, allow, maximumTotalDepthM: 349), Has.Some.Contains("simulator coverage"));
        Assert.That(ForecastDraftAgent.Problems(valid, allow, maximumTotalDepthM: 350), Is.Empty);
        Assert.Throws<ScenarioApiException>(() => ForecastDraftAgent.Parse("{}", allow));
    }

    [Test]
    public async Task Service_DraftsFromFrozenScenarioSourceWithDeterministicStructureTool()
    {
        using var f = new FormationInterpretationFixture();
        f.Data.Source.Package.Field["StageA"] = new JsonObject { ["secret"] = "RAW-HIDDEN-TRUTH" };
        f.Rehash();
        ForecastDraftRequest request = await RequestAsync(f);
        PrepareForecastClient(f.Client);

        ForecastDraftResponse response = await Service(f).DraftAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.Version, Is.EqualTo(ForecastDraftAgent.Version));
            Assert.That(response.CandidateId, Is.EqualTo(request.CandidateId));
            Assert.That(response.StructureEstimate.ControlCount, Is.EqualTo(4));
            Assert.That(response.Draft.FormationTopTrueVerticalDepthM, Is.EqualTo(response.StructureEstimate.TopTrueVerticalDepthM));
            Assert.That(response.Draft.CitedEvidenceIds, Is.SubsetOf(response.StructureEstimate.NearestControls.Select(c => c.GeologyEvidenceId)));
            Assert.That(JsonNode.Parse(f.Client.Payload)!["version"]!.GetValue<string>(), Is.EqualTo("forecast-draft-brief-v1"));
            Assert.That(f.Client.Payload + f.Client.ToolPayload, Does.Not.Contain("RAW-HIDDEN-TRUTH"));
            Assert.That(f.Client.Options!.Tools!.Select(t => t.Name),
                Does.Contain("estimate_formation_structure_at_target").And.Contain("validate_forecast"));
        });
    }

    [Test]
    public async Task Service_RejectsStaleOrSealedRequestsBeforeCallingTheModel()
    {
        using var f = new FormationInterpretationFixture();
        ForecastDraftRequest request = await RequestAsync(f);
        ForecastDraftService service = Service(f);
        foreach (ForecastDraftRequest bad in new[]
        {
            request with { PackageSha256 = new string('b', 64) },
            request with { AnalysisSha256 = new string('b', 64) },
            request with { CandidateId = "candidate:99:99" },
            request with { ScenarioId = Guid.NewGuid() },
            request with { Configuration = request.Configuration with { PorosityCutoff = .2 } }
        })
            Assert.ThrowsAsync<ScenarioApiException>(() => service.DraftAsync(bad));
        await f.Data.ExecuteAsync($"UPDATE scenarios SET status='{ScenarioStatus.PredictionSealed}' WHERE scenario_id='{request.ScenarioId}';");
        Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => service.DraftAsync(request))!.StatusCode, Is.EqualTo(409));
        Assert.That(f.Client.Calls, Is.Zero);
    }

    [Test]
    public async Task Endpoint_RequiresOperatorAndReturnsDraft()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        ForecastDraftRequest request = await RequestAsync(f.Data);
        using HttpResponseMessage rejected = await f.PostForecastAsync(request, "missing-token");
        Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden).Or.EqualTo(HttpStatusCode.BadRequest));
        PrepareForecastClient(f.Data.Client);
        using HttpResponseMessage response = await f.PostForecastAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        Assert.That(JsonNode.Parse(body)!["draft"]!["fluidClasses"]![0]!.GetValue<string>(), Is.EqualTo("Water"));
    }

    private static ForecastDraftService Service(FormationInterpretationFixture f) =>
        new(f.Data.Scenarios, new SqliteScenarioStore(f.Data.ConnectionString), f.Data.Analysis, f.Agent, TimeProvider.System);

    private static async Task<ForecastDraftRequest> RequestAsync(FormationInterpretationFixture f)
    {
        HypothesisScope scope = await f.Data.ScenarioScopeAsync();
        AnalysisPackage package = await f.Data.Scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc);
        AnalysisResult result = f.Data.Analysis.Analyze(package, scope.ReservoirName, AnalysisConfiguration.Default);
        return new(scope.ScenarioId!.Value, result.Ranking[0].CandidateId, AnalysisConfiguration.Default, package.Sha256, result.AnalysisSha256);
    }

    private static void PrepareForecastClient(FormationInterpretationClient client)
    {
        client.InitialToolName = "estimate_formation_structure_at_target";
        client.Respond = (payload, _) =>
        {
            JsonNode estimate = JsonNode.Parse(payload)!["estimate"]!;
            string id = estimate["nearestControls"]![0]!["geologyEvidenceId"]!.GetValue<string>();
            QuantileValues top = estimate["topTrueVerticalDepthM"].Deserialize<QuantileValues>(ForecastDraftAgent.JsonOptions)!;
            QuantileValues @base = estimate["baseTrueVerticalDepthM"].Deserialize<QuantileValues>(ForecastDraftAgent.JsonOptions)!;
            return Task.FromResult(JsonSerializer.Serialize(Draft(id) with
            {
                FormationTopTrueVerticalDepthM = top, FormationBaseTrueVerticalDepthM = @base, TotalDepthM = @base.P10 + 50
            }, ForecastDraftAgent.JsonOptions));
        };
    }

    private static ForecastDraft Draft(string id) => new(350, new(40, 60, 80),
        new(90, 100, 110), new(290, 300, 310), [PredictedFluidClass.Water], [],
        [new(1, 0, 0, 0), new(3, 0, 0, 0), new(5, 0, 0, 0)],
        [$"Structure from inverse-distance picks such as {id}; uncalibrated."], [id],
        $"Visible pick {id} supports a tentative top near 100 m TVD.", ["Synthetic, uncalibrated estimate."]);
}
