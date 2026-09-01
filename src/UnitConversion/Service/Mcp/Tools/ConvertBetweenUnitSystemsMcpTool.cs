using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;
using OSDC.UnitConversion.Model;
using OSDC.UnitConversion.Service.Controllers;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class ConvertBetweenUnitSystemsMcpTool : IMcpTool
{
    private readonly IServiceProvider _services;
    private static readonly JsonObject Schema = new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["physicalQuantityId"] = Uuid(), ["physicalQuantity"] = Text(),
            ["unitSystemInId"] = Uuid(), ["unitSystemIn"] = Text(),
            ["unitSystemOutId"] = Uuid(), ["unitSystemOut"] = Text(),
            ["values"] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "number" }, ["minItems"] = 1, ["maxItems"] = 1000 }
        },
        ["required"] = new JsonArray("values"),
        ["allOf"] = new JsonArray(Either("physicalQuantityId", "physicalQuantity"), Either("unitSystemInId", "unitSystemIn"), Either("unitSystemOutId", "unitSystemOut")),
        ["additionalProperties"] = false
    };

    public ConvertBetweenUnitSystemsMcpTool(IServiceProvider services) => _services = services;
    public string Name => "convert_between_unit_systems";
    public string Title => "Convert Between Unit Systems";
    public string Description => "Convert one or more finite values between the unit choices selected by two unit systems for a physical quantity. UUID references take precedence over names. Returns the resolved systems and units, unrounded numeric results, and meaningful-precision display values. This operation does not persist data.";
    public JsonNode? InputSchema => Schema;
    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("quantity", "object"), ("inputSystem", "object"), ("outputSystem", "object"), ("inputUnit", "object"), ("outputUnit", "object"), ("results", "array"));

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        BasePhysicalQuantity? quantity = ResolveQuantity(arguments);
        if (quantity is null) return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(404, "Physical quantity not found. Use search_physical_quantities and supply its UUID."));

        using IServiceScope scope = _services.CreateScope();
        var controller = ActivatorUtilities.CreateInstance<UnitSystemController>(scope.ServiceProvider);
        List<DrillingUnitSystem>? systems = Extract(controller.GetAllUnitSystem());
        if (systems is null) return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(500, "Unit systems could not be loaded."));

        if (!ResolveSystem(arguments, "unitSystemInId", "unitSystemIn", systems, out DrillingUnitSystem? systemIn, out JsonNode? error)) return Task.FromResult(error);
        if (!ResolveSystem(arguments, "unitSystemOutId", "unitSystemOut", systems, out DrillingUnitSystem? systemOut, out error)) return Task.FromResult(error);

        UnitChoice? unitIn = systemIn!.GetChoice(quantity.ID);
        UnitChoice? unitOut = systemOut!.GetChoice(quantity.ID);
        if (unitIn is null || unitOut is null)
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(422, $"Both unit systems must define a compatible unit choice for {quantity.Name}."));
        }

        if (!ReadValues(arguments, out List<ValueConversion> values, out error)) return Task.FromResult(error);
        if (!ValueConversion.Calculate(quantity, unitIn, unitOut, values)) return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(500, "The resolved unit-system values could not be converted."));

        var results = new JsonArray(values.Select(value => (JsonNode)new JsonObject
        {
            ["inputValue"] = value.DataIn, ["numericValue"] = value.DataOut, ["formattedValue"] = value.DataOutString
        }).ToArray());

        return Task.FromResult<JsonNode?>(new JsonObject
        {
            ["quantity"] = new JsonObject { ["id"] = quantity.ID.ToString(), ["name"] = quantity.Name },
            ["inputSystem"] = SystemReference(systemIn), ["outputSystem"] = SystemReference(systemOut),
            ["inputUnit"] = UnitReference(unitIn), ["outputUnit"] = UnitReference(unitOut),
            ["meaningfulPrecisionInSI"] = quantity.MeaningfulPrecisionInSI,
            ["siUnit"] = new JsonObject { ["name"] = quantity.SIUnitName, ["label"] = quantity.SIUnitLabel },
            ["results"] = results
        });
    }

    private static BasePhysicalQuantity? ResolveQuantity(JsonObject? args)
    {
        if (TryGuid(args, "physicalQuantityId", out Guid id)) return DrillingPhysicalQuantity.GetQuantity(id) ?? BasePhysicalQuantity.GetQuantity(id);
        string? name = ReadText(args, "physicalQuantity");
        if (name is null) return null;
        string token = McpNameNormalizer.NormalizeText(name);
        List<BasePhysicalQuantity> matches = (DrillingPhysicalQuantity.AvailablePhysicalQuantities ?? [])
            .Where(q => McpNameNormalizer.NormalizeText(q.Name) == token || (q.UsualNames ?? []).Any(s => McpNameNormalizer.NormalizeText(s) == token))
            .DistinctBy(q => q.ID).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    private static bool ResolveSystem(JsonObject? args, string idKey, string nameKey, List<DrillingUnitSystem> systems, out DrillingUnitSystem? system, out JsonNode? error)
    {
        error = null;
        List<DrillingUnitSystem> matches = TryGuid(args, idKey, out Guid id)
            ? systems.Where(item => item.ID == id).ToList()
            : systems.Where(item => McpNameNormalizer.NormalizeText(item.Name) == McpNameNormalizer.NormalizeText(ReadText(args, nameKey) ?? string.Empty)).DistinctBy(item => item.ID).ToList();
        system = matches.Count == 1 ? matches[0] : null;
        if (system is not null) return true;
        error = McpToolResponses.CreateError(404, $"The requested unit system was not found or its name was ambiguous. Use list_unit_systems and supply {idKey}.");
        return false;
    }

    private static List<DrillingUnitSystem>? Extract(ActionResult<IEnumerable<DrillingUnitSystem>> result)
    {
        if (result.Value is not null) return result.Value.ToList();
        return (result.Result as ObjectResult)?.Value is IEnumerable<DrillingUnitSystem> values ? values.ToList() : null;
    }

    private static bool ReadValues(JsonObject? args, out List<ValueConversion> values, out JsonNode? error)
    {
        values = [];
        error = null;
        if (args?["values"] is not JsonArray array || array.Count is < 1 or > 1000)
        {
            error = McpToolResponses.CreateValidationError("Argument 'values' must contain between 1 and 1000 finite numbers."); return false;
        }
        foreach (JsonNode? node in array)
        {
            if (node is not JsonValue value || !value.TryGetValue<double>(out double number) || !double.IsFinite(number))
            { error = McpToolResponses.CreateValidationError("Every item in 'values' must be a finite number."); return false; }
            values.Add(new ValueConversion { DataIn = number });
        }
        return true;
    }

    private static JsonObject SystemReference(DrillingUnitSystem value) => new() { ["id"] = value.ID.ToString(), ["name"] = value.Name, ["description"] = value.Description };
    private static JsonObject UnitReference(UnitChoice value) => new() { ["id"] = value.ID.ToString(), ["name"] = value.UnitName, ["label"] = value.UnitLabel };
    private static bool TryGuid(JsonObject? args, string key, out Guid id) => Guid.TryParse(ReadText(args, key), out id) && id != Guid.Empty;
    private static string? ReadText(JsonObject? args, string key) => args?[key] is JsonValue value && value.TryGetValue<string>(out string? text) && !string.IsNullOrWhiteSpace(text) ? text.Trim() : null;
    private static JsonObject Text() => new() { ["type"] = "string", ["minLength"] = 1 };
    private static JsonObject Uuid() => new() { ["type"] = "string", ["format"] = "uuid" };
    private static JsonObject Either(string first, string second) => new() { ["anyOf"] = new JsonArray(new JsonObject { ["required"] = new JsonArray(first) }, new JsonObject { ["required"] = new JsonArray(second) }) };
}
