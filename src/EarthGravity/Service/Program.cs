using Microsoft.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using OSDC.Drilling.EarthGravity.Model;
using OSDC.Drilling.EarthGravity.Service;
using OSDC.Drilling.EarthGravity.Service.Mcp;
using OSDC.Drilling.EarthGravity.Service.Mcp.Tools;
using System.Threading.Tasks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??= "Data Source=EarthGravity.db";
builder.AddServiceDefaults();

builder.Services.AddOptions<EarthGravityServiceOptions>()
    .Bind(builder.Configuration.GetSection(EarthGravityServiceOptions.SectionName))
    .Validate(value => value.MaximumPositionsPerRequest > 0, "MaximumPositionsPerRequest must be positive.")
    .ValidateOnStart();
builder.Services.AddSingleton(provider =>
{
    EarthGravityServiceOptions options = provider.GetRequiredService<IOptions<EarthGravityServiceOptions>>().Value;
    return new EarthGravityEvaluator(options.ModelDirectory);
});
builder.Services.AddSingleton<UsageStatisticsEarthGravity>();
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
            Title = "OSDC Earth Gravity API",
            Version = "v1",
            Description = "Stateless EGM96 evaluation using OSDC SI and WGS84 conventions."
        };
        document.Servers = [new OpenApiServer { Url = "/EarthGravity/api" }];
        return Task.CompletedTask;
    });
});

string serverVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "OSDC Earth Gravity", Version = serverVersion };
    options.Capabilities = new ServerCapabilities { Tools = new ToolsCapability() };
}).WithHttpTransport(options => options.Stateless = true);
builder.Services.AddEarthGravityMcpTool<PingMcpTool>();
builder.Services.AddEarthGravityMcpTool<GetEarthGravityModelInfoMcpTool>();
builder.Services.AddEarthGravityMcpTool<EvaluateEarthGravityMcpTool>();

var app = builder.Build();
_ = app.Services.GetRequiredService<EarthGravityEvaluator>().ModelInfo;

app.UsePathBase("/EarthGravity/api");
var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaders.KnownNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

const string mergedSwaggerPath = "/swagger/merged/swagger.json";
string mergedSwaggerFile = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "json-schema", "EarthGravityMergedModel.json");
if (File.Exists(mergedSwaggerFile))
{
    app.UseCustomSwagger(SwaggerMiddlewareExtensions.ReadOpenApiDocument(mergedSwaggerFile), mergedSwaggerPath);
}
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/swagger", options => options
    .WithTitle("OSDC Earth Gravity API")
    .WithOpenApiRoutePattern(File.Exists(mergedSwaggerFile) ? $"/EarthGravity/api{mergedSwaggerPath}" : "/EarthGravity/api/swagger/v1/swagger.json"));

app.MapGet("/health/live", () => Results.Ok(new { Status = "Healthy" })).ExcludeFromDescription();
app.MapGet("/health/ready", (EarthGravityEvaluator evaluator) => Results.Ok(new { Status = "Healthy", evaluator.ModelInfo.ID })).ExcludeFromDescription();
app.MapGet("/metrics", (UsageStatisticsEarthGravity usage) => Results.Text(
    $"# TYPE earth_gravity_rest_evaluations_total counter\nearth_gravity_rest_evaluations_total {usage.RestEvaluations}\n" +
    $"# TYPE earth_gravity_mcp_evaluations_total counter\nearth_gravity_mcp_evaluations_total {usage.MCPEvaluations}\n" +
    $"# TYPE earth_gravity_failed_evaluations_total counter\nearth_gravity_failed_evaluations_total {usage.FailedEvaluations}\n" +
    $"# TYPE earth_gravity_positions_total counter\nearth_gravity_positions_total {usage.PositionsEvaluated}\n",
    "text/plain; version=0.0.4; charset=utf-8")).ExcludeFromDescription();
app.MapControllers();
app.MapMcp("/mcp");
app.MapDefaultEndpoints();
app.Run();

public partial class Program;
