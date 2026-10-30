using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class ProspectBriefService(
    ScenarioService scenarios,
    SqliteScenarioStore scenarioStore,
    IPetrophysicsAnalysisService analysis,
    FormationInterpretationAgent agent,
    TimeProvider time)
{
    public FormationInterpretationStatus Status => agent.Status;

    public async Task<ProspectBriefResponse> ExplainAsync(ProspectBriefRequest request, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(FormationInterpretationService.RequestTimeout);
        try
        {
            return await ExplainCoreAsync(request, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ScenarioApiException(504, "Prospect brief timed out",
                "The bounded brief request timed out. Nothing was saved and no automatic retry was attempted.");
        }
    }

    private async Task<ProspectBriefResponse> ExplainCoreAsync(ProspectBriefRequest request, CancellationToken ct)
    {
        if (request is null) throw HypothesisValidation.Invalid("A prospect brief request is required.");
        HypothesisScope scope = HypothesisValidation.Scope(
            new(request.FieldId, request.ReservoirName, request.ScenarioId, request.AsOfUtc));
        HypothesisValidation.Text(request.CandidateId, "candidateId", 200);
        AnalysisConfiguration.Validate(request.Configuration);
        HypothesisValidation.Hash(request.PackageSha256);
        HypothesisValidation.Hash(request.AnalysisSha256);
        if (!Status.Configured)
            throw new ScenarioApiException(503, "Prospect brief AI unavailable", Status.Reason!);

        (scope, AnalysisPackage package, AnalysisResult result) = await FormationInterpretationService.LoadVerifiedAsync(
            scenarios, scenarioStore, analysis, scope, request.Configuration, request.PackageSha256, request.AnalysisSha256, Conflict, ct);
        CandidateGridPoint selected = result.CandidateGrid.SingleOrDefault(point => point.CandidateId == request.CandidateId &&
                point.Status == "eligible" && point.Prediction is not null)
            ?? throw Conflict("The selected target is not an eligible candidate in this exact analysis.");
        // Grid predictions are unranked; the ranking carries the target's position when it is a top target.
        RankedCandidate candidate = result.Ranking.FirstOrDefault(r => r.CandidateId == selected.CandidateId) ?? selected.Prediction!;

        FormationStructureEstimate estimate = PetrophysicsAnalysisService.EstimateFormationStructure(
            package, scope.ReservoirName, candidate.EastingM, candidate.NorthingM, request.Configuration.IdwNeighborCount);
        SimulatorCoverage? coverage = PetrophysicsAnalysisService.EstimateSimulatorCoverage(package, scope.ReservoirName);
        FormationInterpretationEvidence evidence = FormationInterpretationEvidence.Build(
            new FormationInterpretationRequest(scope, request.Configuration, package.Sha256, result.AnalysisSha256,
                candidate.CandidateId, null, null, new("", "", "", [])), package, result, selected, ct);
        ProspectBrief brief = await ProspectBriefAgent.GenerateAsync(agent, evidence, candidate, estimate, coverage, ct);
        return new(ProspectBriefAgent.Version, ProspectBriefAgent.PromptVersion, scope.FieldId, scope.ReservoirName,
            scope.ScenarioId, scope.AsOfUtc, candidate.CandidateId, package.Sha256, result.AnalysisSha256,
            result.ConfigurationSha256, time.GetUtcNow().ToUniversalTime(), estimate, brief);
    }

    private static ScenarioApiException Conflict(string detail) => new(409, "Prospect brief evidence changed", detail);
}
