using System.Globalization;
using System.Text.Json;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Persistence;

internal sealed class WorldDeletionBlockedException(string message) : InvalidOperationException(message);

internal sealed record CanonicalPathBinding(
    string WorldId,
    Guid ScenarioId,
    Guid RunId,
    ApprovedPathKind PathKind,
    string ApprovedSealedPredictionSha256,
    IReadOnlyList<ApprovedPathStation> Stations);

internal sealed record RestoredPathBinding(
    ApprovedPathBindingMetadata Metadata,
    IReadOnlyList<ApprovedPathStation> Stations);

internal sealed record SamplingExecution(
    RestrictedTruthSamplingResult Result,
    string ResponseHash,
    string AuditId);

internal sealed class RestrictedTruthSamplingService(
    SqliteTruthSamplingRepository repository,
    TimeProvider timeProvider)
{
    private const string BindingIdentityVersion = "approved-path-binding-v1";
    private const string PropertySetVersion = "stage-b-truth-v1";
    private const string AuditIdentityVersion = "sampling-audit-v1";
    private const int MaximumResponseSamples = 10_000;
    private const int EstimatedBytesPerSample = 200;
    private const int MaximumResponseBytes = 2_000_000;

    internal async Task<ApprovedPathBindingMetadata> RegisterAsync(
        ReservoirWorld world,
        ApprovedPathBindingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateBindingRequest(world, request);
        var canonical = new CanonicalPathBinding(
            world.Summary.WorldId, request.ScenarioId, request.RunId, request.PathKind,
            request.ApprovedSealedPredictionSha256, request.Stations.ToArray());
        string canonicalJson = JsonSerializer.Serialize(canonical, DeterministicEncoding.JsonOptions);
        string canonicalHash = DeterministicEncoding.Sha256Hex(canonicalJson);
        string bindingId = "rpb_" + DeterministicEncoding.Sha256Hex(
            BindingIdentityVersion + "\n" + canonicalJson);
        var persisted = new PersistedPathBinding(
            bindingId, world.Summary.WorldId, request.ScenarioId, request.RunId,
            (int)request.PathKind, request.ApprovedSealedPredictionSha256,
            canonicalJson, canonicalHash, request.Stations.Count, timeProvider.GetUtcNow());
        await repository.SaveBindingAsync(persisted, cancellationToken);
        return Metadata(persisted);
    }

    internal async Task<RestoredPathBinding?> LoadBindingAsync(
        ReservoirWorld world,
        string bindingId,
        CancellationToken cancellationToken = default)
    {
        PersistedPathBinding? persisted = await repository.LoadBindingAsync(bindingId, cancellationToken);
        if (persisted is null)
            return null;
        if (!string.Equals(persisted.WorldId, world.Summary.WorldId, StringComparison.Ordinal))
            throw new ReservoirValidationException(new Dictionary<string, string[]>
            {
                ["bindingId"] = ["Approved path binding belongs to a different world."]
            });
        string canonicalHash = DeterministicEncoding.Sha256Hex(persisted.CanonicalJson);
        if (!string.Equals(canonicalHash, persisted.CanonicalHash, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"Approved path binding {bindingId} failed its canonical hash.");
        string expectedId = "rpb_" + DeterministicEncoding.Sha256Hex(
            BindingIdentityVersion + "\n" + persisted.CanonicalJson);
        if (!string.Equals(expectedId, persisted.BindingId, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"Approved path binding {bindingId} failed its deterministic identity.");
        CanonicalPathBinding canonical = JsonSerializer.Deserialize<CanonicalPathBinding>(
            persisted.CanonicalJson, DeterministicEncoding.JsonOptions)
            ?? throw new PersistenceIntegrityException($"Approved path binding {bindingId} canonical JSON is null.");
        var request = new ApprovedPathBindingRequest
        {
            ScenarioId = canonical.ScenarioId,
            RunId = canonical.RunId,
            PathKind = canonical.PathKind,
            ApprovedSealedPredictionSha256 = canonical.ApprovedSealedPredictionSha256,
            Stations = canonical.Stations
        };
        ValidateBindingRequest(world, request);
        if (canonical.WorldId != persisted.WorldId || canonical.ScenarioId != persisted.ScenarioId ||
            canonical.RunId != persisted.RunId || (int)canonical.PathKind != persisted.PathKind ||
            canonical.ApprovedSealedPredictionSha256 != persisted.ApprovedPredictionSha256 ||
            canonical.Stations.Count != persisted.StationCount)
            throw new PersistenceIntegrityException($"Approved path binding {bindingId} metadata conflicts with its content.");
        return new RestoredPathBinding(Metadata(persisted), canonical.Stations);
    }

    internal async Task<SamplingExecution> SampleAsync(
        ReservoirWorld world,
        TruthSamplingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateSamplingRequest(request);
        RestoredPathBinding binding = await LoadBindingAsync(world, request.BindingId, cancellationToken)
            ?? throw new ReservoirValidationException(new Dictionary<string, string[]>
            {
                ["bindingId"] = ["Approved path binding was not found."]
            });
        IReadOnlyList<ApprovedPathStation> points = InterpolatePath(world, binding.Stations, request);
        var samples = new List<RestrictedTruthSample>(Math.Min(points.Count, request.MaximumSamples));
        var sampledCells = new HashSet<int>();
        foreach (ApprovedPathStation point in points)
        {
            if (!TryCell(world, point, out int cell, out int column))
                continue;
            if (!sampledCells.Add(cell))
                continue;
            double oil = world.OilSaturation[cell];
            double water = world.WaterSaturation[cell];
            double gas = world.GasSaturation[cell];
            double closure = oil + water + gas;
            double permeability = Math.Sqrt(
                world.HorizontalPermeabilityXM2[cell] * world.HorizontalPermeabilityYM2[cell]);
            if (!double.IsFinite(closure) || Math.Abs(closure - 1) > 1e-10 ||
                !double.IsFinite(world.Porosity[cell]) || !double.IsFinite(permeability) ||
                !double.IsFinite(world.NetToGross[cell]) || !double.IsFinite(world.PressurePa[cell]))
                throw new PersistenceIntegrityException($"Restricted truth is invalid at MD {point.MeasuredDepthM:G17} m.");
            samples.Add(new RestrictedTruthSample(
                point.MeasuredDepthM, point.EastingM, point.NorthingM, point.TrueVerticalDepthM,
                world.TopDepthM[column], world.BaseDepthM[column], world.Porosity[cell], permeability,
                world.NetToGross[cell], world.NetToGross[cell] >= 0.5, world.PressurePa[cell],
                oil, water, gas));
            if (samples.Count > request.MaximumSamples || samples.Count > MaximumResponseSamples ||
                samples.Count * EstimatedBytesPerSample > MaximumResponseBytes)
                throw Validation("maximumSamples", "Restricted truth response exceeds its bounded size.");
        }
        if (samples.Count == 0)
            throw Validation("bindingId", "Approved path does not intersect an active reservoir cell.");

        var result = new RestrictedTruthSamplingResult(
            binding.Metadata.BindingId, binding.Metadata.WorldId, binding.Metadata.ScenarioId,
            binding.Metadata.RunId, request.PropertySetVersion, samples.Count, samples);
        string canonicalRequestJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        string requestHash = DeterministicEncoding.Sha256Hex(canonicalRequestJson);
        string responseJson = JsonSerializer.Serialize(result, DeterministicEncoding.JsonOptions);
        string responseHash = DeterministicEncoding.Sha256Hex(responseJson);
        string auditIdentity = string.Join("\n",
            AuditIdentityVersion, binding.Metadata.BindingId, binding.Metadata.WorldId,
            binding.Metadata.ScenarioId.ToString("D"), binding.Metadata.RunId.ToString("D"),
            request.CallerLabel, requestHash, samples.Count.ToString(CultureInfo.InvariantCulture), responseHash);
        string auditId = "rsa_" + DeterministicEncoding.Sha256Hex(auditIdentity);
        await repository.SaveAuditAsync(new PersistedSamplingAudit(
            auditId, binding.Metadata.BindingId, binding.Metadata.WorldId,
            binding.Metadata.ScenarioId, binding.Metadata.RunId, request.CallerLabel,
            requestHash, samples.Count, responseHash, timeProvider.GetUtcNow()), cancellationToken);
        return new SamplingExecution(result, responseHash, auditId);
    }

    internal async Task<PersistedSamplingAudit?> LoadVerifiedAuditAsync(
        string auditId, CancellationToken cancellationToken = default)
    {
        PersistedSamplingAudit? audit = await repository.LoadAuditAsync(auditId, cancellationToken);
        if (audit is null)
            return null;
        string identity = string.Join("\n",
            AuditIdentityVersion, audit.BindingId, audit.WorldId, audit.ScenarioId.ToString("D"),
            audit.RunId.ToString("D"), audit.CallerLabel, audit.CanonicalRequestHash,
            audit.SampledCount.ToString(CultureInfo.InvariantCulture), audit.ResponseHash);
        string expected = "rsa_" + DeterministicEncoding.Sha256Hex(identity);
        if (!string.Equals(expected, audit.AuditId, StringComparison.Ordinal))
            throw new PersistenceIntegrityException($"Sampling audit {auditId} failed its deterministic identity.");
        return audit;
    }

    internal Task<bool> WorldHasBindingsAsync(
        string worldId, CancellationToken cancellationToken = default) =>
        repository.WorldHasBindingsAsync(worldId, cancellationToken);

    private static void ValidateBindingRequest(ReservoirWorld world, ApprovedPathBindingRequest request)
    {
        var errors = new ValidationErrors();
        if (request.ScenarioId == Guid.Empty)
            errors.Add("scenarioId", "Scenario ID must be a nonempty canonical GUID.");
        if (request.RunId == Guid.Empty)
            errors.Add("runId", "Run ID must be a nonempty canonical GUID.");
        if (!Enum.IsDefined(request.PathKind))
            errors.Add("pathKind", "Path kind is invalid.");
        if (!IsLowerSha256(request.ApprovedSealedPredictionSha256))
            errors.Add("approvedSealedPredictionSha256",
                "Approved prediction SHA-256 must be exactly 64 lowercase hexadecimal characters.");
        if (request.Stations is null || request.Stations.Count is < 2 or > 2_000)
        {
            errors.Add("stations", "Approved path must contain between 2 and 2,000 stations.");
            errors.ThrowIfAny();
            return;
        }

        double minimumEasting = world.Grid.OriginEastingM;
        double maximumEasting = minimumEasting + world.Grid.CountX * world.Grid.CellSizeXM;
        double minimumNorthing = world.Grid.OriginNorthingM;
        double maximumNorthing = minimumNorthing + world.Grid.CountY * world.Grid.CellSizeYM;
        double reservoirTop = world.TopDepthM.Min();
        double reservoirBase = world.BaseDepthM.Max();
        double verticalTolerance = Math.Max(1_000, reservoirBase - reservoirTop);
        double previousMd = -1;
        bool outsideCoverage = false;
        for (int index = 0; index < request.Stations.Count; index++)
        {
            ApprovedPathStation? station = request.Stations[index];
            string key = $"stations[{index}]";
            if (station is null)
            {
                errors.Add(key, "Path station must not be null.");
                continue;
            }
            if (!double.IsFinite(station.MeasuredDepthM) || station.MeasuredDepthM < 0)
                errors.Add($"{key}.measuredDepthM", "Measured depth must be finite and nonnegative.");
            if (index > 0 && !(station.MeasuredDepthM > previousMd))
                errors.Add($"{key}.measuredDepthM", "Measured depth must be strictly increasing.");
            if (!double.IsFinite(station.EastingM) || station.EastingM < minimumEasting || station.EastingM > maximumEasting)
            {
                errors.Add($"{key}.eastingM", "Path station is outside the hidden world easting extent.");
                outsideCoverage |= double.IsFinite(station.EastingM);
            }
            if (!double.IsFinite(station.NorthingM) || station.NorthingM < minimumNorthing || station.NorthingM > maximumNorthing)
            {
                errors.Add($"{key}.northingM", "Path station is outside the hidden world northing extent.");
                outsideCoverage |= double.IsFinite(station.NorthingM);
            }
            if (!double.IsFinite(station.TrueVerticalDepthM) || station.TrueVerticalDepthM < 0 ||
                station.TrueVerticalDepthM > reservoirBase + verticalTolerance)
                errors.Add($"{key}.trueVerticalDepthM", "TVD must be finite, nonnegative, and within the bounded deep reservoir envelope.");
            outsideCoverage |= double.IsFinite(station.TrueVerticalDepthM) &&
                station.TrueVerticalDepthM > reservoirBase + verticalTolerance;
            previousMd = station.MeasuredDepthM;
        }
        errors.ThrowIfAny(outsideCoverage ? "PathOutsideModelCoverage" : null);
    }

    private static void ValidateSamplingRequest(TruthSamplingRequest request)
    {
        var errors = new ValidationErrors();
        if (!IsOpaqueId(request.BindingId, "rpb_"))
            errors.Add("bindingId", "Approved path binding ID is invalid.");
        errors.RequireFinite("maximumSpacingM", request.MaximumSpacingM);
        if (request.MaximumSpacingM is < 0.25 or > 1_000)
            errors.Add("maximumSpacingM", "Maximum spacing must be in [0.25, 1,000] m.");
        if (request.MaximumSamples is < 1 or > MaximumResponseSamples)
            errors.Add("maximumSamples", "Maximum samples must be between 1 and 10,000.");
        if (!string.Equals(request.PropertySetVersion, PropertySetVersion, StringComparison.Ordinal))
            errors.Add("propertySetVersion", $"Property set version must be {PropertySetVersion}.");
        if (string.IsNullOrWhiteSpace(request.CallerLabel) || request.CallerLabel.Length > 100)
            errors.Add("callerLabel", "Caller label is required and must not exceed 100 characters.");
        errors.ThrowIfAny();
    }

    private static IReadOnlyList<ApprovedPathStation> InterpolatePath(
        ReservoirWorld world, IReadOnlyList<ApprovedPathStation> stations, TruthSamplingRequest request)
    {
        double gridResolutionSpacing = 0.5 * Math.Min(
            Math.Min(world.Grid.CellSizeXM, world.Grid.CellSizeYM), world.CellThicknessM.Min());
        double effectiveSpacing = Math.Min(request.MaximumSpacingM, Math.Max(0.25, gridResolutionSpacing));
        long count = 1;
        for (int index = 1; index < stations.Count; index++)
            count += (long)Math.Ceiling(
                (stations[index].MeasuredDepthM - stations[index - 1].MeasuredDepthM) / effectiveSpacing);
        if (count > request.MaximumSamples || count > MaximumResponseSamples ||
            count * EstimatedBytesPerSample > MaximumResponseBytes)
            throw Validation("maximumSpacingM",
                "Requested path density exceeds the bounded sampling or response budget.");

        var points = new List<ApprovedPathStation>((int)count) { stations[0] };
        for (int index = 1; index < stations.Count; index++)
        {
            ApprovedPathStation start = stations[index - 1];
            ApprovedPathStation end = stations[index];
            int divisions = (int)Math.Ceiling(
                (end.MeasuredDepthM - start.MeasuredDepthM) / effectiveSpacing);
            for (int division = 1; division <= divisions; division++)
            {
                double fraction = (double)division / divisions;
                points.Add(new ApprovedPathStation
                {
                    MeasuredDepthM = Lerp(start.MeasuredDepthM, end.MeasuredDepthM, fraction),
                    EastingM = Lerp(start.EastingM, end.EastingM, fraction),
                    NorthingM = Lerp(start.NorthingM, end.NorthingM, fraction),
                    TrueVerticalDepthM = Lerp(start.TrueVerticalDepthM, end.TrueVerticalDepthM, fraction)
                });
            }
        }
        return points;
    }

    private static bool TryCell(
        ReservoirWorld world, ApprovedPathStation point, out int cell, out int column)
    {
        int i = Math.Min(world.Grid.CountX - 1,
            (int)((point.EastingM - world.Grid.OriginEastingM) / world.Grid.CellSizeXM));
        int j = Math.Min(world.Grid.CountY - 1,
            (int)((point.NorthingM - world.Grid.OriginNorthingM) / world.Grid.CellSizeYM));
        if (i < 0 || j < 0)
        {
            cell = -1;
            column = -1;
            return false;
        }
        column = world.Grid.ColumnIndex(i, j);
        double top = world.TopDepthM[column];
        double @base = world.BaseDepthM[column];
        if (point.TrueVerticalDepthM < top || point.TrueVerticalDepthM > @base)
        {
            cell = -1;
            return false;
        }
        double fraction = (point.TrueVerticalDepthM - top) / (@base - top);
        int k = Math.Min(world.Grid.CountZ - 1, (int)(fraction * world.Grid.CountZ));
        cell = world.Grid.CellIndex(i, j, k);
        return true;
    }

    private static ApprovedPathBindingMetadata Metadata(PersistedPathBinding binding) => new(
        binding.BindingId, binding.WorldId, binding.ScenarioId, binding.RunId,
        (ApprovedPathKind)binding.PathKind, binding.ApprovedPredictionSha256,
        binding.CanonicalHash, binding.StationCount, binding.CreatedUtc);

    private static bool IsLowerSha256(string value) => value is { Length: 64 } &&
        value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsOpaqueId(string value, string prefix) =>
        value.Length == 68 && value.StartsWith(prefix, StringComparison.Ordinal) &&
        value.AsSpan(4).ToString().All(
            character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static double Lerp(double start, double end, double fraction) =>
        start + fraction * (end - start);

    private static ReservoirValidationException Validation(string key, string message) => new(
        new Dictionary<string, string[]> { [key] = [message] });
}
