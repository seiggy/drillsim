using System.Text.Json.Nodes;

namespace OSDC.Drilling.EarthMagneticField.Service.Mcp.Tools;

public sealed class PingMcpTool : IMcpTool
{
    public string Name => "ping";
    public string Description => "Checks whether the stateless OSDC Earth Magnetic Field MCP server is reachable. It performs no geomagnetic evaluation, reads no coefficients, persists nothing, and does not expose usage statistics. A successful result is {\"Status\":\"ok\",\"Service\":\"OSDC Earth Magnetic Field\"}.";
    public JsonNode InputSchema { get; } = JsonNode.Parse("""{"type":"object","properties":{},"additionalProperties":false}""")!;
    public JsonNode OutputSchema { get; } = JsonNode.Parse("""
    {
      "type":"object",
      "properties":{
        "Status":{"type":"string","const":"ok"},
        "Service":{"type":"string","const":"OSDC Earth Magnetic Field"}
      },
      "required":["Status","Service"],
      "additionalProperties":false
    }
    """)!;

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken) =>
        Task.FromResult<JsonNode?>(new JsonObject { ["Status"] = "ok", ["Service"] = "OSDC Earth Magnetic Field" });
}
