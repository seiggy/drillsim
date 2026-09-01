using Microsoft.OpenApi;
using System.Threading.Tasks;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using NORCE.Drilling.GeothermalProperties.Service;
using NORCE.Drilling.GeothermalProperties.Service.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration["ConnectionStrings:Sqlite"] ??=
    $"Data Source={SqlConnectionManager.HOME_DIRECTORY}{SqlConnectionManager.DATABASE_FILENAME}";
builder.AddServiceDefaults();

// registering the manager of SQLite connections through dependency injection
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<SqlConnectionManager>>();

    var connectionString = builder.Configuration["ConnectionStrings:Sqlite"]!;
    var dbPath = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString).DataSource;
    return new SqlConnectionManager(connectionString, logger, dbPath);
});

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
        document.Servers = [new OpenApiServer { Url = "/geothermalproperties/api" }];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

var basePath = "/geothermalproperties/api";
var scheme = "http";

app.UsePathBase(basePath);

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
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

string relativeSwaggerPath = "/swagger/merged/swagger.json";
string fullSwaggerPath = $"{basePath}{relativeSwaggerPath}";
string exposedModel = "wwwroot/json-schema/GeothermalPropertiesMergedModel.json";
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

app.Run();
