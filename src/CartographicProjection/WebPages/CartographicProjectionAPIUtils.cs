using NORCE.Drilling.CartographicProjection.ModelShared;
using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace NORCE.Drilling.CartographicProjection.WebPages;

public class CartographicProjectionAPIUtils : APIUtils, ICartographicProjectionAPIUtils
{
    public CartographicProjectionAPIUtils(ICartographicProjectionWebPagesConfiguration configuration)
    {
        HostNameCartographicProjection = Require(configuration.CartographicProjectionHostURL, nameof(configuration.CartographicProjectionHostURL));
        HttpClientCartographicProjection = SetHttpClient(HostNameCartographicProjection, HostBasePathCartographicProjection);
        ClientCartographicProjection = new Client(HttpClientCartographicProjection.BaseAddress!.ToString(), HttpClientCartographicProjection);

        HostNameGeodeticDatum = Require(configuration.GeodeticDatumHostURL, nameof(configuration.GeodeticDatumHostURL));
        HttpClientGeodeticDatum = SetHttpClient(HostNameGeodeticDatum, HostBasePathGeodeticDatum);
        ClientGeodeticDatum = new Client(HttpClientGeodeticDatum.BaseAddress!.ToString(), HttpClientGeodeticDatum);

        HostNameUnitConversion = Require(configuration.UnitConversionHostURL, nameof(configuration.UnitConversionHostURL));
    }

    private static string Require(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{propertyName}' must be assigned before WebPages is used.");
        }

        return value;
    }

    public string HostNameCartographicProjection { get; }
    public string HostBasePathCartographicProjection { get; } = "CartographicProjection/api/";
    public HttpClient HttpClientCartographicProjection { get; }
    public Client ClientCartographicProjection { get; }

    public string HostNameGeodeticDatum { get; }
    public string HostBasePathGeodeticDatum { get; } = "GeodeticDatum/api/";
    public HttpClient HttpClientGeodeticDatum { get; }
    public Client ClientGeodeticDatum { get; }

    public string HostNameUnitConversion { get; }
    public string HostBasePathUnitConversion { get; } = "UnitConversion/api/";
}
