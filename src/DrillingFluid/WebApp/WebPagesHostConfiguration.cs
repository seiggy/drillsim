using NORCE.Drilling.DrillingFluid.WebPages;

namespace NORCE.Drilling.DrillingFluid.WebApp;

public class WebPagesHostConfiguration : IDrillingFluidWebPagesConfiguration
{
    public string? DrillingFluidHostURL { get; set; } = string.Empty;
    public string? DrillStringHostURL { get; set; } = string.Empty;
    public string? UnitConversionHostURL { get; set; } = string.Empty;
}
