using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Infrastructure;

internal sealed record ValidatedReveal(
    Guid ScenarioId,
    Guid RevealId,
    Guid RunId,
    Guid ClonedFieldId,
    DateTimeOffset ValidTimeUtc,
    string ObservationModelVersion,
    string ManifestSha256,
    IReadOnlyList<ValidatedRevealEvidence> Evidence,
    ValidatedProductionSeries ProductionSeries);

internal sealed record ValidatedRevealEvidence(
    string EvidenceId,
    string RecordKind,
    string ContentSha256);

internal sealed record ValidatedProductionSeries(
    Guid SeriesId,
    string ModelVersion,
    string ContentSha256,
    int MonthCount,
    IReadOnlyList<int> CheckpointYears);

public sealed class RevealService
{
    private const int MaximumEvidenceCount = 5_000;
    private static readonly int[] RequiredCheckpointYears = [1, 3, 5];
    private readonly SqliteScenarioStore _store;
    private readonly TimeProvider _timeProvider;

    public RevealService(SqliteScenarioStore store, TimeProvider timeProvider)
    {
        _store = store;
        _timeProvider = timeProvider;
    }

    public async Task<RevealReceipt> PrepareAsync(
        Guid scenarioId,
        RevealRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatedReveal reveal = Validate(scenarioId, request);
        AnalysisPackage clonePackage = RevealPackageValidator.Validate(reveal, request.ClonePackage);
        string canonicalBody = PredictionJson.Canonicalize(reveal);
        string bodySha256 = PredictionJson.ComputeSha256(reveal);
        return await _store.PrepareRevealAsync(
            reveal,
            clonePackage,
            canonicalBody,
            bodySha256,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
    }

    public async Task<RevealReceipt> BackfillCloneSnapshotAsync(
        Guid scenarioId,
        RevealRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatedReveal reveal = Validate(scenarioId, request);
        AnalysisPackage clonePackage = RevealPackageValidator.Validate(reveal, request.ClonePackage);
        return await _store.BackfillClonePackageSnapshotAsync(
            reveal,
            clonePackage,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
    }

    public async Task<RevealReceipt> FinalizeAsync(
        Guid scenarioId,
        FinalizeRevealRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guid revealId = CanonicalGuid(request.RevealId, "revealId");
        string manifestSha256 = Sha256(request.ManifestSha256, "manifestSha256");
        return await _store.FinalizeRevealAsync(
            scenarioId,
            revealId,
            manifestSha256,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
    }

    public async Task<RevealReceipt> GetRevealAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default) =>
        await _store.FindRevealAsync(scenarioId, cancellationToken)
        ?? throw NotFound("Reveal not found", $"Scenario {scenarioId:D} has no public reveal receipt.");

    public async Task<RevealReceipt> GetInternalStatusAsync(
        Guid scenarioId,
        Guid revealId,
        CancellationToken cancellationToken = default) =>
        await _store.FindRevealStatusAsync(scenarioId, revealId, cancellationToken)
        ?? throw NotFound("Reveal not found", $"Reveal {revealId:D} is not prepared for scenario {scenarioId:D}.");

    public async Task<PublicProductionSeriesMetadata> GetProductionAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default) =>
        await _store.FindProductionSeriesAsync(scenarioId, cancellationToken)
        ?? throw NotFound("Production series not found", $"Scenario {scenarioId:D} has no public production-series metadata.");

    internal static ValidatedReveal Validate(Guid scenarioId, RevealRequest request)
    {
        if (scenarioId == Guid.Empty)
            throw Invalid("scenarioId", "must be a non-empty GUID.");
        Guid revealId = CanonicalGuid(request.RevealId, "revealId");
        Guid runId = CanonicalGuid(request.RunId, "runId");
        Guid clonedFieldId = CanonicalGuid(request.ClonedFieldId, "clonedFieldId");
        if (request.ValidTimeUtc == default || request.ValidTimeUtc == DateTimeOffset.MinValue ||
            request.ValidTimeUtc == DateTimeOffset.MaxValue)
            throw Invalid("validTimeUtc", "must be a valid timestamp.");
        string observationModelVersion = Token(request.ObservationModelVersion, "observationModelVersion", 128);
        string manifestSha256 = Sha256(request.ManifestSha256, "manifestSha256");
        if (request.Evidence is null || request.Evidence.Count is < 1 or > MaximumEvidenceCount)
            throw Invalid("evidence", $"must contain between 1 and {MaximumEvidenceCount} records.");

        var evidenceIds = new HashSet<string>(StringComparer.Ordinal);
        var evidence = new List<ValidatedRevealEvidence>(request.Evidence.Count);
        for (int index = 0; index < request.Evidence.Count; index++)
        {
            RevealEvidenceRequest? item = request.Evidence[index];
            if (item is null)
                throw Invalid($"evidence[{index}]", "is required.");
            string publicationKind = Token(item.RecordKind, $"evidence[{index}].recordKind", 64);
            if (!EvidenceCatalog.TryMapPublicationKind(publicationKind, out string recordKind))
                throw Invalid($"evidence[{index}].recordKind", "is not a supported public record kind.");
            Guid evidenceEntityId = CanonicalGuid(item.EvidenceId, $"evidence[{index}].evidenceId");
            string evidenceId = EvidenceCatalog.CreateId(recordKind, evidenceEntityId);
            if (!evidenceIds.Add(evidenceId))
                throw Invalid("evidence", $"contains duplicate evidence ID '{item.EvidenceId}'.");
            evidence.Add(new ValidatedRevealEvidence(
                evidenceId,
                recordKind,
                Sha256(item.ContentSha256, $"evidence[{index}].contentSha256")));
        }
        string clonedFieldEvidenceId = EvidenceCatalog.CreateId(EvidenceCatalog.Field, clonedFieldId);
        if (!evidenceIds.Contains(clonedFieldEvidenceId))
            throw Invalid("evidence", "must include the cloned field evidence ID.");

        ProductionSeriesRequest production = request.ProductionSeries
            ?? throw Invalid("productionSeries", "is required.");
        Guid seriesId = CanonicalGuid(production.SeriesId, "productionSeries.seriesId");
        string modelVersion = Token(production.ModelVersion, "productionSeries.modelVersion", 128);
        string productionHash = Sha256(production.ContentSha256, "productionSeries.contentSha256");
        if (production.MonthCount != 60)
            throw Invalid("productionSeries.monthCount", "must equal 60.");
        if (production.CheckpointYears is null ||
            !production.CheckpointYears.SequenceEqual(RequiredCheckpointYears))
        {
            throw Invalid("productionSeries.checkpointYears", "must be exactly [1, 3, 5].");
        }

        return new ValidatedReveal(
            scenarioId,
            revealId,
            runId,
            clonedFieldId,
            request.ValidTimeUtc.ToUniversalTime(),
            observationModelVersion,
            manifestSha256,
            evidence,
            new ValidatedProductionSeries(
                seriesId,
                modelVersion,
                productionHash,
                production.MonthCount,
                RequiredCheckpointYears));
    }

    private static Guid CanonicalGuid(string? value, string field)
    {
        if (value is null || value.Length != 36 || !Guid.TryParseExact(value, "D", out Guid parsed) ||
            parsed == Guid.Empty || !string.Equals(value, parsed.ToString("D"), StringComparison.Ordinal))
        {
            throw Invalid(field, "must be a canonical lowercase non-empty GUID.");
        }
        return parsed;
    }

    private static string Sha256(string? value, string field)
    {
        if (value is null || value.Length != 64 || value.Any(character =>
            character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')))
        {
            throw Invalid(field, "must be exactly 64 lowercase hexadecimal characters.");
        }
        return value;
    }

    private static string Token(string? value, string field, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength ||
            value.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_' or '.' or ':')))
        {
            throw Invalid(field, $"must be a nonblank token of at most {maximumLength} characters.");
        }
        return value;
    }

    private static ScenarioApiException Invalid(string field, string detail) =>
        new(StatusCodes.Status400BadRequest, "Invalid reveal manifest", $"{field}: {detail}");

    private static ScenarioApiException NotFound(string title, string detail) =>
        new(StatusCodes.Status404NotFound, title, detail);
}
