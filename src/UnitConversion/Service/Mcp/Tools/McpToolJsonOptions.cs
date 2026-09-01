using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class McpToolJsonOptions
{
    public static JsonSerializerOptions Default { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = null
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

