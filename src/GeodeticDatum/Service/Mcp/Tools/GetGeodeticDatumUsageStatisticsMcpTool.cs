using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NORCE.Drilling.GeodeticDatum.Service.Controllers;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class GetGeodeticDatumUsageStatisticsMcpTool : IMcpTool
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GetGeodeticDatumUsageStatisticsMcpTool(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Name => "geodetic_datum_usage_statistics_get";

    public string Description => "Retrieve administrative usage counters collected for GeodeticDatum REST operations. This reports endpoint activity rather than spheroid, datum, or coordinate-conversion data and requires no arguments.";

    public JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var scope = _scopeFactory.CreateScope();
        var controller = scope.ServiceProvider.GetRequiredService<GeodeticDatumUsageStatisticsController>();
        var result = controller.GetGeodeticDatumUsageStatistics();
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }
}
