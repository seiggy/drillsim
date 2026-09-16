using System.Net;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed partial class OperatorWorkflowService
{
    public async Task<OperatorSimulationSetup> GetSimulationSetupAsync(Guid scenarioId, CancellationToken ct)
    {
        Scenario scenario = await GetScenarioAsync(scenarioId, ct);
        return await backend.GetSimulationSetupAsync(scenario, ct);
    }

    public async Task<OperatorActionResult> PrepareSimulationAsync(
        Guid scenarioId, OperatorSimulationSetupRequest request, string key, CancellationToken ct)
    {
        _ = ValidateActor(request.Actor);
        ValidateHash(request.ReviewedSealHash);
        if (request.Actor != request.Actor.Trim() || request.Actor.Length > 100 || request.Actor.Any(c => c > 126) ||
            !OperatorBackendClient.SetupLabel(request.ProfileId, 200) ||
            request.Resolution is not ("Preview" or "Standard") || request.RealizationSeed < 0)
            throw new OperatorWorkflowException(400, "Choose a model, resolution, nonnegative seed and operator label of at most 100 visible ASCII characters.");
        _ = await GetScenarioAsync(scenarioId, ct);
        PredictionRecord? prediction = await ledger.GetPredictionAsync(scenarioId, ct);
        ValidatePredictionScenario(scenarioId, prediction);
        if (!IsApproved(prediction) || prediction!.Seal!.Sha256 != request.ReviewedSealHash)
            throw new OperatorWorkflowException(409, "Review, seal and approve this prediction before preparing the simulator.");
        // Replay must reach the backend even after this scenario has completed.
        await backend.PrepareSimulationAsync(scenarioId, request, key, ct);
        return new(scenarioId, null, "simulation-prepared",
            "Simulator prepared. Review the settings, then start the simulation.");
    }
}

public sealed partial class OperatorBackendClient
{
    private sealed record SetupResult(
        Guid ScenarioId, string Outcome, OperatorSimulationConfiguration Configuration, string ReviewedSealHash);

    internal async Task<OperatorSimulationSetup> GetSimulationSetupAsync(Scenario scenario, CancellationToken ct)
    {
        OperatorSimulationSetup setup = await GetAsync<OperatorSimulationSetup>(
            $"scenarios/{scenario.ScenarioId:D}/setup", false, ct) ?? throw InvalidResponse();
        if (setup.ScenarioId != scenario.ScenarioId || setup.Profiles is null || setup.Profiles.Count > 128 ||
            setup.Defaults is null || setup.Defaults.Resolution is not ("Preview" or "Standard") ||
            setup.Defaults.RealizationSeed < 0 ||
            setup.Profiles.Any(profile => profile is null || !SetupLabel(profile.ProfileId, 200) ||
                !SetupLabel(profile.Name, 300) || !SetupLabel(profile.Description, 2000) ||
                profile.WorldModelVersion != scenario.WorldModelVersion) ||
            setup.Profiles.Select(profile => profile.ProfileId).Distinct(StringComparer.Ordinal).Count() != setup.Profiles.Count ||
            setup.ReviewedSealHash is not null && !RecoveryHash(setup.ReviewedSealHash) ||
            setup.Available && (setup.Prepared || setup.Profiles.Count == 0 || setup.ReviewedSealHash is null) ||
            !setup.Available && !SetupLabel(setup.Reason, 2000) ||
            setup.Current is not null && (!setup.Prepared || !ValidSetupConfiguration(setup.Current)) ||
            setup.Defaults.ProfileId is not null &&
                setup.Defaults.ProfileId != setup.Current?.ProfileId &&
                !setup.Profiles.Any(profile => profile.ProfileId == setup.Defaults.ProfileId))
            throw InvalidResponse();
        return setup;
    }

    internal async Task PrepareSimulationAsync(
        Guid scenarioId, OperatorSimulationSetupRequest body, string key, CancellationToken ct)
    {
        using var request = Request(HttpMethod.Post, $"scenarios/{scenarioId:D}/setup", key);
        request.Content = JsonContent.Create(body);
        using HttpResponseMessage response = await clients.CreateClient(ClientName).SendAsync(request, ct);
        RequireSuccess(response);
        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Created)) throw InvalidResponse();
        SetupResult result = await ReadAsync<SetupResult>(response, ct);
        if (result.ScenarioId != scenarioId || result.Outcome != "simulation-prepared" ||
            result.ReviewedSealHash != body.ReviewedSealHash || !ValidSetupConfiguration(result.Configuration) ||
            result.Configuration.ProfileId != body.ProfileId || result.Configuration.Resolution != body.Resolution ||
            result.Configuration.RealizationSeed != body.RealizationSeed || result.Configuration.PreparedBy != body.Actor)
            throw InvalidResponse();
    }

    private static bool ValidSetupConfiguration(OperatorSimulationConfiguration? value) =>
        value is not null && SetupLabel(value.ProfileId, 200) && SetupLabel(value.ProfileName, 300) &&
        value.Resolution is "Preview" or "Standard" && value.RealizationSeed >= 0 &&
        SetupLabel(value.PreparedBy, 100) && value.PreparedUtc != default;

    internal static bool SetupLabel(string? value, int maximum) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && !value.Any(char.IsControl);
}
