using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GeodeticConversionSetModel = NORCE.Drilling.GeodeticDatum.Model.GeodeticConversionSet;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class PutGeodeticConversionSetByIdMcpTool : GeodeticConversionSetToolBase
{
    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonObject Schema = McpToolArgumentHelpers.CreateConversionSetSchema(includeId: true);

    public PutGeodeticConversionSetByIdMcpTool(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public override string Name => "geodetic_conversion_set_update_by_id";

    public override string Description => "Replace and recalculate a persisted geodetic conversion case. The top-level id must equal geodeticConversionSet.MetaInfo.ID and the supplied object is the complete replacement, not a patch. Retrieve the recalculated results with get_by_id afterward; returns 404 when the case does not exist.";

    public override JsonNode? InputSchema => Schema;

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out var id, out var idError))
        {
            return Task.FromResult<JsonNode?>(idError);
        }

        if (!TryDeserialize(arguments, out GeodeticConversionSetModel entity, out var entityError))
        {
            return Task.FromResult<JsonNode?>(entityError);
        }

        using var scope = CreateControllerScope();
        var result = scope.Controller.PutGeodeticConversionSetById(id, entity);
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }

    private static bool TryDeserialize(JsonObject? arguments, out GeodeticConversionSetModel entity, out JsonNode? error)
    {
        entity = default!;
        error = null;

        if (arguments?["geodeticConversionSet"] is not JsonNode node)
        {
            error = McpToolResponses.CreateValidationError("Argument 'geodeticConversionSet' is required.");
            return false;
        }

        try
        {
            entity = node.Deserialize<GeodeticConversionSetModel>(DeserializerOptions) ?? throw new InvalidOperationException();
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError("Argument 'geodeticConversionSet' could not be deserialized.");
            return false;
        }
    }
}
