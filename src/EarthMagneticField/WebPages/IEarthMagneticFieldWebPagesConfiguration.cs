using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace OSDC.Drilling.EarthMagneticField.WebPages;

public interface IEarthMagneticFieldWebPagesConfiguration : IUnitConversionHostURL
{
    string EarthMagneticFieldHostURL { get; }
}
