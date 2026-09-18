using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class AgentResponsesTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task ReasoningToolRound_UsesStatelessResponsesAndReturnsTheFinalNote(bool streaming)
    {
        using var handler = new ResponsesHandler();
        using var http = new HttpClient(handler);
        int toolCalls = 0;
        ChatOptions options = AgentResponsesOptions.Create("Read evidence before writing a note.",
        [
            AIFunctionFactory.Create(() =>
            {
                toolCalls++;
                return "visible-evidence";
            }, "read_evidence")
        ]);
        options.Reasoning = new ReasoningOptions { Effort = ReasoningEffort.High };
#pragma warning disable OPENAI001
        AIAgent agent = new OpenAIClient(new ApiKeyCredential("test-only"), new OpenAIClientOptions
        {
            Endpoint = new Uri("https://fixture.invalid/openai/v1/"),
            Transport = new HttpClientPipelineTransport(http),
            RetryPolicy = new ClientRetryPolicy(0)
        }).GetResponsesClient().AsAIAgent(new ChatClientAgentOptions
        {
            Name = "TransportRegression",
            ChatOptions = options
        }, model: "gpt-6-astra");
#pragma warning restore OPENAI001
        try
        {
            var text = new StringBuilder();
            if (streaming)
            {
                await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("Write a short field note."))
                    text.Append(update.Text);
            }
            else
                text.Append((await agent.RunAsync("Write a short field note.")).Text);

            Assert.Multiple(() =>
            {
                Assert.That(text.ToString(), Is.EqualTo(ResponsesHandler.FinalNote));
                Assert.That(toolCalls, Is.EqualTo(1));
                Assert.That(handler.Requests, Has.Count.EqualTo(2));
            });
            foreach ((Uri uri, JsonNode body) in handler.Requests)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(uri.AbsolutePath, Is.EqualTo("/openai/v1/responses"));
                    Assert.That(body["model"]!.GetValue<string>(), Is.EqualTo("gpt-6-astra"));
                    Assert.That(body["store"]!.GetValue<bool>(), Is.False);
                    Assert.That(body["background"]!.GetValue<bool>(), Is.False);
                    Assert.That(body["include"]!.AsArray().Select(value => value!.GetValue<string>()),
                        Does.Contain("reasoning.encrypted_content"));
                    Assert.That(body["reasoning"]!["effort"]!.GetValue<string>(), Is.EqualTo("high"));
                    Assert.That(body["previous_response_id"], Is.Null);
                    Assert.That(body["conversation"], Is.Null);
                    Assert.That(body["tools"]!.AsArray().Select(tool => tool!["name"]!.GetValue<string>()),
                        Is.EqualTo(new[] { "read_evidence" }));
                });
            }
            JsonArray continuation = handler.Requests[1].Body["input"]!.AsArray();
            Assert.Multiple(() =>
            {
                Assert.That(continuation.Single(item => (string?)item!["type"] == "reasoning")!["encrypted_content"]!
                    .GetValue<string>(), Is.EqualTo("opaque-test-reasoning"));
                JsonNode result = continuation.Single(item => (string?)item!["type"] == "function_call_output")!;
                Assert.That(result["call_id"]!.GetValue<string>(), Is.EqualTo("call_fixture"));
                Assert.That(result["output"]!.GetValue<string>(), Does.Contain("visible-evidence"));
            });
        }
        finally
        {
            (agent as IDisposable)?.Dispose();
        }
    }

    private sealed class ResponsesHandler : HttpMessageHandler
    {
        internal const string FinalNote = "Observed: visible-evidence.";
        internal List<(Uri Uri, JsonNode Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            JsonNode body = JsonNode.Parse(await request.Content!.ReadAsStringAsync(ct))!;
            Requests.Add((request.RequestUri!, body));
            Assert.That(Requests.Count, Is.LessThanOrEqualTo(2), "An unexpected extra model round was requested.");
            JsonArray output = Requests.Count == 1
                ? JsonNode.Parse("""
                    [
                      {"type":"reasoning","id":"rs_fixture","summary":[],"encrypted_content":"opaque-test-reasoning"},
                      {"type":"function_call","id":"fc_fixture","call_id":"call_fixture","name":"read_evidence","arguments":"{}","status":"completed"}
                    ]
                    """)!.AsArray()
                : new JsonArray(new JsonObject
                {
                    ["type"] = "message", ["id"] = "msg_fixture", ["role"] = "assistant", ["status"] = "completed",
                    ["content"] = new JsonArray(new JsonObject
                    {
                        ["type"] = "output_text", ["text"] = FinalNote, ["annotations"] = new JsonArray()
                    })
                });
            var response = new JsonObject
            {
                ["id"] = $"resp_{Requests.Count}", ["object"] = "response", ["created_at"] = 1789650000,
                ["status"] = "completed", ["model"] = "gpt-6-astra", ["output"] = output,
                ["usage"] = new JsonObject { ["input_tokens"] = 10, ["output_tokens"] = 5, ["total_tokens"] = 15 }
            };
            bool streaming = body["stream"]?.GetValue<bool>() == true;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(streaming ? StreamEvents(response) : response.ToJsonString(),
                    Encoding.UTF8, streaming ? "text/event-stream" : "application/json")
            };
        }

        private static string StreamEvents(JsonObject response)
        {
            var events = new StringBuilder();
            int sequence = 0;
            void Emit(string type, JsonObject data)
            {
                data["type"] = type;
                data["sequence_number"] = sequence++;
                events.Append($"event: {type}\ndata: {data.ToJsonString()}\n\n");
            }
            JsonNode started = response.DeepClone();
            started["output"] = new JsonArray();
            started["status"] = "in_progress";
            Emit("response.created", new JsonObject { ["response"] = started });
            JsonArray output = response["output"]!.AsArray();
            for (int index = 0; index < output.Count; index++)
            {
                JsonNode item = output[index]!;
                JsonNode added = item.DeepClone();
                string id = item["id"]!.GetValue<string>();
                string type = item["type"]!.GetValue<string>();
                if (type == "message") added["content"] = new JsonArray();
                if (type == "function_call") added["arguments"] = "";
                Emit("response.output_item.added", new JsonObject { ["output_index"] = index, ["item"] = added });
                if (type == "function_call")
                {
                    Emit("response.function_call_arguments.delta",
                        new JsonObject { ["output_index"] = index, ["item_id"] = id, ["delta"] = "{}" });
                    Emit("response.function_call_arguments.done",
                        new JsonObject { ["output_index"] = index, ["item_id"] = id, ["arguments"] = "{}" });
                }
                if (type == "message")
                {
                    JsonNode part = item["content"]![0]!;
                    JsonNode emptyPart = part.DeepClone();
                    emptyPart["text"] = "";
                    Emit("response.content_part.added",
                        new JsonObject { ["output_index"] = index, ["item_id"] = id, ["content_index"] = 0, ["part"] = emptyPart });
                    Emit("response.output_text.delta",
                        new JsonObject { ["output_index"] = index, ["item_id"] = id, ["content_index"] = 0, ["delta"] = FinalNote });
                    Emit("response.output_text.done",
                        new JsonObject { ["output_index"] = index, ["item_id"] = id, ["content_index"] = 0, ["text"] = FinalNote });
                    Emit("response.content_part.done",
                        new JsonObject { ["output_index"] = index, ["item_id"] = id, ["content_index"] = 0, ["part"] = part.DeepClone() });
                }
                Emit("response.output_item.done", new JsonObject { ["output_index"] = index, ["item"] = item.DeepClone() });
            }
            Emit("response.completed", new JsonObject { ["response"] = response.DeepClone() });
            return events.ToString();
        }
    }
}
