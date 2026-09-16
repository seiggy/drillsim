using System.Globalization;
using System.Text.Json;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Persistence;

internal sealed record CompletionConnectionGroup(
    string OpeningId,
    string ReservoirName,
    CompletionOpeningType Type,
    IReadOnlyList<WellConnection> Connections);

internal sealed record ResolvedCompletionBinding(
    ApprovedCompletionBindingMetadata Metadata,
    IReadOnlyList<CompletionConnectionGroup> Openings);

internal sealed class ApprovedCompletionBindingService(
    SqliteCompletionBindingRepository repository,
    RestrictedTruthSamplingService pathService,
    TimeProvider timeProvider)
{
    internal const string ModelVersion = "observed-log-completion-v1";
    private const string IdentityVersion = "approved-completion-binding-v1";
    private const string AuditIdentityVersion = "completion-binding-audit-v1";
    private const int MaximumConnections = 10_000;
    private const int MaximumMappingProbes = 100_000;

    internal async Task<ApprovedCompletionBindingMetadata> RegisterAsync(
        ReservoirWorld world,
        ApprovedCompletionBindingRequest request,
        CancellationToken cancellationToken = default)
    {
        RestoredPathBinding path = await LoadAsDrilledPathAsync(
            world, request.PathBindingId, cancellationToken);
        ValidateRequest(world, path, request);
        CompletionConnectionGroup[] mapped = MapConnections(world, path.Stations, request.Openings);
        string requestJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        string requestHash = DeterministicEncoding.Sha256Hex(requestJson);
        string connectionJson = JsonSerializer.Serialize(mapped, DeterministicEncoding.JsonOptions);
        string connectionHash = DeterministicEncoding.Sha256Hex(connectionJson);
        int connectionCount = mapped.Sum(group => group.Connections.Count);
        string identity = string.Join("\n",
            IdentityVersion, world.Summary.WorldId, path.Metadata.BindingId,
            path.Metadata.ScenarioId.ToString("D"), path.Metadata.RunId.ToString("D"),
            path.Metadata.ApprovedSealedPredictionSha256, world.Summary.ModelVersion,
            request.CompletionModelVersion, requestHash, connectionHash);
        string completionBindingId = "rcb_" + DeterministicEncoding.Sha256Hex(identity);
        var persisted = new PersistedCompletionBinding(
            completionBindingId, world.Summary.WorldId, path.Metadata.BindingId,
            path.Metadata.ScenarioId, path.Metadata.RunId, path.Metadata.ApprovedSealedPredictionSha256,
            request.CompletionModelVersion, requestJson, requestHash, connectionJson, connectionHash,
            request.Openings.Count, connectionCount, timeProvider.GetUtcNow());
        await repository.SaveBindingAsync(persisted, cancellationToken);
        await SaveAuditAsync(persisted, "registration", requestHash, cancellationToken);
        return Metadata(persisted);
    }

    internal async Task<ApprovedCompletionBindingMetadata?> GetMetadataAsync(
        ReservoirWorld world,
        string completionBindingId,
        CancellationToken cancellationToken = default)
    {
        ResolvedCompletionBinding? binding = await LoadVerifiedAsync(
            world, completionBindingId, cancellationToken);
        return binding?.Metadata;
    }

    internal async Task<ResolvedCompletionBinding?> ResolveAsync(
        ReservoirWorld world,
        string completionBindingId,
        CancellationToken cancellationToken = default)
    {
        ResolvedCompletionBinding? binding = await LoadVerifiedAsync(
            world, completionBindingId, cancellationToken);
        if (binding is null)
            return null;
        string requestHash = DeterministicEncoding.Sha256Hex(JsonSerializer.Serialize(new
        {
            completionBindingId,
            worldId = world.Summary.WorldId
        }, DeterministicEncoding.JsonOptions));
        PersistedCompletionBinding persisted = (await repository.LoadBindingAsync(
            completionBindingId, cancellationToken))!;
        await SaveAuditAsync(persisted, "resolution", requestHash, cancellationToken);
        return binding;
    }

    private async Task<ResolvedCompletionBinding?> LoadVerifiedAsync(
        ReservoirWorld world, string completionBindingId, CancellationToken cancellationToken)
    {
        PersistedCompletionBinding? persisted = await repository.LoadBindingAsync(
            completionBindingId, cancellationToken);
        if (persisted is null)
            return null;
        if (persisted.WorldId != world.Summary.WorldId)
            throw Validation("completionBindingId", "Completion binding belongs to a different world.");
        if (persisted.ModelVersion != ModelVersion)
            throw new PersistenceIntegrityException(
                $"Completion binding {completionBindingId} model version is not supported.");
        RestoredPathBinding path = await LoadAsDrilledPathAsync(
            world, persisted.PathBindingId, cancellationToken);
        if (path.Metadata.ScenarioId != persisted.ScenarioId || path.Metadata.RunId != persisted.RunId ||
            path.Metadata.ApprovedSealedPredictionSha256 != persisted.ApprovedPredictionSha256)
            throw new PersistenceIntegrityException(
                $"Completion binding {completionBindingId} conflicts with its approved path authority.");
        string requestHash = DeterministicEncoding.Sha256Hex(persisted.CanonicalRequestJson);
        string connectionHash = DeterministicEncoding.Sha256Hex(persisted.MappedConnectionJson);
        if (requestHash != persisted.CanonicalRequestHash || connectionHash != persisted.MappedConnectionHash)
            throw new PersistenceIntegrityException(
                $"Completion binding {completionBindingId} failed a canonical content hash.");
        string identity = string.Join("\n",
            IdentityVersion, persisted.WorldId, persisted.PathBindingId,
            persisted.ScenarioId.ToString("D"), persisted.RunId.ToString("D"),
            persisted.ApprovedPredictionSha256, world.Summary.ModelVersion,
            persisted.ModelVersion, requestHash, connectionHash);
        string expectedId = "rcb_" + DeterministicEncoding.Sha256Hex(identity);
        if (expectedId != persisted.CompletionBindingId)
            throw new PersistenceIntegrityException(
                $"Completion binding {completionBindingId} failed its deterministic identity.");

        ApprovedCompletionBindingRequest request = JsonSerializer.Deserialize<ApprovedCompletionBindingRequest>(
            persisted.CanonicalRequestJson, DeterministicEncoding.JsonOptions)
            ?? throw new PersistenceIntegrityException(
                $"Completion binding {completionBindingId} canonical request is null.");
        ValidateRequest(world, path, request);
        CompletionConnectionGroup[] mapped = MapConnections(world, path.Stations, request.Openings);
        string regeneratedJson = JsonSerializer.Serialize(mapped, DeterministicEncoding.JsonOptions);
        if (regeneratedJson != persisted.MappedConnectionJson ||
            request.Openings.Count != persisted.OpeningCount ||
            mapped.Sum(group => group.Connections.Count) != persisted.ProducingConnectionCount)
            throw new PersistenceIntegrityException(
                $"Completion binding {completionBindingId} mapped connections failed regeneration.");
        return new ResolvedCompletionBinding(Metadata(persisted), mapped);
    }

    private async Task<RestoredPathBinding> LoadAsDrilledPathAsync(
        ReservoirWorld world, string pathBindingId, CancellationToken cancellationToken)
    {
        RestoredPathBinding path = await pathService.LoadBindingAsync(
            world, pathBindingId, cancellationToken)
            ?? throw Validation("pathBindingId", "Approved path binding was not found.");
        if (path.Metadata.PathKind != ApprovedPathKind.AsDrilled)
            throw Validation("pathBindingId", "Completion binding requires an AsDrilled approved path.");
        return path;
    }

    private static void ValidateRequest(
        ReservoirWorld world, RestoredPathBinding path, ApprovedCompletionBindingRequest request)
    {
        var errors = new ValidationErrors();
        if (request.PathBindingId != path.Metadata.BindingId)
            errors.Add("pathBindingId", "Path binding ID does not match the verified path.");
        if (request.CompletionModelVersion != ModelVersion)
            errors.Add("completionModelVersion", $"Completion model version must be {ModelVersion}.");
        if (request.Openings is null || request.Openings.Count is < 1 or > 128)
        {
            errors.Add("openings", "Completion design must contain between 1 and 128 openings.");
            errors.ThrowIfAny();
            return;
        }
        double pathMinimumMd = path.Stations[0].MeasuredDepthM;
        double pathMaximumMd = path.Stations[^1].MeasuredDepthM;
        double previousTop = double.NegativeInfinity;
        int producingCount = 0;
        var openingIds = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < request.Openings.Count; index++)
        {
            ApprovedCompletionOpening? opening = request.Openings[index];
            string key = $"openings[{index}]";
            if (opening is null)
            {
                errors.Add(key, "Completion opening must not be null.");
                continue;
            }
            if (!IsToken(opening.OpeningId) || !openingIds.Add(opening.OpeningId))
                errors.Add($"{key}.openingId", "Opening ID must be a unique canonical token.");
            if (string.IsNullOrWhiteSpace(opening.ReservoirName) || opening.ReservoirName.Length > 200 ||
                opening.ReservoirName != world.Summary.ReservoirName)
                errors.Add($"{key}.reservoirName", "Reservoir name must exactly match the bound world reservoir.");
            if (!Enum.IsDefined(opening.Type))
                errors.Add($"{key}.type", "Completion opening type is invalid.");
            Finite(errors, $"{key}.topMeasuredDepthM", opening.TopMeasuredDepthM);
            Finite(errors, $"{key}.baseMeasuredDepthM", opening.BaseMeasuredDepthM);
            if (!(opening.TopMeasuredDepthM >= pathMinimumMd &&
                opening.TopMeasuredDepthM < opening.BaseMeasuredDepthM &&
                opening.BaseMeasuredDepthM <= pathMaximumMd))
                errors.Add(key, "Opening interval must be ordered and contained by the approved path MD range.");
            if (opening.TopMeasuredDepthM < previousTop)
                errors.Add(key, "Completion openings must be ordered by top MD.");
            previousTop = opening.TopMeasuredDepthM;
            Finite(errors, $"{key}.wellboreRadiusM", opening.WellboreRadiusM);
            if (!(opening.WellboreRadiusM > 0 && opening.WellboreRadiusM <= 2))
                errors.Add($"{key}.wellboreRadiusM", "Wellbore radius must be in (0, 2] m.");
            Finite(errors, $"{key}.skin", opening.Skin);
            if (opening.Skin is < -10 or > 100)
                errors.Add($"{key}.skin", "Skin must be in [-10, 100].");
            Finite(errors, $"{key}.efficiency", opening.Efficiency);
            if (!(opening.Efficiency >= 0 && opening.Efficiency <= 1))
                errors.Add($"{key}.efficiency", "Efficiency must be in [0, 1].");
            Finite(errors, $"{key}.uncertaintyM", opening.UncertaintyM);
            if (!(opening.UncertaintyM >= 0 && opening.UncertaintyM <= 1_000))
                errors.Add($"{key}.uncertaintyM", "Uncertainty must be in [0, 1,000] m.");
            if (opening.Type != CompletionOpeningType.Isolated)
            {
                producingCount++;
                if (!(opening.Efficiency > 0))
                    errors.Add($"{key}.efficiency", "A producing opening must have positive efficiency.");
            }
        }
        if (producingCount == 0)
            errors.Add("openings", "At least one Perforated or OpenHole opening is required.");
        for (int left = 0; left < request.Openings.Count; left++)
        for (int right = left + 1; right < request.Openings.Count; right++)
        {
            ApprovedCompletionOpening first = request.Openings[left];
            ApprovedCompletionOpening second = request.Openings[right];
            bool overlap = first.TopMeasuredDepthM < second.BaseMeasuredDepthM &&
                second.TopMeasuredDepthM < first.BaseMeasuredDepthM;
            if (overlap && first.ReservoirName == second.ReservoirName &&
                first.Type != CompletionOpeningType.Isolated &&
                second.Type != CompletionOpeningType.Isolated)
                errors.Add("openings", "Producing openings may not overlap within a reservoir.");
        }
        errors.ThrowIfAny();
    }

    private static CompletionConnectionGroup[] MapConnections(
        ReservoirWorld world,
        IReadOnlyList<ApprovedPathStation> path,
        IReadOnlyList<ApprovedCompletionOpening> openings)
    {
        double spacing = Math.Max(0.25, 0.5 * Math.Min(
            Math.Min(world.Grid.CellSizeXM, world.Grid.CellSizeYM), world.CellThicknessM.Min()));
        int probes = 0;
        var isolatedCells = new HashSet<int>();
        foreach (ApprovedCompletionOpening opening in openings.Where(
            opening => opening.Type == CompletionOpeningType.Isolated))
            foreach (MappedCell mapped in CellsAlongOpening(world, path, opening, spacing, ref probes))
                isolatedCells.Add(mapped.Cell);

        var usedCells = new HashSet<int>();
        var groups = new List<CompletionConnectionGroup>();
        foreach (ApprovedCompletionOpening opening in openings.Where(
            opening => opening.Type != CompletionOpeningType.Isolated))
        {
            MappedCell[] mappedCells = CellsAlongOpening(world, path, opening, spacing, ref probes)
                .Where(mapped => !isolatedCells.Contains(mapped.Cell))
                .DistinctBy(mapped => mapped.Cell)
                .ToArray();
            if (mappedCells.Length == 0)
                throw Validation("openings",
                    $"Producing opening {opening.OpeningId} maps no active reservoir cell after isolation exclusions.");
            var connections = new List<WellConnection>(mappedCells.Length);
            foreach (MappedCell mapped in mappedCells)
            {
                if (!usedCells.Add(mapped.Cell))
                    throw Validation("openings", "Producing openings map the same active grid cell.");
                connections.Add(new WellConnection
                {
                    I = mapped.I,
                    J = mapped.J,
                    K = mapped.K,
                    WellboreRadiusM = opening.WellboreRadiusM,
                    Skin = opening.Skin,
                    OpenFraction = opening.Efficiency
                });
                if (usedCells.Count > MaximumConnections)
                    throw Validation("openings", "Completion design exceeds 10,000 unique active connections.");
            }
            groups.Add(new CompletionConnectionGroup(
                opening.OpeningId, opening.ReservoirName, opening.Type, connections));
        }
        return groups.ToArray();
    }

    private readonly record struct MappedCell(int Cell, int I, int J, int K);

    private static MappedCell[] CellsAlongOpening(
        ReservoirWorld world,
        IReadOnlyList<ApprovedPathStation> path,
        ApprovedCompletionOpening opening,
        double spacing,
        ref int totalProbes)
    {
        int divisions = Math.Max(1, (int)Math.Ceiling(
            (opening.BaseMeasuredDepthM - opening.TopMeasuredDepthM) / spacing));
        totalProbes = checked(totalProbes + divisions + 1);
        if (totalProbes > MaximumMappingProbes)
            throw Validation("openings", "Completion mapping exceeds its bounded probe budget.");
        var cells = new List<MappedCell>(divisions + 1);
        for (int division = 0; division <= divisions; division++)
        {
            double md = opening.TopMeasuredDepthM +
                (opening.BaseMeasuredDepthM - opening.TopMeasuredDepthM) * division / divisions;
            ApprovedPathStation station = InterpolateAtMd(path, md);
            if (TryCell(world, station, out MappedCell mapped))
                cells.Add(mapped);
        }
        return cells.ToArray();
    }


    private static ApprovedPathStation InterpolateAtMd(
        IReadOnlyList<ApprovedPathStation> path, double md)
    {
        int upper = 1;
        while (upper < path.Count && path[upper].MeasuredDepthM < md)
            upper++;
        ApprovedPathStation start = path[upper - 1];
        ApprovedPathStation end = path[upper];
        double fraction = (md - start.MeasuredDepthM) /
            (end.MeasuredDepthM - start.MeasuredDepthM);
        return new ApprovedPathStation
        {
            MeasuredDepthM = md,
            EastingM = Lerp(start.EastingM, end.EastingM, fraction),
            NorthingM = Lerp(start.NorthingM, end.NorthingM, fraction),
            TrueVerticalDepthM = Lerp(start.TrueVerticalDepthM, end.TrueVerticalDepthM, fraction)
        };
    }

    private static bool TryCell(
        ReservoirWorld world, ApprovedPathStation station, out MappedCell mapped)
    {
        int i = Math.Min(world.Grid.CountX - 1,
            (int)((station.EastingM - world.Grid.OriginEastingM) / world.Grid.CellSizeXM));
        int j = Math.Min(world.Grid.CountY - 1,
            (int)((station.NorthingM - world.Grid.OriginNorthingM) / world.Grid.CellSizeYM));
        if (i < 0 || j < 0)
        {
            mapped = default;
            return false;
        }
        int column = world.Grid.ColumnIndex(i, j);
        double top = world.TopDepthM[column];
        double @base = world.BaseDepthM[column];
        if (station.TrueVerticalDepthM < top || station.TrueVerticalDepthM > @base)
        {
            mapped = default;
            return false;
        }
        int k = Math.Min(world.Grid.CountZ - 1,
            (int)((station.TrueVerticalDepthM - top) / (@base - top) * world.Grid.CountZ));
        int cell = world.Grid.CellIndex(i, j, k);
        mapped = new MappedCell(cell, i, j, k);
        return true;
    }

    private async Task SaveAuditAsync(
        PersistedCompletionBinding binding, string action, string requestHash,
        CancellationToken cancellationToken)
    {
        string identity = string.Join("\n",
            AuditIdentityVersion, binding.CompletionBindingId, binding.WorldId,
            binding.PathBindingId, binding.ScenarioId.ToString("D"), binding.RunId.ToString("D"),
            action, requestHash, binding.ProducingConnectionCount.ToString(CultureInfo.InvariantCulture));
        string auditId = "rca_" + DeterministicEncoding.Sha256Hex(identity);
        await repository.SaveAuditAsync(new PersistedCompletionBindingAudit(
            auditId, binding.CompletionBindingId, binding.WorldId, binding.PathBindingId,
            binding.ScenarioId, binding.RunId, action, requestHash,
            binding.ProducingConnectionCount, timeProvider.GetUtcNow()), cancellationToken);
    }

    private static ApprovedCompletionBindingMetadata Metadata(PersistedCompletionBinding binding) => new(
        binding.CompletionBindingId, binding.WorldId, binding.PathBindingId,
        binding.ScenarioId, binding.RunId, binding.ModelVersion,
        binding.OpeningCount, binding.ProducingConnectionCount,
        binding.CanonicalRequestHash, binding.CreatedUtc);

    private static bool IsToken(string value) => value is { Length: > 0 and <= 100 } &&
        (value[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9') &&
        value.All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or
            >= '0' and <= '9' or '.' or '_' or ':' or '-');

    private static void Finite(ValidationErrors errors, string key, double value) =>
        errors.RequireFinite(key, value);

    private static double Lerp(double start, double end, double fraction) =>
        start + fraction * (end - start);

    private static ReservoirValidationException Validation(string key, string message) => new(
        new Dictionary<string, string[]> { [key] = [message] });
}
