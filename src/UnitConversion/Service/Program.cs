using Microsoft.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OSDC.UnitConversion.Service.Mcp;
using OSDC.UnitConversion.Service.Mcp.Prompts;
using OSDC.UnitConversion.Service.Mcp.Resources;
using OSDC.UnitConversion.Service.Mcp.Tools;
using OSDC.UnitConversion.Service;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??=
    $"Data Source={Path.Combine(SqlConnectionManager.HOME_DIRECTORY, SqlConnectionManager.DATABASE_FILENAME)}";
builder.AddServiceDefaults();

string externalConfigPath = builder.Configuration["UNITCONVERSION_EXTERNAL_CONFIG"]
    ?? Path.Combine(SqlConnectionManager.HOME_DIRECTORY, "UnitConversion.Service.json");
builder.Configuration.AddJsonFile(externalConfigPath, optional: true, reloadOnChange: true);

// registering the manager of SQLite connections through dependency injection
builder.Services.AddSingleton(sp => new SqlConnectionManager(
    builder.Configuration["ConnectionStrings:Sqlite"]!,
    sp.GetRequiredService<ILogger<SqlConnectionManager>>()));

// registering the database cleaner service through dependency injection
builder.Services.AddHostedService(sp => new DatabaseCleanerService(
    sp.GetRequiredService<ILogger<DatabaseCleanerService>>(),
    sp.GetRequiredService<SqlConnectionManager>()));

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // preserves C# properties naming conventions (no forced lower case applied)
        options.JsonSerializerOptions.PropertyNamingPolicy = null;

        // allows to serialize enums as strings (and not integers)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi("v1", options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.CreateSchemaReferenceId = jsonTypeInfo => jsonTypeInfo.Type.FullName?.Replace("+", ".");
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = [new OpenApiServer { Url = "/unitconversion/api" }];
        return Task.CompletedTask;
    });
});

builder.Services.Configure<McpHubOptions>(builder.Configuration.GetSection(McpHubOptions.SectionName));
builder.Services.AddHttpClient(nameof(McpHubRegistrationService));
builder.Services.AddHostedService<McpHubRegistrationService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("mcp", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

builder.Services.Configure<VectorDocumentDatabaseOptions>(builder.Configuration.GetSection("VectorDocumentDatabase"));
builder.Services.Configure<VectorDocumentSearchOptions>(builder.Configuration.GetSection("VectorDocumentSearch"));
builder.Services.AddSingleton<VectorDocumentConnectionFactory>();
builder.Services.AddSingleton<IVectorDocumentRepository, VectorDocumentRepository>();
builder.Services.AddHttpClient<ITextEmbeddingGenerator, NomicEmbeddingGenerator>();
builder.Services.AddSingleton<VectorDocumentResourceService>();
builder.Services.AddOptions<McpServerHandlers>()
    .Configure<VectorDocumentResourceService>((handlers, resourceService) =>
    {
        handlers.ListResourcesHandler = resourceService.ListResourcesAsync;
        handlers.ReadResourceHandler = resourceService.ReadResourceAsync;
    });

// MCP server registrations
var serverVersion = typeof(SqlConnectionManager).Assembly.GetName().Version?.ToString() ?? "1.0.0";

var mcpBuilder = builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation
    {
        Name = "UnitConversionService",
        Version = serverVersion
    };
    options.ServerInstructions = "Use convert_values for direct unit conversions and convert_between_unit_systems when unit systems select the units. Resolve ambiguous quantities with search_physical_quantities, then inspect get_physical_quantity. Conversion tools are read-only and do not persist cases. Unit-system create, replace, and delete tools persist changes and should be used only when explicitly requested.";
    options.Capabilities = new ServerCapabilities
    {
        Tools = new ToolsCapability(),
        Resources = new ResourcesCapability(),
        Prompts = new PromptsCapability()
    };
});
mcpBuilder.WithHttpTransport();
mcpBuilder.WithPrompts<UnitConversionPromptCollection>(new JsonSerializerOptions(JsonSerializerDefaults.Web));

builder.Services.AddMcpTool<SearchPhysicalQuantitiesMcpTool>();
builder.Services.AddMcpTool<GetPhysicalQuantityByIdMcpTool>();
builder.Services.AddMcpTool<ConvertValuesMcpTool>();
builder.Services.AddMcpTool<ConvertBetweenUnitSystemsMcpTool>();
builder.Services.AddMcpTool<ListUnitSystemsMcpTool>();
builder.Services.AddMcpTool<GetUnitSystemByIdMcpTool>();
builder.Services.AddMcpTool<PostUnitSystemMcpTool>();
builder.Services.AddMcpTool<PutUnitSystemByIdMcpTool>();
builder.Services.AddMcpTool<DeleteUnitSystemByIdMcpTool>();
builder.Services.AddMcpTool<SearchVectorDocumentsMcpTool>();

// end MCP server

var app = builder.Build();

VectorDocumentSeedInitializer.EnsureSeeded(app.Services);

var basePath = "/unitconversion/api";

app.UsePathBase(basePath);

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value ?? string.Empty;
    if (path.Contains("/.well-known/oauth-protected-resource", System.StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/.well-known/oauth-authorization-server", System.StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/json";
        context.Response.Headers.CacheControl = "no-store";
        string body = "{\"error\":\"oauth_not_configured\",\"error_description\":\"This MCP server does not require OAuth. Connect directly to the MCP endpoint.\",\"authentication\":\"none\"}";
        await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(body));
        return;
    }

    await next();
});

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/swagger", options => options
    .WithTitle("API Version 1")
    .WithOpenApiRoutePattern($"{basePath}/swagger/v1/swagger.json"));

app.UseCors(cors => cors
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials()
           );

app.MapMcp("/mcp").RequireRateLimiting("mcp");
app.MapMcpWebSocket("/mcp/ws").RequireRateLimiting("mcp");
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapDefaultEndpoints();

app.Run();
