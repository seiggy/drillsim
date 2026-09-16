using System.Globalization;
using System.Text.Json;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Persistence;

internal sealed class ReservoirPersistenceInitializer(
    SqliteReservoirRepository repository,
    SqliteTruthSamplingRepository truthRepository,
    SqliteCompletionBindingRepository completionRepository,
    SqliteCompletionProductionRepository productionRepository) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await repository.InitializeAsync(cancellationToken);
        await truthRepository.InitializeAsync(cancellationToken);
        await completionRepository.InitializeAsync(cancellationToken);
        await productionRepository.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


internal sealed class PersistentWorldManager(
    IReservoirWorldFactory factory,
    IWorldStore cache,
    SqliteReservoirRepository repository,
    TimeProvider timeProvider)
{
    internal async Task<ReservoirWorld> CreateAsync(
        WorldGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReservoirWorld world = factory.Create(request);
        string canonicalJson = ReservoirWorldFactory.CanonicalRequestJson(request);
        string truthChecksum = ReservoirWorldFactory.TruthChecksum(world);
        cancellationToken.ThrowIfCancellationRequested();
        await repository.SaveWorldSpecAsync(new PersistedWorldSpec(
            world.Summary.WorldId,
            ReservoirWorldFactory.ModelVersion,
            canonicalJson,
            timeProvider.GetUtcNow(),
            truthChecksum), cancellationToken);
        cache.Upsert(world);
        return world;
    }

    internal async Task<ReservoirWorld?> GetAsync(
        string worldId,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGet(worldId, out ReservoirWorld? cached))
            return cached;

        PersistedWorldSpec? spec = await repository.LoadWorldSpecAsync(worldId, cancellationToken);
        if (spec is null)
            return null;
        return RestoreVerifiedWorld(spec, cancellationToken).World;
    }

    internal async Task<WorldSetupProfileCatalog> GetSetupProfilesAsync(
        Guid fieldId,
        string reservoirName,
        CancellationToken cancellationToken = default)
    {
        WorldSetupRequestValidator.ValidateScope(fieldId, reservoirName);
        Dictionary<string, WorldGenerationRequest> templates = await GetSetupTemplatesAsync(
            fieldId, reservoirName, cancellationToken);
        WorldSetupProfile[] profiles = templates.Keys.Order(StringComparer.Ordinal)
            .Select(profileId => new WorldSetupProfile(
                profileId,
                $"{reservoirName} conditioned model ({profileId[4..12]})",
                "Conditioned synthetic workflow model; not a calibrated commercial forecast.",
                ReservoirWorldFactory.ModelVersion))
            .ToArray();
        return new WorldSetupProfileCatalog(fieldId, reservoirName, profiles);
    }

    internal async Task<ReservoirWorld?> PrepareRealizationAsync(
        WorldSetupRequest request,
        CancellationToken cancellationToken = default)
    {
        WorldSetupRequestValidator.Validate(request);
        Dictionary<string, WorldGenerationRequest> templates = await GetSetupTemplatesAsync(
            request.FieldId, request.ReservoirName, cancellationToken);
        if (!templates.TryGetValue(request.ProfileId, out WorldGenerationRequest? template))
            return null;

        WorldGenerationRequest realization = template with
        {
            Seed = request.RealizationSeed,
            Grid = SetupGrid(template.Grid, request.Resolution)
        };
        try
        {
            return await CreateAsync(realization, cancellationToken);
        }
        catch (ReservoirValidationException exception)
        {
            throw new PersistenceIntegrityException(
                "The selected model cannot produce the requested setup realization.", exception);
        }
    }

    private async Task<Dictionary<string, WorldGenerationRequest>> GetSetupTemplatesAsync(
        Guid fieldId,
        string reservoirName,
        CancellationToken cancellationToken)
    {
        var templates = new Dictionary<string, WorldGenerationRequest>(StringComparer.Ordinal);
        PersistedWorldSpec? after = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<PersistedWorldSpec> page = await repository.ListWorldSpecsAsync(
                fieldId, reservoirName, ReservoirWorldFactory.ModelVersion, after, cancellationToken);
            if (page.Count == 0)
                return templates;

            foreach (PersistedWorldSpec spec in page)
            {
                WorldGenerationRequest template = RestoreVerifiedWorld(spec, cancellationToken).Request;
                if (template.FieldId != fieldId ||
                    !string.Equals(template.ReservoirName, reservoirName, StringComparison.Ordinal))
                    throw new PersistenceIntegrityException("A setup model specification has an invalid scope.");
                WorldGenerationRequest normalized = template with
                {
                    Seed = 0,
                    Grid = SetupGrid(template.Grid, "Standard")
                };
                string profileId = "rsp_" + DeterministicEncoding.Sha256Hex(
                    "reservoir-setup-profile-v1\n" + ReservoirWorldFactory.ModelVersion + "\n" +
                    ReservoirWorldFactory.CanonicalRequestJson(normalized));
                // Pages are ordered by creation time then ID, so the first verified representative wins.
                templates.TryAdd(profileId, template);
            }
            after = page[^1];
        }
    }

    private static GridOptions SetupGrid(GridOptions template, string resolution) =>
        resolution == "Preview"
            ? template with { CountX = 16, CountY = 16, CountZ = 8 }
            : template with { CountX = 64, CountY = 64, CountZ = 20 };

    private (WorldGenerationRequest Request, ReservoirWorld World) RestoreVerifiedWorld(
        PersistedWorldSpec spec,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string worldId = spec.WorldId;
        if (!string.Equals(spec.ModelVersion, ReservoirWorldFactory.ModelVersion, StringComparison.Ordinal))
            throw new PersistenceIntegrityException(
                $"World {worldId} uses model version {spec.ModelVersion}; expected {ReservoirWorldFactory.ModelVersion}.");

        WorldGenerationRequest request;
        string canonicalJson;
        try
        {
            request = ReservoirWorldFactory.RequestFromCanonicalJson(spec.CanonicalRequestJson);
            canonicalJson = ReservoirWorldFactory.CanonicalRequestJson(request);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidDataException or ReservoirValidationException)
        {
            throw new PersistenceIntegrityException($"World {worldId} request JSON is invalid.", exception);
        }
        if (!string.Equals(canonicalJson, spec.CanonicalRequestJson, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"World {worldId} request JSON is not canonical.");
        string expectedWorldId = ReservoirWorldFactory.CreateWorldId(canonicalJson);
        if (!string.Equals(expectedWorldId, worldId, StringComparison.Ordinal))
            throw new PersistenceIntegrityException(
                $"Regenerated world ID {expectedWorldId} does not match persisted ID {worldId}.");
        ReservoirWorld world;
        try
        {
            world = cache.TryGet(worldId, out ReservoirWorld? cached) ? cached! : factory.Create(request);
        }
        catch (ReservoirValidationException exception)
        {
            throw new PersistenceIntegrityException($"World {worldId} cannot be regenerated.", exception);
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.Equals(world.Summary.WorldId, worldId, StringComparison.Ordinal))
            throw new PersistenceIntegrityException(
                $"Regenerated world ID {world.Summary.WorldId} does not match persisted ID {worldId}.");
        string truthChecksum = ReservoirWorldFactory.TruthChecksum(world);
        if (!string.Equals(truthChecksum, spec.TruthChecksum, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"Regenerated world {worldId} failed its truth checksum.");
        cache.Upsert(world);
        return (request, world);
    }

    internal async Task DeleteAsync(string worldId, CancellationToken cancellationToken = default)
    {
        await repository.DeleteWorldAsync(worldId, cancellationToken);
        cache.Remove(worldId);
    }
}

internal sealed record RestoredSimulationState(
    string StateId,
    string WorldId,
    string? ParentStateId,
    double SimulatedTimeSeconds,
    SimulationState State);

internal sealed class PersistentSimulationStateStore(
    SqliteReservoirRepository repository,
    TimeProvider timeProvider)
{
    internal async Task<string> SaveAsync(
        ReservoirWorld world,
        string? parentStateId,
        SimulationRequest request,
        SimulationExecution execution,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(parentStateId, request.ContinueFromStateId, StringComparison.Ordinal))
            throw new ArgumentException(
                "Parent state ID must match the canonical run request continuation ID.", nameof(parentStateId));
        string runJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        string runHash = DeterministicEncoding.Sha256Hex(runJson);
        string stateChecksum = StateBlobCodec.Checksum(execution.FinalState);
        string stateId = CreateStateId(
            world.Summary.WorldId, parentStateId, runHash, execution.Result.SimulatedTimeSeconds, stateChecksum);
        var state = new PersistedState(
            stateId,
            world.Summary.WorldId,
            parentStateId,
            ReservoirWorldFactory.ModelVersion,
            runJson,
            runHash,
            execution.Result.SimulatedTimeSeconds,
            StateBlobCodec.Compress(execution.FinalState.PressurePa),
            StateBlobCodec.Compress(execution.FinalState.OilSaturation),
            StateBlobCodec.Compress(execution.FinalState.WaterSaturation),
            StateBlobCodec.Compress(execution.FinalState.GasSaturation),
            stateChecksum,
            timeProvider.GetUtcNow());
        await repository.SaveStateAsync(state, cancellationToken);
        return stateId;
    }

    internal async Task<RestoredSimulationState?> LoadAsync(
        ReservoirWorld world,
        string stateId,
        CancellationToken cancellationToken = default)
    {
        PersistedState? persisted = await repository.LoadStateAsync(
            world.Summary.WorldId, stateId, cancellationToken);
        if (persisted is null)
            return null;
        ValidateMetadata(world, persisted);

        int cellCount = world.Grid.CellCount;
        var state = new SimulationState(
            StateBlobCodec.Decompress(persisted.PressureBlob, cellCount),
            StateBlobCodec.Decompress(persisted.OilBlob, cellCount),
            StateBlobCodec.Decompress(persisted.WaterBlob, cellCount),
            StateBlobCodec.Decompress(persisted.GasBlob, cellCount));
        string checksum = StateBlobCodec.Checksum(state);
        if (!string.Equals(checksum, persisted.StateChecksum, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"Simulation state {stateId} failed its checksum.");
        ValidateArrays(state);
        return new RestoredSimulationState(
            persisted.StateId, persisted.WorldId, persisted.ParentStateId, persisted.SimulatedTimeSeconds, state);
    }

    internal async Task<SimulationStateSummary?> GetMetadataAsync(
        string worldId,
        string stateId,
        CancellationToken cancellationToken = default)
    {
        PersistedStateMetadata? metadata = await repository.LoadStateMetadataAsync(
            worldId, stateId, cancellationToken);
        if (metadata is null)
            return null;
        if (!string.Equals(metadata.ModelVersion, ReservoirWorldFactory.ModelVersion, StringComparison.Ordinal))
            throw new PersistenceIntegrityException(
                $"State {stateId} uses model version {metadata.ModelVersion}; expected {ReservoirWorldFactory.ModelVersion}.");
        if (!double.IsFinite(metadata.SimulatedTimeSeconds) || metadata.SimulatedTimeSeconds < 0)
            throw new PersistenceIntegrityException($"State {stateId} has invalid simulated time.");
        return new SimulationStateSummary(
            metadata.StateId, metadata.WorldId, metadata.ParentStateId, metadata.SimulatedTimeSeconds,
            metadata.ModelVersion, metadata.CreatedUtc);
    }

    private static void ValidateMetadata(ReservoirWorld world, PersistedState persisted)
    {
        if (!string.Equals(persisted.ModelVersion, ReservoirWorldFactory.ModelVersion, StringComparison.Ordinal))
            throw new PersistenceIntegrityException(
                $"State {persisted.StateId} uses model version {persisted.ModelVersion}; expected {ReservoirWorldFactory.ModelVersion}.");
        if (!double.IsFinite(persisted.SimulatedTimeSeconds) || persisted.SimulatedTimeSeconds < 0)
            throw new PersistenceIntegrityException($"State {persisted.StateId} has invalid simulated time.");
        string runHash = DeterministicEncoding.Sha256Hex(persisted.CanonicalRunRequestJson);
        if (!string.Equals(runHash, persisted.RunRequestHash, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"State {persisted.StateId} run request hash is invalid.");
        SimulationRequest request = JsonSerializer.Deserialize<SimulationRequest>(
            persisted.CanonicalRunRequestJson, DeterministicEncoding.JsonOptions)
            ?? throw new PersistenceIntegrityException($"State {persisted.StateId} run request is null.");
        SimulationRequestValidator.Validate(request, world);
        if (!string.Equals(request.ContinueFromStateId, persisted.ParentStateId, StringComparison.Ordinal))
            throw new PersistenceIntegrityException(
                $"State {persisted.StateId} parent does not match its canonical run request.");
        string canonicalRunJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        if (!string.Equals(canonicalRunJson, persisted.CanonicalRunRequestJson, StringComparison.Ordinal) &&
            !IsPreScheduleCanonicalRequest(persisted.CanonicalRunRequestJson))
            throw new PersistenceIntegrityException($"State {persisted.StateId} run request JSON is not canonical.");
        string expectedStateId = CreateStateId(
            persisted.WorldId, persisted.ParentStateId, persisted.RunRequestHash,
            persisted.SimulatedTimeSeconds, persisted.StateChecksum);
        if (!string.Equals(expectedStateId, persisted.StateId, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"State {persisted.StateId} deterministic identity is invalid.");
    }

    private static bool IsPreScheduleCanonicalRequest(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return !document.RootElement.TryGetProperty("schedule", out _);
    }


    private static void ValidateArrays(SimulationState state)
    {
        for (int cell = 0; cell < state.PressurePa.Length; cell++)
        {
            double closure = state.OilSaturation[cell] + state.WaterSaturation[cell] + state.GasSaturation[cell];
            if (!double.IsFinite(state.PressurePa[cell]) || state.PressurePa[cell] <= 0 ||
                !double.IsFinite(state.OilSaturation[cell]) || state.OilSaturation[cell] < 0 ||
                !double.IsFinite(state.WaterSaturation[cell]) || state.WaterSaturation[cell] < 0 ||
                !double.IsFinite(state.GasSaturation[cell]) || state.GasSaturation[cell] < 0 ||
                Math.Abs(closure - 1) > 1e-10)
                throw new PersistenceIntegrityException($"State array values are invalid in cell {cell}.");
        }
    }

    private static string CreateStateId(
        string worldId,
        string? parentStateId,
        string runRequestHash,
        double simulatedTimeSeconds,
        string stateChecksum)
    {
        string identity = string.Join("\n",
            ReservoirWorldFactory.ModelVersion,
            worldId,
            parentStateId ?? string.Empty,
            runRequestHash,
            BitConverter.DoubleToInt64Bits(simulatedTimeSeconds).ToString("x16", CultureInfo.InvariantCulture),
            stateChecksum);
        return $"rss_{DeterministicEncoding.Sha256Hex(identity)}";
    }
}
