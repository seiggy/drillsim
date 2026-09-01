namespace OSDC.Drilling.EarthMagneticField.Service;

public sealed class EarthMagneticFieldServiceOptions
{
    public const string SectionName = "EarthMagneticField";
    public int MaximumSamplesPerRequest { get; set; } = 10_000;
    public string? ModelDirectory { get; set; }
}
