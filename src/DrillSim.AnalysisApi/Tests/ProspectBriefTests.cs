using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class ProspectBriefTests
{
    private const string Geology = "geology:00000005-0000-0000-0000-000000000000";
    private const string Well = "well:00000003-0000-0000-0000-000000000000";

    [Test]
    public void Validation_RejectsInlineIdsUnknownCitationsBadCountsAndEnums()
    {
        var allow = new HashSet<string> { Geology, Well };
        ProspectBrief valid = Brief(Geology);
        Assert.That(ProspectBriefAgent.Problems(valid, allow), Is.Empty);
        ProspectBriefReason reason = valid.Reasons[0];
        ProspectBriefRisk risk = valid.Risks[0];
        (ProspectBrief Brief, string Problem)[] cases =
        [
            (valid with { Headline = new string('a', 161) }, "headline"),
            (valid with { Headline = "Two lines\nare not plain." }, "headline"),
            (valid with { Recommendation = (ProspectRecommendation)99 }, "recommendation"),
            (valid with { Reasons = [reason] }, "reasons needs 2 to 4"),
            (valid with { Reasons = [reason, reason, reason, reason, reason] }, "reasons needs 2 to 4"),
            (valid with { Risks = [] }, "risks needs 1 to 4"),
            (valid with { Risks = [risk with { Severity = (ProspectRiskSeverity)7 }] }, "severity"),
            (valid with { NextChecks = ["a", "b", "c", "d"] }, "nextChecks"),
            (valid with { Reasons = [reason with { Text = $"Good rock at {Geology}." }, reason] }, "must not contain evidence IDs"),
            (valid with { Reasons = [reason with { Text = "Well 00000003-0000-0000-0000-000000000000 is close." }, reason] }, "must not contain evidence IDs"),
            (valid with { Risks = [risk with { Text = "Target candidate:03:04 is far." }] }, "must not contain evidence IDs"),
            (valid with { Risks = [risk with { Text = "See https://example.com for more." }] }, "URLs"),
            (valid with { Risks = [risk with { Text = "**Bold** risk." }] }, "Markdown"),
            (valid with { Reasons = [reason with { CitedEvidenceIds = [] }, reason] }, "1 to 4 unique"),
            (valid with { Reasons = [reason with { CitedEvidenceIds = [Geology, Geology] }, reason] }, "1 to 4 unique"),
            (valid with { Risks = [risk with { CitedEvidenceIds = ["geology:00000005-0000-0000-0000-000000000009"] }] }, "not returned by tools"),
        ];
        foreach ((ProspectBrief brief, string problem) in cases)
            Assert.That(ProspectBriefAgent.Problems(brief, allow), Has.Some.Contains(problem), problem);
        Assert.That(ProspectBriefAgent.Problems(valid, allow, targetInside: false), Has.Some.Contains("NotYet"));
        Assert.That(ProspectBriefAgent.Problems(valid with { Recommendation = ProspectRecommendation.NotYet }, allow, false), Is.Empty);

        string json = JsonSerializer.Serialize(valid, ProspectBriefAgent.JsonOptions);
        Assert.That(JsonNode.Parse(json)!["recommendation"]!.GetValue<string>(), Is.EqualTo("DrillWithCaution"));
        Assert.That(ProspectBriefAgent.Parse(json, allow), Is.Not.Null);
        Assert.Throws<ScenarioApiException>(() => ProspectBriefAgent.Parse(json.Replace("DrillWithCaution", "Maybe"), allow));
        Assert.Throws<ScenarioApiException>(() => ProspectBriefAgent.Parse(json, new HashSet<string>()));
        Assert.Throws<ScenarioApiException>(() => ProspectBriefAgent.Parse("{}", allow));
    }

    [Test]
    public async Task Service_RejectsStaleOrIneligibleRequestsBeforeCallingTheModel()
    {
        using var f = new FormationInterpretationFixture();
        ProspectBriefRequest request = await RequestAsync(f);
        AnalysisResult result = f.Data.Analysis.Analyze(f.Data.Source.Package, "Target", AnalysisConfiguration.Default);
        string excluded = result.CandidateGrid.First(point => point.Status != "eligible").CandidateId;
        ProspectBriefService service = Service(f);
        (ProspectBriefRequest Request, int Status)[] cases =
        [
            (request with { PackageSha256 = new string('b', 64) }, 409),
            (request with { AnalysisSha256 = new string('b', 64) }, 409),
            (request with { CandidateId = "candidate:99:99" }, 409),
            (request with { CandidateId = excluded }, 409),
            (request with { Configuration = request.Configuration with { PorosityCutoff = .2 } }, 409),
            (request with { ReservoirName = "Missing" }, 409),
            (request with { ScenarioId = Guid.NewGuid() }, 400),
            (request with { ScenarioId = Guid.NewGuid(), AsOfUtc = HypothesisFixture.Instant }, 404),
            (request with { PackageSha256 = "short" }, 400),
        ];
        foreach ((ProspectBriefRequest bad, int status) in cases)
            Assert.That(Assert.ThrowsAsync<ScenarioApiException>(() => service.ExplainAsync(bad))!.StatusCode, Is.EqualTo(status), bad.ToString());
        Assert.That(f.Client.Calls, Is.Zero);
    }

    [Test]
    public async Task Service_ExplainsLiveAndScenarioTargetsWithCitedStructuredBrief()
    {
        using var f = new FormationInterpretationFixture();
        f.Data.Source.Package.Field["StageA"] = new JsonObject { ["secret"] = "RAW-HIDDEN-TRUTH" };
        f.Rehash();
        ProspectBriefRequest request = await RequestAsync(f);
        PrepareBriefClient(f.Client);

        ProspectBriefResponse response = await Service(f).ExplainAsync(request);

        JsonNode payload = JsonNode.Parse(f.Client.Payload)!;
        Assert.Multiple(() =>
        {
            Assert.That(response.Version, Is.EqualTo("prospect-brief-v1"));
            Assert.That(response.PromptVersion, Is.EqualTo("prospect-brief-prompt-v1"));
            Assert.That(response.FieldId, Is.EqualTo(request.FieldId));
            Assert.That(response.ScenarioId, Is.Null);
            Assert.That(response.AsOfUtc, Is.Null);
            Assert.That(response.CandidateId, Is.EqualTo(request.CandidateId));
            Assert.That(response.AnalysisSha256, Is.EqualTo(request.AnalysisSha256));
            Assert.That(response.StructureEstimate.ControlCount, Is.EqualTo(4));
            Assert.That(response.Brief.Recommendation, Is.EqualTo(ProspectRecommendation.DrillWithCaution));
            Assert.That(response.Brief.Reasons.SelectMany(r => r.CitedEvidenceIds).Distinct(),
                Is.SubsetOf(response.StructureEstimate.NearestControls.Select(c => c.GeologyEvidenceId)));
            Assert.That(payload["version"]!.GetValue<string>(), Is.EqualTo("prospect-brief-input-v1"));
            Assert.That(payload["target"]!["rank"]!.GetValue<int>(), Is.EqualTo(1));
            Assert.That(payload["simulatorCoverage"]!["targetInside"]!.GetValue<bool>(), Is.True);
            Assert.That(f.Client.Payload + f.Client.ToolPayload, Does.Not.Contain("RAW-HIDDEN-TRUTH"));
            Assert.That(f.Client.Options!.Tools!.Select(t => t.Name),
                Does.Contain("estimate_formation_structure_at_target").And.Contain("validate_prospect_brief"));
        });

        HypothesisScope scope = await f.Data.ScenarioScopeAsync();
        AnalysisPackage scenarioPackage = await f.Data.Scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc);
        AnalysisResult scenarioResult = f.Data.Analysis.Analyze(scenarioPackage, scope.ReservoirName, AnalysisConfiguration.Default);
        ProspectBriefResponse scenario = await Service(f).ExplainAsync(request with
        {
            ScenarioId = scope.ScenarioId, AsOfUtc = scope.AsOfUtc, PackageSha256 = scenarioPackage.Sha256,
            AnalysisSha256 = scenarioResult.AnalysisSha256, CandidateId = scenarioResult.Ranking[0].CandidateId
        });
        Assert.That(scenario.ScenarioId, Is.EqualTo(scope.ScenarioId));
        Assert.That(scenario.AsOfUtc, Is.EqualTo(scope.AsOfUtc));
    }

    [Test]
    public async Task Endpoint_RequiresOperatorAndReturnsBrief()
    {
        await using var f = await FormationInterpretationHttpFixture.StartAsync();
        Assert.That((await f.Client.GetFromJsonAsync<FormationInterpretationStatus>("/api/prospect-briefs/status"))!.Configured, Is.True);
        ProspectBriefRequest request = await RequestAsync(f.Data);
        using HttpResponseMessage rejected = await f.PostProspectBriefAsync(request, "missing-token");
        Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden).Or.EqualTo(HttpStatusCode.BadRequest));
        PrepareBriefClient(f.Data.Client);
        using HttpResponseMessage response = await f.PostProspectBriefAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);
        JsonNode json = JsonNode.Parse(body)!;
        Assert.Multiple(() =>
        {
            Assert.That(json["version"]!.GetValue<string>(), Is.EqualTo("prospect-brief-v1"));
            Assert.That(json["scenarioId"], Is.Null);
            Assert.That(json["brief"]!["recommendation"]!.GetValue<string>(), Is.EqualTo("DrillWithCaution"));
            Assert.That(json["brief"]!["risks"]![0]!["severity"]!.GetValue<string>(), Is.EqualTo("Medium"));
            Assert.That(json["structureEstimate"]!["controlCount"]!.GetValue<int>(), Is.EqualTo(4));
        });
        using HttpResponseMessage extra = await f.PostProspectBriefAsync(new { request.FieldId, unexpected = true });
        Assert.That(extra.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private static ProspectBriefService Service(FormationInterpretationFixture f) =>
        new(f.Data.Scenarios, new SqliteScenarioStore(f.Data.ConnectionString), f.Data.Analysis, f.Agent, TimeProvider.System);

    private static async Task<ProspectBriefRequest> RequestAsync(FormationInterpretationFixture f)
    {
        HypothesisScope scope = f.Data.LiveScope;
        AnalysisPackage package = await f.Data.Scenarios.GetPackageAsync(scope.FieldId, null, null);
        AnalysisResult result = f.Data.Analysis.Analyze(package, scope.ReservoirName, AnalysisConfiguration.Default);
        return new(scope.FieldId, scope.ReservoirName, null, null, AnalysisConfiguration.Default,
            package.Sha256, result.AnalysisSha256, result.Ranking[0].CandidateId);
    }

    private static void PrepareBriefClient(FormationInterpretationClient client)
    {
        client.InitialToolName = "estimate_formation_structure_at_target";
        client.Respond = (payload, _) =>
        {
            JsonNode control = JsonNode.Parse(payload)!["estimate"]!["nearestControls"]![0]!;
            return Task.FromResult(JsonSerializer.Serialize(Brief(control["geologyEvidenceId"]!.GetValue<string>()),
                ProspectBriefAgent.JsonOptions));
        };
    }

    private static ProspectBrief Brief(string id) => new(
        "Good rock and close well control, but structure depth is uncertain.",
        ProspectRecommendation.DrillWithCaution,
        [
            new("Nearby wells measured porosity (how much pore space the rock has) above the cutoff.", [id]),
            new("The model estimate puts about 20 m of qualifying rock here; this is not proof of oil or gas.", [id])
        ],
        [new("The formation depth is a model estimate from a few wells and may be off by tens of metres.", ProspectRiskSeverity.Medium, [id])],
        ["Check survey coverage of the nearest wells."]);
}
