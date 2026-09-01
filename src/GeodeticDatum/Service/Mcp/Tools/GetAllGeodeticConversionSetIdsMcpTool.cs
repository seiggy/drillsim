using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class GetAllGeodeticConversionSetIdsMcpTool : GeodeticConversionSetToolBase
{
    public GetAllGeodeticConversionSetIdsMcpTool(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public override string Name => "geodetic_conversion_set_get_all_ids";

    public override string Description => "List the UUID of every persisted geodetic coordinate-conversion case without returning coordinates. Use a UUID with geodetic_conversion_set_get_by_id to retrieve calculated results or with delete_by_id for cleanup.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = CreateControllerScope();
        var result = scope.Controller.GetAllGeodeticConversionSetId();
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }
}
