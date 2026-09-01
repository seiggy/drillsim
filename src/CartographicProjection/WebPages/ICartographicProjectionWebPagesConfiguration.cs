using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.CartographicProjection.WebPages;

public interface ICartographicProjectionWebPagesConfiguration :
    ICartographicProjectionHostURL,
    IGeodeticDatumHostURL,
    IUnitConversionHostURL
{
}
