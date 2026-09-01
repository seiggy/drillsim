using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace GenerateResources;

internal static class Program
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string DefaultApiUrl = "https://app.digiwells.no/UnitConversion/api/PhysicalQuantity/HeavyData";
    private const string DataBaseName = "UnitConversionVectors.db";
    private const string DefaultUriPrefix = "resource://unit-conversion/documents/";
    private const string DefaultEmbeddingEndpoint = "http://localhost:8080/embeddings";
    private const string DefaultEmbeddingModel = "nomic-embed-text-v1";
    private const int DefaultEmbeddingDimensions = 768;

    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        // search for home directory
        string DataBasePath = ".";
        while (Directory.GetParent(Path.Combine(DataBasePath, "..")) is not null && !Directory.Exists(Path.Combine(DataBasePath, "home")))
        {
            DataBasePath = Path.Combine(DataBasePath, "..");
        }
        if (!Directory.Exists(DataBasePath))
        {
            Console.WriteLine("Could not find home directory");
            return;
        }
        string databaseFileName = Path.Combine(DataBasePath, "home", DataBaseName);
        try
        {
            if (File.Exists(databaseFileName))
            {
                Console.WriteLine("Delete previous database before creating new one.");
                File.Delete(databaseFileName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Impossible to delete previous database: " + ex.ToString());
            return;
        }

        var settings = GeneratorSettings.FromEnvironment(DefaultApiUrl, databaseFileName, DefaultUriPrefix, DefaultEmbeddingEndpoint, DefaultEmbeddingModel, DefaultEmbeddingDimensions);

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(100)
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GenerateResources/1.0");

        var quantityClient = new PhysicalQuantityClient(httpClient, settings.ApiEndpoint);
        Console.WriteLine($"Fetching physical quantities from {settings.ApiEndpoint} …");
        var quantities = await quantityClient.GetPhysicalQuantitiesAsync(cts.Token);
        Console.WriteLine($"Fetched {quantities.Count} physical quantities.");

        var documentBuilder = new DocumentBuilder(settings.ResourceUriPrefix);
        var documents = documentBuilder.BuildDocuments(quantities);
        Console.WriteLine($"Constructed {documents.Count} documents (quantities + unit choices).");

        using var embeddingClient = new NomicEmbeddingClient(settings.EmbeddingEndpoint, settings.EmbeddingModel, settings.EmbeddingApiKey);
        var store = new SqliteDocumentStore(settings.DatabasePath, settings.EmbeddingDimensions);

        await store.ReplaceAsync(documents, embeddingClient, cts.Token);
        Console.WriteLine($"Vector database created at {Path.GetFullPath(settings.DatabasePath)}");
    }

    private sealed record GeneratorSettings(
        string ApiEndpoint,
        string DatabasePath,
        string ResourceUriPrefix,
        string EmbeddingEndpoint,
        string? EmbeddingApiKey,
        string EmbeddingModel,
        int EmbeddingDimensions)
    {
        public static GeneratorSettings FromEnvironment(
            string defaultApi,
            string defaultDb,
            string defaultPrefix,
            string defaultEndpoint,
            string defaultModel,
            int defaultDims)
        {
            var api = Environment.GetEnvironmentVariable("UNITCONVERSION_API_URL");
            if (string.IsNullOrWhiteSpace(api))
            {
                api = defaultApi;
            }

            var db = Environment.GetEnvironmentVariable("UNITCONVERSION_VECTOR_DB_PATH");
            if (string.IsNullOrWhiteSpace(db))
            {
                db = defaultDb;
            }

            var prefix = Environment.GetEnvironmentVariable("UNITCONVERSION_RESOURCE_URI_PREFIX");
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = defaultPrefix;
            }

            var endpoint = Environment.GetEnvironmentVariable("NOMIC_EMBEDDING_ENDPOINT");
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = defaultEndpoint;
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("NOMIC_EMBEDDING_ENDPOINT must be configured or a default endpoint provided.");
            }

            var apiKey = Environment.GetEnvironmentVariable("NOMIC_API_KEY");

            var model = Environment.GetEnvironmentVariable("NOMIC_EMBEDDING_MODEL");
            if (string.IsNullOrWhiteSpace(model))
            {
                model = defaultModel;
            }

            var dims = defaultDims;
            var dimsEnv = Environment.GetEnvironmentVariable("NOMIC_EMBEDDING_DIMENSIONS");
            if (!string.IsNullOrWhiteSpace(dimsEnv) && int.TryParse(dimsEnv, out var parsed) && parsed > 0)
            {
                dims = parsed;
            }

            return new GeneratorSettings(api, db, prefix, endpoint, apiKey, model, dims);
        }
    }

    private sealed class PhysicalQuantityClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public PhysicalQuantityClient(HttpClient httpClient, string endpoint)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public async Task<IReadOnlyList<PhysicalQuantityDto>> GetPhysicalQuantitiesAsync(CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(_endpoint, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"Physical quantity request failed with {(int)response.StatusCode}: {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var data = await JsonSerializer.DeserializeAsync<List<PhysicalQuantityDto>>(stream, SerializerOptions, cancellationToken).ConfigureAwait(false);
            return data ?? new List<PhysicalQuantityDto>();
        }
    }

    private sealed class DocumentBuilder
    {
        private readonly string _uriPrefix;

        public DocumentBuilder(string uriPrefix)
        {
            _uriPrefix = string.IsNullOrWhiteSpace(uriPrefix)
                ? DefaultUriPrefix
                : uriPrefix.TrimEnd('/') + "/";
        }

        public IReadOnlyList<DocumentRecord> BuildDocuments(IReadOnlyList<PhysicalQuantityDto> quantities)
        {
            var docs = new List<DocumentRecord>();
            double sort = 0;

            foreach (var quantity in quantities.Where(q => q is not null))
            {
                var quantityId = BuildDocumentId("physical-quantity", quantity.Id ?? quantity.Name ?? Guid.NewGuid().ToString("N"));
                var quantityContent = BuildQuantityContent(quantity);
                var quantityMetadata = CreateQuantityMetadata(quantity);
                docs.Add(new DocumentRecord(
                    quantityId,
                    BuildUri(quantityId),
                    quantity.Name ?? quantity.Id ?? quantityId,
                    quantity.Name,
                    $"Reference entry for {quantity.Name ?? quantity.Id ?? "physical quantity"}.",
                    "text/markdown",
                    quantityContent,
                    quantityMetadata,
                    Encoding.UTF8.GetByteCount(quantityContent),
                    sort++));

                if (quantity.UnitChoices is null)
                {
                    continue;
                }

                foreach (var unit in quantity.UnitChoices.Where(u => u is not null))
                {
                    var unitSeed = $"{quantity.Id ?? quantity.Name}:{unit.UnitName ?? unit.UnitLabel ?? Guid.NewGuid().ToString("N")}";
                    var unitId = BuildDocumentId("unit-choice", unitSeed);
                    var unitContent = BuildUnitContent(quantity, unit);
                    var unitMetadata = CreateUnitMetadata(quantity, unit);
                    docs.Add(new DocumentRecord(
                        unitId,
                        BuildUri(unitId),
                        $"{quantity.Name ?? quantity.Id ?? "Quantity"} – {unit.UnitLabel ?? unit.UnitName ?? "Unit"}",
                        $"{unit.UnitLabel ?? unit.UnitName ?? "Unit"} definition",
                        $"Conversion explanation for {unit.UnitLabel ?? unit.UnitName ?? "unit"} within {quantity.Name ?? quantity.Id}.",
                        "text/markdown",
                        unitContent,
                        unitMetadata,
                        Encoding.UTF8.GetByteCount(unitContent),
                        sort++));
                }
            }

            return docs;
        }

        private string BuildUri(string documentId) => _uriPrefix + documentId;

        private static string BuildQuantityContent(PhysicalQuantityDto quantity)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"# {quantity.Name ?? quantity.Id ?? "Physical Quantity"}");
            builder.AppendLine();
            if (quantity.UsualNames is { Count: > 0 })
            {
                builder.AppendLine("**Usual names:** " + string.Join(", ", quantity.UsualNames));
                builder.AppendLine();
            }

            var description = quantity.DrillingPhysicalQuantity?.DescriptionMd;
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.AppendLine("## Description");
                builder.AppendLine(description.Trim());
                builder.AppendLine();
            }

            if (quantity.UnitChoices is { Count: > 0 })
            {
                builder.AppendLine("## Available Units");
                foreach (var unit in quantity.UnitChoices.Where(u => u is not null))
                {
                    builder.AppendLine($"- **{unit.UnitLabel ?? unit.UnitName ?? "Unit"}** — factor: `{unit.ConversionFactorFromSiFormula ?? "unknown"}`, bias: `{unit.ConversionBiasFromSiFormula ?? "none"}`");
                }
            }

            return builder.ToString();
        }

        private static string BuildUnitContent(PhysicalQuantityDto quantity, UnitChoiceDto unit)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"# {quantity.Name ?? quantity.Id ?? "Physical Quantity"} – {unit.UnitLabel ?? unit.UnitName ?? "Unit"}");
            builder.AppendLine();
            builder.AppendLine($"*Physical quantity id:* {quantity.Id ?? "n/a"}");
            if (!string.IsNullOrWhiteSpace(unit.UnitName))
            {
                builder.AppendLine($"*Unit name:* {unit.UnitName}");
            }
            if (!string.IsNullOrWhiteSpace(unit.UnitLabel))
            {
                builder.AppendLine($"*Unit label:* {unit.UnitLabel}");
            }

            builder.AppendLine();
            builder.AppendLine("## Conversion from SI");
            builder.AppendLine($"- Scaling formula: `{unit.ConversionFactorFromSiFormula ?? "unknown"}`");
            builder.AppendLine($"- Bias formula: `{unit.ConversionBiasFromSiFormula ?? "none"}`");

            var description = quantity.DrillingPhysicalQuantity?.DescriptionMd;
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.AppendLine();
                builder.AppendLine("## Quantity Context");
                builder.AppendLine(description.Trim());
            }

            return builder.ToString();
        }

        private static string CreateQuantityMetadata(PhysicalQuantityDto quantity)
        {
            var obj = new JsonObject
            {
                ["physicalQuantityId"] = quantity.Id,
                ["physicalQuantityName"] = quantity.Name,
                ["usualNames"] = quantity.UsualNames is { Count: > 0 }
                    ? new JsonArray(quantity.UsualNames.Select(name => JsonValue.Create(name)).ToArray())
                    : null
            };

            return obj.ToJsonString(SerializerOptions);
        }

        private static string CreateUnitMetadata(PhysicalQuantityDto quantity, UnitChoiceDto unit)
        {
            var obj = new JsonObject
            {
                ["physicalQuantityId"] = quantity.Id,
                ["physicalQuantityName"] = quantity.Name,
                ["unitName"] = unit.UnitName,
                ["unitLabel"] = unit.UnitLabel
            };

            return obj.ToJsonString(SerializerOptions);
        }

        private static string BuildDocumentId(string prefix, string seed)
        {
            var normalized = seed.ToLowerInvariant();
            normalized = Regex.Replace(normalized, "[^a-z0-9]+", "-");
            normalized = normalized.Trim('-');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                normalized = Guid.NewGuid().ToString("N");
            }

            return $"{prefix}-{normalized}";
        }
    }

    private sealed class SqliteDocumentStore
    {
        private readonly string _dbPath;
        private readonly int _dimensions;

        public SqliteDocumentStore(string dbPath, int dimensions)
        {
            _dbPath = dbPath ?? throw new ArgumentNullException(nameof(dbPath));
            _dimensions = dimensions;
        }

        public async Task ReplaceAsync(IReadOnlyList<DocumentRecord> documents, IEmbeddingClient embeddingClient, CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await ExecuteNonQueryAsync(connection, "DELETE FROM mcp_document_embeddings;", cancellationToken).ConfigureAwait(false);
            await ExecuteNonQueryAsync(connection, "DELETE FROM mcp_documents;", cancellationToken).ConfigureAwait(false);

            foreach (var document in documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var embedding = await embeddingClient.GetEmbeddingAsync(document.Content, cancellationToken).ConfigureAwait(false);
                var rowId = await InsertDocumentAsync(connection, document, cancellationToken).ConfigureAwait(false);
                await InsertEmbeddingAsync(connection, rowId, embedding, cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        private static async Task ExecuteNonQueryAsync(SqliteConnection connection, string commandText, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
        {
            const string createDocuments = """
                CREATE TABLE IF NOT EXISTS mcp_documents (
                    InternalId   INTEGER PRIMARY KEY AUTOINCREMENT,
                    Id           TEXT NOT NULL UNIQUE,
                    Uri          TEXT NOT NULL UNIQUE,
                    Name         TEXT NOT NULL,
                    Title        TEXT,
                    Description  TEXT,
                    MimeType     TEXT NOT NULL,
                    Content      TEXT NOT NULL,
                    MetadataJson TEXT,
                    Size         INTEGER,
                    SortOrder    REAL
                );
                """;

            const string createEmbeddings = """
                CREATE TABLE IF NOT EXISTS mcp_document_embeddings (
                    DocumentInternalId INTEGER PRIMARY KEY,
                    Embedding          BLOB NOT NULL,
                    Dimensions         INTEGER NOT NULL,
                    FOREIGN KEY (DocumentInternalId) REFERENCES mcp_documents(InternalId) ON DELETE CASCADE
                );
                """;

            const string createIndex = """
                CREATE INDEX IF NOT EXISTS ix_mcp_documents_sort ON mcp_documents(SortOrder);
                """;

            await ExecuteNonQueryAsync(connection, createDocuments, cancellationToken).ConfigureAwait(false);
            await ExecuteNonQueryAsync(connection, createEmbeddings, cancellationToken).ConfigureAwait(false);
            await ExecuteNonQueryAsync(connection, createIndex, cancellationToken).ConfigureAwait(false);
        }

        private static byte[] ConvertEmbeddingToBytes(IReadOnlyList<float> embedding)
        {
            var buffer = new byte[embedding.Count * sizeof(float)];
            Buffer.BlockCopy(embedding.ToArray(), 0, buffer, 0, buffer.Length);
            return buffer;
        }

        private async Task<long> InsertDocumentAsync(SqliteConnection connection, DocumentRecord document, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO mcp_documents (Id, Uri, Name, Title, Description, MimeType, Content, MetadataJson, Size, SortOrder)
                VALUES ($id, $uri, $name, $title, $description, $mimeType, $content, $metadata, $size, $sort);
                SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$id", document.Id);
            command.Parameters.AddWithValue("$uri", document.Uri);
            command.Parameters.AddWithValue("$name", document.Name);
            command.Parameters.AddWithValue("$title", (object?)document.Title ?? DBNull.Value);
            command.Parameters.AddWithValue("$description", (object?)document.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("$mimeType", document.MimeType);
            command.Parameters.AddWithValue("$content", document.Content);
            command.Parameters.AddWithValue("$metadata", (object?)document.MetadataJson ?? DBNull.Value);
            command.Parameters.AddWithValue("$size", document.Size);
            command.Parameters.AddWithValue("$sort", document.SortOrder);

            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return (long)(result ?? 0L);
        }

        private async Task InsertEmbeddingAsync(SqliteConnection connection, long internalId, IReadOnlyList<float> embedding, CancellationToken cancellationToken)
        {
            var blob = ConvertEmbeddingToBytes(embedding);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO mcp_document_embeddings (DocumentInternalId, Embedding, Dimensions)
                VALUES ($id, $embedding, $dimensions);
                """;
            command.Parameters.AddWithValue("$id", internalId);
            command.Parameters.Add("$embedding", SqliteType.Blob).Value = blob;
            command.Parameters.AddWithValue("$dimensions", _dimensions);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed record DocumentRecord(
        string Id,
        string Uri,
        string Name,
        string? Title,
        string? Description,
        string MimeType,
        string Content,
        string? MetadataJson,
        long Size,
        double SortOrder);

    private interface IEmbeddingClient
    {
        Task<IReadOnlyList<float>> GetEmbeddingAsync(string text, CancellationToken cancellationToken);
    }

    private sealed class NomicEmbeddingClient : IEmbeddingClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _endpoint;
        private readonly string _model;
        private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

        public NomicEmbeddingClient(string endpoint, string model, string? apiKey)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Embedding endpoint is required.", nameof(endpoint));
            }

            _endpoint = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
                ? uri
                : throw new ArgumentException("Embedding endpoint must be an absolute URI.", nameof(endpoint));

            _model = model ?? throw new ArgumentNullException(nameof(model));

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(100)
            };

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
        }

        public async Task<IReadOnlyList<float>> GetEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            var payload = new EmbeddingRequest(_model, text);
            using var response = await _httpClient.PostAsJsonAsync(_endpoint, payload, _options, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"nomic-embed-text request failed with {(int)response.StatusCode}: {body}");
            }

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(_options, cancellationToken).ConfigureAwait(false);
            var embedding = result?.Data.FirstOrDefault()?.Embedding;
            if (embedding is null)
            {
                throw new InvalidOperationException("nomic-embed-text response did not contain an embedding.");
            }

            return embedding;
        }

        public void Dispose() => _httpClient.Dispose();

        private sealed record EmbeddingRequest([property: JsonPropertyName("model")] string Model, [property: JsonPropertyName("input")] string Input);

        private sealed record EmbeddingResponse([property: JsonPropertyName("data")] List<EmbeddingItem> Data);

        private sealed record EmbeddingItem([property: JsonPropertyName("embedding")] List<float> Embedding);
    }

    private sealed record PhysicalQuantityDto(
        [property: JsonPropertyName("Id")] string? Id,
        [property: JsonPropertyName("Name")] string? Name,
        [property: JsonPropertyName("UsualNames")] List<string>? UsualNames,
        [property: JsonPropertyName("DrillingPhysicalQuantity")] DrillingPhysicalQuantityDto? DrillingPhysicalQuantity,
        [property: JsonPropertyName("UnitChoices")] List<UnitChoiceDto>? UnitChoices);

    private sealed record DrillingPhysicalQuantityDto(
        [property: JsonPropertyName("DescriptionMD")] string? DescriptionMd);

    private sealed record UnitChoiceDto(
        [property: JsonPropertyName("UnitName")] string? UnitName,
        [property: JsonPropertyName("UnitLabel")] string? UnitLabel,
        [property: JsonPropertyName("ConversionFactorFromSIFormula")] string? ConversionFactorFromSiFormula,
        [property: JsonPropertyName("ConversionBiasFromSIFormula")] string? ConversionBiasFromSiFormula);
}
