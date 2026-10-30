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
        var (package, reservoir) = await ResolveAsync(fieldId, request, cancellationToken);
        return analysis.Analyze(package, reservoir, request.Configuration);
    }

    public async Task<StaticReservoirModel> BuildReservoirModelAsync(
        Guid fieldId,
        StaticReservoirModelRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Targets is { Count: > 50 } || request.Targets?.Any(t => t is null || string.IsNullOrWhiteSpace(t.Id) ||
                t.Id.Length > 200 || !double.IsFinite(t.EastingM) || !double.IsFinite(t.NorthingM)) == true)
            throw Invalid("targets must be at most 50 points with a nonempty id and finite eastingM/northingM.");
        var (package, reservoir) = await ResolveAsync(fieldId,
            new(request.Configuration, request.ScenarioId, request.AsOf, request.Reservoir), cancellationToken);
        if (string.IsNullOrWhiteSpace(reservoir))
            throw Invalid("reservoir is required for a live field scope.");
        return PetrophysicsAnalysisService.BuildStaticModel(package, reservoir, request.Configuration, request.Targets)
            ?? throw new ScenarioApiException(StatusCodes.Status404NotFound, "No reservoir model",
                $"No located visible pick carries formation '{reservoir}', so no reservoir model can be built.");
    }

    private async Task<(AnalysisPackage Package, string? Reservoir)> ResolveAsync(
        Guid fieldId,
        ConfiguredAnalysisRequest request,
        CancellationToken cancellationToken)
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
        return (package, reservoir);
    }

    private static ScenarioApiException Invalid(string detail) =>
        new(StatusCodes.Status400BadRequest, "Invalid analysis request", detail);
}
