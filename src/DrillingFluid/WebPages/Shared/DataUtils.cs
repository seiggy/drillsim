namespace NORCE.Drilling.DrillingFluid.WebPages.Shared;

public static class DataUtils
{
    public const double DEFAULT_VALUE = 999.25;
    public static string DEFAULT_NAME_DrillingFluid = "Default DrillingFluid Name";
    public static string DEFAULT_DESCR_DrillingFluid = "Default DrillingFluid Description";
    public static string DEFAULT_NAME_MyBaseData = "Default MyBaseData Name";
    public static string DEFAULT_DESCR_MyBaseData = "Default MyBaseData Description";
    public static string FLOATING_COLOUR = "rgba(70, 50, 240, 0.86)";
    public static string FLOATING_COLOUR_DEEP = "rgba(47, 29, 148, 0.86)";
    public static string FLOATING_POSITION = "absolute; top: 45%; left: 20%; width: 75%";

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

    public static readonly string DrillingFluidMyBaseDataListLabel = "MyBaseDataList";
    public static readonly string DrillingFluidOutputParamLabel = "DrillingFluidOutputParam";
    public static readonly string DrillingFluidNameLabel = "DrillingFluid name";
    public static readonly string DrillingFluidDescrLabel = "DrillingFluid description";
    public static readonly string DrillingFluidOutputParamQty = "DepthDrilling";

    public static readonly string MyBaseDataNameLabel = "MyBaseData name";
    public static readonly string MyBaseDataParamLabel = "MyBaseDataParam";
    public static readonly string MyBaseDataParamQty = "DepthDrilling";

    public static readonly string MyBaseDataTypeLabel = "MyBaseData type";
    public static readonly string DerivedData1Label = "DerivedData1 name";
    public static readonly string DerivedData1ParamLabel = "DerivedData1Param";
    public static readonly string DerivedData1ParamQty = "DepthDrilling";
    public static readonly string DerivedData2Label = "DerivedData2 name";
    public static readonly string DerivedData2ParamLabel = "DerivedData2Param";
    public static readonly string DerivedData2ParamQty = "DepthDrilling";

    public static readonly string InputXValuesTitle = "X value";
    public static readonly string InputXValuesQty = "DepthDrilling";
    public static readonly string InputYValuesTitle = "Y value";
    public static readonly string InputYValuesQty = "Length";
    public static readonly string OutputXValuesTitle = "X value";
    public static readonly string OutputXValuesQty = "DepthDrilling";
    public static readonly string OutputYValuesTitle = "Y value";
    public static readonly string OutputYValuesQty = "Length";
}
