using Microsoft.OpenApi;
using System.Threading.Tasks;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using ModelContextProtocol.Protocol;
using NORCE.Drilling.Simulator4nDOF.Service;
using NORCE.Drilling.Simulator4nDOF.Service.Managers;
using NORCE.Drilling.Simulator4nDOF.Service.Mcp;
using NORCE.Drilling.Simulator4nDOF.Service.Mcp.Tools;
using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??=
    $"Data Source={SqlConnectionManager.HOME_DIRECTORY}{SqlConnectionManager.DATABASE_FILENAME}";
builder.AddServiceDefaults();

// registering the manager of SQLite connections through dependency injection
builder.Services.AddSingleton(sp =>
    new SqlConnectionManager(
        builder.Configuration["ConnectionStrings:Sqlite"]!,
        sp.GetRequiredService<ILogger<SqlConnectionManager>>()));

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
        document.Servers = [new OpenApiServer { Url = "/simulator4ndof/api" }];
        return Task.CompletedTask;
    });
});

// MCP server registrations
var serverVersion = typeof(SqlConnectionManager).Assembly.GetName().Version?.ToString() ?? "1.0.0";

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation
    {
        Name = "Simulator4nDOFService",
        Version = serverVersion
    };
    options.Capabilities = new ServerCapabilities
    {
        Tools = new ToolsCapability()
    };
}).WithHttpTransport();

builder.Services.AddLegacyMcpTool<PingMcpTool>();

// end MCP server

var app = builder.Build();

var basePath = "/simulator4ndof/api";
var scheme = "http";

app.UsePathBase(basePath);

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

if (!String.IsNullOrEmpty(builder.Configuration["ClusterHostURL"]))
    ServiceConfiguration.ClusterHostURL = builder.Configuration["ClusterHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["WellBoreHostURL"]))
    ServiceConfiguration.WellBoreHostURL = builder.Configuration["WellBoreHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["WellHostURL"]))
    ServiceConfiguration.WellHostURL = builder.Configuration["WellHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["DrillStringHostURL"]))
    ServiceConfiguration.DrillStringHostURL = builder.Configuration["DrillStringHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["TrajectoryHostURL"]))
    ServiceConfiguration.TrajectoryHostURL = builder.Configuration["TrajectoryHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["DrillingFluidHostURL"]))
    ServiceConfiguration.DrillingFluidHostURL = builder.Configuration["DrillingFluidHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["WellBoreArchitectureHostURL"]))
    ServiceConfiguration.WellBoreArchitectureHostURL = builder.Configuration["WellBoreArchitectureHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["RigHostURL"]))
    ServiceConfiguration.RigHostURL = builder.Configuration["RigHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["GeothermalPropertiesHostURL"]))
    ServiceConfiguration.GeothermalPropertiesHostURL = builder.Configuration["GeothermalPropertiesHostURL"];


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

var mergedDoc = SwaggerMiddlewareExtensions.ReadOpenApiDocument("wwwroot/json-schema/Simulator4nDOFMergedModel.json");
app.UseCustomSwagger(mergedDoc, relativeSwaggerPath);
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/swagger", options => options.WithOpenApiRoutePattern(fullSwaggerPath));

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
