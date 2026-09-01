using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.CartographicProjection.Service.Controllers;
using NORCE.Drilling.CartographicProjection.Service.Managers;
using CartographicProjectionModel = NORCE.Drilling.CartographicProjection.Model.CartographicProjection;

namespace NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;

internal abstract class CartographicProjectionToolBase : IMcpTool
{
    private protected readonly ILoggerFactory LoggerFactory;
    private protected readonly SqlConnectionManager ConnectionManager;

    protected CartographicProjectionToolBase(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
    {
        LoggerFactory = loggerFactory;
        ConnectionManager = connectionManager;
    }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract JsonNode? InputSchema { get; }

    public abstract Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken);

    protected CartographicProjectionController CreateController()
    {
        return new CartographicProjectionController(
            LoggerFactory.CreateLogger<CartographicProjectionManager>(),
            ConnectionManager);
    }

    protected static bool TryDeserialize(JsonObject? arguments, out CartographicProjectionModel projection, out JsonNode? error)
    {
        projection = default!;
        error = null;

        if (arguments?["cartographicProjection"] is not JsonNode projectionNode)
        {
            error = McpToolResponses.CreateValidationError("Argument 'cartographicProjection' is required.");
            return false;
        }

        try
        {
            projection = projectionNode.Deserialize<CartographicProjectionModel>(JsonSettings.Options) ?? throw new InvalidOperationException();
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError("Argument 'cartographicProjection' could not be deserialized.");
            return false;
        }
    }

    protected static JsonObject CreateProjectionSchema(bool includeId)
        => McpToolArgumentHelpers.CreateProjectionSchema(includeId);
}

internal sealed class GetAllCartographicProjectionIdsMcpTool : CartographicProjectionToolBase
{
    public GetAllCartographicProjectionIdsMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_get_all_ids";

    public override string Description => "List the UUID of every stored cartographic projection without transferring complete parameter sets. Use these identifiers with get_by_id, coordinate-conversion cases, or resources such as Fields that reference a projection.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicProjectionId());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicProjectionMetaInfoMcpTool : CartographicProjectionToolBase
{
    public GetAllCartographicProjectionMetaInfoMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_get_all_meta_info";

    public override string Description => "List identity and optional HTTP location metadata for every stored cartographic projection without returning complete projection parameters. Use this for resource discovery when full definitions are unnecessary.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicProjectionMetaInfo());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetCartographicProjectionByIdMcpTool : CartographicProjectionToolBase
{
    public GetCartographicProjectionByIdMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_get_by_id";

    public override string Description => "Retrieve one complete cartographic projection by UUID, including its algorithm, associated GeodeticDatum, angular, linear, scale, zone, satellite, and specialized parameters. Returns 404 when absent and 400 for an empty UUID.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "Unique identifier of the cartographic projection to retrieve.");

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out Guid id, out JsonNode? error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().GetCartographicProjectionById(id));
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicProjectionLightMcpTool : CartographicProjectionToolBase
{
    public GetAllCartographicProjectionLightMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_get_all_light";

    public override string Description => "Retrieve lightweight records for every cartographic projection for discovery and selection workflows. Results retain core identity and projection context while avoiding complete parameter payloads.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicProjectionLight());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicProjectionMcpTool : CartographicProjectionToolBase
{
    public GetAllCartographicProjectionMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_get_all";

    public override string Description => "Retrieve every stored cartographic projection as a complete parameter definition. Use the light, ID, or metadata tools when full algorithm-specific parameters are unnecessary.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicProjection());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class PostCartographicProjectionMcpTool : CartographicProjectionToolBase
{
    private static readonly JsonObject Schema = CreateProjectionSchema(includeId: false);

    public PostCartographicProjectionMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_create";

    public override string Description => "Create and persist a complete cartographic projection. cartographicProjection.MetaInfo.ID must be a caller-generated, non-empty UUID, GeodeticDatumID should identify the datum used by the projection, and only parameters applicable to ProjectionType are used. Returns 200 on success, 400 for malformed data, and 409 for a duplicate ID.";

    public override JsonNode? InputSchema => Schema;

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryDeserialize(arguments, out CartographicProjectionModel projection, out JsonNode? error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().PostCartographicProjection(projection));
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class PutCartographicProjectionByIdMcpTool : CartographicProjectionToolBase
{
    private static readonly JsonObject Schema = CreateProjectionSchema(includeId: true);

    public PutCartographicProjectionByIdMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_update_by_id";

    public override string Description => "Replace an existing cartographic projection with the complete supplied definition. The top-level id must equal cartographicProjection.MetaInfo.ID; this is a full update rather than a patch. Review dependent Fields and conversion cases before changing projection semantics. Returns 400 for mismatched IDs and 404 when absent.";

    public override JsonNode? InputSchema => Schema;

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out Guid id, out JsonNode? idError))
        {
            return Task.FromResult<JsonNode?>(idError);
        }
        if (!TryDeserialize(arguments, out CartographicProjectionModel projection, out JsonNode? projectionError))
        {
            return Task.FromResult<JsonNode?>(projectionError);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().PutCartographicProjectionById(id, projection));
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class DeleteCartographicProjectionByIdMcpTool : CartographicProjectionToolBase
{
    public DeleteCartographicProjectionByIdMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_projection_delete_by_id";

    public override string Description => "Permanently delete one stored cartographic projection by UUID. Check Fields and coordinate-conversion cases that may still reference it before deletion because those workflows require a valid projection. Returns 200 on success and 404 when absent.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "Unique identifier of the cartographic projection to delete.");

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out Guid id, out JsonNode? error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().DeleteCartographicProjectionById(id));
        return Task.FromResult<JsonNode?>(response);
    }
}
