using NORCE.Drilling.CartographicProjection.WebPages;

namespace NORCE.Drilling.CartographicProjection.WebApp;

public class WebPagesHostConfiguration :
    ICartographicProjectionWebPagesConfiguration,
    NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumWebPagesConfiguration
{
    public string CartographicProjectionHostURL { get; set; } = string.Empty;
    public string GeodeticDatumHostURL { get; set; } = string.Empty;
    public string UnitConversionHostURL { get; set; } = string.Empty;
}
