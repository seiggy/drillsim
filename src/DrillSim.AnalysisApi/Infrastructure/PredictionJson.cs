using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Infrastructure;

internal static class PredictionJson
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static string Canonicalize<T>(T value)
    {
        JsonNode node = JsonSerializer.SerializeToNode(value, Options)
            ?? throw new InvalidOperationException($"Could not serialize {typeof(T).Name}.");
        return CanonicalJsonHasher.SerializeCanonical(node);
    }

    public static string ComputeSha256<T>(T value)
    {
        JsonNode node = JsonSerializer.SerializeToNode(value, Options)
            ?? throw new InvalidOperationException($"Could not serialize {typeof(T).Name}.");
        return CanonicalJsonHasher.ComputeCanonicalSha256(node);
    }

    public static T Deserialize<T>(string json, string description)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options)
                ?? throw new InvalidDataException($"Persisted {description} was null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Persisted {description} was invalid JSON.", exception);
        }
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        return options;
    }
}

