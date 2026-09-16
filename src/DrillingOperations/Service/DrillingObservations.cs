using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace DrillingOperations;

public sealed class DrillingObservationOptions
{
    public const string SectionName = "DrillingObservation";
    public const string CurrentModelVersion = "drilling-observations-v1";
    public string ModelVersion { get; init; } = CurrentModelVersion;
    public string CalibrationVersion { get; init; } = "drilling-observation-calibration-default-v1";
    public string Seed { get; init; } = "drilling-observation-seed-v1";
    public double SampleSpacingM { get; init; } = 10;
    public double MaximumObservableLogInterpolationM { get; init; } = 20;
    public int MaximumSamples { get; init; } = 10_000;
    public double BitDiameterM { get; init; } = .216;
    public double BitTypeFactor { get; init; } = 1;
    public double NominalWeightOnBitKN { get; init; } = 120;
    public double NominalRpm { get; init; } = 140;
    public double NominalFlowRateM3PerSecond { get; init; } = .035;
    public double MudDensityKgM3 { get; init; } = 1200;
    public double MudViscosityPaS { get; init; } = .025;
    public double StrengthInterceptMpa { get; init; } = 18;
    public double StrengthGrCoefficientMpaPerApi { get; init; } = .16;
    public double StrengthRhobCoefficientMpaPerKgM3 { get; init; } = .015;
    public double StrengthNphiCoefficientMpaPerFraction { get; init; } = -12;
    public double StrengthLogResistivityCoefficientMpa { get; init; } = 3;
    public double MinimumStrengthMpa { get; init; } = 5;
    public double MaximumStrengthMpa { get; init; } = 180;
    public double RopCoefficientMPerHour { get; init; } = 32;
    public double RopWobExponent { get; init; } = .85;
    public double RopRpmExponent { get; init; } = .55;
    public double RopHydraulicsExponent { get; init; } = .25;
    public double RopStrengthExponent { get; init; } = .9;
    public double MinimumRopMPerHour { get; init; } = .5;
    public double MaximumRopMPerHour { get; init; } = 120;
    public double DrillStringWeightKNPerM { get; init; } = .28;
    public double TorqueCoefficient { get; init; } = .035;
    public double TorqueDragFrictionCoefficient { get; init; } = .22;
    public double LateralVibrationThresholdG { get; init; } = 1.5;
    public double AxialVibrationThresholdG { get; init; } = 1.2;
    public double StickSlipThresholdFraction { get; init; } = .35;
    public double VibrationNoiseStdDevG { get; init; } = .04;
    public double MechanicalNoiseFraction { get; init; } = .01;
    public double WeightOnBitBiasFraction { get; init; }
    public double RpmBiasFraction { get; init; }
    public double MechanicalSensorBiasFraction { get; init; }
    public double NominalAnnularDiameterM { get; init; } = .216;
    public double DrillPipeOuterDiameterM { get; init; } = .127;
    public double AnnularFrictionCoefficient { get; init; } = .035;
    public double StandpipeBasePressurePa { get; init; } = 2_000_000;
    public double NozzlePressureCoefficientPa { get; init; } = 4_000_000;
    public double LossZoneEcdThresholdKgM3 { get; init; } = 1280;
    public double LossZoneSensitivityPerKgM3 { get; init; } = .0006;
    public double MaximumLossFraction { get; init; } = .35;
    public double FlowNoiseStdDevFraction { get; init; } = .005;
    public double PressureNoiseStdDevPa { get; init; } = 10_000;
    public double FlowBiasFraction { get; init; }
    public double PressureBiasPa { get; init; }
    public double SurfaceTemperatureC { get; init; } = 8;
    public double GeothermalGradientCPerKm { get; init; } = 32;
    public double TemperatureNoiseStdDevC { get; init; } = .15;
    public double TemperatureBiasC { get; init; }
    public double CuttingsLagVolumeM3 { get; init; } = 12;
    public double CuttingsTransportEfficiency { get; init; } = .7;
    public double CuttingsDepthUncertaintyM { get; init; } = 8;
    public double CuttingsDepthBiasM { get; init; }
    public double CuttingsRecoveryFraction { get; init; } = .85;
    public double CuttingsSampleSpacingM { get; init; } = 30;
    public double SandFaciesMaximumGrApi { get; init; } = 45;
    public double MixedFaciesMaximumGrApi { get; init; } = 85;
    public double SensorMissingProbability { get; init; } = .005;

    public void Validate()
    {
        Token(ModelVersion); Token(CalibrationVersion); Token(Seed);
        if (ModelVersion != CurrentModelVersion) Fail("Unsupported drilling observation model version.");
        Between(SampleSpacingM, .1, 500); Between(MaximumObservableLogInterpolationM, .1, 500); if (MaximumSamples is < 1 or > 10_000) Fail("MaximumSamples must be between 1 and 10000.");
        Between(BitDiameterM, .05, 1); Between(BitTypeFactor, .1, 5); Between(NominalWeightOnBitKN, 1, 1000); Between(NominalRpm, 1, 1000);
        Between(NominalFlowRateM3PerSecond, .001, .5); Between(MudDensityKgM3, 500, 3000); Between(MudViscosityPaS, .001, 1);
        Finite(StrengthInterceptMpa); Finite(StrengthGrCoefficientMpaPerApi); Finite(StrengthRhobCoefficientMpaPerKgM3); Finite(StrengthNphiCoefficientMpaPerFraction); Finite(StrengthLogResistivityCoefficientMpa);
        Between(MinimumStrengthMpa, .1, 500); Between(MaximumStrengthMpa, MinimumStrengthMpa, 1000); if (MaximumStrengthMpa <= MinimumStrengthMpa) Fail("MaximumStrengthMpa must exceed MinimumStrengthMpa.");
        Between(RopCoefficientMPerHour, .01, 1000); Between(RopWobExponent, 0, 3); Between(RopRpmExponent, 0, 3); Between(RopHydraulicsExponent, 0, 3); Between(RopStrengthExponent, 0, 3); Between(MinimumRopMPerHour, .01, 100); Between(MaximumRopMPerHour, MinimumRopMPerHour, 1000);
        Between(DrillStringWeightKNPerM, .001, 10); Between(TorqueCoefficient, .00001, 10); Between(TorqueDragFrictionCoefficient, 0, 2); Between(LateralVibrationThresholdG, .01, 20); Between(AxialVibrationThresholdG, .01, 20); Between(StickSlipThresholdFraction, .01, 2); Between(VibrationNoiseStdDevG, 0, 5); Between(MechanicalNoiseFraction, 0, .5); Between(WeightOnBitBiasFraction, -.5, .5); Between(RpmBiasFraction, -.5, .5); Between(MechanicalSensorBiasFraction, -.5, .5);
        Between(NominalAnnularDiameterM, .05, 1); Between(DrillPipeOuterDiameterM, .01, Math.Min(BitDiameterM, NominalAnnularDiameterM) - .001); Between(AnnularFrictionCoefficient, .00001, 2); Between(StandpipeBasePressurePa, 0, 100_000_000); Between(NozzlePressureCoefficientPa, 0, 100_000_000); Between(LossZoneEcdThresholdKgM3, 500, 3000); Between(LossZoneSensitivityPerKgM3, 0, .1); Between(MaximumLossFraction, 0, .95); Between(FlowNoiseStdDevFraction, 0, .5); Between(PressureNoiseStdDevPa, 0, 10_000_000); Between(FlowBiasFraction, -.5, .5); Between(PressureBiasPa, -10_000_000, 10_000_000);
        Between(SurfaceTemperatureC, -100, 100); Between(GeothermalGradientCPerKm, 0, 100); Between(TemperatureNoiseStdDevC, 0, 20); Between(TemperatureBiasC, -50, 50); Between(CuttingsLagVolumeM3, 0, 1000); Between(CuttingsTransportEfficiency, .01, 1); Between(CuttingsDepthUncertaintyM, 0, 500); Between(CuttingsDepthBiasM, -500, 500); Between(CuttingsRecoveryFraction, 0, 1); Between(CuttingsSampleSpacingM, .1, 1000); Between(SandFaciesMaximumGrApi, 0, 300); Between(MixedFaciesMaximumGrApi, SandFaciesMaximumGrApi, 300); Between(SensorMissingProbability, 0, 1);
    }
    public string Hash() { Validate(); return DeterministicIdentity.Sha256(CanonicalJson.Serialize(this)); }
    private static void Token(string value) { if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Any(c => !(char.IsAsciiLetterOrDigit(c) || "-_.:".Contains(c)))) Fail("Version token is invalid."); }
    private static void Finite(double value) { if (!double.IsFinite(value) || Math.Abs(value) > 1e6) Fail("Calibration coefficient must be finite and bounded."); }
    private static void Between(double value, double minimum, double maximum) { if (!double.IsFinite(value) || value < minimum || value > maximum) Fail("Option is outside its physical calibration bounds."); }
    private static void Fail(string message) => throw new OptionsValidationException(nameof(DrillingObservationOptions), typeof(DrillingObservationOptions), [message]);
}

public sealed record ObservableRockResponse(double GrApi, double RhobKgM3, double NphiFraction, double DeepResistivityOhmM, double CaliperM = .216);
public sealed record DrillingOperatingPoint(double WeightOnBitKN, double Rpm, double FlowRateM3PerSecond);
public sealed record DrillingMechanicalResponse(double RopMPerHour, double HookloadKN, double SurfaceTorqueKNm, double DragForceKN, double LateralVibrationG, double AxialVibrationG, double StickSlipFraction);
public sealed record DrillingHydraulicResponse(double StandpipePressurePa, double AnnularEcdKgM3, double FlowOutM3PerSecond, double LossRateM3PerSecond);
public sealed record DrillingObservationCurve(string Mnemonic, string Unit, double Minimum, double Maximum, string Classification);
public sealed record DrillingObservationSample(int Ordinal, double SimulatedElapsedSeconds, double MeasuredDepthM, double ObservedTrueVerticalDepthM, double? RockStrengthProxyMpa, double? RopMPerHour, double? HookloadKN, double? SurfaceTorqueKNm, double? DragForceKN, double? LateralVibrationG, double? AxialVibrationG, double? StickSlipFraction, double? StandpipePressurePa, double? AnnularEcdKgM3, double? FlowInM3PerSecond, double? FlowOutM3PerSecond, double? LossRateM3PerSecond, double? DownholeTemperatureC, double? DownholeTemperatureK, IReadOnlyList<string> DysfunctionFlags, IReadOnlyList<string> QcFlags);
public sealed record CuttingsObservation(int Ordinal, double SurfaceArrivalSimulatedSeconds, double ObservedSourceMeasuredDepthM, double ObservedSourceTrueVerticalDepthM, double DepthUncertaintyM, double? LaggedGrApi, string? FaciesClassification, double? RecoveryFraction, IReadOnlyList<string> QcFlags, double SampleMeasuredDepthM = 0, double RecoveredSourceMeasuredDepthM = 0);
public sealed record DrillingObservationEvent(string EventId, string EventType, int StartOrdinal, int EndOrdinal, string Severity, string Qc);
public sealed record DrillingObservationSummary(int SampleCount, int CuttingsSampleCount, int MissingSampleCount, int LossEventCount, int DysfunctionEventCount, double MeanRopMPerHour, double MaximumEcdKgM3, double MeanDownholeTemperatureC);
public sealed record DrillingObservationArtifact(string ArtifactId, string RunId, string ScenarioId, string ExecutionId, string ExecutionHash, string SurveyArtifactId, string SurveyHash, string LogObservationBatchId, string LogHash, string LogContentHash, string ModelVersion, string CalibrationVersion, string OptionsHash, IReadOnlyList<DrillingObservationCurve> Curves, IReadOnlyList<DrillingObservationSample> Samples, IReadOnlyList<CuttingsObservation> Cuttings, IReadOnlyList<DrillingObservationEvent> Events, DrillingObservationSummary Summary, string InputHash, string OutputHash, DateTimeOffset CreatedUtc);

public static class DeterministicDrillingObservationModel
{
    public const int MaximumSamples = 10_000;
    public static double RockStrengthProxy(ObservableRockResponse log, DrillingObservationOptions options)
    {
        options.Validate();
        if (!double.IsFinite(log.GrApi) || !double.IsFinite(log.RhobKgM3) || !double.IsFinite(log.NphiFraction) || !double.IsFinite(log.DeepResistivityOhmM) || log.DeepResistivityOhmM <= 0 || !double.IsFinite(log.CaliperM) || log.CaliperM <= 0) throw new ArgumentOutOfRangeException(nameof(log));
        double value = options.StrengthInterceptMpa + options.StrengthGrCoefficientMpaPerApi * log.GrApi + options.StrengthRhobCoefficientMpaPerKgM3 * (log.RhobKgM3 - 2000) + options.StrengthNphiCoefficientMpaPerFraction * log.NphiFraction + options.StrengthLogResistivityCoefficientMpa * Math.Log10(Math.Max(.01, log.DeepResistivityOhmM)) - 10 * Math.Max(0, log.CaliperM - options.BitDiameterM);
        return Math.Clamp(value, options.MinimumStrengthMpa, options.MaximumStrengthMpa);
    }
    public static double RateOfPenetration(double strengthMpa, DrillingOperatingPoint operating, DrillingObservationOptions options, double dysfunctionFactor = 1)
    {
        options.Validate();
        if (!double.IsFinite(strengthMpa) || strengthMpa <= 0 || !double.IsFinite(dysfunctionFactor) || dysfunctionFactor is < 0 or > 1 || !double.IsFinite(operating.WeightOnBitKN) || operating.WeightOnBitKN <= 0 || !double.IsFinite(operating.Rpm) || operating.Rpm <= 0 || !double.IsFinite(operating.FlowRateM3PerSecond) || operating.FlowRateM3PerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(strengthMpa));
        double strengthReference = Math.Clamp(options.StrengthInterceptMpa + 30, options.MinimumStrengthMpa, options.MaximumStrengthMpa);
        double rop = options.RopCoefficientMPerHour * options.BitTypeFactor * Math.Pow(operating.WeightOnBitKN / options.NominalWeightOnBitKN, options.RopWobExponent) * Math.Pow(operating.Rpm / options.NominalRpm, options.RopRpmExponent) * Math.Pow(operating.FlowRateM3PerSecond / options.NominalFlowRateM3PerSecond, options.RopHydraulicsExponent) * Math.Pow(strengthReference / strengthMpa, options.RopStrengthExponent) * dysfunctionFactor;
        return Math.Clamp(rop, options.MinimumRopMPerHour, options.MaximumRopMPerHour);
    }
    public static DrillingMechanicalResponse Mechanical(double measuredDepthM, double inclinationDegrees, double strengthMpa, DrillingOperatingPoint operating, DrillingObservationOptions options)
    {
        options.Validate(); if (!double.IsFinite(measuredDepthM) || measuredDepthM < 0 || !double.IsFinite(inclinationDegrees)) throw new ArgumentOutOfRangeException(nameof(measuredDepthM));
        double inc = inclinationDegrees * Math.PI / 180, stringWeight = measuredDepthM * options.DrillStringWeightKNPerM, drag = options.TorqueDragFrictionCoefficient * (stringWeight + operating.WeightOnBitKN) * (.2 + Math.Abs(Math.Sin(inc))), torque = options.TorqueCoefficient * operating.WeightOnBitKN * options.BitDiameterM * (1 + options.TorqueDragFrictionCoefficient * (1 + Math.Abs(Math.Sin(inc))));
        double hardness = strengthMpa / Math.Max(options.MinimumStrengthMpa, options.StrengthInterceptMpa + 30), stickSlip = Math.Clamp((hardness - .7) * options.TorqueDragFrictionCoefficient, 0, 1), lateral = Math.Clamp(.15 + hardness * operating.Rpm / options.NominalRpm * .35, 0, 20), axial = Math.Clamp(.1 + hardness * operating.WeightOnBitKN / options.NominalWeightOnBitKN * .3, 0, 20), hookload = Math.Max(0, stringWeight * Math.Cos(inc) - operating.WeightOnBitKN + drag);
        return new(RateOfPenetration(strengthMpa, operating, options, 1 - .45 * stickSlip), hookload, torque, drag, lateral, axial, stickSlip);
    }
    public static DrillingHydraulicResponse Hydraulics(double measuredDepthM, double trueVerticalDepthM, double flowM3PerSecond, double strengthMpa, DrillingObservationOptions options) => Hydraulics(measuredDepthM, trueVerticalDepthM, flowM3PerSecond, strengthMpa, options.NominalAnnularDiameterM, options);
    public static DrillingHydraulicResponse Hydraulics(double measuredDepthM, double trueVerticalDepthM, double flowM3PerSecond, double strengthMpa, double annularDiameterM, DrillingObservationOptions options)
    {
        options.Validate();
        if (!double.IsFinite(measuredDepthM) || measuredDepthM < 0 || !double.IsFinite(trueVerticalDepthM) || trueVerticalDepthM < 0 || !double.IsFinite(flowM3PerSecond) || flowM3PerSecond <= 0 || !double.IsFinite(strengthMpa) || strengthMpa <= 0 || !double.IsFinite(annularDiameterM) || annularDiameterM <= options.DrillPipeOuterDiameterM || annularDiameterM > 1) throw new ArgumentOutOfRangeException(nameof(flowM3PerSecond));
        double annularArea = Math.PI / 4 * (annularDiameterM * annularDiameterM - options.DrillPipeOuterDiameterM * options.DrillPipeOuterDiameterM);
        double flowPathLength = Math.Max(1, measuredDepthM), verticalDepth = Math.Max(1, trueVerticalDepthM);
        double annularLoss = options.AnnularFrictionCoefficient * options.MudViscosityPaS * flowPathLength * flowM3PerSecond / Math.Pow(annularArea, 2) * 1000;
        double nozzle = options.NozzlePressureCoefficientPa * Math.Pow(flowM3PerSecond / options.NominalFlowRateM3PerSecond, 2);
        double standpipe = options.StandpipeBasePressurePa + annularLoss + nozzle;
        double ecd = options.MudDensityKgM3 + annularLoss / (9.80665 * verticalDepth);
        double weakness = 1 - Math.Clamp((strengthMpa - options.MinimumStrengthMpa) / (options.MaximumStrengthMpa - options.MinimumStrengthMpa), 0, 1);
        double lossFraction = Math.Clamp((ecd - options.LossZoneEcdThresholdKgM3) * options.LossZoneSensitivityPerKgM3 * (.5 + weakness), 0, options.MaximumLossFraction), loss = flowM3PerSecond * lossFraction;
        return new(standpipe, ecd, flowM3PerSecond - loss, loss);
    }
    public static (IReadOnlyList<DrillingObservationSample> Samples, IReadOnlyList<CuttingsObservation> Cuttings, IReadOnlyList<DrillingObservationEvent> Events, DrillingObservationSummary Summary) Generate(string scenarioId, string runId, DrillingExecutionArtifact execution, SurveyArtifact survey, LogObservationArtifact logs, DrillingObservationOptions options)
    {
        options.Validate(); ValidateSources(execution, survey, logs);
        double first = Math.Max(execution.Stations[0].MeasuredDepthM, Math.Max(survey.Stations[0].MeasuredDepthM, logs.Samples[0].MeasuredDepthM));
        double last = Math.Min(execution.Stations[^1].MeasuredDepthM, Math.Min(survey.Stations[^1].MeasuredDepthM, logs.Samples[^1].MeasuredDepthM));
        int count = (int)Math.Floor((last - first) / options.SampleSpacingM) + 1;
        if (last > first + (count - 1) * options.SampleSpacingM + 1e-9) count++;
        if (count is < 1 || count > options.MaximumSamples || count > MaximumSamples) throw new RunStageFailureException("DrillingObservationSampleLimitExceeded", "Drilling observation series exceeds its configured limit.");
        var rng = new HashNoise(scenarioId + "\n" + runId + "\ndrilling-observations\n" + options.ModelVersion + "\n" + options.CalibrationVersion + "\n" + options.Seed);
        var samples = new List<DrillingObservationSample>(count); var cuttings = new List<CuttingsObservation>(); int missing = 0; double nextCuttingsMd = first;
        for (int ordinal = 0; ordinal < count; ordinal++)
        {
            double md = ordinal == count - 1 ? last : first + ordinal * options.SampleSpacingM;
            double time = InterpolateTimeline(execution, md);
            SurveyStation surveyPoint = InterpolateSurvey(survey.Stations, md);
            double tvd = surveyPoint.ObservedTrueVerticalDepthM, inclination = surveyPoint.InclinationDegrees;
            bool cuttingsDue = md + 1e-9 >= nextCuttingsMd;
            if (cuttingsDue) while (nextCuttingsMd <= md + 1e-9) nextCuttingsMd += options.CuttingsSampleSpacingM;
            LogObservationSample? log = ObservableLog(logs.Samples, md, options.MaximumObservableLogInterpolationM);
            var qc = new List<string>(); bool sensorMissing = rng.Uniform(ordinal, 0) < options.SensorMissingProbability;
            if (log is null)
            {
                qc.Add("ObservableLogUnavailable"); missing++;
                samples.Add(new(ordinal, time, md, tvd, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, [], qc));
                continue;
            }
            var observable = new ObservableRockResponse(log.GrApi!.Value, log.RhobKgM3!.Value, log.NphiFraction!.Value, log.DeepResistivityOhmM!.Value, log.CaliperM!.Value);
            double strength = RockStrengthProxy(observable, options);
            double wob = options.NominalWeightOnBitKN * (1 + options.WeightOnBitBiasFraction + .04 * rng.Normal(ordinal, 1));
            double rpm = options.NominalRpm * (1 + options.RpmBiasFraction + .03 * rng.Normal(ordinal, 2));
            double flow = options.NominalFlowRateM3PerSecond * (1 + options.FlowBiasFraction + .01 * rng.Normal(ordinal, 3));
            var operating = new DrillingOperatingPoint(Math.Max(1, wob), Math.Max(1, rpm), Math.Max(.001, flow));
            DrillingMechanicalResponse mechanical = Mechanical(md, inclination, strength, operating, options);
            double annularDiameter = Math.Clamp(log.CaliperM.Value, options.DrillPipeOuterDiameterM + .001, 1);
            if (annularDiameter != log.CaliperM.Value) qc.Add("CALIClippedForAnnulus");
            DrillingHydraulicResponse hydraulic = Hydraulics(md, tvd, operating.FlowRateM3PerSecond, strength, annularDiameter, options);
            if (sensorMissing)
            {
                qc.Add("SensorMissing"); missing++;
                samples.Add(new(ordinal, time, md, tvd, strength, null, null, null, null, null, null, null, null, null, null, null, null, null, null, [], qc));
                continue;
            }
            double lateral = Math.Clamp(mechanical.LateralVibrationG + rng.Normal(ordinal, 4) * options.VibrationNoiseStdDevG, 0, 20);
            double axial = Math.Clamp(mechanical.AxialVibrationG + rng.Normal(ordinal, 5) * options.VibrationNoiseStdDevG, 0, 20);
            double stick = Math.Clamp(mechanical.StickSlipFraction + rng.Normal(ordinal, 6) * options.MechanicalNoiseFraction, 0, 1);
            double temperatureC = Clip(options.SurfaceTemperatureC + options.TemperatureBiasC + options.GeothermalGradientCPerKm * tvd / 1000 + rng.Normal(ordinal, 7) * options.TemperatureNoiseStdDevC, -100, 350, qc, "TEMP");
            double flowOut = Clip(hydraulic.FlowOutM3PerSecond * (1 + rng.Normal(ordinal, 8) * options.FlowNoiseStdDevFraction), 0, operating.FlowRateM3PerSecond, qc, "FLOW_OUT");
            double observableLoss = Math.Max(0, operating.FlowRateM3PerSecond - flowOut);
            double pressure = Clip(hydraulic.StandpipePressurePa + options.PressureBiasPa + rng.Normal(ordinal, 9) * options.PressureNoiseStdDevPa, 0, 200_000_000, qc, "SPP");
            double ecd = Clip(hydraulic.AnnularEcdKgM3, 500, 5000, qc, "ECD");
            double rop = Clip(mechanical.RopMPerHour, options.MinimumRopMPerHour, options.MaximumRopMPerHour, qc, "ROP");
            double hookload = Clip(mechanical.HookloadKN * (1 + options.MechanicalSensorBiasFraction), 0, 10000, qc, "HKLD");
            double torque = Clip(mechanical.SurfaceTorqueKNm * (1 + options.MechanicalSensorBiasFraction), 0, 1000, qc, "TORQ");
            double drag = Clip(mechanical.DragForceKN * (1 + options.MechanicalSensorBiasFraction), 0, 10000, qc, "DRAG");
            var flags = new List<string>();
            if (stick >= options.StickSlipThresholdFraction) flags.Add("StickSlip");
            if (lateral >= options.LateralVibrationThresholdG) flags.Add("LateralVibration");
            if (axial >= options.AxialVibrationThresholdG) flags.Add("AxialVibration");
            if (observableLoss > Math.Max(1e-9, operating.FlowRateM3PerSecond * .001)) flags.Add("Losses");
            qc.AddRange(flags.Select(x => x + "Observed"));
            qc.AddRange(log.QcFlags); if (qc.Count == 0) qc.Add("Valid");
            samples.Add(new(ordinal, time, md, tvd, strength, rop, hookload, torque, drag, lateral, axial, stick, pressure, ecd, operating.FlowRateM3PerSecond, flowOut, observableLoss, temperatureC, temperatureC + 273.15, flags, qc));
            if (cuttingsDue)
            {
                double lagSeconds = options.CuttingsLagVolumeM3 / (operating.FlowRateM3PerSecond * options.CuttingsTransportEfficiency);
                double sourceTime = Math.Max(execution.Timeline[0].SimulatedElapsedSeconds, time - lagSeconds);
                double sourceMd = Math.Clamp(InterpolateMeasuredDepthAtTime(execution, sourceTime), first, last);
                double observedMd = Math.Clamp(sourceMd + options.CuttingsDepthBiasM + rng.Normal(ordinal, 10) * options.CuttingsDepthUncertaintyM / 2, first, last);
                double sourceTvd = InterpolateSurvey(survey.Stations, observedMd).ObservedTrueVerticalDepthM;
                double recovery = Math.Clamp(options.CuttingsRecoveryFraction * options.CuttingsTransportEfficiency * (1 - observableLoss / operating.FlowRateM3PerSecond), 0, 1);
                LogObservationSample? lagged = ObservableLog(logs.Samples, sourceMd, options.MaximumObservableLogInterpolationM);
                var cuttingsQc = new List<string>(); if (lagged is null) cuttingsQc.Add("LaggedLogUnavailable"); if (recovery < .5) cuttingsQc.Add("LowRecovery"); if (cuttingsQc.Count == 0) cuttingsQc.Add("SyntheticLagCorrected");
                double? gr = lagged?.GrApi; string? facies = gr is null ? null : gr <= options.SandFaciesMaximumGrApi ? "Sand-prone" : gr <= options.MixedFaciesMaximumGrApi ? "Mixed" : "Shale-prone";
                cuttings.Add(new(cuttings.Count, time, observedMd, sourceTvd, options.CuttingsDepthUncertaintyM, gr, facies, recovery, cuttingsQc, md, observedMd));
            }
        }
        IReadOnlyList<DrillingObservationEvent> events = BuildObservableEventRuns(runId, samples, options);
        double[] ropValues = samples.Where(x => x.RopMPerHour is not null).Select(x => x.RopMPerHour!.Value).ToArray();
        double[] ecdValues = samples.Where(x => x.AnnularEcdKgM3 is not null).Select(x => x.AnnularEcdKgM3!.Value).ToArray();
        double[] temperatures = samples.Where(x => x.DownholeTemperatureC is not null).Select(x => x.DownholeTemperatureC!.Value).ToArray();
        var summary = new DrillingObservationSummary(samples.Count, cuttings.Count, missing, events.Count(x => x.EventType == "Losses"), events.Count(x => x.EventType != "Losses"), ropValues.DefaultIfEmpty(0).Average(), ecdValues.DefaultIfEmpty(0).Max(), temperatures.DefaultIfEmpty(0).Average());
        return (samples, cuttings, events, summary);
    }
    public static IReadOnlyList<DrillingObservationCurve> CurveDefinitions(DrillingObservationOptions options) { options.Validate(); return [new("ROPA","m/h",options.MinimumRopMPerHour,options.MaximumRopMPerHour,"Synthetic"),new("HKLD","kN",0,10000,"Synthetic"),new("TORQ","kN.m",0,1000,"Synthetic"),new("DRAG","kN",0,10000,"Derived"),new("VIB_LAT","g",0,20,"Synthetic"),new("VIB_AX","g",0,20,"Synthetic"),new("STICK_SLIP","fraction",0,1,"Derived"),new("SPP","Pa",0,200_000_000,"Synthetic"),new("ECD","kg/m3",500,5000,"Derived"),new("FLOW_IN","m3/s",0,1,"Synthetic"),new("FLOW_OUT","m3/s",0,1,"Synthetic"),new("LOSSES","m3/s",0,1,"Derived"),new("TEMP_C","degC",-100,350,"Synthetic"),new("TEMP_K","K",173.15,623.15,"Derived"),new("UCS_PROXY","MPa",options.MinimumStrengthMpa,options.MaximumStrengthMpa,"Derived")]; }
    private static double Clip(double value,double minimum,double maximum,List<string> qc,string mnemonic){double clipped=Math.Clamp(value,minimum,maximum);if(clipped!=value)qc.Add(mnemonic+"Clipped");return clipped;}
    private static IReadOnlyList<DrillingObservationEvent> BuildObservableEventRuns(string runId, IReadOnlyList<DrillingObservationSample> samples, DrillingObservationOptions options)
    {
        var events = new List<DrillingObservationEvent>();
        foreach (string type in new[] { "Losses", "StickSlip", "LateralVibration", "AxialVibration" })
        {
            int start = -1;
            for (int i = 0; i <= samples.Count; i++)
            {
                bool active = i < samples.Count && samples[i].DysfunctionFlags.Contains(type, StringComparer.Ordinal);
                if (active && start < 0) start = i;
                if ((!active || i == samples.Count) && start >= 0)
                {
                    int end = i - 1; double magnitude = Enumerable.Range(start, end - start + 1).Max(index => ObservableMagnitude(samples[index], type, options));
                    events.Add(new(DeterministicIdentity.Create("drilling-observation-event-v1", runId, start.ToString(System.Globalization.CultureInfo.InvariantCulture), end.ToString(System.Globalization.CultureInfo.InvariantCulture), type), type, start, end, EventSeverity(type, magnitude), "ObservableThreshold")); start = -1;
                }
            }
        }
        return events.OrderBy(x => x.StartOrdinal).ThenBy(x => x.EventType, StringComparer.Ordinal).ToArray();
    }
    public static string LossEventSeverity(double observableLossFraction)
    {
        if (!double.IsFinite(observableLossFraction) || observableLossFraction < 0 || observableLossFraction > 1) throw new ArgumentOutOfRangeException(nameof(observableLossFraction));
        return observableLossFraction >= .2 ? "High" : observableLossFraction >= .05 ? "Moderate" : "Low";
    }
    private static string EventSeverity(string type, double magnitude) => type == "Losses" ? LossEventSeverity(magnitude) : magnitude >= 2 ? "High" : magnitude >= 1 ? "Moderate" : "Low";
    private static double ObservableMagnitude(DrillingObservationSample sample, string type, DrillingObservationOptions options) => type switch
    {
        "Losses" => sample.FlowInM3PerSecond is > 0 ? sample.LossRateM3PerSecond!.Value / sample.FlowInM3PerSecond.Value : 0,
        "StickSlip" => sample.StickSlipFraction!.Value / options.StickSlipThresholdFraction,
        "LateralVibration" => sample.LateralVibrationG!.Value / options.LateralVibrationThresholdG,
        _ => sample.AxialVibrationG!.Value / options.AxialVibrationThresholdG
    };
    private static void ValidateSources(DrillingExecutionArtifact execution, SurveyArtifact survey, LogObservationArtifact logs) { if (execution.RunId != survey.RunId || execution.RunId != logs.RunId || execution.Stations.Count < 2 || execution.Timeline.Count != execution.Stations.Count || survey.Stations.Count < 2 || logs.Samples.Count < 2 || !StrictlyIncreasing(execution.Stations.Select(x=>x.MeasuredDepthM)) || !StrictlyIncreasing(survey.Stations.Select(x=>x.MeasuredDepthM)) || !StrictlyIncreasing(logs.Samples.Select(x=>x.MeasuredDepthM)) || execution.Timeline.Select(x=>x.StationIndex).Where((x,i)=>x!=i).Any() || execution.Timeline.Zip(execution.Timeline.Skip(1),(a,b)=>b.SimulatedElapsedSeconds<a.SimulatedElapsedSeconds).Any(x=>x)) throw new RunStageFailureException("DrillingObservationSourceInvalid", "Execution, survey, and observable logs must be complete, finite, ordered, and belong to one run."); static bool StrictlyIncreasing(IEnumerable<double> values){double prior=double.NegativeInfinity;foreach(double value in values){if(!double.IsFinite(value)||value<=prior)return false;prior=value;}return true;} }
    private static double InterpolateTimeline(DrillingExecutionArtifact execution, double md) { int i = 1; while (i < execution.Stations.Count && execution.Stations[i].MeasuredDepthM < md) i++; if (i >= execution.Stations.Count) return execution.Timeline[^1].SimulatedElapsedSeconds; DrilledPathStation a = execution.Stations[i - 1], b = execution.Stations[i]; double f = (md - a.MeasuredDepthM) / (b.MeasuredDepthM - a.MeasuredDepthM); return execution.Timeline[i - 1].SimulatedElapsedSeconds + (execution.Timeline[i].SimulatedElapsedSeconds - execution.Timeline[i - 1].SimulatedElapsedSeconds) * f; }
    private static double InterpolateMeasuredDepthAtTime(DrillingExecutionArtifact execution, double elapsedSeconds) { int i = 1; while (i < execution.Timeline.Count && execution.Timeline[i].SimulatedElapsedSeconds < elapsedSeconds) i++; if (i >= execution.Timeline.Count) return execution.Stations[^1].MeasuredDepthM; DrillingTimelinePoint a = execution.Timeline[i - 1], b = execution.Timeline[i]; if (b.SimulatedElapsedSeconds == a.SimulatedElapsedSeconds) return execution.Stations[b.StationIndex].MeasuredDepthM; double f = (elapsedSeconds - a.SimulatedElapsedSeconds) / (b.SimulatedElapsedSeconds - a.SimulatedElapsedSeconds); return execution.Stations[a.StationIndex].MeasuredDepthM + (execution.Stations[b.StationIndex].MeasuredDepthM - execution.Stations[a.StationIndex].MeasuredDepthM) * f; }
    private static SurveyStation InterpolateSurvey(IReadOnlyList<SurveyStation> points, double md) { int i = 1; while (i < points.Count && points[i].MeasuredDepthM < md) i++; if (i >= points.Count) return points[^1]; SurveyStation a = points[i - 1], b = points[i]; double f = (md - a.MeasuredDepthM) / (b.MeasuredDepthM - a.MeasuredDepthM); return a with { MeasuredDepthM = md, ObservedEastingM = L(a.ObservedEastingM, b.ObservedEastingM), ObservedNorthingM = L(a.ObservedNorthingM, b.ObservedNorthingM), ObservedTrueVerticalDepthM = L(a.ObservedTrueVerticalDepthM, b.ObservedTrueVerticalDepthM), InclinationDegrees = L(a.InclinationDegrees, b.InclinationDegrees), AzimuthDegrees = L(a.AzimuthDegrees, b.AzimuthDegrees) }; double L(double x, double y) => x + (y - x) * f; }
    private static LogObservationSample? ObservableLog(IReadOnlyList<LogObservationSample> points, double md, double maximumInterpolationM)
    {
        int upper = 0; while (upper < points.Count && points[upper].MeasuredDepthM < md) upper++;
        if (upper < points.Count && Math.Abs(points[upper].MeasuredDepthM - md) <= 1e-9) return IsUsable(points[upper]) ? points[upper] : null;
        if (upper == 0 || upper >= points.Count) return null;
        LogObservationSample lower = points[upper - 1], higher = points[upper]; double gap = higher.MeasuredDepthM - lower.MeasuredDepthM;
        if (gap > maximumInterpolationM || !IsUsable(lower) || !IsUsable(higher)) return null;
        double f = (md - lower.MeasuredDepthM) / gap;
        return new(md, L(lower.GrApi!.Value, higher.GrApi!.Value), L(lower.RhobKgM3!.Value, higher.RhobKgM3!.Value), L(lower.NphiFraction!.Value, higher.NphiFraction!.Value), L(lower.DeepResistivityOhmM!.Value, higher.DeepResistivityOhmM!.Value), L(lower.CaliperM!.Value, higher.CaliperM!.Value), lower.QcFlags.Concat(higher.QcFlags).Distinct(StringComparer.Ordinal).ToArray());
        double L(double x, double y) => x + (y - x) * f;
        static bool IsUsable(LogObservationSample x) => x.GrApi is not null && x.RhobKgM3 is not null && x.NphiFraction is not null && x.DeepResistivityOhmM is not null && x.CaliperM is not null && double.IsFinite(x.GrApi.Value) && double.IsFinite(x.RhobKgM3.Value) && double.IsFinite(x.NphiFraction.Value) && double.IsFinite(x.DeepResistivityOhmM.Value) && x.DeepResistivityOhmM.Value > 0 && double.IsFinite(x.CaliperM.Value) && x.CaliperM.Value > 0 && !x.QcFlags.Contains("BadHole") && !x.QcFlags.Contains("MissingInterval");
    }
    private sealed class HashNoise(string seed) { public double Uniform(int ordinal, int channel) { byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed + "\n" + ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n" + channel.ToString(System.Globalization.CultureInfo.InvariantCulture))); return (BitConverter.ToUInt64(hash, 0) + 1d) / (ulong.MaxValue + 2d); } public double Normal(int ordinal, int channel) { double a = Uniform(ordinal, channel * 2), b = Uniform(ordinal, channel * 2 + 1); return Math.Sqrt(-2 * Math.Log(a)) * Math.Cos(Math.Tau * b); } }
}



