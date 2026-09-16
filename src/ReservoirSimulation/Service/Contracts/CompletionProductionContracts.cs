using System.Text.Json.Serialization;

namespace ReservoirSimulation.Contracts;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompletionProductionScheduleSegment
{
    public double StartTimeSeconds { get; init; }
    public double DurationSeconds { get; init; }
    public bool ShutIn { get; init; }
    public WellControlMode ControlMode { get; init; } = WellControlMode.Rate;
    public double TargetRateM3PerSecond { get; init; }
    public double TargetBottomHolePressurePa { get; init; }
    public double? MinimumBottomHolePressurePa { get; init; }
    public double? MaximumBottomHolePressurePa { get; init; }
    public double? MaximumAbsoluteRateM3PerSecond { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CompletionProductionRequest
{
    public string ProductionModelVersion { get; init; } = "completion-production-v1";
    public IReadOnlyList<CompletionProductionScheduleSegment> Schedule { get; init; } = [];
    public double InitialTimeStepSeconds { get; init; }
    public SolverOptions Solver { get; init; } = new();
}

public sealed record CompletionProductionCheckpoint(
    int Year,
    double SimulatedTimeSeconds,
    string StateId,
    string? ParentStateId,
    PhaseVolumes CumulativeProducedM3,
    double MaximumBalanceErrorFraction);

public sealed record CompletionProductionMonthlyTruth(
    int Month,
    double SimulatedTimeSeconds,
    double OilRateM3PerSecond,
    double WaterRateM3PerSecond,
    double GasRateM3PerSecond,
    double CumulativeOilM3,
    double CumulativeWaterM3,
    double CumulativeGasM3,
    double BottomHolePressurePa,
    WellControlMode EffectiveControlMode,
    string? SwitchReason);

public sealed record CompletionProductionResult(
    string ProductionRunId,
    string WorldId,
    string CompletionBindingId,
    string ModelVersion,
    IReadOnlyList<CompletionProductionCheckpoint> Checkpoints,
    IReadOnlyList<CompletionProductionMonthlyTruth> MonthlyTruth);
