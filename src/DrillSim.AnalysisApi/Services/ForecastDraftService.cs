using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class ForecastDraftService(
    ScenarioService scenarios,
    SqliteScenarioStore scenarioStore,
    IPetrophysicsAnalysisService analysis,
    FormationInterpretationAgent agent,
    TimeProvider time)
{
    private static readonly ScenarioStatus[] DraftableStatuses =
        [ScenarioStatus.Draft, ScenarioStatus.Armed, ScenarioStatus.PredictionDrafted];
    public FormationInterpretationStatus Status => agent.Status;

    public async Task<ForecastDraftResponse> DraftAsync(ForecastDraftRequest request, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(FormationInterpretationService.RequestTimeout);
        try
        {
            return await DraftCoreAsync(request, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ScenarioApiException(504, "Forecast draft timed out",
                "The bounded drafting request timed out. Nothing was saved and no automatic retry was attempted.");
        }
    }

    private async Task<ForecastDraftResponse> DraftCoreAsync(ForecastDraftRequest request, CancellationToken ct)
    {
        if (request is null || request.ScenarioId == Guid.Empty)
            throw HypothesisValidation.Invalid("A forecast draft request with a scenarioId is required.");
        HypothesisValidation.Text(request.CandidateId, "candidateId", 200);
        AnalysisConfiguration.Validate(request.Configuration);
        HypothesisValidation.Hash(request.PackageSha256);
        HypothesisValidation.Hash(request.AnalysisSha256);
        if (!Status.Configured)
            throw new ScenarioApiException(503, "Forecast draft AI unavailable", Status.Reason!);

        Scenario scenario = await scenarios.GetAsync(request.ScenarioId, ct);
        if (!DraftableStatuses.Contains(scenario.Status))
            throw Conflict("AI forecast drafting is only available before the prediction is sealed.");
        // The general package reader can backfill legacy source snapshots. Drafting must never do that.
        if (await scenarioStore.FindScenarioPackageSnapshotAsync(scenario, ct) is null)
            throw Conflict("This scenario has no immutable source snapshot. AI drafting cannot migrate or backfill it.");
        var scope = new HypothesisScope(scenario.SourceFieldId, scenario.ReservoirName, scenario.ScenarioId, scenario.InitialAsOfUtc);
        AnalysisPackage package = await scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc, ct);
        if (package.FieldId != scope.FieldId || package.Sha256 != request.PackageSha256)
            throw Conflict("The frozen source package differs from the reviewed evidence. Reanalyze before drafting.");
        AnalysisResult result = analysis.Analyze(package, scope.ReservoirName, request.Configuration);
        if (result.AnalysisSha256 != request.AnalysisSha256 || result.ConfigurationSha256 != request.Configuration.ComputeSha256())
            throw Conflict("The analysis or applied configuration fingerprint does not match the reviewed result.");
        CandidateGridPoint selected = result.CandidateGrid.SingleOrDefault(point => point.CandidateId == request.CandidateId &&
                point.Status == "eligible" && point.Prediction is not null)
            ?? throw Conflict("The selected target is not an eligible candidate in this exact analysis.");
        RankedCandidate candidate = selected.Prediction!;
        SimulatorCoverage coverage = PetrophysicsAnalysisService.EstimateSimulatorCoverage(package, scope.ReservoirName)
            ?? throw Conflict("No located visible formation control defines the simulator model extent for this reservoir.");
        if (!coverage.Contains(candidate.EastingM, candidate.NorthingM))
            throw Conflict("The selected target is outside the blind simulator's model coverage and cannot be drilled. Choose a target inside the coverage bounds.");

        FormationStructureEstimate estimate = PetrophysicsAnalysisService.EstimateFormationStructure(
            package, scope.ReservoirName, candidate.EastingM, candidate.NorthingM, request.Configuration.IdwNeighborCount);
        FormationInterpretationEvidence evidence = FormationInterpretationEvidence.Build(
            new FormationInterpretationRequest(scope, request.Configuration, package.Sha256, result.AnalysisSha256,
                candidate.CandidateId, null, null, new("", "", "", [])), package, result, selected, ct);
        StaticReservoirModel? model = PetrophysicsAnalysisService.BuildStaticModel(package, scope.ReservoirName,
            request.Configuration, [new(candidate.CandidateId, candidate.EastingM, candidate.NorthingM)]);
        ForecastDraft draft = await ForecastDraftAgent.GenerateAsync(agent, evidence, candidate, estimate, coverage, model, ct, request.Configuration.TargetFluid);
        return new(ForecastDraftAgent.Version, ForecastDraftAgent.PromptVersion, scenario.ScenarioId, candidate.CandidateId,
            package.Sha256, result.AnalysisSha256, result.ConfigurationSha256, time.GetUtcNow().ToUniversalTime(), estimate, draft);
    }

    private static ScenarioApiException Conflict(string detail) => new(409, "Forecast draft evidence changed", detail);
}
