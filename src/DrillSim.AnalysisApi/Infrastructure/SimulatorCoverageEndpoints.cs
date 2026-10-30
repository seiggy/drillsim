using DrillSim.AnalysisApi.Models;
using DrillSim.AnalysisApi.Services;

namespace DrillSim.AnalysisApi.Infrastructure;

public static class SimulatorCoverageEndpoints
{
    public static IEndpointRouteBuilder MapSimulatorCoverage(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/fields/{fieldId:guid}/simulator-coverage", async (
            Guid fieldId,
            string? reservoir,
            Guid? scenarioId,
            DateTimeOffset? asOf,
            ScenarioService scenarios,
            CancellationToken cancellationToken) =>
        {
            AnalysisPackage package = await scenarios.GetPackageAsync(fieldId, scenarioId, asOf, cancellationToken);
            string? selectedReservoir = await scenarios.ResolveReservoirAsync(scenarioId, reservoir, cancellationToken);
            if (string.IsNullOrWhiteSpace(selectedReservoir))
                throw new ScenarioApiException(StatusCodes.Status400BadRequest, "Reservoir required",
                    "Supply the reservoir query parameter for a live field scope.");
            return Results.Ok(PetrophysicsAnalysisService.EstimateSimulatorCoverage(package, selectedReservoir.Trim())
                ?? throw new ScenarioApiException(StatusCodes.Status404NotFound, "No simulator coverage",
                    $"No located visible control carries formation '{selectedReservoir.Trim()}', so the simulator model extent is unknown."));
        });
        return endpoints;
    }
}
