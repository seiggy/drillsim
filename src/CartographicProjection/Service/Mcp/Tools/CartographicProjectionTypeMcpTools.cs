using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.CartographicProjection.Model;
using NORCE.Drilling.CartographicProjection.Service.Controllers;

namespace NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;

internal abstract class CartographicProjectionTypeToolBase : IMcpTool
{
    private readonly ILoggerFactory _loggerFactory;

    protected CartographicProjectionTypeToolBase(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract JsonNode? InputSchema { get; }

    public abstract Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken);

    protected CartographicProjectionTypeController CreateController()
    {
        return new CartographicProjectionTypeController(_loggerFactory.CreateLogger<CartographicProjectionType>());
    }
}

internal sealed class GetAllCartographicProjectionTypeIdsMcpTool : CartographicProjectionTypeToolBase
{
    public GetAllCartographicProjectionTypeIdsMcpTool(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    public override string Name => "cartographic_projection_type_get_all_ids";

    public override string Description => "List the string identifiers of all supported cartographic projection algorithms, such as UTM, TransverseMercator, and LambertConformalConic. Pass an identifier to cartographic_projection_type_get_by_id to discover its applicable parameters.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicProjectionTypeId());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetCartographicProjectionTypeByIdMcpTool : CartographicProjectionTypeToolBase
{
    public GetCartographicProjectionTypeByIdMcpTool(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    public override string Name => "cartographic_projection_type_get_by_id";

    public override string Description => "Retrieve the prototype for one supported projection algorithm by its case-sensitive ProjectionType name. The prototype's Use... flags identify exactly which CartographicProjection parameters apply when defining that algorithm; it is reference data, not a persisted projection.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateStringSchema("id", "Case-sensitive ProjectionType name returned by cartographic_projection_type_get_all_ids, for example UTM or TransverseMercator.");

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseString(arguments, "id", out string id, out JsonNode? error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().GetCartographicProjectionTypeById(id));
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicProjectionTypeMcpTool : CartographicProjectionTypeToolBase
{
    public GetAllCartographicProjectionTypeMcpTool(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    public override string Name => "cartographic_projection_type_get_all";

    public override string Description => "Retrieve every supported cartographic projection algorithm prototype. Each prototype exposes Use... flags that show which angular, linear, zone, scale, satellite, or specialized parameters are applicable when creating a CartographicProjection.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicProjectionType());
        return Task.FromResult<JsonNode?>(response);
    }
}
