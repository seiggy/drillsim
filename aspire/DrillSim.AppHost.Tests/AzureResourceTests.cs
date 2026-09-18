using Aspire.Hosting;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Foundry;

namespace DrillSim.AppHost.Tests;

[TestFixture]
public sealed class AzureResourceTests
{
    private static Dictionary<string, string?> ExistingSettings => new()
    {
        ["Parameters:azure-openai-endpoint"] = "https://existing.openai.azure.com/openai/v1/",
        ["Parameters:azure-openai-deployment-name"] = "existing-model",
        ["Parameters:azure-openai-subscription-id"] = "existing-ai-subscription",
        ["Parameters:azure-maps-client-id"] = "existing-maps-client",
        ["Parameters:azure-maps-tenant-id"] = "existing-maps-tenant",
        ["Parameters:azure-maps-subscription-id"] = "existing-maps-subscription"
    };

    [Test]
    public async Task NewSetup_DeclaresManagedResourcesAndUsesTheirOutputs()
    {
        var builder = FoundryResourceTests.CreateBuilder();
        var api = FoundryResourceTests.AddApi(builder);
        builder.ConfigureAzureResources(api);
        var foundry = builder.Resources.OfType<FoundryResource>().Single();
        AzureBicepResource maps = builder.Resources.OfType<AzureBicepResource>().Single(resource => resource.Name == "maps");
        string mapsTemplate = maps.GetBicepTemplateString();
        foundry.Outputs["aiFoundryApiEndpoint"] = "https://fixture.services.ai.azure.com/";
        foundry.Outputs["endpoint"] = "https://fixture.cognitiveservices.azure.com/";
        foundry.Outputs["subscriptionId"] = "ai-subscription";
        maps.Outputs["clientId"] = "maps-client";
        maps.Outputs["tenantId"] = "maps-tenant";
        maps.Outputs["subscriptionId"] = "maps-subscription";
        var environment = await FoundryResourceTests.ReadEnvironmentAsync(builder, api.Resource);
        Assert.Multiple(() =>
        {
            Assert.That(builder.Configuration["Azure:CredentialSource"], Is.EqualTo("AzureCli"));
            Assert.That(builder.Configuration["Azure:Location"], Is.EqualTo("eastus2"));
            Assert.That(maps.Parameters["browserOrigin"], Is.EqualTo(AzureResources.BrowserOrigin));
            Assert.That(new Uri(AzureResources.BrowserOrigin).Port, Is.EqualTo(AzureResources.WebPort));
            Assert.That(maps.Parameters.ContainsKey(AzureBicepResource.KnownParameters.PrincipalId), Is.True);
            Assert.That(maps.Parameters[AzureBicepResource.KnownParameters.PrincipalId], Is.Null);
            Assert.That(maps.Parameters[AzureBicepResource.KnownParameters.PrincipalType], Is.Null);
            Assert.That(mapsTemplate, Does.Contain("423170ca-a8f6-4b0f-8487-9e4eb8f49bfa").And.Contain("disableLocalAuth: true"));
            Assert.That(mapsTemplate, Does.Not.Contain("listKeys"));
            Assert.That(environment["AZURE_MAPS_CLIENT_ID"], Is.EqualTo("maps-client"));
            Assert.That(environment["AZURE_MAPS_TENANT_ID"], Is.EqualTo("maps-tenant"));
            Assert.That(environment["AZURE_MAPS_SUBSCRIPTION_ID"], Is.EqualTo("maps-subscription"));
            Assert.That(environment["AZURE_OPENAI_ENDPOINT"], Is.EqualTo("https://fixture.services.ai.azure.com/openai/v1/"));
            Assert.That(environment["AZURE_OPENAI_SUBSCRIPTION_ID"], Is.EqualTo("ai-subscription"));
            Assert.That(environment.Keys.Any(key => key.EndsWith("_KEY", StringComparison.Ordinal)), Is.False);
        });
    }

    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(true, false)]
    public async Task ExistingConnectionsAreReusedWithoutDeclaringAzureResources(bool normalizedEnvironmentKeys, bool provision)
    {
        var settings = ExistingSettings.ToDictionary(
            pair => normalizedEnvironmentKeys ? pair.Key.Replace('-', '_') : pair.Key, pair => pair.Value);
        settings["AzureResources:Provision"] = provision.ToString();
        var builder = FoundryResourceTests.CreateBuilder(settings);
        var api = FoundryResourceTests.AddApi(builder);
        builder.ConfigureAzureResources(api);
        var values = await FoundryResourceTests.ReadEnvironmentAsync(builder, api.Resource);
        Assert.Multiple(() =>
        {
            Assert.That(builder.Resources.OfType<AzureBicepResource>(), Is.Empty);
            Assert.That(values["AZURE_OPENAI_DEPLOYMENT_NAME"], Is.EqualTo("existing-model"));
            Assert.That(values["AZURE_OPENAI_ENDPOINT"], Is.EqualTo("https://existing.openai.azure.com/openai/v1/"));
            Assert.That(values["AZURE_MAPS_CLIENT_ID"], Is.EqualTo("existing-maps-client"));
            Assert.That(builder.Configuration["Azure:CredentialSource"], Is.Null);
        });
    }

    [TestCase("azure-openai", "maps")]
    [TestCase("azure-maps", "foundry")]
    public void OnlyMissingConnectionsAreProvisioned(string existingPrefix, string expectedResource)
    {
        var builder = FoundryResourceTests.CreateBuilder(ExistingSettings.Where(
            pair => pair.Key.StartsWith($"Parameters:{existingPrefix}", StringComparison.Ordinal)));
        builder.ConfigureAzureResources(FoundryResourceTests.AddApi(builder));
        Assert.That(builder.Resources.OfType<AzureBicepResource>().Select(resource => resource.Name),
            Is.EqualTo(new[] { expectedResource }));
    }

    [Test]
    public async Task OfflineModeDoesNotDeclareOrContactAzureResources()
    {
        var builder = FoundryResourceTests.CreateBuilder(new Dictionary<string, string?>
        {
            ["AzureResources:Provision"] = "false"
        });
        var api = FoundryResourceTests.AddApi(builder);
        builder.ConfigureAzureResources(api);
        var values = await FoundryResourceTests.ReadEnvironmentAsync(builder, api.Resource);
        Assert.That(builder.Resources.OfType<AzureBicepResource>(), Is.Empty);
        Assert.That(values.Values.All(string.IsNullOrEmpty), Is.True);
    }

    [TestCase("azure-openai-endpoint")]
    [TestCase("azure-maps-client-id")]
    public void PartialExistingConfigurationDoesNotCreateUnexpectedResources(string setting)
    {
        var builder = FoundryResourceTests.CreateBuilder(new Dictionary<string, string?> { [$"Parameters:{setting}"] = "configured" });
        Assert.Throws<InvalidOperationException>(() => builder.ConfigureAzureResources(FoundryResourceTests.AddApi(builder)));
        Assert.That(builder.Resources.OfType<AzureBicepResource>(), Is.Empty);
    }

    [TestCase("AzureCli")]
    [TestCase("azurecli")]
    public void ExplicitLocationIsPreservedAndConflictingCredentialSourceIsRejected(string credentialSource)
    {
        var builder = FoundryResourceTests.CreateBuilder(new Dictionary<string, string?>
        {
            ["Azure:Location"] = "swedencentral",
            ["Azure:CredentialSource"] = credentialSource
        });
        builder.ConfigureAzureResources(FoundryResourceTests.AddApi(builder));
        Assert.That(builder.Configuration["Azure:Location"], Is.EqualTo("swedencentral"));
        var conflicting = FoundryResourceTests.CreateBuilder(new Dictionary<string, string?>
        {
            ["Azure:CredentialSource"] = "VisualStudio"
        });
        Assert.Throws<InvalidOperationException>(() => conflicting.ConfigureAzureResources(FoundryResourceTests.AddApi(conflicting)));
        Assert.That(conflicting.Resources.OfType<AzureBicepResource>(), Is.Empty);
    }

    [Test]
    public void AzureCliDirectoryPreservesSelectedProfileOrUsesCliDefault()
    {
        string selected = Path.Combine(Path.GetTempPath(), "chosen-azure-profile");
        Assert.That(AzureResources.AzureConfigDirectory(selected), Is.EqualTo(selected));
        Assert.That(AzureResources.AzureConfigDirectory(null),
            Is.EqualTo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azure")));
    }
}
