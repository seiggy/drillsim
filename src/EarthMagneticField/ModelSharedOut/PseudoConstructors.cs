namespace OSDC.Drilling.EarthMagneticField.ModelShared;

/// <summary>Convenience constructors for the generated stateless Earth Magnetic Field DTOs.</summary>
public static class PseudoConstructors
{
    public static EarthMagneticFieldEvaluationPoint ConstructEarthMagneticFieldEvaluationPoint() => new()
    {
        Latitude = 0,
        Longitude = 0,
        Depth = 0,
        DateTimeUtc = DateTimeOffset.UtcNow
    };

    public static EvaluateEarthMagneticFieldRequest ConstructEvaluateEarthMagneticFieldRequest() => new()
    {
        Model = EarthMagneticFieldModel.WMM2025,
        Samples = [ConstructEarthMagneticFieldEvaluationPoint()]
    };
}
