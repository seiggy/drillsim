using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.DrillingFluid.WebPages;

public interface IDrillingFluidWebPagesConfiguration :
    IDrillingFluidHostURL,
    IDrillStringHostURL,
    IUnitConversionHostURL
{
}
