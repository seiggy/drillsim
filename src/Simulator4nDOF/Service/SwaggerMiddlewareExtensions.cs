using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using System;
using System.IO;

public static class SwaggerMiddlewareExtensions
{
    public static void UseCustomSwagger(this IApplicationBuilder app, OpenApiDocument mergedDoc, string relativePath)
    {
        app.Map(relativePath, builder =>
        {
            builder.Run(async context =>
            {
                var req = context.Request;
                var pathBase = req.PathBase.HasValue ? req.PathBase.Value : string.Empty;
                mergedDoc.Servers = [new OpenApiServer { Url = string.IsNullOrEmpty(pathBase) ? "/" : pathBase }];

                context.Response.ContentType = "application/json";
                var json = await mergedDoc.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
                await context.Response.WriteAsync(json);
            });
        });
    }

    public static OpenApiDocument ReadOpenApiDocument(string filePath)
    {
        var readResult = OpenApiDocument.Parse(File.ReadAllText(filePath), "json");
        var diagnostic = readResult.Diagnostic;
        OpenApiDocument document = readResult.Document ?? throw new InvalidOperationException($"Unable to parse OpenAPI document '{filePath}'.");

        if (diagnostic.Errors.Count > 0)
        {
            Console.WriteLine("Warnings or errors while reading OpenAPI document:");
            foreach (var error in diagnostic.Errors)
                Console.WriteLine($"- {error.Message}");
        }

        return document;
    }
}
