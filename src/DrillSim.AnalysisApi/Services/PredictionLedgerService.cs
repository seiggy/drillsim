using System.Globalization;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

public sealed class PredictionLedgerService
{
    private readonly SqliteScenarioStore _store;
    private readonly ScenarioService _scenarios;
    private readonly IPetrophysicsAnalysisService _analysis;
    private readonly TimeProvider _timeProvider;

    public PredictionLedgerService(
        SqliteScenarioStore store,
        ScenarioService scenarios,
        IPetrophysicsAnalysisService analysis,
        TimeProvider timeProvider)
    {
        _store = store;
        _scenarios = scenarios;
        _analysis = analysis;
        _timeProvider = timeProvider;
    }

    public async Task<PredictionRecord> GetAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default)
    {
        _ = await _scenarios.GetAsync(scenarioId, cancellationToken);
        return await _store.FindPredictionAsync(scenarioId, cancellationToken)
            ?? throw new ScenarioApiException(
                StatusCodes.Status404NotFound,
                "Prediction not found",
                $"Scenario {scenarioId:D} has no prediction draft.");
    }

    public async Task<PredictionRecord> PutDraftAsync(
        Guid scenarioId,
        PredictionBody body,
        int? expectedRevision = null,
        CancellationToken cancellationToken = default)
    {
        Scenario scenario = await _scenarios.GetAsync(scenarioId, cancellationToken);
        PredictionValidator.Validate(body);
        if (body.AnalysisBinding is PredictionAnalysisBinding binding)
        {
            AnalysisPackage package = await _scenarios.GetPackageAsync(
                scenario.SourceFieldId, scenarioId, scenario.InitialAsOfUtc, cancellationToken);
            _ = ConfiguredPredictionBinding.Validate(scenario, body, package, _analysis);
        }
        string bodyJson = PredictionJson.Canonicalize(body);
        return await _store.SavePredictionDraftAsync(
            scenarioId,
            bodyJson,
            expectedRevision,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
    }

    public async Task<PredictionRecord> SealAsync(
        Guid scenarioId,
        CancellationToken cancellationToken = default,
        int? expectedRevision = null)
    {
        Scenario scenario = await _scenarios.GetAsync(scenarioId, cancellationToken);
        PredictionRecord prediction = await GetAsync(scenarioId, cancellationToken);
        if (expectedRevision is not null && expectedRevision != prediction.Revision)
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Prediction revision conflict",
                "The reviewed draft revision has changed. Reload and review the current draft before sealing.");
        if (prediction.Seal is not null)
            return prediction;

        PredictionValidator.Validate(prediction.Body);
        PredictionValidator.EnsureSealable(prediction.Body);
        AnalysisPackage package = await _scenarios.GetPackageAsync(
            scenario.SourceFieldId,
            scenarioId,
            prediction.Body.AnalysisBinding is null ? scenario.AsOfUtc : scenario.InitialAsOfUtc,
            cancellationToken);
        if (!string.Equals(prediction.Body.FieldPackageSha256, package.Sha256, StringComparison.Ordinal))
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Field package hash mismatch",
                "The prediction field package SHA-256 does not match the scenario's current as-of package.");
        }

        HashSet<string> visibleEvidenceIds = EvidenceCatalog.Enumerate(package)
            .Select(item => item.EvidenceId)
            .ToHashSet(StringComparer.Ordinal);
        int hiddenCitationCount = prediction.Body.CitedEvidenceIds.Count(id => !visibleEvidenceIds.Contains(id));
        if (hiddenCitationCount > 0)
        {
            throw new ScenarioApiException(
                StatusCodes.Status409Conflict,
                "Prediction evidence mismatch",
                $"The prediction cites {hiddenCitationCount} evidence ID(s) that are not visible at the current scenario clock.");
        }

        AnalysisResult analysis = prediction.Body.AnalysisBinding is null
            ? _analysis.Analyze(package, scenario.ReservoirName)
            : ConfiguredPredictionBinding.Validate(scenario, prediction.Body, package, _analysis);
        AnalysisResult? fourNeighbors = prediction.Body.AnalysisBinding is null ? null :
            _analysis.Analyze(package, scenario.ReservoirName, analysis.Configuration with { IdwNeighborCount = 4 });
        IReadOnlyList<BaselineSnapshot> baselines = BaselineFactory.Create(scenarioId, prediction.Body, analysis, fourNeighbors);
        string bodyJson = PredictionJson.Canonicalize(prediction.Body);
        string sealedSha256 = PredictionJson.ComputeSha256(prediction.Body);
        return await _store.SealPredictionAsync(
            scenarioId,
            bodyJson,
            sealedSha256,
            baselines,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken,
            expectedRevision);
    }

    public async Task<PredictionRecord> ApproveAsync(
        Guid scenarioId,
        string? actor,
        CancellationToken cancellationToken = default)
    {
        string normalizedActor = PredictionValidator.ValidateActor(actor);
        _ = await _scenarios.GetAsync(scenarioId, cancellationToken);
        return await _store.ApprovePredictionAsync(
            scenarioId,
            normalizedActor,
            _timeProvider.GetUtcNow().ToUniversalTime(),
            cancellationToken);
    }

    public static int? ParseIfMatchRevision(string? value)
    {
        if (value is null)
            return null;
        if (value.Length < 3 || value[0] != '"' || value[^1] != '"' ||
            !int.TryParse(value.AsSpan(1, value.Length - 2), NumberStyles.None, CultureInfo.InvariantCulture, out int revision) ||
            revision < 1 || !string.Equals(value, FormatRevisionEtag(revision), StringComparison.Ordinal))
        {
            throw new ScenarioApiException(
                StatusCodes.Status400BadRequest,
                "Invalid If-Match header",
                "If-Match must be an exact quoted positive integer revision, for example \"1\".");
        }
        return revision;
    }

    public static string FormatRevisionEtag(int revision) => $"\"{revision.ToString(CultureInfo.InvariantCulture)}\"";

    public static string ComputeCanonicalSha256(PredictionBody body)
    {
        PredictionValidator.Validate(body);
        return PredictionJson.ComputeSha256(body);
    }
}
