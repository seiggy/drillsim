using System.Reflection;

namespace NORCE.Drilling.CartographicProjection.WebApp;

public static class ExternalRazorAssemblies
{
    public static IReadOnlyList<Assembly> All { get; } =
    [
        typeof(NORCE.Drilling.CartographicProjection.WebPages.CartographicProjection).Assembly,
        typeof(NORCE.Drilling.GeodeticDatum.WebPages.GeodeticDatumMain).Assembly,
        typeof(OSDC.UnitConversion.WebPages.SingleUnitConversionMain).Assembly,
    ];
}
