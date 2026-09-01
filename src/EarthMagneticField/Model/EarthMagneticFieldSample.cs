namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>A geomagnetic-field result in the public north-east-down frame.</summary>
public class EarthMagneticFieldSample
{
    /// <summary>The validated input, including its UTC evaluation instant.</summary>
    public EarthMagneticFieldEvaluationPoint Input { get; set; } = new();

    /// <summary>Northerly magnetic-flux-density component in SI teslas.</summary>
    public double North { get; set; }

    /// <summary>Easterly magnetic-flux-density component in SI teslas.</summary>
    public double East { get; set; }

    /// <summary>Downward magnetic-flux-density component in SI teslas.</summary>
    public double Down { get; set; }

    /// <summary>Horizontal magnetic-flux-density magnitude in SI teslas.</summary>
    public double HorizontalIntensity { get; set; }

    /// <summary>Total magnetic-flux-density magnitude in SI teslas.</summary>
    public double TotalIntensity { get; set; }

    /// <summary>Declination in SI radians, positive east of geodetic north; null when horizontal intensity is zero.</summary>
    public double? Declination { get; set; }

    /// <summary>Inclination in SI radians, positive downward from horizontal; null when total intensity is zero.</summary>
    public double? Inclination { get; set; }
}
