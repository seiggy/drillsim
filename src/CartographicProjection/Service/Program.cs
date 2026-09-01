using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;
using NORCE.Drilling.CartographicProjection.Service;
using NORCE.Drilling.CartographicProjection.Service.Managers;
using NORCE.Drilling.CartographicProjection.Service.Mcp;
using NORCE.Drilling.CartographicProjection.Service.Mcp.Tools;
using System;
using System.IO;
using System.Threading.Tasks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??=
    $"Data Source={SqlConnectionManager.HOME_DIRECTORY}{SqlConnectionManager.DATABASE_FILENAME}";
builder.AddServiceDefaults();

string externalConfigPath = builder.Configuration["CARTOGRAPHICPROJECTION_EXTERNAL_CONFIG"]
    ?? Path.Combine(SqlConnectionManager.HOME_DIRECTORY, "CartographicProjection.Service.json");
builder.Configuration.AddJsonFile(externalConfigPath, optional: true, reloadOnChange: true);

// registering the manager of SQLite connections through dependency injection
builder.Services.AddSingleton(sp =>
    new SqlConnectionManager(
        builder.Configuration["ConnectionStrings:Sqlite"]!,
        sp.GetRequiredService<ILogger<SqlConnectionManager>>()));

// registering the database cleaner service through dependency injection
builder.Services.AddHostedService(sp => new DatabaseCleanerService(
    sp.GetRequiredService<ILogger<DatabaseCleanerService>>(),
    sp.GetRequiredService<SqlConnectionManager>()));

// serialization settings (using System.Json)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        JsonSettings.ApplyTo(options.JsonSerializerOptions);
    });

builder.Services.AddOpenApi("v1", options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.CreateSchemaReferenceId = jsonTypeInfo => jsonTypeInfo.Type.FullName;
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = [new OpenApiServer { Url = "/cartographicprojection/api" }];
        return Task.CompletedTask;
    });
});

builder.Services.Configure<McpHubOptions>(builder.Configuration.GetSection(McpHubOptions.SectionName));
builder.Services.AddHttpClient(nameof(McpHubRegistrationService));
builder.Services.AddHostedService<McpHubRegistrationService>();

// MCP server registrations
var serverVersion = typeof(SqlConnectionManager).Assembly.GetName().Version?.ToString() ?? "1.0.0";

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation
    {
        Name = "CartographicProjectionService",
        Version = serverVersion
    };
    options.Capabilities = new ServerCapabilities
    {
        Tools = new ToolsCapability()
    };
}).WithHttpTransport();

builder.Services.AddLegacyMcpTool<PingMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicProjectionIdsMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicProjectionMetaInfoMcpTool>();
builder.Services.AddLegacyMcpTool<GetCartographicProjectionByIdMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicProjectionLightMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicProjectionMcpTool>();
builder.Services.AddLegacyMcpTool<PostCartographicProjectionMcpTool>();
builder.Services.AddLegacyMcpTool<PutCartographicProjectionByIdMcpTool>();
builder.Services.AddLegacyMcpTool<DeleteCartographicProjectionByIdMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicConversionSetIdsMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicConversionSetMetaInfoMcpTool>();
builder.Services.AddLegacyMcpTool<GetCartographicConversionSetByIdMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicConversionSetLightMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicConversionSetMcpTool>();
builder.Services.AddLegacyMcpTool<PostCartographicConversionSetMcpTool>();
builder.Services.AddLegacyMcpTool<PutCartographicConversionSetByIdMcpTool>();
builder.Services.AddLegacyMcpTool<DeleteCartographicConversionSetByIdMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicProjectionTypeIdsMcpTool>();
builder.Services.AddLegacyMcpTool<GetCartographicProjectionTypeByIdMcpTool>();
builder.Services.AddLegacyMcpTool<GetAllCartographicProjectionTypeMcpTool>();
builder.Services.AddLegacyMcpTool<GetCartographicProjectionUsageStatisticsMcpTool>();

// end MCP server

var app = builder.Build();

var basePath = "/cartographicprojection/api";
var scheme = "http";

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

if (!String.IsNullOrEmpty(builder.Configuration["GeodeticDatumHostURL"]))
    Configuration.GeodeticDatumHostURL = builder.Configuration["GeodeticDatumHostURL"];

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

string relativeSwaggerPath = "/swagger/merged/swagger.json";
string fullSwaggerPath = $"{basePath}{relativeSwaggerPath}";

var mergedDoc = SwaggerMiddlewareExtensions.ReadOpenApiDocument("wwwroot/json-schema/CartographicProjectionMergedModel.json");
app.UseCustomSwagger(mergedDoc, relativeSwaggerPath);
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/swagger", options => options
    .WithTitle("Merged API Version 1")
    .WithOpenApiRoutePattern(fullSwaggerPath));

app.UseCors(cors => cors
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials()
           );

app.MapMcp("/mcp");
app.MapMcpWebSocket("/mcp/ws");
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapDefaultEndpoints();

app.Run();
