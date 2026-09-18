using System.Reflection;
using System.Text.Json.Nodes;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using Aspire.Hosting.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DrillSim.AppHost.Tests;

#pragma warning disable ASPIREUSERSECRETS001, ASPIREFILESYSTEM001, ASPIREPIPELINES001 // In-memory builder adapter; no live services or secret files.

[TestFixture]
public sealed class ServiceKeyParameterTests
{
    private static readonly string[] KeyNames =
    [
        "reservoir-simulation-operator-key",
        "drilling-operations-internal-key",
        "drilling-operations-analysis-callback-key",
        "publication-import-key"
    ];

    [TestCaseSource(nameof(KeyNames))]
    public async Task MissingKey_IsGeneratedOnceMarkedSecretAndPersisted(string name)
    {
        var secrets = new TestSecrets();
        var builder = new TestBuilder(secrets);
        IResourceBuilder<ParameterResource> parameter = builder.AddServiceKey(name);
        string? first = await parameter.Resource.GetValueAsync(CancellationToken.None);
        string? second = await parameter.Resource.GetValueAsync(CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(parameter.Resource.Secret, Is.True);
            Assert.That(first, Has.Length.EqualTo(64));
            Assert.That(first!.All(char.IsAsciiLetterOrDigit), Is.True);
            Assert.That(first == second, Is.True, "Every service reference must receive the same key.");
            Assert.That(secrets.Values[$"Parameters:{name}"] == first, Is.True);
            Assert.That(secrets.Writes, Is.EqualTo(1));
        });
    }

    [TestCaseSource(nameof(KeyNames))]
    public async Task ConfiguredKey_IsReusedWithoutWritingOrRotatingIt(string name)
    {
        var secrets = new TestSecrets();
        var builder = new TestBuilder(secrets, new Dictionary<string, string?>
        {
            [$"Parameters:{name}"] = "fixture-existing-service-key"
        });
        string? value = await builder.AddServiceKey(name).Resource.GetValueAsync(CancellationToken.None);
        Assert.That(value == "fixture-existing-service-key", Is.True);
        Assert.That(secrets.Writes, Is.Zero);
    }

    [Test]
    public async Task SeparateKeys_AreDistinctAndReloadedOnTheNextLaunch()
    {
        var secrets = new TestSecrets();
        var first = new TestBuilder(secrets);
        string?[] original = await Task.WhenAll(KeyNames.Select(async name =>
            await first.AddServiceKey(name).Resource.GetValueAsync(CancellationToken.None)));
        Assert.That(original.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(KeyNames.Length));
        var next = new TestBuilder(secrets, secrets.Values);
        string?[] reloaded = await Task.WhenAll(KeyNames.Select(async name =>
            await next.AddServiceKey(name).Resource.GetValueAsync(CancellationToken.None)));
        Assert.That(original.SequenceEqual(reloaded, StringComparer.Ordinal), Is.True);
        Assert.That(secrets.Writes, Is.EqualTo(KeyNames.Length));
    }

    [Test]
    public async Task EmptyConfiguration_IsTreatedAsMissingByAspire()
    {
        var secrets = new TestSecrets();
        var builder = new TestBuilder(secrets, new Dictionary<string, string?>
        {
            [$"Parameters:{KeyNames[0]}"] = string.Empty
        });
        string? generated = await builder.AddServiceKey(KeyNames[0]).Resource.GetValueAsync(CancellationToken.None);
        Assert.That(generated, Has.Length.EqualTo(64));
        Assert.That(secrets.Values[$"Parameters:{KeyNames[0]}"] == generated, Is.True);
        Assert.That(secrets.Writes, Is.EqualTo(1));
    }

    [Test]
    public void AppHost_WiresAllFourKeysThroughTheTestedHelper()
    {
        string appHost = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "AppHost.cs.txt"));
        foreach (string name in KeyNames)
            Assert.That(appHost, Does.Contain($"builder.AddServiceKey(\"{name}\")"));
    }

    private sealed class TestBuilder : IDistributedApplicationBuilder
    {
        private readonly DistributedApplicationBuilder _builder;
        private readonly TestSecrets _secrets;
        public IUserSecretsManager UserSecretsManager => _secrets;
        public Assembly? AppHostAssembly => _builder.AppHostAssembly;
        public string AppHostDirectory => _builder.AppHostDirectory;
        public ConfigurationManager Configuration => _builder.Configuration;
        public IHostEnvironment Environment => _builder.Environment;
        public IDistributedApplicationEventing Eventing => _builder.Eventing;
        public DistributedApplicationExecutionContext ExecutionContext => _builder.ExecutionContext;
        public IFileSystemService FileSystemService => _builder.FileSystemService;
        public IDistributedApplicationPipeline Pipeline => _builder.Pipeline;
        public IResourceCollection Resources => _builder.Resources;
        public IServiceCollection Services => _builder.Services;
        public IResourceBuilder<T> AddResource<T>(T resource) where T : IResource => _builder.AddResource(resource);
        public IResourceBuilder<T> CreateResourceBuilder<T>(T resource) where T : IResource => _builder.CreateResourceBuilder(resource);
        public DistributedApplication Build() => throw new NotSupportedException("Tests must not start an AppHost.");

        internal TestBuilder(TestSecrets secrets, IEnumerable<KeyValuePair<string, string?>>? configuration = null)
        {
            _secrets = secrets;
            _builder = new DistributedApplicationBuilder(new DistributedApplicationOptions
            {
                AssemblyName = typeof(ServiceKeyParameterTests).Assembly.GetName().Name,
                ProjectDirectory = TestContext.CurrentContext.TestDirectory,
                DisableDashboard = true,
                Args = ["--environment", "Development"]
            });
            Configuration.Sources.Clear();
            Configuration.AddInMemoryCollection(configuration ?? []);
        }
    }

    private sealed class TestSecrets : IUserSecretsManager
    {
        public Dictionary<string, string?> Values { get; } = new(StringComparer.Ordinal);
        public int Writes { get; private set; }
        public string FilePath => "in-memory-test-secrets";
        public bool IsAvailable => true;

        public bool TrySetSecret(string name, string value)
        {
            Values[name] = value;
            Writes++;
            return true;
        }

        public void GetOrSetSecret(IConfigurationManager configuration, string name, Func<string> valueGenerator) =>
            throw new NotSupportedException("Unexpected persistence path.");
        public bool TryDeleteSecret(string name) => throw new NotSupportedException("Tests must not delete secrets.");
        public Task SaveStateAsync(JsonObject state, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Unexpected persistence path.");
    }
}
