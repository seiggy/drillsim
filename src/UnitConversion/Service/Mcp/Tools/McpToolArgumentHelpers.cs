using System;
using System.Text.Json.Nodes;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class McpToolArgumentHelpers
{
    public static JsonObject CreateGuidSchema(string propertyName, string description) => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            [propertyName] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["description"] = description }
        },
        ["required"] = new JsonArray(propertyName),
        ["additionalProperties"] = false
    };

    public static JsonObject CreateUnitSystemSchema(bool includeId = false)
    {
        var properties = new JsonObject { ["unitSystem"] = CreateUnitSystemObjectSchema() };
        var required = new JsonArray("unitSystem");
        if (includeId)
        {
            properties.Insert(0, "id", new JsonObject { ["type"] = "string", ["format"] = "uuid", ["description"] = "UUID of the unit system to replace." });
            required.Insert(0, "id");
        }
        return new JsonObject
        {
            ["type"] = "object", ["properties"] = properties, ["required"] = required, ["additionalProperties"] = false
        };
    }

    public static bool TryParseGuid(JsonObject? arguments, string key, out Guid value, out JsonNode? error)
    {
        value = Guid.Empty;
        error = null;
        if (arguments?[key] is not JsonValue node || !node.TryGetValue<string>(out string? text) ||
            !Guid.TryParse(text, out value) || value == Guid.Empty)
        {
            error = McpToolResponses.CreateValidationError($"Argument '{key}' must be a non-empty UUID.");
            return false;
        }
        return true;
    }

    private static JsonObject CreateUnitSystemObjectSchema() => new()
    {
        ["type"] = "object",
        ["description"] = "A named mapping from physical-quantity UUIDs to compatible unit-choice UUIDs.",
        ["properties"] = new JsonObject
        {
            ["id"] = new JsonObject { ["type"] = "string", ["format"] = "uuid", ["description"] = "Stable caller-assigned UUID." },
            ["name"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 },
            ["description"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
            ["isDefault"] = new JsonObject { ["type"] = "boolean", ["description"] = "Custom systems should normally set this to false." },
            ["choices"] = new JsonObject
            {
                ["type"] = "object",
                ["description"] = "Map from physical-quantity UUID strings to compatible unit-choice UUID strings.",
                ["minProperties"] = 1,
                ["additionalProperties"] = new JsonObject { ["type"] = "string", ["format"] = "uuid" }
            }
        },
        ["required"] = new JsonArray("id", "name", "choices"),
        ["additionalProperties"] = false
    };
}
