using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NORCE.Drilling.GeodeticDatum.Model;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class PutSpheroidByIdMcpTool : SpheroidToolBase
{
    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonObject Schema = McpToolArgumentHelpers.CreateSpheroidSchema(includeId: true);

    public PutSpheroidByIdMcpTool(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public override string Name => "spheroid_update_by_id";

    public override string Description => "Replace one stored spheroid with a complete definition and recalculate derived shape parameters. The top-level id must equal spheroid.MetaInfo.ID; this is not a patch. Review geodetic datums that embed or depend on this spheroid before changing it. Returns 404 when absent.";

    public override JsonNode? InputSchema => Schema;

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out var id, out var idError))
        {
            return Task.FromResult<JsonNode?>(idError);
        }

        if (!TryDeserialize(arguments, out var spheroid, out var spheroidError))
        {
            return Task.FromResult<JsonNode?>(spheroidError);
        }

        using var scope = CreateControllerScope();
        var result = scope.Controller.PutSpheroidById(id, spheroid);
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }

    private static bool TryDeserialize(JsonObject? arguments, out Spheroid spheroid, out JsonNode? error)
    {
        spheroid = default!;
        error = null;

        if (arguments?["spheroid"] is not JsonNode node)
        {
            error = McpToolResponses.CreateValidationError("Argument 'spheroid' is required.");
            return false;
        }

        try
        {
            spheroid = node.Deserialize<Spheroid>(DeserializerOptions) ?? throw new InvalidOperationException();
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError("Argument 'spheroid' could not be deserialized.");
            return false;
        }
    }
}
