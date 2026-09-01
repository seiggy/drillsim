using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.GeothermalProperties.WebPages;

public interface IGeothermalPropertiesWebPagesConfiguration :
    IGeothermalPropertiesHostURL,
    IUnitConversionHostURL,
    ITrajectoryHostURL,
    IWellBoreHostURL
{
}
