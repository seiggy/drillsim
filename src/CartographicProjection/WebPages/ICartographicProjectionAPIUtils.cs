using NORCE.Drilling.CartographicProjection.ModelShared;

namespace NORCE.Drilling.CartographicProjection.WebPages;

public interface ICartographicProjectionAPIUtils
{
    string HostNameCartographicProjection { get; }
    string HostBasePathCartographicProjection { get; }
    HttpClient HttpClientCartographicProjection { get; }
    Client ClientCartographicProjection { get; }

    string HostNameGeodeticDatum { get; }
    string HostBasePathGeodeticDatum { get; }
    HttpClient HttpClientGeodeticDatum { get; }
    Client ClientGeodeticDatum { get; }

    string HostNameUnitConversion { get; }
    string HostBasePathUnitConversion { get; }
}
