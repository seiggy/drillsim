using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSDC.UnitConversion.Service.Controllers;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class GetUnitSystemByIdMcpTool : IMcpTool
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetUnitSystemByIdMcpTool> _logger;

    public GetUnitSystemByIdMcpTool(IServiceProvider serviceProvider, ILogger<GetUnitSystemByIdMcpTool> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string Name => "get_unit_system";

    public string Title => "Get Unit System";

    public string Description => "Retrieve one unit system by UUID, including its complete physical-quantity-to-unit-choice mapping. Mapping entries identify both UUIDs and, when available, their canonical quantity and unit names.";

    public JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "UUID of the unit system to retrieve, including its Choices mapping.");

    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("id", "string"), ("name", "string"), ("choices", "array"));

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out var id, out var parseError))
        {
            return Task.FromResult(parseError);
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var controller = ActivatorUtilities.CreateInstance<UnitSystemController>(scope.ServiceProvider);

            ActionResult<DrillingUnitSystem> actionResult = controller.GetUnitSystemById(id);

            if (actionResult.Value is not null)
            {
                return Task.FromResult<JsonNode?>(Map(actionResult.Value));
            }

            if (actionResult.Result is OkObjectResult okObjectResult && okObjectResult.Value is not null)
            {
                if (okObjectResult.Value is DrillingUnitSystem unitSystem)
                {
                    return Task.FromResult<JsonNode?>(Map(unitSystem));
                }
            }

            if (actionResult.Result is NotFoundResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status404NotFound, $"Unit system '{id}' was not found."));
            }

            if (actionResult.Result is BadRequestResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("The unit system id must be a non-empty UUID."));
            }

            if (actionResult.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? StatusCodes.Status500InternalServerError;
                var message = objectResult.Value as string ?? "Unable to retrieve the unit system.";
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(statusCode, message));
            }

            if (actionResult.Result is StatusCodeResult statusCodeResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(statusCodeResult.StatusCode, "Unable to retrieve the unit system."));
            }

            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "Unable to retrieve the unit system."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName}.", Name);
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving the unit system."));
        }
    }

    private static JsonObject Map(DrillingUnitSystem system)
    {
        var choices = new JsonArray();
        foreach ((string quantityIdText, string unitIdText) in system.Choices.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Guid.TryParse(quantityIdText, out Guid quantityId);
            Guid.TryParse(unitIdText, out Guid unitId);
            var quantity = DrillingPhysicalQuantity.GetQuantity(quantityId);
            var unit = quantity is null ? null : OSDC.UnitConversion.Conversion.PhysicalQuantityHierarchy.Enumerate(quantity)
                .SelectMany(parent => parent.UnitChoices ?? []).FirstOrDefault(choice => choice.ID == unitId);
            choices.Add(new JsonObject
            {
                ["physicalQuantityId"] = quantityIdText, ["physicalQuantity"] = quantity?.Name,
                ["unitChoiceId"] = unitIdText, ["unitName"] = unit?.UnitName, ["unitLabel"] = unit?.UnitLabel
            });
        }
        return new JsonObject
        {
            ["id"] = system.ID.ToString(), ["name"] = system.Name, ["description"] = system.Description,
            ["isDefault"] = system.IsDefault, ["isSI"] = system.IsSI, ["choices"] = choices
        };
    }
}

