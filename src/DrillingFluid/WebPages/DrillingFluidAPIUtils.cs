using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.DrillingFluid.WebPages;

public class DrillingFluidAPIUtils : APIUtils, IDrillingFluidAPIUtils
{
    public DrillingFluidAPIUtils(IDrillingFluidWebPagesConfiguration configuration)
    {
        HostNameDrillingFluid = Require(configuration.DrillingFluidHostURL, nameof(configuration.DrillingFluidHostURL));
        HostBasePathDrillingFluid = "DrillingFluid/api/";
        HttpClientDrillingFluid = SetHttpClient(HostNameDrillingFluid, HostBasePathDrillingFluid);
        ClientDrillingFluid = new NORCE.Drilling.DrillingFluid.ModelShared.Client(HttpClientDrillingFluid.BaseAddress!.ToString(), HttpClientDrillingFluid);

        HostNameUnitConversion = Require(configuration.UnitConversionHostURL, nameof(configuration.UnitConversionHostURL));
        HostBasePathUnitConversion = "UnitConversion/api/";

        HostNameDrillString = Require(configuration.DrillStringHostURL, nameof(configuration.DrillStringHostURL));
        HostBasePathDrillString = "DrillString/api/";
        HttpClientDrillString = SetHttpClient(HostNameDrillString, HostBasePathDrillString);
        ClientDrillString = new NORCE.Drilling.DrillingFluid.ModelShared.Client(HttpClientDrillString.BaseAddress!.ToString(), HttpClientDrillString);
    }

    public string HostNameDrillingFluid { get; }
    public string HostBasePathDrillingFluid { get; }
    public HttpClient HttpClientDrillingFluid { get; }
    public NORCE.Drilling.DrillingFluid.ModelShared.Client ClientDrillingFluid { get; }

    public string HostNameUnitConversion { get; }
    public string HostBasePathUnitConversion { get; }

    public string HostNameDrillString { get; }
    public string HostBasePathDrillString { get; }
    public HttpClient HttpClientDrillString { get; }
    public NORCE.Drilling.DrillingFluid.ModelShared.Client ClientDrillString { get; }

    private static string Require(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required host URL configuration: {propertyName}");
        }

        return value;
    }
}
