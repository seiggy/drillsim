using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class ScenarioService
{
    private const int MaximumLabelLength = 200;
    private readonly SqliteScenarioStore _store;
    private readonly IFieldPackageService _packages;
    private readonly IPackageHasher _hasher;
    private readonly TimeProvider _timeProvider;

    public ScenarioService(
        SqliteScenarioStore store,
        IFieldPackageService packages,
        IPackageHasher hasher,
        TimeProvider timeProvider)
    {
        _store = store;
        _packages = packages;
        _hasher = hasher;
        _timeProvider = timeProvider;
    }

    public async Task<Scenario> CreateAsync(
        CreateScenarioRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        NormalizedScenarioRequest normalized = Normalize(request);
        Guid scenarioId = ScenarioIdentity.Create(
            normalized.SourceFieldId,
            normalized.ReservoirName,
            normalized.AsOfUtc,
            normalized.SeedLabel,
            normalized.AssumptionsSha256);
        Scenario? existing = await _store.FindAsync(scenarioId, cancellationToken);
        if (existing is not null)
            return existing;

        AnalysisPackage package = await _packages.BuildPackageAsync(normalized.SourceFieldId, cancellationToken);
        if (package.FieldId != normalized.SourceFieldId)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Field package mismatch",
                $"The field package identified field {package.FieldId:D}, not requested field {normalized.SourceFieldId:D}.");
        }

        DateTimeOffset now = _timeProvider.GetUtcNow().ToUniversalTime();
        var scenario = new Scenario(
            scenarioId,
            normalized.SourceFieldId,
            null,
            normalized.ReservoirName,
            normalized.AsOfUtc,
            normalized.AsOfUtc,
            normalized.SeedLabel,
            ScenarioModelVersions.World,
            ScenarioModelVersions.Observation,
            ScenarioModelVersions.Scoring,
            ScenarioStatus.Draft,
            normalized.AssumptionsSha256,
            now,
            now);
        EvidenceVisibility[] visibility = EvidenceCatalog.Enumerate(package)
            .Select(item => new EvidenceVisibility(
                scenarioId,
                item.EvidenceId,
                item.RecordKind,
                normalized.AsOfUtc,
                null,
                null))
            .ToArray();

        return await _store.CreateAsync(scenario, visibility, package, now, cancellationToken);
    }

    public Task<IReadOnlyList<Scenario>> ListAsync(CancellationToken cancellationToken = default) =>
        _store.ListAsync(cancellationToken);

    public async Task<Scenario> GetAsync(Guid scenarioId, CancellationToken cancellationToken = default) =>
        await _store.FindAsync(scenarioId, cancellationToken)
        ?? throw new ScenarioApiException(
            StatusCodes.Status404NotFound,
            "Scenario not found",
            $"Scenario {scenarioId:D} does not exist.");

    public async Task<AnalysisPackage> GetPackageAsync(
        Guid fieldId,
        Guid? scenarioId,
        DateTimeOffset? asOf,
        CancellationToken cancellationToken = default)
    {
        if (scenarioId is null)
            return await _packages.BuildPackageAsync(fieldId, cancellationToken);

        Scenario scenario = await GetAsync(scenarioId.Value, cancellationToken);
        return await GetPackageAsync(fieldId, scenario, asOf, cancellationToken);
    }

    internal Task<AnalysisPackage> GetCurrentPackageAsync(
        Scenario scenario,
        Guid fieldId,
        CancellationToken cancellationToken = default) =>
        GetPackageAsync(fieldId, scenario, scenario.AsOfUtc, cancellationToken);

    private async Task<AnalysisPackage> GetPackageAsync(
        Guid fieldId,
        Scenario scenario,
        DateTimeOffset? asOf,
        CancellationToken cancellationToken)
    {
        EnsureFieldMatches(scenario, fieldId);
        DateTimeOffset effectiveTime = GetEffectiveTime(scenario, asOf);
        IReadOnlyList<EvidenceVisibility> visibility =
            await _store.GetEvidenceVisibilityAsync(scenario.ScenarioId, cancellationToken);
        AnalysisPackage package = fieldId == scenario.SourceFieldId
            ? await GetSourcePackageSnapshotAsync(scenario, cancellationToken)
            : await GetClonePackageSnapshotAsync(scenario, cancellationToken);
        if (package.FieldId != fieldId)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Field package mismatch",
                $"The field package identified field {package.FieldId:D}, not requested field {fieldId:D}.");
        }
        return FilterPackage(package, scenario, effectiveTime, visibility, _hasher);
    }

    private async Task<AnalysisPackage> GetClonePackageSnapshotAsync(
        Scenario scenario,
        CancellationToken cancellationToken)
    {
        AnalysisPackage? snapshot =
            await _store.FindClonePackageSnapshotAsync(scenario, cancellationToken);
        return snapshot ?? throw new ScenarioApiException(
            StatusCodes.Status409Conflict,
            "Clone package snapshot unavailable",
            $"Scenario {scenario.ScenarioId:D} has no immutable revealed clone package snapshot. Operator backfill is required.");
    }

    public async Task<EvidenceVisibilitySummary> GetVisibilitySummaryAsync(
        Guid scenarioId,
        DateTimeOffset? asOf = null,
        CancellationToken cancellationToken = default)
    {
        Scenario scenario = await GetAsync(scenarioId, cancellationToken);
        return await GetVisibilitySummaryAsync(scenario, asOf, cancellationToken);
    }

    internal Task<EvidenceVisibilitySummary> GetCurrentVisibilitySummaryAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default) =>
        GetVisibilitySummaryAsync(scenario, scenario.AsOfUtc, cancellationToken);

    internal async Task<PublicScorecard> GetCurrentScorecardAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        if (scenario.Status != ScenarioStatus.Scored)
            throw ScorecardUnavailable();

        PublicScorecard scorecard = await _store.FindScorecardAsync(
            scenario.ScenarioId,
            cancellationToken) ?? throw ScorecardUnavailable();
        RevealReceipt? reveal = await _store.FindRevealAsync(
            scenario.ScenarioId,
            cancellationToken);
        if (reveal is null ||
            scorecard.ScenarioId != scenario.ScenarioId ||
            reveal.ScenarioId != scenario.ScenarioId ||
            scorecard.RevealId != reveal.RevealId ||
            reveal.ClonedFieldId != scenario.ClonedFieldId ||
            scorecard.CreatedValidTimeUtc > scenario.AsOfUtc ||
            reveal.AsOfUtc > scenario.AsOfUtc ||
            scorecard.CreatedValidTimeUtc != reveal.AsOfUtc ||
            !string.Equals(
                scorecard.ScoringModelVersion,
                scenario.ScoringModelVersion,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Scenario {scenario.ScenarioId:D} has a public scorecard inconsistent with the bound scenario snapshot.");
        }

        return scorecard;
    }

    private async Task<EvidenceVisibilitySummary> GetVisibilitySummaryAsync(
        Scenario scenario,
        DateTimeOffset? asOf,
        CancellationToken cancellationToken)
    {
        DateTimeOffset effectiveTime = GetEffectiveTime(scenario, asOf);
        IReadOnlyList<EvidenceVisibility> rows =
            await _store.GetEvidenceVisibilityAsync(scenario.ScenarioId, cancellationToken);
        AnalysisPackage sourcePackage = await GetSourcePackageSnapshotAsync(scenario, cancellationToken);
        var inventoryPackages = new List<AnalysisPackage> { sourcePackage };
        if (scenario.ClonedFieldId is not null)
            inventoryPackages.Add(await GetClonePackageSnapshotAsync(scenario, cancellationToken));

        var inventory = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (AnalysisPackage package in inventoryPackages)
            foreach (EvidenceDescriptor item in EvidenceCatalog.Enumerate(package))
                inventory[item.EvidenceId] = item.RecordKind;
        foreach (EvidenceVisibility row in rows)
            inventory[row.EvidenceId] = row.RecordKind;
        Dictionary<string, EvidenceVisibility> rowsById = rows
            .ToDictionary(item => item.EvidenceId, StringComparer.Ordinal);

        List<ClassifiedEvidence> classified = inventory.Select(item => new ClassifiedEvidence(
            item.Key,
            item.Value,
            rowsById.TryGetValue(item.Key, out EvidenceVisibility? row) && IsVisible(row, effectiveTime)
                ? EvidenceVisibilityStatus.Visible
                : EvidenceVisibilityStatus.Hidden))
            .ToList();
        foreach (AnalysisPackage package in inventoryPackages)
        {
            AddRecordsWithoutIds(classified, EvidenceCatalog.Cluster, package.Clusters);
            AddRecordsWithoutIds(classified, EvidenceCatalog.Well, package.Wells);
            AddRecordsWithoutIds(classified, EvidenceCatalog.WellBore, package.WellBores);
            AddRecordsWithoutIds(classified, EvidenceCatalog.Architecture, package.WellBoreArchitectures);
            AddRecordsWithoutIds(classified, EvidenceCatalog.Trajectory, package.Trajectories);
            AddRecordsWithoutIds(classified, EvidenceCatalog.Geology, package.GeologicalProperties);
        }

        EvidenceVisibilityCount[] counts = classified
            .GroupBy(item => (item.RecordKind, item.Status))
            .OrderBy(group => EvidenceCatalog.KindOrder(group.Key.RecordKind))
            .ThenBy(group => group.Key.RecordKind, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Status)
            .Select(group => new EvidenceVisibilityCount(
                group.Key.RecordKind,
                group.Key.Status,
                group.Count(),
                group.Key.Status == EvidenceVisibilityStatus.Visible
                    ? group.Select(item => item.EvidenceId!).Order(StringComparer.Ordinal).ToArray()
                    : null))
            .ToArray();

        return new EvidenceVisibilitySummary(scenario.ScenarioId, effectiveTime, counts);
    }

    private static void AddRecordsWithoutIds(
        ICollection<ClassifiedEvidence> destination,
        string recordKind,
        IReadOnlyList<JsonNode> records)
    {
        foreach (JsonNode record in records)
            if (JsonAccess.MetaId(record) is null)
                destination.Add(new ClassifiedEvidence(null, recordKind, EvidenceVisibilityStatus.Hidden));
    }

    private async Task<AnalysisPackage> GetSourcePackageSnapshotAsync(
        Scenario scenario,
        CancellationToken cancellationToken)
    {
        ScenarioPackageSnapshot? snapshot =
            await _store.FindScenarioPackageSnapshotAsync(scenario, cancellationToken);
        if (snapshot is not null)
            return snapshot.Package;

        AnalysisPackage sourcePackage =
            await _packages.BuildPackageAsync(scenario.SourceFieldId, cancellationToken);
        if (sourcePackage.FieldId != scenario.SourceFieldId)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Field package mismatch",
                $"The field package identified field {sourcePackage.FieldId:D}, not requested field {scenario.SourceFieldId:D}.");
        }

        PredictionRecord? prediction = await _store.FindPredictionAsync(scenario.ScenarioId, cancellationToken);
        if (prediction?.Seal is null ||
            !string.Equals(prediction.Body.FieldPackageSha256, sourcePackage.Sha256, StringComparison.OrdinalIgnoreCase) ||
            prediction.Baselines.Any(baseline =>
                !string.Equals(
                    baseline.PackageSha256,
                    prediction.Body.FieldPackageSha256,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Legacy scenario snapshot unavailable",
                $"Scenario {scenario.ScenarioId:D} has no trustworthy source snapshot. Recreate or explicitly migrate the scenario.");
        }

        ScenarioPackageSnapshot backfilled = await _store.BackfillScenarioPackageSnapshotAsync(
            scenario,
            sourcePackage,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
        return backfilled.Package;
    }

    internal static AnalysisPackage FilterPackage(
        AnalysisPackage package,
        Scenario scenario,
        DateTimeOffset effectiveTime,
        IReadOnlyList<EvidenceVisibility> visibility,
        IPackageHasher hasher)
    {
        HashSet<string> visibleIds = visibility
            .Where(item => IsVisible(item, effectiveTime))
            .Select(item => item.EvidenceId)
            .ToHashSet(StringComparer.Ordinal);
        string fieldEvidenceId = EvidenceCatalog.CreateId(EvidenceCatalog.Field, package.FieldId);
        if (!visibleIds.Contains(fieldEvidenceId))
        {
            throw new ScenarioApiException(
                StatusCodes.Status404NotFound,
                "Field evidence is not visible",
                $"Field {package.FieldId:D} is not visible in scenario {scenario.ScenarioId:D} at {effectiveTime:O}.");
        }

        JsonNode[] clusters = Filter(EvidenceCatalog.Cluster, package.Clusters, visibleIds);
        JsonNode[] wells = Filter(EvidenceCatalog.Well, package.Wells, visibleIds);
        JsonNode[] wellBores = Filter(EvidenceCatalog.WellBore, package.WellBores, visibleIds);
        JsonNode[] architectures = Filter(EvidenceCatalog.Architecture, package.WellBoreArchitectures, visibleIds);
        JsonNode[] trajectories = Filter(EvidenceCatalog.Trajectory, package.Trajectories, visibleIds);
        JsonNode[] geology = Filter(EvidenceCatalog.Geology, package.GeologicalProperties, visibleIds);
        var counts = new SourceCounts(
            1,
            clusters.Length,
            wells.Length,
            wellBores.Length,
            architectures.Length,
            trajectories.Length,
            geology.Length);
        string[] gaps = RecomputeDataGaps(clusters, wells, wellBores, architectures, trajectories, geology);
        string hash = hasher.Compute(
            package.FieldId,
            package.Field,
            clusters,
            wells,
            wellBores,
            architectures,
            trajectories,
            geology,
            counts,
            gaps);

        return package with
        {
            GeneratedAt = effectiveTime,
            Clusters = clusters,
            Wells = wells,
            WellBores = wellBores,
            WellBoreArchitectures = architectures,
            Trajectories = trajectories,
            GeologicalProperties = geology,
            SourceCounts = counts,
            DataGaps = gaps,
            Sha256 = hash
        };
    }

    private static JsonNode[] Filter(
        string kind,
        IReadOnlyList<JsonNode> items,
        IReadOnlySet<string> visibleIds) =>
        items.Where(item => EvidenceCatalog.TryCreateId(kind, item) is string evidenceId && visibleIds.Contains(evidenceId))
            .ToArray();

    private static bool IsVisible(EvidenceVisibility item, DateTimeOffset effectiveTime) =>
        item.VisibleFromUtc <= effectiveTime &&
        (item.VisibleUntilUtc is null || effectiveTime < item.VisibleUntilUtc.Value);

    private static void EnsureFieldMatches(Scenario scenario, Guid fieldId)
    {
        if (fieldId == scenario.SourceFieldId || fieldId == scenario.ClonedFieldId)
            return;

        throw new ScenarioApiException(
            StatusCodes.Status409Conflict,
            "Scenario field mismatch",
            $"Field {fieldId:D} is not the source or cloned field for scenario {scenario.ScenarioId:D}.");
    }

    private static DateTimeOffset GetEffectiveTime(Scenario scenario, DateTimeOffset? asOf)
    {
        DateTimeOffset effectiveTime = (asOf ?? scenario.AsOfUtc).ToUniversalTime();
        if (effectiveTime < scenario.InitialAsOfUtc)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Invalid scenario time",
                $"As-of time {effectiveTime:O} predates initial scenario time {scenario.InitialAsOfUtc:O}.");
        }
        if (effectiveTime > scenario.AsOfUtc)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Invalid scenario time",
                $"As-of time {effectiveTime:O} is later than current scenario clock {scenario.AsOfUtc:O}.");
        }
        return effectiveTime;
    }

    private static NormalizedScenarioRequest Normalize(CreateScenarioRequest request)
    {
        if (request.SourceFieldId == Guid.Empty)
            throw Validation("sourceFieldId", "SourceFieldId must be a non-empty GUID.");
        if (request.AsOfUtc == default || request.AsOfUtc == DateTimeOffset.MinValue || request.AsOfUtc == DateTimeOffset.MaxValue)
            throw Validation("asOfUtc", "AsOfUtc must be a valid timestamp.");

        string reservoirName = NormalizeLabel(request.ReservoirName, "reservoirName");
        string seedLabel = NormalizeLabel(request.SeedLabel, "seedLabel");
        string assumptionsSha256 = request.AssumptionsSha256?.Trim().ToLowerInvariant() ?? string.Empty;
        if (assumptionsSha256.Length != 64 || assumptionsSha256.Any(character => !Uri.IsHexDigit(character)))
            throw Validation("assumptionsSha256", "AssumptionsSha256 must contain exactly 64 hexadecimal characters.");

        return new NormalizedScenarioRequest(
            request.SourceFieldId,
            reservoirName,
            request.AsOfUtc.ToUniversalTime(),
            seedLabel,
            assumptionsSha256);
    }

    private static string NormalizeLabel(string? value, string name)
    {
        string normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            throw Validation(name, $"{name} is required.");
        if (normalized.Length > MaximumLabelLength)
            throw Validation(name, $"{name} cannot exceed {MaximumLabelLength} characters.");
        return normalized;
    }

    private static ScenarioApiException Validation(string name, string message) =>
        new(StatusCodes.Status400BadRequest, "Invalid scenario request", $"{name}: {message}");

    private static ScenarioApiException ScorecardUnavailable() =>
        new(
            StatusCodes.Status404NotFound,
            "Scorecard not available",
            "No public scorecard was available at the bound scenario snapshot.");

    private static string[] RecomputeDataGaps(
        IReadOnlyList<JsonNode> clusters,
        IReadOnlyList<JsonNode> wells,
        IReadOnlyList<JsonNode> wellBores,
        IReadOnlyList<JsonNode> architectures,
        IReadOnlyList<JsonNode> trajectories,
        IReadOnlyList<JsonNode> geology)
    {
        var gaps = new List<string>();
        if (trajectories.Count == 0)
            gaps.Add("No trajectories were returned for the field.");
        if (clusters.Count == 0)
            gaps.Add("No clusters were returned for the field.");
        if (clusters.Count > 0 && wells.Count == 0)
            gaps.Add("No wells were returned for the field clusters.");
        if (wells.Count > 0 && wellBores.Count == 0)
            gaps.Add("No wellbores were returned for the field wells.");

        HashSet<Guid> wellBoreIds = wellBores.Select(JsonAccess.MetaId).OfType<Guid>().ToHashSet();
        if (wellBores.Count > 0 && architectures.Count == 0)
            gaps.Add("No wellbore architecture matched the included wellbores.");
        int withoutArchitecture = wellBoreIds.Count - architectures
            .Select(item => JsonAccess.Guid(item, "WellBoreID"))
            .OfType<Guid>()
            .Where(wellBoreIds.Contains)
            .Distinct()
            .Count();
        if (withoutArchitecture > 0)
            gaps.Add($"{withoutArchitecture} included wellbore(s) have no architecture record.");
        if (wellBores.Count > 0 && geology.Count == 0)
            gaps.Add("No geological properties matched the included wellbores.");
        HashSet<Guid> geologyWellBoreIds = geology
            .Select(item => JsonAccess.Guid(item, "WellBoreID"))
            .OfType<Guid>()
            .ToHashSet();
        int withoutGeology = wellBoreIds.Count(id => !geologyWellBoreIds.Contains(id));
        if (withoutGeology > 0)
            gaps.Add($"{withoutGeology} included wellbore(s) had no geological-properties record.");
        HashSet<Guid> trajectoryWellBoreIds = trajectories
            .Select(item => JsonAccess.Guid(item, "WellBoreID"))
            .OfType<Guid>()
            .ToHashSet();
        int withoutTrajectory = wellBoreIds.Count(id => !trajectoryWellBoreIds.Contains(id));
        if (withoutTrajectory > 0)
            gaps.Add($"{withoutTrajectory} included wellbore(s) have no survey trajectory.");

        return gaps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    private sealed record ClassifiedEvidence(
        string? EvidenceId,
        string RecordKind,
        EvidenceVisibilityStatus Status);

    private sealed record NormalizedScenarioRequest(
        Guid SourceFieldId,
        string ReservoirName,
        DateTimeOffset AsOfUtc,
        string SeedLabel,
        string AssumptionsSha256);
}
