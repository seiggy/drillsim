using System.Text.Json.Serialization;

namespace ReservoirSimulation.Contracts;

public static class KernelMetadata
{
    public const string Name = "Immiscible three-phase black-oil IMPES";
    public const string Limitation = "First kernel only: immiscible oil, water, and free gas share one mobility-weighted pressure initialized with kernel-default PVT; independent capillary phase pressures are not represented, and there is no compositional or solution-gas behavior. Completion production starts from the unchanged hidden initial state; post-drilling state coupling is not represented.";
}

public sealed record SimulationRequest
{
    public double DurationSeconds { get; init; } = 86_400;
    public double InitialTimeStepSeconds { get; init; } = 3_600;
    public SolverOptions Solver { get; init; } = new();
    public FluidModelOptions Fluids { get; init; } = new();
    public IReadOnlyList<WellControl> Wells { get; init; } = [];
    public IReadOnlyList<ScheduleSegment> Schedule { get; init; } = [];
    public string? ContinueFromStateId { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SolverOptions
{
    public double MinimumTimeStepSeconds { get; init; } = 1;
    public double MaximumTimeStepSeconds { get; init; } = 86_400;
    public double MaximumSaturationChange { get; init; } = 0.05;
    public double GrowthSaturationChange { get; init; } = 0.0125;
    public double TimeStepGrowthFactor { get; init; } = 1.5;
    public double TimeStepShrinkFactor { get; init; } = 0.5;
    public double CgRelativeTolerance { get; init; } = 1e-8;
    public int CgMaximumIterations { get; init; } = 1_000;
    public int MaximumStepAttempts { get; init; } = 100_000;
    public int MaximumSamplesPerWell { get; init; } = 1_000;
}

public sealed record FluidModelOptions
{
    public double ReferencePressurePa { get; init; } = 20_000_000;
    public double OilDensityKgPerM3 { get; init; } = 800;
    public double WaterDensityKgPerM3 { get; init; } = 1_000;
    public double GasDensityKgPerM3 { get; init; } = 100;
    public double OilViscosityPaS { get; init; } = 0.003;
    public double WaterViscosityPaS { get; init; } = 0.001;
    public double GasViscosityPaS { get; init; } = 0.00002;
    /// <summary>Isothermal phase compressibility in inverse pascals.</summary>
    public double OilCompressibilityPerPa { get; init; } = 1e-9;
    public double WaterCompressibilityPerPa { get; init; } = 4e-10;
    public double GasCompressibilityPerPa { get; init; } = 1e-8;
    public double RockCompressibilityPerPa { get; init; } = 5e-10;
    public double ResidualOilSaturation { get; init; } = 0.10;
    public double ResidualWaterSaturation { get; init; } = 0.15;
    public double ResidualGasSaturation { get; init; } = 0.05;
    public double OilCoreyExponent { get; init; } = 2;
    public double WaterCoreyExponent { get; init; } = 2;
    public double GasCoreyExponent { get; init; } = 2;
    public double OilRelativePermeabilityEndPoint { get; init; } = 1;
    public double WaterRelativePermeabilityEndPoint { get; init; } = 0.7;
    public double GasRelativePermeabilityEndPoint { get; init; } = 0.8;
    public double GravityMPerS2 { get; init; } = 9.80665;
}

public enum WellControlMode
{
    Rate,
    Bhp
}

public sealed record ScheduleSegment
{
    public double StartTimeSeconds { get; init; }
    public double DurationSeconds { get; init; }
    public IReadOnlyList<WellControl> Wells { get; init; } = [];
}

/// <summary>Vertical Peaceman connection; WI uses Kx/Ky, Dx/Dy, cell height, radius, skin, and open fraction.</summary>
public sealed record WellConnection
{
    public int I { get; init; }
    public int J { get; init; }
    public int K { get; init; }
    public double WellboreRadiusM { get; init; } = 0.1;
    public double Skin { get; init; }
    public double OpenFraction { get; init; } = 1;
}


public sealed record WellControl
{
    public WellControlMode ControlMode { get; init; } = WellControlMode.Rate;
    public double TargetBottomHolePressurePa { get; init; }
    public double? MinimumBottomHolePressurePa { get; init; }
    public double? MaximumBottomHolePressurePa { get; init; }
    public double? MaximumAbsoluteRateM3PerSecond { get; init; }
    public IReadOnlyList<WellConnection> Connections { get; init; } = [];
    public string Name { get; init; } = string.Empty;
    public int I { get; init; }
    public int J { get; init; }
    public int KStart { get; init; }
    public int KEnd { get; init; }
    /// <summary>Reservoir-volume rate in m3/s; positive injects and negative produces.</summary>
    public double TotalRateM3PerSecond { get; init; }
    /// <summary>Injected water fraction; used only for a positive total rate.</summary>
    public double InjectionWaterFraction { get; init; }
    /// <summary>Injected free-gas fraction; used only for a positive total rate.</summary>
    public double InjectionGasFraction { get; init; }
}

public sealed record PhaseVolumes(double Oil, double Water, double Gas);

public sealed record WellRateSample(
    WellControlMode RequestedControlMode,
    WellControlMode EffectiveControlMode,
    double BottomHolePressurePa,
    string? SwitchReason,
    double TimeSeconds,
    double TimeStepSeconds,
    PhaseVolumes SignedRatesM3PerSecond,
    PhaseVolumes CumulativeInjectedM3,
    PhaseVolumes CumulativeProducedM3);

public sealed record WellTimeSeries(
    string Name,
    int I,
    int J,
    int KStart,
    int KEnd,
    IReadOnlyList<WellRateSample> Samples,
    int ConnectionCount);

/// <param name="InPlaceNormalizedBalanceErrorFraction">Absolute error normalized by phase inventory with a 1e-8 total pore-fluid-volume floor.</param>
/// <param name="ThroughputNormalizedBalanceErrorFraction">Absolute error normalized by throughput with the same floor; zero when throughput is zero.</param>
public sealed record PhaseMaterialBalance(
    double CumulativeInjectedM3,
    double CumulativeProducedM3,
    double BalanceErrorM3,
    double InPlaceNormalizedBalanceErrorFraction,
    double ThroughputNormalizedBalanceErrorFraction);

public sealed record MaterialBalanceDiagnostics(
    PhaseMaterialBalance Oil,
    PhaseMaterialBalance Water,
    PhaseMaterialBalance Gas,
    PhaseMaterialBalance Total);

public sealed record SimulationStateRanges(
    ScalarRange PressurePa,
    ScalarRange OilSaturation,
    ScalarRange WaterSaturation,
    ScalarRange GasSaturation,
    double MaximumSaturationClosureError);

public sealed record SimulationStepDiagnostics(
    int AcceptedSteps,
    int RejectedSteps,
    int TotalCgIterations,
    double MinimumAcceptedTimeStepSeconds,
    double MaximumAcceptedTimeStepSeconds);

public sealed record SimulationResult(
    string WorldId,
    string KernelName,
    string ModelLimitation,
    double SimulatedTimeSeconds,
    SimulationStepDiagnostics Steps,
    SimulationStateRanges StateRanges,
    MaterialBalanceDiagnostics MaterialBalance,
    double MaximumBalanceErrorFraction,
    IReadOnlyList<WellTimeSeries> Wells);

public sealed record SimulationRunEnvelope(
    SimulationResult Result,
    string FinalStateId);

public sealed record SimulationStateSummary(
    string StateId,
    string WorldId,
    string? ParentStateId,
    double SimulatedTimeSeconds,
    string ModelVersion,
    DateTimeOffset CreatedUtc);

public sealed record WorldDeletionResult(
    string WorldId,
    string Status);

public sealed record ServiceStatus(
    string Service,
    string KernelName,
    string ModelLimitation,
    string TruthIsolation);
