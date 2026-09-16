namespace ReservoirSimulation.Contracts;

public sealed record WorldGenerationRequest
{
    public Guid FieldId { get; init; }
    public string ReservoirName { get; init; } = string.Empty;
    public int Seed { get; init; }
    public CalibrationArtifactReference CalibrationArtifact { get; init; } = new();
    public GridOptions Grid { get; init; } = new();
    public HeterogeneityOptions Heterogeneity { get; init; } = new();
    public IReadOnlyList<StructuralConditioningPoint> StructuralConditioningPoints { get; init; } = [];
    public IReadOnlyList<ConditioningPoint> ConditioningPoints { get; init; } = [];
    public FluidContactOptions FluidContacts { get; init; } = new();
}

public sealed record CalibrationArtifactReference
{
    public string Id { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}


public sealed record GridOptions
{
    public int CountX { get; init; } = 64;
    public int CountY { get; init; } = 64;
    public int CountZ { get; init; } = 20;
    public double HorizontalPaddingM { get; init; } = 500;
}

public sealed record HeterogeneityOptions
{
    public double IdwPower { get; init; } = 2;
    public int SpectralModeCount { get; init; } = 24;
    public double CorrelationLengthXM { get; init; } = 1_400;
    public double CorrelationLengthYM { get; init; } = 1_400;
    public double CorrelationLengthZM { get; init; } = 12;
    public double ControlFadeDistanceM { get; init; } = 1_200;
    public double TopDepthStdDevM { get; init; } = 4;
    public double BaseDepthStdDevM { get; init; } = 4;
    public double PorosityStdDev { get; init; } = 0.025;
    public double LogPermeabilityStdDev { get; init; } = 0.7;
    public double PressureStdDevPa { get; init; } = 500_000;
    public double WaterSaturationStdDev { get; init; } = 0.025;
    public double GasSaturationStdDev { get; init; } = 0.015;
    public double NetToGrossStdDev { get; init; }
    public double ShalePorosity { get; init; } = 0.05;
    public double ShalePermeabilityM2 { get; init; } = 1e-20;
}

public sealed record StructuralConditioningPoint
{
    public double EastingM { get; init; }
    public double NorthingM { get; init; }
    public double ReservoirTopDepthM { get; init; }
    public double ReservoirBaseDepthM { get; init; }
}

public sealed record FluidContactPoint
{
    public double EastingM { get; init; }
    public double NorthingM { get; init; }
    public double ContactDepthTvdM { get; init; }
}

public sealed record FluidContactOptions
{
    public double TransitionThicknessM { get; init; } = 2;
    public IReadOnlyList<FluidContactPoint> GasWater { get; init; } = [];
    public IReadOnlyList<FluidContactPoint> GasOil { get; init; } = [];
    public IReadOnlyList<FluidContactPoint> OilWater { get; init; } = [];
}


public sealed record ConditioningPoint
{
    public double EastingM { get; init; }
    public double NorthingM { get; init; }
    public double ReservoirTopDepthM { get; init; }
    public double ReservoirBaseDepthM { get; init; }
    public double Porosity { get; init; }
    public double PermeabilityM2 { get; init; }
    public double PressurePa { get; init; }
    public double WaterSaturation { get; init; }
    public double GasSaturation { get; init; }
    public double NetToGross { get; init; } = 1;
}

public sealed record ScalarRange(double Minimum, double Maximum);

public sealed record WorldGridSummary(
    int CountX,
    int CountY,
    int CountZ,
    int CellCount);

public sealed record WorldSummary(
    string WorldId,
    Guid FieldId,
    string ReservoirName,
    string ModelLabel,
    string ModelVersion,
    CalibrationArtifactReference CalibrationArtifact,
    WorldGridSummary Grid,
    int ConditioningControlCount,
    int StructuralControlCount);
