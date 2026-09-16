using System.Globalization;
using System.Text.Json;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Persistence;

internal sealed record CompletionProductionRateEnvelope(
    double ConnectedPoreVolumeM3,
    double RecommendedMaximumRateM3PerSecond,
    double SelectedRateM3PerSecond,
    double TargetProducedPoreVolumeFraction);


internal sealed class CompletionProductionService(
    SqliteCompletionProductionRepository repository,
    SqliteReservoirRepository stateRepository,
    ApprovedCompletionBindingService completionBindings,
    PersistentSimulationStateStore stateStore,
    IReservoirSimulator simulator,
    TimeProvider timeProvider,
    Action<int, SimulationExecution>? diagnosticObserver = null)
{
    internal static CompletionProductionRateEnvelope CalculateRateEnvelope(
        ReservoirWorld world, ResolvedCompletionBinding completion,
        double initialTimeStepSeconds, double maximumSaturationChange,
        double activeProductionSeconds, double targetProducedPoreVolumeFraction)
    {
        if (!(initialTimeStepSeconds > 0) || !(maximumSaturationChange > 0) ||
            !(activeProductionSeconds > 0) || !(targetProducedPoreVolumeFraction > 0))
            throw new ArgumentOutOfRangeException(nameof(targetProducedPoreVolumeFraction));
        int[] cells = completion.Openings
            .SelectMany(opening => opening.Connections)
            .Select(connection => world.Grid.CellIndex(connection.I, connection.J, connection.K))
            .Distinct()
            .ToArray();
        double connectedPoreVolume = cells.Sum(cell => world.PoreVolumeM3[cell]);
        double cflMaximumRate = 0.5 * maximumSaturationChange * connectedPoreVolume /
            initialTimeStepSeconds;
        double targetRate = targetProducedPoreVolumeFraction * world.PoreVolumeM3.Sum() /
            activeProductionSeconds;
        double selectedRate = Math.Min(cflMaximumRate, targetRate);
        if (!(selectedRate > 0) || !double.IsFinite(selectedRate))
            throw new InvalidOperationException("Recommended completion production rate is invalid.");
        return new CompletionProductionRateEnvelope(
            connectedPoreVolume, cflMaximumRate, selectedRate, targetProducedPoreVolumeFraction);
    }


    internal const string ModelVersion = "completion-production-v1";
    internal const double YearSeconds = 365.25 * 24 * 60 * 60;
    private const string IdentityVersion = "completion-production-run-v1";
    private const string AuditIdentityVersion = "completion-production-audit-v1";
    private const string ProducerName = "completion-bound-producer";

    internal async Task<CompletionProductionResult> RunAsync(
        ReservoirWorld world,
        string completionBindingId,
        CompletionProductionRequest request,
        CancellationToken cancellationToken = default)
    {
        ResolvedCompletionBinding completion = await completionBindings.ResolveAsync(
            world, completionBindingId, cancellationToken)
            ?? throw Validation("completionBindingId", "Completion binding was not found.");
        ValidateRequest(world, completion, request);
        string requestJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        string requestHash = DeterministicEncoding.Sha256Hex(requestJson);
        string productionRunId = "rpr_" + DeterministicEncoding.Sha256Hex(string.Join("\n",
            IdentityVersion, world.Summary.WorldId, completionBindingId, requestHash));
        PersistedCompletionProductionRun? existing = await repository.LoadRunAsync(
            completionBindingId, productionRunId, cancellationToken);
        if (existing is not null)
            return await VerifyPersistedAsync(world, completion, existing, cancellationToken);

        WellConnection[] connections = completion.Openings
            .SelectMany(opening => opening.Connections)
            .ToArray();
        int[] checkpointYears = [1, 3, 5];
        var checkpoints = new List<CompletionProductionCheckpoint>(3);
        var globalSamples = new List<GlobalWellSample>();
        SimulationState? initialState = null;
        double initialTime = 0;
        string? parentStateId = null;
        PhaseVolumes cumulativeOffset = ZeroVolumes();

        foreach (int year in checkpointYears)
        {
            double checkpointTime = year * YearSeconds;
            SimulationRequest simulationRequest = BuildSimulationRequest(
                request, connections, checkpointTime, parentStateId);
            SimulationExecution execution = initialState is null
                ? simulator.Run(world, simulationRequest, cancellationToken)
                : simulator.Run(world, simulationRequest, initialState, initialTime, cancellationToken);
            diagnosticObserver?.Invoke(year, execution);
            if (execution.Result.MaterialBalance.Total.CumulativeInjectedM3 > 1e-10)
                throw new SimulationFailureException(
                    $"Production checkpoint year {year} attempted injection, which is not supported.");
            if (execution.Result.MaximumBalanceErrorFraction > 1e-6)
                throw new SimulationFailureException(
                    $"Production checkpoint year {year} exceeded the 1e-6 material-balance gate: " +
                    $"{execution.Result.MaximumBalanceErrorFraction:G17}.");

            AppendGlobalSamples(execution.Result, cumulativeOffset, globalSamples);
            PhaseVolumes intervalProduced = ProducedVolumes(execution.Result);
            cumulativeOffset = Add(cumulativeOffset, intervalProduced);
            string stateId = await stateStore.SaveAsync(
                world, parentStateId, simulationRequest, execution, cancellationToken);
            await stateRepository.PinStateAsync(new PersistedStatePin(
                stateId, "completion-production-checkpoint",
                $"{productionRunId}:year-{year}", timeProvider.GetUtcNow()), cancellationToken);
            checkpoints.Add(new CompletionProductionCheckpoint(
                year, checkpointTime, stateId, parentStateId, cumulativeOffset,
                execution.Result.MaximumBalanceErrorFraction));
            RestoredSimulationState restored = await stateStore.LoadAsync(world, stateId, cancellationToken)
                ?? throw new PersistenceIntegrityException($"Checkpoint state {stateId} disappeared.");
            initialState = restored.State;
            initialTime = restored.SimulatedTimeSeconds;
            parentStateId = stateId;
        }

        CompletionProductionMonthlyTruth[] monthly = BuildMonthlyTruth(
            world, request, globalSamples);
        var result = new CompletionProductionResult(
            productionRunId, world.Summary.WorldId, completionBindingId, ModelVersion,
            checkpoints, monthly);
        string responseJson = JsonSerializer.Serialize(result, DeterministicEncoding.JsonOptions);
        string responseHash = DeterministicEncoding.Sha256Hex(responseJson);
        string checkpointIdsJson = JsonSerializer.Serialize(
            checkpoints.Select(checkpoint => checkpoint.StateId).ToArray(),
            DeterministicEncoding.JsonOptions);
        var persisted = new PersistedCompletionProductionRun(
            productionRunId, world.Summary.WorldId, completionBindingId, ModelVersion,
            requestJson, requestHash, responseJson, responseHash, checkpointIdsJson,
            timeProvider.GetUtcNow());
        await repository.SaveRunAsync(persisted, cancellationToken);
        await SaveAuditAsync(persisted, "registration", cancellationToken);
        await SaveAuditAsync(persisted, "execution", cancellationToken);
        return result;
    }

    internal async Task<CompletionProductionResult?> GetAsync(
        ReservoirWorld world,
        string completionBindingId,
        string productionRunId,
        CancellationToken cancellationToken = default)
    {
        ResolvedCompletionBinding? completion = await completionBindings.ResolveAsync(
            world, completionBindingId, cancellationToken);
        if (completion is null)
            return null;
        PersistedCompletionProductionRun? persisted = await repository.LoadRunAsync(
            completionBindingId, productionRunId, cancellationToken);
        if (persisted is null)
            return null;
        CompletionProductionResult result = await VerifyPersistedAsync(
            world, completion, persisted, cancellationToken);
        await SaveAuditAsync(persisted, "resolution", cancellationToken);
        return result;
    }

    private async Task<CompletionProductionResult> VerifyPersistedAsync(
        ReservoirWorld world,
        ResolvedCompletionBinding completion,
        PersistedCompletionProductionRun persisted,
        CancellationToken cancellationToken)
    {
        if (persisted.WorldId != world.Summary.WorldId ||
            persisted.CompletionBindingId != completion.Metadata.CompletionBindingId ||
            persisted.ModelVersion != ModelVersion)
            throw new PersistenceIntegrityException(
                $"Completion production run {persisted.ProductionRunId} authority is inconsistent.");
        string requestHash = DeterministicEncoding.Sha256Hex(persisted.CanonicalRequestJson);
        string responseHash = DeterministicEncoding.Sha256Hex(persisted.ResponseJson);
        if (requestHash != persisted.CanonicalRequestHash || responseHash != persisted.ResponseHash)
            throw new PersistenceIntegrityException(
                $"Completion production run {persisted.ProductionRunId} failed a content hash.");
        string expectedId = "rpr_" + DeterministicEncoding.Sha256Hex(string.Join("\n",
            IdentityVersion, persisted.WorldId, persisted.CompletionBindingId, requestHash));
        if (expectedId != persisted.ProductionRunId)
            throw new PersistenceIntegrityException(
                $"Completion production run {persisted.ProductionRunId} failed deterministic identity.");
        CompletionProductionRequest request = JsonSerializer.Deserialize<CompletionProductionRequest>(
            persisted.CanonicalRequestJson, DeterministicEncoding.JsonOptions)
            ?? throw new PersistenceIntegrityException("Completion production request is null.");
        ValidateRequest(world, completion, request);
        CompletionProductionResult result = JsonSerializer.Deserialize<CompletionProductionResult>(
            persisted.ResponseJson, DeterministicEncoding.JsonOptions)
            ?? throw new PersistenceIntegrityException("Completion production response is null.");
        string[] checkpointIds = JsonSerializer.Deserialize<string[]>(
            persisted.CheckpointStateIdsJson, DeterministicEncoding.JsonOptions)
            ?? throw new PersistenceIntegrityException("Checkpoint identity list is null.");
        if (result.ProductionRunId != persisted.ProductionRunId ||
            result.WorldId != persisted.WorldId ||
            result.CompletionBindingId != persisted.CompletionBindingId ||
            result.ModelVersion != ModelVersion || result.Checkpoints.Count != 3 ||
            !checkpointIds.SequenceEqual(result.Checkpoints.Select(checkpoint => checkpoint.StateId)))
            throw new PersistenceIntegrityException(
                $"Completion production run {persisted.ProductionRunId} response metadata is inconsistent.");
        foreach (CompletionProductionCheckpoint checkpoint in result.Checkpoints)
        {
            RestoredSimulationState? state = await stateStore.LoadAsync(
                world, checkpoint.StateId, cancellationToken);
            if (state is null || state.ParentStateId != checkpoint.ParentStateId ||
                state.SimulatedTimeSeconds != checkpoint.SimulatedTimeSeconds ||
                await stateRepository.CountStatePinsAsync(checkpoint.StateId, cancellationToken) == 0)
                throw new PersistenceIntegrityException(
                    $"Completion production checkpoint {checkpoint.StateId} is not pinned and restart-safe.");
        }
        return result;
    }

    private static void ValidateRequest(
        ReservoirWorld world, ResolvedCompletionBinding completion, CompletionProductionRequest request)
    {
        var errors = new ValidationErrors();
        if (request.ProductionModelVersion != ModelVersion)
            errors.Add("productionModelVersion", $"Production model version must be {ModelVersion}.");
        if (request.Schedule is null || request.Schedule.Count == 0)
        {
            errors.Add("schedule", "A five-year production schedule is required.");
            errors.ThrowIfAny();
            return;
        }
        double horizon = 5 * YearSeconds;
        double tolerance = 1e-10 * horizon;
        double expectedStart = 0;
        bool hasProduction = false;
        bool hasShutIn = false;
        for (int index = 0; index < request.Schedule.Count; index++)
        {
            CompletionProductionScheduleSegment? segment = request.Schedule[index];
            string key = $"schedule[{index}]";
            if (segment is null)
            {
                errors.Add(key, "Schedule segment must not be null.");
                continue;
            }
            Finite(errors, $"{key}.startTimeSeconds", segment.StartTimeSeconds);
            Finite(errors, $"{key}.durationSeconds", segment.DurationSeconds);
            if (!(segment.DurationSeconds > 0) ||
                Math.Abs(segment.StartTimeSeconds - expectedStart) > tolerance)
                errors.Add(key, "Production schedule must be ordered, contiguous, and positive-duration.");
            if (double.IsFinite(segment.StartTimeSeconds) && double.IsFinite(segment.DurationSeconds))
                expectedStart = segment.StartTimeSeconds + segment.DurationSeconds;
            if (!Enum.IsDefined(segment.ControlMode))
                errors.Add($"{key}.controlMode", "Control mode is invalid.");
            ValidateOptionalPositive(errors, $"{key}.minimumBottomHolePressurePa",
                segment.MinimumBottomHolePressurePa);
            ValidateOptionalPositive(errors, $"{key}.maximumBottomHolePressurePa",
                segment.MaximumBottomHolePressurePa);
            ValidateOptionalPositive(errors, $"{key}.maximumAbsoluteRateM3PerSecond",
                segment.MaximumAbsoluteRateM3PerSecond);
            if (segment.MinimumBottomHolePressurePa is double minimum &&
                segment.MaximumBottomHolePressurePa is double maximum && minimum > maximum)
                errors.Add(key, "Minimum BHP must not exceed maximum BHP.");
            if (segment.ShutIn)
            {
                hasShutIn = true;
                continue;
            }
            hasProduction = true;
            Finite(errors, $"{key}.targetRateM3PerSecond", segment.TargetRateM3PerSecond);
            Finite(errors, $"{key}.targetBottomHolePressurePa", segment.TargetBottomHolePressurePa);
            if (segment.ControlMode == WellControlMode.Rate && !(segment.TargetRateM3PerSecond < 0))
                errors.Add($"{key}.targetRateM3PerSecond", "Production rate target must be negative.");
            if (segment.ControlMode == WellControlMode.Bhp &&
                !(segment.TargetBottomHolePressurePa > 0 &&
                  segment.TargetBottomHolePressurePa < world.PressurePa.Max()))
                errors.Add($"{key}.targetBottomHolePressurePa",
                    "Production BHP target must be positive and below initial reservoir pressure.");
        }
        if (Math.Abs(expectedStart - horizon) > tolerance)
            errors.Add("schedule", "Production schedule must exactly span five 365.25-day years.");
        if (!hasProduction)
            errors.Add("schedule", "At least one producing segment is required.");
        if (!hasShutIn)
            errors.Add("schedule", "At least one shut-in segment is required.");
        if (!double.IsFinite(request.InitialTimeStepSeconds) || !(request.InitialTimeStepSeconds > 0))
            errors.Add("initialTimeStepSeconds", "Initial timestep must be finite and positive.");
        if (request.Solver.MaximumSamplesPerWell < 60)
            errors.Add("solver.maximumSamplesPerWell", "At least 60 bounded well samples are required.");
        errors.ThrowIfAny();

        WellConnection[] connections = completion.Openings.SelectMany(opening => opening.Connections).ToArray();
        SimulationRequest validationRequest = BuildSimulationRequest(
            request, connections, horizon, null);
        SimulationRequestValidator.Validate(validationRequest, world);
    }

    private static SimulationRequest BuildSimulationRequest(
        CompletionProductionRequest request,
        IReadOnlyList<WellConnection> connections,
        double checkpointTime,
        string? parentStateId)
    {
        var segments = new List<ScheduleSegment>();
        foreach (CompletionProductionScheduleSegment segment in request.Schedule)
        {
            if (segment.StartTimeSeconds >= checkpointTime)
                break;
            double duration = Math.Min(
                segment.StartTimeSeconds + segment.DurationSeconds, checkpointTime) -
                segment.StartTimeSeconds;
            segments.Add(new ScheduleSegment
            {
                StartTimeSeconds = segment.StartTimeSeconds,
                DurationSeconds = duration,
                Wells = segment.ShutIn ? [] : [BuildWell(segment, connections)]
            });
        }
        return new SimulationRequest
        {
            DurationSeconds = checkpointTime,
            InitialTimeStepSeconds = request.InitialTimeStepSeconds,
            Solver = request.Solver,
            Fluids = new FluidModelOptions(),
            ContinueFromStateId = parentStateId,
            Schedule = segments
        };
    }

    private static WellControl BuildWell(
        CompletionProductionScheduleSegment segment, IReadOnlyList<WellConnection> connections) => new()
    {
        Name = ProducerName,
        ControlMode = segment.ControlMode,
        TotalRateM3PerSecond = segment.TargetRateM3PerSecond,
        TargetBottomHolePressurePa = segment.TargetBottomHolePressurePa,
        MinimumBottomHolePressurePa = segment.MinimumBottomHolePressurePa,
        MaximumBottomHolePressurePa = segment.MaximumBottomHolePressurePa,
        MaximumAbsoluteRateM3PerSecond = segment.MaximumAbsoluteRateM3PerSecond,
        Connections = connections
    };

    private static void AppendGlobalSamples(
        SimulationResult result, PhaseVolumes offset, List<GlobalWellSample> destination)
    {
        WellTimeSeries? well = result.Wells.SingleOrDefault(series => series.Name == ProducerName);
        if (well is null)
            return;
        foreach (WellRateSample sample in well.Samples)
            destination.Add(new GlobalWellSample(
                sample.TimeSeconds, sample.TimeStepSeconds, sample.SignedRatesM3PerSecond,
                Add(offset, sample.CumulativeProducedM3), sample.BottomHolePressurePa,
                sample.EffectiveControlMode, sample.SwitchReason));
    }

    private static CompletionProductionMonthlyTruth[] BuildMonthlyTruth(
        ReservoirWorld world, CompletionProductionRequest request, List<GlobalWellSample> samples)
    {
        samples.Sort((left, right) => left.TimeSeconds.CompareTo(right.TimeSeconds));
        var result = new CompletionProductionMonthlyTruth[60];
        PhaseVolumes previousCumulative = ZeroVolumes();
        double lastBhp = world.PressurePa.Average();
        for (int month = 1; month <= result.Length; month++)
        {
            double time = month * YearSeconds / 12;
            CompletionProductionScheduleSegment segment = request.Schedule.First(item =>
                time > item.StartTimeSeconds &&
                time <= item.StartTimeSeconds + item.DurationSeconds + 1e-9);
            GlobalWellSample? sample = SampleAt(samples, time);
            PhaseVolumes cumulative = CumulativeAt(samples, time);
            cumulative = new PhaseVolumes(
                Math.Max(previousCumulative.Oil, cumulative.Oil),
                Math.Max(previousCumulative.Water, cumulative.Water),
                Math.Max(previousCumulative.Gas, cumulative.Gas));
            double oilRate = 0;
            double waterRate = 0;
            double gasRate = 0;
            WellControlMode mode = segment.ControlMode;
            string? reason = segment.ShutIn ? "Shut-in" : sample?.SwitchReason;
            if (!segment.ShutIn && sample is not null)
            {
                oilRate = ProductionRate(sample.Rates.Oil);
                waterRate = ProductionRate(sample.Rates.Water);
                gasRate = ProductionRate(sample.Rates.Gas);
                lastBhp = sample.BottomHolePressurePa;
                mode = sample.EffectiveControlMode;
            }
            ValidateMonthlyValues(month, oilRate, waterRate, gasRate, cumulative, lastBhp);
            result[month - 1] = new CompletionProductionMonthlyTruth(
                month, time, oilRate, waterRate, gasRate,
                cumulative.Oil, cumulative.Water, cumulative.Gas, lastBhp, mode, reason);
            previousCumulative = cumulative;
        }
        return result;
    }

    private static GlobalWellSample? SampleAt(List<GlobalWellSample> samples, double time) =>
        samples.FirstOrDefault(sample =>
            time > sample.TimeSeconds - sample.TimeStepSeconds && time <= sample.TimeSeconds)
        ?? samples.LastOrDefault(sample => sample.TimeSeconds <= time)
        ?? samples.FirstOrDefault(sample => sample.TimeSeconds >= time);

    private static PhaseVolumes CumulativeAt(List<GlobalWellSample> samples, double time)
    {
        GlobalWellSample? active = samples.FirstOrDefault(sample =>
            time > sample.TimeSeconds - sample.TimeStepSeconds && time <= sample.TimeSeconds);
        if (active is not null)
        {
            double fraction = (time - (active.TimeSeconds - active.TimeStepSeconds)) /
                active.TimeStepSeconds;
            return new PhaseVolumes(
                active.CumulativeProduced.Oil -
                    (1 - fraction) * ProductionRate(active.Rates.Oil) * active.TimeStepSeconds,
                active.CumulativeProduced.Water -
                    (1 - fraction) * ProductionRate(active.Rates.Water) * active.TimeStepSeconds,
                active.CumulativeProduced.Gas -
                    (1 - fraction) * ProductionRate(active.Rates.Gas) * active.TimeStepSeconds);
        }
        return samples.LastOrDefault(sample => sample.TimeSeconds <= time)?.CumulativeProduced
            ?? ZeroVolumes();
    }

    private static PhaseVolumes ProducedVolumes(SimulationResult result) => new(
        result.MaterialBalance.Oil.CumulativeProducedM3,
        result.MaterialBalance.Water.CumulativeProducedM3,
        result.MaterialBalance.Gas.CumulativeProducedM3);

    private static double ProductionRate(double signedRate)
    {
        if (signedRate > 1e-12)
            throw new SimulationFailureException(
                 $"Completion production schedule attempted injection at {signedRate:G17} m3/s, which is not supported.");
        return Math.Max(0, -signedRate);
    }

    private static void ValidateMonthlyValues(
        int month, double oilRate, double waterRate, double gasRate,
        PhaseVolumes cumulative, double bhp)
    {
        if (!double.IsFinite(oilRate) || !double.IsFinite(waterRate) || !double.IsFinite(gasRate) ||
            oilRate < 0 || waterRate < 0 || gasRate < 0 ||
            !double.IsFinite(cumulative.Oil) || !double.IsFinite(cumulative.Water) ||
            !double.IsFinite(cumulative.Gas) || cumulative.Oil < 0 ||
            cumulative.Water < 0 || cumulative.Gas < 0 ||
            !double.IsFinite(bhp) || bhp <= 0)
            throw new SimulationFailureException($"Monthly production truth is invalid at month {month}.");
    }

    private async Task SaveAuditAsync(
        PersistedCompletionProductionRun run, string action, CancellationToken cancellationToken)
    {
        string auditId = "rpa_" + DeterministicEncoding.Sha256Hex(string.Join("\n",
            AuditIdentityVersion, run.ProductionRunId, run.CompletionBindingId, run.WorldId,
            action, run.CanonicalRequestHash, "3"));
        await repository.SaveAuditAsync(new PersistedCompletionProductionAudit(
            auditId, run.ProductionRunId, run.CompletionBindingId, run.WorldId, action,
            run.CanonicalRequestHash, 3, timeProvider.GetUtcNow()), cancellationToken);
    }

    private static PhaseVolumes Add(PhaseVolumes left, PhaseVolumes right) => new(
        left.Oil + right.Oil, left.Water + right.Water, left.Gas + right.Gas);

    private static PhaseVolumes ZeroVolumes() => new(0, 0, 0);

    private static void ValidateOptionalPositive(
        ValidationErrors errors, string key, double? value)
    {
        if (value is not double actual)
            return;
        Finite(errors, key, actual);
        if (!(actual > 0))
            errors.Add(key, "Constraint must be positive.");
    }

    private static void Finite(ValidationErrors errors, string key, double value) =>
        errors.RequireFinite(key, value);

    private static ReservoirValidationException Validation(string key, string message) => new(
        new Dictionary<string, string[]> { [key] = [message] });

    private sealed record GlobalWellSample(
        double TimeSeconds,
        double TimeStepSeconds,
        PhaseVolumes Rates,
        PhaseVolumes CumulativeProduced,
        double BottomHolePressurePa,
        WellControlMode EffectiveControlMode,
        string? SwitchReason);
}
