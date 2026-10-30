using System.Text.Json.Serialization;

namespace DrillSim.AnalysisApi.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record StaticReservoirModelRequest(
    [property: JsonRequired] AnalysisConfiguration Configuration,
    Guid? ScenarioId = null,
    DateTimeOffset? AsOf = null,
    string? Reservoir = null,
    IReadOnlyList<ReservoirModelTarget>? Targets = null);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
[JsonNumberHandling(JsonNumberHandling.Strict)]
public sealed record ReservoirModelTarget(
    [property: JsonRequired] string Id,
    [property: JsonRequired] double EastingM,
    [property: JsonRequired] double NorthingM);

/// <summary>Visible-evidence static model: structure from picks, properties from logs, fluids from field-wide contacts.</summary>
public sealed record StaticReservoirModel(
    string Method,
    Guid FieldId,
    string ReservoirName,
    string PackageSha256,
    string ConfigurationSha256,
    ReservoirModelGrid Grid,
    IReadOnlyList<FluidContactEstimate> Contacts,
    ReservoirModelVolumes Volumes,
    ReservoirModelInputs Inputs,
    IReadOnlyList<ReservoirModelWell> Wells,
    IReadOnlyList<ReservoirModelColumn> Targets,
    IReadOnlyList<string> Limitations);

/// <summary>Column summaries at grid nodes, row-major (index = row * Columns + column; row runs north).</summary>
public sealed record ReservoirModelGrid(
    double MinEastingM,
    double MinNorthingM,
    double CellSizeM,
    int Columns,
    int Rows,
    int Layers,
    double PaddingM,
    double[] TopTvdM,
    double[] BaseTvdM,
    double[] NetRockM,
    double[] GasM,
    double[] OilM,
    double[] WaterM,
    double[] Porosity);

/// <param name="Type">GasOilContact, OilWaterContact or GasWaterContact.</param>
/// <param name="DeepestAboveTvdM">Deepest classified sample of the upper fluid that supports the contact.</param>
/// <param name="ShallowestBelowTvdM">Shallowest classified sample of the lower fluid that supports the contact.</param>
public sealed record FluidContactEstimate(
    string Type,
    double TrueVerticalDepthM,
    double DeepestAboveTvdM,
    double ShallowestBelowTvdM,
    int SupportingSamples,
    int MisclassifiedSamples,
    int Wells);

public sealed record ReservoirModelVolumes(
    double GrossRockM3,
    double NetRockM3,
    double GasNetRockM3,
    double OilNetRockM3,
    double HydrocarbonPoreVolumeM3);

public sealed record ReservoirModelInputs(
    int StructuralControls,
    int PropertyWells,
    int FluidWells,
    int ClassifiedSamples,
    double MedianGrossM,
    string Hydrocarbon);

public sealed record ReservoirModelWell(
    string WellBoreId,
    string? WellId,
    string Name,
    bool Control,
    bool Logged,
    double[][] Path,
    double? TopTvdM,
    double? BaseTvdM,
    double? PickEastingM,
    double? PickNorthingM);

public sealed record ReservoirModelColumn(
    string Id,
    double EastingM,
    double NorthingM,
    bool InsideModel,
    double TopTvdM,
    double BaseTvdM,
    double GrossM,
    double NetRockM,
    double GasM,
    double OilM,
    double WaterM,
    double NetToGross,
    double Porosity,
    double PermeabilityMd,
    string FluidAtTop,
    double NearestControlM,
    double NearestPropertyWellM,
    IReadOnlyList<string> PropertyEvidenceIds);
