using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class AssistantPageContextTests
{
    [Test]
    public void Rewrite_PrependsPageContextToLatestUserMessageOnly()
    {
        const string body = """
            {"threadId":"t","runId":"r","messages":[
              {"id":"1","role":"user","content":"first"},
              {"id":"2","role":"assistant","content":"answer"},
              {"id":"3","role":"user","content":"Why is target 1 first?"}],
             "context":[{"description":"Prospects screen: targets","value":"{\"rank\":1}"},{"description":" ","value":"skip"}]}
            """;

        JsonNode root = JsonNode.Parse(AssistantPageContext.Rewrite(body))!;

        Assert.That(root["messages"]![0]!["content"]!.GetValue<string>(), Is.EqualTo("first"));
        Assert.That(root["messages"]![2]!["content"]!.GetValue<string>(), Is.EqualTo(
            "<page_context>\n## Prospects screen: targets\n{\"rank\":1}\n</page_context>\n\nWhy is target 1 first?"));
    }

    [TestCase("not json")]
    [TestCase("""{"messages":[{"role":"user","content":"hi"}],"context":[]}""")]
    [TestCase("""{"messages":[{"role":"assistant","content":"hi"}],"context":[{"description":"d","value":"v"}]}""")]
    public void Rewrite_LeavesBodyUnchangedWhenThereIsNothingToInject(string body) =>
        Assert.That(AssistantPageContext.Rewrite(body), Is.EqualTo(body));

    [Test]
    public void Rewrite_CapsOversizedContext()
    {
        var body = new JsonObject
        {
            ["messages"] = new JsonArray(new JsonObject { ["role"] = "user", ["content"] = "q" }),
            ["context"] = new JsonArray(new JsonObject { ["description"] = "big", ["value"] = new string('x', 50_000) }),
        }.ToJsonString();

        string content = JsonNode.Parse(AssistantPageContext.Rewrite(body))!["messages"]![0]!["content"]!.GetValue<string>();

        Assert.That(content, Does.Contain("[truncated]").And.EndWith("</page_context>\n\nq"));
        Assert.That(content.Length, Is.LessThan(AssistantPageContext.MaximumContextCharacters + 100));
    }
}
