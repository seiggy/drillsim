using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;
using Microsoft.Data.Sqlite;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed partial class SqliteScenarioStore
{
    private static async Task ValidateConfiguredPredictionAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid scenarioId,
        PredictionBody body,
        IReadOnlyList<BaselineSnapshot>? baselines,
        CancellationToken ct)
    {
        Scenario scenario = await FindAsync(connection, scenarioId, ct, transaction)
            ?? throw Conflict("Scenario not found", "A scenario is required for a configured prediction.");
        ScenarioPackageSnapshot snapshot = await FindScenarioPackageSnapshotAsync(connection, transaction, scenario, ct)
            ?? throw Conflict("Source snapshot unavailable", "Configured predictions require an immutable scenario source snapshot.");
        IReadOnlyList<EvidenceVisibility> visibility = await ReadEvidenceVisibilityAsync(connection, transaction, scenarioId, ct);
        AnalysisPackage package = ScenarioService.FilterPackage(snapshot.Package, scenario, scenario.InitialAsOfUtc,
            visibility, new CanonicalJsonHasher());
        var analyzer = new PetrophysicsAnalysisService(TimeProvider.System);
        AnalysisResult result = ConfiguredPredictionBinding.Validate(scenario, body, package, analyzer);
        if (baselines is null) return;
        AnalysisResult four = analyzer.Analyze(package, scenario.ReservoirName, result.Configuration with { IdwNeighborCount = 4 });
        IReadOnlyList<BaselineSnapshot> expected = BaselineFactory.Create(scenarioId, body, result, four);
        if (PredictionJson.Canonicalize(expected.OrderBy(item => item.Kind).ToArray()) !=
            PredictionJson.Canonicalize(baselines.OrderBy(item => item.Kind).ToArray()))
            throw Conflict("Configured baseline mismatch", "Baselines do not match the frozen package and configured four-baseline definitions.");
    }
}
