using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ModelContextProtocol.Protocol;
using OSDC.Drilling.EarthMagneticField.Model;
using OSDC.Drilling.EarthMagneticField.Service;
using OSDC.Drilling.EarthMagneticField.Service.Mcp;
using OSDC.Drilling.EarthMagneticField.Service.Mcp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??= "Data Source=EarthMagneticField.db";
builder.AddServiceDefaults();

builder.Services.AddOptions<EarthMagneticFieldServiceOptions>()
    .Bind(builder.Configuration.GetSection(EarthMagneticFieldServiceOptions.SectionName))
    .Validate(value => value.MaximumSamplesPerRequest > 0, "MaximumSamplesPerRequest must be positive.")
    .ValidateOnStart();
builder.Services.AddSingleton(provider =>
{
    EarthMagneticFieldServiceOptions options = provider.GetRequiredService<IOptions<EarthMagneticFieldServiceOptions>>().Value;
    return new EarthMagneticFieldEvaluator(options.ModelDirectory);
});
builder.Services.AddSingleton<UsageStatisticsEarthMagneticField>();
builder.Services.AddControllers().AddJsonOptions(options => JsonSettings.ApplyTo(options.JsonSerializerOptions));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(configuration =>
{
    configuration.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OSDC Earth Magnetic Field API",
        Version = "v1",
        Description = "Stateless WMM2025 and IGRF14 evaluation using UTC, WGS84 SI coordinates, and north-east-down magnetic flux density in teslas."
    });
    configuration.CustomSchemaIds(type => type.FullName);
    foreach (string assemblyName in new[] { "Service", "Model" })
    {
        string xmlPath = Path.Combine(AppContext.BaseDirectory, assemblyName + ".xml");
        if (File.Exists(xmlPath)) configuration.IncludeXmlComments(xmlPath);
    }
});

string serverVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "OSDC Earth Magnetic Field", Version = serverVersion };
    options.Capabilities = new ServerCapabilities { Tools = new ToolsCapability() };
}).WithHttpTransport(options => options.Stateless = true);
builder.Services.AddEarthMagneticFieldMcpTool<PingMcpTool>();
builder.Services.AddEarthMagneticFieldMcpTool<GetEarthMagneticFieldModelInfoMcpTool>();
builder.Services.AddEarthMagneticFieldMcpTool<EvaluateEarthMagneticFieldMcpTool>();

var app = builder.Build();
_ = app.Services.GetRequiredService<EarthMagneticFieldEvaluator>().ServiceInfo;

app.UsePathBase("/EarthMagneticField/api");
var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaders.KnownNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

const string mergedSwaggerPath = "/swagger/merged/swagger.json";
string mergedSwaggerFile = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "json-schema", "EarthMagneticFieldMergedModel.json");
if (File.Exists(mergedSwaggerFile))
{
    app.UseCustomSwagger(SwaggerMiddlewareExtensions.ReadOpenApiDocument(mergedSwaggerFile), mergedSwaggerPath);
}
else
{
    app.UseSwagger();
}
app.UseSwaggerUI(configuration =>
    configuration.SwaggerEndpoint(File.Exists(mergedSwaggerFile) ? $"/EarthMagneticField/api{mergedSwaggerPath}" : "/EarthMagneticField/api/swagger/v1/swagger.json", "OSDC Earth Magnetic Field API"));

app.MapGet("/health/live", () => Results.Ok(new { Status = "Healthy" })).ExcludeFromDescription();
app.MapGet("/health/ready", (EarthMagneticFieldEvaluator evaluator) => Results.Ok(new { Status = "Healthy", Models = evaluator.ServiceInfo.Models.Select(model => model.ID) })).ExcludeFromDescription();
app.MapGet("/metrics", (UsageStatisticsEarthMagneticField usage) => Results.Text(
    $"# TYPE earth_magnetic_field_rest_evaluations_total counter\nearth_magnetic_field_rest_evaluations_total {usage.RestEvaluations}\n" +
    $"# TYPE earth_magnetic_field_mcp_evaluations_total counter\nearth_magnetic_field_mcp_evaluations_total {usage.MCPEvaluations}\n" +
    $"# TYPE earth_magnetic_field_failed_evaluations_total counter\nearth_magnetic_field_failed_evaluations_total {usage.FailedEvaluations}\n" +
    $"# TYPE earth_magnetic_field_samples_evaluated_total counter\nearth_magnetic_field_samples_evaluated_total {usage.SamplesEvaluated}\n",
    "text/plain; version=0.0.4; charset=utf-8")).ExcludeFromDescription();
app.MapControllers();
app.MapMcp("/mcp");
app.MapDefaultEndpoints();
app.Run();

public partial class Program;
