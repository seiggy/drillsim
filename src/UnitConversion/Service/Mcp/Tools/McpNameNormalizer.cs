using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class McpNameNormalizer
{
    private static readonly IReadOnlyDictionary<string, string> UnitWordAliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["meter"] = "metre", ["meters"] = "metre", ["metres"] = "metre",
            ["liter"] = "litre", ["liters"] = "litre", ["litres"] = "litre",
            ["foot"] = "foot", ["feet"] = "foot", ["ft"] = "foot",
            ["inch"] = "inch", ["inches"] = "inch",
            ["yard"] = "yard", ["yards"] = "yard", ["yd"] = "yard",
            ["mile"] = "mile", ["miles"] = "mile",
            ["furlong"] = "furlong", ["furlongs"] = "furlong",
            ["second"] = "second", ["seconds"] = "second", ["sec"] = "second", ["secs"] = "second", ["s"] = "second",
            ["minute"] = "minute", ["minutes"] = "minute", ["min"] = "minute", ["mins"] = "minute",
            ["hour"] = "hour", ["hours"] = "hour", ["hr"] = "hour", ["hrs"] = "hour", ["h"] = "hour",
            ["day"] = "day", ["days"] = "day", ["d"] = "day",
            ["week"] = "week", ["weeks"] = "week",
            ["fortnight"] = "fortnight", ["fortnights"] = "fortnight",
            ["degree"] = "degree", ["degrees"] = "degree",
            ["celsius"] = "celsius", ["centigrade"] = "celsius",
            ["fahrenheit"] = "fahrenheit",
            ["kelvins"] = "kelvin",
            ["pounds"] = "pound", ["lbs"] = "pound",
            ["grams"] = "gram", ["kilograms"] = "kilogram"
        };

    public static string NormalizeText(string value) => JoinNormalizedWords(value, useUnitAliases: false);

    public static string NormalizeUnit(string value) => JoinNormalizedWords(value, useUnitAliases: true);

    private static string JoinNormalizedWords(string value, bool useUnitAliases)
    {
        string decomposed = value.Normalize(NormalizationForm.FormD);
        var words = new List<string>();
        var current = new StringBuilder();

        void FlushWord()
        {
            if (current.Length == 0) return;
            string word = current.ToString();
            words.Add(useUnitAliases && UnitWordAliases.TryGetValue(word, out string? canonical) ? canonical : word);
            current.Clear();
        }

        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(character))
            {
                current.Append(char.ToLowerInvariant(character));
                continue;
            }

            FlushWord();
            if (useUnitAliases && character is '/' or '⁄') words.Add("per");
            else if (useUnitAliases && character == '°') words.Add("degree");
        }

        FlushWord();
        return string.Concat(words);
    }
}
