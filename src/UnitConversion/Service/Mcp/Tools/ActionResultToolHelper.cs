using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

internal static class ActionResultToolHelper
{
    public static bool TryResolveResult<T>(
        ActionResult<T> actionResult,
        string failureMessage,
        out T? value,
        out JsonNode? error)
    {
        if (actionResult.Value is { } resolvedValue)
        {
            value = resolvedValue;
            error = null;
            return true;
        }

        if (actionResult.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is T typedValue)
            {
                value = typedValue;
                error = null;
                return true;
            }

            var statusCode = objectResult.StatusCode ?? StatusCodes.Status500InternalServerError;
            error = McpToolResponses.CreateError(statusCode, failureMessage);
            value = default;
            return false;
        }

        if (actionResult.Result is StatusCodeResult statusCodeResult)
        {
            error = McpToolResponses.CreateError(statusCodeResult.StatusCode, failureMessage);
            value = default;
            return false;
        }

        error = McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, failureMessage);
        value = default;
        return false;
    }

    public static JsonNode? CreateResponse(
        IActionResult? result,
        string successMessage,
        string failureMessage)
    {
        if (result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            if (statusCode is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices)
            {
                if (objectResult.Value is JsonNode node)
                {
                    return node;
                }

                if (objectResult.Value is not null)
                {
                    return JsonSerializer.SerializeToNode(objectResult.Value, objectResult.Value.GetType(), McpToolJsonOptions.Default);
                }

                return McpToolResponses.CreateSuccess(successMessage);
            }

            var message = objectResult.Value as string ?? failureMessage;
            return McpToolResponses.CreateError(statusCode, message);
        }

        if (result is StatusCodeResult statusCodeResult)
        {
            if (statusCodeResult.StatusCode is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices)
            {
                return McpToolResponses.CreateSuccess(successMessage);
            }

            return McpToolResponses.CreateError(statusCodeResult.StatusCode, failureMessage);
        }

        if (result is EmptyResult)
        {
            return McpToolResponses.CreateSuccess(successMessage);
        }

        if (result is null)
        {
            return McpToolResponses.CreateSuccess(successMessage);
        }

        return McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, failureMessage);
    }
}
