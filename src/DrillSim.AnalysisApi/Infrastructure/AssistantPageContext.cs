using System.Text;
using System.Text.Json.Nodes;

namespace DrillSim.AnalysisApi.Infrastructure;

/// <summary>
/// AG-UI clients send what the page shows as RunAgentInput.context, but the hosting layer does not pass it to the model.
/// This rewrites the request so the latest user message starts with a &lt;page_context&gt; block built from those items.
/// </summary>
internal static class AssistantPageContext
{
    internal const int MaximumContextCharacters = 24_000;

    public static async Task InvokeAsync(HttpContext context, Func<Task> next)
    {
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            await next();
            return;
        }
        context.Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            body = await reader.ReadToEndAsync(context.RequestAborted);
        string rewritten = Rewrite(body);
        byte[] bytes = Encoding.UTF8.GetBytes(rewritten);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        await next();
    }

    internal static string Rewrite(string body)
    {
        JsonNode? root;
        try { root = JsonNode.Parse(body); }
        catch (System.Text.Json.JsonException) { return body; }
        if (root?["context"] is not JsonArray items || items.Count == 0 || root["messages"] is not JsonArray messages)
            return body;
        var block = new StringBuilder("<page_context>\n");
        foreach (JsonNode? item in items)
        {
            string? description = item?["description"]?.GetValue<string>();
            string? value = item?["value"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(description) || value is null) continue;
            block.Append("## ").Append(description.Trim()).Append('\n').Append(value).Append('\n');
            if (block.Length > MaximumContextCharacters)
            {
                block.Length = MaximumContextCharacters;
                block.Append("\n[truncated]\n");
                break;
            }
        }
        block.Append("</page_context>\n\n");
        JsonNode? user = messages.LastOrDefault(message => message?["role"]?.GetValue<string>() == "user");
        if (user is null) return body;
        switch (user["content"])
        {
            case JsonValue text when text.TryGetValue(out string? value):
                user["content"] = block + value;
                break;
            case JsonArray parts:
                parts.Insert(0, new JsonObject { ["type"] = "text", ["text"] = block.ToString() });
                break;
            default:
                return body;
        }
        return root.ToJsonString();
    }
}
