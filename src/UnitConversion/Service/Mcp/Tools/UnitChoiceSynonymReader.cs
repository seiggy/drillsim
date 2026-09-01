using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OSDC.UnitConversion.Conversion;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

/// <summary>
/// Reads the synonym extension when the service is built against a newly generated conversion package,
/// while remaining buildable against the previously published package during the generation cycle.
/// </summary>
internal static class UnitChoiceSynonymReader
{
    private static readonly PropertyInfo? SynonymsProperty = typeof(UnitChoice).GetProperty("Synonyms", BindingFlags.Instance | BindingFlags.Public);

    public static IReadOnlyCollection<string> Get(UnitChoice choice) =>
        SynonymsProperty?.GetValue(choice) is IEnumerable<string> values
            ? values.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(System.StringComparer.OrdinalIgnoreCase).ToArray()
            : [];
}
