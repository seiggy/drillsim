using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

/// <summary>
/// Provides opened SQLite connections that know how to load the sqlite-vec extension when available.
/// </summary>
internal sealed class VectorDocumentConnectionFactory
{
    private readonly ILogger<VectorDocumentConnectionFactory> _logger;
    private readonly VectorDocumentDatabaseOptions _options;
    private readonly string _connectionString;

    public VectorDocumentConnectionFactory(
        IOptions<VectorDocumentDatabaseOptions> options,
        ILogger<VectorDocumentConnectionFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
        _connectionString = $"Data Source={_options.DatabasePath}";
    }

    public async Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        TryLoadVectorExtension(connection);
        return connection;
    }

    private void TryLoadVectorExtension(SqliteConnection connection)
    {
        if (!_options.EnableSqliteExtensions || string.IsNullOrWhiteSpace(_options.SqliteVectorExtensionPath))
        {
            return;
        }

        var extensionPath = _options.SqliteVectorExtensionPath;
        if (!Path.IsPathRooted(extensionPath))
        {
            extensionPath = Path.GetFullPath(extensionPath);
        }

        if (!File.Exists(extensionPath))
        {
            _logger.LogWarning("sqlite-vec extension configured at {ExtensionPath} was not found. Vector operations will be skipped.", extensionPath);
            return;
        }

        try
        {
            connection.EnableExtensions(true);
            connection.LoadExtension(extensionPath);
            _logger.LogDebug("sqlite-vec extension loaded from {ExtensionPath}.", extensionPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load sqlite-vec extension from {ExtensionPath}. Continuing without vector acceleration.", extensionPath);
        }
    }
}
