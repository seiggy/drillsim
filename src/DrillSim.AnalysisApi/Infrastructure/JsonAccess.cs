using System.Text.Json.Nodes;

namespace DrillSim.AnalysisApi.Infrastructure;

internal static class JsonAccess
{
    public static JsonNode? Get(JsonNode? parent, string name)
    {
        if (parent is not JsonObject jsonObject)
            return null;
        foreach (var (key, value) in jsonObject)
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
                return value;
        return null;
    }

    public static Guid? Guid(JsonNode? parent, string name)
    {
        JsonNode? node = Get(parent, name);
        if (node is null)
            return null;
        if (node is JsonValue value && value.TryGetValue<Guid>(out Guid guid))
            return guid;
        return System.Guid.TryParse(node.ToJsonString().Trim((char)34), out guid) ? guid : null;
    }

    public static Guid? MetaId(JsonNode? node) => Guid(Get(node, "MetaInfo"), "ID");

    public static double? Number(JsonNode? parent, string name)
    {
        JsonNode? node = Get(parent, name);
        if (node is not JsonValue value)
            return null;
        if (value.TryGetValue<double>(out double number))
            return double.IsFinite(number) ? number : null;
        return double.TryParse(value.ToString(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out number) && double.IsFinite(number) ? number : null;
    }

    public static string? String(JsonNode? parent, string name)
    {
        JsonNode? node = Get(parent, name);
        return node is JsonValue value && value.TryGetValue<string>(out string? text) ? text : null;
    }

    public static double? GaussianMean(JsonNode? parent, string name)
    {
        JsonNode? property = Get(parent, name);
        return Number(Get(property, "GaussianValue"), "Mean")
            ?? Number(property, "Mean")
            ?? (property is JsonValue ? Number(parent, name) : null);
    }
}
