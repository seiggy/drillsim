using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using Microsoft.Extensions.AI;

namespace DrillSim.AnalysisApi.Services;

internal sealed class FormationInterpretationTools
{
    internal const int MaximumBriefBytes = 8 * 1024;
    internal const int MaximumResultBytes = FormationInterpretationEvidence.MaximumContextBytes;
    internal const int MaximumTotalResultBytes = 2 * 1024 * 1024;
    internal const int MaximumCalls = 128;
    internal const int MaximumModelIterations = 40;
    internal const int MaximumPageSize = 128;
    private readonly JsonObject _snapshot;
    private readonly IReadOnlySet<string> _visible;
    private readonly Dictionary<string, JsonObject> _records = new(StringComparer.Ordinal);
    private readonly HashSet<string> _delivered = new(StringComparer.Ordinal);
    private readonly object _gate = new();
    private int _calls;
    private int _resultBytes;
    private bool _evidenceRead;
    private bool _budgetExceeded;

    internal FormationInterpretationTools(FormationInterpretationEvidence evidence)
    {
        _snapshot = JsonNode.Parse(evidence.Json)!.AsObject();
        _visible = evidence.CitationAllowlist;
        foreach (string collection in new[] { "clusters", "wells", "wellBores", "architecture", "surveys", "geology" })
            foreach (JsonObject record in Rows(_snapshot[collection]).Cast<JsonObject>())
                _records.Add(Text(record, "evidenceId")!, record);
        JsonObject field = _snapshot["field"]!.AsObject();
        _records.Add(Text(field, "evidenceId")!, field);
        _delivered.Add(Text(field, "evidenceId")!);

        JsonNode notes = _snapshot["humanNotesUntrusted"]!;
        bool includeNotes = Encoding.UTF8.GetByteCount(notes.ToJsonString()) <= 2048;
        Brief = Serialize(new
        {
            version = "formation-interpretation-brief-v2",
            scope = _snapshot["scope"],
            packageSha256 = _snapshot["packageSha256"],
            analysisSha256 = _snapshot["analysisSha256"],
            configurationSha256 = _snapshot["configurationSha256"],
            savedHypothesis = _snapshot["savedHypothesis"],
            snapshotSha256 = _snapshot["snapshotSha256"],
            evidenceTime = _snapshot["evidenceTime"],
            field = Select(field, "evidenceId", "name"),
            actualVisibleCounts = _snapshot["actualVisibleCounts"],
            configuration = _snapshot["appliedAnalysis"]!["configuration"],
            selectedCandidateId = _snapshot["appliedAnalysis"]!["selectedCandidate"]?["candidateId"],
            humanNotesUntrusted = includeNotes ? notes : null,
            notesAvailable = new
            {
                included = includeNotes,
                nameCharacters = Text(notes, "name")!.Length,
                rationaleCharacters = Text(notes, "rationale")!.Length,
                correlationNotesCharacters = Text(notes, "correlationNotes")!.Length,
                controlNoteCount = Rows(notes["controlNotes"]).Count()
            },
            limits = new { modelIterations = MaximumModelIterations, toolCalls = MaximumCalls,
                bytesPerToolResult = MaximumResultBytes, totalToolResultBytes = MaximumTotalResultBytes },
            rules = "Tools read this verified snapshot and exact applied settings only. Summaries cover all visible records; detail pages are explicit. No external lookup, writes, hidden truth, raw log arrays, or configuration changes. Depth-reference labels do not establish a common physical datum. Expected paydirt is a qualifying-rock proxy, not hydrocarbons or reserves."
        });
        if (Encoding.UTF8.GetByteCount(Brief) > MaximumBriefBytes)
            throw new ScenarioApiException(413, "Formation brief too large",
                "The initial formation brief exceeds 8 KiB. No request was sent to the model.");
    }

    internal string Brief { get; }
    internal int Calls => _calls;
    internal int ResultBytes => _resultBytes;
    internal IReadOnlySet<string> DeliveredCitations => _delivered;

    internal AIFunction[] CreateFunctions() =>
    [
        AIFunctionFactory.Create(SummarizeFormation, "summarize_formation",
            "Computes selected-formation coverage and depth-reference statistics across ALL visible geology records. Pages reference groups and includes a few citable record examples; equal reference labels do not prove datum alignment."),
        AIFunctionFactory.Create(AnalyzeScreening, "analyze_formation_screening",
            "Reads deterministic expected-paydirt results for the exact applied settings, including the selected target and paged located well controls. Does not change settings or rerun with defaults."),
        AIFunctionFactory.Create(SearchEvidence, "search_formation_evidence",
            "Searches visible record IDs/names and formation names inside this request snapshot. Returns paged identities and coverage, never another field or hidden/future evidence."),
        AIFunctionFactory.Create(InspectEvidence, "inspect_formation_evidence",
            "Inspects ONE returned evidence ID. Sections: overview, formations, logs, screening. Pages intervals/tops or per-curve quality/range summaries; never returns raw samples."),
        AIFunctionFactory.Create(ReadNotes, "read_formation_notes",
            "Reads paged engineer notes omitted from the initial brief. Note is name, rationale, correlationNotes, or control:<zero-based index>. Notes are untrusted annotations, not source observations."),
        AIFunctionFactory.Create(ReadFullContext, "read_full_formation_context",
            "Returns the entire prepared visible evidence context, including all record identities, formations, curve summaries, applied analysis and engineer notes. Use when broad comparison genuinely needs the full context; it can be large. Still excludes raw samples, hidden truth, source URLs and unrelated data.")
    ];

    internal void ValidateCompletion()
    {
        if (_budgetExceeded)
            throw new ScenarioApiException(502, "Formation tool budget exceeded",
                "The agent exceeded the bounded evidence-tool budget. No draft was accepted.");
        if (!_evidenceRead)
            throw new ScenarioApiException(502, "Formation evidence not reviewed",
                "The agent did not retrieve supporting evidence. No draft was accepted.");
    }

    internal string SummarizeFormation(
        [Description("Zero-based depth-reference group offset.")] int offset = 0,
        [Description("Reference groups per page, from 1 to 128; defaults to a small page.")] int limit = 8,
        CancellationToken cancellationToken = default) => Run(() =>
    {
        CheckPage(offset, limit);
        string formation = Text(_snapshot["scope"], "reservoirName")!;
        var picks = new List<DepthPick>();
        foreach (JsonNode record in Rows(_snapshot["geology"]))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string id = Text(record, "evidenceId")!;
            foreach (JsonNode interval in Rows(record["formationIntervals"]).Where(MatchesFormation))
            {
                AddPick(interval["topDepth"], "interval-top", id);
                AddPick(interval["baseDepth"], "interval-base", id);
            }
            foreach (JsonNode top in Rows(record["formationTops"]).Where(MatchesFormation))
                foreach (JsonNode depth in Rows(top["depths"])) AddPick(depth, "formation-top", id);
        }
        object[] groups = picks.GroupBy(p => (p.Kind, p.Reference, p.Unit, p.Datum, p.PositiveDown))
            .OrderBy(g => g.Key.Kind, StringComparer.Ordinal)
            .ThenBy(g => g.Key.Reference, StringComparer.Ordinal).ThenBy(g => g.Key.Unit, StringComparer.Ordinal)
            .ThenBy(g => g.Key.Datum, StringComparer.Ordinal).ThenBy(g => g.Key.PositiveDown)
            .Select(g =>
            {
                DepthPick[] values = g.Where(p => p.Value.HasValue).OrderBy(p => p.Value).ToArray();
                return (object)new
                {
                    kind = g.Key.Kind, reference = g.Key.Reference, unit = g.Key.Unit, datum = g.Key.Datum,
                    positiveDown = g.Key.PositiveDown, pickCount = g.Count(), usableValueCount = values.Length,
                    minimum = values.FirstOrDefault()?.Value, maximum = values.LastOrDefault()?.Value,
                    sourceEvidenceIds = new[] { values.FirstOrDefault()?.EvidenceId, values.LastOrDefault()?.EvidenceId }
                        .OfType<string>().Distinct(StringComparer.Ordinal).ToArray()
                };
            }).ToArray();
        string response = Publish(new
        {
            formation,
            actualVisibleCounts = _snapshot["actualVisibleCounts"],
            depthGroups = Page(groups, offset, limit),
            depthComparison = "Diagnostic ranges in declared units, grouped by reference labels only. MD across wells is not structural elevation; unknown metadata is not a default or a common datum. No conversion or structural surface fitting was performed.",
            recordExamples = Rows(_snapshot["geology"]).Take(Math.Min(limit, 4)).Select(Overview).ToArray(),
            examplesAreNotTheEvidencePopulation = true,
            missingData = _snapshot["missingData"]
        });
        return response;

        bool MatchesFormation(JsonNode node) => string.Equals(Text(node, "formationName"), formation, StringComparison.OrdinalIgnoreCase);
        void AddPick(JsonNode? depth, string kind, string id) => picks.Add(new(id, kind,
            Text(depth, "reference"), Text(depth, "unit"), Text(depth, "datum"),
            depth?["positiveDown"]?.GetValue<bool>(), depth?["value"]?.GetValue<double>()));
    }, cancellationToken);

    internal string AnalyzeScreening(
        [Description("Zero-based located-well offset.")] int offset = 0,
        [Description("Located wells per page, from 1 to 128; defaults to a small page.")] int limit = 4,
        CancellationToken cancellationToken = default) => Run(() =>
    {
        CheckPage(offset, limit);
        JsonNode analysis = _snapshot["appliedAnalysis"]!;
        return Publish(new
        {
            basis = "Deterministic screening already computed for this exact snapshot and applied configuration; not fluid-conditioned pay or economic reserves.",
            configuration = analysis["configuration"], methodology = analysis["methodology"],
            wellControls = Page(Rows(analysis["wellSummaries"]).ToArray(), offset, limit),
            selectedCandidate = analysis["selectedCandidate"],
            selectedCandidateLimitation = analysis["selectedCandidateLimitation"],
            gridPointCount = analysis["gridPointCount"], eligiblePointCount = analysis["eligiblePointCount"]
        });
    }, cancellationToken);

    internal string SearchEvidence(
        [Description("Record kind: geology, trajectory, architecture, wellbore, well, cluster, field, or all.")] string kind = "geology",
        [Description("Case-insensitive literal name, formation, or ID text, at most 80 characters.")] string query = "",
        [Description("Zero-based result offset.")] int offset = 0,
        [Description("Records per page, from 1 to 128; defaults to a small page.")] int limit = 8,
        CancellationToken cancellationToken = default) => Run(() =>
    {
        CheckPage(offset, limit);
        if (kind is not ("geology" or "trajectory" or "architecture" or "wellbore" or "well" or "cluster" or "field" or "all") ||
            query is null || query.Length > 80 || query.Any(char.IsControl))
            throw Invalid("Use a documented record kind and literal query of at most 80 characters.");
        JsonObject[] records = _records.Where(pair => kind == "all" || pair.Key.StartsWith(kind + ":", StringComparison.Ordinal))
            .Where(pair => query.Length == 0 || SearchTerms(pair.Value).Any(text => text.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => Overview(pair.Value)).ToArray();
        return Publish(new { matches = Page(records, offset, limit) });
    }, cancellationToken);

    internal string InspectEvidence(
        [Description("An exact visible evidence ID returned by the summary or search tools.")] string evidenceId,
        [Description("overview, formations, logs, or screening.")] string section = "overview",
        [Description("Zero-based detail-row offset.")] int offset = 0,
        [Description("Detail rows per page, from 1 to 128; defaults to a small page.")] int limit = 8,
        CancellationToken cancellationToken = default) => Run(() =>
    {
        CheckPage(offset, limit);
        if (evidenceId is null || !_records.TryGetValue(evidenceId, out JsonObject? record))
            throw Invalid("That evidence ID is not available in this request snapshot. Search the visible evidence first.");
        if (section is not ("overview" or "formations" or "logs" or "screening"))
            throw Invalid("Use overview, formations, logs, or screening.");
        if (section != "overview" && !evidenceId.StartsWith("geology:", StringComparison.Ordinal))
            throw Invalid("Formation, log and screening sections require a visible geology ID.");
        JsonObject identity = Overview(record);
        if (section == "overview")
            return Publish(new { record = evidenceId.StartsWith("geology:", StringComparison.Ordinal)
                ? identity : record.DeepClone(), numericRangeFields = _snapshot["numericRangeFields"],
                numericRangeMeaning = _snapshot["numericRangeMeaning"] });
        if (section == "formations")
            return Publish(new { record = identity, formations = Page(
                Rows(record["formationIntervals"]).Select(row => (object)new { kind = "interval", value = row })
                    .Concat(Rows(record["formationTops"]).Select(row => (object)new { kind = "top", value = row })).ToArray(), offset, limit) });
        if (section == "screening")
            return Publish(new { record = identity, appliedGeologyResults = record["appliedGeologyResults"],
                petrophysicsTableSummary = record["petrophysicsTableSummary"],
                tableMeaning = _snapshot["petrophysicsTableMeaning"], numericRangeFields = _snapshot["numericRangeFields"] });
        object[] curves = Rows(record["logRuns"]).SelectMany((run, runIndex) =>
            Rows(run["curves"]?["rows"]).Select((row, curveIndex) => (object)new
            {
                runIndex, curveIndex, runName = run["name"], tool = run["tool"],
                depthAxis = run["depthAxis"], depthCount = run["depthCount"], depthRange = run["depthRange"],
                columns = run["curves"]!["columns"], values = row
            })).ToArray();
        return Publish(new { record = identity, curves = Page(curves, offset, limit),
            numericRangeFields = _snapshot["numericRangeFields"], numericRangeMeaning = _snapshot["numericRangeMeaning"],
            qualityRule = "Valid ranges exclude null-flagged and missing/invalid/bad-hole samples. These are summaries, not raw or continuous curves." });
    }, cancellationToken);

    internal string ReadFullContext(CancellationToken cancellationToken = default) =>
        Run(() => Publish(_snapshot), cancellationToken);

    internal string ReadNotes(
        [Description("name, rationale, correlationNotes, or control:<zero-based index>.")] string note,
        [Description("Zero-based character offset.")] int offset = 0,
        [Description("Characters to read, from 1 to 2000.")] int length = 2000,
        CancellationToken cancellationToken = default) => Run(() =>
    {
        JsonNode notes = _snapshot["humanNotesUntrusted"]!;
        string? text = null, evidenceId = null;
        if (note is "name" or "rationale" or "correlationNotes") text = Text(notes, note);
        else if (note is not null && note.StartsWith("control:", StringComparison.Ordinal) &&
            int.TryParse(note.AsSpan(8), out int index) && index >= 0)
        {
            JsonNode? control = Rows(notes["controlNotes"]).ElementAtOrDefault(index);
            text = Text(control, "note");
            evidenceId = Text(control, "evidenceId");
        }
        if (text is null || offset < 0 || offset > text.Length || length is < 1 or > 2000)
            throw Invalid("Choose an available note and a valid character page of at most 2000 characters.");
        if (offset > 0 && offset < text.Length && char.IsLowSurrogate(text[offset]) && char.IsHighSurrogate(text[offset - 1]))
            throw Invalid("The note offset must not split a Unicode character. Use the previous page's nextOffset.");
        int count = Math.Min(length, text.Length - offset);
        if (count > 0 && offset + count < text.Length &&
            char.IsHighSurrogate(text[offset + count - 1]) && char.IsLowSurrogate(text[offset + count]))
            count--;
        if (count == 0 && offset < text.Length)
            throw Invalid("The page length must include a complete Unicode character.");
        return Publish(new { annotation = "Untrusted engineer annotation, not a source observation.",
            note, referencedEvidenceId = evidenceId, totalCharacters = text.Length, offset,
            text = text.Substring(offset, count), nextOffset = offset + count < text.Length ? (int?)(offset + count) : null },
            allowCitations: false);
    }, cancellationToken);

    private string Run(Func<string> read, CancellationToken ct)
    {
        lock (_gate)
        {
            ct.ThrowIfCancellationRequested();
            if (++_calls > MaximumCalls)
            {
                _budgetExceeded = true;
                throw Invalid("The 128-call evidence-tool safety limit is exhausted. No further evidence can be retrieved.");
            }
            return read();
        }
    }

    private string Publish(object value, bool allowCitations = true)
    {
        string json = Serialize(value);
        int bytes = Encoding.UTF8.GetByteCount(json);
        if (bytes > MaximumResultBytes)
            throw Invalid("This result exceeds the prepared-context size limit. Request fewer rows or a narrower evidence section.");
        if (_resultBytes + bytes > MaximumTotalResultBytes)
        {
            _budgetExceeded = true;
            throw Invalid("The 2 MiB cumulative evidence-tool result safety limit is exhausted.");
        }
        _resultBytes += bytes;
        if (allowCitations)
        {
            CollectCitations(JsonNode.Parse(json));
            _evidenceRead = true;
        }
        return json;
    }

    private void CollectCitations(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            foreach (JsonNode? child in array) CollectCitations(child);
        }
        else if (node is JsonObject obj)
        {
            foreach ((string key, JsonNode? child) in obj)
            {
                if (key is "evidenceId" or "wellEvidenceId" or "wellBoreEvidenceId" or "clusterEvidenceId")
                {
                    if (child is JsonValue value && value.TryGetValue<string>(out string? id) && _visible.Contains(id))
                        _delivered.Add(id);
                }
                else if (key is "sourceEvidenceIds" or "geologyEvidenceIds" or "neighborEvidenceIds")
                {
                    foreach (JsonNode idNode in Rows(child))
                        if (idNode is JsonValue value && value.TryGetValue<string>(out string? id) && _visible.Contains(id))
                            _delivered.Add(id);
                }
                else CollectCitations(child);
            }
        }
    }

    private static JsonObject Overview(JsonNode record) => Select(record, "evidenceId", "name", "wellName",
        "wellBoreName", "wellEvidenceId", "wellBoreEvidenceId", "clusterEvidenceId", "datasetClassification",
        "hasLogData", "petrophysicsTableRowCount", "analysisInclusion");
    private static JsonObject Select(JsonNode record, params string[] keys)
    {
        var result = new JsonObject();
        foreach (string key in keys)
            if (record[key] is JsonNode value) result[key] = value.DeepClone();
        return result;
    }
    private static IEnumerable<string> SearchTerms(JsonObject record) =>
        new[] { Text(record, "evidenceId"), Text(record, "name"), Text(record, "wellName"), Text(record, "wellBoreName") }
            .Concat(Rows(record["formationIntervals"]).Concat(Rows(record["formationTops"]))
                .Select(row => Text(row, "formationName"))).OfType<string>();
    private static object Page<T>(T[] rows, int offset, int limit)
    {
        if (offset > rows.Length) throw Invalid("The page offset is beyond the available rows.");
        T[] page = rows.Skip(offset).Take(limit).ToArray();
        return new { total = rows.Length, offset, returned = page.Length,
            nextOffset = offset + page.Length < rows.Length ? (int?)(offset + page.Length) : null, rows = page };
    }
    private static void CheckPage(int offset, int limit)
    {
        if (offset < 0 || limit is < 1 or > MaximumPageSize)
            throw Invalid("Use a nonnegative offset and a page size from 1 to 128.");
    }
    private static IEnumerable<JsonNode> Rows(JsonNode? node) => node is JsonArray array ? array.OfType<JsonNode>() : [];
    private static string? Text(JsonNode? node, string key) => node?[key]?.GetValue<string>();
    private static string Serialize(object value) => JsonSerializer.Serialize(value, FormationInterpretationAgent.JsonOptions);
    private static ArgumentException Invalid(string message) => new(message);
    private sealed record DepthPick(string EvidenceId, string Kind, string? Reference, string? Unit,
        string? Datum, bool? PositiveDown, double? Value);
}
