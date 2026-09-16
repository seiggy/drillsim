using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReservoirSimulation.Contracts;

public sealed record WorldSetupProfile(
    string ProfileId,
    string Name,
    string Description,
    string WorldModelVersion);

public sealed record WorldSetupProfileCatalog(
    Guid FieldId,
    string ReservoirName,
    IReadOnlyList<WorldSetupProfile> Profiles);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record WorldSetupRequest
{
    [JsonRequired]
    [JsonConverter(typeof(WorldSetupFieldIdJsonConverter))]
    public Guid FieldId { get; init; }

    [JsonRequired]
    public string ReservoirName { get; init; } = string.Empty;

    [JsonRequired]
    public string ProfileId { get; init; } = string.Empty;

    [JsonRequired]
    public string Resolution { get; init; } = string.Empty;

    [JsonRequired]
    [JsonNumberHandling(JsonNumberHandling.Strict)]
    public int RealizationSeed { get; init; }
}

internal sealed class WorldSetupFieldIdJsonConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            !TryParseCanonical(reader.GetString(), out Guid fieldId))
            throw new JsonException("Field ID must be a canonical lowercase, hyphenated GUID.");
        return fieldId;
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("D"));

    internal static bool TryParseCanonical(string? value, out Guid fieldId) =>
        Guid.TryParseExact(value, "D", out fieldId) &&
        string.Equals(value, fieldId.ToString("D"), StringComparison.Ordinal);
}
