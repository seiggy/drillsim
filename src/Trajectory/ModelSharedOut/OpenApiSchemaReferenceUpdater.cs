using Microsoft.OpenApi;
using System.Text.Json.Nodes;

public class OpenApiSchemaReferenceUpdater
{
    private readonly Dictionary<string, string> _renamedSchemas = new();

    public void MergeSchemasAndUpdateRefs(OpenApiDocument target, OpenApiDocument source, Func<string, string> keyTransformer)
    {
        foreach (var (oldKey, schema) in source.Components.Schemas)
        {
            string newKey = keyTransformer(oldKey);
            if (oldKey != newKey)
                _renamedSchemas[oldKey] = newKey;

            IOpenApiSchema clonedSchema = CloneSchema(schema, target);
            if (!target.Components.Schemas.TryGetValue(newKey, out IOpenApiSchema? existingSchema) ||
                CountDeclaredProperties(clonedSchema) > CountDeclaredProperties(existingSchema))
            {
                target.Components.Schemas[newKey] = clonedSchema;
            }
        }

        UpdateAllReferences(target);
    }

    private void UpdateAllReferences(OpenApiDocument document)
    {
        foreach (var path in document.Paths.Values.OfType<OpenApiPathItem>())
        {
            foreach (var operation in path.Operations.Values)
            {
                foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
                {
                    if (parameter.Schema is not null)
                        parameter.Schema = UpdateSchemaRef(parameter.Schema, document);
                }

                if (operation.RequestBody is OpenApiRequestBody requestBody)
                    UpdateContent(requestBody.Content, document);

                foreach (var response in operation.Responses.Values.OfType<OpenApiResponse>())
                    UpdateContent(response.Content, document);
            }
        }

        foreach (var parameter in document.Components.Parameters.Values.OfType<OpenApiParameter>())
        {
            if (parameter.Schema is not null)
                parameter.Schema = UpdateSchemaRef(parameter.Schema, document);
        }

        foreach (var requestBody in document.Components.RequestBodies.Values.OfType<OpenApiRequestBody>())
            UpdateContent(requestBody.Content, document);
        foreach (var response in document.Components.Responses.Values.OfType<OpenApiResponse>())
            UpdateContent(response.Content, document);

        foreach (string name in document.Components.Schemas.Keys.ToArray())
            document.Components.Schemas[name] = UpdateSchemaRef(document.Components.Schemas[name], document);
    }

    private void UpdateContent(IDictionary<string, OpenApiMediaType> content, OpenApiDocument document)
    {
        foreach (var mediaType in content.Values.OfType<OpenApiMediaType>())
            mediaType.Schema = UpdateSchemaRef(mediaType.Schema, document);
    }

    private IOpenApiSchema UpdateSchemaRef(IOpenApiSchema schema, OpenApiDocument document)
    {
        if (schema is OpenApiSchemaReference reference)
        {
            string id = _renamedSchemas.GetValueOrDefault(reference.Reference.Id, reference.Reference.Id);
            return id == reference.Reference.Id
                ? schema
                : new OpenApiSchemaReference(id, document, reference.Reference.ExternalResource);
        }

        var mutableSchema = (OpenApiSchema)schema;
        foreach (string name in mutableSchema.Properties.Keys.ToArray())
            mutableSchema.Properties[name] = UpdateSchemaRef(mutableSchema.Properties[name], document);

        if (mutableSchema.Items is not null)
            mutableSchema.Items = UpdateSchemaRef(mutableSchema.Items, document);
        if (mutableSchema.AdditionalProperties is not null)
            mutableSchema.AdditionalProperties = UpdateSchemaRef(mutableSchema.AdditionalProperties, document);

        for (int index = 0; index < mutableSchema.AllOf.Count; index++)
            mutableSchema.AllOf[index] = UpdateSchemaRef(mutableSchema.AllOf[index], document);
        for (int index = 0; index < mutableSchema.AnyOf.Count; index++)
            mutableSchema.AnyOf[index] = UpdateSchemaRef(mutableSchema.AnyOf[index], document);
        for (int index = 0; index < mutableSchema.OneOf.Count; index++)
            mutableSchema.OneOf[index] = UpdateSchemaRef(mutableSchema.OneOf[index], document);

        return mutableSchema;
    }

    private static int CountDeclaredProperties(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchemaReference)
            return 0;

        return schema.Properties.Count
            + schema.AllOf.Sum(CountDeclaredProperties)
            + schema.AnyOf.Sum(CountDeclaredProperties)
            + schema.OneOf.Sum(CountDeclaredProperties);
    }

    private IOpenApiSchema CloneSchema(IOpenApiSchema source, OpenApiDocument target)
    {
        if (source is OpenApiSchemaReference reference)
        {
            string id = _renamedSchemas.GetValueOrDefault(reference.Reference.Id, reference.Reference.Id);
            return new OpenApiSchemaReference(id, target, reference.Reference.ExternalResource);
        }

        var clone = new OpenApiSchema
        {
            Title = source.Title,
            Type = source.Type,
            Format = source.Format,
            Description = source.Description,
            Deprecated = source.Deprecated,
            ReadOnly = source.ReadOnly,
            WriteOnly = source.WriteOnly,
            ExternalDocs = source.ExternalDocs,
            Example = source.Example,
            Default = source.Default,
            Enum = new List<JsonNode>(source.Enum),
            Required = new HashSet<string>(source.Required),
            Discriminator = source.Discriminator,
            MaxItems = source.MaxItems,
            MinItems = source.MinItems,
            MaxLength = source.MaxLength,
            MinLength = source.MinLength,
            Maximum = source.Maximum,
            Minimum = source.Minimum,
            Pattern = source.Pattern,
            UniqueItems = source.UniqueItems,
            MultipleOf = source.MultipleOf,
            Extensions = new Dictionary<string, IOpenApiExtension>(source.Extensions)
        };

        foreach (var (name, property) in source.Properties)
            clone.Properties.Add(name, CloneSchema(property, target));

        if (source.Items is not null)
            clone.Items = CloneSchema(source.Items, target);
        if (source.AdditionalProperties is not null)
            clone.AdditionalProperties = CloneSchema(source.AdditionalProperties, target);

        clone.AllOf = source.AllOf.Select(schema => CloneSchema(schema, target)).ToList();
        clone.AnyOf = source.AnyOf.Select(schema => CloneSchema(schema, target)).ToList();
        clone.OneOf = source.OneOf.Select(schema => CloneSchema(schema, target)).ToList();

        return clone;
    }
}
