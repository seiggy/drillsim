using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.CartographicProjection.Service.Controllers;

namespace NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;

internal sealed class GetCartographicProjectionUsageStatisticsMcpTool : IMcpTool
{
    private readonly ILoggerFactory _loggerFactory;

    public GetCartographicProjectionUsageStatisticsMcpTool(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public string Name => "cartographic_projection_usage_statistics_get";

    public string Description => "Retrieve the CartographicProjection microservice usage counters collected for REST operations. This administrative result reports endpoint activity rather than projection definitions or coordinate-conversion data and requires no arguments.";

    public JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var controller = new CartographicProjectionUsageStatisticsController(
            _loggerFactory.CreateLogger<CartographicProjectionUsageStatisticsController>());
        var response = McpActionResultConverter.FromActionResult(controller.GetCartographicProjectionUsageStatistics());
        return Task.FromResult<JsonNode?>(response);
    }
}
