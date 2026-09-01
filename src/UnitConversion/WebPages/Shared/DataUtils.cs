using System.Globalization;

namespace OSDC.UnitConversion.WebPages;

public static class DataUtils
{
    // default values
    public static readonly string DEFAULT_NAME_UnitConversionSet = "Default UnitConversionSet Name";
    public static readonly string DEFAULT_DESCR_UnitConversionSet = "Default UnitConversionSet Description";

    public static readonly CultureInfo CULTURE_EN_US;
    public static readonly CultureInfo CULTURE_EN_US_WO_SEP;

    static DataUtils()
    {
        CULTURE_EN_US = new CultureInfo("en-US");
        CULTURE_EN_US_WO_SEP = new CultureInfo("en-US");
        CULTURE_EN_US_WO_SEP.NumberFormat.NumberGroupSeparator = "";
    }
}