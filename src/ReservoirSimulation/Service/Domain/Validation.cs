using ReservoirSimulation.Contracts;

namespace ReservoirSimulation.Domain;

internal sealed class ReservoirValidationException(Dictionary<string, string[]> errors, string? diagnosticCode = null)
    : ArgumentException("One or more reservoir simulation values are invalid.")
{
    internal Dictionary<string, string[]> Errors { get; } = errors;
    internal string? DiagnosticCode { get; } = diagnosticCode;
}

internal sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    internal void Add(string key, string message)
    {
        if (!_errors.TryGetValue(key, out List<string>? messages))
            _errors[key] = messages = [];
        messages.Add(message);
    }

    internal void RequireFinite(string key, double value)
    {
        if (!double.IsFinite(value))
            Add(key, "Value must be finite.");
    }

    internal void ThrowIfAny(string? diagnosticCode = null)
    {
        if (_errors.Count == 0)
            return;

        throw new ReservoirValidationException(_errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToArray(),
            StringComparer.Ordinal), diagnosticCode);
    }
}

internal static class WorldRequestValidator
{
    internal static void Validate(WorldGenerationRequest? request)
    {
        var errors = new ValidationErrors();
        if (request is null)
        {
            errors.Add("request", "A request body is required.");
            errors.ThrowIfAny();
            return;
        }

        if (request.FieldId == Guid.Empty)
            errors.Add("fieldId", "Field ID must not be empty.");
        if (string.IsNullOrWhiteSpace(request.ReservoirName))
            errors.Add("reservoirName", "Reservoir name is required.");
        else if (request.ReservoirName.Length > 200)
            errors.Add("reservoirName", "Reservoir name must not exceed 200 characters.");

        ValidateCalibrationArtifact(request.CalibrationArtifact, errors);
        ValidateGrid(request.Grid, errors);
        ValidateHeterogeneity(request.Heterogeneity, errors);
        ValidateStructuralControls(request.StructuralConditioningPoints, errors);
        ValidateControls(request.ConditioningPoints, errors);
        ValidateContacts(request.FluidContacts, request.Heterogeneity?.IdwPower ?? 2, errors);
        errors.ThrowIfAny();
    }

    private static void ValidateCalibrationArtifact(
        CalibrationArtifactReference? artifact,
        ValidationErrors errors)
    {
        if (artifact is null)
        {
            errors.Add("calibrationArtifact", "A sealed calibration artifact reference is required.");
            return;
        }
        if (string.IsNullOrWhiteSpace(artifact.Id))
            errors.Add("calibrationArtifact.id", "Calibration artifact ID is required.");
        else if (artifact.Id.Length > 256)
            errors.Add("calibrationArtifact.id", "Calibration artifact ID must not exceed 256 characters.");
        if (artifact.Sha256 is null || artifact.Sha256.Length != 64 ||
            !artifact.Sha256.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f'))
            errors.Add("calibrationArtifact.sha256",
                "Calibration artifact SHA-256 must be exactly 64 lowercase hexadecimal characters.");
    }


    private static void ValidateGrid(GridOptions? grid, ValidationErrors errors)
    {
        if (grid is null)
        {
            errors.Add("grid", "Grid options are required.");
            return;
        }

        if (grid.CountX <= 0) errors.Add("grid.countX", "Count must be positive.");
        if (grid.CountY <= 0) errors.Add("grid.countY", "Count must be positive.");
        if (grid.CountZ <= 0) errors.Add("grid.countZ", "Count must be positive.");
        errors.RequireFinite("grid.horizontalPaddingM", grid.HorizontalPaddingM);
        if (grid.HorizontalPaddingM <= 0)
            errors.Add("grid.horizontalPaddingM", "Horizontal padding must be positive.");

        long cells = (long)grid.CountX * grid.CountY * grid.CountZ;
        // ponytail: this first slice deliberately caps a structured Cartesian grid; use domain decomposition before lifting it.
        if (grid.CountX > 256 || grid.CountY > 256 || grid.CountZ > 128 || cells > 2_000_000)
            errors.Add("grid", "Structured grid exceeds the 256 x 256 x 128 or 2,000,000-cell service limit.");
    }

    private static void ValidateHeterogeneity(HeterogeneityOptions? value, ValidationErrors errors)
    {
        if (value is null)
        {
            errors.Add("heterogeneity", "Heterogeneity options are required.");
            return;
        }

        Positive("heterogeneity.idwPower", value.IdwPower, errors);
        if (value.SpectralModeCount <= 0 || value.SpectralModeCount > 256)
            errors.Add("heterogeneity.spectralModeCount", "Spectral mode count must be between 1 and 256.");
        Positive("heterogeneity.correlationLengthXM", value.CorrelationLengthXM, errors);
        Positive("heterogeneity.correlationLengthYM", value.CorrelationLengthYM, errors);
        Positive("heterogeneity.correlationLengthZM", value.CorrelationLengthZM, errors);
        Positive("heterogeneity.controlFadeDistanceM", value.ControlFadeDistanceM, errors);
        Nonnegative("heterogeneity.topDepthStdDevM", value.TopDepthStdDevM, errors);
        Nonnegative("heterogeneity.baseDepthStdDevM", value.BaseDepthStdDevM, errors);
        Nonnegative("heterogeneity.porosityStdDev", value.PorosityStdDev, errors);
        Nonnegative("heterogeneity.logPermeabilityStdDev", value.LogPermeabilityStdDev, errors);
        Nonnegative("heterogeneity.pressureStdDevPa", value.PressureStdDevPa, errors);
        Nonnegative("heterogeneity.waterSaturationStdDev", value.WaterSaturationStdDev, errors);
        Nonnegative("heterogeneity.gasSaturationStdDev", value.GasSaturationStdDev, errors);
        Nonnegative("heterogeneity.netToGrossStdDev", value.NetToGrossStdDev, errors);
        errors.RequireFinite("heterogeneity.shalePorosity", value.ShalePorosity);
        if (!(value.ShalePorosity > 0 && value.ShalePorosity < 1))
            errors.Add("heterogeneity.shalePorosity", "Shale porosity must be between zero and one.");
        errors.RequireFinite("heterogeneity.shalePermeabilityM2", value.ShalePermeabilityM2);
        if (!(value.ShalePermeabilityM2 >= 1e-22 && value.ShalePermeabilityM2 <= 1e-14))
            errors.Add("heterogeneity.shalePermeabilityM2", "Shale permeability must be in [1e-22, 1e-14] m2.");
    }

    private static void ValidateControls(IReadOnlyList<ConditioningPoint>? controls, ValidationErrors errors)
    {
        if (controls is null || controls.Count == 0)
        {
            errors.Add("conditioningPoints", "At least one conditioning point is required.");
            return;
        }

        var coordinates = new HashSet<(double Easting, double Northing)>();
        for (int index = 0; index < controls.Count; index++)
        {
            ConditioningPoint? point = controls[index];
            string key = $"conditioningPoints[{index}]";
            if (point is null)
            {
                errors.Add(key, "Conditioning point must not be null.");
                continue;
            }

            FinitePoint(point, key, errors);
            if (double.IsFinite(point.EastingM) && double.IsFinite(point.NorthingM) &&
                !coordinates.Add((point.EastingM, point.NorthingM)))
                errors.Add(key, "Conditioning point coordinates must be unique.");
            if (!(point.ReservoirTopDepthM < point.ReservoirBaseDepthM))
                errors.Add($"{key}.reservoirBaseDepthM", "Reservoir top depth must be less than base depth.");
            if (!(point.Porosity > 0 && point.Porosity < 1))
                errors.Add($"{key}.porosity", "Porosity must be between zero and one.");
            if (!(point.PermeabilityM2 >= 1e-22 && point.PermeabilityM2 <= 1e-8))
                errors.Add($"{key}.permeabilityM2", "Permeability must be in [1e-22, 1e-8] m2.");
            if (!(point.PressurePa > 0))
                errors.Add($"{key}.pressurePa", "Pressure must be positive.");
            if (!(point.WaterSaturation >= 0 && point.WaterSaturation <= 1))
                errors.Add($"{key}.waterSaturation", "Water saturation must be in [0, 1].");
            if (!(point.GasSaturation >= 0 && point.GasSaturation <= 1))
                errors.Add($"{key}.gasSaturation", "Gas saturation must be in [0, 1].");
            if (!(point.NetToGross >= 0 && point.NetToGross <= 1))
                errors.Add($"{key}.netToGross", "Net-to-gross must be in [0, 1].");
            if (point.WaterSaturation + point.GasSaturation > 1)
                errors.Add(key, "Water and gas saturation sum must not exceed one.");
        }
    }

    private static void ValidateStructuralControls(
        IReadOnlyList<StructuralConditioningPoint>? controls,
        ValidationErrors errors)
    {
        if (controls is null)
        {
            errors.Add("structuralConditioningPoints", "Structural control collection is required; use an empty collection for fallback.");
            return;
        }

        for (int index = 0; index < controls.Count; index++)
        {
            StructuralConditioningPoint? point = controls[index];
            string key = $"structuralConditioningPoints[{index}]";
            if (point is null)
            {
                errors.Add(key, "Structural conditioning point must not be null.");
                continue;
            }
            errors.RequireFinite($"{key}.eastingM", point.EastingM);
            errors.RequireFinite($"{key}.northingM", point.NorthingM);
            errors.RequireFinite($"{key}.reservoirTopDepthM", point.ReservoirTopDepthM);
            errors.RequireFinite($"{key}.reservoirBaseDepthM", point.ReservoirBaseDepthM);
            if (!(point.ReservoirTopDepthM < point.ReservoirBaseDepthM))
                errors.Add($"{key}.reservoirBaseDepthM", "Reservoir top depth must be less than base depth.");
        }
    }


    private static void ValidateContacts(
        FluidContactOptions? contacts,
        double idwPower,
        ValidationErrors errors)
    {
        if (contacts is null)
        {
            errors.Add("fluidContacts", "Fluid contact options are required.");
            return;
        }
        errors.RequireFinite("fluidContacts.transitionThicknessM", contacts.TransitionThicknessM);
        if (contacts.TransitionThicknessM < 0)
            errors.Add("fluidContacts.transitionThicknessM", "Transition thickness must be nonnegative.");
        ValidateContactList("fluidContacts.gasWater", contacts.GasWater, errors);
        bool gasOilValid = ValidateContactList("fluidContacts.gasOil", contacts.GasOil, errors);
        bool oilWaterValid = ValidateContactList("fluidContacts.oilWater", contacts.OilWater, errors);
        if (contacts.GasWater is { Count: > 0 } &&
            (contacts.GasOil is { Count: > 0 } || contacts.OilWater is { Count: > 0 }))
            errors.Add("fluidContacts", "Gas-water contacts cannot be combined with gas-oil or oil-water contacts.");

        if (!double.IsFinite(idwPower) || idwPower <= 0 || !gasOilValid || !oilWaterValid ||
            contacts.GasOil.Count == 0 || contacts.OilWater.Count == 0)
            return;
        foreach ((double easting, double northing) in contacts.GasOil
            .Concat(contacts.OilWater)
            .Select(point => (point.EastingM, point.NorthingM))
            .Distinct())
        {
            double gasOilDepth = InterpolateContact(contacts.GasOil, easting, northing, idwPower);
            double oilWaterDepth = InterpolateContact(contacts.OilWater, easting, northing, idwPower);
            if (!(gasOilDepth < oilWaterDepth))
                errors.Add("fluidContacts",
                    $"Gas-oil contact must be above oil-water contact near ({easting:G17}, {northing:G17}).");
        }
    }

    private static bool ValidateContactList(
        string key,
        IReadOnlyList<FluidContactPoint>? points,
        ValidationErrors errors)
    {
        if (points is null)
        {
            errors.Add(key, "Contact collection is required; use an empty collection when absent.");
            return false;
        }
        bool valid = true;
        var coordinates = new HashSet<(double Easting, double Northing)>();
        for (int index = 0; index < points.Count; index++)
        {
            FluidContactPoint? point = points[index];
            string pointKey = $"{key}[{index}]";
            if (point is null)
            {
                errors.Add(pointKey, "Contact point must not be null.");
                valid = false;
                continue;
            }
            if (!double.IsFinite(point.EastingM) || !double.IsFinite(point.NorthingM) ||
                !double.IsFinite(point.ContactDepthTvdM))
            {
                errors.Add(pointKey, "Contact coordinates and depth must be finite.");
                valid = false;
            }
            else if (!coordinates.Add((point.EastingM, point.NorthingM)))
            {
                errors.Add(pointKey, "Contact point coordinates must be unique within a contact surface.");
                valid = false;
            }
        }
        return valid;
    }

    internal static double InterpolateContact(
        IReadOnlyList<FluidContactPoint> points,
        double easting,
        double northing,
        double power)
    {
        double weighted = 0;
        double weights = 0;
        foreach (FluidContactPoint point in points)
        {
            double dx = easting - point.EastingM;
            double dy = northing - point.NorthingM;
            double distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= 1e-20)
                return point.ContactDepthTvdM;
            double weight = 1 / Math.Pow(distanceSquared, 0.5 * power);
            weighted += weight * point.ContactDepthTvdM;
            weights += weight;
        }
        return weighted / weights;
    }


    private static void FinitePoint(ConditioningPoint point, string key, ValidationErrors errors)
    {
        errors.RequireFinite($"{key}.eastingM", point.EastingM);
        errors.RequireFinite($"{key}.northingM", point.NorthingM);
        errors.RequireFinite($"{key}.reservoirTopDepthM", point.ReservoirTopDepthM);
        errors.RequireFinite($"{key}.reservoirBaseDepthM", point.ReservoirBaseDepthM);
        errors.RequireFinite($"{key}.porosity", point.Porosity);
        errors.RequireFinite($"{key}.permeabilityM2", point.PermeabilityM2);
        errors.RequireFinite($"{key}.pressurePa", point.PressurePa);
        errors.RequireFinite($"{key}.waterSaturation", point.WaterSaturation);
        errors.RequireFinite($"{key}.gasSaturation", point.GasSaturation);
        errors.RequireFinite($"{key}.netToGross", point.NetToGross);
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
}
