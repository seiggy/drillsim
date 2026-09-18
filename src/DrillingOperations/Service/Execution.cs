using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace DrillingOperations;

public sealed class DrillingExecutionOptions
{
    public const string SectionName = "DrillingExecution";
    public double MaxLateralDeviationM { get; init; } = 6;
    public double MaxTvdDeviationM { get; init; } = 2;
    public double NominalRopMPerHour { get; init; } = 30;
    public double MaxDoglegDegreesPer30M { get; init; } = 4;
    public double OutputStationSpacingM { get; init; } = 30;
    public string HardwareCalibrationVersion { get; init; } = "kinematic-hardware-default-v1";

    public void Validate()
    {
        PositiveBounded(MaxLateralDeviationM, 100, nameof(MaxLateralDeviationM));
        PositiveBounded(MaxTvdDeviationM, 50, nameof(MaxTvdDeviationM));
        PositiveBounded(NominalRopMPerHour, 500, nameof(NominalRopMPerHour));
        PositiveBounded(MaxDoglegDegreesPer30M, 30, nameof(MaxDoglegDegreesPer30M));
        if (!double.IsFinite(OutputStationSpacingM) || OutputStationSpacingM < 1 || OutputStationSpacingM > 500)
            throw new OptionsValidationException(nameof(OutputStationSpacingM), typeof(DrillingExecutionOptions), ["OutputStationSpacingM must be finite and between 1 and 500 metres."]);
        if (string.IsNullOrWhiteSpace(HardwareCalibrationVersion) || HardwareCalibrationVersion.Length > 128 ||
            HardwareCalibrationVersion.Any(static c => !(char.IsAsciiLetterOrDigit(c) || "-_.:".Contains(c))))
            throw new OptionsValidationException(nameof(HardwareCalibrationVersion), typeof(DrillingExecutionOptions), ["Hardware calibration version is invalid."]);
    }
    public string Hash()
    {
        Validate();
        return DeterministicIdentity.Sha256(CanonicalJson.Serialize(new
        {
            MaxLateralDeviationM, MaxTvdDeviationM, NominalRopMPerHour,
            MaxDoglegDegreesPer30M, OutputStationSpacingM, HardwareCalibrationVersion
        }));
    }
    private static void PositiveBounded(double value, double maximum, string name)
    {
        if (!double.IsFinite(value) || value <= 0 || value > maximum)
            throw new OptionsValidationException(name, typeof(DrillingExecutionOptions), [$"{name} must be finite, positive, and at most {maximum}."]);
    }
}

public sealed record MaterializedPlan(
    string PlanId, string RunId, string ScenarioId, string BindingId,
    string ScenarioWellId, string ScenarioWellBoreId, string PlannedTrajectoryId,
    string CandidateId, IReadOnlyList<PlanPathStation> Stations, string PathHash,
    string SourcePredictionSealSha256, int SourcePredictionRevision, string SourcePackageSha256,
    string ArtifactHash, DateTimeOffset CreatedUtc) { public string TrajectoryRole => TrajectoryMetadata.Planned; public string VerticalDirection => TrajectoryMetadata.VerticalDirection; public string ReferenceFrame => TrajectoryMetadata.ReferenceFrame; }
public sealed record PlanPathStation(double MeasuredDepthM, double TrueVerticalDepthM, double EastingM, double NorthingM);
public sealed record DrilledPathStation(double MeasuredDepthM, double TrueVerticalDepthM, double EastingM, double NorthingM);
public sealed record DrillingTimelinePoint(int StationIndex, double SimulatedElapsedSeconds);
public sealed record DrillingExecutionArtifact(
    string ExecutionId, string RunId, string PlanId, string ModelVersion,
    string OptionsHash, string HardwareCalibrationVersion,
    IReadOnlyList<DrilledPathStation> Stations, IReadOnlyList<DrillingTimelinePoint> Timeline,
    double StartSimulatedSeconds, double EndSimulatedSeconds, string InputHash, string OutputHash,
    DateTimeOffset CreatedUtc) { public string TrajectoryRole => TrajectoryMetadata.AsDrilledTruth; public string VerticalDirection => TrajectoryMetadata.VerticalDirection; public string ReferenceFrame => TrajectoryMetadata.ReferenceFrame; }

public sealed class RunStageFailureException(string diagnosticCode, string message) : Exception(message)
{
    public string DiagnosticCode { get; } = diagnosticCode;
}

public sealed class BuiltInDependencyCapabilityProbe : IDependencyCapabilityProbe
{
    public bool IsAvailable(RunStageKind stage) => stage is RunStageKind.S1MaterializePlan or RunStageKind.S2ExecuteDrilling or RunStageKind.S3GenerateSurvey or RunStageKind.S4SampleGeology or RunStageKind.S5GenerateLogs or RunStageKind.S6DesignCompletion or RunStageKind.S7RunProduction;
}

public sealed class RunStageExecutor(
    DrillingOperationsStore store,
    AnalysisVerificationClient analysis,
    IOptions<DrillingExecutionOptions> options,
    IOptions<SurveyObservationOptions> surveyOptions,
    IOptions<TruthSamplingOptions> truthOptions,
    IOptions<LogObservationOptions> logOptions,
    IOptions<DrillingObservationOptions> drillingObservationOptions,
    IOptions<CompletionDesignOptions> completionOptions,
    ReservoirCompletionClient completionClient,
    IOptions<ProductionExecutionOptions> productionOptions,
    IOptions<ProductionMeterOptions> meterOptions,
    ReservoirProductionClient productionClient,
    ReservoirSamplingClient samplingClient) : IRunCheckpointExecutor
{
    public async Task<bool> AdvanceOneAsync(string runId, CancellationToken cancellationToken)
    {
        RunResponse? run = await store.GetRunAsync(runId, cancellationToken);
        if (run is null || RunStateMachine.IsTerminal(run.Status) || run.Status is RunStatus.AwaitingDependency or RunStatus.AwaitingApproval or RunStatus.Blocked or RunStatus.ReadyToReveal or RunStatus.PublishFailed or RunStatus.Revealed) return false;
        IReadOnlyList<StageResponse> stages = await store.GetStagesAsync(runId, cancellationToken);
        RunStageKind? next = stages.Where(static stage => stage.Status is not StageStatus.Completed)
            .OrderBy(static stage => stage.Stage).Select(static stage => (RunStageKind?)stage.Stage).FirstOrDefault();
        if (next is null) return false;
        if (next is RunStageKind.S0BindWorld) return await store.CompleteS0Async(runId, cancellationToken);
        if (next is RunStageKind.S1MaterializePlan)
        {
            if (!RequestValidation.IsCanonicalGuid(run.ScenarioId, out Guid scenarioId)) throw new RunStageFailureException("StoredScenarioInvalid", "Stored scenario identity is invalid.");
            try
            {
                (AnalysisScenarioDto scenario, AnalysisPredictionDto prediction) = await analysis.GetAsync(scenarioId, cancellationToken);
                return await store.MaterializePlanAsync(runId, scenario, prediction, cancellationToken);
            }
            catch (BindingVerificationException exception) when (exception.StatusCode is 502 or 503)
            {
                return await store.SetAwaitingDependencyAsync(runId, RunStageKind.S1MaterializePlan, "AnalysisVerificationUnavailable", cancellationToken);
            }
            catch (BindingVerificationException exception)
            {
                throw new RunStageFailureException("AuthoritativePredictionChanged", exception.Message);
            }
        }
        if (next is RunStageKind.S2ExecuteDrilling)
        {
            TruthBindingResponse binding = await store.GetBindingAsync(run.ScenarioId, cancellationToken)
                ?? throw new RunStageFailureException("BindingMissing", "Binding missing.");
            MaterializedPlan plan = await store.GetMaterializedPlanAsync(runId, cancellationToken)
                ?? throw new RunStageFailureException("MaterializedPlanMissing", "Plan missing.");
            try { await samplingClient.BindPlannedPathAsync(binding, plan, cancellationToken); }
            catch (StageASamplingException exception) when (exception.StatusCode is 502 or 503)
            { return await store.SetAwaitingDependencyAsync(runId, next.Value, exception.DiagnosticCode, cancellationToken); }
            catch (StageASamplingException exception)
            { throw new RunStageFailureException(exception.DiagnosticCode, exception.Message); }
            return await store.ExecuteDeterministicDrillingAsync(runId, options.Value, cancellationToken);
        }
        if (next is RunStageKind.S3GenerateSurvey)
            return await store.GenerateSurveyAsync(runId, surveyOptions.Value, cancellationToken);
        if (next is RunStageKind.S4SampleGeology)
        {
            TruthBindingResponse binding = await store.GetBindingAsync(run.ScenarioId, cancellationToken) ?? throw new RunStageFailureException("BindingMissing", "Binding missing.");
            DrillingExecutionArtifact execution = await store.GetDrillingExecutionAsync(runId, cancellationToken) ?? throw new RunStageFailureException("DrillingExecutionMissing", "Execution missing.");
            try { return await store.PersistTruthSamplesAsync(runId, await samplingClient.SampleAsync(binding, execution, truthOptions.Value, cancellationToken), cancellationToken); }
            catch (StageASamplingException exception) when (exception.StatusCode is 502 or 503)
            { return await store.SetAwaitingDependencyAsync(runId, next.Value, exception.DiagnosticCode, cancellationToken); }
            catch (StageASamplingException exception) { throw new RunStageFailureException(exception.DiagnosticCode, exception.Message); }
        }
        if (next is RunStageKind.S5GenerateLogs)
            return await store.GenerateLogsAsync(runId, logOptions.Value, drillingObservationOptions.Value, cancellationToken);
        if (next is RunStageKind.S6DesignCompletion)
        {
            try
            {
                if (!RequestValidation.IsCanonicalGuid(run.ScenarioId, out Guid scenarioId)) throw new RunStageFailureException("StoredScenarioInvalid", "Scenario ID invalid.");
                (AnalysisScenarioDto scenario, _) = await analysis.GetAsync(scenarioId, cancellationToken);
                if (scenario.Status != "HumanApproved") throw new RunStageFailureException("AuthoritativeScenarioChanged", "Scenario is no longer approved.");
                return await store.GenerateCompletionDesignAsync(runId, scenario.ReservoirName, completionOptions.Value, completionClient, cancellationToken);
            }
            catch (BindingVerificationException e) when (e.StatusCode is 502 or 503) { return await store.SetAwaitingDependencyAsync(runId, next.Value, "AnalysisVerificationUnavailable", cancellationToken); }
            catch (CompletionUpstreamException e) when (e.StatusCode is 502 or 503) { return await store.SetAwaitingDependencyAsync(runId, next.Value, e.DiagnosticCode, cancellationToken); }
            catch (CompletionUpstreamException e) { throw new RunStageFailureException(e.DiagnosticCode, e.Message); }
        }
        if (next is RunStageKind.S7RunProduction)
        {
            try { return await store.ExecuteProductionAsync(runId, productionOptions.Value, meterOptions.Value, productionClient, cancellationToken); }
            catch (ProductionUpstreamException e) when (e.StatusCode is 502 or 503) { return await store.SetAwaitingDependencyAsync(runId, next.Value, e.DiagnosticCode, cancellationToken); }
            catch (ProductionUpstreamException e) { throw new RunStageFailureException(e.DiagnosticCode, e.Message); }
        }
        if (next is RunStageKind.S8PublishReveal)
            return await store.SetAwaitingDependencyAsync(runId, next.Value, "PublicationAdapterUnavailable", cancellationToken);
        return false;
    }
}

public static class DeterministicDrillingModel
{
    public const string ModelVersion = "deterministic-kinematic-drilling-v1";
    public const int MaximumOutputStations = 10_000;

    public static IReadOnlyList<PlanPathStation> Densify(IReadOnlyList<PlanPathStation> planned, double spacingM)
    {
        if (planned.Count < 2) throw new RunStageFailureException("PlanPathInvalid", "At least two planned stations are required.");
        if (!double.IsFinite(spacingM) || spacingM < 1 || spacingM > 500)
            throw new RunStageFailureException("OutputStationSpacingInvalid", "Output station spacing must be between 1 and 500 metres.");
        var output = new List<PlanPathStation>(Math.Min(MaximumOutputStations, planned.Count * 2)) { planned[0] };
        for (int segment = 1; segment < planned.Count; segment++)
        {
            PlanPathStation from = planned[segment - 1], to = planned[segment];
            double measuredDepthInterval = to.MeasuredDepthM - from.MeasuredDepthM;
            if (!double.IsFinite(measuredDepthInterval) || measuredDepthInterval <= 0)
                throw new RunStageFailureException("PlanPathInvalid", "Planned measured depth must increase strictly.");
            double spatialLength = Math.Sqrt(Math.Pow(to.EastingM - from.EastingM, 2) + Math.Pow(to.NorthingM - from.NorthingM, 2) + Math.Pow(to.TrueVerticalDepthM - from.TrueVerticalDepthM, 2));
            if (!double.IsFinite(spatialLength) || spatialLength <= 1e-9)
                throw new RunStageFailureException("ZeroLengthSpatialSegment", "A positive measured-depth interval cannot have zero spatial length.");
            double requiredDivisions = Math.Ceiling(measuredDepthInterval / spacingM);
            if (!double.IsFinite(requiredDivisions) || requiredDivisions > MaximumOutputStations - output.Count)
                throw new RunStageFailureException("OutputStationLimitExceeded", $"Densified path exceeds {MaximumOutputStations} stations.");
            int divisions = (int)requiredDivisions;
            for (int division = 1; division <= divisions; division++)
            {
                double fraction = division / (double)divisions;
                output.Add(new PlanPathStation(
                    division == divisions ? to.MeasuredDepthM : from.MeasuredDepthM + measuredDepthInterval * fraction,
                    from.TrueVerticalDepthM + (to.TrueVerticalDepthM - from.TrueVerticalDepthM) * fraction,
                    from.EastingM + (to.EastingM - from.EastingM) * fraction,
                    from.NorthingM + (to.NorthingM - from.NorthingM) * fraction));
            }
        }
        return output;
    }

    public static (IReadOnlyList<DrilledPathStation> Stations, IReadOnlyList<DrillingTimelinePoint> Timeline, double DurationSeconds)
        Execute(string runId, IReadOnlyList<PlanPathStation> approvedPath, DrillingExecutionOptions options)
    {
        options.Validate();
        IReadOnlyList<PlanPathStation> planned = Densify(approvedPath, options.OutputStationSpacingM);
        byte[] seed = SHA256.HashData(Encoding.UTF8.GetBytes(runId + "\n" + options.Hash()));
        double phaseA = BitConverter.ToUInt32(seed, 0) / (double)uint.MaxValue * Math.Tau;
        double phaseB = BitConverter.ToUInt32(seed, 4) / (double)uint.MaxValue * Math.Tau;
        double lateralAmplitude = options.MaxLateralDeviationM * 0.70;
        double tvdAmplitude = options.MaxTvdDeviationM * 0.70;
        double totalMd = planned[^1].MeasuredDepthM - planned[0].MeasuredDepthM;
        List<DrilledPathStation>? result = null;
        for (int reduction = 0; reduction < 16; reduction++)
        {
            double scale = Math.Pow(0.5, reduction);
            result = Build(scale);
            if (MaximumDogleg(result) <= options.MaxDoglegDegreesPer30M + 1e-9) break;
            if (reduction == 15) throw new RunStageFailureException("DoglegBoundUnavailable", "The approved path cannot satisfy the configured dogleg bound.");
        }
        List<DrilledPathStation> final = result ?? throw new RunStageFailureException("DrillingExecutionFailed", "No drilling path was generated.");
        double duration = totalMd / options.NominalRopMPerHour * 3600;
        var timeline = final.Select((station, index) => new DrillingTimelinePoint(index,
            (station.MeasuredDepthM - final[0].MeasuredDepthM) / totalMd * duration)).ToArray();
        return (final, timeline, duration);

        List<DrilledPathStation> Build(double scale)
        {
            var output = new List<DrilledPathStation>(planned.Count)
            {
                new(planned[0].MeasuredDepthM, planned[0].TrueVerticalDepthM, planned[0].EastingM, planned[0].NorthingM)
            };
            for (int index = 1; index < planned.Count; index++)
            {
                PlanPathStation station = planned[index];
                double t = (station.MeasuredDepthM - planned[0].MeasuredDepthM) / totalMd;
                double envelope = Math.Sin(Math.PI * t / 2);
                double eastOffset = lateralAmplitude * scale * envelope * Math.Sin(Math.Tau * t + phaseA) / Math.Sqrt(2);
                double northOffset = lateralAmplitude * scale * envelope * Math.Sin(Math.Tau * t + phaseB) / Math.Sqrt(2);
                double tvdOffset = tvdAmplitude * scale * envelope * Math.Sin(Math.PI * t + phaseB);
                double minimumTvd = Math.Max(0, station.TrueVerticalDepthM - options.MaxTvdDeviationM);
                double maximumTvd = station.TrueVerticalDepthM + options.MaxTvdDeviationM;
                double tvd = Math.Clamp(station.TrueVerticalDepthM + tvdOffset, minimumTvd, maximumTvd);
                output.Add(new(station.MeasuredDepthM, tvd, station.EastingM + eastOffset, station.NorthingM + northOffset));
            }
            return output;
        }
    }

    public static double MaximumDogleg(IReadOnlyList<DrilledPathStation> stations)
    {
        double maximum = 0;
        for (int index = 2; index < stations.Count; index++)
        {
            (double X, double Y, double Z) first = Vector(stations[index - 2], stations[index - 1]);
            (double X, double Y, double Z) second = Vector(stations[index - 1], stations[index]);
            double firstLength = Length(first), secondLength = Length(second);
            if (!double.IsFinite(firstLength) || !double.IsFinite(secondLength) || firstLength <= 1e-9 || secondLength <= 1e-9)
                throw new RunStageFailureException("ZeroLengthSpatialSegment", "Dogleg calculation encountered a zero-length spatial segment.");
            double cosine = Math.Clamp((first.X * second.X + first.Y * second.Y + first.Z * second.Z) / (firstLength * secondLength), -1, 1);
            double angleDegrees = Math.Acos(cosine) * 180 / Math.PI;
            double averageMeasuredDepthInterval = ((stations[index - 1].MeasuredDepthM - stations[index - 2].MeasuredDepthM) +
                (stations[index].MeasuredDepthM - stations[index - 1].MeasuredDepthM)) / 2;
            if (!double.IsFinite(averageMeasuredDepthInterval) || averageMeasuredDepthInterval <= 0)
                throw new RunStageFailureException("PlanPathInvalid", "Dogleg calculation requires increasing measured depth.");
            maximum = Math.Max(maximum, angleDegrees * 30 / averageMeasuredDepthInterval);
        }
        return maximum;
    }
    private static (double X, double Y, double Z) Vector(DrilledPathStation a, DrilledPathStation b) => (b.EastingM - a.EastingM, b.NorthingM - a.NorthingM, b.TrueVerticalDepthM - a.TrueVerticalDepthM);
    private static double Length((double X, double Y, double Z) value) => Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
}









