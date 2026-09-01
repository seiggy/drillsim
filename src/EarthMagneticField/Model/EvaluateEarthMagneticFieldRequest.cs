using System.ComponentModel.DataAnnotations;

namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>A stateless synchronous geomagnetic-field evaluation request.</summary>
public class EvaluateEarthMagneticFieldRequest
{
    /// <summary>Reference model used for every sample in this batch.</summary>
    public EarthMagneticFieldModel Model { get; set; } = EarthMagneticFieldModel.WMM2025;

    /// <summary>Evaluation points. Any invalid sample rejects the complete request.</summary>
    [Required, MinLength(1)]
    public List<EarthMagneticFieldEvaluationPoint> Samples { get; set; } = [];
}
