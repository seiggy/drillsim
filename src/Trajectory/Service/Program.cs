using DrillSim.PublicationGate;
using Microsoft.OpenApi;
using System.Threading.Tasks;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using NORCE.Drilling.Trajectory.Service;
using NORCE.Drilling.Trajectory.Service.Managers;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??= $"Data Source={Path.Combine("..", "home", "Trajectory.db")}";
builder.AddServiceDefaults();
builder.AddScenarioPublicationGate();

// registering the managers of SQLite connections through dependency injection
builder.Services.AddSingleton(sp => new SqlConnectionManagerTrajectory(
    builder.Configuration["ConnectionStrings:Sqlite"]!,
    sp.GetRequiredService<ILogger<SqlConnectionManagerTrajectory>>()));
builder.Services.AddSingleton<SqlConnectionManager>(sp => sp.GetRequiredService<SqlConnectionManagerTrajectory>());
builder.Services.AddSingleton<SqlConnectionManagerSeparationFactorResults>();
builder.Services.AddSingleton<SqlConnectionManagerOctree>();

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
        document.Servers = [new OpenApiServer { Url = "/trajectory/api" }];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

var basePath = "/trajectory/api";

app.UsePathBase(basePath);
app.UseScenarioPublicationGate();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

if (!String.IsNullOrEmpty(builder.Configuration["FieldHostURL"]))
    ServiceConfiguration.FieldHostURL = builder.Configuration["FieldHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["ClusterHostURL"]))
    ServiceConfiguration.ClusterHostURL = builder.Configuration["ClusterHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["WellBoreHostURL"]))
    ServiceConfiguration.WellBoreHostURL = builder.Configuration["WellBoreHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["WellBoreArchitectureHostURL"]))
    ServiceConfiguration.WellBoreArchitectureHostURL = builder.Configuration["WellBoreArchitectureHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["WellHostURL"]))
    ServiceConfiguration.WellHostURL = builder.Configuration["WellHostURL"];
if (!String.IsNullOrEmpty(builder.Configuration["SurveyInstrumentHostURL"]))
    ServiceConfiguration.SurveyInstrumentHostURL = builder.Configuration["SurveyInstrumentHostURL"];

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
string exposedModel = "wwwroot/json-schema/TrajectoryMergedModel.json";
string scalarDocumentPath = $"{basePath}/swagger/v1/swagger.json";
if (File.Exists(exposedModel))
{
    var mergedDoc = SwaggerMiddlewareExtensions.ReadOpenApiDocument(exposedModel);
    app.UseCustomSwagger(mergedDoc, relativeSwaggerPath);
    scalarDocumentPath = fullSwaggerPath;
}
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.MapScalarApiReference("/swagger", options => options.WithOpenApiRoutePattern(scalarDocumentPath));

app.UseCors(cors => cors
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials()
           );

app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapDefaultEndpoints();

app.MapScenarioPublicationGateEndpoints();
app.Run();

public partial class Program { }
