using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrillingOperations;

public sealed class BindingVerificationException(int statusCode, string message, Exception? inner = null) : Exception(message, inner)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class AnalysisVerificationClient(HttpClient httpClient)
{
    public async Task<(AnalysisScenarioDto Scenario, AnalysisPredictionDto Prediction)> GetAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        AnalysisScenarioDto scenario = await GetScenarioAsync(scenarioId, cancellationToken);
        AnalysisPredictionDto prediction = await ReadAsync<AnalysisPredictionDto>($"api/scenarios/{scenarioId:D}/prediction", cancellationToken);
        try { ConfiguredPredictionIntegrity.ValidateApproved(prediction, scenario); }
        catch (ScoringException exception) { throw new BindingVerificationException(409, "Configured prediction verification failed.", exception); }
        return (scenario, prediction);
    }

    public Task<AnalysisScenarioDto> GetScenarioAsync(Guid scenarioId, CancellationToken cancellationToken) =>
        ReadAsync<AnalysisScenarioDto>($"api/scenarios/{scenarioId:D}", cancellationToken);

    public Task<AnalysisPredictionDto?> FindPredictionAsync(Guid scenarioId, CancellationToken cancellationToken) =>
        ReadOptionalAsync<AnalysisPredictionDto>($"api/scenarios/{scenarioId:D}/prediction", true, cancellationToken);

    private async Task<T> ReadAsync<T>(string path, CancellationToken cancellationToken) where T : class =>
        await ReadOptionalAsync<T>(path, false, cancellationToken)
            ?? throw new BindingVerificationException(502, "Analysis API returned an empty verification document.");

    private async Task<T?> ReadOptionalAsync<T>(string path, bool allowMissing, CancellationToken cancellationToken) where T : class
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(path, cancellationToken);
            if (allowMissing && response.StatusCode == HttpStatusCode.NotFound) return null;
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict)
                throw new BindingVerificationException(409, "Required authoritative scenario or prediction state does not exist.");
            if (!response.IsSuccessStatusCode)
                throw new BindingVerificationException((int)response.StatusCode >= 500 ? 502 : 409,
                    "Analysis API rejected authoritative binding verification.");
            return await response.Content.ReadFromJsonAsync<T>(CanonicalJson.SerializerOptions, cancellationToken)
                ?? throw new BindingVerificationException(502, "Analysis API returned an empty verification document.");
        }
        catch (BindingVerificationException) { throw; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new BindingVerificationException(503, "Analysis API verification timed out."); }
        catch (HttpRequestException exception)
        { throw new BindingVerificationException(503, "Analysis API is unavailable.", exception); }
        catch (JsonException exception)
        { throw new BindingVerificationException(502, "Analysis API returned malformed verification JSON.", exception); }
    }
}

public sealed class ReservoirVerificationClient(HttpClient httpClient)
{
    public async Task<ReservoirWorldDto> GetAsync(string worldId, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync($"reservoirsimulation/api/worlds/{Uri.EscapeDataString(worldId)}", cancellationToken);
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict)
                throw new BindingVerificationException(409, "The submitted reservoir world does not exist.");
            if (!response.IsSuccessStatusCode)
                throw new BindingVerificationException((int)response.StatusCode >= 500 ? 502 : 409,
                    "Reservoir Simulation rejected authoritative binding verification.");
            return await response.Content.ReadFromJsonAsync<ReservoirWorldDto>(CanonicalJson.SerializerOptions, cancellationToken)
                ?? throw new BindingVerificationException(502, "Reservoir Simulation returned an empty world summary.");
        }
        catch (BindingVerificationException) { throw; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw new BindingVerificationException(503, "Reservoir Simulation verification timed out."); }
        catch (HttpRequestException exception)
        { throw new BindingVerificationException(503, "Reservoir Simulation is unavailable.", exception); }
        catch (JsonException exception)
        { throw new BindingVerificationException(502, "Reservoir Simulation returned malformed verification JSON.", exception); }
    }
}

public sealed class BindingVerificationService(AnalysisVerificationClient analysis, ReservoirVerificationClient reservoir)
{
    public async Task VerifyAsync(BindWorldRequest request, CancellationToken cancellationToken)
    {
        if (!RequestValidation.IsCanonicalGuid(request.ScenarioId, out Guid scenarioId))
            throw new BindingVerificationException(400, "ScenarioId must be a canonical nonempty GUID.");

        (AnalysisScenarioDto scenario, AnalysisPredictionDto prediction) = await analysis.GetAsync(scenarioId, cancellationToken);
        if (scenario.ScenarioId != scenarioId || scenario.SourceFieldId == Guid.Empty ||
            !StringComparer.Ordinal.Equals(scenario.Status, "HumanApproved"))
            Mismatch("Scenario is not in authoritative HumanApproved state.");
        if (prediction.ScenarioId != scenarioId || prediction.Seal is null || prediction.Approval is null)
            Mismatch("Prediction seal and human approval are required.");
        if (!StringComparer.Ordinal.Equals(prediction.Seal!.Sha256, prediction.Approval!.SealedSha256) ||
            !StringComparer.Ordinal.Equals(request.ApprovedSealedPredictionHash, prediction.Seal.Sha256))
            Mismatch("Prediction seal and approval hashes do not match the submitted binding.");
        if (!StringComparer.Ordinal.Equals(request.SourcePackageSha256, prediction.Body?.FieldPackageSha256))
            Mismatch("Prediction source package hash does not match the submitted binding.");
        if (!StringComparer.Ordinal.Equals(request.WorldModelVersion, scenario.WorldModelVersion))
            Mismatch("Scenario world model version does not match the submitted binding.");

        ReservoirWorldDto world = await reservoir.GetAsync(request.WorldId, cancellationToken);
        if (!StringComparer.Ordinal.Equals(world.WorldId, request.WorldId)) Mismatch("Reservoir world identity mismatch.");
        if (world.FieldId != scenario.SourceFieldId) Mismatch("Reservoir world field does not match the scenario source field.");
        if (!StringComparer.Ordinal.Equals(world.ReservoirName, scenario.ReservoirName)) Mismatch("Reservoir world reservoir does not match the scenario.");
        if (!StringComparer.Ordinal.Equals(world.CalibrationArtifact?.Id, request.CalibrationArtifactId) ||
            !StringComparer.Ordinal.Equals(world.CalibrationArtifact?.Sha256, request.CalibrationArtifactSha256))
            Mismatch("Reservoir world calibration artifact does not match the submitted binding.");
        if (!StringComparer.Ordinal.Equals(world.ModelVersion, request.WorldModelVersion))
            Mismatch("Reservoir world model version does not match the scenario and binding.");
    }

    private static void Mismatch(string message) => throw new BindingVerificationException(409, message);
}

public sealed record AnalysisScenarioDto(Guid ScenarioId, Guid SourceFieldId, string ReservoirName, string WorldModelVersion, string Status, DateTimeOffset? InitialAsOfUtc = null, string? ObservationModelVersion = null);
public sealed record AnalysisPredictionDto(Guid ScenarioId, AnalysisPredictionBodyDto? Body, int Revision, AnalysisSealDto? Seal, AnalysisApprovalDto? Approval, IReadOnlyList<AnalysisBaselineDto>? Baselines = null);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AnalysisPredictionBodyDto(
    string CandidateId,
    IReadOnlyList<AnalysisPathStationDto>? ProposedWellPath,
    string FieldPackageSha256,
    IReadOnlyList<AnalysisFormationPredictionDto>? Formations = null,
    AnalysisQuantilesDto? ExpectedPaydirtM = null,
    IReadOnlyList<string>? FluidClasses = null,
    IReadOnlyList<AnalysisContactPredictionDto>? ContactPredictions = null,
    IReadOnlyList<AnalysisProductionForecastDto>? ProductionForecasts = null,
    IReadOnlyList<string>? UncertaintyAssumptions = null,
    IReadOnlyList<string>? CitedEvidenceIds = null,
    string? Rationale = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AnalysisPredictionBindingDto? AnalysisBinding = null);
public sealed record AnalysisPathStationDto(double MeasuredDepthM, double TrueVerticalDepthM, double EastingM, double NorthingM);
public sealed record AnalysisQuantilesDto(double P90,double P50,double P10);
public sealed record AnalysisFormationPredictionDto(string FormationName,AnalysisQuantilesDto TopTrueVerticalDepthM,AnalysisQuantilesDto BaseTrueVerticalDepthM);
public sealed record AnalysisContactPredictionDto(string ContactType,AnalysisQuantilesDto TrueVerticalDepthM);
public sealed record AnalysisProductionForecastDto(int Year,double OilM3,double GasM3,double WaterM3);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AnalysisBaselineDto(Guid BaselineId,string ContentSha256,Guid ScenarioId,string Kind,string ModelVersion,string PackageSha256,string CandidateId,double TargetEastingM,double TargetNorthingM,IReadOnlyList<string> ContributingEvidenceIds,AnalysisQuantilesDto? FormationTopTrueVerticalDepthM,AnalysisQuantilesDto? FormationBaseTrueVerticalDepthM,AnalysisQuantilesDto? ExpectedPaydirtM,IReadOnlyList<string>? FluidClasses,IReadOnlyList<AnalysisContactPredictionDto>? ContactPredictions,IReadOnlyList<AnalysisProductionForecastDto>? ProductionForecasts,string Limitation,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] AnalysisBaselineBindingDto? AnalysisBinding = null);
public sealed record AnalysisSealDto(string Sha256,string? BaselinesSha256=null,DateTimeOffset? SealedUtc=null);
public sealed record AnalysisApprovalDto(string SealedSha256,string? Actor=null,DateTimeOffset? ApprovedUtc=null);
public sealed record ReservoirWorldDto(string WorldId, Guid FieldId, string ReservoirName, string ModelVersion, CalibrationArtifactDto? CalibrationArtifact);
public sealed record CalibrationArtifactDto(string Id, string Sha256);
