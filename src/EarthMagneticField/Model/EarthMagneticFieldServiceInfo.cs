namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>Installed models and public conventions of the stateless service.</summary>
public class EarthMagneticFieldServiceInfo
{
    public string Name { get; set; } = "OSDC Earth Magnetic Field";
    public string Description { get; set; } = "Stateless WMM2025 and IGRF14 geomagnetic-field evaluation.";
    public string CoordinateFrame { get; set; } = "north-east-down";
    public string TimeConvention { get; set; } = "UTC";
    public string DepthReference { get; set; } = "WGS84 reference ellipsoid";
    public string DepthPositiveDirection { get; set; } = "down";
    public List<EarthMagneticModelInfo> Models { get; set; } = [];
}
