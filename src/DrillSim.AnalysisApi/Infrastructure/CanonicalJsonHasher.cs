using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Infrastructure;

public interface IPackageHasher
{
    string Compute(Guid fieldId, JsonNode field, IReadOnlyList<JsonNode> clusters,
        IReadOnlyList<JsonNode> wells, IReadOnlyList<JsonNode> wellBores,
        IReadOnlyList<JsonNode> wellBoreArchitectures,
        IReadOnlyList<JsonNode> trajectories, IReadOnlyList<JsonNode> geologicalProperties,
        SourceCounts sourceCounts, IReadOnlyList<string> dataGaps);
}

public sealed class CanonicalJsonHasher : IPackageHasher
{
    public string Compute(Guid fieldId, JsonNode field, IReadOnlyList<JsonNode> clusters,
        IReadOnlyList<JsonNode> wells, IReadOnlyList<JsonNode> wellBores,
        IReadOnlyList<JsonNode> wellBoreArchitectures,
        IReadOnlyList<JsonNode> trajectories, IReadOnlyList<JsonNode> geologicalProperties,
        SourceCounts sourceCounts, IReadOnlyList<string> dataGaps)
    {
        var content = new JsonObject
        {
            ["fieldId"] = fieldId,
            ["field"] = field.DeepClone(),
            ["clusters"] = ToArray(clusters),
            ["wells"] = ToArray(wells),
            ["wellBores"] = ToArray(wellBores),
            ["wellBoreArchitectures"] = ToArray(wellBoreArchitectures),
            ["trajectories"] = ToArray(trajectories),
            ["geologicalProperties"] = ToArray(geologicalProperties),
            ["sourceCounts"] = JsonNode.Parse(JsonSerializer.Serialize(sourceCounts)),
            ["dataGaps"] = new JsonArray(dataGaps.Select(gap => (JsonNode?)JsonValue.Create(gap)).ToArray())
        };

        return ComputeCanonicalSha256(content);
    }

    internal static string ComputeCanonicalSha256(JsonNode content) =>
        Convert.ToHexString(SHA256.HashData(SerializeCanonicalBytes(content))).ToLowerInvariant();

    internal static string SerializeCanonical(JsonNode content) =>
        Encoding.UTF8.GetString(SerializeCanonicalBytes(content));

    private static byte[] SerializeCanonicalBytes(JsonNode content)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
            WriteCanonical(writer, content);
        return stream.ToArray();
    }

    private static JsonArray ToArray(IReadOnlyList<JsonNode> items) =>
        new(items.Select(item => item.DeepClone()).ToArray());

    private static void WriteCanonical(Utf8JsonWriter writer, JsonNode? node)
    {
        switch (node)
        {
            case null:
                writer.WriteNullValue();
                break;
            case JsonObject jsonObject:
                writer.WriteStartObject();
                foreach (var (name, value) in jsonObject.OrderBy(property => property.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(name);
                    WriteCanonical(writer, value);
                }
                writer.WriteEndObject();
                break;
            case JsonArray array:
                writer.WriteStartArray();
                foreach (JsonNode? item in array)
                    WriteCanonical(writer, item);
                writer.WriteEndArray();
                break;
            default:
                node.WriteTo(writer);
                break;
        }
    }
}

