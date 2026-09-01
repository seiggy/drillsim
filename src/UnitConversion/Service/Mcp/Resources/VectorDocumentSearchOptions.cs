using System;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

/// <summary>
/// Describes how query-time vector searches should be executed.
/// </summary>
public sealed class VectorDocumentSearchOptions
{
    /// <summary>
    /// Gets or sets the default number of search results to return when no limit is provided.
    /// </summary>
    public int DefaultLimit { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of results that callers may request.
    /// </summary>
    public int MaxLimit { get; set; } = 40;

    /// <summary>
    /// Gets or sets the embedding model identifier that should be used for query encoding.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Gets or sets the expected embedding dimensionality.
    /// </summary>
    public int EmbeddingDimensions { get; set; } = 1536;

    /// <summary>
    /// Gets or sets the configuration used when calling the OpenAI embeddings endpoint.
    /// </summary>
    public NomicEmbeddingOptions Nomic { get; set; } = new();

    /// <summary>
    /// Nested options that control nomic-ai/nomic-embed-text embedding calls.
    /// </summary>
    public sealed class NomicEmbeddingOptions
    {
        /// <summary>
        /// Gets or sets the absolute embeddings endpoint URL (for example http://localhost:8080/embeddings).
        /// </summary>
        public string Endpoint { get; set; } = "http://localhost:8080/embeddings";

        /// <summary>
        /// Gets or sets an optional API key. Useful when fronting the inference server with a gateway.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the environment variable that stores the API key. Defaults to NOMIC_API_KEY.
        /// </summary>
        public string? ApiKeyEnvironmentVariable { get; set; } = "NOMIC_API_KEY";

        /// <summary>
        /// Gets or sets the header used when sending the API key. Defaults to Authorization.
        /// </summary>
        public string? ApiKeyHeader { get; set; } = "Authorization";

        /// <summary>
        /// Gets or sets the value prefix inserted before the API key (for example "Bearer ").
        /// </summary>
        public string? ApiKeyPrefix { get; set; } = "Bearer ";

        /// <summary>
        /// Gets or sets the timeout applied to outbound embedding HTTP calls.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
