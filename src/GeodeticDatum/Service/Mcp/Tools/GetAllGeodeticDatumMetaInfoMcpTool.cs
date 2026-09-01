using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class GetAllGeodeticDatumMetaInfoMcpTool : GeodeticDatumToolBase
{
    public GetAllGeodeticDatumMetaInfoMcpTool(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public override string Name => "geodetic_datum_get_all_meta";

    public override string Description => "List identity and optional HTTP location metadata for every stored geodetic datum without returning spheroid or seven-parameter transformation values. Use this for inexpensive resource discovery.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = CreateControllerScope();
        var result = scope.Controller.GetAllGeodeticDatumMetaInfo();
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }
}
