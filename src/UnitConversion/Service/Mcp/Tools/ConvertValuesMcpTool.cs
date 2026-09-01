using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Model;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class ConvertValuesMcpTool : IMcpTool
{
    private static readonly JsonObject Schema = new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["physicalQuantityId"] = Uuid("Exact physical-quantity UUID."),
            ["physicalQuantity"] = Text("Canonical physical-quantity name or synonym."),
            ["unitInId"] = Uuid("Exact source unit-choice UUID."),
            ["unitIn"] = Text("Source unit name, label, symbol, or synonym."),
            ["unitOutId"] = Uuid("Exact target unit-choice UUID."),
            ["unitOut"] = Text("Target unit name, label, symbol, or synonym."),
            ["values"] = new JsonObject
            {
                ["type"] = "array", ["items"] = new JsonObject { ["type"] = "number" },
                ["minItems"] = 1, ["maxItems"] = 1000,
                ["description"] = "Finite values expressed in the resolved source unit."
            }
        },
        ["required"] = new JsonArray("values"),
        ["allOf"] = new JsonArray(
            Either("physicalQuantityId", "physicalQuantity"),
            Either("unitInId", "unitIn"),
            Either("unitOutId", "unitOut")),
        ["additionalProperties"] = false
    };

    public string Name => "convert_values";
    public string Title => "Convert Values";
    public string Description => "Convert one or more finite values between unit choices compatible with a physical quantity. UUID references take precedence over names. Specialized quantities may inherit units from compatible ancestors. Returns unrounded numeric results, meaningful-precision display values, resolved identities, and symbolic conversion definitions. This operation does not persist data.";
    public JsonNode? InputSchema => Schema;
    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("quantity", "object"), ("inputUnit", "object"), ("outputUnit", "object"), ("results", "array"));

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (!TryResolveQuantity(arguments, out BasePhysicalQuantity? quantity, out JsonNode? error)) return Task.FromResult(error);
        if (!TryResolveUnit(arguments, "unitInId", "unitIn", quantity!, out ResolvedUnit input, out error)) return Task.FromResult(error);
        if (!TryResolveUnit(arguments, "unitOutId", "unitOut", quantity!, out ResolvedUnit output, out error)) return Task.FromResult(error);
        if (!TryReadValues(arguments, out List<ValueConversion> values, out error)) return Task.FromResult(error);

        if (!ValueConversion.Calculate(quantity!, input.Choice, output.Choice, values))
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(500, "The resolved values and units could not be converted."));
        }

        var results = new JsonArray();
        foreach (ValueConversion value in values)
        {
            results.Add(new JsonObject
            {
                ["inputValue"] = value.DataIn,
                ["numericValue"] = value.DataOut,
                ["formattedValue"] = value.DataOutString
            });
        }

        return Task.FromResult<JsonNode?>(new JsonObject
        {
            ["quantity"] = QuantityReference(quantity!),
            ["inputUnit"] = UnitReference(input, quantity!),
            ["outputUnit"] = UnitReference(output, quantity!),
            ["meaningfulPrecisionInSI"] = quantity!.MeaningfulPrecisionInSI,
            ["siUnit"] = new JsonObject { ["name"] = quantity.SIUnitName, ["label"] = quantity.SIUnitLabel },
            ["results"] = results
        });
    }

    private static bool TryResolveQuantity(JsonObject? args, out BasePhysicalQuantity? quantity, out JsonNode? error)
    {
        quantity = null;
        error = null;
        if (TryGuid(args, "physicalQuantityId", out Guid id))
        {
            quantity = DrillingPhysicalQuantity.GetQuantity(id) ?? BasePhysicalQuantity.GetQuantity(id);
        }
        else if (ReadText(args, "physicalQuantity") is { } name)
        {
            string token = McpNameNormalizer.NormalizeText(name);
            var matches = (DrillingPhysicalQuantity.AvailablePhysicalQuantities ?? [])
                .Where(q => McpNameNormalizer.NormalizeText(q.Name) == token || (q.UsualNames ?? []).Any(s => McpNameNormalizer.NormalizeText(s) == token))
                .DistinctBy(q => q.ID).ToList();
            if (matches.Count == 1) quantity = matches[0];
            else if (matches.Count > 1)
            {
                error = McpToolResponses.CreateValidationError($"Physical quantity '{name}' is ambiguous. Use search_physical_quantities and supply physicalQuantityId.");
                return false;
            }
        }
        else
        {
            error = McpToolResponses.CreateValidationError("Supply physicalQuantityId or physicalQuantity.");
            return false;
        }

        if (quantity is null)
        {
            error = McpToolResponses.CreateError(404, "The requested physical quantity was not found. Use search_physical_quantities to resolve it.");
            return false;
        }
        return true;
    }

    private static bool TryResolveUnit(JsonObject? args, string idKey, string nameKey, BasePhysicalQuantity quantity, out ResolvedUnit resolved, out JsonNode? error)
    {
        resolved = default;
        error = null;
        List<ResolvedUnit> available = PhysicalQuantityHierarchy.Enumerate(quantity)
            .SelectMany(declaring => (declaring.UnitChoices ?? []).Select(choice => new ResolvedUnit(choice, declaring)))
            .DistinctBy(item => item.Choice.ID).ToList();

        if (TryGuid(args, idKey, out Guid id))
        {
            List<ResolvedUnit> byId = available.Where(item => item.Choice.ID == id).ToList();
            if (byId.Count == 1) resolved = byId[0];
        }
        else if (ReadText(args, nameKey) is { } name)
        {
            string token = McpNameNormalizer.NormalizeUnit(name);
            List<ResolvedUnit> exact = available.Where(item => UnitLabels(item.Choice).Any(label => McpNameNormalizer.NormalizeUnit(label) == token)).ToList();
            if (exact.Count == 1) resolved = exact[0];
            else if (exact.Count > 1)
            {
                error = McpToolResponses.CreateValidationError($"Unit '{name}' is ambiguous for {quantity.Name}; supply {idKey}.");
                return false;
            }
        }
        else
        {
            error = McpToolResponses.CreateValidationError($"Supply {idKey} or {nameKey}.");
            return false;
        }

        if (resolved.Choice is null)
        {
            error = McpToolResponses.CreateError(404, $"The requested unit was not found for {quantity.Name} or its compatible ancestors. Inspect get_physical_quantity results and parent quantities.");
            return false;
        }
        return true;
    }

    private static IEnumerable<string> UnitLabels(UnitChoice choice)
    {
        if (!string.IsNullOrWhiteSpace(choice.UnitName)) yield return choice.UnitName;
        if (!string.IsNullOrWhiteSpace(choice.UnitLabel)) yield return choice.UnitLabel;
        foreach (string synonym in UnitChoiceSynonymReader.Get(choice)) yield return synonym;
    }

    private static bool TryReadValues(JsonObject? args, out List<ValueConversion> values, out JsonNode? error)
    {
        values = [];
        error = null;
        if (args?["values"] is not JsonArray array || array.Count is < 1 or > 1000)
        {
            error = McpToolResponses.CreateValidationError("Argument 'values' must contain between 1 and 1000 finite numbers.");
            return false;
        }
        foreach (JsonNode? node in array)
        {
            if (node is not JsonValue value || !value.TryGetValue<double>(out double number) || !double.IsFinite(number))
            {
                error = McpToolResponses.CreateValidationError("Every item in 'values' must be a finite number.");
                return false;
            }
            values.Add(new ValueConversion { DataIn = number });
        }
        return true;
    }

    private static JsonObject QuantityReference(BasePhysicalQuantity quantity) => new() { ["id"] = quantity.ID.ToString(), ["name"] = quantity.Name };
    private static JsonObject UnitReference(ResolvedUnit unit, BasePhysicalQuantity requested) => new()
    {
        ["id"] = unit.Choice.ID.ToString(), ["name"] = unit.Choice.UnitName, ["label"] = unit.Choice.UnitLabel,
        ["declaredByQuantity"] = QuantityReference(unit.DeclaringQuantity), ["inherited"] = unit.DeclaringQuantity.ID != requested.ID,
        ["conversionFromSI"] = new JsonObject
        {
            ["factorFormula"] = unit.Choice.ConversionFactorFromSIFormula, ["biasFormula"] = unit.Choice.ConversionBiasFromSIFormula,
            ["factor"] = unit.Choice.ConversionFactorFromSI, ["bias"] = unit.Choice.ConversionBiasFromSI
        }
    };

    private static bool TryGuid(JsonObject? args, string key, out Guid id) => Guid.TryParse(ReadText(args, key), out id) && id != Guid.Empty;
    private static string? ReadText(JsonObject? args, string key) => args?[key] is JsonValue value && value.TryGetValue<string>(out string? text) && !string.IsNullOrWhiteSpace(text) ? text.Trim() : null;
    private static JsonObject Text(string description) => new() { ["type"] = "string", ["minLength"] = 1, ["description"] = description };
    private static JsonObject Uuid(string description) => new() { ["type"] = "string", ["format"] = "uuid", ["description"] = description };
    private static JsonObject Either(string first, string second) => new() { ["anyOf"] = new JsonArray(new JsonObject { ["required"] = new JsonArray(first) }, new JsonObject { ["required"] = new JsonArray(second) }) };
    private readonly record struct ResolvedUnit(UnitChoice Choice, BasePhysicalQuantity DeclaringQuantity);
}
