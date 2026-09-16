using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Infrastructure;

internal sealed record EvidenceDescriptor(string EvidenceId, string RecordKind);

internal static class EvidenceCatalog
{
    public const string Field = "field";
    public const string Cluster = "cluster";
    public const string Well = "well";
    public const string WellBore = "wellbore";
    public const string Architecture = "architecture";
    public const string Trajectory = "trajectory";
    public const string Geology = "geology";

    public static IReadOnlyList<EvidenceDescriptor> Enumerate(AnalysisPackage package)
    {
        var items = new List<EvidenceDescriptor>
        {
            new(CreateId(Field, package.FieldId), Field)
        };
        Add(items, Cluster, package.Clusters);
        Add(items, Well, package.Wells);
        Add(items, WellBore, package.WellBores);
        Add(items, Architecture, package.WellBoreArchitectures);
        Add(items, Trajectory, package.Trajectories);
        Add(items, Geology, package.GeologicalProperties);

        return items
            .DistinctBy(item => item.EvidenceId, StringComparer.Ordinal)
            .OrderBy(item => KindOrder(item.RecordKind))
            .ThenBy(item => item.EvidenceId, StringComparer.Ordinal)
            .ToArray();
    }

    public static bool IsKnownKind(string kind) => kind is
        Field or Cluster or Well or WellBore or Architecture or Trajectory or Geology;

    public static bool TryMapPublicationKind(string publicationKind, out string evidenceKind)
    {
        evidenceKind = publicationKind switch
        {
            "Field" => Field,
            "Cluster" => Cluster,
            "Well" => Well,
            "WellBore" => WellBore,
            "WellBoreArchitecture" => Architecture,
            "Trajectory" => Trajectory,
            "GeologicalProperties" => Geology,
            _ => string.Empty
        };
        return evidenceKind.Length != 0;
    }

    public static string CreateId(string kind, Guid id) =>
        $"{kind}:{id:D}";

    public static string? TryCreateId(string kind, JsonNode item) =>
        JsonAccess.MetaId(item) is Guid id ? CreateId(kind, id) : null;

    public static int KindOrder(string kind) => kind switch
    {
        Field => 0,
        Cluster => 1,
        Well => 2,
        WellBore => 3,
        Architecture => 4,
        Trajectory => 5,
        Geology => 6,
        _ => int.MaxValue
    };

    private static void Add(List<EvidenceDescriptor> destination, string kind, IReadOnlyList<JsonNode> items)
    {
        foreach (JsonNode item in items)
            if (TryCreateId(kind, item) is string evidenceId)
                destination.Add(new EvidenceDescriptor(evidenceId, kind));
    }
}

