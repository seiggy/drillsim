using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Services;

internal sealed record FormationInterpretationEvidence(string Json, IReadOnlySet<string> CitationAllowlist)
{
    internal const int MaximumContextBytes = 192 * 1024;
    private const int MaximumRecords = 1024;
    private const int MaximumArrayItems = 100_000;
    private const int MaximumSampleValues = 2_000_000;
    private static readonly JsonSerializerOptions ContextJsonOptions = new(FormationInterpretationAgent.JsonOptions)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static FormationInterpretationEvidence Build(FormationInterpretationRequest request, AnalysisPackage package,
        AnalysisResult analysis, CandidateGridPoint? selected, CancellationToken ct)
    {
        var builder = new EvidenceBuilder(package, ct);
        object context = builder.Build(request, analysis, selected);
        string json = JsonSerializer.Serialize(context, ContextJsonOptions);
        int bytes = Encoding.UTF8.GetByteCount(json);
        if (bytes > MaximumContextBytes)
            throw TooLarge($"The complete evidence context is {bytes} bytes and exceeds 192 KiB. No formation records were silently truncated.");
        return new(json, builder.Citations);
    }

    private sealed class EvidenceBuilder(AnalysisPackage package, CancellationToken ct)
    {
        public HashSet<string> Citations { get; } = new(StringComparer.Ordinal);
        private readonly HashSet<string> _visible = HypothesisValidation.Evidence(package);
        private readonly List<string> _gaps = [];
        private int _sampleValues;

        public object Build(FormationInterpretationRequest request, AnalysisResult analysis, CandidateGridPoint? selected)
        {
            ct.ThrowIfCancellationRequested();
            if (1L + package.Clusters.Count + package.Wells.Count + package.WellBores.Count +
                package.WellBoreArchitectures.Count + package.Trajectories.Count + package.GeologicalProperties.Count > MaximumRecords)
                throw TooLarge("AI drafting supports at most 1024 visible source records. No evidence was omitted.");
            var wells = package.Wells.ToDictionary(well => JsonAccess.MetaId(well) ?? throw MissingId(), well => well);
            var bores = package.WellBores.ToDictionary(bore => JsonAccess.MetaId(bore) ?? throw MissingId(), bore => bore);
            string? Link(JsonNode node, string property, string kind)
            {
                Guid? id = JsonAccess.Guid(node, property);
                return id is Guid value && _visible.Contains(EvidenceCatalog.CreateId(kind, value))
                    ? EvidenceCatalog.CreateId(kind, value) : null;
            }
            object[] clusters = package.Clusters.Select(node => (object)new
            {
                evidenceId = Cite(node, EvidenceCatalog.Cluster), name = Text(node, "Name"),
                position = Position(JsonAccess.Get(node, "ReferencePoint"))
            }).ToArray();
            object[] wellRecords = package.Wells.Select(node => (object)new
            {
                evidenceId = Cite(node, EvidenceCatalog.Well), name = Text(node, "Name"),
                clusterEvidenceId = Link(node, "ClusterID", EvidenceCatalog.Cluster),
                datasetClassification = Classification(JsonAccess.Get(JsonAccess.Get(node, "Dataset"), "Provenance"), well: true)
            }).ToArray();
            object[] boreRecords = package.WellBores.Select(node => (object)new
            {
                evidenceId = Cite(node, EvidenceCatalog.WellBore), name = Text(node, "Name"),
                wellEvidenceId = Link(node, "WellID", EvidenceCatalog.Well)
            }).ToArray();
            object[] architecture = package.WellBoreArchitectures.Select(node => (object)new
            {
                evidenceId = Cite(node, EvidenceCatalog.Architecture), name = Text(node, "Name"),
                wellBoreEvidenceId = Link(node, "WellBoreID", EvidenceCatalog.WellBore),
                detail = "Visible architecture identity only; geometry and completion details are not supplied to this drafting agent."
            }).ToArray();
            object[] surveys = package.Trajectories.Select(node =>
            {
                JsonArray stations = Array(node, "SurveyStationList");
                Spend(stations.Count);
                string? bore = Link(node, "WellBoreID", EvidenceCatalog.WellBore);
                string? well = Link(node, "WellID", EvidenceCatalog.Well);
                if (stations.Count == 0) _gaps.Add("A visible trajectory has no survey stations; a tie-in is not a surveyed path.");
                if (bore is null) _gaps.Add("A visible trajectory has no resolvable wellbore association; do not assign its path to a bore.");
                return (object)new
                {
                    evidenceId = Cite(node, EvidenceCatalog.Trajectory), name = Text(node, "Name"),
                    wellBoreEvidenceId = bore, wellEvidenceId = well,
                    association = bore is not null ? "wellbore-linked" : well is not null ? "well-linked-only" : "unresolved",
                    surveyStationCount = stations.Count,
                    stationsWithMd = stations.Count(station => Md(station).HasValue),
                    stationsWithTvd = stations.Count(station => Tvd(station).HasValue),
                    stationsWithMetricPosition = stations.Count(station =>
                        JsonAccess.Number(station, "RiemannianEast").HasValue && JsonAccess.Number(station, "RiemannianNorth").HasValue ||
                        JsonAccess.Number(station, "X").HasValue && JsonAccess.Number(station, "Y").HasValue),
                    mdRange = Stats(stations.Select(Md)),
                    tvdRange = Stats(stations.Select(Tvd)),
                    inclinationRadians = Stats(stations.Select(station => JsonAccess.Number(station, "Inclination"))),
                    azimuthRadians = Stats(stations.Select(station => JsonAccess.Number(station, "Azimuth"))),
                    declaredDepthUnit = Text(node, "DepthUnit"), declaredDepthDatum = Text(node, "DepthDatum"),
                    declaredSpatialReference = Text(node, "SpatialReference"),
                    tieIn = Position(JsonAccess.Get(node, "TieInPoint")),
                    coverageLimitation = "Ranges and counts are not continuous path coverage. No MD-to-TVD interpolation or datum reconciliation is performed."
                };
            }).ToArray();
            var loggedBores = new HashSet<Guid>();
            var structuralBores = new HashSet<Guid>();
            var reservoirBores = new HashSet<Guid>();
            int intervalCount = 0, topCount = 0;
            object[] geology = package.GeologicalProperties.Select(node =>
            {
                ct.ThrowIfCancellationRequested();
                string evidenceId = Cite(node, EvidenceCatalog.Geology);
                Guid? boreId = JsonAccess.Guid(node, "WellBoreID");
                JsonNode? bore = boreId is Guid id ? bores.GetValueOrDefault(id) : null;
                Guid? wellId = JsonAccess.Guid(bore, "WellID");
                JsonNode? well = wellId is Guid wid ? wells.GetValueOrDefault(wid) : null;
                if (bore is null || well is null) _gaps.Add("A visible geology record has an unresolved wellbore/well association.");
                JsonNode? petrophysics = JsonAccess.Get(node, "Petrophysics");
                JsonArray intervals = Array(petrophysics, "FormationIntervals");
                JsonArray tops = Array(petrophysics, "FormationTops");
                JsonArray table = Array(node, "GeologicalPropertyTable");
                JsonArray runs = Array(petrophysics, "LogRuns");
                intervalCount += intervals.Count;
                topCount += tops.Count;
                if (intervalCount + topCount > 2048)
                    throw TooLarge("More than 2048 visible formation intervals/tops exceed the drafting budget; none were truncated.");
                Spend(table.Count * 4);
                object[] runSummaries = runs.Select(run =>
                {
                    JsonArray depths = Array(run, "DepthValues");
                    Spend(depths.Count);
                    JsonArray curves = Array(run, "Curves");
                    return (object)new
                    {
                        name = Text(run, "Name"), tool = Text(run, "Tool"), depthAxis = DepthAxis(JsonAccess.Get(run, "DepthAxis")),
                        depthCount = depths.Count, depthRange = Stats(depths.Select(Number)),
                        curves = new
                        {
                            columns = new[]
                            {
                                "originalMnemonic", "canonicalMnemonic", "originalUnit", "canonicalUnit", "classification",
                                "valueCount", "validValueCount", "missingOrRejectedValueCount", "nullFlagCount",
                                "qualityFlagEntryCount", "arraysAligned", "valueRange", "validDepthRange"
                            },
                            rows = curves.Select(curve =>
                            {
                                JsonArray values = Array(curve, "Values"), nulls = Array(curve, "NullFlags"), quality = Array(curve, "QualityFlags");
                                Spend(values.Count + nulls.Count + quality.Count);
                                double? At(int index) => UsableValue(values, nulls, quality, index);
                                int nonNull = Enumerable.Range(0, values.Count).Count(index => At(index).HasValue);
                                return new object?[]
                                {
                                    Text(curve, "OriginalMnemonic"), Text(curve, "CanonicalMnemonic"),
                                    Text(curve, "OriginalUnit"), Text(curve, "CanonicalUnit"), Classification(curve),
                                    values.Count, nonNull, values.Count - nonNull,
                                    nulls.Count(item => Boolean(item) == true), quality.Count(item => item is not null),
                                    values.Count == depths.Count && (nulls.Count == 0 || nulls.Count == values.Count) &&
                                        (quality.Count == 0 || quality.Count == values.Count),
                                    Stats(Enumerable.Range(0, values.Count).Select(At)),
                                    Stats(Enumerable.Range(0, Math.Min(values.Count, depths.Count))
                                        .Where(index => At(index).HasValue).Select(index => Number(depths[index])))
                                };
                            }).ToArray()
                        }
                    };
                }).ToArray();
                bool logged = table.Any(row => JsonAccess.GaussianMean(row, "Porosity").HasValue ||
                    JsonAccess.GaussianMean(row, "Permeability").HasValue) || runs.Any(run => Array(run, "Curves").Any(curve =>
                    {
                        JsonArray values = Array(curve, "Values"), nulls = Array(curve, "NullFlags"), quality = Array(curve, "QualityFlags");
                        return Enumerable.Range(0, values.Count).Any(index => UsableValue(values, nulls, quality, index).HasValue);
                    }));
                if (boreId is Guid linkedId && bore is not null)
                {
                    if (logged) loggedBores.Add(linkedId);
                    else if (intervals.Count + tops.Count > 0) structuralBores.Add(linkedId);
                    if (intervals.Concat(tops).Any(item => string.Equals(JsonAccess.String(item, "FormationName"),
                        request.Scope.ReservoirName, StringComparison.OrdinalIgnoreCase))) reservoirBores.Add(linkedId);
                }
                NetPayResult[] applied = analysis.WellSummaries.SelectMany(summary => summary.GeologicalResults)
                    .Where(result => result.EvidenceId == evidenceId).ToArray();
                if (table.Count == 0) _gaps.Add("A geology record has no petrophysics table; formation geometry or log curves alone do not imply analyzed expected paydirt.");
                return (object)new
                {
                    evidenceId, name = Text(node, "Name"),
                    wellBoreEvidenceId = bore is null ? null : EvidenceCatalog.TryCreateId(EvidenceCatalog.WellBore, bore),
                    wellBoreName = Text(bore, "Name"),
                    wellEvidenceId = well is null ? null : EvidenceCatalog.TryCreateId(EvidenceCatalog.Well, well),
                    wellName = Text(well, "Name"),
                    datasetClassification = Classification(JsonAccess.Get(petrophysics, "Provenance")),
                    hasLogData = logged,
                    formationIntervals = intervals.Select((interval, index) => new
                    {
                        recordIndex = index, formationName = Text(interval, "FormationName"), topDepth = Depth(JsonAccess.Get(interval, "TopDepth")),
                        baseDepth = Depth(JsonAccess.Get(interval, "BaseDepth")), confidence = JsonAccess.Number(interval, "Confidence"),
                        method = Text(interval, "Method"), classification = Classification(interval)
                    }).ToArray(),
                    formationTops = tops.Select((top, index) => new
                    {
                        recordIndex = index, formationName = Text(top, "FormationName"), depths = Array(top, "Depths").Select(Depth).ToArray(),
                        confidence = JsonAccess.Number(top, "Confidence"), method = Text(top, "Method"), classification = Classification(top)
                    }).ToArray(),
                    logRuns = runSummaries,
                    petrophysicsTableRowCount = table.Count,
                    petrophysicsTableSummary = table.Count == 0 ? null : new
                    {
                        rowCount = table.Count,
                        completeSampleCount = table.Count(row => new[] { "MeasuredDepth", "Porosity", "Permeability", "PressureDifferential" }
                            .All(key => JsonAccess.GaussianMean(row, key).HasValue)),
                        md = Stats(table.Select(row => JsonAccess.GaussianMean(row, "MeasuredDepth"))),
                        porosityFraction = Stats(table.Select(row => JsonAccess.GaussianMean(row, "Porosity"))),
                        permeabilityM2 = Stats(table.Select(row => JsonAccess.GaussianMean(row, "Permeability"))),
                        pressureDifferential = Stats(table.Select(row => JsonAccess.GaussianMean(row, "PressureDifferential")))
                    },
                    appliedGeologyResults = applied,
                    analysisInclusion = applied.Length > 0 ? "included in located well screening" : "not included in located well screening"
                };
            }).ToArray();
            structuralBores.ExceptWith(loggedBores);
            if (intervalCount + topCount == 0) _gaps.Add("No formation intervals or tops are supplied; a formation correlation is not established.");
            _gaps.Add("Null or omitted metadata, including depth reference, unit, datum, method, confidence and classification, means undeclared, not a default.");
            _gaps.Add("Survey ranges do not establish coverage at each formation; do not convert MD to TVD or correlate across unknown datums.");
            _gaps.Add("Per-bore log counts describe any visible log data, not completeness or hydrocarbon pay. Water-bearing rock can satisfy the screening cutoffs.");
            string fieldId = EvidenceCatalog.CreateId(EvidenceCatalog.Field, package.FieldId);
            Citations.Add(fieldId);
            foreach (HypothesisControlNote note in request.Notes.ControlNotes)
                if (!Citations.Contains(note.EvidenceId))
                    throw HypothesisValidation.Invalid("Control notes must cite evidence actually supplied in the selected visible drafting context.");
            if (analysis.WellSummaries.Any(summary => !Citations.Contains(summary.WellEvidenceId) ||
                summary.GeologyEvidenceIds.Any(id => !Citations.Contains(id))) ||
                selected?.Prediction?.NeighborEvidenceIds.Any(id => !Citations.Contains(id)) == true)
                throw new ScenarioApiException(409, "Unresolved analysis evidence", "Analysis references are not all present in the supplied visible context.");
            return new
            {
                version = "formation-interpretation-evidence-v1",
                request.Scope, request.SavedHypothesis, request.SnapshotSha256,
                packageSha256 = package.Sha256, analysisSha256 = analysis.AnalysisSha256, configurationSha256 = analysis.ConfigurationSha256,
                evidenceTime = package.GeneratedAt,
                aggregation = "Complete visible formation records and source identities. Each log run has a curves table: rows use the exact order in columns; ranges/counts summarize all samples. Null-flagged and missing/invalid/bad-hole values are excluded from valid counts/ranges. Raw curves, source URLs, external datasets, hidden truth and unsupplied fields are not provided.",
                numericRangeFields = new[] { "validCount", "minimum", "maximum" },
                numericRangeMeaning = "Every numeric range is a three-cell array in numericRangeFields order. Zero validCount with null minimum/maximum means no usable values, not a zero-valued measurement.",
                petrophysicsTableMeaning = "Summaries include all table rows, not restricted to the selected reservoir. Gaussian means only; not raw logs. Applied results use the exact analyzer's reservoir selection and cutoffs.",
                field = new
                {
                    evidenceId = fieldId, name = Text(package.Field, "Name"), referencePoint = Position(JsonAccess.Get(package.Field, "ReferencePoint")),
                    hasProjectionDefinitionReference = JsonAccess.Guid(package.Field, "ProjectionDefinitionID").HasValue,
                    spatialReferenceLimitation = "A projection reference is not its definition. No CRS/datum lookup or transformation was performed. Screened easting/northing are the analyzer's local metric coordinates, not verified map-grid coordinates."
                },
                actualVisibleCounts = new
                {
                    clusters = clusters.Length, wells = wellRecords.Length, wellBores = boreRecords.Length,
                    architectures = architecture.Length, trajectories = surveys.Length, geologyRecords = geology.Length,
                    formationIntervals = intervalCount, formationTops = topCount,
                    boresWithAnyLogData = loggedBores.Count, structuralOnlyBores = structuralBores.Count,
                    selectedReservoirIntersectingBores = reservoirBores.Count,
                    selectedReservoirIntersectingBoresWithAnyLogData = reservoirBores.Count(loggedBores.Contains),
                    selectedReservoirStructuralOnlyBores = reservoirBores.Count(structuralBores.Contains),
                    locatedScreeningWells = analysis.WellSummaries.Count
                },
                clusters, wells = wellRecords, wellBores = boreRecords, architecture, surveys, geology,
                appliedAnalysis = new
                {
                    analysis.ModelVersion, analysis.Configuration, analysis.Methodology, analysis.CandidateGridBounds,
                    analysis.WellSummaries, selectedCandidate = selected,
                    selectedCandidateLimitation = selected is null ? "No target selected; do not invent target estimates." : "Model-estimated expected paydirt, not reserves or observed thickness.",
                    gridPointCount = analysis.CandidateGrid.Count,
                    eligiblePointCount = analysis.CandidateGrid.Count(point => point.Status == "eligible")
                },
                missingData = package.DataGaps.Concat(analysis.DataGaps).Concat(_gaps).Distinct(StringComparer.Ordinal)
                    .Select(gap => BoundedText(gap)).ToArray(),
                citationAllowlist = Citations.Order(StringComparer.Ordinal).ToArray(),
                humanNotesUntrusted = request.Notes
            };
        }

        private string Cite(JsonNode node, string kind)
        {
            string id = EvidenceCatalog.TryCreateId(kind, node) ?? throw MissingId();
            Citations.Add(id);
            return id;
        }

        private void Spend(int count)
        {
            ct.ThrowIfCancellationRequested();
            _sampleValues += count;
            if (_sampleValues > MaximumSampleValues)
                throw TooLarge("The log/survey aggregation budget is two million sample values. No samples were silently truncated.");
        }
    }

    private static object? Position(JsonNode? node) => node is null ? null : new
    {
        easting = JsonAccess.Number(node, "RiemannianEast"), northing = JsonAccess.Number(node, "RiemannianNorth"),
        x = JsonAccess.Number(node, "X"), y = JsonAccess.Number(node, "Y"), z = JsonAccess.Number(node, "Z"),
        longitudeRadians = JsonAccess.Number(node, "Longitude"), latitudeRadians = JsonAccess.Number(node, "Latitude"),
        md = Md(node), tvd = Tvd(node),
        declaredUnit = Text(node, "Unit"), declaredDatum = Text(node, "Datum"),
        declaredVerticalDatum = Text(node, "VerticalDatum")
    };

    private static object? Depth(JsonNode? node) => node is null ? null : new
    {
        reference = EnumText(node, "Reference", ["MeasuredDepth", "TrueVerticalDepth", "TrueVerticalDepthSubsea"]),
        value = JsonAccess.Number(node, "Value"), unit = Text(node, "Unit"), datum = Text(node, "Datum"),
        positiveDown = Boolean(JsonAccess.Get(node, "PositiveDown")),
        originalValue = JsonAccess.Number(node, "OriginalValue"), originalUnit = Text(node, "OriginalUnit")
    };

    private static object? DepthAxis(JsonNode? node) => node is null ? null : new
    {
        reference = EnumText(node, "Reference", ["MeasuredDepth", "TrueVerticalDepth", "TrueVerticalDepthSubsea"]),
        originalUnit = Text(node, "OriginalUnit"), canonicalUnit = Text(node, "CanonicalUnit"),
        datum = Text(node, "Datum"), positiveDown = Boolean(JsonAccess.Get(node, "PositiveDown"))
    };

    private static string? Classification(JsonNode? node, bool well = false) => EnumText(node, "Classification", well
        ? ["Observed", "Derived", "ModelEstimated", "Synthetic"]
        : ["Observed", "HumanInterpreted", "Derived", "ModelEstimated", "Synthetic"]);

    private static string? EnumText(JsonNode? node, string property, string[] values)
    {
        string? text = Text(node, property);
        if (text is not null) return values.FirstOrDefault(value => string.Equals(value, text, StringComparison.OrdinalIgnoreCase)) ?? "unrecognized";
        double? number = JsonAccess.Number(node, property);
        return number is null ? null : number >= 0 && number < values.Length && number == Math.Truncate(number.Value)
            ? values[(int)number.Value] : "unrecognized";
    }

    private static JsonArray Array(JsonNode? node, string property)
    {
        if (JsonAccess.Get(node, property) is not JsonArray array) return [];
        if (array.Count > MaximumArrayItems)
            throw TooLarge("A visible evidence array exceeds 100000 entries. No records or samples were silently truncated.");
        return array;
    }

    private static object?[] Stats(IEnumerable<double?> values)
    {
        int count = 0;
        double min = double.PositiveInfinity, max = double.NegativeInfinity;
        foreach (double? value in values)
        {
            if (value is not double number || !double.IsFinite(number)) continue;
            count++;
            min = Math.Min(min, number);
            max = Math.Max(max, number);
        }
        return [count, count == 0 ? null : min, count == 0 ? null : max];
    }

    private static double? Number(JsonNode? node)
    {
        if (node is not JsonValue value || value.GetValueKind() != JsonValueKind.Number) return null;
        return double.TryParse(value.ToJsonString(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out double number) && double.IsFinite(number) ? number : null;
    }
    private static bool? Boolean(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out bool flag) ? flag : null;
    private static double? UsableValue(JsonArray values, JsonArray nulls, JsonArray quality, int index)
    {
        if (Boolean(nulls.ElementAtOrDefault(index)) == true) return null;
        string? flag = quality.ElementAtOrDefault(index) is JsonValue value && value.TryGetValue<string>(out string? text) ? text : null;
        if (flag is not null && System.Text.RegularExpressions.Regex.IsMatch(flag, "missing|bad.?hole|invalid",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant))
            return null;
        return Number(values[index]);
    }
    private static double? Md(JsonNode? node) => JsonAccess.Number(node, "MD") ?? JsonAccess.Number(node, "Abscissa");
    private static double? Tvd(JsonNode? node) => JsonAccess.Number(node, "TVD") ?? JsonAccess.Number(node, "VerticalDepth");
    private static string? Text(JsonNode? node, string key) => BoundedText(JsonAccess.String(node, key));
    private static string? BoundedText(string? value)
    {
        if (value is not null && (value.Length > 2000 || value.Any(c => char.IsControl(c) && c is not '\n' and not '\r' and not '\t')))
            throw TooLarge("A visible evidence label exceeds the bounded text contract. No labels were silently truncated.");
        return value;
    }
    private static ScenarioApiException MissingId() =>
        new(409, "Uncitable visible evidence", "A visible source record has no stable evidence ID. Correct the source before requesting a cited interpretation.");
    private static ScenarioApiException TooLarge(string detail) => new(413, "Formation interpretation evidence too large", detail);
}
