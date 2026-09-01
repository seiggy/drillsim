using NORCE.Drilling.GeothermalProperties.ModelShared;

namespace NORCE.Drilling.GeothermalProperties.WebPages;

public interface IGeothermalPropertiesAPIUtils
{
    string HostNameGeothermalProperties { get; }
    string HostBasePathGeothermalProperties { get; }
    HttpClient HttpClientGeothermalProperties { get; }
    Client ClientGeothermalProperties { get; }

    string HostNameUnitConversion { get; }
    string HostBasePathUnitConversion { get; }

    string HostNameTrajectory { get; }
    string HostBasePathTrajectory { get; }
    HttpClient HttpClientTrajectory { get; }
    Client ClientTrajectory { get; }

    string HostNameWellBore { get; }
    string HostBasePathWellBore { get; }
    HttpClient HttpClientWellBore { get; }
    Client ClientWellBore { get; }
}
