using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSDC.Drilling.EarthMagneticField.Service;

/// <summary>Requires an explicit zero UTC offset instead of relying on the server's local time zone.</summary>
public sealed class StrictUtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("A UTC date-time string is required.");

        string? text = reader.GetString();
        bool explicitUtc = text != null &&
            (text.EndsWith("Z", StringComparison.OrdinalIgnoreCase) || text.EndsWith("+00:00", StringComparison.Ordinal));
        if (!explicitUtc || !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out DateTimeOffset value) || value.Offset != TimeSpan.Zero)
            throw new JsonException("DateTimeUtc must contain Z or +00:00.");
        return value.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
}
