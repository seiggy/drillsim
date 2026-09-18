using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Foundry;
using Azure.Provisioning;
using Azure.Provisioning.CognitiveServices;
using Azure.Provisioning.Expressions;
using Microsoft.Extensions.Configuration;

namespace DrillSim.AppHost;

internal static class FoundryResources
{
    internal const string DefaultModel = "gpt-6-astra";
    internal const string DefaultModelVersion = "2026-09-03";
    internal const int DefaultCapacity = 100;

    internal static IResourceBuilder<FoundryDeploymentResource> AddManagedFoundry(
        this IDistributedApplicationBuilder builder, IResourceBuilder<ProjectResource> api)
    {
        string model = builder.Configuration["AzureResources:ModelName"] ?? DefaultModel;
        string version = builder.Configuration["AzureResources:ModelVersion"] ?? DefaultModelVersion;
        int capacity = builder.Configuration.GetValue<int?>("AzureResources:ModelCapacity") ?? DefaultCapacity;
        if (string.IsNullOrWhiteSpace(model) || model.Length > 128 || model.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(version) || version.Length > 64 || version.Any(char.IsControl) || capacity < 1)
            throw new InvalidOperationException("AzureResources requires a model name, model version, and positive deployment capacity.");
        if (model != DefaultModel && string.IsNullOrWhiteSpace(builder.Configuration["AzureResources:ModelVersion"]))
            throw new InvalidOperationException("Set AzureResources:ModelVersion when choosing a different model.");

        var foundry = builder.AddFoundry("foundry")
            .ClearDefaultRoleAssignments()
            .ConfigureInfrastructure(infrastructure =>
            {
                // Agent Framework runs locally; Foundry only hosts the model.
                foreach (var host in infrastructure.GetProvisionableResources().OfType<CognitiveServicesCapabilityHost>().ToArray())
                    infrastructure.Remove(host);
                infrastructure.Add(new ProvisioningOutput("subscriptionId", typeof(string))
                {
                    Value = BicepFunction.GetSubscription().SubscriptionId
                });
            });
        var deployment = foundry.AddDeployment("chat-model", new FoundryModel
        {
            Name = model,
            Version = version,
            Format = "OpenAI"
        }).WithProperties(resource =>
        {
            resource.DeploymentName = "drillsim-chat";
            resource.SkuName = "GlobalStandard";
            resource.SkuCapacity = capacity;
        });

        api.WithReference(deployment)
            .WithRoleAssignments(foundry, CognitiveServicesBuiltInRole.CognitiveServicesOpenAIUser)
            .WithEnvironment("AZURE_OPENAI_ENDPOINT",
                ReferenceExpression.Create($"{foundry.Resource.AIFoundryApiEndpoint}openai/v1/"))
            .WithEnvironment("AZURE_OPENAI_DEPLOYMENT_NAME", deployment.Resource.DeploymentName)
            .WithEnvironment("AZURE_OPENAI_SUBSCRIPTION_ID", foundry.GetOutput("subscriptionId"))
            .WaitFor(deployment);
        return deployment;
    }
}
