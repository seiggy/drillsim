namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>Geomagnetic results in the same order as the request samples.</summary>
public class EvaluateEarthMagneticFieldResponse
{
    public EarthMagneticModelInfo Model { get; set; } = new();
    public List<EarthMagneticFieldSample> Samples { get; set; } = [];
}
