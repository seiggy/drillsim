using System.Security.Cryptography;
using System.Text;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public interface IOperatorLedger
{
    Task<Scenario> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken);
    Task<PredictionRecord?> GetPredictionAsync(Guid scenarioId, CancellationToken cancellationToken);
    Task ApproveAsync(Guid scenarioId, string actor, CancellationToken cancellationToken);
}

internal sealed class OperatorLedger(ScenarioService scenarios, PredictionLedgerService predictions) : IOperatorLedger
{
    public Task<Scenario> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken) =>
        scenarios.GetAsync(scenarioId, cancellationToken);

    public async Task<PredictionRecord?> GetPredictionAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        try { return await predictions.GetAsync(scenarioId, cancellationToken); }
        catch (ScenarioApiException exception) when (exception.StatusCode == 404) { return null; }
    }

    public async Task ApproveAsync(Guid scenarioId, string actor, CancellationToken cancellationToken)
    {
        try { _ = await predictions.ApproveAsync(scenarioId, actor, cancellationToken); }
        catch (ScenarioApiException exception) when (exception.StatusCode is 400 or 404 or 409)
        { throw new OperatorWorkflowException(exception.StatusCode, "Prediction approval prerequisites or immutable actor do not match."); }
    }
}

public sealed partial class OperatorWorkflowService(IOperatorLedger ledger, OperatorBackendClient backend)
{
    private const string CompletionUnavailable =
        "No verified completion review is available yet. Wait for S6, or update the simulator if it is already awaiting completion approval.";
    private const string BindingRequired =
        "Choose a model profile and prepare the simulator in Simulation.";
    private static readonly OperatorCompletion UnavailableCompletion = new(false, false, CompletionUnavailable, []);

    public async Task<OperatorScenarioView> GetAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        Scenario scenario = await GetScenarioAsync(scenarioId, cancellationToken);
        PredictionRecord? prediction = await ledger.GetPredictionAsync(scenarioId, cancellationToken);
        ValidatePredictionScenario(scenarioId, prediction);
        bool sealedPrediction = prediction?.Seal is not null;
        bool approved = IsApproved(prediction);
        OperatorBackendClient.Binding? binding = await backend.GetBindingAsync(scenarioId, cancellationToken);
        OperatorBackendClient.Run? current = await backend.GetCurrentRunAsync(scenarioId, cancellationToken);
        IReadOnlyList<OperatorStage> stages = current is null ? [] :
            await backend.GetStagesAsync(current.RunId, cancellationToken);
        bool bound = BindingMatches(scenario, prediction, binding);
        string? preflightReason = scenario.Status is ScenarioStatus.Scored or ScenarioStatus.Revealed ||
            current?.Status is "Scored" or "Revealed"
            ? "This simulation has finished. Review its published evidence and evaluation, or create a new scenario."
            : current is not null ? "A simulation run already exists. Follow its progress and available actions."
            : !sealedPrediction ? "Save, review, and seal a prediction first."
            : !approved ? "Explicitly approve the reviewed prediction seal first."
            : !bound ? BindingRequired
            : scenario.Status != ScenarioStatus.HumanApproved ? "Approve the sealed prediction before starting a simulation."
            : null;
        bool cancellable = current is not null && CanCancel(current);
        bool resumable = current is not null && CanResume(current);
        bool publishable = current is not null && CanPublish(current);
        bool scoreable = current is not null && CanScore(current);
        OperatorCompletion completion = await GetCompletionAsync(
            scenario, current, approved && bound,
            stages.Any(x => x.Stage == "S6DesignCompletion" && x.Status == "AwaitingApproval"), cancellationToken);
        OperatorPublicationRecoveryReview? recovery = current is { Status: "Failed", CurrentStage: "S8PublishReveal" }
            ? await backend.GetPublicationRecoveryAsync(scenarioId, current.RunId, cancellationToken) : null;
        OperatorScoringCorrectionReview? correction = current is { Status: "Failed", CurrentStage: "S9Score" }
            ? await backend.GetScoringCorrectionAsync(scenarioId, current.RunId, cancellationToken) : null;
        return new(true, null, scenarioId, scenario.Status.ToString(),
            new(sealedPrediction, approved, prediction?.Seal?.Sha256),
            new(preflightReason is null, preflightReason, bound),
            current is null ? null : new(current.RunId, current.Status, current.CurrentStage, stages,
                stages.Count(x => x.Status == "Completed") * 10, FailureReason(current)),
            completion,
            new(
                Available(sealedPrediction && !approved && scenario.Status == ScenarioStatus.PredictionSealed,
                    approved ? "The immutable prediction is already approved." : "A sealed prediction is required."),
                Available(preflightReason is null, preflightReason),
                Available(cancellable, "Only a pre-publication nonterminal run can be cancelled."),
                Available(resumable, "Resume is permitted only while awaiting an S1-S7 dependency."),
                Available(completion.ApprovalEnabled, completion.Reason),
                Available(publishable, scenario.Status is ScenarioStatus.Revealed or ScenarioStatus.Scored
                    ? "The new evidence has already been published." : "Finish completion review and production before publishing new evidence."),
                Available(scoreable, scenario.Status == ScenarioStatus.Scored
                    ? "Evaluation is complete. Review the results below." : "Publish the new evidence before evaluating the prediction."))
            {
                RecoverPublication = Available(recovery?.RecoveryEnabled == true, recovery?.Reason ?? "Only a verified Failed/S8 publication write rejection can be recovered."),
                CorrectScore = Available(correction?.CorrectionEnabled == true, correction?.Reason ?? "Only the verified rejected S9 bounds failure can be corrected.")
            })
        {
            PublicationRecovery = recovery,
            ScoreCorrection = correction
        };
    }

    public async Task<OperatorPublicationRecoveryReview> GetPublicationRecoveryAsync(
        Guid scenarioId, Guid runId, CancellationToken cancellationToken)
    {
        _ = await GetScenarioAsync(scenarioId, cancellationToken);
        _ = await backend.GetOwnedRunAsync(scenarioId, runId, cancellationToken);
        return await backend.GetPublicationRecoveryAsync(scenarioId, runId, cancellationToken);
    }

    public async Task<OperatorPublicationRecoveryResult> RecoverPublicationAsync(
        Guid scenarioId, Guid runId, OperatorPublicationRecoveryRequest request, string key, CancellationToken cancellationToken)
    {
        if (!RecoveryLabel(request.Actor, 100) || !RecoveryLabel(request.Reason, 500))
            throw new OperatorWorkflowException(400, "Recovery requires an actor (1–100) and reason (1–500), using visible ASCII without surrounding whitespace.");
        ValidateHash(request.ReviewedPublicationHash);
        _ = await GetScenarioAsync(scenarioId, cancellationToken);
        _ = await backend.GetOwnedRunAsync(scenarioId, runId, cancellationToken);
        // The backend must arbitrate replay before current status/hash guards. Never auto-publish here.
        return await backend.RecoverPublicationAsync(scenarioId, runId, request, key, cancellationToken);
    }

    private static bool RecoveryLabel(string? value, int max) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= max && value == value.Trim() && value.All(c => c is >= ' ' and <= '~');

    public async Task<OperatorActionResult> ApprovePredictionAsync(
        Guid scenarioId, string? actor, string? reviewedSealHash, CancellationToken cancellationToken)
    {
        string label = ValidateActor(actor);
        ValidateHash(reviewedSealHash);
        Scenario scenario = await GetScenarioAsync(scenarioId, cancellationToken);
        PredictionRecord? prediction = await ledger.GetPredictionAsync(scenarioId, cancellationToken);
        ValidatePredictionScenario(scenarioId, prediction);
        if (prediction?.Seal is null)
            throw new OperatorWorkflowException(409, "Seal a reviewed prediction before approving it.");
        if (prediction.Seal.Sha256 != reviewedSealHash)
            throw new OperatorWorkflowException(409, "The reviewed prediction seal does not match. Reload and review before approving.");
        if (scenario.Status is not (ScenarioStatus.PredictionSealed or ScenarioStatus.HumanApproved))
            throw new OperatorWorkflowException(409, "Scenario is not awaiting prediction approval.");
        await ledger.ApproveAsync(scenarioId, label, cancellationToken);
        return new(scenarioId, null, "approved", null);
    }

    public async Task<OperatorActionResult> StartAsync(
        Guid scenarioId, string? actor, string key, CancellationToken cancellationToken)
    {
        _ = ValidateActor(actor);
        Scenario scenario = await GetScenarioAsync(scenarioId, cancellationToken);
        PredictionRecord? prediction = await ledger.GetPredictionAsync(scenarioId, cancellationToken);
        ValidatePredictionScenario(scenarioId, prediction);
        if (!IsApproved(prediction) || scenario.Status != ScenarioStatus.HumanApproved)
            throw new OperatorWorkflowException(409, "Execution requires a sealed, explicitly approved prediction in HumanApproved state.");
        OperatorBackendClient.Binding? binding = await backend.GetBindingAsync(scenarioId, cancellationToken);
        if (!BindingMatches(scenario, prediction, binding))
            throw new OperatorWorkflowException(409, BindingRequired);

        // The existing S1 executor materializes the actual plan from the immutable ledger.
        // This request artifact identifies that sealed input, not a client-supplied path or hidden world.
        string planHash = Hash($"local-operator-approved-plan-v1\n{scenarioId:D}\n{prediction!.Seal!.Sha256}\n{prediction.Body.FieldPackageSha256}");
        OperatorBackendClient.Run run = await backend.StartAsync(
            scenarioId, prediction.Seal.Sha256, $"approved-plan:{planHash}", planHash, key, cancellationToken);
        return new(scenarioId, run.RunId, "started", "Run accepted. Refresh for authoritative checkpoint progress.");
    }

    public async Task<OperatorActionResult> ActAsync(
        Guid scenarioId, Guid runId, string action, string? actor, string key, CancellationToken cancellationToken)
    {
        _ = ValidateActor(actor);
        _ = await GetScenarioAsync(scenarioId, cancellationToken);
        OperatorBackendClient.Run run = await backend.GetOwnedRunAsync(scenarioId, runId, cancellationToken);
        if (action == "publish")
            return await PublishAsync(scenarioId, run, key, cancellationToken);
        if (action == "score")
        {
            if (!CanScore(run) && run.Status != "Scored")
                throw new OperatorWorkflowException(409, "Scoring requires a published reveal or retryable scoring checkpoint.");
            return await ScoreAsync(scenarioId, runId, key, cancellationToken);
        }
        if (action == "cancel" && !CanCancel(run) && run.Status != "Cancelled")
            throw new OperatorWorkflowException(409, "This run cannot be cancelled through the local operator workflow.");
        if (action == "resume" && !CanResume(run) && run.Status is not ("Queued" or "Running"))
            throw new OperatorWorkflowException(409, "Resume requires a retryable S1-S7 dependency checkpoint.");
        if (action is not ("cancel" or "resume"))
            throw new OperatorWorkflowException(400, "Unknown operator action.");
        // Existing endpoints arbitrate concurrency and exact idempotency replay.
        await backend.PostRunActionAsync(runId, action, key, cancellationToken);
        return new(scenarioId, runId, action == "cancel" ? "cancelled" : "resumed",
            "Action accepted. Refresh for authoritative checkpoint progress.");
    }

    public async Task<OperatorActionResult> ApproveCompletionAsync(
        Guid scenarioId, Guid runId, string? actor, string? reviewedOpeningHash, string key,
        CancellationToken cancellationToken)
    {
        string label = ValidateActor(actor);
        if (label.Length > 100 || label.Any(c => c > 126))
            throw new OperatorWorkflowException(400, "Completion actor must contain at most 100 visible ASCII characters.");
        ValidateHash(reviewedOpeningHash);
        Scenario scenario = await GetScenarioAsync(scenarioId, cancellationToken);
        OperatorBackendClient.Run run = await backend.GetOwnedRunAsync(scenarioId, runId, cancellationToken);
        PredictionRecord? prediction = await ledger.GetPredictionAsync(scenarioId, cancellationToken);
        ValidatePredictionScenario(scenarioId, prediction);
        OperatorBackendClient.Binding? binding = await backend.GetBindingAsync(scenarioId, cancellationToken);
        OperatorCompletion completion = await GetCompletionAsync(
            scenario, run, IsApproved(prediction) && BindingMatches(scenario, prediction, binding),
            true, cancellationToken);
        if (!completion.Available)
            throw new OperatorWorkflowException(409, CompletionUnavailable);
        if (completion.OpeningsHash != reviewedOpeningHash)
            throw new OperatorWorkflowException(409, "The reviewed completion openings changed. Reload and review before approving.");
        if (!completion.ApprovalEnabled && completion.Status != "Approved")
            throw new OperatorWorkflowException(409, "Completion approval requires an owned S6 approval pause and matching approved prediction.");
        await backend.ApproveCompletionAsync(runId, label, reviewedOpeningHash!, key, cancellationToken);
        return new(scenarioId, runId, "completion-approved", "Reviewed completion approved. Refresh for production checkpoint progress.");
    }

    private async Task<OperatorCompletion> GetCompletionAsync(
        Scenario scenario, OperatorBackendClient.Run? run, bool approvedAndBound,
        bool awaitingStage, CancellationToken cancellationToken)
    {
        if (run is null) return UnavailableCompletion;
        OperatorBackendClient.Completion? review = await backend.GetCompletionAsync(scenario.ScenarioId, run.RunId, cancellationToken);
        if (review is null) return UnavailableCompletion;
        if (review.Openings.Any(x => !string.Equals(x.ReservoirName, scenario.ReservoirName, StringComparison.Ordinal)))
            throw new OperatorWorkflowException(409, "Completion review reservoir does not match the requested scenario.");
        bool canApprove = review.Status == "Draft" && approvedAndBound && awaitingStage &&
            scenario.Status == ScenarioStatus.HumanApproved && run.Status == "AwaitingApproval" &&
            run.CurrentStage == "S6DesignCompletion";
        return new(true, canApprove,
            review.Status == "Approved" ? "This immutable completion design is already approved."
                : canApprove ? "Review every observable opening and explicitly approve this openings hash."
                : "Completion approval requires the S6 approval pause and matching approved prediction.",
            review.Openings, review.OpeningsHash, review.Status, review.RunId, review.ScenarioId);
    }

    private async Task<OperatorActionResult> PublishAsync(
        Guid scenarioId, OperatorBackendClient.Run run, string key, CancellationToken cancellationToken)
    {
        if (!CanPublish(run) && run.Status is not ("Revealed" or "Scored") &&
            !(run.CurrentStage == "S9Score" && run.Status is "Running" or "AwaitingDependency"))
            throw new OperatorWorkflowException(409, "Run is not ready to publish new evidence.");
        if (CanPublish(run))
        {
            try
            {
                await backend.PostRunActionAsync(run.RunId, "publish", key, cancellationToken);
            }
            catch (Exception exception) when (exception is OperatorWorkflowException or HttpRequestException or OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested) throw;
                bool rejected = exception is OperatorWorkflowException { StatusCode: 409 };
                return new(scenarioId, run.RunId, rejected ? "publication-failed" : "publication-pending",
                    "Publication was not confirmed. Refresh and retry the same attempt key after an uncertain response; use a new key for a new explicit attempt. Existing publication changes are not rolled back.",
                    false, "reveal-required");
            }
        }
        return await ScoreAsync(scenarioId, run.RunId, $"operator-score:{Hash(key)}", cancellationToken);
    }

    private async Task<OperatorActionResult> ScoreAsync(
        Guid scenarioId, Guid runId, string key, CancellationToken cancellationToken)
    {
        try
        {
            await backend.PostRunActionAsync(runId, "score", key, cancellationToken);
            return new(scenarioId, runId, "scored", "Reveal published and evaluation completed.", true, "scored");
        }
        catch (Exception exception) when (exception is OperatorWorkflowException or HttpRequestException or OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested) throw;
            bool rejected = exception is OperatorWorkflowException { StatusCode: 409 };
            return new(scenarioId, runId, rejected ? "scoring-failed" : "scoring-pending",
                "Reveal succeeded and remains published. Scoring was not confirmed; inspect progress and explicitly retry evaluation.",
                true, rejected ? "failed" : "pending");
        }
    }

    public static OperatorScenarioView Disabled(Guid scenarioId, string reason)
    {
        OperatorActionAvailability unavailable = Available(false, reason);
        return new(false, reason, scenarioId, null, new(false, false, null), new(false, reason, false),
            null, UnavailableCompletion, new(unavailable, unavailable, unavailable, unavailable, unavailable, unavailable, unavailable));
    }

    private async Task<Scenario> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        if (scenarioId == Guid.Empty) throw new OperatorWorkflowException(400, "A nonempty scenario identifier is required.");
        try
        {
            Scenario scenario = await ledger.GetScenarioAsync(scenarioId, cancellationToken);
            if (scenario.ScenarioId != scenarioId)
                throw new OperatorWorkflowException(404, "Scenario not found.");
            return scenario;
        }
        catch (ScenarioApiException exception) when (exception.StatusCode == 404)
        { throw new OperatorWorkflowException(404, "Scenario not found."); }
    }

    private static void ValidatePredictionScenario(Guid scenarioId, PredictionRecord? prediction)
    {
        if (prediction is not null && prediction.ScenarioId != scenarioId)
            throw new OperatorWorkflowException(409, "Prediction does not belong to this scenario.");
    }

    private static bool IsApproved(PredictionRecord? prediction) =>
        prediction?.Seal is not null && prediction.Approval is not null &&
        prediction.Seal.Sha256 == prediction.Approval.SealedSha256;

    private static bool BindingMatches(
        Scenario scenario, PredictionRecord? prediction, OperatorBackendClient.Binding? binding) =>
        prediction?.Seal is not null && binding is not null && binding.ScenarioId == scenario.ScenarioId &&
        binding.ApprovedSealedPredictionHash == prediction.Seal.Sha256 &&
        binding.SourcePackageSha256 == prediction.Body.FieldPackageSha256 &&
        binding.WorldModelVersion == scenario.WorldModelVersion;

    private static bool CanCancel(OperatorBackendClient.Run run) =>
        run.Status is "Queued" or "Running" or "Blocked" or "AwaitingDependency" or "AwaitingApproval" or "ReadyToReveal" &&
        run.CurrentStage is not ("S8PublishReveal" or "S9Score");

    private static bool CanResume(OperatorBackendClient.Run run) =>
        run.Status == "AwaitingDependency" &&
        Array.IndexOf(OperatorBackendClient.StageNames, run.CurrentStage) is >= 1 and <= 7;

    private static bool CanPublish(OperatorBackendClient.Run run) =>
        run.Status is "ReadyToReveal" or "PublishFailed" ||
        (run.Status == "Running" && run.CurrentStage == "S8PublishReveal");

    private static bool CanScore(OperatorBackendClient.Run run) =>
        run.Status == "Revealed" ||
        (run.CurrentStage == "S9Score" && run.Status is "Running" or "AwaitingDependency");

    private static string? FailureReason(OperatorBackendClient.Run run) => run.Status switch
    {
        "Failed" when run.DiagnosticCode == "PlannedPathOutsideModelCoverage" =>
            "The approved well path is outside the prepared model's coverage. Drilling did not start. " +
            "Create a new scenario with a target and path inside the modeled area and depth range, or ask the operator for a model covering that location. " +
            "Changing the seed or Preview/Standard resolution does not expand coverage.",
        "Failed" when run.DiagnosticCode == "AsDrilledPathOutsideModelCoverage" =>
            "The simulated drill path left the prepared model's coverage. This was detected when sampling geology; " +
            "it is a path/model mismatch, not a geology calculation failure. Create a new scenario with more room for steering " +
            "inside the model area, or ask the operator for a model covering that path.",
        "Failed" when run.DiagnosticCode == "StageADataMismatch" =>
            "The reservoir service rejected the path or geology-sampling request. Check its ReservoirRequestRejected log " +
            "on the failing request trace for the invalid fields. A terminal run cannot resume; correct the identified " +
            "path/model or sampling-settings mismatch before creating a new scenario.",
        "Failed" => "Simulation failed. Internal diagnostics remain operator-local; a terminal run is not automatically retried.",
        "PublishFailed" => "Publication did not complete. Inspect the existing evidence state before an explicit retry.",
        "AwaitingDependency" => "The current checkpoint is waiting for a dependency. Only its permitted retry action is enabled.",
        "AwaitingApproval" => "Review the observable completion openings and explicitly approve their exact hash to continue.",
        "Blocked" => "The current checkpoint is blocked; operator setup is required.",
        _ => null
    };

    private static OperatorActionAvailability Available(bool enabled, string? reason) => new(enabled, enabled ? null : reason);
    private static string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string ValidateActor(string? actor)
    {
        string value = actor?.Trim() ?? string.Empty;
        if (value.Length is < 1 or > 200 || value.Any(char.IsControl))
            throw new OperatorWorkflowException(400, "Actor must be a nonempty audit label of at most 200 characters without control characters.");
        return PredictionValidator.ValidateActor(value);
    }

    private static void ValidateHash(string? value)
    {
        if (value is null || value.Length != 64 || value.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'f')))
            throw new OperatorWorkflowException(400, "A reviewed lowercase SHA-256 hash is required.");
    }
}
