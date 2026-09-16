namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class AppHostTopologyTests
{
    [Test]
    public void AnalysisResources_CannotReachHiddenReservoirTruth()
    {
        string source = File.ReadAllText(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Topology",
            "AppHost.cs"));
        string reservoir = Slice(source, ".AddProject(\"reservoir-simulation\"", "var analysisApi");
        string analysisApi = Slice(source, ".AddProject(\"analysis-api\"", "// Internal Stage B orchestration");
        string analysisWeb = Slice(source, ".AddViteApp(\"analysis-web\"", "var clusterUi");

        Assert.Multiple(() =>
        {
            Assert.That(reservoir, Does.Contain("RESERVOIR_SIMULATION_OPERATOR_KEY"));
            Assert.That(reservoir, Does.Not.Contain("WithExternalHttpEndpoints"));
            Assert.That(analysisApi, Does.Not.Contain("reservoir-simulation"));
            Assert.That(analysisApi, Does.Not.Contain("RESERVOIR_SIMULATION_OPERATOR_KEY"));
            Assert.That(analysisWeb, Does.Not.Contain("reservoir-simulation"));
            Assert.That(analysisWeb, Does.Not.Contain("RESERVOIR_SIMULATION_OPERATOR_KEY"));
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
