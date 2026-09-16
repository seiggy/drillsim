using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class FormationInterpretationToolTests
{
    [Test]
    public async Task BriefStartsSmallButExplicitFullContextRetainsEveryPreparedRecord()
    {
        using var f = new FormationInterpretationFixture();
        FormationInterpretationRequest request = await f.RequestAsync();
        FormationInterpretationEvidence evidence = f.BuildEvidence(request);
        var tools = new FormationInterpretationTools(evidence);
        JsonNode brief = JsonNode.Parse(tools.Brief)!;
        Assert.Multiple(() =>
        {
            Assert.That(Encoding.UTF8.GetByteCount(tools.Brief), Is.LessThanOrEqualTo(FormationInterpretationTools.MaximumBriefBytes));
            Assert.That(tools.Brief.Length, Is.LessThan(evidence.Json.Length / 2));
            Assert.That(brief["geology"], Is.Null);
            Assert.That(brief["wells"], Is.Null);
            Assert.That(brief["citationAllowlist"], Is.Null);
            Assert.That(brief["packageSha256"]!.GetValue<string>(), Is.EqualTo(request.PackageSha256));
            Assert.That(brief["analysisSha256"]!.GetValue<string>(), Is.EqualTo(request.AnalysisSha256));
            Assert.That(tools.CreateFunctions().Select(tool => tool.Name), Is.EquivalentTo(new[]
            {
                "summarize_formation", "analyze_formation_screening", "search_formation_evidence",
                "inspect_formation_evidence", "read_formation_notes", "read_full_formation_context"
            }));
        });
        string full = tools.ReadFullContext();
        Assert.That(JsonNode.DeepEquals(JsonNode.Parse(full), JsonNode.Parse(evidence.Json)), Is.True);
        Assert.That(tools.DeliveredCitations, Is.EquivalentTo(evidence.CitationAllowlist));
        Assert.DoesNotThrow(tools.ValidateCompletion);
    }

    [Test]
    public async Task SummaryGroupsAllPicksWithoutCombiningDifferentReferencesOrUnits()
    {
        using var f = new FormationInterpretationFixture();
        for (int index = 0; index < f.Data.Source.Package.GeologicalProperties.Count; index++)
        {
            JsonNode interval = f.Data.Source.Package.GeologicalProperties[index]["Petrophysics"]!["FormationIntervals"]![0]!;
            interval["TopDepth"] = new JsonObject
            {
                ["Value"] = 100 + index * 10, ["Reference"] = index == 0 ? 1 : 0,
                ["Unit"] = index == 0 ? "ft" : "m", ["Datum"] = index == 0 ? "MSL" : "RKB", ["PositiveDown"] = true
            };
        }
        f.Rehash();
        var tools = new FormationInterpretationTools(f.BuildEvidence(await f.RequestAsync(selectCandidate: false)));
        JsonNode summary = JsonNode.Parse(tools.SummarizeFormation())!;
        JsonNode[] tops = summary["depthGroups"]!["rows"]!.AsArray().OfType<JsonNode>()
            .Where(row => row["kind"]!.GetValue<string>() == "interval-top").ToArray();
        JsonNode md = tops.Single(row => row["reference"]!.GetValue<string>() == "MeasuredDepth");
        JsonNode tvd = tops.Single(row => row["reference"]!.GetValue<string>() == "TrueVerticalDepth");
        Assert.Multiple(() =>
        {
            Assert.That(md["pickCount"]!.GetValue<int>(), Is.EqualTo(4));
            Assert.That(md["minimum"]!.GetValue<double>(), Is.EqualTo(110));
            Assert.That(md["maximum"]!.GetValue<double>(), Is.EqualTo(140));
            Assert.That(md["unit"]!.GetValue<string>(), Is.EqualTo("m"));
            Assert.That(tvd["pickCount"]!.GetValue<int>(), Is.EqualTo(1));
            Assert.That(tvd["minimum"]!.GetValue<double>(), Is.EqualTo(100));
            Assert.That(tvd["unit"]!.GetValue<string>(), Is.EqualTo("ft"));
            Assert.That(summary["actualVisibleCounts"]!["geologyRecords"]!.GetValue<int>(), Is.EqualTo(5));
            Assert.That(summary["depthComparison"]!.GetValue<string>(), Does.Contain("not structural elevation"));
        });
    }

    [Test]
    public async Task ScreeningUsesExactAppliedAnalysisAndPagesControls()
    {
        using var f = new FormationInterpretationFixture();
        AnalysisConfiguration configuration = AnalysisConfiguration.Default with { PorosityCutoff = .25, IdwNeighborCount = 3 };
        FormationInterpretationRequest request = await f.RequestAsync(configuration: configuration);
        var tools = new FormationInterpretationTools(f.BuildEvidence(request));
        JsonNode screening = JsonNode.Parse(tools.AnalyzeScreening(limit: 1))!;
        AnalysisResult expected = f.Data.Analysis.Analyze(f.Data.Source.Package, request.Scope.ReservoirName, configuration);
        Assert.Multiple(() =>
        {
            Assert.That(screening["configuration"]!["porosityCutoff"]!.GetValue<double>(), Is.EqualTo(.25));
            Assert.That(screening["configuration"]!["idwNeighborCount"]!.GetValue<int>(), Is.EqualTo(3));
            Assert.That(screening["wellControls"]!["total"]!.GetValue<int>(), Is.EqualTo(expected.WellSummaries.Count));
            Assert.That(screening["wellControls"]!["returned"]!.GetValue<int>(), Is.EqualTo(1));
            Assert.That(screening["wellControls"]!["nextOffset"]!.GetValue<int>(), Is.EqualTo(1));
            Assert.That(screening["wellControls"]!["rows"]![0]!["netPayThicknessM"]!.GetValue<double>(),
                Is.EqualTo(expected.WellSummaries[0].NetPayThicknessM));
            Assert.That(screening["selectedCandidate"]!["candidateId"]!.GetValue<string>(), Is.EqualTo(request.SelectedCandidateId));
        });
        Assert.DoesNotThrow(tools.ValidateCompletion);
    }

    [Test]
    public async Task SearchAndInspectionAreFrozenScopedAndDoNotGrantUnretrievedCitations()
    {
        using var f = new FormationInterpretationFixture();
        f.Data.Source.Package.Field["StageA"] = "NEVER-EXPOSE-HIDDEN-TRUTH";
        f.Data.Source.Package.GeologicalProperties[0]["UndisclosedModel"] = "NEVER-EXPOSE-HIDDEN-TRUTH";
        f.Rehash();
        FormationInterpretationEvidence evidence = f.BuildEvidence(await f.RequestAsync());
        var tools = new FormationInterpretationTools(evidence);
        JsonNode firstPage = JsonNode.Parse(tools.SearchEvidence(limit: 1))!;
        string id = firstPage["matches"]!["rows"]![0]!["evidenceId"]!.GetValue<string>();
        Assert.That(firstPage["matches"]!["nextOffset"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(tools.DeliveredCitations, Does.Contain(id));
        string unreturned = evidence.CitationAllowlist.First(value => !tools.DeliveredCitations.Contains(value));
        Assert.Throws<ScenarioApiException>(() => FormationInterpretationAgent.Parse(
            JsonSerializer.Serialize(new FormationInterpretationDraft("Name", "Rationale",
                $"Unretrieved evidence [{unreturned}].", [unreturned], ["Review the evidence."]),
                FormationInterpretationAgent.JsonOptions), tools.DeliveredCitations));
        Assert.Throws<ArgumentException>(() => tools.InspectEvidence($"geology:{Guid.NewGuid():D}"));
        Assert.Throws<ArgumentException>(() => tools.SearchEvidence(kind: "hidden-truth"));
        Assert.Throws<ArgumentException>(() => tools.SearchEvidence(query: new string('x', 81)));
        string before = tools.ReadFullContext();
        f.Data.Source.Package.Field["Name"] = "CHANGED-AFTER-BINDING";
        f.Data.Source.Package.GeologicalProperties[0]["Name"] = "CHANGED-AFTER-BINDING";
        Assert.That(tools.ReadFullContext(), Is.EqualTo(before));
        Assert.That(before, Does.Not.Contain("NEVER-EXPOSE-HIDDEN-TRUTH").And.Not.Contain("CHANGED-AFTER-BINDING"));
        JsonNode all = JsonNode.Parse(tools.SearchEvidence(limit: FormationInterpretationTools.MaximumPageSize))!;
        Assert.That(all["matches"]!["rows"]!.AsArray(), Has.Count.EqualTo(5));
    }

    [Test]
    public async Task InspectionPagesQualitySummariesWithoutRawSamples()
    {
        using var f = new FormationInterpretationFixture();
        JsonNode record = f.Data.Source.Package.GeologicalProperties[0];
        record["Petrophysics"]!["LogRuns"] = JsonNode.Parse("""
            [{"DepthValues":[100,110,120],"Curves":[
              {"CanonicalMnemonic":"PHI","Values":[0.2,0.9,0.4],"QualityFlags":[null,"bad-hole",null]},
              {"CanonicalMnemonic":"PERM","Values":[1,2,3],"NullFlags":[true,true,true]}
            ]}]
            """);
        f.Rehash();
        var tools = new FormationInterpretationTools(f.BuildEvidence(await f.RequestAsync()));
        string id = EvidenceCatalog.TryCreateId(EvidenceCatalog.Geology, record)!;
        JsonNode page = JsonNode.Parse(tools.InspectEvidence(id, "logs", limit: 1))!;
        JsonNode row = page["curves"]!["rows"]![0]!;
        string[] columns = row["columns"]!.AsArray().Select(value => value!.GetValue<string>()).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(page["curves"]!["total"]!.GetValue<int>(), Is.EqualTo(2));
            Assert.That(page["curves"]!["nextOffset"]!.GetValue<int>(), Is.EqualTo(1));
            Assert.That(row["values"]![Array.IndexOf(columns, "validValueCount")]!.GetValue<int>(), Is.EqualTo(2));
            Assert.That(row["values"]![Array.IndexOf(columns, "valueRange")]!.ToJsonString(), Is.EqualTo("[2,0.2,0.4]"));
            Assert.That(page.ToJsonString(), Does.Not.Contain("DepthValues").And.Not.Contain("QualityFlags").And.Not.Contain("0.9"));
        });
        JsonNode last = JsonNode.Parse(tools.InspectEvidence(id, "logs", offset: 1, limit: 1))!;
        Assert.That(last["curves"]!["nextOffset"], Is.Null);
    }

    [Test]
    public async Task LongNotesRemainAvailableWithoutFillingTheBriefOrAuthorizingCitations()
    {
        using var f = new FormationInterpretationFixture();
        FormationInterpretationRequest request = await f.RequestAsync();
        string evidenceId = EvidenceCatalog.TryCreateId(EvidenceCatalog.Geology, f.Data.Source.Package.GeologicalProperties[0])!;
        request = request with { Notes = new("Title", new string('r', 6000), "",
            [new(evidenceId, "Engineer annotation, not a formation observation.")]) };
        var tools = new FormationInterpretationTools(f.BuildEvidence(request));
        JsonNode brief = JsonNode.Parse(tools.Brief)!;
        Assert.That(brief["humanNotesUntrusted"], Is.Null);
        Assert.That(brief["notesAvailable"]!["included"]!.GetValue<bool>(), Is.False);
        Assert.That(JsonNode.Parse(tools.ReadNotes("rationale", offset: 4000))!["text"]!.GetValue<string>(), Has.Length.EqualTo(2000));
        Assert.That(tools.ReadNotes("control:0"), Does.Contain(evidenceId));
        Assert.That(tools.DeliveredCitations, Does.Not.Contain(evidenceId));
        Assert.Throws<ScenarioApiException>(tools.ValidateCompletion);
        Assert.Throws<ArgumentException>(() => tools.ReadNotes("control:999"));
        Assert.Throws<ArgumentException>(() => tools.ReadNotes("rationale", offset: 6001));
        Assert.That(JsonNode.Parse(tools.ReadFullContext())!["humanNotesUntrusted"]!["rationale"]!.GetValue<string>(), Has.Length.EqualTo(6000));
        Assert.DoesNotThrow(tools.ValidateCompletion);
    }

    [Test]
    public async Task BroadSafetyLimitsAndCancellationRemainExplicit()
    {
        using var f = new FormationInterpretationFixture();
        FormationInterpretationEvidence evidence = f.BuildEvidence(await f.RequestAsync());
        var cancelled = new FormationInterpretationTools(evidence);
        Assert.Throws<OperationCanceledException>(() => cancelled.ReadFullContext(new CancellationToken(true)));
        Assert.That(cancelled.Calls, Is.Zero);
        var tools = new FormationInterpretationTools(evidence);
        Assert.Throws<ArgumentException>(() => tools.SearchEvidence(limit: FormationInterpretationTools.MaximumPageSize + 1));
        for (int count = tools.Calls; count < FormationInterpretationTools.MaximumCalls; count++)
            tools.SearchEvidence(query: "no-such-evidence", limit: 1);
        Assert.Throws<ArgumentException>(() => tools.SearchEvidence());
        Assert.That(Assert.Throws<ScenarioApiException>(tools.ValidateCompletion)!.Title, Is.EqualTo("Formation tool budget exceeded"));
    }

    [Test]
    public async Task NotePagesPreserveUnicodeCharacters()
    {
        using var f = new FormationInterpretationFixture();
        FormationInterpretationRequest request = await f.RequestAsync();
        string note = new string('x', 1999) + "\U0001F680end";
        request = request with { Notes = request.Notes with { Rationale = note } };
        var tools = new FormationInterpretationTools(f.BuildEvidence(request));
        JsonNode first = JsonNode.Parse(tools.ReadNotes("rationale"))!;
        int next = first["nextOffset"]!.GetValue<int>();
        JsonNode second = JsonNode.Parse(tools.ReadNotes("rationale", next))!;
        Assert.That(first["text"]!.GetValue<string>() + second["text"]!.GetValue<string>(), Is.EqualTo(note));
        Assert.Throws<ArgumentException>(() => tools.ReadNotes("rationale", offset: 2000));
        Assert.Throws<ArgumentException>(() => tools.ReadNotes("rationale", offset: 1999, length: 1));
    }

    [Test]
    public async Task AgentCanChooseTargetedInspectionWithoutAWholeFormationSummary()
    {
        using var f = new FormationInterpretationFixture();
        string id = EvidenceCatalog.TryCreateId(EvidenceCatalog.Geology, f.Data.Source.Package.GeologicalProperties[0])!;
        f.Client.InitialToolName = "inspect_formation_evidence";
        f.Client.InitialToolArguments = new Dictionary<string, object?> { ["evidenceId"] = id, ["section"] = "formations" };
        f.Client.Respond = (_, _) => Task.FromResult(JsonSerializer.Serialize(
            new FormationInterpretationDraft("Targeted review", "Review this control.",
                $"The retrieved control [{id}] requires datum review.", [id], ["No structural continuity is established."]),
            FormationInterpretationAgent.JsonOptions));
        FormationInterpretationResponse response = await f.Service.DraftAsync(await f.RequestAsync());
        Assert.That(response.Draft.CitedEvidenceIds, Is.EqualTo(new[] { id }));
        Assert.That(f.Client.Calls, Is.EqualTo(2));
        Assert.That(f.Client.ToolPayload, Does.Contain("\"formations\"").And.Not.Contain("\"depthGroups\""));
    }

    [Test]
    public async Task AgentCanChooseFullContextThroughNativeFunctionInvocation()
    {
        using var f = new FormationInterpretationFixture();
        f.Client.InitialToolName = "read_full_formation_context";
        FormationInterpretationResponse response = await f.Service.DraftAsync(await f.RequestAsync());
        Assert.Multiple(() =>
        {
            Assert.That(response.Draft.CitedEvidenceIds, Has.Count.EqualTo(1));
            Assert.That(f.Client.Calls, Is.EqualTo(2));
            Assert.That(f.Client.Payload, Does.Contain("formation-interpretation-brief-v2").And.Not.Contain("\"logRuns\""));
            Assert.That(f.Client.ToolPayload, Does.Contain("formation-interpretation-evidence-v1").And.Contain("\"logRuns\""));
        });
        Assert.That(await f.Data.Service.ListAsync(f.Data.LiveScope, null), Is.Empty);
    }
}
