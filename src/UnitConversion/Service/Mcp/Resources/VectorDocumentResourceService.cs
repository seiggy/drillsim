using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

/// <summary>
/// Bridges the SQLite-backed vector document catalog with the MCP resources/list and resources/read handlers.
/// </summary>
internal sealed class VectorDocumentResourceService
{
    private readonly IVectorDocumentRepository _repository;
    private readonly VectorDocumentDatabaseOptions _options;
    private readonly ILogger<VectorDocumentResourceService> _logger;

    public VectorDocumentResourceService(
        IVectorDocumentRepository repository,
        IOptions<VectorDocumentDatabaseOptions> options,
        ILogger<VectorDocumentResourceService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _repository = repository;
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask<ListResourcesResult> ListResourcesAsync(
        RequestContext<ListResourcesRequestParams> request,
        CancellationToken cancellationToken)
    {
        var cursor = request.Params?.Cursor;
        var page = await _repository.ListAsync(cursor, Math.Max(1, _options.PageSize), cancellationToken).ConfigureAwait(false);

        var result = new ListResourcesResult
        {
            NextCursor = page.NextCursor
        };

        foreach (var document in page.Documents.Select(CreateResource))
        {
            result.Resources.Add(document);
        }

        return result;
    }

    public async ValueTask<ReadResourceResult> ReadResourceAsync(
        RequestContext<ReadResourceRequestParams> request,
        CancellationToken cancellationToken)
    {
        var uri = request.Params?.Uri;
        if (string.IsNullOrWhiteSpace(uri))
        {
            throw new InvalidOperationException("A resource URI must be provided.");
        }

        var document = await _repository.GetByUriAsync(uri, cancellationToken).ConfigureAwait(false);
        if (document is null)
        {
            _logger.LogWarning("Resource {Uri} was not found in the vector document database.", uri);
            throw new InvalidOperationException($"Resource '{uri}' was not found.");
        }

        var result = new ReadResourceResult();
        result.Contents.Add(CreateContent(document));
        return result;
    }

    private Resource CreateResource(VectorDocumentRecord document)
    {
        return new Resource
        {
            Uri = document.Uri,
            Name = document.Name,
            Title = document.Title,
            Description = document.Description,
            MimeType = document.MimeType ?? _options.DefaultMimeType,
            Meta = Clone(document.Metadata),
            Size = document.Size
        };
    }

    private ResourceContents CreateContent(VectorDocumentRecord document)
    {
        return new TextResourceContents
        {
            Text = document.Content,
            Uri = document.Uri,
            MimeType = document.MimeType ?? _options.DefaultMimeType,
            Meta = Clone(document.Metadata)
        };
    }

    private static JsonObject? Clone(JsonObject? value)
    {
        if (value is null)
        {
            return null;
        }

        return JsonNode.Parse(value.ToJsonString())?.AsObject();
    }
}
