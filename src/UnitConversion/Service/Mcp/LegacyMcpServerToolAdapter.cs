using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace OSDC.UnitConversion.Service.Mcp;

/// <summary>
/// Adapts the service's <see cref="IMcpTool"/> abstraction to the Model Context Protocol tool contract.
/// </summary>
internal sealed class McpServerToolAdapter : McpServerTool
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IMcpTool _tool;
    private readonly ILogger _logger;
    private readonly Tool _protocolTool;
    private readonly IReadOnlyList<object> _metadata = Array.Empty<object>();

    public McpServerToolAdapter(IMcpTool tool, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _tool = tool;
        _logger = loggerFactory.CreateLogger(tool.GetType());

        _protocolTool = new Tool
        {
            Name = tool.Name,
            Title = tool.Title,
            Description = tool.Description,
            Annotations = new ToolAnnotations
            {
                Title = tool.Title,
                ReadOnlyHint = tool.ReadOnly,
                DestructiveHint = tool.Destructive,
                IdempotentHint = tool.Idempotent,
                OpenWorldHint = tool.OpenWorld
            }
        };

        if (tool.InputSchema is JsonNode schemaNode)
        {
            _protocolTool.InputSchema = JsonSerializer.SerializeToElement(schemaNode, SerializerOptions);
        }

        if (tool.OutputSchema is JsonNode outputSchemaNode)
        {
            _protocolTool.OutputSchema = JsonSerializer.SerializeToElement(outputSchemaNode, SerializerOptions);
        }
    }

    public override Tool ProtocolTool => _protocolTool;

    public override IReadOnlyList<object> Metadata => _metadata;

    public override async ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var arguments = ConvertArguments(request.Params?.Arguments);

        try
        {
            var result = await _tool.InvokeAsync(arguments, cancellationToken).ConfigureAwait(false);
            return CreateCallToolResult(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP tool {ToolName} failed while handling request.", _tool.Name);

            return new CallToolResult
            {
                IsError = true,
                Content =
                {
                    new TextContentBlock
                    {
                        Text = $"Tool '{_tool.Name}' failed: {ex.Message}"
                    }
                }
            };
        }
    }

    private JsonObject? ConvertArguments(IDictionary<string, JsonElement>? arguments)
    {
        if (arguments is null || arguments.Count == 0)
        {
            return null;
        }

        var result = new JsonObject();

        foreach (var (key, element) in arguments)
        {
            try
            {
                result[key] = JsonNode.Parse(element.GetRawText());
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Failed to parse argument '{ArgumentKey}' for tool {ToolName}. Passing raw JSON text.", key, _tool.Name);
                result[key] = JsonValue.Create(element.GetRawText());
            }
        }

        return result;
    }

    internal CallToolResult CreateCallToolResult(JsonNode? result)
    {
        var serialized = result?.ToJsonString(SerializerOptions) ?? "null";
        var isError = IsErrorResult(result);
        var callResult = new CallToolResult
        {
            IsError = isError,
            StructuredContent = isError || result is null ? null : JsonSerializer.SerializeToElement(result, SerializerOptions)
        };
        callResult.Content.Add(new TextContentBlock { Text = isError ? GetErrorMessage(result) : serialized });
        if (!isError && result is JsonObject payload && payload["resourceLinks"] is JsonArray links)
        {
            foreach (JsonObject link in links.OfType<JsonObject>())
            {
                string? uri = link["uri"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(uri)) continue;
                callResult.Content.Add(new ResourceLinkBlock
                {
                    Uri = uri,
                    Name = link["name"]?.GetValue<string>() ?? uri,
                    Title = link["title"]?.GetValue<string>(),
                    Description = link["description"]?.GetValue<string>(),
                    MimeType = link["mimeType"]?.GetValue<string>()
                });
            }
        }
        return callResult;
    }

    private static bool IsErrorResult(JsonNode? result) =>
        result is JsonObject payload &&
        payload["status"] is JsonValue status &&
        status.TryGetValue<int>(out var statusCode) &&
        statusCode >= 400;

    private static string GetErrorMessage(JsonNode? result)
    {
        if (result is JsonObject payload &&
            payload["error"] is JsonValue error &&
            error.TryGetValue<string>(out var message) &&
            !string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return result?.ToJsonString(SerializerOptions) ?? "The tool failed without returning error details.";
    }
}
