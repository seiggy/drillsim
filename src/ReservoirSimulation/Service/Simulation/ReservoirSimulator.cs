using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Simulation;

internal interface IReservoirSimulator
{
    SimulationExecution Run(ReservoirWorld world, SimulationRequest request, CancellationToken cancellationToken = default);
    SimulationExecution Run(ReservoirWorld world, SimulationRequest request, SimulationState initialState,
        double initialSimulatedTimeSeconds, CancellationToken cancellationToken = default);
}

internal sealed record SimulationState(
    double[] PressurePa,
    double[] OilSaturation,
    double[] WaterSaturation,
    double[] GasSaturation);

internal sealed record SimulationExecution(
    SimulationResult Result,
    SimulationState FinalState,
    NumericalWorkDiagnostics Work);

internal sealed class ReservoirSimulator(bool reverseInitialUpwindForDiagnostics = false) : IReservoirSimulator
{
    private static readonly ConditionalWeakTable<ReservoirWorld, ConcurrentDictionary<FluidModelOptions, double[]>> EquilibriumFluxCache = new();
    private const int MaximumUpwindIterations = 8;
    private const double PhasePotentialRelativeTolerance = 1e-8;
    private const double SaturationRoundoffTolerance = 1e-8;
    private const double EquilibriumSaturationTolerance = 1e-5;
    private const double EquilibriumPressureRelativeTolerance = 2e-5;
    internal const string PressureCouplingModelVersion = "pressure-coupling-v2";
    private const double NegligibleCouplingRelativeThreshold = 1e-2;
    private const double PressureCoefficientContrastThreshold = 100;
    private const double ReservoirScaleBalanceFloorFraction = 1e-8;

    public SimulationExecution Run(
        ReservoirWorld world,
        SimulationRequest request,
        CancellationToken cancellationToken = default) =>
        RunCore(world, request, null, 0, cancellationToken);

    public SimulationExecution Run(
        ReservoirWorld world,
        SimulationRequest request,
        SimulationState initialState,
        double initialSimulatedTimeSeconds,
        CancellationToken cancellationToken = default) =>
        RunCore(world, request, initialState, initialSimulatedTimeSeconds, cancellationToken);

    private SimulationExecution RunCore(
        ReservoirWorld world,
        SimulationRequest request,
        SimulationState? initialState,
        double initialSimulatedTimeSeconds,
        CancellationToken cancellationToken)
    {
        SimulationRequestValidator.Validate(request, world);
        var work = new NumericalWorkTracker();
        if (!double.IsFinite(initialSimulatedTimeSeconds) || initialSimulatedTimeSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(initialSimulatedTimeSeconds));
        SimulationState state = initialState is null
            ? new SimulationState(
                (double[])world.PressurePa.Clone(),
                (double[])world.OilSaturation.Clone(),
                (double[])world.WaterSaturation.Clone(),
                (double[])world.GasSaturation.Clone())
            : CloneAndValidateInitialState(world, initialState);
        double[] initialInventory = Inventory(world, state);
        var compressibilityStorage = new double[Phases.Count];
        bool hasSchedule = request.Schedule.Count > 0;
        double finalSimulatedTime = hasSchedule
            ? request.DurationSeconds
            : initialSimulatedTimeSeconds + request.DurationSeconds;
        if (hasSchedule && initialSimulatedTimeSeconds >= finalSimulatedTime)
            throw new SimulationFailureException(
                "Continuation time must be before the end of the supplied schedule.");
        WellControl[] resultWells = (hasSchedule
                ? request.Schedule.SelectMany(segment => segment.Wells)
                : request.Wells)
            .DistinctBy(well => well.Name, StringComparer.Ordinal)
            .ToArray();
        double runDuration = finalSimulatedTime - initialSimulatedTimeSeconds;
        WellHistoryBuilder[] histories = resultWells.Select(well => new WellHistoryBuilder(
            well, initialSimulatedTimeSeconds, runDuration, request.Solver.MaximumSamplesPerWell)).ToArray();
        Dictionary<string, WellHistoryBuilder> historiesByName = histories.ToDictionary(
            history => history.Name, StringComparer.Ordinal);

        double time = 0;
        double timeStep = request.InitialTimeStepSeconds;
        int acceptedSteps = 0;
        int rejectedSteps = 0;
        int totalCgIterations = 0;
        double minimumAcceptedTimeStep = double.PositiveInfinity;
        double maximumAcceptedTimeStep = 0;
        double[]? rejectedPressureGuess = null;
        double rejectedPressureTimeStep = 0;

        while (initialSimulatedTimeSeconds + time < finalSimulatedTime)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (acceptedSteps + rejectedSteps >= request.Solver.MaximumStepAttempts)
                throw new SimulationFailureException(
                    $"Maximum step-attempt budget of {request.Solver.MaximumStepAttempts} was exhausted at t={(initialSimulatedTimeSeconds + time):G17} s.");
            double absoluteTime = initialSimulatedTimeSeconds + time;
            IReadOnlyList<WellControl> activeWells = ActiveWells(request, absoluteTime, finalSimulatedTime, out double controlEndTime);
            if (activeWells.Count == 0 && IsDiscreteNoFlowEquilibrium(world, state, request.Fluids))
            {
                double equilibriumAdvance = controlEndTime - absoluteTime;
                time += equilibriumAdvance;
                acceptedSteps++;
                minimumAcceptedTimeStep = Math.Min(minimumAcceptedTimeStep, equilibriumAdvance);
                maximumAcceptedTimeStep = Math.Max(maximumAcceptedTimeStep, equilibriumAdvance);
                timeStep = request.Solver.MaximumTimeStepSeconds;
                rejectedPressureGuess = null;
                continue;
            }
            double remaining = finalSimulatedTime - absoluteTime;
            double attemptedTimeStep = Math.Min(Math.Min(timeStep, remaining), controlEndTime - absoluteTime);
            StepAttempt attempt = Advance(
                world, state, request, attemptedTimeStep, activeWells, reverseInitialUpwindForDiagnostics,
                rejectedPressureGuess, rejectedPressureTimeStep, work);
            totalCgIterations += attempt.CgIterations;

            string? rejectionReason = !attempt.IsPhysical
                ? attempt.FailureReason
                : attempt.MaximumSaturationChange > request.Solver.MaximumSaturationChange
                    ? $"maximum saturation change {attempt.MaximumSaturationChange:G6} exceeded {request.Solver.MaximumSaturationChange:G6}"
                    : null;
            if (rejectionReason is not null)
            {
                rejectedSteps++;
                rejectedPressureGuess = attempt.State.PressurePa;
                rejectedPressureTimeStep = attemptedTimeStep;
                if (attemptedTimeStep <= request.Solver.MinimumTimeStepSeconds * (1 + 1e-12))
                    throw new SimulationFailureException(
                        $"Adaptive timestep underflow after {rejectionReason} at t={(initialSimulatedTimeSeconds + time):G17} s; " +
                        $"minimum timestep is {request.Solver.MinimumTimeStepSeconds:G17} s.");
                timeStep = Math.Max(request.Solver.MinimumTimeStepSeconds,
                    attemptedTimeStep * request.Solver.TimeStepShrinkFactor);
                continue;
            }

            AccumulateCompressibility(world, state, attempt.State, request.Fluids, compressibilityStorage);
            state = attempt.State;
            rejectedPressureGuess = null;
            time += attemptedTimeStep;
            acceptedSteps++;
            minimumAcceptedTimeStep = Math.Min(minimumAcceptedTimeStep, attemptedTimeStep);
            maximumAcceptedTimeStep = Math.Max(maximumAcceptedTimeStep, attemptedTimeStep);
            for (int wellIndex = 0; wellIndex < activeWells.Count; wellIndex++)
                historiesByName[activeWells[wellIndex].Name].Accept(
                    initialSimulatedTimeSeconds + time, attemptedTimeStep, attempt.WellRates[wellIndex]);

            if (attempt.MaximumSaturationChange <= request.Solver.GrowthSaturationChange)
                timeStep = Math.Min(request.Solver.MaximumTimeStepSeconds,
                    attemptedTimeStep * request.Solver.TimeStepGrowthFactor);
            else
                timeStep = attemptedTimeStep;
        }

        ValidateFinalState(state);
        double[] finalInventory = Inventory(world, state);
        PhaseVolumes injected = SumVolumes(histories.Select(history => history.CumulativeInjected));
        PhaseVolumes produced = SumVolumes(histories.Select(history => history.CumulativeProduced));
        MaterialBalanceDiagnostics balances = BuildBalances(
            initialInventory, finalInventory, compressibilityStorage, injected, produced);
        var result = new SimulationResult(
            world.Summary.WorldId,
            KernelMetadata.Name,
            KernelMetadata.Limitation,
            finalSimulatedTime,
            new SimulationStepDiagnostics(
                acceptedSteps, rejectedSteps, totalCgIterations,
                minimumAcceptedTimeStep, maximumAcceptedTimeStep),
            new SimulationStateRanges(
                RangeCalculator.Of(state.PressurePa),
                RangeCalculator.Of(state.OilSaturation),
                RangeCalculator.Of(state.WaterSaturation),
                RangeCalculator.Of(state.GasSaturation),
                MaximumClosureError(state)),
            balances,
            new[]
            {
                balances.Oil.InPlaceNormalizedBalanceErrorFraction,
                balances.Oil.ThroughputNormalizedBalanceErrorFraction,
                balances.Water.InPlaceNormalizedBalanceErrorFraction,
                balances.Water.ThroughputNormalizedBalanceErrorFraction,
                balances.Gas.InPlaceNormalizedBalanceErrorFraction,
                balances.Gas.ThroughputNormalizedBalanceErrorFraction,
                balances.Total.InPlaceNormalizedBalanceErrorFraction,
                balances.Total.ThroughputNormalizedBalanceErrorFraction
            }.Max(),
            histories.Select(history => history.Build()).ToArray());
        return new SimulationExecution(result, state, work.Snapshot());
    }

    private static double[] GetEquilibriumFlux(ReservoirWorld world, FluidModelOptions fluids)
    {
        ConcurrentDictionary<FluidModelOptions, double[]> cache = EquilibriumFluxCache.GetValue(
            world, static _ => new ConcurrentDictionary<FluidModelOptions, double[]>());
        return cache.GetOrAdd(fluids, model => ComputeEquilibriumFlux(world, model));
    }

    private static double[] ComputeEquilibriumFlux(ReservoirWorld world, FluidModelOptions fluids)
    {
        PhaseState phases = FluidPhysics.Evaluate(
            world.PressurePa, world.OilSaturation, world.WaterSaturation, world.GasSaturation, fluids);
        int cellCount = world.Grid.CellCount;
        var fluxes = new double[world.Faces.Length * Phases.Count];
        for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
        {
            GridFace face = world.Faces[faceIndex];
            for (int phase = 0; phase < Phases.Count; phase++)
            {
                int offsetA = FluidPhysics.Offset(phase, face.CellA, cellCount);
                int offsetB = FluidPhysics.Offset(phase, face.CellB, cellCount);
                double faceDensity = 0.5 * (phases.DensityKgPerM3[offsetA] + phases.DensityKgPerM3[offsetB]);
                double potential = world.PressurePa[face.CellA] - world.PressurePa[face.CellB] -
                    faceDensity * fluids.GravityMPerS2 * face.DepthDifferenceM;
                double tolerance = PhasePotentialRelativeTolerance * Math.Max(1,
                    Math.Max(Math.Abs(world.PressurePa[face.CellA]), Math.Abs(world.PressurePa[face.CellB])));
                if (Math.Abs(potential) <= tolerance)
                    continue;
                int upstreamOffset = potential >= 0 ? offsetA : offsetB;
                fluxes[faceIndex * Phases.Count + phase] =
                    face.GeometricTransmissibility * phases.Mobility[upstreamOffset] * potential;
            }
        }
        return fluxes;
    }


    private static bool IsDiscreteNoFlowEquilibrium(
        ReservoirWorld world, SimulationState state, FluidModelOptions fluids)
    {
        double pressureOffset = 0;
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
            pressureOffset += state.PressurePa[cell] - world.PressurePa[cell];
        pressureOffset /= world.Grid.CellCount;
        bool nearInitialEquilibrium = true;
        for (int cell = 0; cell < world.Grid.CellCount && nearInitialEquilibrium; cell++)
        {
            double pressureTolerance = EquilibriumPressureRelativeTolerance * Math.Max(1, world.PressurePa[cell]);
            nearInitialEquilibrium =
                Math.Abs((state.PressurePa[cell] - world.PressurePa[cell]) - pressureOffset) <= pressureTolerance &&
                Math.Abs(state.OilSaturation[cell] - world.OilSaturation[cell]) <= EquilibriumSaturationTolerance &&
                Math.Abs(state.WaterSaturation[cell] - world.WaterSaturation[cell]) <= EquilibriumSaturationTolerance &&
                Math.Abs(state.GasSaturation[cell] - world.GasSaturation[cell]) <= EquilibriumSaturationTolerance;
        }
        if (nearInitialEquilibrium)
            return true;

        PhaseState phases = FluidPhysics.Evaluate(
            state.PressurePa, state.OilSaturation, state.WaterSaturation, state.GasSaturation, fluids);
        int cellCount = world.Grid.CellCount;
        foreach (GridFace face in world.Faces)
        for (int phase = 0; phase < Phases.Count; phase++)
        {
            int offsetA = FluidPhysics.Offset(phase, face.CellA, cellCount);
            int offsetB = FluidPhysics.Offset(phase, face.CellB, cellCount);
            double faceDensity = 0.5 * (phases.DensityKgPerM3[offsetA] + phases.DensityKgPerM3[offsetB]);
            double potential = state.PressurePa[face.CellA] - state.PressurePa[face.CellB] -
                faceDensity * fluids.GravityMPerS2 * face.DepthDifferenceM;
            double tolerance = PhasePotentialRelativeTolerance * Math.Max(1,
                Math.Max(Math.Abs(state.PressurePa[face.CellA]), Math.Abs(state.PressurePa[face.CellB])));
            if (Math.Abs(potential) <= tolerance)
                continue;
            int upstreamOffset = potential >= 0 ? offsetA : offsetB;
            if (face.GeometricTransmissibility * phases.Mobility[upstreamOffset] > 0)
                return false;
        }
        return true;
    }


    private static IReadOnlyList<WellControl> ActiveWells(
        SimulationRequest request, double absoluteTime, double finalTime, out double controlEndTime)
    {
        if (request.Schedule.Count == 0)
        {
            controlEndTime = finalTime;
            return request.Wells;
        }
        foreach (ScheduleSegment segment in request.Schedule)
        {
            double end = segment.StartTimeSeconds + segment.DurationSeconds;
            if (absoluteTime >= segment.StartTimeSeconds && absoluteTime < end)
            {
                controlEndTime = end;
                return segment.Wells;
            }
        }
        throw new SimulationFailureException(
            $"No schedule segment covers absolute simulation time {absoluteTime:G17} s.");
    }


    private static StepAttempt Advance(
        ReservoirWorld world,
        SimulationState current,
        SimulationRequest request,
        double timeStep,
        IReadOnlyList<WellControl> activeWells,
        bool reverseInitialUpwind,
        double[]? rejectedPressureGuess,
        double rejectedPressureTimeStep,
        NumericalWorkTracker work)
    {
        int cellCount = world.Grid.CellCount;
        FluidModelOptions fluids = request.Fluids;
        PhaseState phaseState = FluidPhysics.Evaluate(
            current.PressurePa, current.OilSaturation, current.WaterSaturation, current.GasSaturation, fluids);
        double[]? equilibriumFlux = world.HasExplicitFluidContacts
            ? GetEquilibriumFlux(world, fluids)
            : null;
        WellPreparation wells;
        long wellStarted = Stopwatch.GetTimestamp();
        try
        {
            wells = PeacemanWellModel.Prepare(world, activeWells, phaseState.Mobility, current.PressurePa);
            work.AddWell(wellStarted);
        }
        catch (WellControlSolveException exception)
        {
            return StepAttempt.Invalid(current, [], 0, exception.Message);
        }
        double[] storageDiagonal = BuildStorageDiagonal(world, current, fluids, timeStep);
        long upwindStarted = Stopwatch.GetTimestamp();
        byte[] upwind = SelectUpwind(world, current.PressurePa, phaseState.DensityKgPerM3, phaseState.Mobility, fluids.GravityMPerS2);
        work.AddUpwind(upwindStarted);
        if (reverseInitialUpwind)
            for (int index = 0; index < upwind.Length; index++) upwind[index] = (byte)(1 - upwind[index]);
        double[] pressure = InitialPressureGuess(
            current, wells, storageDiagonal, timeStep, rejectedPressureGuess, rejectedPressureTimeStep);
        var faceConductance = new double[world.Faces.Length];
        var matrixDiagonal = new double[cellCount];
        var rightHandSide = new double[cellCount];
        int cgIterations = 0;
        bool stable = false;
        byte[]? priorUpwind = null;
        var lockedUpwind = new bool[upwind.Length];
        var lockedValues = new byte[upwind.Length];
        bool useLaggedLowContrastUpwind =
            world.PermeabilityM2.Max() / world.PermeabilityM2.Min() < 1_000;

        for (int upwindIteration = 0; upwindIteration < MaximumUpwindIterations; upwindIteration++)
        {
            BuildPressureSystem(world, current, fluids, phaseState, wells, storageDiagonal, matrixDiagonal, upwind,
                faceConductance, rightHandSide, equilibriumFlux);
            bool stronglyHeterogeneous = RequiresStructuredPreconditioner(faceConductance);
            work.SelectPressurePath(stronglyHeterogeneous
                ? "high-contrast-symmetric-sparse-ic0-coarse"
                : "low-contrast-jacobi-sparse-coarse");
            var system = new MatrixFreePressureSystem(
                matrixDiagonal, world.Faces, world.FaceOffsets, world.IncidentFaceIndices,
                world.CellColors, faceConductance, work, stronglyHeterogeneous);
            CoarsePressurePreconditioner? coarsePreconditioner = BuildCoarsePreconditioner(
                world, matrixDiagonal, faceConductance);
            CgResult cg = ConjugateGradientSolver.Solve(system, rightHandSide, pressure,
                request.Solver.CgRelativeTolerance, request.Solver.CgMaximumIterations,
                stronglyHeterogeneous, coarsePreconditioner, work);
            cgIterations += cg.Iterations;
            if (!cg.Converged)
                return StepAttempt.Invalid(
                    current, Array.Empty<WellSampleState>(), cgIterations,
                    $"pressure CG did not converge in {cg.Iterations} iterations; relative residual={cg.RelativeResidual:G6}.");
            if (pressure.Any(value => !double.IsFinite(value)))
                throw new SimulationFailureException("Pressure solve produced a nonfinite state.");
            if (useLaggedLowContrastUpwind)
            {
                stable = true;
                break;
            }

            upwindStarted = Stopwatch.GetTimestamp();
            byte[] nextUpwind = SelectUpwind(world, pressure, phaseState.DensityKgPerM3, phaseState.Mobility, fluids.GravityMPerS2);
            work.AddUpwind(upwindStarted);
            for (int index = 0; index < nextUpwind.Length; index++)
                if (lockedUpwind[index]) nextUpwind[index] = lockedValues[index];
            if (priorUpwind is not null && nextUpwind.AsSpan().SequenceEqual(priorUpwind))
            {
                for (int index = 0; index < nextUpwind.Length; index++)
                {
                    if (nextUpwind[index] == upwind[index])
                        continue;
                    int faceIndex = index / Phases.Count;
                    int phase = index % Phases.Count;
                    GridFace face = world.Faces[faceIndex];
                    double mobilityA = phaseState.Mobility[FluidPhysics.Offset(phase, face.CellA, cellCount)];
                    double mobilityB = phaseState.Mobility[FluidPhysics.Offset(phase, face.CellB, cellCount)];
                    lockedValues[index] = mobilityA <= mobilityB ? (byte)0 : (byte)1;
                    lockedUpwind[index] = true;
                    nextUpwind[index] = lockedValues[index];
                }
            }
            if (upwind.AsSpan().SequenceEqual(nextUpwind) || UpwindSystemsEquivalent(
                world, upwind, nextUpwind, phaseState.Mobility, faceConductance))
            {
                upwind = nextUpwind;
                stable = true;
                break;
            }
            priorUpwind = (byte[])upwind.Clone();
            upwind = nextUpwind;
        }

        if (!stable)
            return StepAttempt.Invalid(current, Array.Empty<WellSampleState>(), cgIterations,
                $"phase-potential upwinding did not stabilize; locked {lockedUpwind.Count(value => value)} two-cycle faces");

        if (pressure.Any(value => value <= 0))
            return StepAttempt.Invalid(current, Array.Empty<WellSampleState>(), cgIterations, "an invalid nonpositive pressure state");

        double[] divergence = BuildPhaseDivergence(
            world, pressure, phaseState, upwind, fluids.GravityMPerS2, faceConductance, equilibriumFlux);
        WellStepAllocation wellStep;
        wellStarted = Stopwatch.GetTimestamp();
        try
        {
            wellStep = PeacemanWellModel.Finalize(wells, pressure, cellCount);
            work.AddWell(wellStarted);
        }
        catch (WellControlSolveException exception)
        {
            return StepAttempt.Invalid(current, [], cgIterations, exception.Message);
        }
        var oil = new double[cellCount];
        var water = new double[cellCount];
        var gas = new double[cellCount];
        double maximumChange = 0;
        int closurePhase = DominantInventoryPhase(world, current);
        bool physical = true;
        string? invalidSaturationReason = null;

        for (int cell = 0; cell < cellCount; cell++)
        {
            double pressureChange = pressure[cell] - current.PressurePa[cell];
            oil[cell] = UpdatedSaturation(Phases.Oil, cell, current.OilSaturation[cell], pressureChange,
                world, fluids, wellStep.CellPhaseSourceM3PerSecond, divergence, timeStep);
            water[cell] = UpdatedSaturation(Phases.Water, cell, current.WaterSaturation[cell], pressureChange,
                world, fluids, wellStep.CellPhaseSourceM3PerSecond, divergence, timeStep);
            gas[cell] = UpdatedSaturation(Phases.Gas, cell, current.GasSaturation[cell], pressureChange,
                world, fluids, wellStep.CellPhaseSourceM3PerSecond, divergence, timeStep);

            double closure = oil[cell] + water[cell] + gas[cell];
            if (!double.IsFinite(closure))
                throw new SimulationFailureException("Explicit phase transport produced a nonfinite saturation state.");
            if (Math.Abs(closure - 1) > 1e-5)
                return StepAttempt.Invalid(current, Array.Empty<WellSampleState>(), cgIterations,
                    $"pressure/transport saturation closure {closure:G17} exceeded tolerance in cell {cell}");
            double closureCorrection = 1 - closure;
            if (closurePhase == Phases.Oil && oil[cell] + closureCorrection >= -SaturationRoundoffTolerance)
                oil[cell] += closureCorrection;
            else if (closurePhase == Phases.Water && water[cell] + closureCorrection >= -SaturationRoundoffTolerance)
                water[cell] += closureCorrection;
            else if (closurePhase == Phases.Gas && gas[cell] + closureCorrection >= -SaturationRoundoffTolerance)
                gas[cell] += closureCorrection;
            else if (oil[cell] >= water[cell] && oil[cell] >= gas[cell])
                oil[cell] += closureCorrection;
            else if (water[cell] >= gas[cell])
                water[cell] += closureCorrection;
            else
                gas[cell] += closureCorrection;

            if (oil[cell] < -SaturationRoundoffTolerance || oil[cell] > 1 + SaturationRoundoffTolerance ||
                water[cell] < -SaturationRoundoffTolerance || water[cell] > 1 + SaturationRoundoffTolerance ||
                gas[cell] < -SaturationRoundoffTolerance || gas[cell] > 1 + SaturationRoundoffTolerance)
            {
                physical = false;
                invalidSaturationReason ??=
                    $"invalid saturation state in cell {cell}: So={oil[cell]:G17}, Sw={water[cell]:G17}, Sg={gas[cell]:G17}";
            }

            if (physical)
            {
                water[cell] = Math.Clamp(water[cell], 0, 1);
                gas[cell] = Math.Clamp(gas[cell], 0, 1);
                oil[cell] = 1 - water[cell] - gas[cell];
                if (oil[cell] < -SaturationRoundoffTolerance)
                    physical = false;
                else if (oil[cell] < 0)
                {
                    double hydrocarbonClosure = water[cell] + gas[cell];
                    water[cell] /= hydrocarbonClosure;
                    gas[cell] /= hydrocarbonClosure;
                    oil[cell] = 0;
                }
            }

            maximumChange = Math.Max(maximumChange, Math.Abs(oil[cell] - current.OilSaturation[cell]));
            maximumChange = Math.Max(maximumChange, Math.Abs(water[cell] - current.WaterSaturation[cell]));
            maximumChange = Math.Max(maximumChange, Math.Abs(gas[cell] - current.GasSaturation[cell]));
        }

        var next = new SimulationState(pressure, oil, water, gas);
        return physical
            ? new StepAttempt(next, wellStep.WellSamples, maximumChange, cgIterations, true, null)
            : StepAttempt.Invalid(next, wellStep.WellSamples, cgIterations,
                invalidSaturationReason ?? "an invalid saturation state");
    }

    private static bool RequiresStructuredPreconditioner(ReadOnlySpan<double> conductance)
    {
        double minimum = double.PositiveInfinity;
        double maximum = 0;
        foreach (double value in conductance)
            if (value > 0)
            {
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }
        return maximum / minimum >= PressureCoefficientContrastThreshold;
    }


    private static CoarsePressurePreconditioner? BuildCoarsePreconditioner(
        ReservoirWorld world, ReadOnlySpan<double> fineDiagonal, ReadOnlySpan<double> fineConductance)
    {
        if (world.Grid.CellCount < 4_096)
            return null;
        const int factorX = 8;
        const int factorY = 8;
        const int factorZ = 10;
        int coarseX = (world.Grid.CountX + factorX - 1) / factorX;
        int coarseY = (world.Grid.CountY + factorY - 1) / factorY;
        int coarseZ = (world.Grid.CountZ + factorZ - 1) / factorZ;
        int coarseCount = coarseX * coarseY * coarseZ;
        var mapping = new int[world.Grid.CellCount];
        for (int cell = 0; cell < mapping.Length; cell++)
        {
            int i = cell % world.Grid.CountX;
            int plane = cell / world.Grid.CountX;
            int j = plane % world.Grid.CountY;
            int k = plane / world.Grid.CountY;
            mapping[cell] = ((k / factorZ) * coarseY + j / factorY) * coarseX + i / factorX;
        }
        var coarseDiagonal = new double[coarseCount];
        for (int cell = 0; cell < mapping.Length; cell++)
            coarseDiagonal[mapping[cell]] += fineDiagonal[cell];
        var edges = new Dictionary<(int A, int B), double>();
        for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
        {
            int coarseA = mapping[world.Faces[faceIndex].CellA];
            int coarseB = mapping[world.Faces[faceIndex].CellB];
            if (coarseA == coarseB)
                continue;
            if (coarseA > coarseB) (coarseA, coarseB) = (coarseB, coarseA);
            var edge = (coarseA, coarseB);
            edges[edge] = edges.GetValueOrDefault(edge) + fineConductance[faceIndex];
        }
        KeyValuePair<(int A, int B), double>[] edgeValues = edges.ToArray();
        GridFace[] coarseFaces = edgeValues
            .Select(item => new GridFace(item.Key.A, item.Key.B, 0, 0))
            .ToArray();
        double[] coarseConductance = edgeValues.Select(item => item.Value).ToArray();
        return new CoarsePressurePreconditioner(mapping, coarseDiagonal, coarseFaces, coarseConductance);
    }


    private static double[] InitialPressureGuess(
        SimulationState current, WellPreparation wells, double[] storageDiagonal, double timeStep,
        double[]? rejectedPressureGuess, double rejectedPressureTimeStep)
    {
        var pressure = (double[])current.PressurePa.Clone();
        if (rejectedPressureGuess is not null && rejectedPressureTimeStep > 0)
        {
            double scale = timeStep / rejectedPressureTimeStep;
            for (int cell = 0; cell < pressure.Length; cell++)
                pressure[cell] += (rejectedPressureGuess[cell] - pressure[cell]) * scale;
        }
        else
        {
            ApplyGlobalPressureModeInitialGuess(current, wells, storageDiagonal, pressure);
        }
        return pressure;
    }


    private static void ApplyGlobalPressureModeInitialGuess(
        SimulationState current, WellPreparation wells, double[] storageDiagonal, double[] pressure)
    {
        double numerator = 0;
        double denominator = 0;
        int cellCount = pressure.Length;
        for (int cell = 0; cell < cellCount; cell++)
        {
            double fixedRate = 0;
            for (int phase = 0; phase < Phases.Count; phase++)
                fixedRate += wells.FixedCellPhaseSourceM3PerSecond[
                    FluidPhysics.Offset(phase, cell, cellCount)];
            numerator += fixedRate + wells.BhpCellRightHandSide[cell] -
                wells.BhpCellConductance[cell] * current.PressurePa[cell];
            denominator += storageDiagonal[cell] + wells.BhpCellConductance[cell];
        }
        double shift = numerator / denominator;
        if (!double.IsFinite(shift))
            throw new SimulationFailureException("Global pressure-mode initial guess is nonfinite.");
        for (int cell = 0; cell < cellCount; cell++)
            pressure[cell] += shift;
    }


    private static int DominantInventoryPhase(ReservoirWorld world, SimulationState state)
    {
        double oil = 0;
        double water = 0;
        double gas = 0;
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
        {
            oil += world.PoreVolumeM3[cell] * state.OilSaturation[cell];
            water += world.PoreVolumeM3[cell] * state.WaterSaturation[cell];
            gas += world.PoreVolumeM3[cell] * state.GasSaturation[cell];
        }
        return oil >= water && oil >= gas ? Phases.Oil : water >= gas ? Phases.Water : Phases.Gas;
    }


    private static double[] BuildStorageDiagonal(
        ReservoirWorld world,
        SimulationState state,
        FluidModelOptions fluids,
        double timeStep)
    {
        var diagonal = new double[world.Grid.CellCount];
        for (int cell = 0; cell < diagonal.Length; cell++)
        {
            double effectiveCompressibility =
                state.OilSaturation[cell] * (fluids.RockCompressibilityPerPa + fluids.OilCompressibilityPerPa) +
                state.WaterSaturation[cell] * (fluids.RockCompressibilityPerPa + fluids.WaterCompressibilityPerPa) +
                state.GasSaturation[cell] * (fluids.RockCompressibilityPerPa + fluids.GasCompressibilityPerPa);
            diagonal[cell] = world.PoreVolumeM3[cell] * effectiveCompressibility / timeStep;
            if (!(diagonal[cell] > 0) || !double.IsFinite(diagonal[cell]))
                throw new SimulationFailureException($"Cell {cell} has invalid pressure storage.");
        }
        return diagonal;
    }

    private static void BuildPressureSystem(
        ReservoirWorld world,
        SimulationState current,
        FluidModelOptions fluids,
        PhaseState phaseState,
        WellPreparation wells,
        double[] storageDiagonal,
        double[] matrixDiagonal,
        byte[] upwind,
        double[] faceConductance,
        double[] rightHandSide,
        double[]? equilibriumFlux)
    {
        int cellCount = world.Grid.CellCount;
        for (int cell = 0; cell < cellCount; cell++)
        {
            double totalSource = 0;
            for (int phase = 0; phase < Phases.Count; phase++)
                totalSource += wells.FixedCellPhaseSourceM3PerSecond[FluidPhysics.Offset(phase, cell, cellCount)];
            matrixDiagonal[cell] = storageDiagonal[cell] + wells.BhpCellConductance[cell];
            rightHandSide[cell] = storageDiagonal[cell] * current.PressurePa[cell] +
                totalSource + wells.BhpCellRightHandSide[cell];
        }

        double maximumFaceConductance = 0;
        for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
        {
            GridFace face = world.Faces[faceIndex];
            double totalConductance = 0;
            double gravityTerm = 0;
            for (int phase = 0; phase < Phases.Count; phase++)
            {
                int upstreamCell = upwind[faceIndex * Phases.Count + phase] == 0 ? face.CellA : face.CellB;
                int offset = FluidPhysics.Offset(phase, upstreamCell, cellCount);
                double conductance = face.GeometricTransmissibility * phaseState.Mobility[offset];
                totalConductance += conductance;
                double faceDensity = 0.5 * (
                    phaseState.DensityKgPerM3[FluidPhysics.Offset(phase, face.CellA, cellCount)] +
                    phaseState.DensityKgPerM3[FluidPhysics.Offset(phase, face.CellB, cellCount)]);
                gravityTerm += conductance * faceDensity *
                    fluids.GravityMPerS2 * face.DepthDifferenceM;
            }
            if (!(totalConductance >= 0) || !double.IsFinite(totalConductance) || !double.IsFinite(gravityTerm))
                throw new SimulationFailureException("A face transmissibility or gravity term became invalid.");
            faceConductance[faceIndex] = totalConductance;
            maximumFaceConductance = Math.Max(maximumFaceConductance, totalConductance);
            rightHandSide[face.CellA] += gravityTerm;
            rightHandSide[face.CellB] -= gravityTerm;
        }
        double couplingThreshold = maximumFaceConductance * NegligibleCouplingRelativeThreshold;
        for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
        {
            if (!(faceConductance[faceIndex] > 0 && faceConductance[faceIndex] < couplingThreshold))
                continue;
            GridFace face = world.Faces[faceIndex];
            double gravityTerm = 0;
            for (int phase = 0; phase < Phases.Count; phase++)
            {
                int upstreamCell = upwind[faceIndex * Phases.Count + phase] == 0 ? face.CellA : face.CellB;
                int offset = FluidPhysics.Offset(phase, upstreamCell, cellCount);
                double conductance = face.GeometricTransmissibility * phaseState.Mobility[offset];
                double faceDensity = 0.5 * (
                    phaseState.DensityKgPerM3[FluidPhysics.Offset(phase, face.CellA, cellCount)] +
                    phaseState.DensityKgPerM3[FluidPhysics.Offset(phase, face.CellB, cellCount)]);
                gravityTerm += conductance * faceDensity * fluids.GravityMPerS2 * face.DepthDifferenceM;
            }
            rightHandSide[face.CellA] -= gravityTerm;
            rightHandSide[face.CellB] += gravityTerm;
            faceConductance[faceIndex] = 0;
        }
        if (equilibriumFlux is not null)
        {
            for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
            {
                if (faceConductance[faceIndex] == 0)
                    continue;
                double baselineTotal = 0;
                for (int phase = 0; phase < Phases.Count; phase++)
                    baselineTotal += equilibriumFlux[faceIndex * Phases.Count + phase];
                GridFace face = world.Faces[faceIndex];
                rightHandSide[face.CellA] += baselineTotal;
                rightHandSide[face.CellB] -= baselineTotal;
            }
        }
    }


    private static bool UpwindSystemsEquivalent(
        ReservoirWorld world, ReadOnlySpan<byte> current, ReadOnlySpan<byte> next,
        ReadOnlySpan<double> mobility, ReadOnlySpan<double> activeFaceConductance)
    {
        int cellCount = world.Grid.CellCount;
        for (int index = 0; index < current.Length; index++)
        {
            if (current[index] == next[index])
                continue;
            if (activeFaceConductance[index / Phases.Count] == 0)
                continue;
            GridFace face = world.Faces[index / Phases.Count];
            int phase = index % Phases.Count;
            int offsetA = FluidPhysics.Offset(phase, face.CellA, cellCount);
            int offsetB = FluidPhysics.Offset(phase, face.CellB, cellCount);
            double mobilityA = mobility[offsetA];
            double mobilityB = mobility[offsetB];
            double mobilityScale = Math.Max(1e-30, Math.Max(mobilityA, mobilityB));
            if (Math.Abs(mobilityA - mobilityB) > 1e-10 * mobilityScale)
                return false;
        }
        return true;
    }


    private static byte[] SelectUpwind(
        ReservoirWorld world,
        ReadOnlySpan<double> pressure,
        ReadOnlySpan<double> density,
        ReadOnlySpan<double> mobility,
        double gravity)
    {
        int cellCount = world.Grid.CellCount;
        var upwind = new byte[world.Faces.Length * Phases.Count];
        for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
        {
            GridFace face = world.Faces[faceIndex];
            for (int phase = 0; phase < Phases.Count; phase++)
            {
                double averageDensity = 0.5 * (
                    density[FluidPhysics.Offset(phase, face.CellA, cellCount)] +
                    density[FluidPhysics.Offset(phase, face.CellB, cellCount)]);
                double potentialDifference = pressure[face.CellA] - pressure[face.CellB] -
                    averageDensity * gravity * face.DepthDifferenceM;
                int offset = faceIndex * Phases.Count + phase;
                double tolerance = PhasePotentialRelativeTolerance * Math.Max(1,
                    Math.Max(Math.Abs(pressure[face.CellA]), Math.Abs(pressure[face.CellB])));
                if (Math.Abs(potentialDifference) <= tolerance)
                {
                    double mobilityA = mobility[FluidPhysics.Offset(phase, face.CellA, cellCount)];
                    double mobilityB = mobility[FluidPhysics.Offset(phase, face.CellB, cellCount)];
                    upwind[offset] = mobilityA <= mobilityB ? (byte)0 : (byte)1;
                }
                else
                {
                    upwind[offset] = potentialDifference >= 0 ? (byte)0 : (byte)1;
                }
            }
        }
        return upwind;
    }

    private static double[] BuildPhaseDivergence(
        ReservoirWorld world,
        ReadOnlySpan<double> pressure,
        PhaseState phaseState,
        ReadOnlySpan<byte> upwind,
        double gravity,
        ReadOnlySpan<double> activeFaceConductance,
        double[]? equilibriumFlux)
    {
        int cellCount = world.Grid.CellCount;
        var divergence = new double[Phases.Count * cellCount];
        for (int faceIndex = 0; faceIndex < world.Faces.Length; faceIndex++)
        {
            if (activeFaceConductance[faceIndex] == 0) continue;
            GridFace face = world.Faces[faceIndex];
            for (int phase = 0; phase < Phases.Count; phase++)
            {
                int upstreamCell = upwind[faceIndex * Phases.Count + phase] == 0 ? face.CellA : face.CellB;
                int upstreamOffset = FluidPhysics.Offset(phase, upstreamCell, cellCount);
                double faceDensity = 0.5 * (
                    phaseState.DensityKgPerM3[FluidPhysics.Offset(phase, face.CellA, cellCount)] +
                    phaseState.DensityKgPerM3[FluidPhysics.Offset(phase, face.CellB, cellCount)]);
                double potentialDifference = pressure[face.CellA] - pressure[face.CellB] -
                    faceDensity * gravity * face.DepthDifferenceM;
                double potentialTolerance = PhasePotentialRelativeTolerance * Math.Max(1,
                    Math.Max(Math.Abs(pressure[face.CellA]), Math.Abs(pressure[face.CellB])));
                double flux = Math.Abs(potentialDifference) <= potentialTolerance
                    ? 0
                    : world.Faces[faceIndex].GeometricTransmissibility *
                        phaseState.Mobility[upstreamOffset] * potentialDifference;
                if (equilibriumFlux is not null)
                    flux -= equilibriumFlux[faceIndex * Phases.Count + phase];
                if (!double.IsFinite(flux))
                    throw new SimulationFailureException("Phase transport produced a nonfinite face flux.");
                divergence[FluidPhysics.Offset(phase, face.CellA, cellCount)] += flux;
                divergence[FluidPhysics.Offset(phase, face.CellB, cellCount)] -= flux;
            }
        }
        return divergence;
    }

    private static double UpdatedSaturation(
        int phase,
        int cell,
        double currentSaturation,
        double pressureChange,
        ReservoirWorld world,
        FluidModelOptions fluids,
        ReadOnlySpan<double> sources,
        ReadOnlySpan<double> divergence,
        double timeStep)
    {
        int offset = FluidPhysics.Offset(phase, cell, world.Grid.CellCount);
        double transport = timeStep * (sources[offset] - divergence[offset]) / world.PoreVolumeM3[cell];
        double pressureStorage = currentSaturation *
            (fluids.RockCompressibilityPerPa + FluidPhysics.Compressibility(phase, fluids)) * pressureChange;
        return currentSaturation + transport - pressureStorage;
    }

    private static void AccumulateCompressibility(
        ReservoirWorld world,
        SimulationState previous,
        SimulationState next,
        FluidModelOptions fluids,
        double[] storage)
    {
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
        {
            double pressureChange = next.PressurePa[cell] - previous.PressurePa[cell];
            storage[Phases.Oil] += world.PoreVolumeM3[cell] * previous.OilSaturation[cell] *
                (fluids.RockCompressibilityPerPa + fluids.OilCompressibilityPerPa) * pressureChange;
            storage[Phases.Water] += world.PoreVolumeM3[cell] * previous.WaterSaturation[cell] *
                (fluids.RockCompressibilityPerPa + fluids.WaterCompressibilityPerPa) * pressureChange;
            storage[Phases.Gas] += world.PoreVolumeM3[cell] * previous.GasSaturation[cell] *
                (fluids.RockCompressibilityPerPa + fluids.GasCompressibilityPerPa) * pressureChange;
        }
    }

    private static double[] Inventory(ReservoirWorld world, SimulationState state)
    {
        var result = new double[Phases.Count];
        for (int cell = 0; cell < world.Grid.CellCount; cell++)
        {
            result[Phases.Oil] += world.PoreVolumeM3[cell] * state.OilSaturation[cell];
            result[Phases.Water] += world.PoreVolumeM3[cell] * state.WaterSaturation[cell];
            result[Phases.Gas] += world.PoreVolumeM3[cell] * state.GasSaturation[cell];
        }
        return result;
    }

    private static MaterialBalanceDiagnostics BuildBalances(
        double[] initial,
        double[] final,
        double[] storage,
        PhaseVolumes injected,
        PhaseVolumes produced)
    {
        double[] injection = [injected.Oil, injected.Water, injected.Gas];
        double[] production = [produced.Oil, produced.Water, produced.Gas];
        double totalInitial = initial.Sum();
        PhaseMaterialBalance Phase(int phase) => BuildPhaseBalance(
            initial[phase],
            final[phase],
            storage[phase],
            injection[phase],
            production[phase],
            totalInitial);

        PhaseMaterialBalance total = BuildPhaseBalance(
            totalInitial,
            final.Sum(),
            storage.Sum(),
            injection.Sum(),
            production.Sum(),
            totalInitial);
        return new MaterialBalanceDiagnostics(Phase(Phases.Oil), Phase(Phases.Water), Phase(Phases.Gas), total);
    }

    private static PhaseMaterialBalance BuildPhaseBalance(
        double initialInPlace,
        double finalInPlace,
        double compressibilityStorage,
        double cumulativeInjected,
        double cumulativeProduced,
        double totalInitialInPlace)
    {
        double error = finalInPlace - initialInPlace + compressibilityStorage -
            cumulativeInjected + cumulativeProduced;
        double absoluteError = Math.Abs(error);
        double reservoirScaleFloor = totalInitialInPlace * ReservoirScaleBalanceFloorFraction;
        double inPlaceFraction = absoluteError / Math.Max(initialInPlace, reservoirScaleFloor);
        double throughput = cumulativeInjected + cumulativeProduced;
        double throughputFraction = throughput > 0
            ? absoluteError / Math.Max(throughput, reservoirScaleFloor)
            : 0;
        return new PhaseMaterialBalance(
            cumulativeInjected, cumulativeProduced, error, inPlaceFraction, throughputFraction);
    }

    private static PhaseVolumes SumVolumes(IEnumerable<PhaseVolumes> values)
    {
        double oil = 0;
        double water = 0;
        double gas = 0;
        foreach (PhaseVolumes value in values)
        {
            oil += value.Oil;
            water += value.Water;
            gas += value.Gas;
        }
        return new PhaseVolumes(oil, water, gas);
    }

    private static double MaximumClosureError(SimulationState state)
    {
        double maximum = 0;
        for (int cell = 0; cell < state.PressurePa.Length; cell++)
            maximum = Math.Max(maximum, Math.Abs(
                state.OilSaturation[cell] + state.WaterSaturation[cell] + state.GasSaturation[cell] - 1));
        return maximum;
    }

    private static SimulationState CloneAndValidateInitialState(ReservoirWorld world, SimulationState state)
    {
        int count = world.Grid.CellCount;
        if (state.PressurePa.Length != count || state.OilSaturation.Length != count ||
            state.WaterSaturation.Length != count || state.GasSaturation.Length != count)
            throw new SimulationFailureException(
                "Continuation state arrays do not match the world grid.");
        var clone = new SimulationState(
            (double[])state.PressurePa.Clone(),
            (double[])state.OilSaturation.Clone(),
            (double[])state.WaterSaturation.Clone(),
            (double[])state.GasSaturation.Clone());
        ValidateFinalState(clone);
        return clone;
    }


    private static void ValidateFinalState(SimulationState state)
    {
        for (int cell = 0; cell < state.PressurePa.Length; cell++)
        {
            if (!double.IsFinite(state.PressurePa[cell]) || state.PressurePa[cell] <= 0 ||
                !double.IsFinite(state.OilSaturation[cell]) || !double.IsFinite(state.WaterSaturation[cell]) ||
                !double.IsFinite(state.GasSaturation[cell]) || state.OilSaturation[cell] < 0 ||
                state.WaterSaturation[cell] < 0 || state.GasSaturation[cell] < 0 ||
                Math.Abs(state.OilSaturation[cell] + state.WaterSaturation[cell] + state.GasSaturation[cell] - 1) > 1e-10)
                throw new SimulationFailureException($"Final state is nonfinite or invalid in cell {cell}.");
        }
    }

    private sealed record StepAttempt(
        SimulationState State,
        WellSampleState[] WellRates,
        double MaximumSaturationChange,
        int CgIterations,
        bool IsPhysical,
        string? FailureReason)
    {
        internal static StepAttempt Invalid(
            SimulationState state,
            WellSampleState[] wellRates,
            int cgIterations,
            string reason) => new(state, wellRates, double.PositiveInfinity, cgIterations, false, reason);
    }

    private sealed class WellHistoryBuilder(
        WellControl well,
        double simulationStartTime,
        double simulationDuration,
        int maximumSamples)
    {
        private readonly List<WellRateSample> _samples = new(maximumSamples);
        private readonly double[] _injected = new double[Phases.Count];
        private readonly double[] _produced = new double[Phases.Count];
        private readonly double[] _rateVolume = new double[Phases.Count];
        private double _bhpVolume;
        private WellControlMode _requestedMode;
        private WellControlMode _effectiveMode;
        private string? _switchReason;
        private readonly double _bucketWidth = simulationDuration / maximumSamples;
        private double _bucketDuration;
        private int _bucketIndex;

        internal string Name => well.Name;
        internal PhaseVolumes CumulativeInjected => new(_injected[0], _injected[1], _injected[2]);
        internal PhaseVolumes CumulativeProduced => new(_produced[0], _produced[1], _produced[2]);

        internal void Accept(double time, double timeStep, WellSampleState sample)
        {
            double cursor = time - timeStep;
            PhaseVolumes rates = sample.Rates;
            double[] values = [rates.Oil, rates.Water, rates.Gas];
            while (cursor < time)
            {
                double bucketEnd = _bucketIndex == maximumSamples - 1
                    ? simulationStartTime + simulationDuration
                    : simulationStartTime + (_bucketIndex + 1) * _bucketWidth;
                if (bucketEnd <= cursor && _bucketIndex < maximumSamples - 1)
                {
                    _bucketIndex++;
                    continue;
                }

                double segmentEnd = Math.Min(time, bucketEnd);
                double segmentDuration = segmentEnd - cursor;
                if (!(segmentDuration > 0))
                    throw new SimulationFailureException("Well response aggregation encountered a nonpositive interval.");
                for (int phase = 0; phase < Phases.Count; phase++)
                {
                    _rateVolume[phase] += values[phase] * segmentDuration;
                    _injected[phase] += Math.Max(0, values[phase]) * segmentDuration;
                    _produced[phase] += Math.Max(0, -values[phase]) * segmentDuration;
                }
                _bhpVolume += sample.BottomHolePressurePa * segmentDuration;
                _requestedMode = sample.RequestedMode;
                _effectiveMode = sample.EffectiveMode;
                _switchReason = sample.SwitchReason ?? _switchReason;
                _bucketDuration += segmentDuration;
                cursor = segmentEnd;

                if (cursor >= bucketEnd)
                {
                    Flush(bucketEnd);
                    if (_bucketIndex < maximumSamples - 1)
                        _bucketIndex++;
                }
            }
        }

        internal WellTimeSeries Build()
        {
            if (_bucketDuration > 0)
                Flush(simulationStartTime + simulationDuration);
            return new WellTimeSeries(
                well.Name, well.I, well.J, well.KStart, well.KEnd, _samples.ToArray(),
                well.Connections.Count > 0 ? well.Connections.Count : well.KEnd - well.KStart + 1);
        }

        private void Flush(double sampleTime)
        {
            if (_samples.Count >= maximumSamples)
                throw new SimulationFailureException(
                    $"Well response exceeded its {maximumSamples}-sample budget.");
            var averageRates = new PhaseVolumes(
                _rateVolume[0] / _bucketDuration,
                _rateVolume[1] / _bucketDuration,
                _rateVolume[2] / _bucketDuration);
            _samples.Add(new WellRateSample(
                _requestedMode, _effectiveMode, _bhpVolume / _bucketDuration, _switchReason,
                sampleTime, _bucketDuration, averageRates, CumulativeInjected, CumulativeProduced));
            Array.Clear(_rateVolume);
            _bhpVolume = 0;
            _switchReason = null;
            _bucketDuration = 0;
        }
    }
}
