using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrillingOperations;

public sealed record SimulationModelProfile(string ProfileId, string Name, string Description, string WorldModelVersion);
public sealed record SimulationSetupDefaults(string? ProfileId, string Resolution, int RealizationSeed);
public sealed record SimulationConfiguration(
    string ProfileId, string ProfileName, string Resolution, int RealizationSeed, string PreparedBy, DateTimeOffset PreparedUtc);
public sealed record SimulationSetupView(
    Guid ScenarioId, bool Available, string? Reason, bool Prepared, SimulationConfiguration? Current,
    IReadOnlyList<SimulationModelProfile> Profiles, SimulationSetupDefaults Defaults, string? ReviewedSealHash);
public sealed record SimulationSetupResult(
    Guid ScenarioId, string Outcome, SimulationConfiguration Configuration, string ReviewedSealHash);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrepareSimulationRequest(
    [property: JsonRequired] string Actor,
    [property: JsonRequired] string ProfileId,
    [property: JsonRequired] string Resolution,
    [property: JsonRequired] int RealizationSeed,
    [property: JsonRequired] string ReviewedSealHash);

public sealed class SimulationSetupException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class ReservoirSetupClient(HttpClient client)
{
    private sealed record ProfileCatalog(Guid FieldId, string ReservoirName, IReadOnlyList<SimulationModelProfile> Profiles);

    public async Task<IReadOnlyList<SimulationModelProfile>> GetProfilesAsync(
        AnalysisScenarioDto scenario, CancellationToken ct)
    {
        using HttpResponseMessage response = await client.GetAsync(
            $"reservoirsimulation/api/worlds/setup-profiles?fieldId={scenario.SourceFieldId:D}&reservoirName={Uri.EscapeDataString(scenario.ReservoirName)}", ct);
        RequireSuccess(response);
        await response.Content.LoadIntoBufferAsync(128 * 1024, ct);
        ProfileCatalog catalog = await response.Content.ReadFromJsonAsync<ProfileCatalog>(CanonicalJson.SerializerOptions, ct)
            ?? throw InvalidCatalog();
        if (catalog.FieldId != scenario.SourceFieldId || catalog.ReservoirName != scenario.ReservoirName ||
            catalog.Profiles is null || catalog.Profiles.Count > 128 ||
            catalog.Profiles.Any(profile => profile is null ||
                !SimulationSetupCoordinator.IsLabel(profile.ProfileId, 200) ||
                !SimulationSetupCoordinator.IsLabel(profile.Name, 300) ||
                !SimulationSetupCoordinator.IsLabel(profile.Description, 2000) ||
                profile.WorldModelVersion != scenario.WorldModelVersion) ||
            catalog.Profiles.Select(profile => profile.ProfileId).Distinct(StringComparer.Ordinal).Count() != catalog.Profiles.Count)
            throw InvalidCatalog();
        return catalog.Profiles;
    }

    public async Task<ReservoirWorldDto> PrepareAsync(
        AnalysisScenarioDto scenario, PrepareSimulationRequest request, CancellationToken ct)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync("reservoirsimulation/api/worlds/setup", new
        {
            fieldId = scenario.SourceFieldId, reservoirName = scenario.ReservoirName,
            request.ProfileId, request.Resolution, request.RealizationSeed
        }, CanonicalJson.SerializerOptions, ct);
        RequireSuccess(response);
        await response.Content.LoadIntoBufferAsync(128 * 1024, ct);
        ReservoirWorldDto world = await response.Content.ReadFromJsonAsync<ReservoirWorldDto>(CanonicalJson.SerializerOptions, ct)
            ?? throw InvalidCatalog();
        if (world.FieldId != scenario.SourceFieldId || world.ReservoirName != scenario.ReservoirName ||
            world.ModelVersion != scenario.WorldModelVersion || string.IsNullOrWhiteSpace(world.WorldId) ||
            world.WorldId.Length > 200 || world.CalibrationArtifact is null ||
            !SimulationSetupCoordinator.IsLabel(world.CalibrationArtifact.Id, 200) ||
            !SimulationSetupCoordinator.IsHash(world.CalibrationArtifact.Sha256))
            throw InvalidCatalog();
        return world;
    }

    private static void RequireSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        throw new SimulationSetupException(response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict ? 409 : 503,
            "The reservoir service could not prepare the selected model. Refresh model options before retrying.");
    }

    private static SimulationSetupException InvalidCatalog() => new(503, "The reservoir model response could not be verified.");
}

public sealed class SimulationSetupCoordinator(
    DrillingOperationsStore store, AnalysisVerificationClient analysis,
    ReservoirSetupClient reservoir, BindingVerificationService verifier)
{
    public async Task<SimulationSetupView> GetAsync(Guid scenarioId, CancellationToken ct)
    {
        AnalysisScenarioDto scenario = await GetScenarioAsync(scenarioId, ct);
        AnalysisPredictionDto? prediction = await analysis.FindPredictionAsync(scenarioId, ct);
        if (prediction is not null && prediction.ScenarioId != scenarioId)
            throw new SimulationSetupException(409, "The prediction does not belong to this scenario.");
        TruthBindingResponse? binding = await store.GetBindingAsync(scenarioId.ToString("D"), ct);
        SimulationSetupResult? setup = await store.GetSimulationSetupAsync(scenarioId.ToString("D"), ct);
        bool prepared = binding is not null && prediction?.Seal is not null &&
            binding.ApprovedSealedPredictionHash == prediction.Seal.Sha256 &&
            binding.SourcePackageSha256 == prediction.Body?.FieldPackageSha256 &&
            binding.WorldModelVersion == scenario.WorldModelVersion;
        if (binding is not null && !prepared || setup is not null && (!prepared || setup.ReviewedSealHash != prediction?.Seal?.Sha256))
            throw new SimulationSetupException(409, "The existing simulation setup does not match the current prediction.");
        IReadOnlyList<SimulationModelProfile> profiles = prepared ? [] : await reservoir.GetProfilesAsync(scenario, ct);
        string? reason = prepared ? "The simulator is already prepared. Its settings are fixed for this scenario."
            : prediction?.Seal is null ? "Save and seal a prediction first."
            : prediction.Approval?.SealedSha256 != prediction.Seal.Sha256 || scenario.Status != "HumanApproved"
                ? "Approve the sealed prediction before preparing the simulator."
            : profiles.Count == 0 ? "No prepared model profile is available for this field and reservoir."
            : null;
        int seed = (int)(BinaryPrimitives.ReadUInt32LittleEndian(SHA256.HashData(scenarioId.ToByteArray())) & int.MaxValue);
        return new(scenarioId, reason is null, reason, prepared, setup?.Configuration, profiles,
            new(setup?.Configuration.ProfileId ?? profiles.FirstOrDefault()?.ProfileId,
                setup?.Configuration.Resolution ?? "Preview", setup?.Configuration.RealizationSeed ?? seed),
            prediction?.Seal?.Sha256);
    }

    public async Task<ApiOutcome> PrepareAsync(
        string route, string key, Guid scenarioId, PrepareSimulationRequest request, CancellationToken ct)
    {
        Validate(request);
        string canonical = CanonicalJson.Serialize(request);
        ApiOutcome? replay = await store.TryReplayAsync(route, key, canonical, ct);
        if (replay is not null) return replay;
        (AnalysisScenarioDto scenario, AnalysisPredictionDto prediction) = await analysis.GetAsync(scenarioId, ct);
        if (scenario.ScenarioId != scenarioId || scenario.Status != "HumanApproved" ||
            prediction.ScenarioId != scenarioId || prediction.Seal?.Sha256 != request.ReviewedSealHash ||
            prediction.Approval?.SealedSha256 != request.ReviewedSealHash || prediction.Body is null)
            throw new SimulationSetupException(409, "The reviewed prediction must be sealed and approved for this scenario.");
        if (await store.GetBindingAsync(scenarioId.ToString("D"), ct) is not null)
            throw new SimulationSetupException(409, "This scenario already has an immutable simulation setup. Start it or create a new scenario.");
        IReadOnlyList<SimulationModelProfile> profiles = await reservoir.GetProfilesAsync(scenario, ct);
        SimulationModelProfile profile = profiles.SingleOrDefault(item => item.ProfileId == request.ProfileId)
            ?? throw new SimulationSetupException(409, "The selected model profile is not available for this field and reservoir.");
        ReservoirWorldDto world = await reservoir.PrepareAsync(scenario, request, ct);
        var binding = new BindWorldRequest(scenarioId.ToString("D"), request.ReviewedSealHash,
            prediction.Body.FieldPackageSha256, world.WorldId, world.ModelVersion,
            world.CalibrationArtifact!.Id, world.CalibrationArtifact.Sha256);
        await verifier.VerifyAsync(binding, ct);
        return await store.PrepareSimulationAsync(route, key, request, profile.Name, binding, ct);
    }

    private async Task<AnalysisScenarioDto> GetScenarioAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) throw new SimulationSetupException(400, "A scenario is required.");
        AnalysisScenarioDto scenario = await analysis.GetScenarioAsync(id, ct);
        if (scenario.ScenarioId != id || scenario.SourceFieldId == Guid.Empty ||
            !IsLabel(scenario.ReservoirName, 200) || !IsLabel(scenario.WorldModelVersion, 200))
            throw new SimulationSetupException(409, "The scenario setup scope could not be verified.");
        return scenario;
    }

    internal static void Validate(PrepareSimulationRequest request)
    {
        if (request is null || !IsLabel(request.Actor, 100) || request.Actor != request.Actor.Trim() ||
            request.Actor.Any(c => c > 126) || !IsLabel(request.ProfileId, 200) ||
            request.Resolution is not ("Preview" or "Standard") || request.RealizationSeed < 0 ||
            !IsHash(request.ReviewedSealHash))
            throw new SimulationSetupException(400, "Supply an operator label, model profile, Preview or Standard resolution, nonnegative seed, and the reviewed prediction seal.");
    }

    internal static bool IsLabel(string? value, int maximum) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && !value.Any(char.IsControl);
    internal static bool IsHash(string? value) =>
        value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
}
