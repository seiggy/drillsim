using DrillSim.AnalysisApi.Services;
using static DrillSim.AnalysisApi.Services.DrillSimAssistant;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class AssistantChartTests
{
    private static readonly ChartSeries[] Wells = [new("Net pay", [new("31/2-1", 80.3), new("31/5-4 S", 100.3)])];

    [Test]
    public void ValidateChart_AcceptsWellFormedCharts()
    {
        Assert.That(ValidateChart("Net pay by well (m)", "bar", "Well", "Net pay (m)", Wells), Is.Empty);
        Assert.That(ValidateChart("Gas rate", "line", "Month", "Rate (m3)", [new("Gas", [new("1", 2), new("2.5e1", 3)])]), Is.Empty);
    }

    [Test]
    public void ValidateChart_ReportsFixableProblems()
    {
        Assert.That(ValidateChart("t", "pie", "x", "y", Wells), Has.Some.Contains("kind"));
        Assert.That(ValidateChart("t", "line", "x", "y", Wells), Has.Some.Contains("numeric x"));
        Assert.That(ValidateChart("t", "bar", "x", "y", [new("s", [new("a", double.NaN)])]), Has.Some.Contains("finite y"));
        Assert.That(ValidateChart("t", "bar", "x", "y", [.. Enumerable.Repeat(Wells[0], 7)]), Has.Some.Contains("1-6"));
        Assert.That(ValidateChart("", "bar", "", "y", null), Has.Count.EqualTo(3));
        Assert.That(ValidateChart("t", "bar", "x", "y",
            [new("s", [.. Enumerable.Range(0, 31).Select(index => new ChartPoint($"c{index}", index))])]), Has.Some.Contains("30 categories"));
    }

    [Test]
    public void Instructions_AskForTablesAndChartsWithoutHtml()
    {
        Assert.That(Instructions, Does.Contain("show_chart").And.Contain("pipe table").And.Contain("Never use HTML"));
    }
}
