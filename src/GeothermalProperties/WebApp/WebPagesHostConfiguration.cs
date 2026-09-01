using NORCE.Drilling.GeothermalProperties.WebPages;

namespace NORCE.Drilling.GeothermalProperties.WebApp;

public class WebPagesHostConfiguration : IGeothermalPropertiesWebPagesConfiguration
{
    public string? GeothermalPropertiesHostURL { get; set; } = string.Empty;
    public string? UnitConversionHostURL { get; set; } = string.Empty;
    public string? TrajectoryHostURL { get; set; } = string.Empty;
    public string? WellBoreHostURL { get; set; } = string.Empty;
}
