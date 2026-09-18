using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace DrillSim.AnalysisApi.Services;

internal static class AgentResponsesOptions
{
#pragma warning disable OPENAI001 // Stateless tool rounds must carry encrypted reasoning locally.
    internal static ChatOptions Create(string instructions, IList<AITool> tools) => new()
    {
        Instructions = instructions,
        Tools = tools,
        RawRepresentationFactory = _ => new CreateResponseOptions
        {
            StoredOutputEnabled = false,
            BackgroundModeEnabled = false,
            IncludedProperties = { IncludedResponseProperty.ReasoningEncryptedContent }
        }
    };
#pragma warning restore OPENAI001
}
