using OSDC.Drilling.Cluster.WebPages;

namespace OSDC.Drilling.Cluster.WebApp;

public class WebPagesHostConfiguration :
    IClusterWebPagesConfiguration,
    OSDC.Drilling.Field.WebPages.IFieldWebPagesConfiguration,
    OSDC.Drilling.Rig.WebPages.IRigWebPagesConfiguration,
    NORCE.Drilling.CartographicProjection.WebPages.ICartographicProjectionWebPagesConfiguration,
    NORCE.Drilling.GeodeticDatum.WebPages.IGeodeticDatumWebPagesConfiguration,
    OSDC.Drilling.EarthGravity.WebPages.IEarthGravityWebPagesConfiguration,
    OSDC.Drilling.EarthMagneticField.WebPages.IEarthMagneticFieldWebPagesConfiguration,
    OSDC.Drilling.EarthVerticalDatum.WebPages.IEarthVerticalDatumWebPagesConfiguration
{
    public string? ClusterHostURL { get; set; } = string.Empty;
    public string? FieldHostURL { get; set; } = string.Empty;
    public string? RigHostURL { get; set; } = string.Empty;
    public string? TrajectoryHostURL { get; set; } = string.Empty;
    public string? EarthCartographicProjectionHostURL { get; set; } = string.Empty;
    public string? EarthGeodesyHostURL { get; set; } = string.Empty;
    public string EarthGravityHostURL { get; set; } = string.Empty;
    public string EarthMagneticFieldHostURL { get; set; } = string.Empty;
    public string? EarthVerticalDatumHostURL { get; set; } = string.Empty;
    public string? UnitConversionHostURL { get; set; } = string.Empty;

    public string? CartographicProjectionHostURL
    {
        get => EarthCartographicProjectionHostURL ?? string.Empty;
        set => EarthCartographicProjectionHostURL = value;
    }

    public string? GeodeticDatumHostURL
    {
        get => EarthGeodesyHostURL ?? string.Empty;
        set => EarthGeodesyHostURL = value;
    }

    string OSDC.Drilling.EarthVerticalDatum.WebPages.IEarthVerticalDatumWebPagesConfiguration.EarthVerticalDatumHostURL => EarthVerticalDatumHostURL ?? string.Empty;
    string OSDC.Drilling.Rig.WebPages.IRigWebPagesConfiguration.VerticalDatumHostURL
    {
        get => EarthVerticalDatumHostURL ?? string.Empty;
        set => EarthVerticalDatumHostURL = value;
    }
}
