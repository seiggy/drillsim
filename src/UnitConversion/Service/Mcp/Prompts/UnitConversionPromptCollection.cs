using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace OSDC.UnitConversion.Service.Mcp.Prompts;

/// <summary>
/// Provides the static prompts that describe how to interact with the UnitConversion MCP server.
/// </summary>
internal sealed class UnitConversionPromptCollection : IEnumerable<McpServerPrompt>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    private readonly IReadOnlyList<McpServerPrompt> _prompts;

    public UnitConversionPromptCollection()
    {
        _prompts =
        [
            CreatePrompt(
                "resolve-and-convert",
                "Resolve and Convert Measurements",
                "Resolve informal quantity and unit names, retrieve documentation when ambiguity or provenance requires it, and run the appropriate conversion tool.",
                HandleQuantityUnitAlignmentAsync,
                new PromptArgument { Name = "userPrompt", Title = "Conversion request", Description = "Free-form measurement or conversion request to resolve.", Required = true }),
            CreatePrompt(
                "unit-system-report",
                "Unit System Report",
                "Inspect and summarize a unit system by UUID.",
                HandleUnitSystemReportAsync,
                new PromptArgument { Name = "unitSystemId", Title = "Unit system UUID", Description = "UUID returned by list_unit_systems.", Required = true },
                new PromptArgument { Name = "focusUnit", Title = "Optional focus", Description = "Optional unit or dimension to emphasize.", Required = false },
                new PromptArgument { Name = "resourceUri", Title = "Optional resource", Description = "Optional MCP documentation resource URI to read.", Required = false })
        ];
    }

    public IEnumerator<McpServerPrompt> GetEnumerator() => _prompts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static McpServerPrompt CreatePrompt(
        string name,
        string title,
        string description,
        Func<RequestContext<GetPromptRequestParams>, CancellationToken, ValueTask<GetPromptResult>> handler,
        params PromptArgument[] arguments)
    {
        var options = new McpServerPromptCreateOptions
        {
            Name = name,
            Title = title,
            Description = description,
            SerializerOptions = SerializerOptions
        };

        McpServerPrompt prompt = McpServerPrompt.Create(handler, options);
        prompt.ProtocolPrompt.Arguments ??= [];
        foreach (PromptArgument argument in arguments)
        {
            prompt.ProtocolPrompt.Arguments.Add(argument);
        }
        return prompt;
    }

    private static ValueTask<GetPromptResult> HandleQuantityUnitAlignmentAsync(
        RequestContext<GetPromptRequestParams> request,
        CancellationToken cancellationToken)
    {
        var callerPrompt = ReadArgument(request, "userPrompt");
        var builder = new StringBuilder()
            .AppendLine("You triage free-form conversion requests for the Unit Conversion MCP server.")
            .AppendLine("Apply this workflow whenever a caller lists multiple physical quantities or uses informal unit names:")
            .AppendLine("1. Parse the caller text and extract every physical quantity plus requested unit choices (including slang, abbreviations, and implied units).")
            .AppendLine("2. Call search_physical_quantities for each quantity term. Use an exact canonical or synonym match directly; ask for clarification when multiple candidates remain plausible.")
            .AppendLine("3. Call get_physical_quantity with the selected UUID to inspect units, symbolic conversion definitions, hierarchy, and MeaningfulPrecisionInSI. Use search_documentation and resources/read only when ambiguity, explanation, or provenance requires supporting documentation.")
            .AppendLine("4. Normalize unit names against UnitName/UnitLabel. The conversion tool accepts common symbols, plurals, and British/American variants such as metre/meter. If a unit is absent on the requested quantity, search parentPhysicalQuantities in order; do not switch the requested semantic quantity merely to gain a unit.")
            .AppendLine("5. Invoke convert_values for direct unit-choice conversions and convert_between_unit_systems when switching between unit systems. Numeric values remain unrounded; formattedValue uses the requested quantity's MeaningfulPrecisionInSI.")
            .AppendLine("6. Summarize the outcome per quantity with canonical quantity and unit UUIDs. Cite documentation resource URIs only when documentation was actually used.")
            .AppendLine("If no document matches a term, ask the caller to clarify instead of guessing.");

        if (!string.IsNullOrWhiteSpace(callerPrompt))
        {
            builder.AppendLine()
                .AppendLine("Caller prompt excerpt:")
                .AppendLine(callerPrompt.Trim());
        }

        var result = new GetPromptResult
        {
            Description = "Guides the model through resolving ambiguous physical quantities and unit choices before running conversions."
        };
        result.Messages.Add(new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock { Text = builder.ToString() }
        });
        return ValueTask.FromResult(result);
    }

    private static ValueTask<GetPromptResult> HandleUnitSystemReportAsync(
        RequestContext<GetPromptRequestParams> request,
        CancellationToken cancellationToken)
    {
        var unitSystemId = ReadArgument(request, "unitSystemId");
        if (string.IsNullOrWhiteSpace(unitSystemId))
        {
            throw new ArgumentException("Prompt argument 'unitSystemId' is required.");
        }
        var fallbackUnit = ReadArgument(request, "focusUnit") ?? "none specified";
        var referenceResource = ReadArgument(request, "resourceUri");

        var builder = new StringBuilder()
            .AppendLine($"Create a concise report about the unit system '{unitSystemId}'.")
            .AppendLine("Call get_unit_system with this UUID before composing the answer.")
            .AppendLine("Cover:")
            .AppendLine("1. Canonical name, default status, and whether it is SI-compliant.")
            .AppendLine("2. Important conversion factors or derived units that differ from SI.")
            .AppendLine($"3. Contextual guidance for the focus unit or dimension: {fallbackUnit}.")
            .AppendLine("4. Cite any resource documents used so the result can be reproduced.");

        if (!string.IsNullOrWhiteSpace(referenceResource))
        {
            builder.AppendLine()
                .AppendLine($"Seed context with the resource at {referenceResource} if available.");
        }

        var message = new PromptMessage
        {
            Role = Role.User,
            Content = new TextContentBlock
            {
                Text = builder.ToString()
            }
        };

        var result = new GetPromptResult
        {
            Description = "Generates a structured set of tasks to inspect a unit system by id."
        };
        result.Messages.Add(message);
        return ValueTask.FromResult(result);
    }

    private static string? ReadArgument(RequestContext<GetPromptRequestParams> request, string argumentName)
    {
        if (request.Params?.Arguments is not IDictionary<string, JsonElement> arguments)
        {
            return null;
        }

        if (!arguments.TryGetValue(argumentName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number when value.TryGetDouble(out var number) => number.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
