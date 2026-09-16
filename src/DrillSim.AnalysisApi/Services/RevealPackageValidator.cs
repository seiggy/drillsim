using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Infrastructure;

internal static class RevealPackageValidator
{
    private static readonly string[] ForbiddenNames =
    [
        "truthsample", "productiontruth", "stageapath", "completionbinding",
        "worldid", "bindingmetadata", "gridcell", "ijk", "oilSaturation",
        "waterSaturation", "gasSaturation", "trueVerticalDepthM"
    ];

    public static AnalysisPackage Validate(ValidatedReveal reveal, AnalysisPackage? package)
    {
        if (package is null)
            throw Invalid("clonePackage", "is required.");
        if (package.FieldId != reveal.ClonedFieldId ||
            JsonAccess.MetaId(package.Field) != reveal.ClonedFieldId)
            throw Invalid("clonePackage.fieldId", "must match clonedFieldId.");
        if (package.GeneratedAt.ToUniversalTime() != reveal.ValidTimeUtc)
            throw Invalid("clonePackage.generatedAt", "must equal validTimeUtc.");

        try
        {
            SqliteScenarioStore.ValidatePackageIntegrity(package, reveal.ClonedFieldId);
        }
        catch (InvalidDataException exception)
        {
            throw Invalid("clonePackage", exception.Message);
        }

        string[] gaps = ComputeDataGaps(package);
        if (!package.DataGaps.SequenceEqual(gaps, StringComparer.Ordinal))
            throw Invalid("clonePackage.dataGaps", "does not match the observable package graph.");

        Dictionary<string, (string Kind, JsonNode Record)> records = Enumerate(package);
        Dictionary<string, ValidatedRevealEvidence> evidence = reveal.Evidence
            .ToDictionary(item => item.EvidenceId, StringComparer.Ordinal);
        if (records.Count != evidence.Count || !records.Keys.ToHashSet(StringComparer.Ordinal)
                .SetEquals(evidence.Keys))
            throw Invalid("clonePackage", "records do not exactly match reveal evidence.");

        foreach ((string id, (string kind, JsonNode record)) in records)
        {
            ValidatedRevealEvidence commitment = evidence[id];
            if (!string.Equals(commitment.RecordKind, kind, StringComparison.Ordinal) ||
                !string.Equals(BusinessContentHash(record), commitment.ContentSha256, StringComparison.Ordinal))
                throw Invalid("clonePackage", $"record '{id}' does not match its committed kind and business hash.");
            if (ContainsForbiddenProperty(record))
                throw Invalid("clonePackage", $"record '{id}' contains forbidden hidden data.");
        }

        ValidateGraph(package);
        ValidateProductionSeries(reveal, package);
        string canonical = PredictionJson.Canonicalize(package);
        return PredictionJson.Deserialize<AnalysisPackage>(canonical, "clone package snapshot");
    }

    private static void ValidateProductionSeries(ValidatedReveal reveal, AnalysisPackage package)
    {
        var representations = new List<JsonArray>();
        foreach (JsonNode well in package.Wells)
        {
            if (well is not JsonObject wellObject ||
                !TryProperty(wellObject, "Dataset", out JsonNode? datasetNode) ||
                datasetNode is not JsonObject dataset ||
                !TryProperty(dataset, "MonthlyProduction", out JsonNode? productionNode))
                continue;
            if (productionNode is null || productionNode.GetValueKind() == JsonValueKind.Null)
                continue;
            if (productionNode is not JsonArray production)
                throw Invalid("clonePackage", "monthly production representation must be an array.");
            representations.Add(production);
        }

        if (representations.Count != 1)
            throw Invalid("clonePackage", "must contain exactly one monthly production representation.");
        JsonArray months = representations[0];
        if (months.Count != 60)
            throw Invalid("clonePackage", "monthly production representation must contain exactly 60 ordered months.");
        if (!string.Equals(
                BusinessContentHash(months),
                reveal.ProductionSeries.ContentSha256,
                StringComparison.Ordinal))
            throw Invalid("productionSeries.contentSha256", "does not match the clone package monthly production representation.");
    }

    private static bool TryProperty(JsonObject value, string name, out JsonNode? node)
    {
        KeyValuePair<string, JsonNode?> property = value.FirstOrDefault(
            item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
        node = property.Value;
        return property.Key is not null;
    }

    private static Dictionary<string, (string Kind, JsonNode Record)> Enumerate(AnalysisPackage package)
    {
        var result = new Dictionary<string, (string, JsonNode)>(StringComparer.Ordinal)
        {
            [EvidenceCatalog.CreateId(EvidenceCatalog.Field, package.FieldId)] =
                (EvidenceCatalog.Field, package.Field)
        };
        Add(EvidenceCatalog.Cluster, package.Clusters);
        Add(EvidenceCatalog.Well, package.Wells);
        Add(EvidenceCatalog.WellBore, package.WellBores);
        Add(EvidenceCatalog.Architecture, package.WellBoreArchitectures);
        Add(EvidenceCatalog.Trajectory, package.Trajectories);
        Add(EvidenceCatalog.Geology, package.GeologicalProperties);
        return result;

        void Add(string kind, IReadOnlyList<JsonNode> items)
        {
            foreach (JsonNode item in items)
            {
                Guid id = JsonAccess.MetaId(item)
                    ?? throw Invalid("clonePackage", $"{kind} record has no identity.");
                if (!result.TryAdd(EvidenceCatalog.CreateId(kind, id), (kind, item)))
                    throw Invalid("clonePackage", "contains duplicate record identities.");
            }
        }
    }

    private static void ValidateGraph(AnalysisPackage package)
    {
        HashSet<Guid> fieldIds = [package.FieldId];
        Dictionary<Guid, JsonNode> clusters = ById(package.Clusters, "cluster");
        Dictionary<Guid, JsonNode> wells = ById(package.Wells, "well");
        Dictionary<Guid, JsonNode> bores = ById(package.WellBores, "wellbore");
        Dictionary<Guid, JsonNode> trajectories = ById(package.Trajectories, "trajectory");

        bool invalid = package.Clusters.Any(item => !fieldIds.Contains(RequiredGuid(item, "FieldID"))) ||
            package.Wells.Any(item => !clusters.ContainsKey(RequiredGuid(item, "ClusterID"))) ||
            package.WellBores.Any(item => !wells.ContainsKey(RequiredGuid(item, "WellID"))) ||
            package.WellBoreArchitectures.Any(item => !bores.ContainsKey(RequiredGuid(item, "WellBoreID")));

        foreach (JsonNode trajectory in package.Trajectories)
        {
            Guid field = RequiredGuid(trajectory, "FieldID");
            Guid cluster = RequiredGuid(trajectory, "ClusterID");
            Guid well = RequiredGuid(trajectory, "WellID");
            Guid bore = RequiredGuid(trajectory, "WellBoreID");
            invalid |= !fieldIds.Contains(field) || !clusters.ContainsKey(cluster) ||
                !wells.ContainsKey(well) || !bores.ContainsKey(bore) ||
                RequiredGuid(wells.GetValueOrDefault(well), "ClusterID") != cluster ||
                RequiredGuid(bores.GetValueOrDefault(bore), "WellID") != well;
        }

        foreach (JsonNode geology in package.GeologicalProperties)
        {
            Guid bore = RequiredGuid(geology, "WellBoreID");
            invalid |= !bores.ContainsKey(bore);
            Guid? trajectory = OptionalGuid(geology, "TrajectoryID");
            invalid |= trajectory is Guid id &&
                (!trajectories.TryGetValue(id, out JsonNode? owner) ||
                 RequiredGuid(owner, "WellBoreID") != bore);
        }

        if (invalid)
            throw Invalid("clonePackage", "contains foreign or dangling graph references.");
    }

    private static Dictionary<Guid, JsonNode> ById(IReadOnlyList<JsonNode> records, string kind)
    {
        try
        {
            return records.ToDictionary(
                item => JsonAccess.MetaId(item)
                    ?? throw Invalid("clonePackage", $"{kind} record has no identity."));
        }
        catch (ArgumentException)
        {
            throw Invalid("clonePackage", $"contains duplicate {kind} identities.");
        }
    }

    private static Guid RequiredGuid(JsonNode? node, string name) =>
        JsonAccess.Guid(node ?? throw Invalid("clonePackage", $"record is missing {name}."), name)
        ?? throw Invalid("clonePackage", $"record has an invalid {name}.");

    private static Guid? OptionalGuid(JsonNode node, string name)
    {
        if (node is not JsonObject value)
            throw Invalid("clonePackage", "record must be an object.");
        KeyValuePair<string, JsonNode?> property = value.FirstOrDefault(
            item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (property.Key is null || property.Value is null)
            return null;
        return property.Value.GetValueKind() == JsonValueKind.Null
            ? null
            : Guid.TryParse(property.Value.GetValue<string>(), out Guid id) && id != Guid.Empty
                ? id
                : throw Invalid("clonePackage", $"record has an invalid {name}.");
    }

    internal static string BusinessContentHash(JsonNode value)
    {
        using var document = JsonDocument.Parse(value.ToJsonString());
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            WriteBusiness(document.RootElement, writer);
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    private static void WriteBusiness(JsonElement value, Utf8JsonWriter writer)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (JsonProperty property in value.EnumerateObject()
                    .Where(item => !IsServerOwned(item.Name))
                    .OrderBy(item => item.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteBusiness(property.Value, writer);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (JsonElement item in value.EnumerateArray())
                    WriteBusiness(item, writer);
                writer.WriteEndArray();
                break;
            default:
                value.WriteTo(writer);
                break;
        }
    }

    private static bool IsServerOwned(string name) =>
        name.Equals("CreationDate", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("LastModificationDate", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsForbiddenProperty(JsonNode value)
    {
        if (value is JsonObject obj)
        {
            foreach ((string name, JsonNode? child) in obj)
            {
                if (ForbiddenNames.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    child is not null && ContainsForbiddenProperty(child))
                    return true;
            }
        }
        else if (value is JsonArray array && array.Any(item => item is not null && ContainsForbiddenProperty(item)))
        {
            return true;
        }
        return false;
    }

    internal static string[] ComputeDataGaps(AnalysisPackage package)
    {
        var gaps = new List<string>();
        if (package.Trajectories.Count == 0) gaps.Add("No trajectories were returned for the field.");
        if (package.Clusters.Count == 0) gaps.Add("No clusters were returned for the field.");
        if (package.Clusters.Count > 0 && package.Wells.Count == 0) gaps.Add("No wells were returned for the field clusters.");
        if (package.Wells.Count > 0 && package.WellBores.Count == 0) gaps.Add("No wellbores were returned for the field wells.");
        HashSet<Guid> boreIds = package.WellBores.Select(JsonAccess.MetaId).OfType<Guid>().ToHashSet();
        if (package.WellBores.Count > 0 && package.WellBoreArchitectures.Count == 0) gaps.Add("No wellbore architecture matched the included wellbores.");
        int withoutArchitecture = boreIds.Count - package.WellBoreArchitectures
            .Select(item => JsonAccess.Guid(item, "WellBoreID")).OfType<Guid>().Where(boreIds.Contains).Distinct().Count();
        if (withoutArchitecture > 0) gaps.Add($"{withoutArchitecture} included wellbore(s) have no architecture record.");
        if (package.WellBores.Count > 0 && package.GeologicalProperties.Count == 0) gaps.Add("No geological properties matched the included wellbores.");
        HashSet<Guid> geologyBores = package.GeologicalProperties.Select(item => JsonAccess.Guid(item, "WellBoreID")).OfType<Guid>().ToHashSet();
        int withoutGeology = boreIds.Count(id => !geologyBores.Contains(id));
        if (withoutGeology > 0) gaps.Add($"{withoutGeology} included wellbore(s) had no geological-properties record.");
        HashSet<Guid> trajectoryBores = package.Trajectories.Select(item => JsonAccess.Guid(item, "WellBoreID")).OfType<Guid>().ToHashSet();
        int withoutTrajectory = boreIds.Count(id => !trajectoryBores.Contains(id));
        if (withoutTrajectory > 0) gaps.Add($"{withoutTrajectory} included wellbore(s) have no survey trajectory.");
        return gaps.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    private static ScenarioApiException Invalid(string field, string detail) =>
        new(StatusCodes.Status400BadRequest, "Invalid reveal manifest", $"{field}: {detail}");
}
