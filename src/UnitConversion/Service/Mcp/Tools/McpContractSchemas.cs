using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class McpContractSchemas
{
    public static JsonObject Object(params string[] required)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = true
        };
        if (required.Length > 0)
        {
            schema["required"] = new JsonArray(required.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray());
        }
        return schema;
    }

    public static JsonObject TypedObject(params (string Name, string Type)[] properties) => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject(properties.Select(property =>
            new KeyValuePair<string, JsonNode?>(property.Name, new JsonObject { ["type"] = property.Type }))),
        ["required"] = new JsonArray(properties.Select(property => (JsonNode?)JsonValue.Create(property.Name)).ToArray()),
        ["additionalProperties"] = true
    };

    public static JsonObject Array(string itemType = "object") => new()
    {
        ["type"] = "array",
        ["items"] = new JsonObject { ["type"] = itemType }
    };

    public static JsonObject Paged(string itemProperty) => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            [itemProperty] = new JsonObject { ["type"] = "array", ["items"] = new JsonObject { ["type"] = "object" } },
            ["nextCursor"] = new JsonObject { ["type"] = new JsonArray("string", "null") }
        },
        ["required"] = new JsonArray(itemProperty),
        ["additionalProperties"] = false
    };
}
