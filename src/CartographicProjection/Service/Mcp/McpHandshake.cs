using System.Text.Json.Nodes;

namespace NORCE.Drilling.CartographicProjection.Service.Mcp;

public sealed record McpHandshake(
    string ProtocolVersion,
    string? ClientName,
    string? ClientVersion,
    string? SessionId,
    JsonObject? Capabilities);
