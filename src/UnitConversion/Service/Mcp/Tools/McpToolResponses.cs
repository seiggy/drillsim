using System.Text.Json.Nodes;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class McpToolResponses
{
    public static JsonNode CreateError(int statusCode, string message)
    {
        return new JsonObject
        {
            ["status"] = statusCode,
            ["error"] = message
        };
    }

    public static JsonNode CreateValidationError(string message) => CreateError(400, message);

    public static JsonNode CreateSuccess(string message)
    {
        var payload = new JsonObject
        {
            ["status"] = "ok"
        };

        if (!string.IsNullOrWhiteSpace(message))
        {
            payload["message"] = message;
        }

        return payload;
    }
}
