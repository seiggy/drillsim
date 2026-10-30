using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class FormationInterpretationService(
    ScenarioService scenarios,
    SqliteScenarioStore scenarioStore,
    HypothesisService hypotheses,
    IPetrophysicsAnalysisService analysis,
    FormationInterpretationAgent agent,
    TimeProvider time)
{
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(10);
    public FormationInterpretationStatus Status => agent.Status;

    public async Task<FormationInterpretationResponse> DraftAsync(
        FormationInterpretationRequest request, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);
        try
        {
            return await DraftCoreAsync(request, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ScenarioApiException(504, "Formation interpretation timed out",
                "The bounded drafting request timed out. No draft was saved and no automatic retry was attempted.");
        }
    }

    private async Task<FormationInterpretationResponse> DraftCoreAsync(FormationInterpretationRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        HypothesisScope scope = Validate(request);
        if (!Status.Configured)
            throw new ScenarioApiException(503, "Formation interpretation AI unavailable", Status.Reason!);

        AnalysisPackage package;
        AnalysisResult result;
        if (request.SavedHypothesis is { } reference)
        {
            HypothesisRevision saved = await hypotheses.GetAsync(scope, reference, ct);
            if (saved.SnapshotSha256 != request.SnapshotSha256 || saved.Input.Configuration != request.Configuration ||
                HypothesisValidation.ScopeKey(saved.Scope) != HypothesisValidation.ScopeKey(scope))
                throw Conflict("The saved revision, snapshot or applied configuration differs from the reviewed request.");
            scope = saved.Scope;
            package = saved.Package;
            result = saved.Analysis;
        }
        else
            (scope, package, result) = await LoadVerifiedAsync(scenarios, scenarioStore, analysis, scope,
                request.Configuration, request.PackageSha256, request.AnalysisSha256, Conflict, ct);
        EnsureExactEvidence(scope, package, result, request.Configuration, request.PackageSha256, request.AnalysisSha256, Conflict);
        CandidateGridPoint? selected = request.SelectedCandidateId is null ? null :
            result.CandidateGrid.SingleOrDefault(point => point.CandidateId == request.SelectedCandidateId &&
                point.Status == "eligible" && point.Prediction is not null)
            ?? throw Conflict("The selected target is not an eligible candidate in this exact analysis.");
        FormationInterpretationEvidence evidence = FormationInterpretationEvidence.Build(request with { Scope = scope }, package, result, selected, ct);
        FormationInterpretationDraft draft = await agent.GenerateAsync(evidence, ct);
        return new("formation-interpretation-v1", FormationInterpretationAgent.PromptVersion, scope, package.Sha256,
            result.AnalysisSha256, result.ConfigurationSha256, request.SelectedCandidateId, request.SavedHypothesis,
            request.SnapshotSha256, time.GetUtcNow().ToUniversalTime(), draft);
    }

    /// <summary>
    /// Loads the live or scenario-visible package for an explicit scope, re-runs the exact analysis and rejects any
    /// fingerprint drift. Scenario scopes must match the scenario and, for the source field, an immutable snapshot.
    /// </summary>
    internal static async Task<(HypothesisScope Scope, AnalysisPackage Package, AnalysisResult Result)> LoadVerifiedAsync(
        ScenarioService scenarios, SqliteScenarioStore scenarioStore, IPetrophysicsAnalysisService analysis,
        HypothesisScope scope, AnalysisConfiguration configuration, string packageSha256, string analysisSha256,
        Func<string, ScenarioApiException> conflict, CancellationToken ct)
    {
        if (scope.ScenarioId is Guid scenarioId)
        {
            Scenario scenario = await scenarios.GetAsync(scenarioId, ct);
            if (!string.Equals(scope.ReservoirName, scenario.ReservoirName, StringComparison.OrdinalIgnoreCase) ||
                (scope.FieldId != scenario.SourceFieldId && scope.FieldId != scenario.ClonedFieldId) ||
                scope.AsOfUtc < scenario.InitialAsOfUtc || scope.AsOfUtc > scenario.AsOfUtc)
                throw conflict("The requested field, reservoir or as-of time is outside the selected scenario.");
            // The general package reader can backfill legacy source snapshots. Drafting must never do that.
            if (scope.FieldId == scenario.SourceFieldId &&
                await scenarioStore.FindScenarioPackageSnapshotAsync(scenario, ct) is null)
                throw conflict("This scenario has no immutable source snapshot. AI drafting cannot migrate or backfill it.");
            scope = scope with { ReservoirName = scenario.ReservoirName };
        }
        AnalysisPackage package = await scenarios.GetPackageAsync(scope.FieldId, scope.ScenarioId, scope.AsOfUtc, ct);
        if (package.FieldId != scope.FieldId || package.Sha256 != packageSha256)
            throw conflict("The visible package changed. Refresh and review the evidence before drafting.");
        AnalysisResult result = analysis.Analyze(package, scope.ReservoirName, configuration);
        EnsureExactEvidence(scope, package, result, configuration, packageSha256, analysisSha256, conflict);
        return (scope, package, result);
    }

    private static void EnsureExactEvidence(HypothesisScope scope, AnalysisPackage package, AnalysisResult result,
        AnalysisConfiguration configuration, string packageSha256, string analysisSha256, Func<string, ScenarioApiException> conflict)
    {
        if (package.FieldId != scope.FieldId || package.Sha256 != packageSha256 ||
            result.FieldId != scope.FieldId || result.PackageSha256 != package.Sha256 ||
            result.AnalysisSha256 != analysisSha256 || result.Configuration != configuration ||
            result.ConfigurationSha256 != configuration.ComputeSha256() ||
            !string.Equals(result.ReservoirName, scope.ReservoirName, StringComparison.OrdinalIgnoreCase))
            throw conflict("The package, analysis or applied configuration fingerprint does not match the exact reviewed evidence.");
    }

    private static HypothesisScope Validate(FormationInterpretationRequest? request)
    {
        if (request is null) throw HypothesisValidation.Invalid("A formation interpretation request is required.");
        HypothesisScope scope = HypothesisValidation.Scope(request.Scope);
        AnalysisConfiguration.Validate(request.Configuration);
        HypothesisValidation.Hash(request.PackageSha256);
        HypothesisValidation.Hash(request.AnalysisSha256);
        if (request.SelectedCandidateId is not null) HypothesisValidation.Text(request.SelectedCandidateId, "selectedCandidateId", 200);
        if (request.SavedHypothesis is not null)
        {
            HypothesisValidation.Reference(request.SavedHypothesis);
            HypothesisValidation.Hash(request.SnapshotSha256);
        }
        else if (request.SnapshotSha256 is not null)
            throw HypothesisValidation.Invalid("snapshotSha256 requires an exact savedHypothesis reference.");
        if (request.Notes is null || request.Notes.ControlNotes is null || request.Notes.ControlNotes.Count > 32)
            throw HypothesisValidation.Invalid("notes and at most 32 controlNotes are required.");
        OptionalText(request.Notes.Name, "name", 120);
        OptionalText(request.Notes.Rationale, "rationale", 10_000);
        OptionalText(request.Notes.CorrelationNotes, "correlationNotes", 4_000);
        foreach (HypothesisControlNote? note in request.Notes.ControlNotes)
        {
            if (note is null) throw HypothesisValidation.Invalid("Control notes cannot contain null.");
            HypothesisValidation.Text(note.EvidenceId, "evidenceId", 200);
            HypothesisValidation.Text(note.Note, "control note", 2000);
        }
        if (request.Notes.ControlNotes.Select(note => note.EvidenceId).Distinct(StringComparer.Ordinal).Count() != request.Notes.ControlNotes.Count)
            throw HypothesisValidation.Invalid("Control-note evidence IDs must be unique.");
        return scope;
    }

    private static void OptionalText(string? text, string field, int maximum)
    {
        if (text is null) throw HypothesisValidation.Invalid($"{field} must be a string (empty is allowed).");
        if (text.Length > 0) HypothesisValidation.Text(text, field, maximum);
    }

    private static ScenarioApiException Conflict(string detail) => new(409, "Formation interpretation evidence changed", detail);
}
