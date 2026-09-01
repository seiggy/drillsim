namespace NORCE.Drilling.CartographicProjection.WebPages;

public static class DataUtils
{
    public const double DEFAULT_VALUE = 999.25;
    public static string DEFAULT_NAME_CartographicConversionSet = "Default CartographicConversionSet Name";
    public static string DEFAULT_DESCR_CartographicConversionSet = "Default CartographicConversionSet Description";
    public static string DEFAULT_NAME_CartographicProjection = "Default CartographicProjection Name";
    public static string DEFAULT_DESCR_CartographicProjection = "Default CartographicProjection Description";

    public static class UnitAndReferenceParameters
    {
        public static string? UnitSystemName { get; set; } = "Metric";
        public static string? DepthReferenceName { get; set; }
        public static string? PositionReferenceName { get; set; }
        public static string? AzimuthReferenceName { get; set; }
        public static string? PressureReferenceName { get; set; }
        public static string? DateReferenceName { get; set; }
    }

    public static void UpdateUnitSystemName(string val)
    {
        UnitAndReferenceParameters.UnitSystemName = val;
    }

    public static readonly string CartographicConversionSetCartographicCoordinateListLabel = "CartographicCoordinateList";
    public static readonly string CartographicConversionSetOutputParamLabel = "CartographicConversionSetOutputParam";
    public static readonly string CartographicConversionSetNameLabel = "CartographicConversionSet name";
    public static readonly string CartographicConversionSetDescrLabel = "CartographicConversionSet description";
    public static readonly string CartographicConversionSetOutputParamQty = "DepthDrilling";
    public static readonly string CartographicProjectionNameLabel = "Name";
    public static readonly string CartographicProjectionLabel = "Description";
}
