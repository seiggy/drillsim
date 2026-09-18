using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Microsoft.Extensions.Configuration;

namespace DrillSim.AppHost.Tests;

[TestFixture]
public sealed class FoundryResourceTests
{
    [Test]
    public void ManagedFoundry_DeclaresAccountModelAndSubscriptionOutput()
    {
        var builder = CreateBuilder();
        var api = AddApi(builder);
        var model = builder.AddManagedFoundry(api);
        string bicep = model.Resource.Parent.GetBicepTemplateString();
        var assignment = api.Resource.Annotations.OfType<RoleAssignmentAnnotation>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(bicep, Does.Contain("Microsoft.CognitiveServices/accounts"));
            Assert.That(bicep, Does.Contain("AIServices").And.Contain("disableLocalAuth: true"));
            Assert.That(bicep, Does.Contain("gpt-6-astra").And.Contain("2026-09-03"));
            Assert.That(bicep, Does.Contain("GlobalStandard").And.Contain("capacity: 100"));
            Assert.That(bicep, Does.Contain("output subscriptionId string = subscription().subscriptionId"));
            Assert.That(bicep, Does.Not.Contain("listKeys").And.Not.Contain("accounts/projects"));
            Assert.That(bicep, Does.Not.Contain("capabilityHosts"));
            Assert.That(model.Resource.Parent.GetBicepTemplateString(), Is.EqualTo(bicep));
            Assert.That(model.Resource.DeploymentName, Is.EqualTo("drillsim-chat"));
            Assert.That(assignment.Target, Is.SameAs(model.Resource.Parent));
            Assert.That(assignment.Roles.Single().Id, Is.EqualTo("5e0bd9bd-7b93-4f28-af87-19fc36ad61bd"));
        });
    }

    [Test]
    public async Task ModelOutputsSupplyTheExistingApiEnvironmentContract()
    {
        var builder = CreateBuilder();
        var api = AddApi(builder);
        var model = builder.AddManagedFoundry(api);
        model.Resource.Parent.Outputs["aiFoundryApiEndpoint"] = "https://fixture.services.ai.azure.com/";
        model.Resource.Parent.Outputs["endpoint"] = "https://fixture.cognitiveservices.azure.com/";
        model.Resource.Parent.Outputs["subscriptionId"] = "subscription-fixture";
        var values = await ReadEnvironmentAsync(builder, api.Resource);
        Assert.Multiple(() =>
        {
            Assert.That(values["AZURE_OPENAI_ENDPOINT"], Is.EqualTo("https://fixture.services.ai.azure.com/openai/v1/"));
            Assert.That(values["AZURE_OPENAI_DEPLOYMENT_NAME"], Is.EqualTo("drillsim-chat"));
            Assert.That(values["AZURE_OPENAI_SUBSCRIPTION_ID"], Is.EqualTo("subscription-fixture"));
            Assert.That(values.Keys, Does.Not.Contain("AZURE_OPENAI_API_KEY"));
        });
    }

    [Test]
    public void ExplicitModelSettingsAreAppliedWithoutChangingSku()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>
        {
            ["AzureResources:ModelName"] = "fixture-model",
            ["AzureResources:ModelVersion"] = "fixture-version",
            ["AzureResources:ModelCapacity"] = "25"
        });
        var model = builder.AddManagedFoundry(AddApi(builder));
        Assert.Multiple(() =>
        {
            Assert.That(model.Resource.ModelName, Is.EqualTo("fixture-model"));
            Assert.That(model.Resource.ModelVersion, Is.EqualTo("fixture-version"));
            Assert.That(model.Resource.SkuCapacity, Is.EqualTo(25));
            Assert.That(model.Resource.SkuName, Is.EqualTo("GlobalStandard"));
        });
    }

    [TestCase("AzureResources:ModelName", "")]
    [TestCase("AzureResources:ModelVersion", "")]
    [TestCase("AzureResources:ModelCapacity", "0")]
    [TestCase("AzureResources:ModelName", "different-model-without-version")]
    public void InvalidModelConfigurationDoesNotDeclareCloudResources(string key, string value)
    {
        var builder = CreateBuilder(new Dictionary<string, string?> { [key] = value });
        var api = AddApi(builder);
        Assert.Throws<InvalidOperationException>(() => builder.AddManagedFoundry(api));
        Assert.That(builder.Resources.OfType<AzureBicepResource>(), Is.Empty);
    }

    internal static IDistributedApplicationBuilder CreateBuilder(
        IEnumerable<KeyValuePair<string, string?>>? configuration = null)
    {
        var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            AssemblyName = typeof(FoundryResourceTests).Assembly.GetName().Name,
            ProjectDirectory = TestContext.CurrentContext.TestDirectory,
            DisableDashboard = true,
            Args = ["--environment", "Development"]
        });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(configuration ?? []);
        return builder;
    }

    internal static IResourceBuilder<ProjectResource> AddApi(IDistributedApplicationBuilder builder) =>
        builder.AddResource(new ProjectResource("analysis-api"));

    internal static async Task<Dictionary<string, string?>> ReadEnvironmentAsync(
        IDistributedApplicationBuilder builder, IResource resource)
    {
        var context = new EnvironmentCallbackContext(builder.ExecutionContext, resource,
            new Dictionary<string, object>(), CancellationToken.None);
        foreach (var annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>())
            await annotation.Callback(context);
        var values = new Dictionary<string, string?>();
        foreach ((string name, object value) in context.EnvironmentVariables)
            values[name] = value is IValueProvider provider
                ? await provider.GetValueAsync(CancellationToken.None) : value.ToString();
        return values;
    }
}
