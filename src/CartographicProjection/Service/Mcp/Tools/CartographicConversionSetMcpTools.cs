using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.CartographicProjection.Service.Controllers;
using NORCE.Drilling.CartographicProjection.Service.Managers;
using CartographicConversionSetModel = NORCE.Drilling.CartographicProjection.Model.CartographicConversionSet;

namespace NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;

internal abstract class CartographicConversionSetToolBase : IMcpTool
{
    private protected readonly ILoggerFactory LoggerFactory;
    private protected readonly SqlConnectionManager ConnectionManager;

    protected CartographicConversionSetToolBase(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
    {
        LoggerFactory = loggerFactory;
        ConnectionManager = connectionManager;
    }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract JsonNode? InputSchema { get; }

    public abstract Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken);

    protected CartographicConversionSetController CreateController()
    {
        return new CartographicConversionSetController(
            LoggerFactory.CreateLogger<CartographicConversionSetManager>(),
            ConnectionManager);
    }

    protected static bool TryDeserialize(JsonObject? arguments, out CartographicConversionSetModel conversionSet, out JsonNode? error)
    {
        conversionSet = default!;
        error = null;

        if (arguments?["cartographicConversionSet"] is not JsonNode conversionSetNode)
        {
            error = McpToolResponses.CreateValidationError("Argument 'cartographicConversionSet' is required.");
            return false;
        }

        try
        {
            conversionSet = conversionSetNode.Deserialize<CartographicConversionSetModel>(JsonSettings.Options) ?? throw new InvalidOperationException();
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError("Argument 'cartographicConversionSet' could not be deserialized.");
            return false;
        }
    }

    protected static JsonObject CreateConversionSetSchema(bool includeId)
        => McpToolArgumentHelpers.CreateConversionSetSchema(includeId);
}

internal sealed class GetAllCartographicConversionSetIdsMcpTool : CartographicConversionSetToolBase
{
    public GetAllCartographicConversionSetIdsMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_get_all_ids";

    public override string Description => "List the UUID of every persisted cartographic coordinate-conversion case without returning coordinates. Use a case UUID with cartographic_conversion_set_get_by_id to retrieve calculated results or with the delete tool for cleanup.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicConversionSetId());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicConversionSetMetaInfoMcpTool : CartographicConversionSetToolBase
{
    public GetAllCartographicConversionSetMetaInfoMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_get_all_meta_info";

    public override string Description => "List identity and optional HTTP location metadata for every persisted cartographic coordinate-conversion case without transferring its coordinate list. Use this to discover cases before retrieving full results.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicConversionSetMetaInfo());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetCartographicConversionSetByIdMcpTool : CartographicConversionSetToolBase
{
    public GetCartographicConversionSetByIdMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_get_by_id";

    public override string Description => "Retrieve a persisted coordinate-conversion case and its calculated results using the UUID assigned during creation. Call this after cartographic_conversion_set_create; inspect each item for projected, datum, WGS84, octree, and grid-convergence values, then delete temporary cases when finished. Returns 404 when absent.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "UUID assigned to the coordinate-conversion case when it was created.");

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out Guid id, out JsonNode? error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().GetCartographicConversionSetById(id));
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicConversionSetLightMcpTool : CartographicConversionSetToolBase
{
    public GetAllCartographicConversionSetLightMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_get_all_light";

    public override string Description => "Retrieve lightweight metadata for every persisted coordinate-conversion case, including its selected CartographicProjection but not its coordinate list. Use get_by_id to obtain calculated coordinates for a selected case.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicConversionSetLight());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class GetAllCartographicConversionSetMcpTool : CartographicConversionSetToolBase
{
    public GetAllCartographicConversionSetMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_get_all";

    public override string Description => "Retrieve every persisted cartographic coordinate-conversion case with its complete calculated coordinate list. This may transfer substantial data; prefer the ID, metadata, light, or get-by-ID tools for targeted workflows.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateEmptySchema();

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = McpActionResultConverter.FromActionResult(CreateController().GetAllCartographicConversionSet());
        return Task.FromResult<JsonNode?>(response);
    }
}

internal sealed class PostCartographicConversionSetMcpTool : CartographicConversionSetToolBase
{
    private static readonly JsonObject Schema = CreateConversionSetSchema(includeId: false);

    public PostCartographicConversionSetMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_create";

    public override string Description => "Create and calculate a persistent coordinate-conversion case. Generate a new UUID in cartographicConversionSet.MetaInfo.ID, select an existing CartographicProjectionID, and supply one or more coordinate inputs. A successful 200 response confirms persistence: call cartographic_conversion_set_get_by_id with the same UUID to retrieve calculated results, then delete_by_id to remove a temporary case.";

    public override JsonNode? InputSchema => Schema;

    public override async Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryDeserialize(arguments, out CartographicConversionSetModel conversionSet, out JsonNode? error))
        {
            return error;
        }

        var response = McpActionResultConverter.FromActionResult(await CreateController().PostCartographicConversionSet(conversionSet).ConfigureAwait(false));
        return response;
    }
}

internal sealed class PutCartographicConversionSetByIdMcpTool : CartographicConversionSetToolBase
{
    private static readonly JsonObject Schema = CreateConversionSetSchema(includeId: true);

    public PutCartographicConversionSetByIdMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_update_by_id";

    public override string Description => "Replace and recalculate an existing coordinate-conversion case. The top-level id must equal cartographicConversionSet.MetaInfo.ID; supply the complete desired case because this is not a patch. After a successful 200 response, call get_by_id to retrieve recalculated results. Returns 400 for malformed or mismatched IDs and 404 when absent.";

    public override JsonNode? InputSchema => Schema;

    public override async Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out Guid id, out JsonNode? idError))
        {
            return idError;
        }
        if (!TryDeserialize(arguments, out CartographicConversionSetModel conversionSet, out JsonNode? conversionSetError))
        {
            return conversionSetError;
        }

        var response = McpActionResultConverter.FromActionResult(await CreateController().PutCartographicConversionSetById(id, conversionSet).ConfigureAwait(false));
        return response;
    }
}

internal sealed class DeleteCartographicConversionSetByIdMcpTool : CartographicConversionSetToolBase
{
    public DeleteCartographicConversionSetByIdMcpTool(ILoggerFactory loggerFactory, SqlConnectionManager connectionManager)
        : base(loggerFactory, connectionManager) { }

    public override string Name => "cartographic_conversion_set_delete_by_id";

    public override string Description => "Permanently delete a persisted coordinate-conversion case by UUID. In the normal temporary workflow, call this only after get_by_id has returned the calculated results. Deleting the case does not delete its CartographicProjection or GeodeticDatum. Returns 200 on success and 404 when absent.";

    public override JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "UUID of the coordinate-conversion case to delete after its results have been consumed.");

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out Guid id, out JsonNode? error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        var response = McpActionResultConverter.FromActionResult(CreateController().DeleteCartographicConversionSetById(id));
        return Task.FromResult<JsonNode?>(response);
    }
}
