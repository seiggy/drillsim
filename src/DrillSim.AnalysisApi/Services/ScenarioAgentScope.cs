using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;
using Microsoft.Extensions.AI;

namespace DrillSim.AnalysisApi.Services;

internal sealed record ScenarioAgentScope(
    Scenario Scenario,
    Guid FieldId,
    string ReservoirName,
    DateTimeOffset AsOfUtc)
{
    internal const string AgentName = "DrillSimBlindScenarioAnalyst";
    internal const string HeaderName = "X-DrillSim-Scenario-Id";

    internal static ScenarioAgentScope Create(Scenario scenario)
    {
        bool useClone = scenario.Status is ScenarioStatus.Revealed or ScenarioStatus.Scored;
        if (useClone && scenario.ClonedFieldId is null)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Invalid scenario state",
                $"Scenario {scenario.ScenarioId:D} has status {scenario.Status} but no cloned field.");
        }

        return new ScenarioAgentScope(
            scenario,
            useClone ? scenario.ClonedFieldId!.Value : scenario.SourceFieldId,
            scenario.ReservoirName,
            scenario.AsOfUtc);
    }
}

internal static class ScenarioAgentScopeBinding
{
    internal static bool IsScenarioEndpoint(PathString path) =>
        path.Equals("/agui/scenario") || path.Equals("/agui/scenario/");

    internal static async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue(ScenarioAgentScope.HeaderName, out var values) ||
            values.Count != 1 ||
            !Guid.TryParseExact(values[0], "D", out Guid scenarioId) ||
            scenarioId == Guid.Empty)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid scenario scope",
                detail: $"A single valid {ScenarioAgentScope.HeaderName} header is required.")
                .ExecuteAsync(context);
            return;
        }

        ScenarioService scenarios = context.RequestServices.GetRequiredService<ScenarioService>();
        Scenario scenario = await scenarios.GetAsync(scenarioId, context.RequestAborted);
        context.Features.Set(ScenarioAgentScope.Create(scenario));
        await next(context);
    }

    internal static ScenarioAgentScope GetRequired(HttpContext? context) =>
        context?.Features.Get<ScenarioAgentScope>()
        ?? throw new InvalidOperationException("The scenario agent request scope was not bound.");
}

internal static class ScenarioAgentTools
{
    internal static AIFunction[] Create(
        ScenarioAgentScope scope,
        ScenarioService scenarios,
        IPetrophysicsAnalysisService analysis) =>
        Create(() => scope, scenarios, analysis);

    internal static AIFunction[] Create(
        IHttpContextAccessor accessor,
        ScenarioService scenarios,
        IPetrophysicsAnalysisService analysis) =>
        Create(
            () => ScenarioAgentScopeBinding.GetRequired(accessor.HttpContext),
            scenarios,
            analysis);

    private static AIFunction[] Create(
        Func<ScenarioAgentScope> getScope,
        ScenarioService scenarios,
        IPetrophysicsAnalysisService analysis)
    {
        AIFunction scenarioClockTool = AIFunctionFactory.Create(
            async (CancellationToken cancellationToken) =>
            {
                ScenarioAgentScope scope = getScope();
                EvidenceVisibilitySummary visibility =
                    await scenarios.GetCurrentVisibilitySummaryAsync(scope.Scenario, cancellationToken);
                return new ScenarioClock(scope.Scenario, visibility);
            },
            "get_scenario_clock",
            "Gets the bound scenario clock and its public evidence visibility summary without advancing time.");
        AIFunction asOfPackageTool = AIFunctionFactory.Create(
            (CancellationToken cancellationToken) =>
            {
                ScenarioAgentScope scope = getScope();
                return scenarios.GetCurrentPackageAsync(scope.Scenario, scope.FieldId, cancellationToken);
            },
            "get_asof_field_package",
            "Gets only evidence visible for the bound field at the bound scenario's current clock.");
        AIFunction asOfAnalysisTool = AIFunctionFactory.Create(
            async (CancellationToken cancellationToken) =>
            {
                ScenarioAgentScope scope = getScope();
                AnalysisPackage package =
                    await scenarios.GetCurrentPackageAsync(scope.Scenario, scope.FieldId, cancellationToken);
                return analysis.Analyze(package, scope.ReservoirName);
            },
            "analyze_asof_field",
            "Runs deterministic expected paydirt screening for the bound field and reservoir using only evidence visible at the bound scenario clock.");
        AIFunction scenarioScorecardTool = AIFunctionFactory.Create(
            (CancellationToken cancellationToken) =>
            {
                ScenarioAgentScope scope = getScope();
                return scenarios.GetCurrentScorecardAsync(scope.Scenario, cancellationToken);
            },
            "get_scenario_scorecard",
            "Gets the bound scenario's public aggregate scorecard after scoring; it contains no raw truth or internal execution data.");

        return [scenarioClockTool, asOfPackageTool, asOfAnalysisTool, scenarioScorecardTool];
    }
}
