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

public sealed class PostUnitSystemMcpTool : IMcpTool
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PostUnitSystemMcpTool> _logger;

    private static readonly JsonObject Schema = McpToolArgumentHelpers.CreateUnitSystemSchema();

    public PostUnitSystemMcpTool(IServiceProvider serviceProvider, ILogger<PostUnitSystemMcpTool> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string Name => "create_unit_system";

    public string Title => "Create Unit System";

    public string Description => "Create and persist a custom unit system with a caller-assigned UUID and complete physical-quantity-to-unit-choice mapping. Every selected unit is validated against its quantity. The service derives isSI from the selected choices and returns an error for invalid mappings or an existing UUID.";

    public JsonNode? InputSchema => Schema;

    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("status", "string"), ("message", "string"), ("isSI", "boolean"));

    public bool ReadOnly => false;

    public bool Idempotent => false;

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
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

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var controller = ActivatorUtilities.CreateInstance<UnitSystemController>(scope.ServiceProvider);

            var actionResult = controller.PostUnitSystem(unitSystem);

            if (actionResult is StatusCodeResult status && status.StatusCode == StatusCodes.Status409Conflict)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status409Conflict, $"Unit system '{unitSystem.ID}' already exists."));
            }

            if (actionResult is BadRequestResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("The provided unit system payload is invalid."));
            }

            var response = ActionResultToolHelper.CreateResponse(actionResult, "Unit system created.", "Failed to create the unit system.");
            if (response?["status"]?.GetValue<string>() == "ok")
            {
                response["isSI"] = unitSystem.IsSI;
            }
            return Task.FromResult<JsonNode?>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName}.", Name);
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating the unit system."));
        }
    }
}
