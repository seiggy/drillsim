using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NJsonSchema;
using NSwag.CodeGeneration.CSharp;
using OSDC.UnitConversion.ModelShared;
using System.Text;

/// <summary>
/// This console program aims at automatically generating the external C# classes USED BY A CLIENT of the current microservice (e.g. WebApp/ or ServiceTest/)
///     - C# classes are stored in generated file named CSHARP_MODEL
///     - CSHARP_MODEL is generated automatically (NSwag.CodeGeneration.CSharp library) based on JSON_BUNDLE
///     - JSON_BUNDLE is generated automatically by parsing each OpenAPI dependency of the current Model that are stored in json-schemas/VERSION/
///     - the content of folder json-schemas/VERSION/ is parsed automatically
///         - it should contain at least the OpenAPI schema generated on build by the current Service/ (see Service.csproj::Target named "CreateSwaggerJson")
///         _ as a consequence, the Service/ project SHOULD BE BUILT BEFORE THE CURRENT PROGRAM IS RUN
///         - as an illustration, an additional dependency (NORCE Drilling microservice named Field) is given by default and could be removed if not useful
/// It implements the concept of distributed shared model in a microservice architecture
///     - which means that a microservice handles the external classes it needs by itself, using the OpenAPI schema of its dependencies as a source of truth
///     - default option in this program expects the user to manually collect these dependency schemas (typically found here:https://someServer/someMicroserviceDependency/api/swagger/v1/swagger.json)
///     - option 2 discovers these dependencies online each time the current program executes, the risk being that modifications brought to the dependencies by another team go unaware
///     - more info: https://github.com/NORCE-DrillingAndWells/DrillingAndWells/wiki/MS-Development#distributed-shared-data-model
/// </summary>
class Program
{
    private static bool finished_ = false;
    private static readonly object lock_ = new object();

    private static readonly string VERSION = "3.2.1"; // must match the one in ModelShared/Program.cs and Service.csproj
    private static readonly string JSON_BUNDLE = "BundleFromOpenApi.json";
    private static readonly string CSHARP_MODEL = "ModelSharedFromOpenApi.cs";
    private static readonly string NAMESPACE = "OSDC.UnitConversion.ModelShared";

    // option 2 variables
    // As an illustration, it is assumed that Model depends on Field
    //private static readonly List<string> MS_DEPENDENCIES = new List<string> { "Field" };
    //private static readonly string HOST = "https://dev.digiwells.no/";
    //private static readonly string ENDPOINT = "swagger/v1/swagger.json";

    static async Task Main()
    {
        bool finished = false;
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.Black;
        do
        {
            Thread.Sleep(100);

            if (await GenerateModelFromDependencies())
                Console.BackgroundColor = ConsoleColor.Red;

            lock (lock_)
            {
                finished = finished_;
            }
        } while (!finished);

        return;
    }


    static async Task<bool> GenerateModelFromDependencies()
    {
        PrettyPrint("ModelShared (Service)", "Starting generation process (OpenApi document bundle and shared C# model)...");

        bool error = true;
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        if (directory != null)
        {
            string modelSharedDir = directory.FullName + "\\ModelShared";
            string jsonDirectory = modelSharedDir + "\\json-schemas\\" + "\\" + VERSION;
            string? res = "Y";
            if (File.Exists(modelSharedDir + "\\" + JSON_BUNDLE) || File.Exists(modelSharedDir + "\\" + CSHARP_MODEL))
            {
                PrettyPrint("ModelShared (Service)", $"Shared model files already exist!\n" +
                            "\tAre you sure you want to overwrite the following existing files?\n" +
                            $"\t\t- C# model (.\\ModelShared\\{CSHARP_MODEL})\n" +
                            $"\t\t- OpenApi bundle (.\\ModelShared\\{JSON_BUNDLE})\n" +
                            //// option 2: activate the following line in case of online dependency discovery
                            //$"\t\t- backups of the individual OpenApi documents (.\\json-schemas\\{VERSION}\\microserviceDependency.json)\n" +
                            "\tType Y for YES, or any other key for NO");
                res = Console.ReadLine();
            }
            if (res == "Y")
            {
                try
                {
                    if (Directory.Exists(jsonDirectory))
                    {
                        //// Option 2: Online discovery of service dependencies - and local storage
                        //foreach (string msi in MS_DEPENDENCIES)
                        //{
                        //    PrettyPrint(msi, "Processing Open Api doc into bundle...");
                        //    HttpClient httpClient = new HttpClient { BaseAddress = new Uri(HOST + msi + "/api/") };
                        //    using var stream = await httpClient.GetStreamAsync(ENDPOINT);
                        //    var doc = new OpenApiStreamReader().Read(stream, out var diagnostic);
                        //    var oString = doc.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
                        //    // forcing backup of swagger.json in json-schemas/VERSION/
                        //    PrettyPrint(msi, "Backup OpenApi doc (" + jsonDirectory + ")");
                        //    using (StreamWriter outputFile = new StreamWriter(jsonDirectory + "\\" + msi + ".json"))
                        //    {
                        //        outputFile.Write(oString);
                        //    }
                        //}
                        //Thread.Sleep(1000); // make sure files are written
                        // a bundle OpenApi schema (json format) is created to combine OpenApi schema dependencies (Microsoft.OpenApi.Models.OpenApiDocument is used)                       
                        OpenApiDocument document = new OpenApiDocument
                        {
                            Info = new OpenApiInfo
                            {
                                Title = "ModelShared (Service)",
                                Description = "Data model containing model dependencies of the clients of the current microservice (WebApp, ServiceTest)",
                                Version = VERSION
                            },
                            Components = new OpenApiComponents
                            {
                                Schemas = new Dictionary<string, OpenApiSchema> { }
                            },
                            Paths = new OpenApiPaths()
                        };

                        // Reading locally stored dependencies
                        IEnumerable<string> files = Directory.EnumerateFiles(jsonDirectory, "*.json");
                        foreach (string file in files)
                        {
                            PrettyPrint(file, "Processing Open Api doc into API client...");
                            var stream = File.OpenRead(file);
                            var doc = new OpenApiStreamReader().Read(stream, out var diagnostic);
                            foreach (var p in doc.Paths)
                            {
                                document.Paths.TryAdd(p.Key, p.Value);
                            }
                            foreach (var s in doc.Components.Schemas)
                            {
                                document.Components.Schemas.TryAdd(s.Key, s.Value);
                            }
                        }
                        var outputString = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);

                        // storing json schema bundle (just for verification)
                        using (StreamWriter writer = new StreamWriter(modelSharedDir + "\\" + JSON_BUNDLE))
                        {
                            writer.WriteLine(outputString);
                        }

                        PrettyPrint("ModelShared (Service)", "OpenApi document has been generated successfully!");

                        // C# code generation through NSwag library (NSwag.OpenApiDocument is used)
                        NSwag.OpenApiDocument nswDocument = await NSwag.OpenApiDocument.FromJsonAsync(outputString);

                        var settings = new CSharpClientGeneratorSettings
                        {
                            CSharpGeneratorSettings =
                            {
                                Namespace = NAMESPACE,
                                TypeNameGenerator = new CustomTypeNameGenerator(),
                                JsonLibrary = NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.SystemTextJson
                            }
                        };
                        var generator = new CSharpClientGenerator(nswDocument, settings);
                        var code = generator.GenerateFile();
                        using (StreamWriter writer = new StreamWriter(modelSharedDir + "\\" + CSHARP_MODEL))
                        {
                            writer.WriteLine(code);
                        }
                        error = false;
                        PrettyPrint("ModelShared (Service)", "C# client and base classes have been generated successfully!");
                    }
                    else
                    {
                        PrintError($"Folder {jsonDirectory} where shared model json schemas dependencies should be stored does not exist. Create it first!");
                    }
                }
                catch (HttpRequestException ex)
                {
                    PrintError("Problem with HttpRequest sent to a microservice", ex.Message);
                }
                catch (Exception ex)
                {
                    PrintError("Problem occurred", ex.Message);
                }
            }
            else
            {
                PrintError("The shared model files (Service) will not be overriden:\n" +
                            modelSharedDir + "\\" + CSHARP_MODEL + "\n" +
                            modelSharedDir + "\\" + JSON_BUNDLE);
            }
        }
        else
        {
            PrintError("Unable to locate current folder");
        }

        lock (lock_)
        {
            finished_ = true;
        }

        return error;
    }

    static void PrettyPrint(string header, string message, string exception = "")
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("> " + header).
           Append("\n\t" + message);
        if (exception != "")
            sb.Append("\n\t" + exception);
        Console.WriteLine(sb);
    }

    static void PrintError(string message, string exception = "")
    {
        Console.BackgroundColor = ConsoleColor.Red;
        StringBuilder sb = new StringBuilder();
        sb.Append('\n', 1).
           Append("\n" + message + "\n").
           Append("\t" + exception + "\n").
           Append('\n', 1);
        Console.WriteLine(sb);
    }

    public static HttpClient SetHttpClient(string host, string hostBasePath)
    {
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri(host + hostBasePath)
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }

    /// <summary>
    /// This custom type generator is designed to format type names: getting rid of namespaces; and getting rid of '+' signs (associated with enums type names)
    /// Note that '+' signs associated with enums should have been filtered out at this stage (Startup.cs::AddSwaggerGen() service settings are tuned to achieve this at the swagger.json stage)
    /// </summary>
    public class CustomTypeNameGenerator : ITypeNameGenerator
    {
        public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            return typeNameHint != null ? typeNameHint.Split('.', '+').Last() : String.Empty;
        }
    }

}