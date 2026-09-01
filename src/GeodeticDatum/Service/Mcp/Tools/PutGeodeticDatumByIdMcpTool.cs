using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GeodeticDatumModel = NORCE.Drilling.GeodeticDatum.Model.GeodeticDatum;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class PutGeodeticDatumByIdMcpTool : GeodeticDatumToolBase
{
    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonObject Schema = McpToolArgumentHelpers.CreateGeodeticDatumSchema(includeId: true);

    public PutGeodeticDatumByIdMcpTool(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public override string Name => "geodetic_datum_update_by_id";

    public override string Description => "Replace one stored geodetic datum with a complete definition. The top-level id must equal geodeticDatum.MetaInfo.ID; this is not a patch. Changes affect subsequent coordinate conversions that use this datum, so review dependent projections and conversion cases first. Returns 404 when absent.";

    public override JsonNode? InputSchema => Schema;

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out var id, out var idError))
        {
            return Task.FromResult<JsonNode?>(idError);
        }

        if (!TryDeserialize(arguments, out GeodeticDatumModel datum, out var datumError))
        {
            return Task.FromResult<JsonNode?>(datumError);
        }

        using var scope = CreateControllerScope();
        var result = scope.Controller.PutGeodeticDatumById(id, datum);
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }

    private static bool TryDeserialize(JsonObject? arguments, out GeodeticDatumModel datum, out JsonNode? error)
    {
        datum = default!;
        error = null;

        if (arguments?["geodeticDatum"] is not JsonNode datumNode)
        {
            error = McpToolResponses.CreateValidationError("Argument 'geodeticDatum' is required.");
            return false;
        }

        try
        {
            datum = datumNode.Deserialize<GeodeticDatumModel>(DeserializerOptions) ?? throw new InvalidOperationException();
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError("Argument 'geodeticDatum' could not be deserialized.");
            return false;
        }
    }
}
