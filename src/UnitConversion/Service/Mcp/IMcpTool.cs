using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Service.Mcp;

public interface IMcpTool
{
    string Name { get; }

    string Title => Name;

    string Description { get; }

    JsonNode? InputSchema { get; }

    JsonNode? OutputSchema => null;

    bool ReadOnly => true;

    bool Destructive => false;

    bool Idempotent => true;

    bool OpenWorld => false;

    Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken);
}
