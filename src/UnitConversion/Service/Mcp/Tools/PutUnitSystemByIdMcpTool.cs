using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSDC.UnitConversion.Service.Controllers;
using OSDC.UnitConversion.Conversion.UnitSystem.DrillingEngineering;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class PutUnitSystemByIdMcpTool : IMcpTool
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PutUnitSystemByIdMcpTool> _logger;

    private static readonly JsonObject Schema = McpToolArgumentHelpers.CreateUnitSystemSchema(includeId: true);

    public PutUnitSystemByIdMcpTool(IServiceProvider serviceProvider, ILogger<PutUnitSystemByIdMcpTool> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string Name => "replace_unit_system";

    public string Title => "Replace Unit System";

    public string Description => "Replace an existing custom unit system and its complete Choices mapping. The top-level id must equal unitSystem.ID. Each map entry is validated as a physical-quantity UUID paired with one of that quantity's unit-choice UUIDs. The service derives isSI from the selected choices; this is a full update, not a partial patch.";

    public JsonNode? InputSchema => Schema;

    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("status", "string"), ("message", "string"), ("isSI", "boolean"));

    public bool ReadOnly => false;

    public bool Destructive => true;

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out var id, out var parseError))
        {
            return Task.FromResult(parseError);
        }

        if (arguments?["unitSystem"] is not JsonNode unitSystemNode)
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'unitSystem' is required."));
        }

        DrillingUnitSystem? unitSystem;
        try
        {
            unitSystem = unitSystemNode.Deserialize<DrillingUnitSystem>(McpToolJsonOptions.Default);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Failed to deserialize unit system payload.");
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'unitSystem' must be a valid unit system payload."));
        }

        if (unitSystem is null)
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'unitSystem' must be a valid unit system payload."));
        }

        if (unitSystem.ID != id)
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("The unit system payload ID must match the 'id' argument."));
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var controller = ActivatorUtilities.CreateInstance<UnitSystemController>(scope.ServiceProvider);

            var actionResult = controller.PutUnitSystemById(id, unitSystem);

            if (actionResult is NotFoundResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status404NotFound, $"Unit system '{id}' was not found."));
            }

            if (actionResult is BadRequestResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("The unit system payload is invalid or its ID does not match the target id."));
            }

            var response = ActionResultToolHelper.CreateResponse(actionResult, "Unit system updated.", "Failed to update the unit system.");
            if (response?["status"]?.GetValue<string>() == "ok")
            {
                response["isSI"] = unitSystem.IsSI;
            }
            return Task.FromResult<JsonNode?>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName}.", Name);
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the unit system."));
        }
    }
}
