using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Protocol;
using NUnit.Framework;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Service.Mcp;
using OSDC.UnitConversion.Service.Mcp.Prompts;
using OSDC.UnitConversion.Service.Mcp.Tools;

namespace OSDC.UnitConversion.ServiceTest;

[TestFixture]
public class McpContractTests
{
    [Test]
    public void Adapter_AdvertisesModernContractMetadata()
    {
        var tool = new ContractTool();
        var adapter = new McpServerToolAdapter(tool, NullLoggerFactory.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(adapter.ProtocolTool.Name, Is.EqualTo("contract_test"));
            Assert.That(adapter.ProtocolTool.Title, Is.EqualTo("Contract Test"));
            Assert.That(adapter.ProtocolTool.InputSchema.ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Object));
            Assert.That(adapter.ProtocolTool.OutputSchema?.ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Object));
            Assert.That(adapter.ProtocolTool.Annotations?.ReadOnlyHint, Is.True);
            Assert.That(adapter.ProtocolTool.Annotations?.DestructiveHint, Is.False);
            Assert.That(adapter.ProtocolTool.Annotations?.IdempotentHint, Is.True);
            Assert.That(adapter.ProtocolTool.Annotations?.OpenWorldHint, Is.False);
        });
    }

    [Test]
    public void Adapter_ReturnsTextAndStructuredContentForSuccess()
    {
        var adapter = new McpServerToolAdapter(new ContractTool(), NullLoggerFactory.Instance);
        CallToolResult result = adapter.CreateCallToolResult(new JsonObject { ["value"] = 42 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsError, Is.Not.True);
            Assert.That(result.StructuredContent, Is.Not.Null);
            Assert.That(result.Content.OfType<TextContentBlock>().Single().Text, Does.Contain("42"));
        });
    }

    [Test]
    public void Adapter_ReportsDomainFailuresAsToolErrors()
    {
        var adapter = new McpServerToolAdapter(new ContractTool(), NullLoggerFactory.Instance);
        CallToolResult result = adapter.CreateCallToolResult(McpToolResponses.CreateError(422, "Incompatible units."));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsError, Is.True);
            Assert.That(result.StructuredContent, Is.Null);
            Assert.That(result.Content.OfType<TextContentBlock>().Single().Text, Is.EqualTo("Incompatible units."));
        });
    }

    [Test]
    public void Adapter_EmitsResourceLinksFromDocumentationResults()
    {
        var adapter = new McpServerToolAdapter(new ContractTool(), NullLoggerFactory.Instance);
        var payload = new JsonObject
        {
            ["resourceLinks"] = new JsonArray(new JsonObject
            {
                ["uri"] = "resource://unit-conversion/documents/example",
                ["name"] = "example",
                ["title"] = "Example"
            })
        };

        CallToolResult result = adapter.CreateCallToolResult(payload);
        Assert.That(result.Content.OfType<ResourceLinkBlock>().Single().Uri, Is.EqualTo("resource://unit-conversion/documents/example"));
    }

    [Test]
    public async Task ConvertValues_IsPureAndReturnsResolvedContract()
    {
        BasePhysicalQuantity quantity = DrillingPhysicalQuantity.AvailablePhysicalQuantities
            .First(item => item.UnitChoices is { Count: >= 2 });
        UnitChoice input = quantity.UnitChoices![0];
        UnitChoice output = quantity.UnitChoices[1];
        var arguments = new JsonObject
        {
            ["physicalQuantityId"] = quantity.ID.ToString(),
            ["unitInId"] = input.ID.ToString(),
            ["unitOutId"] = output.ID.ToString(),
            ["values"] = new JsonArray(10d, -2.5d)
        };

        JsonNode? result = await new ConvertValuesMcpTool().InvokeAsync(arguments, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result?["quantity"]?["id"]?.GetValue<string>(), Is.EqualTo(quantity.ID.ToString()));
            Assert.That(result?["inputUnit"]?["id"]?.GetValue<string>(), Is.EqualTo(input.ID.ToString()));
            Assert.That(result?["outputUnit"]?["id"]?.GetValue<string>(), Is.EqualTo(output.ID.ToString()));
            Assert.That(result?["results"]?.AsArray(), Has.Count.EqualTo(2));
            Assert.That(result?["results"]?[0]?["numericValue"], Is.Not.Null);
            Assert.That(result?["results"]?[0]?["formattedValue"], Is.Not.Null);
        });
    }

    [Test]
    public void Prompts_AdvertiseArgumentsAndOnlyReferenceCurrentOperations()
    {
        var prompts = new UnitConversionPromptCollection().ToList();
        var resolve = prompts.Single(prompt => prompt.ProtocolPrompt.Name == "resolve-and-convert").ProtocolPrompt;
        var report = prompts.Single(prompt => prompt.ProtocolPrompt.Name == "unit-system-report").ProtocolPrompt;

        Assert.Multiple(() =>
        {
            Assert.That(resolve.Arguments?.Single(argument => argument.Name == "userPrompt").Required, Is.True);
            Assert.That(report.Arguments?.Single(argument => argument.Name == "unitSystemId").Required, Is.True);
        });
    }

    [Test]
    public void Mutations_AreAccuratelyAnnotated()
    {
        using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        var create = new McpServerToolAdapter(new PostUnitSystemMcpTool(services, NullLogger<PostUnitSystemMcpTool>.Instance), NullLoggerFactory.Instance).ProtocolTool;
        var replace = new McpServerToolAdapter(new PutUnitSystemByIdMcpTool(services, NullLogger<PutUnitSystemByIdMcpTool>.Instance), NullLoggerFactory.Instance).ProtocolTool;
        var delete = new McpServerToolAdapter(new DeleteUnitSystemByIdMcpTool(services, NullLogger<DeleteUnitSystemByIdMcpTool>.Instance), NullLoggerFactory.Instance).ProtocolTool;

        Assert.Multiple(() =>
        {
            Assert.That(create.Annotations?.ReadOnlyHint, Is.False);
            Assert.That(create.Annotations?.DestructiveHint, Is.False);
            Assert.That(create.Annotations?.IdempotentHint, Is.False);
            Assert.That(replace.Annotations?.DestructiveHint, Is.True);
            Assert.That(replace.Annotations?.IdempotentHint, Is.True);
            Assert.That(delete.Annotations?.DestructiveHint, Is.True);
            Assert.That(delete.Annotations?.IdempotentHint, Is.False);
            Assert.That(create.InputSchema.GetProperty("properties").GetProperty("unitSystem").GetProperty("properties")
                    .TryGetProperty("isSI", out _), Is.False,
                "isSI must be derived by the service, not supplied by an MCP caller.");
        });
    }

    private sealed class ContractTool : IMcpTool
    {
        public string Name => "contract_test";
        public string Title => "Contract Test";
        public string Description => "Test tool.";
        public JsonNode? InputSchema => new JsonObject { ["type"] = "object", ["additionalProperties"] = false };
        public JsonNode? OutputSchema => new JsonObject { ["type"] = "object" };
        public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken) => Task.FromResult<JsonNode?>(new JsonObject { ["value"] = 42 });
    }
}
