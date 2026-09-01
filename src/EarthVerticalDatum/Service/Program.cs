using Microsoft.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using OSDC.Drilling.EarthVerticalDatum.Model;
using OSDC.Drilling.EarthVerticalDatum.Service;
using OSDC.Drilling.EarthVerticalDatum.Service.Mcp;
using OSDC.Drilling.EarthVerticalDatum.Service.Mcp.Tools;
using System.Threading.Tasks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??= "Data Source=EarthVerticalDatum.db";
builder.AddServiceDefaults();

builder.Services.AddOptions<EarthVerticalDatumServiceOptions>()
    .Bind(builder.Configuration.GetSection(EarthVerticalDatumServiceOptions.SectionName))
    .Validate(value => value.MaximumPositionsPerRequest > 0, "MaximumPositionsPerRequest must be positive.")
    .ValidateOnStart();
builder.Services.AddSingleton(provider =>
{
    EarthVerticalDatumServiceOptions options = provider.GetRequiredService<IOptions<EarthVerticalDatumServiceOptions>>().Value;
    return new EarthVerticalDatumEvaluator(options.ModelDirectory);
});
builder.Services.AddSingleton<UsageStatisticsEarthVerticalDatum>();
builder.Services.AddControllers().AddJsonOptions(options => JsonSettings.ApplyTo(options.JsonSerializerOptions));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi("v1", options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.CreateSchemaReferenceId = jsonTypeInfo => jsonTypeInfo.Type.FullName;
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "OSDC Earth Vertical Datum API",
            Version = "v1",
            Description = "Stateless bidirectional conversion between EGM84 mean-sea-level and WGS84 ellipsoidal depths using OSDC SI and positive-down conventions."
        };
        document.Servers = [new OpenApiServer { Url = "/EarthVerticalDatum/api" }];
        return Task.CompletedTask;
    });
});

string serverVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "OSDC Earth Vertical Datum", Version = serverVersion };
    options.Capabilities = new ServerCapabilities { Tools = new ToolsCapability() };
}).WithHttpTransport(options => options.Stateless = true);
builder.Services.AddEarthVerticalDatumMcpTool<PingMcpTool>();
builder.Services.AddEarthVerticalDatumMcpTool<GetEarthVerticalDatumModelInfoMcpTool>();
builder.Services.AddEarthVerticalDatumMcpTool<ConvertMeanSeaLevelToWgs84McpTool>();
builder.Services.AddEarthVerticalDatumMcpTool<ConvertWgs84ToMeanSeaLevelMcpTool>();

var app = builder.Build();
_ = app.Services.GetRequiredService<EarthVerticalDatumEvaluator>().ModelInfo;

app.UsePathBase("/EarthVerticalDatum/api");
var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaders.KnownNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

const string mergedSwaggerPath = "/swagger/merged/swagger.json";
string mergedSwaggerFile = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "json-schema", "EarthVerticalDatumMergedModel.json");
if (File.Exists(mergedSwaggerFile))
{
    app.UseCustomSwagger(SwaggerMiddlewareExtensions.ReadOpenApiDocument(mergedSwaggerFile), mergedSwaggerPath);
}
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/swagger", options => options
    .WithTitle("OSDC Earth Vertical Datum API")
    .WithOpenApiRoutePattern(File.Exists(mergedSwaggerFile) ? $"/EarthVerticalDatum/api{mergedSwaggerPath}" : "/EarthVerticalDatum/api/swagger/v1/swagger.json"));

app.MapGet("/health/live", () => Results.Ok(new { Status = "Healthy" })).ExcludeFromDescription();
app.MapGet("/health/ready", (EarthVerticalDatumEvaluator evaluator) => Results.Ok(new { Status = "Healthy", evaluator.ModelInfo.ID })).ExcludeFromDescription();
app.MapGet("/metrics", (UsageStatisticsEarthVerticalDatum usage) => Results.Text(
    $"# TYPE earth_vertical_datum_rest_conversions_total counter\nearth_vertical_datum_rest_conversions_total {usage.RestConversions}\n" +
    $"# TYPE earth_vertical_datum_mcp_conversions_total counter\nearth_vertical_datum_mcp_conversions_total {usage.MCPConversions}\n" +
    $"# TYPE earth_vertical_datum_failed_conversions_total counter\nearth_vertical_datum_failed_conversions_total {usage.FailedConversions}\n" +
    $"# TYPE earth_vertical_datum_positions_converted_total counter\nearth_vertical_datum_positions_converted_total {usage.PositionsConverted}\n",
    "text/plain; version=0.0.4; charset=utf-8")).ExcludeFromDescription();
app.MapControllers();
app.MapMcp("/mcp");
app.MapDefaultEndpoints();
app.Run();

public partial class Program;
