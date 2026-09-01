using System;
using System.IO;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

/// <summary>
/// Configuration settings describing how the vectorized document SQLite database should be accessed.
/// </summary>
public sealed class VectorDocumentDatabaseOptions
{
    /// <summary>
    /// Gets or sets the optional seed database path copied into the container image.
    /// When configured, the service copies this file to <see cref="DatabasePath"/> at startup
    /// only if the target database does not already exist.
    /// </summary>
    public string? SeedDatabasePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "seed", "UnitConversionVectors.db");

    /// <summary>
    /// Gets or sets the absolute or relative path to the SQLite database that stores the vectorized resources.
    /// Defaults to a file that lives next to the primary UnitConversion database.
    /// </summary>
    public string DatabasePath { get; set; } = Path.Combine(SqlConnectionManager.HOME_DIRECTORY, "UnitConversionVectors.db");

    /// <summary>
    /// Gets or sets the table that exposes the MCP-ready document rows.
    /// </summary>
    public string DocumentTableName { get; set; } = "mcp_documents";

    /// <summary>
    /// Gets or sets the optional path to the sqlite-vec extension to load for vector queries.
    /// </summary>
    public string? SqliteVectorExtensionPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether native SQLite extensions should be enabled when opening the vector database.
    /// </summary>
    public bool EnableSqliteExtensions { get; set; } = true;

    /// <summary>
    /// Gets or sets the default page size used when serving resources/list requests.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the prefix that is used when the database row does not provide an explicit URI.
    /// </summary>
    public string ResourceUriPrefix { get; set; } = "resource://unit-conversion/documents/";

    /// <summary>
    /// Gets or sets the MIME type to use when the database row does not specify one.
    /// </summary>
    public string? DefaultMimeType { get; set; } = "text/plain";
}
