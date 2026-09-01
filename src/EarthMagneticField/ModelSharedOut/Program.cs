using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;

DirectoryInfo? root = new(Directory.GetCurrentDirectory());
while (root != null && root.GetFiles("*.sln").Length == 0) root = root.Parent;
if (root == null) throw new InvalidOperationException("Unable to locate the Earth Magnetic Field solution root.");

string sharedDirectory = Path.Combine(root.FullName, "ModelSharedOut");
string inputDirectory = Path.Combine(sharedDirectory, "json-schemas");
string serviceSchemaDirectory = Path.Combine(root.FullName, "Service", "wwwroot", "json-schema");
Directory.CreateDirectory(inputDirectory);
Directory.CreateDirectory(serviceSchemaDirectory);

var merged = new OpenApiDocument
{
    Info = new OpenApiInfo
    {
        Title = "OSDC Earth Magnetic Field distributed shared model",
        Description = "Generated API client and DTO contract consumed by WebPages and ServiceTest.",
        Version = "1.0"
    },
    Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() },
    Paths = new OpenApiPaths()
};

string[] sources = Directory.GetFiles(inputDirectory, "*.json");
if (sources.Length == 0)
    throw new InvalidOperationException($"No OpenAPI documents were found in '{inputDirectory}'. Build Service in Debug or run dotnet swagger first.");

foreach (string sourcePath in sources)
{
    await using FileStream stream = File.OpenRead(sourcePath);
    OpenApiDocument source = new OpenApiStreamReader().Read(stream, out var diagnostic);
    if (diagnostic.Errors.Count != 0)
        throw new InvalidOperationException($"'{sourcePath}' contains invalid OpenAPI: {string.Join("; ", diagnostic.Errors.Select(error => error.Message))}");

    foreach ((string path, OpenApiPathItem item) in source.Paths) merged.Paths[path] = item;
    new OpenApiSchemaReferenceUpdater().MergeSchemasAndUpdateRefs(merged, source, key => key.Split('.', '+').Last());
}

string mergedJson = merged.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json)
    .Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.3\"");
await File.WriteAllTextAsync(Path.Combine(serviceSchemaDirectory, "EarthMagneticFieldMergedModel.json"), mergedJson);

NSwag.OpenApiDocument nswagDocument = await NSwag.OpenApiDocument.FromJsonAsync(mergedJson);
var settings = new CSharpClientGeneratorSettings
{
    CSharpGeneratorSettings =
    {
        Namespace = "OSDC.Drilling.EarthMagneticField.ModelShared",
        TypeNameGenerator = new ShortTypeNameGenerator(),
        JsonLibrary = CSharpJsonLibrary.SystemTextJson
    },
    GenerateClientClasses = true,
    GenerateDtoTypes = true,
    GenerateOptionalParameters = true
};
string generatedCode = new CSharpClientGenerator(nswagDocument, settings).GenerateFile();
await File.WriteAllTextAsync(Path.Combine(sharedDirectory, "EarthMagneticFieldMergedModel.cs"), generatedCode);
Console.WriteLine("Generated ModelSharedOut/EarthMagneticFieldMergedModel.cs and Service/wwwroot/json-schema/EarthMagneticFieldMergedModel.json.");

internal sealed class ShortTypeNameGenerator : ITypeNameGenerator
{
    public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames) =>
        typeNameHint?.Split('.', '+').Last() ?? "Anonymous";
}
