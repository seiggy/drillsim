using System.Text.Json;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Domain;

internal interface IReservoirWorldFactory
{
    ReservoirWorld Create(WorldGenerationRequest request);
}

internal sealed class ReservoirWorldFactory : IReservoirWorldFactory
{
    internal const string ModelVersion = "reservoir-hidden-world-v3";

    public ReservoirWorld Create(WorldGenerationRequest request)
    {
        WorldRequestValidator.Validate(request);
        request = CanonicalizeValidated(request);
        ConditioningPoint[] propertyControls = request.ConditioningPoints.ToArray();
        StructuralConditioningPoint[] structuralControls = request.StructuralConditioningPoints.ToArray();
        string canonicalJson = JsonSerializer.Serialize(request, DeterministicEncoding.JsonOptions);
        string worldId = CreateWorldId(canonicalJson);
        GridGeometry grid = CreateGrid(request.Grid, propertyControls, structuralControls);
        HeterogeneityOptions options = request.Heterogeneity;
        var topNoise = Noise(request.Seed, 1, options);
        var baseNoise = Noise(request.Seed, 2, options);
        var porosityNoise = Noise(request.Seed, 3, options);
        var permeabilityNoise = Noise(request.Seed, 4, options);
        var pressureNoise = Noise(request.Seed, 5, options);
        var waterNoise = Noise(request.Seed, 6, options);
        var gasNoise = Noise(request.Seed, 7, options);
        var netToGrossNoise = Noise(request.Seed, 8, options);
        bool hasContacts = HasContacts(request.FluidContacts);

        var top = new double[grid.ColumnCount];
        var baseDepth = new double[grid.ColumnCount];
        for (int j = 0; j < grid.CountY; j++)
        for (int i = 0; i < grid.CountX; i++)
        {
            int column = grid.ColumnIndex(i, j);
            double easting = grid.Easting(i);
            double northing = grid.Northing(j);
            double x = easting - grid.OriginEastingM;
            double y = northing - grid.OriginNorthingM;
            double fade = StructuralFade(request, easting, northing, options.ControlFadeDistanceM);
            double topMean = StructuralValue(request, easting, northing, topValue: true);
            double baseMean = StructuralValue(request, easting, northing, topValue: false);
            top[column] = topMean + fade * options.TopDepthStdDevM * topNoise.At(x, y, 0);
            double rawBase = baseMean + fade * options.BaseDepthStdDevM * baseNoise.At(x, y, 0);
            double minimumThickness = Math.Max(0.1, 0.05 * (baseMean - topMean));
            baseDepth[column] = Math.Max(rawBase, top[column] + minimumThickness);
            ValidateContactOrderingAt(request.FluidContacts, easting, northing, options.IdwPower);
        }

        var propertyMeansByColumn = new PropertyMeans[grid.ColumnCount];
        var propertyFadeByColumn = new double[grid.ColumnCount];
        for (int j = 0; j < grid.CountY; j++)
        for (int i = 0; i < grid.CountX; i++)
        {
            int column = grid.ColumnIndex(i, j);
            double easting = grid.Easting(i);
            double northing = grid.Northing(j);
            propertyMeansByColumn[column] = InterpolateProperties(
                propertyControls, easting, northing, options.IdwPower);
            propertyFadeByColumn[column] = PropertyFade(
                propertyControls, easting, northing, options.ControlFadeDistanceM);
        }


        int count = grid.CellCount;
        var depth = new double[count];
        var thickness = new double[count];
        var netToGross = new double[count];
        var porosity = new double[count];
        var logPermeability = new double[count];
        var pressure = new double[count];
        var oil = new double[count];
        var water = new double[count];
        var gas = new double[count];

        Parallel.For(0, count, cell =>
        {
            int i = cell % grid.CountX;
            int planeIndex = cell / grid.CountX;
            int j = planeIndex % grid.CountY;
            int k = planeIndex / grid.CountY;
            int column = grid.ColumnIndex(i, j);
            double easting = grid.Easting(i);
            double northing = grid.Northing(j);
            double x = easting - grid.OriginEastingM;
            double y = northing - grid.OriginNorthingM;
            double dz = (baseDepth[column] - top[column]) / grid.CountZ;
            double cellDepth = top[column] + (k + 0.5) * dz;
            double z = cellDepth - top[column];
            double fade = propertyFadeByColumn[column];
            PropertyMeans means = propertyMeansByColumn[column];
            depth[cell] = cellDepth;
            thickness[cell] = dz;

            double reservoirPorosity = Math.Clamp(
                means.Porosity + fade * options.PorosityStdDev * porosityNoise.At(x, y, z),
                1e-6, 1 - 1e-6);
            double reservoirLogPermeability = Math.Clamp(
                means.LogPermeability + fade * options.LogPermeabilityStdDev * permeabilityNoise.At(x, y, z),
                Math.Log(1e-22), Math.Log(1e-8));
            double localNetToGross = Math.Clamp(
                means.NetToGross +
                fade * options.NetToGrossStdDev * netToGrossNoise.At(x, y, z), 0, 1);
            netToGross[cell] = localNetToGross;
            porosity[cell] = localNetToGross * reservoirPorosity +
                (1 - localNetToGross) * options.ShalePorosity;
            logPermeability[cell] = localNetToGross * reservoirLogPermeability +
                (1 - localNetToGross) * Math.Log(options.ShalePermeabilityM2);

            pressure[cell] = hasContacts
                ? means.PressurePa
                : Math.Clamp(means.PressurePa +
                    fade * options.PressureStdDevPa * pressureNoise.At(x, y, z), 1, 1e10);

            if (hasContacts)
            {
                (oil[cell], water[cell], gas[cell]) = ContactSaturations(
                    request.FluidContacts, easting, northing, cellDepth, options.IdwPower);
            }
            else
            {
                LegacySaturations(means, options, waterNoise, gasNoise,
                    x, y, z, fade, out oil[cell], out water[cell], out gas[cell]);
            }
        });

        if (hasContacts)
            InitializeHydrostaticPressure(grid, propertyControls, depth, oil, water, gas, pressure, options.IdwPower);

        var summary = new WorldSummary(
            worldId,
            request.FieldId,
            request.ReservoirName,
            "Separately conditioned structure and NTG-weighted hidden reservoir properties",
            ModelVersion,
            request.CalibrationArtifact,
            new WorldGridSummary(grid.CountX, grid.CountY, grid.CountZ, grid.CellCount),
            propertyControls.Length,
            structuralControls.Length);

        return new ReservoirWorld(summary, hasContacts, grid, top, baseDepth, depth, thickness, netToGross, porosity,
            logPermeability, pressure, oil, water, gas);
    }

    internal static string CanonicalRequestJson(WorldGenerationRequest request)
    {
        WorldRequestValidator.Validate(request);
        return JsonSerializer.Serialize(CanonicalizeValidated(request), DeterministicEncoding.JsonOptions);
    }

    internal static WorldGenerationRequest RequestFromCanonicalJson(string json)
    {
        WorldGenerationRequest request = JsonSerializer.Deserialize<WorldGenerationRequest>(
            json, DeterministicEncoding.JsonOptions)
            ?? throw new InvalidDataException("Canonical world request JSON is null.");
        WorldRequestValidator.Validate(request);
        return request;
    }

    internal static string TruthChecksum(ReservoirWorld world) => DeterministicEncoding.ArrayChecksum(
        world.TopDepthM,
        world.BaseDepthM,
        world.CellDepthM,
        world.CellThicknessM,
        world.NetToGross,
        world.Porosity,
        world.LogPermeability,
        world.PressurePa,
        world.OilSaturation,
        world.WaterSaturation,
        world.GasSaturation);

    private static WorldGenerationRequest CanonicalizeValidated(WorldGenerationRequest request)
    {
        ConditioningPoint[] propertyControls = request.ConditioningPoints
            .OrderBy(point => point.EastingM)
            .ThenBy(point => point.NorthingM)
            .ThenBy(point => point.ReservoirTopDepthM)
            .ThenBy(point => point.ReservoirBaseDepthM)
            .ThenBy(point => point.Porosity)
            .ThenBy(point => point.PermeabilityM2)
            .ThenBy(point => point.PressurePa)
            .ThenBy(point => point.WaterSaturation)
            .ThenBy(point => point.GasSaturation)
            .ThenBy(point => point.NetToGross)
            .ToArray();
        StructuralConditioningPoint[] structuralControls = request.StructuralConditioningPoints
            .OrderBy(point => point.EastingM)
            .ThenBy(point => point.NorthingM)
            .ThenBy(point => point.ReservoirTopDepthM)
            .ThenBy(point => point.ReservoirBaseDepthM)
            .ToArray();
        FluidContactOptions contacts = request.FluidContacts with
        {
            GasWater = SortContacts(request.FluidContacts.GasWater),
            GasOil = SortContacts(request.FluidContacts.GasOil),
            OilWater = SortContacts(request.FluidContacts.OilWater)
        };
        return request with
        {
            ConditioningPoints = propertyControls,
            StructuralConditioningPoints = structuralControls,
            FluidContacts = contacts
        };
    }

    private static FluidContactPoint[] SortContacts(IReadOnlyList<FluidContactPoint> contacts) => contacts
        .OrderBy(point => point.EastingM)
        .ThenBy(point => point.NorthingM)
        .ThenBy(point => point.ContactDepthTvdM)
        .ToArray();

    private static GridGeometry CreateGrid(
        GridOptions options,
        ConditioningPoint[] propertyControls,
        StructuralConditioningPoint[] structuralControls)
    {
        double[] eastings = propertyControls.Select(point => point.EastingM)
            .Concat(structuralControls.Select(point => point.EastingM)).ToArray();
        double[] northings = propertyControls.Select(point => point.NorthingM)
            .Concat(structuralControls.Select(point => point.NorthingM)).ToArray();
        double minimumEasting = eastings.Min() - options.HorizontalPaddingM;
        double maximumEasting = eastings.Max() + options.HorizontalPaddingM;
        double minimumNorthing = northings.Min() - options.HorizontalPaddingM;
        double maximumNorthing = northings.Max() + options.HorizontalPaddingM;
        return new GridGeometry(
            options.CountX, options.CountY, options.CountZ,
            minimumEasting, minimumNorthing,
            (maximumEasting - minimumEasting) / options.CountX,
            (maximumNorthing - minimumNorthing) / options.CountY);
    }

    private static double StructuralValue(
        WorldGenerationRequest request, double easting, double northing, bool topValue)
    {
        if (request.StructuralConditioningPoints.Count > 0)
            return InterpolateStructure(request.StructuralConditioningPoints, easting, northing,
                point => topValue ? point.ReservoirTopDepthM : point.ReservoirBaseDepthM,
                request.Heterogeneity.IdwPower);
        return InterpolateProperty(request.ConditioningPoints, easting, northing,
            point => topValue ? point.ReservoirTopDepthM : point.ReservoirBaseDepthM,
            request.Heterogeneity.IdwPower);
    }

    private static double StructuralFade(
        WorldGenerationRequest request, double easting, double northing, double fadeDistanceM)
    {
        IEnumerable<(double Easting, double Northing)> coordinates = request.StructuralConditioningPoints.Count > 0
            ? request.StructuralConditioningPoints.Select(point => (point.EastingM, point.NorthingM))
            : request.ConditioningPoints.Select(point => (point.EastingM, point.NorthingM));
        return ControlFade(coordinates, easting, northing, fadeDistanceM);
    }

    private static void LegacySaturations(
        PropertyMeans means,
        HeterogeneityOptions options,
        CorrelatedSpectralNoise waterNoise,
        CorrelatedSpectralNoise gasNoise,
        double x, double y, double z, double fade,
        out double oil, out double water, out double gas)
    {
        water = Math.Clamp(
            means.WaterSaturation + fade * options.WaterSaturationStdDev * waterNoise.At(x, y, z), 0, 1);
        gas = Math.Clamp(
            means.GasSaturation + fade * options.GasSaturationStdDev * gasNoise.At(x, y, z), 0, 1);
        double sum = water + gas;
        if (sum > 1)
        {
            water /= sum;
            gas /= sum;
        }
        oil = 1 - water - gas;
    }


    private static (double Oil, double Water, double Gas) ContactSaturations(
        FluidContactOptions contacts, double easting, double northing, double depth, double power)
    {
        const double connateWater = 0.15;
        const double hydrocarbonSaturation = 0.85;
        if (contacts.GasWater.Count > 0)
        {
            double contact = WorldRequestValidator.InterpolateContact(contacts.GasWater, easting, northing, power);
            double gasWeight = AboveFraction(depth, contact, contacts.TransitionThicknessM);
            double waterWeight = 1 - gasWeight;
            return (0, connateWater * gasWeight + waterWeight, hydrocarbonSaturation * gasWeight);
        }

        double gasZone = contacts.GasOil.Count > 0
            ? AboveFraction(depth, WorldRequestValidator.InterpolateContact(
                contacts.GasOil, easting, northing, power), contacts.TransitionThicknessM)
            : 0;
        double waterZone = contacts.OilWater.Count > 0
            ? 1 - AboveFraction(depth, WorldRequestValidator.InterpolateContact(
                contacts.OilWater, easting, northing, power), contacts.TransitionThicknessM)
            : 0;
        if (gasZone + waterZone > 1)
        {
            double scale = gasZone + waterZone;
            gasZone /= scale;
            waterZone /= scale;
        }
        double oilZone = 1 - gasZone - waterZone;
        return (
            hydrocarbonSaturation * oilZone,
            connateWater * (gasZone + oilZone) + waterZone,
            hydrocarbonSaturation * gasZone);
    }

    private static double AboveFraction(double depth, double contactDepth, double transitionThickness)
    {
        if (transitionThickness == 0)
            return depth < contactDepth ? 1 : 0;
        double normalized = Math.Clamp(
            (depth - (contactDepth - 0.5 * transitionThickness)) / transitionThickness, 0, 1);
        double smooth = normalized * normalized * (3 - 2 * normalized);
        return 1 - smooth;
    }

    private static void InitializeHydrostaticPressure(
        GridGeometry grid,
        ConditioningPoint[] controls,
        double[] depth,
        double[] oil,
        double[] water,
        double[] gas,
        double[] pressure,
        double power)
    {
        var fluids = new FluidModelOptions();
        double referencePressure = controls.Average(control => control.PressurePa);
        double datumDepth = depth.AsSpan(0, grid.ColumnCount).ToArray().Min();
        for (int j = 0; j < grid.CountY; j++)
        for (int i = 0; i < grid.CountX; i++)
        {
            int first = grid.CellIndex(i, j, 0);
            pressure[first] = HydrostaticPressureBelow(
                referencePressure, depth[first] - datumDepth,
                oil[first], water[first], gas[first],
                oil[first], water[first], gas[first], fluids);
            for (int k = 1; k < grid.CountZ; k++)
            {
                int above = grid.CellIndex(i, j, k - 1);
                int below = grid.CellIndex(i, j, k);
                pressure[below] = HydrostaticPressureBelow(
                    pressure[above], depth[below] - depth[above],
                    oil[above], water[above], gas[above],
                    oil[below], water[below], gas[below], fluids);
            }
        }
    }

    private readonly record struct HydrostaticPhaseData(
        int Phase,
        double DensityAbove,
        double MobilityAbove,
        double MobilityBelow);

    private static double HydrostaticPressureBelow(
        double pressureAbove, double depthDifference,
        double oilAbove, double waterAbove, double gasAbove,
        double oilBelow, double waterBelow, double gasBelow,
        FluidModelOptions fluids)
    {
        var oil = HydrostaticData(Phases.Oil, oilAbove, oilBelow, pressureAbove, fluids);
        var water = HydrostaticData(Phases.Water, waterAbove, waterBelow, pressureAbove, fluids);
        var gas = HydrostaticData(Phases.Gas, gasAbove, gasBelow, pressureAbove, fluids);
        double lower = 0;
        double maximumReferenceDensity = Math.Max(fluids.WaterDensityKgPerM3,
            Math.Max(fluids.OilDensityKgPerM3, fluids.GasDensityKgPerM3));
        double upper = 2 * maximumReferenceDensity * fluids.GravityMPerS2 * depthDifference;
        double upperFlux = HydrostaticFlux(
            upper, pressureAbove, depthDifference, oil, water, gas, fluids);
        for (int expansion = 0; upperFlux > 0 && expansion < 8; expansion++)
        {
            upper *= 2;
            upperFlux = HydrostaticFlux(
                upper, pressureAbove, depthDifference, oil, water, gas, fluids);
        }
        if (upperFlux > 0)
            throw new InvalidOperationException("Unable to bracket mobility-weighted hydrostatic pressure.");

        for (int iteration = 0; iteration < 80; iteration++)
        {
            double midpoint = 0.5 * (lower + upper);
            double flux = HydrostaticFlux(
                midpoint, pressureAbove, depthDifference, oil, water, gas, fluids);
            if (flux > 0)
                lower = midpoint;
            else
                upper = midpoint;
        }
        return pressureAbove + 0.5 * (lower + upper);
    }

    private static HydrostaticPhaseData HydrostaticData(
        int phase, double saturationAbove, double saturationBelow,
        double pressureAbove, FluidModelOptions fluids) => new(
        phase,
        PhaseDensity(phase, pressureAbove, fluids),
        PhaseMobility(phase, saturationAbove, fluids),
        PhaseMobility(phase, saturationBelow, fluids));

    private static double HydrostaticFlux(
        double pressureDifference, double pressureAbove, double depthDifference,
        HydrostaticPhaseData oil, HydrostaticPhaseData water, HydrostaticPhaseData gas,
        FluidModelOptions fluids)
    {
        double pressureBelow = pressureAbove + pressureDifference;
        return HydrostaticPhaseFlux(oil, pressureBelow, pressureDifference, depthDifference, fluids) +
               HydrostaticPhaseFlux(water, pressureBelow, pressureDifference, depthDifference, fluids) +
               HydrostaticPhaseFlux(gas, pressureBelow, pressureDifference, depthDifference, fluids);
    }

    private static double HydrostaticPhaseFlux(
        HydrostaticPhaseData phase, double pressureBelow,
        double pressureDifference, double depthDifference, FluidModelOptions fluids)
    {
        double densityBelow = PhaseDensity(phase.Phase, pressureBelow, fluids);
        double faceDensity = 0.5 * (phase.DensityAbove + densityBelow);
        double potential = -pressureDifference + faceDensity * fluids.GravityMPerS2 * depthDifference;
        double mobility = potential >= 0 ? phase.MobilityAbove : phase.MobilityBelow;
        return mobility * potential;
    }


    private static double PhaseMobility(int phase, double saturation, FluidModelOptions fluids)
    {
        double mobile = 1 - fluids.ResidualOilSaturation - fluids.ResidualWaterSaturation -
            fluids.ResidualGasSaturation;
        double residual = phase switch
        {
            Phases.Oil => fluids.ResidualOilSaturation,
            Phases.Water => fluids.ResidualWaterSaturation,
            _ => fluids.ResidualGasSaturation
        };
        double endpoint = phase switch
        {
            Phases.Oil => fluids.OilRelativePermeabilityEndPoint,
            Phases.Water => fluids.WaterRelativePermeabilityEndPoint,
            _ => fluids.GasRelativePermeabilityEndPoint
        };
        double exponent = phase switch
        {
            Phases.Oil => fluids.OilCoreyExponent,
            Phases.Water => fluids.WaterCoreyExponent,
            _ => fluids.GasCoreyExponent
        };
        double viscosity = phase switch
        {
            Phases.Oil => fluids.OilViscosityPaS,
            Phases.Water => fluids.WaterViscosityPaS,
            _ => fluids.GasViscosityPaS
        };
        double effective = Math.Clamp((saturation - residual) / mobile, 0, 1);
        return endpoint * Math.Pow(effective, exponent) / viscosity;
    }

    private static double PhaseDensity(int phase, double pressure, FluidModelOptions fluids)
    {
        (double referenceDensity, double compressibility) = phase switch
        {
            Phases.Oil => (fluids.OilDensityKgPerM3, fluids.OilCompressibilityPerPa),
            Phases.Water => (fluids.WaterDensityKgPerM3, fluids.WaterCompressibilityPerPa),
            _ => (fluids.GasDensityKgPerM3, fluids.GasCompressibilityPerPa)
        };
        return referenceDensity * Math.Exp(compressibility * (pressure - fluids.ReferencePressurePa));
    }

    private static void ValidateContactOrderingAt(
        FluidContactOptions contacts, double easting, double northing, double power)
    {
        if (contacts.GasOil.Count == 0 || contacts.OilWater.Count == 0)
            return;
        double gasOil = WorldRequestValidator.InterpolateContact(contacts.GasOil, easting, northing, power);
        double oilWater = WorldRequestValidator.InterpolateContact(contacts.OilWater, easting, northing, power);
        if (!(gasOil < oilWater))
            throw new ReservoirValidationException(new Dictionary<string, string[]>
            {
                ["fluidContacts"] =
                [
                    $"Gas-oil contact must remain above oil-water contact near ({easting:G17}, {northing:G17})."
                ]
            });
    }

    private static bool HasContacts(FluidContactOptions contacts) =>
        contacts.GasWater.Count > 0 || contacts.GasOil.Count > 0 || contacts.OilWater.Count > 0;

    private static CorrelatedSpectralNoise Noise(int seed, ulong stream, HeterogeneityOptions options) =>
        new(seed, stream * 0xD6E8FEB86659FD93UL, options.SpectralModeCount,
            options.CorrelationLengthXM, options.CorrelationLengthYM, options.CorrelationLengthZM);

    private readonly record struct PropertyMeans(
        double Porosity,
        double LogPermeability,
        double PressurePa,
        double WaterSaturation,
        double GasSaturation,
        double NetToGross);

    private static PropertyMeans InterpolateProperties(
        IReadOnlyList<ConditioningPoint> controls, double easting, double northing, double power)
    {
        double weights = 0;
        double porosity = 0;
        double logPermeability = 0;
        double pressure = 0;
        double water = 0;
        double gas = 0;
        double netToGross = 0;
        foreach (ConditioningPoint point in controls)
        {
            double dx = easting - point.EastingM;
            double dy = northing - point.NorthingM;
            double distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= 1e-20)
                return new PropertyMeans(
                    point.Porosity, Math.Log(point.PermeabilityM2), point.PressurePa,
                    point.WaterSaturation, point.GasSaturation, point.NetToGross);
            double weight = 1 / Math.Pow(distanceSquared, 0.5 * power);
            weights += weight;
            porosity += weight * point.Porosity;
            logPermeability += weight * Math.Log(point.PermeabilityM2);
            pressure += weight * point.PressurePa;
            water += weight * point.WaterSaturation;
            gas += weight * point.GasSaturation;
            netToGross += weight * point.NetToGross;
        }
        return new PropertyMeans(
            porosity / weights, logPermeability / weights, pressure / weights,
            water / weights, gas / weights, netToGross / weights);
    }


    private static double InterpolateProperty(
        IReadOnlyList<ConditioningPoint> controls, double easting, double northing,
        Func<ConditioningPoint, double> selector, double power) =>
        Interpolate(controls, easting, northing,
            point => point.EastingM, point => point.NorthingM, selector, power);

    private static double InterpolateStructure(
        IReadOnlyList<StructuralConditioningPoint> controls, double easting, double northing,
        Func<StructuralConditioningPoint, double> selector, double power)
    {
        double exactSum = 0;
        int exactCount = 0;
        double weighted = 0;
        double weights = 0;
        foreach (StructuralConditioningPoint point in controls)
        {
            double dx = easting - point.EastingM;
            double dy = northing - point.NorthingM;
            double distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= 1e-20)
            {
                exactSum += selector(point);
                exactCount++;
                continue;
            }
            double weight = 1 / Math.Pow(distanceSquared, 0.5 * power);
            weighted += weight * selector(point);
            weights += weight;
        }
        return exactCount > 0 ? exactSum / exactCount : weighted / weights;
    }


    private static double Interpolate<T>(
        IReadOnlyList<T> controls, double easting, double northing,
        Func<T, double> eastingSelector, Func<T, double> northingSelector,
        Func<T, double> valueSelector, double power)
    {
        double weighted = 0;
        double weights = 0;
        foreach (T point in controls)
        {
            double dx = easting - eastingSelector(point);
            double dy = northing - northingSelector(point);
            double distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= 1e-20)
                return valueSelector(point);
            double weight = 1 / Math.Pow(distanceSquared, 0.5 * power);
            weighted += weight * valueSelector(point);
            weights += weight;
        }
        return weighted / weights;
    }

    private static double PropertyFade(
        IEnumerable<ConditioningPoint> controls, double easting, double northing, double fadeDistanceM) =>
        ControlFade(controls.Select(point => (point.EastingM, point.NorthingM)),
            easting, northing, fadeDistanceM);

    private static double ControlFade(
        IEnumerable<(double Easting, double Northing)> controls,
        double easting, double northing, double fadeDistanceM)
    {
        double nearestSquared = double.PositiveInfinity;
        foreach ((double controlEasting, double controlNorthing) in controls)
        {
            double dx = easting - controlEasting;
            double dy = northing - controlNorthing;
            nearestSquared = Math.Min(nearestSquared, dx * dx + dy * dy);
        }
        return Math.Clamp(Math.Sqrt(nearestSquared) / fadeDistanceM, 0, 1);
    }

    internal static string CreateWorldId(string canonicalRequestJson) =>
        "rsw_" + DeterministicEncoding.Sha256Hex(ModelVersion + "\n" + canonicalRequestJson);
}
