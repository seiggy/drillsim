using Microsoft.OpenApi;

public static class SwaggerMiddlewareExtensions
{
    public static void UseCustomSwagger(this IApplicationBuilder app, OpenApiDocument document, string relativePath)
    {
        app.Map(relativePath, branch => branch.Run(async context =>
        {
            document.Servers = [new OpenApiServer { Url = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : "/" }];
            context.Response.ContentType = "application/json";
            string json = (await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0))
                .Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.3\"");
            await context.Response.WriteAsync(json);
        }));
    }

    public static OpenApiDocument ReadOpenApiDocument(string path)
    {
        var readResult = OpenApiDocument.Parse(File.ReadAllText(path), "json");
        return readResult.Document ?? throw new InvalidOperationException($"Unable to parse OpenAPI document '{path}'.");
    }
}
