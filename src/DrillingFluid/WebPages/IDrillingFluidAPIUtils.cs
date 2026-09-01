namespace NORCE.Drilling.DrillingFluid.WebPages;

public interface IDrillingFluidAPIUtils
{
    string HostNameDrillingFluid { get; }
    string HostBasePathDrillingFluid { get; }
    HttpClient HttpClientDrillingFluid { get; }
    NORCE.Drilling.DrillingFluid.ModelShared.Client ClientDrillingFluid { get; }

    string HostNameUnitConversion { get; }
    string HostBasePathUnitConversion { get; }

    string HostNameDrillString { get; }
    string HostBasePathDrillString { get; }
    HttpClient HttpClientDrillString { get; }
    NORCE.Drilling.DrillingFluid.ModelShared.Client ClientDrillString { get; }
}
