using System;
using System.Linq;
using System.Text.Json.Nodes;
using OSDC.UnitConversion.Conversion;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class PhysicalQuantityContractMapper
{
    public static JsonObject ToDetails(BasePhysicalQuantity quantity)
    {
        var units = new JsonArray();
        foreach (UnitChoice choice in quantity.UnitChoices ?? [])
        {
            units.Add(ToUnit(choice));
        }

        var synonyms = new JsonArray();
        foreach (string synonym in (quantity.UsualNames ?? []).OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
        {
            synonyms.Add(synonym);
        }

        return new JsonObject
        {
            ["id"] = quantity.ID.ToString(),
            ["name"] = quantity.Name,
            ["descriptionMarkdown"] = quantity.DescriptionMD,
            ["synonyms"] = synonyms,
            ["typicalSymbol"] = quantity.TypicalSymbol,
            ["siUnit"] = new JsonObject
            {
                ["name"] = quantity.SIUnitName,
                ["label"] = quantity.SIUnitLabel,
                ["labelLatex"] = quantity.SIUnitLabelLatex
            },
            ["meaningfulPrecisionInSI"] = quantity.MeaningfulPrecisionInSI,
            ["dimensions"] = new JsonObject
            {
                ["length"] = quantity.LengthDimension,
                ["mass"] = quantity.MassDimension,
                ["time"] = quantity.TimeDimension,
                ["temperature"] = quantity.TemperatureDimension,
                ["amountOfSubstance"] = quantity.AmountSubstanceDimension,
                ["electricCurrent"] = quantity.ElectricCurrentDimension,
                ["luminousIntensity"] = quantity.LuminousIntensityDimension,
                ["planeAngle"] = quantity.PlaneAngleDimension,
                ["solidAngle"] = quantity.SolidAngleDimension
            },
            ["unitChoices"] = units,
            ["hierarchy"] = PhysicalQuantityMcpMetadata.Create(quantity)
        };
    }

    public static JsonObject ToUnit(UnitChoice choice) => new()
    {
        ["id"] = choice.ID.ToString(),
        ["name"] = choice.UnitName,
        ["label"] = choice.UnitLabel,
        ["isSI"] = choice.IsSI,
        ["isDefault"] = choice.IsDefault,
        ["siUnitName"] = choice.SIUnitName,
        ["synonyms"] = new JsonArray(UnitChoiceSynonymReader.Get(choice).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).Select(value => (JsonNode?)JsonValue.Create(value)).ToArray()),
        ["conversionFromSI"] = new JsonObject
        {
            ["factorFormula"] = choice.ConversionFactorFromSIFormula,
            ["biasFormula"] = choice.ConversionBiasFromSIFormula,
            ["factor"] = choice.ConversionFactorFromSI,
            ["bias"] = choice.ConversionBiasFromSI,
            ["description"] = choice.ConversionDescription
        }
    };
}
