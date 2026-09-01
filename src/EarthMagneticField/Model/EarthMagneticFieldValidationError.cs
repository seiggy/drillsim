namespace OSDC.Drilling.EarthMagneticField.Model;

public record EarthMagneticFieldValidationError(int? SampleIndex, string Property, string Code, string Message);
