using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class StaticReservoirModelTests
{
    [Test]
    public void StaticModel_HonoursPicksFitsContactFromLogsAndSplitsNetRockByFluid()
    {
        AnalysisPackage package = AnalysisConfigurationTests.CreatePackage(withTrajectories: true);
        foreach (JsonNode trajectory in package.Trajectories)
            trajectory["SurveyStationList"] = new JsonArray(Station(0), Station(400));
        for (int i = 0; i < package.GeologicalProperties.Count; i++)
            package.GeologicalProperties[i]["Petrophysics"]!["LogRuns"] = new JsonArray(LogRun(gasBaseMd: 180, tightBelowMd: i == 4 ? 250 : 999));

        StaticReservoirModel model = PetrophysicsAnalysisService.BuildStaticModel(package, "Target", AnalysisConfiguration.Default,
            [new("near-corner", 0, 0), new("centre", 1000, 1000), new("outside", 50_000, 0)])!;
        ReservoirModelColumn corner = model.Targets[0], centre = model.Targets[1];

        Assert.Multiple(() =>
        {
            Assert.That(model.Method, Is.EqualTo(PetrophysicsAnalysisService.StaticModelMethod));
            Assert.That(model.Contacts.Select(c => (c.Type, c.TrueVerticalDepthM)), Is.EqualTo(new[] { ("GasWaterContact", 185.0) }));
            Assert.That(model.Contacts[0].MisclassifiedSamples, Is.Zero);
            Assert.That(model.Inputs.Hydrocarbon, Is.EqualTo("gas"));
            Assert.That((corner.TopTvdM, corner.BaseTvdM, corner.NetRockM), Is.EqualTo((100.0, 300.0, 200.0)));
            Assert.That(corner.GasM, Is.EqualTo(85).Within(0.2));
            Assert.That(corner.WaterM, Is.EqualTo(115).Within(0.2));
            Assert.That(corner.FluidAtTop, Is.EqualTo("gas"));
            Assert.That(centre.NetRockM, Is.LessThan(200), "the tight lower section at the centre well reduces net rock nearby");
            Assert.That(centre.GasM, Is.EqualTo(85).Within(0.5), "tight rock is below the contact, so gas is unchanged");
            Assert.That(centre.PropertyEvidenceIds, Has.Count.EqualTo(4));
            Assert.That(model.Targets[2].InsideModel, Is.False);
            Assert.That(model.Grid.TopTvdM, Has.Length.EqualTo(model.Grid.Columns * model.Grid.Rows));
            Assert.That(model.Grid.MinEastingM, Is.EqualTo(-PetrophysicsAnalysisService.StaticModelPaddingM));
            Assert.That(model.Volumes.GasNetRockM3, Is.GreaterThan(0).And.LessThan(model.Volumes.NetRockM3));
            Assert.That(model.Wells.Count(w => w.Control && w.Logged), Is.EqualTo(5));
        });
        Assert.That(PetrophysicsAnalysisService.BuildStaticModel(package, "Missing", AnalysisConfiguration.Default), Is.Null);
    }

    [Test]
    public void TargetFluid_ScalesCandidatePayByModelShareAndWellPayByLogShare_WithoutChangingDefaultFingerprint()
    {
        AnalysisPackage package = AnalysisConfigurationTests.CreatePackage(withTrajectories: true);
        foreach (JsonNode trajectory in package.Trajectories)
            trajectory["SurveyStationList"] = new JsonArray(Station(0), Station(400));
        for (int i = 0; i < package.GeologicalProperties.Count; i++)
            package.GeologicalProperties[i]["Petrophysics"]!["LogRuns"] = new JsonArray(LogRun(gasBaseMd: 180, tightBelowMd: 999));
        var service = new PetrophysicsAnalysisService(TimeProvider.System);
        AnalysisResult rock = service.Analyze(package, "Target", AnalysisConfiguration.Default);
        AnalysisResult gas = service.Analyze(package, "Target", AnalysisConfiguration.Default with { TargetFluid = "Gas" });
        AnalysisResult oil = service.Analyze(package, "Target", AnalysisConfiguration.Default with { TargetFluid = "Oil" });
        RankedCandidate rockTop = rock.CandidateGrid.First(p => p.Prediction is not null).Prediction!;
        RankedCandidate gasSame = gas.CandidateGrid.Single(p => p.CandidateId.EndsWith(rockTop.CandidateId, StringComparison.Ordinal)).Prediction!;
        ReservoirModelColumn column = PetrophysicsAnalysisService.BuildStaticModel(package, "Target", AnalysisConfiguration.Default,
            [new("x", rockTop.EastingM, rockTop.NorthingM)])!.Targets[0];

        Assert.Multiple(() =>
        {
            Assert.That(System.Text.Json.JsonSerializer.Serialize(AnalysisConfiguration.Default), Does.Not.Contain("argetFluid"));
            Assert.That(rockTop.TargetFluidFraction, Is.Null);
            Assert.That(gasSame.TargetFluidFraction, Is.EqualTo(column.GasM / column.NetRockM).Within(1e-3));
            Assert.That(gasSame.P50NetPayM, Is.EqualTo(rockTop.P50NetPayM * gasSame.TargetFluidFraction!.Value).Within(1e-9));
            Assert.That(gasSame.RelativeUncertainty, Is.EqualTo(rockTop.RelativeUncertainty).Within(1e-9));
            Assert.That(oil.CandidateGrid.Where(p => p.Prediction is not null).All(p => p.Prediction!.P50NetPayM == 0), "No oil in the logs: no oil pay anywhere.");
            Assert.That(gas.WellSummaries[0].NetPayThicknessM, Is.EqualTo(rock.WellSummaries[0].NetPayThicknessM * 9 / 21).Within(1e-9),
                "Well gas pay = rock pay x share of good-rock log readings that are gas (9 of 21).");
            Assert.That(gas.Methodology.Label, Does.Contain("gas"));
        });
        Assert.Throws<ScenarioApiException>(() => AnalysisConfiguration.Validate(AnalysisConfiguration.Default with { TargetFluid = "Water" }));
    }

    private static JsonObject Station(double depth) => new() { ["MD"] = depth, ["TVD"] = depth };

    private static JsonObject LogRun(double gasBaseMd, double tightBelowMd)
    {
        double[] depths = Enumerable.Range(0, 21).Select(i => 100.0 + i * 10).ToArray();
        JsonObject Curve(string mnemonic, string unit, Func<double, double> value) => new()
        {
            ["CanonicalMnemonic"] = mnemonic, ["CanonicalUnit"] = unit,
            ["Values"] = new JsonArray(depths.Select(d => (JsonNode)value(d)).ToArray())
        };
        return new JsonObject
        {
            ["DepthValues"] = new JsonArray(depths.Select(d => (JsonNode)d).ToArray()),
            ["Curves"] = new JsonArray(
                Curve("PHIE", "fraction", d => d > tightBelowMd ? 0.05 : 0.25),
                Curve("PERM", "m2", _ => 1e-13),
                Curve("SG", "fraction", d => d <= gasBaseMd ? 0.8 : 0),
                Curve("SO", "fraction", _ => 0),
                Curve("SW", "fraction", d => d <= gasBaseMd ? 0.2 : 1))
        };
    }
}
