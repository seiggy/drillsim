namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>Identity, validity, and provenance of one installed geomagnetic reference model.</summary>
public class EarthMagneticModelInfo
{
    public EarthMagneticFieldModel Model { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ID { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public DateTimeOffset MinimumUtc { get; set; }
    public DateTimeOffset MaximumUtc { get; set; }
    public double MinimumDepth { get; set; }
    public double MaximumDepth { get; set; }
    public int Degree { get; set; }
    public int Order { get; set; }
    public string GeographicLibVersion { get; set; } = string.Empty;
    public string ReferenceEllipsoid { get; set; } = "WGS84";
    public string CoordinateFrame { get; set; } = "north-east-down";
    public string MagneticFluxDensityUnit { get; set; } = "tesla";
    public string AngleUnit { get; set; } = "radian";
    public string DepthPositiveDirection { get; set; } = "down";
    public bool ConcurrentEvaluationEnabled { get; set; } = true;
    public string MetadataSHA256 { get; set; } = string.Empty;
    public string CoefficientSHA256 { get; set; } = string.Empty;
}
