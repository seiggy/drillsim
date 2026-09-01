using OSDC.Drilling.EarthMagneticField.ModelShared;

namespace OSDC.Drilling.EarthMagneticField.WebPages;

public interface IEarthMagneticFieldAPIUtils
{
    string HostNameEarthMagneticField { get; }
    string HostBasePathEarthMagneticField { get; }
    HttpClient HttpClientEarthMagneticField { get; }
    Client ClientEarthMagneticField { get; }
    string HostNameUnitConversion { get; }
    string HostBasePathUnitConversion { get; }
}
