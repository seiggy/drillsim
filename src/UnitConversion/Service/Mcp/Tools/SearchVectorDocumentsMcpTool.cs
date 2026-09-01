using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OSDC.UnitConversion.Service.Mcp.Resources;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

/// <summary>
/// Exposes a semantic search over the vectorized resource catalog so LLMs can gather relevant document ids.
/// </summary>
public sealed class SearchVectorDocumentsMcpTool : IMcpTool
{
    private static readonly JsonObject Schema = new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["query"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "Free-form user query describing the needed physical quantities or unit choices."
            },
            ["maxResults"] = new JsonObject
            {
                ["type"] = "integer",
                ["minimum"] = 1,
                ["maximum"] = 200,
                ["description"] = "Optional cap on the number of results to return (defaults to server preference)."
            }
        },
        ["required"] = new JsonArray { "query" },
        ["additionalProperties"] = false
    };

    private readonly IVectorDocumentRepository _repository;
    private readonly ITextEmbeddingGenerator _embeddingGenerator;
    private readonly VectorDocumentSearchOptions _options;
    private readonly ILogger<SearchVectorDocumentsMcpTool> _logger;

    public SearchVectorDocumentsMcpTool(
        IVectorDocumentRepository repository,
        ITextEmbeddingGenerator embeddingGenerator,
        IOptions<VectorDocumentSearchOptions> options,
        ILogger<SearchVectorDocumentsMcpTool> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "search_documentation";

    public string Title => "Search Unit Conversion Documentation";

    public string Description => "Semantically search the UnitConversion documentation catalog for physical quantities, units, formulas, or unit systems. Returns ranked MCP resource links that clients can read directly. This tool uses the configured embedding provider and may make an external network request.";

    public JsonNode? InputSchema => Schema;

    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("resourceLinks", "array"));

    public bool OpenWorld => true;

    public async Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (!TryReadQuery(arguments, out var query, out var error))
        {
            return error;
        }

        var maxResults = ReadMaxResults(arguments);
        if (string.IsNullOrWhiteSpace(_options.EmbeddingModel))
        {
            return McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "Vector search is not configured because VectorDocumentSearch:EmbeddingModel is empty.");
        }

        var databasePath = _repository.DatabasePath;
        if (string.IsNullOrWhiteSpace(databasePath) || !File.Exists(databasePath))
        {
            return McpToolResponses.CreateError(StatusCodes.Status503ServiceUnavailable, $"Vector document database was not found at '{databasePath}'.");
        }

        try
        {
            var embedding = await _embeddingGenerator.GenerateAsync(query, cancellationToken).ConfigureAwait(false);
            if (embedding.Length != _options.EmbeddingDimensions)
            {
                _logger.LogWarning("Generated embedding length {Length} does not match the configured dimensions {Dimensions}.", embedding.Length, _options.EmbeddingDimensions);
                return McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, $"Generated embedding length {embedding.Length} does not match configured dimension {_options.EmbeddingDimensions}.");
            }

            var hits = await _repository.SearchAsync(embedding, maxResults, cancellationToken).ConfigureAwait(false);
            var results = new JsonArray();
            foreach (var hit in hits)
            {
                var node = new JsonObject
                {
                    ["uri"] = hit.Uri,
                    ["id"] = hit.Id,
                    ["score"] = Math.Round(hit.Score, 6),
                    ["mimeType"] = "text/markdown"
                };

                if (!string.IsNullOrWhiteSpace(hit.Name))
                {
                    node["name"] = hit.Name;
                }

                if (!string.IsNullOrWhiteSpace(hit.Title))
                {
                    node["title"] = hit.Title;
                }

                results.Add(node);
            }

            return new JsonObject
            {
                ["resourceLinks"] = results
            };
        }
        catch (EmbeddingProviderException ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for {ToolName}.", Name);
            return McpToolResponses.CreateError(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute {ToolName}.", Name);
            return McpToolResponses.CreateError(StatusCodes.Status500InternalServerError, "Vector search failed while reading the vector document database.");
        }
    }

    private bool TryReadQuery(JsonObject? arguments, out string query, out JsonNode? error)
    {
        query = string.Empty;
        error = null;

        if (arguments?["query"] is not JsonValue value || value.GetValue<string?>() is not { } raw || string.IsNullOrWhiteSpace(raw))
        {
            error = McpToolResponses.CreateValidationError("Argument 'query' is required and must be a non-empty string.");
            return false;
        }

        query = raw.Trim();
        return true;
    }

    private int ReadMaxResults(JsonObject? arguments)
    {
        if (arguments?["maxResults"] is JsonValue value && value.TryGetValue<int>(out var requested))
        {
            return Math.Clamp(requested, 1, Math.Max(_options.MaxLimit, 1));
        }

        return Math.Clamp(_options.DefaultLimit, 1, Math.Max(_options.MaxLimit, 1));
    }
}
