using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Microsoft.Extensions.Configuration;

namespace DrillSim.AppHost;

internal static class AzureResources
{
    internal const int WebPort = 5173;
    internal static string BrowserOrigin => $"http://localhost:{WebPort}";

    private static readonly (string Parameter, string Environment)[] OpenAiSettings =
    [
        ("azure-openai-endpoint", "AZURE_OPENAI_ENDPOINT"),
        ("azure-openai-deployment-name", "AZURE_OPENAI_DEPLOYMENT_NAME"),
        ("azure-openai-subscription-id", "AZURE_OPENAI_SUBSCRIPTION_ID")
    ];
    private static readonly (string Parameter, string Environment)[] MapsSettings =
    [
        ("azure-maps-client-id", "AZURE_MAPS_CLIENT_ID"),
        ("azure-maps-tenant-id", "AZURE_MAPS_TENANT_ID"),
        ("azure-maps-subscription-id", "AZURE_MAPS_SUBSCRIPTION_ID")
    ];

    internal static void ConfigureAzureResources(
        this IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> api)
    {
        bool existingOpenAi = HasExistingSettings(builder.Configuration, OpenAiSettings);
        bool existingMaps = HasExistingSettings(builder.Configuration, MapsSettings);
        bool provision = builder.Configuration.GetValue("AzureResources:Provision", true);
        if (provision && (!existingOpenAi || !existingMaps))
        {
            string? credential = builder.Configuration["Azure:CredentialSource"];
            if (!string.IsNullOrWhiteSpace(credential) &&
                !string.Equals(credential.Trim(), "AzureCli", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Local Azure provisioning must use Azure:CredentialSource=AzureCli to match the Analysis API identity.");
            builder.Configuration["Azure:CredentialSource"] = "AzureCli";
            builder.Configuration["Azure:Location"] ??= "eastus2";
            builder.Configuration["Azure:ResourceGroupPrefix"] ??= "rg-drillsim";
        }

        if (existingOpenAi || !provision)
            UseConfiguredSettings(builder, api, OpenAiSettings);
        else
            builder.AddManagedFoundry(api);

        if (existingMaps || !provision)
            UseConfiguredSettings(builder, api, MapsSettings);
        else
        {
            var maps = builder.AddBicepTemplate("maps", Path.Combine("Infrastructure", "azure-maps.bicep"))
                .WithParameter("browserOrigin", BrowserOrigin)
                .WithParameter(AzureBicepResource.KnownParameters.PrincipalId)
                .WithParameter(AzureBicepResource.KnownParameters.PrincipalType);
            api.WithEnvironment("AZURE_MAPS_CLIENT_ID", maps.GetOutput("clientId"))
                .WithEnvironment("AZURE_MAPS_TENANT_ID", maps.GetOutput("tenantId"))
                .WithEnvironment("AZURE_MAPS_SUBSCRIPTION_ID", maps.GetOutput("subscriptionId"))
                .WaitFor(maps);
        }
    }

    internal static string AzureConfigDirectory(string? configured) =>
        string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".azure")
            : configured;

    private static bool HasExistingSettings(
        IConfiguration configuration, IReadOnlyList<(string Parameter, string Environment)> settings)
    {
        int configured = settings.Count(setting => !string.IsNullOrWhiteSpace(ConfiguredValue(configuration, setting.Parameter)));
        if (configured > 0 && configured != settings.Count)
            throw new InvalidOperationException(
                $"Existing Azure connections require all of these Parameters values: {string.Join(", ", settings.Select(setting => setting.Parameter))}. Complete the group or remove it to use Aspire provisioning.");
        return configured == settings.Count;
    }

    private static void UseConfiguredSettings(IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> api, IEnumerable<(string Parameter, string Environment)> settings)
    {
        foreach (var setting in settings)
            api.WithEnvironment(setting.Environment, builder.AddParameter(setting.Parameter,
                ConfiguredValue(builder.Configuration, setting.Parameter) ?? string.Empty));
    }

    private static string? ConfiguredValue(IConfiguration configuration, string name) =>
        configuration[$"Parameters:{name}"] ?? configuration[$"Parameters:{name.Replace('-', '_')}"];
}
