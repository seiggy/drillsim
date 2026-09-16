using System.Runtime.CompilerServices;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Simulation;

internal static class Phases
{
    internal const int Oil = 0;
    internal const int Water = 1;
    internal const int Gas = 2;
    internal const int Count = 3;
}

internal sealed record PhaseState(double[] Mobility, double[] DensityKgPerM3);

internal static class FluidPhysics
{
    internal static PhaseState Evaluate(
        ReadOnlySpan<double> pressurePa,
        ReadOnlySpan<double> oilSaturation,
        ReadOnlySpan<double> waterSaturation,
        ReadOnlySpan<double> gasSaturation,
        FluidModelOptions fluids)
    {
        int count = pressurePa.Length;
        var mobility = new double[Phases.Count * count];
        var density = new double[Phases.Count * count];
        double mobileSaturation = 1 - fluids.ResidualOilSaturation -
            fluids.ResidualWaterSaturation - fluids.ResidualGasSaturation;

        for (int cell = 0; cell < count; cell++)
        {
            double oilEffective = Math.Clamp((oilSaturation[cell] - fluids.ResidualOilSaturation) / mobileSaturation, 0, 1);
            double waterEffective = Math.Clamp((waterSaturation[cell] - fluids.ResidualWaterSaturation) / mobileSaturation, 0, 1);
            double gasEffective = Math.Clamp((gasSaturation[cell] - fluids.ResidualGasSaturation) / mobileSaturation, 0, 1);
            mobility[Offset(Phases.Oil, cell, count)] = fluids.OilRelativePermeabilityEndPoint *
                Math.Pow(oilEffective, fluids.OilCoreyExponent) / fluids.OilViscosityPaS;
            mobility[Offset(Phases.Water, cell, count)] = fluids.WaterRelativePermeabilityEndPoint *
                Math.Pow(waterEffective, fluids.WaterCoreyExponent) / fluids.WaterViscosityPaS;
            mobility[Offset(Phases.Gas, cell, count)] = fluids.GasRelativePermeabilityEndPoint *
                Math.Pow(gasEffective, fluids.GasCoreyExponent) / fluids.GasViscosityPaS;

            density[Offset(Phases.Oil, cell, count)] = PressureDensity(
                fluids.OilDensityKgPerM3, fluids.OilCompressibilityPerPa, pressurePa[cell], fluids.ReferencePressurePa);
            density[Offset(Phases.Water, cell, count)] = PressureDensity(
                fluids.WaterDensityKgPerM3, fluids.WaterCompressibilityPerPa, pressurePa[cell], fluids.ReferencePressurePa);
            density[Offset(Phases.Gas, cell, count)] = PressureDensity(
                fluids.GasDensityKgPerM3, fluids.GasCompressibilityPerPa, pressurePa[cell], fluids.ReferencePressurePa);
        }

        if (mobility.Any(value => !double.IsFinite(value) || value < 0) ||
            density.Any(value => !double.IsFinite(value) || value <= 0))
            throw new SimulationFailureException("Fluid property evaluation produced a nonfinite or invalid value.");
        return new PhaseState(mobility, density);
    }

    internal static int Offset(int phase, int cell, int cellCount) => phase * cellCount + cell;

    internal static double Compressibility(int phase, FluidModelOptions fluids) => phase switch
    {
        Phases.Oil => fluids.OilCompressibilityPerPa,
        Phases.Water => fluids.WaterCompressibilityPerPa,
        Phases.Gas => fluids.GasCompressibilityPerPa,
        _ => throw new ArgumentOutOfRangeException(nameof(phase))
    };

    private static double PressureDensity(double referenceDensity, double compressibility, double pressure, double referencePressure) =>
        referenceDensity * Math.Exp(compressibility * (pressure - referencePressure));
}

internal sealed class WellControlSolveException(string message) : InvalidOperationException(message);

internal sealed record WellSampleState(
    WellControlMode RequestedMode,
    WellControlMode EffectiveMode,
    double BottomHolePressurePa,
    PhaseVolumes Rates,
    string? SwitchReason);

internal sealed record PreparedConnection(
    int Cell,
    double WellIndexM3,
    double OilConductance,
    double WaterConductance,
    double GasConductance)
{
    internal double TotalConductance => OilConductance + WaterConductance + GasConductance;
    internal double PhaseConductance(int phase) => phase switch
    {
        Phases.Oil => OilConductance,
        Phases.Water => WaterConductance,
        Phases.Gas => GasConductance,
        _ => throw new ArgumentOutOfRangeException(nameof(phase))
    };
}

internal sealed record PreparedWell(
    WellControl Well,
    WellControlMode EffectiveMode,
    double BottomHolePressurePa,
    string? SwitchReason,
    PreparedConnection[] Connections,
    PhaseVolumes FixedRates);

internal sealed record WellPreparation(
    double[] FixedCellPhaseSourceM3PerSecond,
    double[] BhpCellConductance,
    double[] BhpCellRightHandSide,
    PreparedWell[] Wells);

internal sealed record WellStepAllocation(
    double[] CellPhaseSourceM3PerSecond,
    WellSampleState[] WellSamples);

internal static class PeacemanWellModel
{
    private static readonly ConditionalWeakTable<ReservoirWorld, ConditionalWeakTable<object, ConnectionGeometry[]>> GeometryCache = new();
    private readonly record struct ConnectionGeometry(int Cell, double WellIndex);
    internal static WellPreparation Prepare(
        ReservoirWorld world,
        IReadOnlyList<WellControl> wells,
        ReadOnlySpan<double> mobility,
        ReadOnlySpan<double> pressure)
    {
        int cellCount = world.Grid.CellCount;
        var fixedSources = new double[Phases.Count * cellCount];
        var bhpConductance = new double[cellCount];
        var bhpRightHandSide = new double[cellCount];
        var preparedWells = new PreparedWell[wells.Count];

        for (int wellIndex = 0; wellIndex < wells.Count; wellIndex++)
        {
            WellControl well = wells[wellIndex];
            PreparedConnection[] connections = PrepareConnections(world, well, mobility);
            double totalConductance = connections.Sum(connection => connection.TotalConductance);
            if (!(totalConductance > 0) || !double.IsFinite(totalConductance))
                throw new WellControlSolveException($"Well {well.Name} has no finite mobile connection productivity.");

            (WellControlMode effectiveMode, double bhp, double effectiveRate, string? reason) =
                ResolveControl(well, connections, pressure);
            PhaseVolumes fixedRates = new(0, 0, 0);
            if (effectiveMode == WellControlMode.Rate)
            {
                fixedRates = AllocateFixedRate(
                    well, connections, effectiveRate, fixedSources, cellCount);
            }
            else
            {
                foreach (PreparedConnection connection in connections)
                {
                    bhpConductance[connection.Cell] += connection.TotalConductance;
                    bhpRightHandSide[connection.Cell] += connection.TotalConductance * bhp;
                }
            }
            preparedWells[wellIndex] = new PreparedWell(
                well, effectiveMode, bhp, reason, connections, fixedRates);
        }

        return new WellPreparation(fixedSources, bhpConductance, bhpRightHandSide, preparedWells);
    }

    internal static WellStepAllocation Finalize(
        WellPreparation preparation, ReadOnlySpan<double> pressure, int cellCount)
    {
        var sources = (double[])preparation.FixedCellPhaseSourceM3PerSecond.Clone();
        var samples = new WellSampleState[preparation.Wells.Length];
        for (int wellIndex = 0; wellIndex < preparation.Wells.Length; wellIndex++)
        {
            PreparedWell prepared = preparation.Wells[wellIndex];
            PhaseVolumes rates = prepared.EffectiveMode == WellControlMode.Rate
                ? prepared.FixedRates
                : AllocateBhpRate(prepared, pressure, sources, cellCount);
            double bottomHolePressure = prepared.EffectiveMode == WellControlMode.Rate
                ? BhpForRate(prepared.Connections, pressure, rates.Oil + rates.Water + rates.Gas)
                : prepared.BottomHolePressurePa;
            samples[wellIndex] = new WellSampleState(
                prepared.Well.ControlMode, prepared.EffectiveMode, bottomHolePressure,
                rates, prepared.SwitchReason);
        }
        if (sources.Any(value => !double.IsFinite(value)) ||
            samples.Any(sample => !double.IsFinite(sample.BottomHolePressurePa) || sample.BottomHolePressurePa <= 0 ||
                !double.IsFinite(sample.Rates.Oil) || !double.IsFinite(sample.Rates.Water) ||
                !double.IsFinite(sample.Rates.Gas)))
            throw new WellControlSolveException("Well-control finalization produced a nonfinite rate or BHP.");
        return new WellStepAllocation(sources, samples);
    }

    internal static double ComputeWellIndex(
        double permeabilityXM2,
        double permeabilityYM2,
        double cellSizeXM,
        double cellSizeYM,
        double cellHeightM,
        double wellboreRadiusM,
        double skin,
        double openFraction)
    {
        double kyOverKx = permeabilityYM2 / permeabilityXM2;
        double kxOverKy = permeabilityXM2 / permeabilityYM2;
        double equivalentRadius = 0.28 * Math.Sqrt(
            Math.Sqrt(kyOverKx) * cellSizeXM * cellSizeXM +
            Math.Sqrt(kxOverKy) * cellSizeYM * cellSizeYM) /
            (Math.Pow(kyOverKx, 0.25) + Math.Pow(kxOverKy, 0.25));
        double denominator = Math.Log(equivalentRadius / wellboreRadiusM) + skin;
        if (!(denominator > 0) || !double.IsFinite(denominator))
            throw new WellControlSolveException(
                "Peaceman connection has nonpositive logarithmic radius/skin denominator.");
        double wellIndex = 2 * Math.PI * Math.Sqrt(permeabilityXM2 * permeabilityYM2) *
            cellHeightM * openFraction / denominator;
        if (!(wellIndex > 0) || !double.IsFinite(wellIndex))
            throw new WellControlSolveException("Peaceman well index is nonpositive or nonfinite.");
        return wellIndex;
    }

    private static PreparedConnection[] PrepareConnections(
        ReservoirWorld world, WellControl well, ReadOnlySpan<double> mobility)
    {
        ConnectionGeometry[] geometry;
        if (well.Connections.Count > 0)
        {
            ConditionalWeakTable<object, ConnectionGeometry[]> worldCache = GeometryCache.GetValue(
                world, static _ => new ConditionalWeakTable<object, ConnectionGeometry[]>());
            geometry = worldCache.GetValue(well.Connections, _ => BuildConnectionGeometry(
                world, well.Connections));
        }
        else
        {
            WellConnection[] legacy = Enumerable.Range(well.KStart, well.KEnd - well.KStart + 1)
                .Select(k => new WellConnection { I = well.I, J = well.J, K = k })
                .ToArray();
            geometry = BuildConnectionGeometry(world, legacy);
        }

        int cellCount = world.Grid.CellCount;
        var result = new PreparedConnection[geometry.Length];
        for (int index = 0; index < geometry.Length; index++)
        {
            ConnectionGeometry connection = geometry[index];
            result[index] = new PreparedConnection(
                connection.Cell, connection.WellIndex,
                connection.WellIndex * mobility[FluidPhysics.Offset(Phases.Oil, connection.Cell, cellCount)],
                connection.WellIndex * mobility[FluidPhysics.Offset(Phases.Water, connection.Cell, cellCount)],
                connection.WellIndex * mobility[FluidPhysics.Offset(Phases.Gas, connection.Cell, cellCount)]);
        }
        return result;
    }

    private static ConnectionGeometry[] BuildConnectionGeometry(
        ReservoirWorld world, IReadOnlyList<WellConnection> controls)
    {
        var result = new ConnectionGeometry[controls.Count];
        for (int index = 0; index < controls.Count; index++)
        {
            WellConnection connection = controls[index];
            int cell = world.Grid.CellIndex(connection.I, connection.J, connection.K);
            double wellIndex = ComputeWellIndex(
                world.HorizontalPermeabilityXM2[cell],
                world.HorizontalPermeabilityYM2[cell],
                world.Grid.CellSizeXM,
                world.Grid.CellSizeYM,
                world.CellThicknessM[cell],
                connection.WellboreRadiusM,
                connection.Skin,
                connection.OpenFraction);
            result[index] = new ConnectionGeometry(cell, wellIndex);
        }
        return result;
    }


    private static (WellControlMode Mode, double Bhp, double Rate, string? Reason) ResolveControl(
        WellControl well, PreparedConnection[] connections, ReadOnlySpan<double> pressure)
    {
        double requestedRate;
        double bhp;
        WellControlMode mode = well.ControlMode;
        string? reason = null;
        if (mode == WellControlMode.Rate)
        {
            requestedRate = well.TotalRateM3PerSecond;
            if (well.MaximumAbsoluteRateM3PerSecond is double maximumRate &&
                Math.Abs(requestedRate) > maximumRate)
            {
                requestedRate = Math.CopySign(maximumRate, requestedRate);
                reason = "Maximum rate constraint";
            }
            bhp = BhpForRate(connections, pressure, requestedRate);
        }
        else
        {
            bhp = well.TargetBottomHolePressurePa;
            bhp = ApplyBhpBounds(well, bhp, ref reason);
            requestedRate = RateForBhp(connections, pressure, bhp);
            if (well.MaximumAbsoluteRateM3PerSecond is double maximumRate &&
                Math.Abs(requestedRate) > maximumRate)
            {
                requestedRate = Math.CopySign(maximumRate, requestedRate);
                bhp = BhpForRate(connections, pressure, requestedRate);
                mode = WellControlMode.Rate;
                reason = "Maximum rate constraint";
            }
        }

        double boundedBhp = ApplyBhpBounds(well, bhp, ref reason);
        if (boundedBhp != bhp)
        {
            bhp = boundedBhp;
            requestedRate = RateForBhp(connections, pressure, bhp);
            mode = WellControlMode.Bhp;
        }
        if (!(bhp > 0))
            throw new WellControlSolveException("Effective well control produced a nonpositive BHP.");
        if (!double.IsFinite(requestedRate))
            throw new WellControlSolveException("Effective well control produced a nonfinite rate.");
        return (mode, bhp, requestedRate, reason);
    }

    private static double ApplyBhpBounds(WellControl well, double bhp, ref string? reason)
    {
        if (well.MinimumBottomHolePressurePa is double minimum && bhp < minimum)
        {
            reason = "Minimum BHP constraint";
            return minimum;
        }
        if (well.MaximumBottomHolePressurePa is double maximum && bhp > maximum)
        {
            reason = "Maximum BHP constraint";
            return maximum;
        }
        return bhp;
    }

    private static double BhpForRate(
        PreparedConnection[] connections, ReadOnlySpan<double> pressure, double rate)
    {
        double productivity = 0;
        double pressureTerm = 0;
        foreach (PreparedConnection connection in connections)
        {
            productivity += connection.TotalConductance;
            pressureTerm += connection.TotalConductance * pressure[connection.Cell];
        }
        double bhp = (rate + pressureTerm) / productivity;
        if (!double.IsFinite(bhp))
            throw new WellControlSolveException("Rate control produced an invalid bottom-hole pressure.");
        return bhp;
    }

    private static double RateForBhp(
        PreparedConnection[] connections, ReadOnlySpan<double> pressure, double bhp)
    {
        double rate = 0;
        foreach (PreparedConnection connection in connections)
            rate += connection.TotalConductance * (bhp - pressure[connection.Cell]);
        return rate;
    }


    private static PhaseVolumes AllocateFixedRate(
        WellControl well, PreparedConnection[] connections, double rate, double[] sources, int cellCount)
    {
        var phaseRates = new double[Phases.Count];
        if (rate > 0)
        {
            double productivity = connections.Sum(connection => connection.TotalConductance);
            double[] fractions = InjectionFractions(well);
            foreach (PreparedConnection connection in connections)
            {
                double connectionRate = rate * connection.TotalConductance / productivity;
                for (int phase = 0; phase < Phases.Count; phase++)
                {
                    double phaseRate = connectionRate * fractions[phase];
                    sources[FluidPhysics.Offset(phase, connection.Cell, cellCount)] += phaseRate;
                    phaseRates[phase] += phaseRate;
                }
            }
        }
        else
        {
            double productivity = connections.Sum(connection => connection.TotalConductance);
            foreach (PreparedConnection connection in connections)
            for (int phase = 0; phase < Phases.Count; phase++)
            {
                double phaseRate = rate * connection.PhaseConductance(phase) / productivity;
                sources[FluidPhysics.Offset(phase, connection.Cell, cellCount)] += phaseRate;
                phaseRates[phase] += phaseRate;
            }
        }
        return new PhaseVolumes(phaseRates[0], phaseRates[1], phaseRates[2]);
    }

    private static PhaseVolumes AllocateBhpRate(
        PreparedWell prepared, ReadOnlySpan<double> pressure, double[] sources, int cellCount)
    {
        var rates = new double[Phases.Count];
        double[] injectionFractions = InjectionFractions(prepared.Well);
        foreach (PreparedConnection connection in prepared.Connections)
        {
            double pressureDifference = prepared.BottomHolePressurePa - pressure[connection.Cell];
            if (pressureDifference >= 0)
            {
                double totalRate = connection.TotalConductance * pressureDifference;
                for (int phase = 0; phase < Phases.Count; phase++)
                {
                    double phaseRate = totalRate * injectionFractions[phase];
                    sources[FluidPhysics.Offset(phase, connection.Cell, cellCount)] += phaseRate;
                    rates[phase] += phaseRate;
                }
            }
            else
            {
                for (int phase = 0; phase < Phases.Count; phase++)
                {
                    double phaseRate = connection.PhaseConductance(phase) * pressureDifference;
                    sources[FluidPhysics.Offset(phase, connection.Cell, cellCount)] += phaseRate;
                    rates[phase] += phaseRate;
                }
            }
        }
        return new PhaseVolumes(rates[0], rates[1], rates[2]);
    }

    private static double[] InjectionFractions(WellControl well) =>
    [
        1 - well.InjectionWaterFraction - well.InjectionGasFraction,
        well.InjectionWaterFraction,
        well.InjectionGasFraction
    ];
}
