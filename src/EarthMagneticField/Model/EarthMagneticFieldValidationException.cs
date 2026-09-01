namespace OSDC.Drilling.EarthMagneticField.Model;

public sealed class EarthMagneticFieldValidationException(IReadOnlyList<EarthMagneticFieldValidationError> errors)
    : Exception("The Earth magnetic field evaluation request is invalid.")
{
    public IReadOnlyList<EarthMagneticFieldValidationError> Errors { get; } = errors;
}
