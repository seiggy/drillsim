using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace OSDC.UnitConversion.Service.Mcp;

internal static class McpServiceCollectionExtensions
{
    public static IServiceCollection AddMcpTool<TTool>(this IServiceCollection services)
        where TTool : class, IMcpTool
    {
        services.AddSingleton<TTool>();
        services.AddSingleton<IMcpTool>(sp => sp.GetRequiredService<TTool>());
        services.AddSingleton<McpServerTool>(sp =>
        {
            var tool = sp.GetRequiredService<TTool>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new McpServerToolAdapter(tool, loggerFactory);
        });

        return services;
    }
}
