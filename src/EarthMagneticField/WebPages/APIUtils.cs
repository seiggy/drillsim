using OSDC.Drilling.EarthMagneticField.ModelShared;

namespace OSDC.Drilling.EarthMagneticField.WebPages;

public class APIUtils : OSDC.DotnetLibraries.Drilling.WebAppUtils.APIUtils, IEarthMagneticFieldAPIUtils
{
    public APIUtils(IEarthMagneticFieldWebPagesConfiguration configuration)
    {
        HostNameEarthMagneticField = Require(configuration.EarthMagneticFieldHostURL, nameof(configuration.EarthMagneticFieldHostURL));
        HttpClientEarthMagneticField = SetHttpClient(HostNameEarthMagneticField, HostBasePathEarthMagneticField);
        ClientEarthMagneticField = new Client(HttpClientEarthMagneticField.BaseAddress!.ToString(), HttpClientEarthMagneticField);
        HostNameUnitConversion = Require(configuration.UnitConversionHostURL, nameof(configuration.UnitConversionHostURL));
    }

    public string HostNameEarthMagneticField { get; }
    public string HostBasePathEarthMagneticField { get; } = "EarthMagneticField/api/";
    public HttpClient HttpClientEarthMagneticField { get; }
    public Client ClientEarthMagneticField { get; }
    public string HostNameUnitConversion { get; }
    public string HostBasePathUnitConversion { get; } = "UnitConversion/api/";

    private static string Require(string? value, string property)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Configuration value '{property}' is required.");
        return value;
    }
}
