using OSDC.Drilling.EarthMagneticField.WebPages;

namespace OSDC.Drilling.EarthMagneticField.WebApp;

public class WebPagesHostConfiguration : IEarthMagneticFieldWebPagesConfiguration
{
    public string EarthMagneticFieldHostURL { get; set; } = string.Empty;
    public string? UnitConversionHostURL { get; set; } = string.Empty;
}
