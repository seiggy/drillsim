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
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class GetPhysicalQuantityByIdMcpTool : IMcpTool
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetPhysicalQuantityByIdMcpTool> _logger;

    public GetPhysicalQuantityByIdMcpTool(IServiceProvider serviceProvider, ILogger<GetPhysicalQuantityByIdMcpTool> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string Name => "get_physical_quantity";

    public string Title => "Get Physical Quantity";

    public string Description => "Retrieve one physical quantity by UUID, including canonical name, synonyms, dimensions, SI unit, meaningful SI precision, directly declared unit choices, symbolic conversion definitions, and compatible parent quantities.";

    public JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "UUID of the physical quantity to retrieve.");

    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("id", "string"), ("name", "string"), ("siUnit", "object"), ("dimensions", "object"), ("unitChoices", "array"), ("hierarchy", "object"));

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (!McpToolArgumentHelpers.TryParseGuid(arguments, "id", out var id, out var parseError))
        {
            return Task.FromResult(parseError);
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var controller = ActivatorUtilities.CreateInstance<PhysicalQuantityController>(scope.ServiceProvider);

            ActionResult<DrillingPhysicalQuantity> actionResult = controller.GetPhysicalQuantityById(id);

            if (actionResult.Value is not null)
            {
                return Task.FromResult<JsonNode?>(PhysicalQuantityContractMapper.ToDetails(actionResult.Value));
            }

            if (actionResult.Result is OkObjectResult okObjectResult && okObjectResult.Value is not null)
            {
                if (okObjectResult.Value is OSDC.UnitConversion.Conversion.BasePhysicalQuantity quantity)
                {
                    return Task.FromResult<JsonNode?>(PhysicalQuantityContractMapper.ToDetails(quantity));
                }
            }

            if (actionResult.Result is NotFoundResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status404NotFound, $"Physical quantity '{id}' was not found."));
            }

            if (actionResult.Result is BadRequestResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("The physical quantity id must be a non-empty UUID."));
            }

            if (actionResult.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? StatusCodes.Status500InternalServerError;
                var message = objectResult.Value as string ?? "Unable to retrieve the physical quantity.";
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(statusCode, message));
            }

            if (actionResult.Result is StatusCodeResult statusCodeResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(statusCodeResult.StatusCode, "Unable to retrieve the physical quantity."));
            }

            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "Unable to retrieve the physical quantity."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName}.", Name);
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving the physical quantity."));
        }
    }

}
