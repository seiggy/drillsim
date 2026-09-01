using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

internal static class VectorDocumentSeedInitializer
{
    public static void EnsureSeeded(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<VectorDocumentDatabaseOptions>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("VectorDocumentSeedInitializer");

        if (string.IsNullOrWhiteSpace(options.DatabasePath))
        {
            logger.LogWarning("Vector document database seeding skipped because VectorDocumentDatabase:DatabasePath is not configured.");
            return;
        }

        if (string.IsNullOrWhiteSpace(options.SeedDatabasePath))
        {
            logger.LogInformation("Vector document database seeding skipped because VectorDocumentDatabase:SeedDatabasePath is not configured.");
            return;
        }

        var targetPath = Path.GetFullPath(options.DatabasePath);
        var seedPath = Path.GetFullPath(options.SeedDatabasePath);

        if (string.Equals(targetPath, seedPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(targetPath))
        {
            logger.LogDebug("Vector document database already exists at {DatabasePath}; seed copy skipped.", targetPath);
            return;
        }

        if (!File.Exists(seedPath))
        {
            logger.LogWarning("Vector document seed database was not found at {SeedDatabasePath}. MCP document resources will be unavailable unless {DatabasePath} exists.", seedPath, targetPath);
            return;
        }

        try
        {
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(seedPath, targetPath, overwrite: false);
            logger.LogInformation("Copied vector document seed database from {SeedDatabasePath} to {DatabasePath}.", seedPath, targetPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to copy vector document seed database from {SeedDatabasePath} to {DatabasePath}.", seedPath, targetPath);
        }
    }
}
