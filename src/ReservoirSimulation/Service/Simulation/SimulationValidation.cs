using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Simulation;

internal sealed class SimulationFailureException(string message) : InvalidOperationException(message);

internal static class SimulationRequestValidator
{
    internal static void Validate(SimulationRequest? request, ReservoirWorld world)
    {
        var errors = new ValidationErrors();
        if (request is null)
        {
            errors.Add("request", "A request body is required.");
            errors.ThrowIfAny();
            return;
        }

        Positive("durationSeconds", request.DurationSeconds, errors);
        Positive("initialTimeStepSeconds", request.InitialTimeStepSeconds, errors);
        ValidateSolver(request.Solver, request.InitialTimeStepSeconds, errors);
        ValidateFluids(request.Fluids, errors);
        if (request.ContinueFromStateId is not null && !IsStateId(request.ContinueFromStateId))
            errors.Add("continueFromStateId", "Continuation state ID is invalid.");
        ValidateScheduleAndWells(request, world.Grid, errors);
        errors.ThrowIfAny();
    }

    private static void ValidateSolver(SolverOptions? value, double initialTimeStep, ValidationErrors errors)
    {
        if (value is null)
        {
            errors.Add("solver", "Solver options are required.");
            return;
        }

        Positive("solver.minimumTimeStepSeconds", value.MinimumTimeStepSeconds, errors);
        Positive("solver.maximumTimeStepSeconds", value.MaximumTimeStepSeconds, errors);
        if (value.MinimumTimeStepSeconds > value.MaximumTimeStepSeconds)
            errors.Add("solver", "Minimum timestep must not exceed maximum timestep.");
        if (initialTimeStep < value.MinimumTimeStepSeconds || initialTimeStep > value.MaximumTimeStepSeconds)
            errors.Add("initialTimeStepSeconds", "Initial timestep must be within solver minimum and maximum.");
        UnitOpen("solver.maximumSaturationChange", value.MaximumSaturationChange, errors);
        errors.RequireFinite("solver.growthSaturationChange", value.GrowthSaturationChange);
        if (value.GrowthSaturationChange < 0 || value.GrowthSaturationChange >= value.MaximumSaturationChange)
            errors.Add("solver.growthSaturationChange", "Growth saturation change must be nonnegative and below the rejection threshold.");
        errors.RequireFinite("solver.timeStepGrowthFactor", value.TimeStepGrowthFactor);
        if (value.TimeStepGrowthFactor <= 1)
            errors.Add("solver.timeStepGrowthFactor", "Timestep growth factor must exceed one.");
        errors.RequireFinite("solver.timeStepShrinkFactor", value.TimeStepShrinkFactor);
        if (!(value.TimeStepShrinkFactor > 0 && value.TimeStepShrinkFactor < 1))
            errors.Add("solver.timeStepShrinkFactor", "Timestep shrink factor must be between zero and one.");
        UnitOpen("solver.cgRelativeTolerance", value.CgRelativeTolerance, errors);
        if (value.CgRelativeTolerance > 1e-6)
            errors.Add("solver.cgRelativeTolerance", "CG relative tolerance must not exceed 1e-6.");
        if (value.CgMaximumIterations <= 0)
            errors.Add("solver.cgMaximumIterations", "CG maximum iterations must be positive.");
        if (value.MaximumStepAttempts is < 1 or > 100_000)
            errors.Add("solver.maximumStepAttempts", "Maximum step attempts must be between 1 and 100,000.");
        if (value.MaximumSamplesPerWell is < 1 or > 10_000)
            errors.Add("solver.maximumSamplesPerWell", "Maximum samples per well must be between 1 and 10,000.");
    }

    private static void ValidateFluids(FluidModelOptions? value, ValidationErrors errors)
    {
        if (value is null)
        {
            errors.Add("fluids", "Fluid model options are required.");
            return;
        }

        Positive("fluids.referencePressurePa", value.ReferencePressurePa, errors);
        Positive("fluids.oilDensityKgPerM3", value.OilDensityKgPerM3, errors);
        Positive("fluids.waterDensityKgPerM3", value.WaterDensityKgPerM3, errors);
        Positive("fluids.gasDensityKgPerM3", value.GasDensityKgPerM3, errors);
        Positive("fluids.oilViscosityPaS", value.OilViscosityPaS, errors);
        Positive("fluids.waterViscosityPaS", value.WaterViscosityPaS, errors);
        Positive("fluids.gasViscosityPaS", value.GasViscosityPaS, errors);
        Nonnegative("fluids.oilCompressibilityPerPa", value.OilCompressibilityPerPa, errors);
        Nonnegative("fluids.waterCompressibilityPerPa", value.WaterCompressibilityPerPa, errors);
        Nonnegative("fluids.gasCompressibilityPerPa", value.GasCompressibilityPerPa, errors);
        Nonnegative("fluids.rockCompressibilityPerPa", value.RockCompressibilityPerPa, errors);
        if (value.RockCompressibilityPerPa + Math.Min(value.OilCompressibilityPerPa,
                Math.Min(value.WaterCompressibilityPerPa, value.GasCompressibilityPerPa)) <= 0)
            errors.Add("fluids", "Every possible phase state must have positive pressure storage.");
        Residual("fluids.residualOilSaturation", value.ResidualOilSaturation, errors);
        Residual("fluids.residualWaterSaturation", value.ResidualWaterSaturation, errors);
        Residual("fluids.residualGasSaturation", value.ResidualGasSaturation, errors);
        if (value.ResidualOilSaturation + value.ResidualWaterSaturation + value.ResidualGasSaturation >= 1)
            errors.Add("fluids", "Residual saturations must sum to less than one.");
        Positive("fluids.oilCoreyExponent", value.OilCoreyExponent, errors);
        Positive("fluids.waterCoreyExponent", value.WaterCoreyExponent, errors);
        Positive("fluids.gasCoreyExponent", value.GasCoreyExponent, errors);
        UnitClosed("fluids.oilRelativePermeabilityEndPoint", value.OilRelativePermeabilityEndPoint, errors);
        UnitClosed("fluids.waterRelativePermeabilityEndPoint", value.WaterRelativePermeabilityEndPoint, errors);
        UnitClosed("fluids.gasRelativePermeabilityEndPoint", value.GasRelativePermeabilityEndPoint, errors);
        Nonnegative("fluids.gravityMPerS2", value.GravityMPerS2, errors);
    }

    private static void ValidateScheduleAndWells(
        SimulationRequest request, GridGeometry grid, ValidationErrors errors)
    {
        if (request.Schedule is null)
        {
            errors.Add("schedule", "Schedule collection is required; use an empty collection for legacy wells.");
            return;
        }
        if (request.Schedule.Count == 0)
        {
            ValidateWells(request.Wells, grid, "wells", errors);
            return;
        }
        if (request.Wells is null || request.Wells.Count > 0)
            errors.Add("wells", "Top-level wells must be empty when an explicit schedule is supplied.");

        double expectedStart = 0;
        double tolerance = 1e-10 * Math.Max(1, request.DurationSeconds);
        for (int index = 0; index < request.Schedule.Count; index++)
        {
            ScheduleSegment? segment = request.Schedule[index];
            string key = $"schedule[{index}]";
            if (segment is null)
            {
                errors.Add(key, "Schedule segment must not be null.");
                continue;
            }
            errors.RequireFinite($"{key}.startTimeSeconds", segment.StartTimeSeconds);
            errors.RequireFinite($"{key}.durationSeconds", segment.DurationSeconds);
            if (segment.StartTimeSeconds < 0)
                errors.Add($"{key}.startTimeSeconds", "Schedule start must be nonnegative.");
            if (!(segment.DurationSeconds > 0))
                errors.Add($"{key}.durationSeconds", "Schedule duration must be positive.");
            if (Math.Abs(segment.StartTimeSeconds - expectedStart) > tolerance)
                errors.Add($"{key}.startTimeSeconds",
                    "Schedule segments must be ordered and have no overlaps or gaps.");
            ValidateWells(segment.Wells, grid, $"{key}.wells", errors);
            if (double.IsFinite(segment.StartTimeSeconds) && double.IsFinite(segment.DurationSeconds))
                expectedStart = segment.StartTimeSeconds + segment.DurationSeconds;
        }
        if (Math.Abs(expectedStart - request.DurationSeconds) > tolerance)
            errors.Add("schedule", "Schedule segments must exactly cover DurationSeconds.");
    }

    private static void ValidateWells(
        IReadOnlyList<WellControl>? wells, GridGeometry grid, string collectionKey, ValidationErrors errors)
    {
        if (wells is null)
        {
            errors.Add(collectionKey, "Well collection is required; use an empty collection for no wells.");
            return;
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < wells.Count; index++)
        {
            WellControl? well = wells[index];
            string key = $"{collectionKey}[{index}]";
            if (well is null)
            {
                errors.Add(key, "Well must not be null.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(well.Name))
                errors.Add($"{key}.name", "Well name is required.");
            else if (!names.Add(well.Name))
                errors.Add($"{key}.name", "Well names must be unique within a schedule segment.");
            if (!Enum.IsDefined(well.ControlMode))
                errors.Add($"{key}.controlMode", "Well control mode is invalid.");
            errors.RequireFinite($"{key}.totalRateM3PerSecond", well.TotalRateM3PerSecond);
            errors.RequireFinite($"{key}.targetBottomHolePressurePa", well.TargetBottomHolePressurePa);
            if (well.ControlMode == WellControlMode.Rate && well.TotalRateM3PerSecond == 0)
                errors.Add($"{key}.totalRateM3PerSecond", "Rate-control target must be nonzero.");
            if (well.ControlMode == WellControlMode.Bhp && !(well.TargetBottomHolePressurePa > 0))
                errors.Add($"{key}.targetBottomHolePressurePa", "BHP-control target must be positive.");
            ValidateOptionalPositive($"{key}.minimumBottomHolePressurePa", well.MinimumBottomHolePressurePa, errors);
            ValidateOptionalPositive($"{key}.maximumBottomHolePressurePa", well.MaximumBottomHolePressurePa, errors);
            ValidateOptionalPositive($"{key}.maximumAbsoluteRateM3PerSecond", well.MaximumAbsoluteRateM3PerSecond, errors);
            if (well.MinimumBottomHolePressurePa is double minimum &&
                well.MaximumBottomHolePressurePa is double maximum && minimum > maximum)
                errors.Add(key, "Minimum BHP must not exceed maximum BHP.");
            UnitClosed($"{key}.injectionWaterFraction", well.InjectionWaterFraction, errors);
            UnitClosed($"{key}.injectionGasFraction", well.InjectionGasFraction, errors);
            if (well.InjectionWaterFraction + well.InjectionGasFraction > 1)
                errors.Add(key, "Injection water and gas fractions must not sum above one.");

            if (well.Connections is null)
            {
                errors.Add($"{key}.connections", "Connection collection is required.");
                continue;
            }
            if (well.Connections.Count == 0)
                ValidateLegacyCompletion(well, grid, key, errors);
            else
                ValidateConnections(well.Connections, grid, key, errors);
        }
    }

    private static void ValidateLegacyCompletion(
        WellControl well, GridGeometry grid, string key, ValidationErrors errors)
    {
        if (well.I < 0 || well.I >= grid.CountX) errors.Add($"{key}.i", "I index is outside the grid.");
        if (well.J < 0 || well.J >= grid.CountY) errors.Add($"{key}.j", "J index is outside the grid.");
        if (well.KStart < 0 || well.KStart >= grid.CountZ) errors.Add($"{key}.kStart", "K start is outside the grid.");
        if (well.KEnd < well.KStart || well.KEnd >= grid.CountZ)
            errors.Add($"{key}.kEnd", "K end must be at or after K start and inside the grid.");
    }

    private static void ValidateConnections(
        IReadOnlyList<WellConnection> connections, GridGeometry grid, string wellKey, ValidationErrors errors)
    {
        var cells = new HashSet<(int I, int J, int K)>();
        for (int index = 0; index < connections.Count; index++)
        {
            WellConnection? connection = connections[index];
            string key = $"{wellKey}.connections[{index}]";
            if (connection is null)
            {
                errors.Add(key, "Well connection must not be null.");
                continue;
            }
            if (connection.I < 0 || connection.I >= grid.CountX) errors.Add($"{key}.i", "I index is outside the grid.");
            if (connection.J < 0 || connection.J >= grid.CountY) errors.Add($"{key}.j", "J index is outside the grid.");
            if (connection.K < 0 || connection.K >= grid.CountZ) errors.Add($"{key}.k", "K index is outside the grid.");
            if (!cells.Add((connection.I, connection.J, connection.K)))
                errors.Add(key, "A well may have only one connection per grid cell.");
            errors.RequireFinite($"{key}.wellboreRadiusM", connection.WellboreRadiusM);
            if (!(connection.WellboreRadiusM > 0 &&
                connection.WellboreRadiusM < 0.5 * Math.Min(grid.CellSizeXM, grid.CellSizeYM)))
                errors.Add($"{key}.wellboreRadiusM", "Wellbore radius must be positive and below half a horizontal cell.");
            errors.RequireFinite($"{key}.skin", connection.Skin);
            if (connection.Skin is < -10 or > 100)
                errors.Add($"{key}.skin", "Skin must be in [-10, 100].");
            errors.RequireFinite($"{key}.openFraction", connection.OpenFraction);
            if (!(connection.OpenFraction > 0 && connection.OpenFraction <= 1))
                errors.Add($"{key}.openFraction", "Open fraction must be in (0, 1].");
        }
    }

    private static void ValidateOptionalPositive(string key, double? value, ValidationErrors errors)
    {
        if (value is not double actual)
            return;
        errors.RequireFinite(key, actual);
        if (!(actual > 0))
            errors.Add(key, "Constraint must be positive.");
    }


    private static void Positive(string key, double value, ValidationErrors errors)
    {
        errors.RequireFinite(key, value);
        if (!(value > 0)) errors.Add(key, "Value must be positive.");
    }

    private static void Nonnegative(string key, double value, ValidationErrors errors)
    {
        errors.RequireFinite(key, value);
        if (value < 0) errors.Add(key, "Value must be nonnegative.");
    }

    private static void Residual(string key, double value, ValidationErrors errors) => UnitClosed(key, value, errors);

    private static void UnitOpen(string key, double value, ValidationErrors errors)
    {
        errors.RequireFinite(key, value);
        if (!(value > 0 && value < 1)) errors.Add(key, "Value must be between zero and one.");
    }

    private static void UnitClosed(string key, double value, ValidationErrors errors)
    {
        errors.RequireFinite(key, value);
        if (!(value >= 0 && value <= 1)) errors.Add(key, "Value must be in [0, 1].");
    }

    private static bool IsStateId(string value)
    {
        if (value.Length != 68 || !value.StartsWith("rss_", StringComparison.Ordinal))
            return false;
        return value.AsSpan(4).ToString().All(
            character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
    }
}
