using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSDC.UnitConversion.Service.Controllers;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class DeleteUnitSystemByIdMcpTool : IMcpTool
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeleteUnitSystemByIdMcpTool> _logger;

    public DeleteUnitSystemByIdMcpTool(IServiceProvider serviceProvider, ILogger<DeleteUnitSystemByIdMcpTool> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string Name => "delete_unit_system";

    public string Title => "Delete Unit System";

    public string Description => "Permanently delete one stored unit system by UUID. Check persistent UnitSystemConversionSet records that reference it before deletion; future calculations require both referenced systems to exist. Built-in/default systems should normally be retained.";

    public JsonNode? InputSchema => McpToolArgumentHelpers.CreateGuidSchema("id", "UUID of the stored unit system to delete.");

    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("status", "string"), ("message", "string"));

    public bool ReadOnly => false;

    public bool Destructive => true;

    public bool Idempotent => false;

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

            var actionResult = controller.DeleteUnitSystemById(id);

            if (actionResult is NotFoundResult)
            {
                return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status404NotFound, $"Unit system '{id}' was not found."));
            }

            var response = ActionResultToolHelper.CreateResponse(actionResult, "Unit system deleted.", "Failed to delete the unit system.");
            return Task.FromResult<JsonNode?>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName}.", Name);
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "An unexpected error occurred while deleting the unit system."));
        }
    }
}
