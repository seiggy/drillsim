using NORCE.Drilling.GeothermalProperties.ModelShared;
using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.GeothermalProperties.WebPages;

public class GeothermalPropertiesAPIUtils : APIUtils, IGeothermalPropertiesAPIUtils
{
    public GeothermalPropertiesAPIUtils(IGeothermalPropertiesWebPagesConfiguration configuration)
    {
        HostNameGeothermalProperties = Require(configuration.GeothermalPropertiesHostURL, nameof(configuration.GeothermalPropertiesHostURL));
        HostBasePathGeothermalProperties = "GeothermalProperties/api/";
        HttpClientGeothermalProperties = SetHttpClient(HostNameGeothermalProperties, HostBasePathGeothermalProperties);
        ClientGeothermalProperties = new Client(HttpClientGeothermalProperties.BaseAddress!.ToString(), HttpClientGeothermalProperties);

        HostNameUnitConversion = Require(configuration.UnitConversionHostURL, nameof(configuration.UnitConversionHostURL));
        HostBasePathUnitConversion = "UnitConversion/api/";

        HostNameTrajectory = Require(configuration.TrajectoryHostURL, nameof(configuration.TrajectoryHostURL));
        HostBasePathTrajectory = "Trajectory/api/";
        HttpClientTrajectory = SetHttpClient(HostNameTrajectory, HostBasePathTrajectory);
        ClientTrajectory = new Client(HttpClientTrajectory.BaseAddress!.ToString(), HttpClientTrajectory);

        HostNameWellBore = Require(configuration.WellBoreHostURL, nameof(configuration.WellBoreHostURL));
        HostBasePathWellBore = "WellBore/api/";
        HttpClientWellBore = SetHttpClient(HostNameWellBore, HostBasePathWellBore);
        ClientWellBore = new Client(HttpClientWellBore.BaseAddress!.ToString(), HttpClientWellBore);
    }

    public string HostNameGeothermalProperties { get; }
    public string HostBasePathGeothermalProperties { get; }
    public HttpClient HttpClientGeothermalProperties { get; }
    public Client ClientGeothermalProperties { get; }

    public string HostNameUnitConversion { get; }
    public string HostBasePathUnitConversion { get; }

    public string HostNameTrajectory { get; }
    public string HostBasePathTrajectory { get; }
    public HttpClient HttpClientTrajectory { get; }
    public Client ClientTrajectory { get; }

    public string HostNameWellBore { get; }
    public string HostBasePathWellBore { get; }
    public HttpClient HttpClientWellBore { get; }
    public Client ClientWellBore { get; }

    private static string Require(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required host URL configuration: {propertyName}");
        }

        return value;
    }
}
