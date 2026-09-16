using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class ConfiguredAnalysisService(ScenarioService scenarios, IPetrophysicsAnalysisService analysis)
{
    public async Task<AnalysisResult> AnalyzeAsync(
        Guid fieldId,
        ConfiguredAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        AnalysisConfiguration.Validate(request.Configuration);
        if (fieldId == Guid.Empty || request.ScenarioId == Guid.Empty)
            throw Invalid("fieldId and scenarioId must be nonempty UUIDs.");
        if (request.Reservoir is { Length: > 200 } || request.Reservoir is not null && string.IsNullOrWhiteSpace(request.Reservoir))
            throw Invalid("reservoir must be a nonempty name of at most 200 characters when supplied.");
        if (request.AsOf is not null && request.ScenarioId is null)
            throw Invalid("asOf requires scenarioId; an unrestricted live package cannot honor a scenario clock.");

        string? reservoir = request.Reservoir?.Trim();
        if (request.ScenarioId is Guid scenarioId)
        {
            Scenario scenario = await scenarios.GetAsync(scenarioId, cancellationToken);
            if (reservoir is not null && !string.Equals(reservoir, scenario.ReservoirName, StringComparison.OrdinalIgnoreCase))
                throw new ScenarioApiException(StatusCodes.Status409Conflict, "Scenario reservoir mismatch",
                    $"Scenario {scenarioId:D} is bound to reservoir '{scenario.ReservoirName}'.");
            reservoir = scenario.ReservoirName;
        }
        AnalysisPackage package = await scenarios.GetPackageAsync(fieldId, request.ScenarioId, request.AsOf, cancellationToken);
        if (package.FieldId != fieldId)
            throw new ScenarioApiException(StatusCodes.Status409Conflict, "Field package mismatch",
                "The package does not belong to the requested field.");
        return analysis.Analyze(package, reservoir, request.Configuration);
    }

    private static ScenarioApiException Invalid(string detail) =>
        new(StatusCodes.Status400BadRequest, "Invalid analysis request", detail);
}
