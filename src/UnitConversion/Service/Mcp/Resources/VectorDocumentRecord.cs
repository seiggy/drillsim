using System.Text.Json.Nodes;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

public sealed class VectorDocumentRecord
{
    public required string Id { get; init; }

    public required string Uri { get; init; }

    public required string Name { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? MimeType { get; init; }

    public string Content { get; init; } = string.Empty;

    public JsonObject? Metadata { get; init; }

    public long? Size { get; init; }

    public double? SortOrder { get; init; }
}
