using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OSDC.UnitConversion.Service.NSwag
{
    public class PolymorphismDocumentFilter<T> : IDocumentFilter
    {
        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            //RegisterSubClasses(context, typeof(T));   
            RegisterSubClassesFullName(context, typeof(T));
        }

        private static void RegisterSubClasses(DocumentFilterContext context, Type abstractType)
        {
            try
            {
                var schemaRepository = context.SchemaRepository.Schemas;

                //Console.WriteLine("\nBEFORE BaseType registration");
                //ShowSchema(schemaRepository, "");

                var schemaGenerator = context.SchemaGenerator;
                OpenApiSchema? parentSchema;
                if (!schemaRepository.TryGetValue(abstractType.Name, out parentSchema))
                {
                    parentSchema = schemaGenerator.GenerateSchema(abstractType, context.SchemaRepository);
                }

                // set up a discriminator property (it must be required)
                parentSchema.Discriminator = new OpenApiDiscriminator { PropertyName = Utils.DISCRIMINATOR_NAME };
                parentSchema.Required.Add(Utils.DISCRIMINATOR_NAME);

                if (!parentSchema.Properties.ContainsKey(Utils.DISCRIMINATOR_NAME))
                    parentSchema.Properties.Add(Utils.DISCRIMINATOR_NAME, new OpenApiSchema { Type = "string", Default = new OpenApiString(abstractType.Name) });

                // register all subclasses
                var derivedTypes = abstractType.GetTypeInfo().Assembly.GetTypes()
                    .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

                //Console.WriteLine("\nBEFORE derivedTypes registration");
                //ShowSchema(schemaRepository, "");

                //Console.WriteLine($"\nNow registering derived types {string.Join(",", derivedTypes.Select(x => x.Name))}");
                foreach (var type in derivedTypes)
                {
                    schemaGenerator.GenerateSchema(type, context.SchemaRepository);
                }

                //Console.WriteLine("\nAFTER derivedTypes registration");
                //ShowSchema(schemaRepository, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> PROBLEM while registering subclasses for polymorphism <<< \n" + ex.Message);
            }
        }

        private static void RegisterSubClassesFullName(DocumentFilterContext context, Type abstractType)
        {
            try
            {
                var schemaRepository = context.SchemaRepository.Schemas;

                //Console.WriteLine("\nBEFORE BaseType registration");
                //ShowSchema(schemaRepository, "");

                var schemaGenerator = context.SchemaGenerator;
                OpenApiSchema? parentSchema;
                if (!schemaRepository.TryGetValue(abstractType.FullName!, out parentSchema))
                //if (!schemaRepository.TryGetValue(abstractType.Name, out OpenApiSchema parentSchema))
                {
                    parentSchema = schemaGenerator.GenerateSchema(abstractType, context.SchemaRepository);
                }

                // set up a discriminator property (it must be required)
                parentSchema.Discriminator = new OpenApiDiscriminator { PropertyName = Utils.DISCRIMINATOR_NAME };
                parentSchema.Required.Add(Utils.DISCRIMINATOR_NAME);

                if (!parentSchema.Properties.ContainsKey(Utils.DISCRIMINATOR_NAME))
                    parentSchema.Properties.Add(Utils.DISCRIMINATOR_NAME, new OpenApiSchema { Type = "string", Default = new OpenApiString(abstractType.FullName) });

                // register all subclasses
                var derivedTypes = abstractType.GetTypeInfo().Assembly.GetTypes()
                    .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

                //Console.WriteLine("\nBEFORE derivedTypes registration");
                //ShowSchema(schemaRepository, "");

                //Console.WriteLine($"\nNow registering derived types {string.Join(",", derivedTypes.Select(x => x.Name))}");
                foreach (var type in derivedTypes)
                {
                    schemaGenerator.GenerateSchema(type, context.SchemaRepository);
                }

                //Console.WriteLine("\nAFTER derivedTypes registration");
                //ShowSchema(schemaRepository, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> PROBLEM while registering subclasses for polymorphism <<< \n" + ex.Message);
            }
        }

        private static void ShowSchema(IDictionary<string, OpenApiSchema> dict, string indent)
        {
            foreach (var schema in dict)
            {
                Console.WriteLine($"{indent}{schema.Key}");
                foreach (var prop in schema.Value.Properties)
                {
                    Console.WriteLine($"\t{indent}{prop.Key}");
                    if (prop.Value.Properties.Any())
                        ShowSchema(prop.Value.Properties, $"\t{indent}");
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PolymorphismSchemaFilter<T> : ISchemaFilter
    {
        private readonly Lazy<HashSet<Type>> derivedTypes = new Lazy<HashSet<Type>>(Init);

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            //ApplySchema(schema, context);
            ApplySchemaFullName(schema, context);
        }

        private void ApplySchema(OpenApiSchema schema, SchemaFilterContext context)
        {
            try
            {
                //string str = string.Join(",", schema.Properties.Select(x => x.Key));
                //if (str != "")
                //    Console.WriteLine($"\tFiltering for polymorphism - {str}");
                var type = context.Type;
                if (!derivedTypes.Value.Contains(type))
                    return;
                var clonedSchema = new OpenApiSchema
                {
                    Properties = schema.Properties,
                    Type = schema.Type,
                    Required = schema.Required
                };
                if (context.SchemaRepository.Schemas.TryGetValue(typeof(T).Name, out OpenApiSchema? _))
                {
                    schema.AllOf = new List<OpenApiSchema> {
                        new OpenApiSchema { Reference = new OpenApiReference { Id = typeof(T).Name, Type = ReferenceType.Schema } },
                        clonedSchema
                    };
                    //Console.WriteLine($"schema actually set for {str}");
                }

                var assembly = Assembly.GetAssembly(type);
                var assemblyName = new AssemblyName();
                if (assembly != null)
                    assemblyName = assembly.GetName();
                if (assemblyName != null)
                {
                    schema.Discriminator = new OpenApiDiscriminator { PropertyName = Utils.DISCRIMINATOR_NAME };
                    schema.AddExtension("x-ms-discriminator-value", new OpenApiString($"{type.Name}, {assemblyName.FullName}"));
                }

                // reset properties for they are included in allOf, should be null but code does not handle it
                schema.Properties = new Dictionary<string, OpenApiSchema>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> PROBLEM while filtering a schema for polymorphism <<< " + ex.Message);
            }
        }

        private void ApplySchemaFullName(OpenApiSchema schema, SchemaFilterContext context)
        {
            try
            {
                //string str = string.Join(",", schema.Properties.Select(x => x.Key));
                //if (str != "")
                //    Console.WriteLine($"\tFiltering for polymorphism - {str}");
                var type = context.Type;
                if (!derivedTypes.Value.Contains(type))
                    return;
                var clonedSchema = new OpenApiSchema
                {
                    Properties = schema.Properties,
                    Type = schema.Type,
                    Required = schema.Required
                };
                if (context.SchemaRepository.Schemas.TryGetValue(typeof(T).FullName!, out OpenApiSchema? _))
                {
                    schema.AllOf = new List<OpenApiSchema> {
                        new OpenApiSchema { Reference = new OpenApiReference { Id = typeof(T).FullName, Type = ReferenceType.Schema } },
                        clonedSchema
                    };
                    //Console.WriteLine($"schema actually set for {str}");
                }


                var assembly = Assembly.GetAssembly(type);
                var assemblyName = new AssemblyName();
                if (assembly != null)
                    assemblyName = assembly.GetName();
                if (assemblyName != null)
                {
                    schema.Discriminator = new OpenApiDiscriminator { PropertyName = Utils.DISCRIMINATOR_NAME };
                    schema.AddExtension("x-ms-discriminator-value", new OpenApiString($"{type.Name}, {assemblyName.FullName}"));
                }

                // reset properties for they are included in allOf, should be null but code does not handle it
                schema.Properties = new Dictionary<string, OpenApiSchema>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> PROBLEM while filtering a schema for polymorphism <<< " + ex.Message);
            }
        }

        private static HashSet<Type> Init()
        {
            var abstractType = typeof(T);
            var dTypes = abstractType.GetTypeInfo().Assembly
                .GetTypes()
                .Where(x => abstractType != x && abstractType.IsAssignableFrom(x));

            var result = new HashSet<Type>();

            foreach (var item in dTypes)
                result.Add(item);

            return result;
        }

    }
}