namespace OSDC.Drilling.EarthMagneticField.Model;

/// <summary>Details returned when an evaluation request is rejected atomically.</summary>
public class EarthMagneticFieldValidationProblem
{
    public string Error { get; set; } = "invalid_request";
    public string Message { get; set; } = string.Empty;
    public List<EarthMagneticFieldValidationError> Errors { get; set; } = [];
}
