using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.UnitConversion.Service.Mcp.Tools;

public sealed class SearchPhysicalQuantitiesMcpTool : IMcpTool
{
    private static readonly JsonObject Schema = new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["query"] = new JsonObject { ["type"] = "string", ["minLength"] = 1, ["description"] = "Canonical name, synonym, or partial quantity name." },
            ["limit"] = new JsonObject { ["type"] = "integer", ["minimum"] = 1, ["maximum"] = 50, ["default"] = 10 }
        },
        ["required"] = new JsonArray("query"),
        ["additionalProperties"] = false
    };

    public string Name => "search_physical_quantities";
    public string Title => "Search Physical Quantities";
    public string Description => "Search canonical physical-quantity names and synonyms. Returns ranked candidates with stable UUIDs, match type, SI unit, meaningful SI precision, and hierarchy information. Use get_physical_quantity for full unit choices.";
    public JsonNode? InputSchema => Schema;
    public JsonNode? OutputSchema => McpContractSchemas.TypedObject(("results", "array"));

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        if (arguments?["query"] is not JsonValue queryNode || !queryNode.TryGetValue<string>(out string? query) || string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'query' is required and must be a non-empty string."));
        }

        int limit = 10;
        if (arguments?["limit"] is JsonValue limitNode && limitNode.TryGetValue<int>(out int requested))
        {
            limit = Math.Clamp(requested, 1, 50);
        }

        string token = McpNameNormalizer.NormalizeText(query);
        if (token.Length == 0)
        {
            return Task.FromResult<JsonNode?>(McpToolResponses.CreateValidationError("Argument 'query' must contain letters or digits."));
        }
        var matches = new List<Match>();
        foreach (BasePhysicalQuantity quantity in DrillingPhysicalQuantity.AvailablePhysicalQuantities ?? [])
        {
            AddMatch(matches, quantity, quantity.Name, "canonicalName", 1.0, token);
            foreach (string synonym in quantity.UsualNames ?? [])
            {
                AddMatch(matches, quantity, synonym, "synonym", 0.98, token);
            }
        }

        var results = new JsonArray();
        foreach (Match match in matches
                     .GroupBy(value => value.Quantity.ID)
                     .Select(group => group.OrderByDescending(value => value.Score).ThenBy(value => value.MatchedLabel.Length).First())
                     .OrderByDescending(value => value.Score)
                     .ThenBy(value => value.Quantity.Name, StringComparer.OrdinalIgnoreCase)
                     .Take(limit))
        {
            results.Add(new JsonObject
            {
                ["id"] = match.Quantity.ID.ToString(),
                ["name"] = match.Quantity.Name,
                ["matchedLabel"] = match.MatchedLabel,
                ["matchType"] = match.MatchType,
                ["score"] = match.Score,
                ["siUnitName"] = match.Quantity.SIUnitName,
                ["siUnitLabel"] = match.Quantity.SIUnitLabel,
                ["meaningfulPrecisionInSI"] = match.Quantity.MeaningfulPrecisionInSI,
                ["hierarchy"] = PhysicalQuantityMcpMetadata.Create(match.Quantity)
            });
        }

        return Task.FromResult<JsonNode?>(new JsonObject { ["results"] = results });
    }

    private static void AddMatch(List<Match> matches, BasePhysicalQuantity quantity, string? label, string exactType, double exactScore, string token)
    {
        if (string.IsNullOrWhiteSpace(label)) return;
        string normalized = McpNameNormalizer.NormalizeText(label);
        if (normalized.Length == 0) return;

        if (normalized.Equals(token, StringComparison.Ordinal))
        {
            matches.Add(new Match(quantity, label, exactType, exactScore));
        }
        else if (normalized.Contains(token, StringComparison.Ordinal) || token.Contains(normalized, StringComparison.Ordinal))
        {
            double coverage = (double)Math.Min(normalized.Length, token.Length) / Math.Max(normalized.Length, token.Length);
            matches.Add(new Match(quantity, label, "partial", Math.Round(0.55 + 0.30 * coverage, 4)));
        }
    }

    private sealed record Match(BasePhysicalQuantity Quantity, string MatchedLabel, string MatchType, double Score);
}
