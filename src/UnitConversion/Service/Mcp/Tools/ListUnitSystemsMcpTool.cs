using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Service.Controllers;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class ListUnitSystemsMcpTool : IMcpTool
{
    private readonly IServiceProvider _services;
    private static readonly JsonObject Schema = new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 100, ["default"] = 25 },
            ["cursor"] = new JsonObject { ["type"] = "string", ["description"] = "Opaque cursor returned by the previous call." }
        },
        ["additionalProperties"] = false
    };

    public ListUnitSystemsMcpTool(IServiceProvider services) => _services = services;
    public string Name => "list_unit_systems";
    public string Title => "List Unit Systems";
    public string Description => "List built-in and custom unit systems as paginated summaries. Use get_unit_system with a returned UUID to inspect its complete physical-quantity-to-unit-choice mapping.";
    public JsonNode? InputSchema => Schema;
    public JsonNode? OutputSchema => McpContractSchemas.Paged("unitSystems");

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        int limit = arguments?["limit"] is JsonValue limitNode && limitNode.TryGetValue<int>(out int requested) ? Math.Clamp(requested, 1, 100) : 25;
        int offset = 0;
        if (arguments?["cursor"] is JsonValue cursorNode && cursorNode.TryGetValue<string>(out string? cursor) &&
            (!int.TryParse(cursor, out offset) || offset < 0))
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'cursor' is invalid."));
        }

        using IServiceScope scope = _services.CreateScope();
        var controller = ActivatorUtilities.CreateInstance<UnitSystemController>(scope.ServiceProvider);
        ActionResult<IEnumerable<UnitSystemLight>> result = controller.GetAllUnitSystemLight();
        IEnumerable<UnitSystemLight>? values = result.Value ?? (result.Result as ObjectResult)?.Value as IEnumerable<UnitSystemLight>;
        if (values is null) return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(500, "Unit systems could not be loaded."));

        List<UnitSystemLight> ordered = values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.ID).ToList();
        var items = new JsonArray(ordered.Skip(offset).Take(limit).Select(item => (JsonNode)new JsonObject
        {
            ["id"] = item.ID.ToString(), ["name"] = item.Name, ["description"] = item.Description,
            ["isDefault"] = item.IsDefault, ["isSI"] = item.IsSI
        }).ToArray());
        int nextOffset = offset + items.Count;
        return Task.FromResult<JsonNode?>(new JsonObject
        {
            ["unitSystems"] = items,
            ["nextCursor"] = nextOffset < ordered.Count ? nextOffset.ToString() : null
        });
    }
}
