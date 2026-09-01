using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NORCE.Drilling.GeodeticDatum.Model;

namespace NORCE.Drilling.GeodeticDatum.Service.Mcp.Tools;

internal sealed class PostSpheroidMcpTool : SpheroidToolBase
{
    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonObject Schema = McpToolArgumentHelpers.CreateSpheroidSchema();

    public PostSpheroidMcpTool(IServiceScopeFactory scopeFactory)
        : base(scopeFactory)
    {
    }

    public override string Name => "spheroid_create";

    public override string Description => "Create and persist a reference spheroid. spheroid.MetaInfo.ID must be a caller-generated UUID. Provide at least two defined shape parameters, including a semi-major or semi-minor axis; axes are meters and eccentricity/flattening values are dimensionless. The service calculates missing equivalent parameters.";

    public override JsonNode? InputSchema => Schema;

    public override Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryDeserialize(arguments, out var spheroid, out var error))
        {
            return Task.FromResult<JsonNode?>(error);
        }

        using var scope = CreateControllerScope();
        var result = scope.Controller.PostSpheroid(spheroid);
        var response = McpActionResultConverter.FromActionResult(result);
        return Task.FromResult<JsonNode?>(response);
    }

    private static bool TryDeserialize(JsonObject? arguments, out Spheroid spheroid, out JsonNode? error)
    {
        spheroid = default!;
        error = null;

        if (arguments is null)
        {
            error = McpToolResponses.CreateValidationError("Arguments are required.");
            return false;
        }

        if (arguments["spheroid"] is not JsonNode spheroidNode)
        {
            error = McpToolResponses.CreateValidationError("Argument 'spheroid' is required.");
            return false;
        }

        try
        {
            spheroid = spheroidNode.Deserialize<Spheroid>(DeserializerOptions) ?? throw new InvalidOperationException();
            error = null;
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            error = McpToolResponses.CreateValidationError("Argument 'spheroid' could not be deserialized.");
            return false;
        }
    }
}
