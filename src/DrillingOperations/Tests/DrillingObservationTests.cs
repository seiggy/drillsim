using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class DrillingObservationModelTests
{
    [Test]
    public void ForwardResponses_HaveExpectedMonotonicPhysicalDirections()
    {
        var options = new DrillingObservationOptions();
        var nominal = new DrillingOperatingPoint(options.NominalWeightOnBitKN, options.NominalRpm, options.NominalFlowRateM3PerSecond);
        double soft = DeterministicDrillingObservationModel.RateOfPenetration(25, nominal, options);
        double hard = DeterministicDrillingObservationModel.RateOfPenetration(80, nominal, options);
        double highWob = DeterministicDrillingObservationModel.RateOfPenetration(50, nominal with { WeightOnBitKN = nominal.WeightOnBitKN * 1.25 }, options);
        double lowWob = DeterministicDrillingObservationModel.RateOfPenetration(50, nominal with { WeightOnBitKN = nominal.WeightOnBitKN * .75 }, options);
        double highRpm = DeterministicDrillingObservationModel.RateOfPenetration(50, nominal with { Rpm = nominal.Rpm * 1.25 }, options);
        double lowRpm = DeterministicDrillingObservationModel.RateOfPenetration(50, nominal with { Rpm = nominal.Rpm * .75 }, options);
        var lowFriction = new DrillingObservationOptions { TorqueDragFrictionCoefficient = .1 };
        var highFriction = new DrillingObservationOptions { TorqueDragFrictionCoefficient = .5 };
        DrillingMechanicalResponse lowMechanical = DeterministicDrillingObservationModel.Mechanical(1500, 60, 50, nominal, lowFriction);
        DrillingMechanicalResponse highMechanical = DeterministicDrillingObservationModel.Mechanical(1500, 60, 50, nominal, highFriction);
        DrillingHydraulicResponse lowFlow = DeterministicDrillingObservationModel.Hydraulics(1500, 1500, .02, 50, options);
        DrillingHydraulicResponse highFlow = DeterministicDrillingObservationModel.Hydraulics(1500, 1500, .05, 50, options);
        DrillingHydraulicResponse heavyMud = DeterministicDrillingObservationModel.Hydraulics(1500, 1500, .05, 50, new DrillingObservationOptions { MudDensityKgM3 = 1500 });
        DrillingHydraulicResponse narrowAnnulus = DeterministicDrillingObservationModel.Hydraulics(1500, 1500, .05, 50, .18, options);
        DrillingHydraulicResponse wideAnnulus = DeterministicDrillingObservationModel.Hydraulics(1500, 1500, .05, 50, .30, options);
        DrillingHydraulicResponse shortVertical = DeterministicDrillingObservationModel.Hydraulics(1000, 1000, .04, 50, options);
        DrillingHydraulicResponse longHorizontal = DeterministicDrillingObservationModel.Hydraulics(2000, 1000, .04, 50, options);
        DrillingHydraulicResponse sameMdDeeper = DeterministicDrillingObservationModel.Hydraulics(2000, 2000, .04, 50, options);
        Assert.Multiple(() =>
        {
            Assert.That(soft, Is.GreaterThan(hard)); Assert.That(highWob, Is.GreaterThan(lowWob)); Assert.That(highRpm, Is.GreaterThan(lowRpm));
            Assert.That(highMechanical.DragForceKN, Is.GreaterThan(lowMechanical.DragForceKN)); Assert.That(highMechanical.SurfaceTorqueKNm, Is.GreaterThan(lowMechanical.SurfaceTorqueKNm));
            Assert.That(highFlow.AnnularEcdKgM3, Is.GreaterThan(lowFlow.AnnularEcdKgM3)); Assert.That(heavyMud.AnnularEcdKgM3, Is.GreaterThan(highFlow.AnnularEcdKgM3)); Assert.That(highFlow.StandpipePressurePa, Is.GreaterThan(lowFlow.StandpipePressurePa)); Assert.That(narrowAnnulus.AnnularEcdKgM3, Is.GreaterThan(wideAnnulus.AnnularEcdKgM3));
            Assert.That(longHorizontal.StandpipePressurePa, Is.GreaterThan(shortVertical.StandpipePressurePa));
            Assert.That(longHorizontal.AnnularEcdKgM3, Is.GreaterThan(shortVertical.AnnularEcdKgM3));
            Assert.That(sameMdDeeper.StandpipePressurePa, Is.EqualTo(longHorizontal.StandpipePressurePa).Within(1e-12));
            Assert.That(sameMdDeeper.AnnularEcdKgM3, Is.LessThan(longHorizontal.AnnularEcdKgM3));
        });
    }

    [Test]
    public void LossEvents_UseObservableLossFractionSpecificSeverity()
    {
        var source = Source();
        string Severity(double threshold)
        {
            var result = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-loss-" + threshold.ToString(System.Globalization.CultureInfo.InvariantCulture), source.Execution, source.Survey, source.Logs, CleanOptions(lossThresholdKgM3: threshold));
            return result.Events.Single(x => x.EventType == "Losses").Severity;
        }
        Assert.Multiple(() =>
        {
            Assert.That(Severity(1200), Is.EqualTo("Low"));
            Assert.That(Severity(1100), Is.EqualTo("Moderate"));
            Assert.That(Severity(800), Is.EqualTo("High"));
            Assert.That(DeterministicDrillingObservationModel.LossEventSeverity(.049), Is.EqualTo("Low"));
            Assert.That(DeterministicDrillingObservationModel.LossEventSeverity(.05), Is.EqualTo("Moderate"));
            Assert.That(DeterministicDrillingObservationModel.LossEventSeverity(.2), Is.EqualTo("High"));
        });
    }

    [Test]
    public void Generation_ProducesLossTemperatureAndLaggedCuttingsWithoutHiddenProperties()
    {
        var source = Source();
        var noLagOptions = CleanOptions();
        var lagOptions = CleanOptions(cuttingsLagVolumeM3: 300, depthUncertaintyM: 8, lossThresholdKgM3: 1000);
        var noLag = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-observation", source.Execution, source.Survey, source.Logs, noLagOptions);
        var lagged = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-observation", source.Execution, source.Survey, source.Logs, lagOptions);
        var closedForm = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-observation", source.Execution, source.Survey, source.Logs, CleanOptions(cuttingsLagVolumeM3: 300));
        CuttingsObservation direct = noLag.Cuttings.Single(x => x.Ordinal == 12), delayed = lagged.Cuttings.Single(x => x.Ordinal == 12), consistent = closedForm.Cuttings.Single(x => x.Ordinal == 12);
        DrillingObservationSample surfaceSample = closedForm.Samples.Single(x => x.MeasuredDepthM == consistent.SampleMeasuredDepthM);
        double expectedSourceTime = Math.Max(0, consistent.SurfaceArrivalSimulatedSeconds - 300 / (surfaceSample.FlowInM3PerSecond!.Value * lagOptions.CuttingsTransportEfficiency));
        double expectedSourceMd = expectedSourceTime / 120;
        string json = CanonicalJson.Serialize(lagged);
        Assert.Multiple(() =>
        {
            Assert.That(lagged.Samples.All(x => x.LossRateM3PerSecond > 0), Is.True); Assert.That(lagged.Events.Any(x => x.EventType == "Losses"), Is.True);
            Assert.That(lagged.Samples[^1].DownholeTemperatureC!.Value, Is.GreaterThan(lagged.Samples[0].DownholeTemperatureC!.Value));double inferredGradient=(lagged.Samples[^1].DownholeTemperatureC!.Value-lagged.Samples[0].DownholeTemperatureC!.Value)/(lagged.Samples[^1].ObservedTrueVerticalDepthM-lagged.Samples[0].ObservedTrueVerticalDepthM)*1000;Assert.That(inferredGradient,Is.EqualTo(lagOptions.GeothermalGradientCPerKm).Within(1e-10));
            Assert.That(delayed.ObservedSourceMeasuredDepthM, Is.LessThan(direct.ObservedSourceMeasuredDepthM)); Assert.That(delayed.SampleMeasuredDepthM, Is.EqualTo(direct.SampleMeasuredDepthM));
            Assert.That(consistent.SurfaceArrivalSimulatedSeconds, Is.EqualTo(surfaceSample.SimulatedElapsedSeconds));
            Assert.That(consistent.RecoveredSourceMeasuredDepthM, Is.EqualTo(expectedSourceMd).Within(1e-10)); Assert.That(delayed.RecoveredSourceMeasuredDepthM, Is.EqualTo(delayed.ObservedSourceMeasuredDepthM)); Assert.That(delayed.FaciesClassification, Is.Not.EqualTo(direct.FaciesClassification)); Assert.That(delayed.DepthUncertaintyM, Is.EqualTo(8));
            Assert.That(json, Does.Not.Contain("oilSaturation").And.Not.Contain("waterSaturation").And.Not.Contain("gasSaturation").And.Not.Contain("porosity").And.Not.Contain("permeability").And.Not.Contain("worldId").And.Not.Contain("bindingId"));
        });
    }

    [Test]
    public void Generation_IsDeterministicCalibrationSensitiveBoundedAndExplicitlyMissing()
    {
        var source = Source(); var options = CleanOptions();
        var first = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-deterministic", source.Execution, source.Survey, source.Logs, options);
        var replay = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-deterministic", source.Execution, source.Survey, source.Logs, options);
        var changed = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-deterministic", source.Execution, source.Survey, source.Logs, CleanOptions(calibrationVersion: "changed-v1"));
        var missing = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-missing", source.Execution, source.Survey, source.Logs, CleanOptions(missingProbability: 1));
        Assert.Multiple(() =>
        {
            Assert.That(CanonicalJson.Serialize(first.Samples), Is.EqualTo(CanonicalJson.Serialize(replay.Samples))); Assert.That(CanonicalJson.Serialize(first.Samples), Is.Not.EqualTo(CanonicalJson.Serialize(changed.Samples)));
            Assert.That(first.Samples.Count, Is.LessThanOrEqualTo(10_000)); Assert.That(first.Samples.All(Finite), Is.True); Assert.That(missing.Samples.All(x => x.RopMPerHour is null && x.QcFlags.Contains("SensorMissing") && x.DysfunctionFlags.Count == 0), Is.True); Assert.That(missing.Events, Is.Empty); Assert.That(missing.Summary.LossEventCount, Is.Zero); Assert.That(missing.Summary.DysfunctionEventCount, Is.Zero);
            Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() => new DrillingObservationOptions { BitDiameterM = .1, DrillPipeOuterDiameterM = .2 }.Validate());
            Assert.Throws<RunStageFailureException>(() => DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "too-many", source.Execution, source.Survey, source.Logs, CleanOptions(spacingM: .1, maximumSamples: 100)));
        });
        static bool Finite(DrillingObservationSample x) => new double?[] { x.RockStrengthProxyMpa,x.RopMPerHour,x.HookloadKN,x.SurfaceTorqueKNm,x.DragForceKN,x.LateralVibrationG,x.AxialVibrationG,x.StickSlipFraction,x.StandpipePressurePa,x.AnnularEcdKgM3,x.FlowInM3PerSecond,x.FlowOutM3PerSecond,x.LossRateM3PerSecond,x.DownholeTemperatureC,x.DownholeTemperatureK }.All(v=>v is null||double.IsFinite(v.Value));
    }


    [Test]
    public void ObservableNoise_DrivesFlagsAndContiguousEventRuns()
    {
        var source = Source();
        var options = new DrillingObservationOptions { MaximumObservableLogInterpolationM = 100, SensorMissingProbability = 0, VibrationNoiseStdDevG = .5, MechanicalNoiseFraction = .2, FlowNoiseStdDevFraction = .03, LateralVibrationThresholdG = .2, AxialVibrationThresholdG = .15, StickSlipThresholdFraction = .05, LossZoneEcdThresholdKgM3 = 1000 };
        var result = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-noisy-events", source.Execution, source.Survey, source.Logs, options);
        Assert.That(result.Events, Is.Not.Empty);
        foreach (DrillingObservationSample sample in result.Samples)
        {
            Assert.That(sample.DysfunctionFlags.Contains("StickSlip"), Is.EqualTo(sample.StickSlipFraction >= options.StickSlipThresholdFraction));
            Assert.That(sample.DysfunctionFlags.Contains("LateralVibration"), Is.EqualTo(sample.LateralVibrationG >= options.LateralVibrationThresholdG));
            Assert.That(sample.DysfunctionFlags.Contains("AxialVibration"), Is.EqualTo(sample.AxialVibrationG >= options.AxialVibrationThresholdG));
            bool observableLoss = sample.FlowInM3PerSecond is > 0 && sample.LossRateM3PerSecond > Math.Max(1e-9, sample.FlowInM3PerSecond.Value * .001);
            Assert.That(sample.DysfunctionFlags.Contains("Losses"), Is.EqualTo(observableLoss));
        }
        foreach (DrillingObservationEvent observationEvent in result.Events)
        {
            Assert.That(Enumerable.Range(observationEvent.StartOrdinal, observationEvent.EndOrdinal - observationEvent.StartOrdinal + 1).All(i => result.Samples[i].DysfunctionFlags.Contains(observationEvent.EventType)), Is.True);
            if (observationEvent.StartOrdinal > 0) Assert.That(result.Samples[observationEvent.StartOrdinal - 1].DysfunctionFlags, Does.Not.Contain(observationEvent.EventType));
            if (observationEvent.EndOrdinal + 1 < result.Samples.Count) Assert.That(result.Samples[observationEvent.EndOrdinal + 1].DysfunctionFlags, Does.Not.Contain(observationEvent.EventType));
        }
    }

    [Test]
    public void MissingLogIntervals_AreNotFilledFromDistantValidSamples()
    {
        var source = Source();
        LogObservationSample[] sparse = [ValidLog(0, 20), ValidLog(10, 20), ValidLog(190, 120), ValidLog(200, 120)];
        var result = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-local-logs", source.Execution, source.Survey, source.Logs with { Samples = sparse }, CleanOptions(maximumLogInterpolationM: 20));
        DrillingObservationSample unavailable = result.Samples.Single(x => x.MeasuredDepthM == 100);
        Assert.Multiple(() =>
        {
            Assert.That(unavailable.RopMPerHour, Is.Null);
            Assert.That(unavailable.QcFlags, Does.Contain("ObservableLogUnavailable"));
            Assert.That(unavailable.DysfunctionFlags, Is.Empty);
            Assert.That(result.Samples.Single(x => x.MeasuredDepthM == 10).RopMPerHour, Is.Not.Null);
            Assert.That(result.Samples.Single(x => x.MeasuredDepthM == 190).RopMPerHour, Is.Not.Null);
        });
    }

    [Test]
    public void Generation_UsesConfiguredSeedBiasAndHandlesHorizontalThenUpwardSurvey()
    {
        var source = Source();
        SurveyArtifact directionalSurvey = source.Survey with { Stations = [Station(0, 100), Station(100, 100), Station(200, 50)] };
        var baselineOptions = CleanOptions(seed: "seed-a");
        var baseline = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-directional", source.Execution, directionalSurvey, source.Logs, baselineOptions);
        var replay = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-directional", source.Execution, directionalSurvey, source.Logs, CleanOptions(seed: "seed-a"));
        var alternateSeed = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-directional", source.Execution, directionalSurvey, source.Logs, CleanOptions(seed: "seed-b"));
        var biased = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-directional", source.Execution, directionalSurvey, source.Logs, CleanOptions(seed: "seed-a", temperatureBiasC: 5, pressureBiasPa: 100_000));
        Assert.Multiple(() =>
        {
            Assert.That(CanonicalJson.Serialize(replay.Samples), Is.EqualTo(CanonicalJson.Serialize(baseline.Samples)));
            Assert.That(CanonicalJson.Serialize(alternateSeed.Samples), Is.Not.EqualTo(CanonicalJson.Serialize(baseline.Samples)));
            Assert.That(baseline.Samples[10].DownholeTemperatureC, Is.EqualTo(baseline.Samples[0].DownholeTemperatureC));
            Assert.That(baseline.Samples[^1].DownholeTemperatureC!.Value, Is.LessThan(baseline.Samples[10].DownholeTemperatureC!.Value));
            Assert.That(biased.Samples.All(x => x.DownholeTemperatureC is not null && x.DownholeTemperatureK == x.DownholeTemperatureC + 273.15), Is.True);
            Assert.That(biased.Samples[0].DownholeTemperatureC!.Value - baseline.Samples[0].DownholeTemperatureC!.Value, Is.EqualTo(5).Within(1e-12));
            Assert.That(biased.Samples[0].StandpipePressurePa!.Value - baseline.Samples[0].StandpipePressurePa!.Value, Is.EqualTo(100_000).Within(1e-8));
        });
    }

    [Test]
    public void SourceAndPublicationReference_ContainOnlyObservableAggregateProvenance()
    {
        string modelSource=File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,"Source","DrillingObservations.cs"));string persistenceSource=File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,"Source","Persistence.DrillingObservations.cs"));
        foreach(string forbidden in new[]{"TruthSampleBatch","TruthSampleDto","OilSaturation","WaterSaturation","GasSaturation","Porosity","PermeabilityM2"})Assert.Multiple(()=>{Assert.That(modelSource,Does.Not.Contain(forbidden));Assert.That(persistenceSource,Does.Not.Contain(forbidden));});
        var source=Source();var generated=DeterministicDrillingObservationModel.Generate(TestData.ScenarioId,"run-reference",source.Execution,source.Survey,source.Logs,CleanOptions());var artifact=new DrillingObservationArtifact(Guid.NewGuid().ToString("D"),"run-reference",TestData.ScenarioId,source.Execution.ExecutionId,TestData.HashA,source.Survey.SurveyArtifactId,TestData.HashB,source.Logs.ObservationBatchId,TestData.HashC,TestData.HashD,DrillingObservationOptions.CurrentModelVersion,"cal",TestData.HashD,DeterministicDrillingObservationModel.CurveDefinitions(CleanOptions()),generated.Samples,generated.Cuttings,generated.Events,generated.Summary,TestData.HashA,TestData.HashB,DateTimeOffset.UtcNow);PublicationDrillingObservationReference reference=DeterministicPublicationPlanner.DrillingObservationReference(artifact)!;string json=CanonicalJson.Serialize(reference);Assert.Multiple(()=>{Assert.That(reference.SampleCount,Is.EqualTo(generated.Samples.Count));Assert.That(json,Does.Not.Contain("samples").And.Not.Contain("measuredDepth").And.Not.Contain("facies"));});
    }

    private static DrillingObservationOptions CleanOptions(string calibrationVersion="cal-v1",double cuttingsLagVolumeM3=0,double depthUncertaintyM=0,double lossThresholdKgM3=1280,double missingProbability=0,double spacingM=10,int maximumSamples=10_000,double maximumLogInterpolationM=100,string seed="seed-v1",double temperatureBiasC=0,double pressureBiasPa=0)=>new(){CalibrationVersion=calibrationVersion,Seed=seed,SampleSpacingM=spacingM,MaximumSamples=maximumSamples,MaximumObservableLogInterpolationM=maximumLogInterpolationM,CuttingsSampleSpacingM=10,CuttingsLagVolumeM3=cuttingsLagVolumeM3,CuttingsDepthUncertaintyM=depthUncertaintyM,LossZoneEcdThresholdKgM3=lossThresholdKgM3,VibrationNoiseStdDevG=0,MechanicalNoiseFraction=0,FlowNoiseStdDevFraction=0,PressureNoiseStdDevPa=0,TemperatureNoiseStdDevC=0,TemperatureBiasC=temperatureBiasC,PressureBiasPa=pressureBiasPa,SensorMissingProbability=missingProbability};
    internal static (DrillingExecutionArtifact Execution,SurveyArtifact Survey,LogObservationArtifact Logs) Source()
    {
        DrilledPathStation[] path=[new(0,100,0,0),new(100,200,0,0),new(200,300,0,0)];DrillingTimelinePoint[] timeline=[new(0,0),new(1,12000),new(2,24000)];var execution=new DrillingExecutionArtifact("execution","run","plan",DeterministicDrillingModel.ModelVersion,TestData.HashA,"hardware",path,timeline,0,24000,TestData.HashA,TestData.HashA,DateTimeOffset.Parse("2025-01-01T00:00:00Z"));SurveyStation[] stations=[Station(0,100),Station(100,200),Station(200,300)];var survey=new SurveyArtifact("survey","run","execution","survey-v1","cal",TestData.HashA,stations,TestData.HashA,TestData.HashA,TestData.HashB,0,24000,DateTimeOffset.Parse("2025-01-01T00:00:00Z"));LogObservationSample[] logs=[new(0,20,2300,.2,10,.216,["Valid"]),new(100,120,2500,.25,2,.216,["Valid"]),new(200,120,2500,.25,2,.216,["Valid"])];var logArtifact=new LogObservationArtifact("logs","run",TestData.ScenarioId,"path",LogObservationOptions.CurrentModelVersion,"cal",TestData.HashA,[],logs,[],new(0,0,0,0,0),TestData.HashA,TestData.HashC,DateTimeOffset.Parse("2025-01-01T00:00:00Z"));return(execution,survey,logArtifact);
    }
    private static LogObservationSample ValidLog(double md,double gr)=>new(md,gr,2400,.2,5,.216,["Valid"]);
    private static SurveyStation Station(double md,double tvd)=>new(md,0,0,tvd,30,0,new(1,0,0,0,1,0,0,0,1),"Valid",1,1);
}

[TestFixture,NonParallelizable]
public sealed class DrillingObservationPersistenceTests
{
    [Test]
    public void OutputHash_ExcludesCreatedUtcAcrossRetryClocks()
    {
        var source = DrillingObservationModelTests.Source();
        var options = new DrillingObservationOptions { MaximumObservableLogInterpolationM = 100, SensorMissingProbability = 0 };
        var generated = DeterministicDrillingObservationModel.Generate(TestData.ScenarioId, "run-hash", source.Execution, source.Survey, source.Logs, options);
        var firstClock = new FixedTimeProvider(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        var retryClock = new FixedTimeProvider(DateTimeOffset.Parse("2025-02-01T00:00:00Z"));
        DrillingObservationArtifact first = Artifact(firstClock.GetUtcNow());
        DrillingObservationArtifact retry = Artifact(retryClock.GetUtcNow());
        Assert.Multiple(() =>
        {
            Assert.That(first.CreatedUtc, Is.Not.EqualTo(retry.CreatedUtc));
            Assert.That(DrillingOperationsStore.ComputeDrillingObservationOutputHash(first), Is.EqualTo(DrillingOperationsStore.ComputeDrillingObservationOutputHash(retry)));
        });
        DrillingObservationArtifact Artifact(DateTimeOffset created) => new("artifact", "run-hash", TestData.ScenarioId, source.Execution.ExecutionId, TestData.HashA, source.Survey.SurveyArtifactId, TestData.HashB, source.Logs.ObservationBatchId, TestData.HashC, TestData.HashD, options.ModelVersion, options.CalibrationVersion, options.Hash(), DeterministicDrillingObservationModel.CurveDefinitions(options), generated.Samples, generated.Cuttings, generated.Events, generated.Summary, TestData.HashD, string.Empty, created);
    }

    [Test]
    public async Task RealS5Retry_WithDifferentAuditClocks_PreservesP4IdentityAndContentHash()
    {
        var preparationClock = new FixedTimeProvider(DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        await using var firstFixture = new StoreFixture(preparationClock);
        await using var retryFixture = new StoreFixture(preparationClock);
        RunResponse firstRun = await PrepareThroughS4Async(firstFixture, "stable-log-provenance");
        RunResponse retryRun = await PrepareThroughS4Async(retryFixture, "stable-log-provenance");
        Assert.That(retryRun.RunId, Is.EqualTo(firstRun.RunId));
        var firstStore = new DrillingOperationsStore(firstFixture.ConnectionString, new FixedTimeProvider(DateTimeOffset.Parse("2025-02-01T00:00:00Z")));
        var retryStore = new DrillingOperationsStore(retryFixture.ConnectionString, new FixedTimeProvider(DateTimeOffset.Parse("2025-03-01T00:00:00Z")));
        await firstStore.InitializeAsync(); await retryStore.InitializeAsync();
        Assert.That(await firstStore.GenerateLogsAsync(firstRun.RunId, new LogObservationOptions(), new DrillingObservationOptions()), Is.True);
        Assert.That(await retryStore.GenerateLogsAsync(retryRun.RunId, new LogObservationOptions(), new DrillingObservationOptions()), Is.True);
        LogObservationArtifact firstLog = (await firstStore.GetLogObservationBatchAsync(firstRun.RunId))!;
        LogObservationArtifact retryLog = (await retryStore.GetLogObservationBatchAsync(retryRun.RunId))!;
        DrillingObservationArtifact first = (await firstStore.GetDrillingObservationArtifactAsync(firstRun.RunId))!;
        DrillingObservationArtifact retry = (await retryStore.GetDrillingObservationArtifactAsync(retryRun.RunId))!;
        var firstCommitment = await ReadS5CommitmentAsync(firstFixture.ConnectionString, firstRun.RunId);
        var retryCommitment = await ReadS5CommitmentAsync(retryFixture.ConnectionString, retryRun.RunId);
        Assert.Multiple(() =>
        {
            Assert.That(firstLog.CreatedUtc, Is.Not.EqualTo(retryLog.CreatedUtc));
            Assert.That(firstLog.OutputHash, Is.Not.EqualTo(retryLog.OutputHash));
            Assert.That(DrillingOperationsStore.ComputeLogObservationContentHash(firstLog), Is.EqualTo(DrillingOperationsStore.ComputeLogObservationContentHash(retryLog)));
            Assert.That(first.LogHash, Is.EqualTo(firstLog.OutputHash));
            Assert.That(retry.LogHash, Is.EqualTo(retryLog.OutputHash));
            Assert.That(first.LogContentHash, Is.EqualTo(retry.LogContentHash));
            Assert.That(first.ArtifactId, Is.EqualTo(retry.ArtifactId));
            Assert.That(first.InputHash, Is.EqualTo(retry.InputHash));
            Assert.That(first.OutputHash, Is.EqualTo(retry.OutputHash));
            Assert.That(CanonicalJson.Serialize(first.Samples), Is.EqualTo(CanonicalJson.Serialize(retry.Samples)));
            Assert.That(firstCommitment.InputJson, Is.EqualTo(retryCommitment.InputJson));
            Assert.That(firstCommitment.InputHash, Is.EqualTo(retryCommitment.InputHash));
            Assert.That(firstCommitment.OutputJson, Is.EqualTo(retryCommitment.OutputJson));
            Assert.That(firstCommitment.OutputHash, Is.EqualTo(retryCommitment.OutputHash));
            Assert.That(firstCommitment.InputJson, Does.Contain(first.LogContentHash).And.Not.Contain(first.LogHash));
            Assert.That(firstCommitment.OutputJson, Does.Contain(first.LogContentHash).And.Not.Contain(first.LogHash));
        });
        string legacyInput = CanonicalJson.Serialize(new { contractVersion = "s5-combined-observations-v3", logObservation = new { observationBatchId = firstLog.ObservationBatchId, inputHash = firstLog.InputHash, artifactHash = firstLog.OutputHash }, drillingObservation = new { artifactId = first.ArtifactId, first.InputHash, artifactHash = first.OutputHash } });
        string legacyOutput = CanonicalJson.Serialize(new { contractVersion = "s5-combined-observations-v3", logObservation = new { observationBatchId = firstLog.ObservationBatchId, artifactHash = firstLog.OutputHash, sampleCount = firstLog.Samples.Count, pressureTestCount = firstLog.PressureTests.Count, observationModelVersion = firstLog.ObservationModelVersion, qcSummary = firstLog.QcSummary }, drillingObservation = new { artifactId = first.ArtifactId, artifactHash = first.OutputHash, modelVersion = first.ModelVersion, calibrationVersion = first.CalibrationVersion, first.Summary } });
        await using (var connection = new SqliteConnection(firstFixture.ConnectionString))
        {
            await connection.OpenAsync(); await using SqliteCommand legacy = connection.CreateCommand(); legacy.CommandText = "UPDATE RunStages SET InputJson=$input,InputHash=$inputHash,OutputJson=$output,OutputHash=$outputHash WHERE RunId=$run AND Stage=5;";
            legacy.Parameters.AddWithValue("$input", legacyInput); legacy.Parameters.AddWithValue("$inputHash", DeterministicIdentity.Sha256(legacyInput)); legacy.Parameters.AddWithValue("$output", legacyOutput); legacy.Parameters.AddWithValue("$outputHash", DeterministicIdentity.Sha256(legacyOutput)); legacy.Parameters.AddWithValue("$run", firstRun.RunId); await legacy.ExecuteNonQueryAsync();
        }
        Assert.That(await firstStore.GetDrillingObservationArtifactAsync(firstRun.RunId), Is.Not.Null, "Version-aware validation must retain v3 stage commitments.");
    }

    [Test]
    public async Task VersionlessCombinedS5Commitment_WithRecomputedHashes_IsRejected()
    {
        await using var fixture = new StoreFixture(new FixedTimeProvider(DateTimeOffset.Parse("2025-01-01T00:00:00Z")));
        RunResponse run = await PrepareThroughS4Async(fixture, "versionless-s5-tamper");
        Assert.That(await fixture.Store.GenerateLogsAsync(run.RunId, new LogObservationOptions(), new DrillingObservationOptions()), Is.True);
        LogObservationArtifact log = (await fixture.Store.GetLogObservationBatchAsync(run.RunId))!;
        DrillingObservationArtifact artifact = (await fixture.Store.GetDrillingObservationArtifactAsync(run.RunId))!;
        string input = CanonicalJson.Serialize(new { logObservation = new { observationBatchId = log.ObservationBatchId, inputHash = log.InputHash, contentHash = artifact.LogContentHash }, drillingObservation = new { artifactId = artifact.ArtifactId, artifact.InputHash, artifactHash = artifact.OutputHash } });
        string output = CanonicalJson.Serialize(new { logObservation = new { observationBatchId = log.ObservationBatchId, contentHash = artifact.LogContentHash, sampleCount = log.Samples.Count, pressureTestCount = log.PressureTests.Count, observationModelVersion = log.ObservationModelVersion, qcSummary = log.QcSummary }, drillingObservation = new { artifactId = artifact.ArtifactId, artifactHash = artifact.OutputHash, modelVersion = artifact.ModelVersion, calibrationVersion = artifact.CalibrationVersion, artifact.Summary } });
        await using (var connection = new SqliteConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync(); await using SqliteCommand tamper = connection.CreateCommand(); tamper.CommandText = "UPDATE RunStages SET InputJson=$input,InputHash=$inputHash,OutputJson=$output,OutputHash=$outputHash WHERE RunId=$run AND Stage=5;";
            tamper.Parameters.AddWithValue("$input", input); tamper.Parameters.AddWithValue("$inputHash", DeterministicIdentity.Sha256(input)); tamper.Parameters.AddWithValue("$output", output); tamper.Parameters.AddWithValue("$outputHash", DeterministicIdentity.Sha256(output)); tamper.Parameters.AddWithValue("$run", run.RunId); await tamper.ExecuteNonQueryAsync();
        }
        Assert.Multiple(() => { Assert.That(input, Does.Not.Contain("contractVersion")); Assert.That(output, Does.Not.Contain("contractVersion")); });
        Assert.ThrowsAsync<PersistenceIntegrityException>(async () => await fixture.Store.GetDrillingObservationArtifactAsync(run.RunId));
    }

    [Test]
    public async Task MigratedLegacyArtifact_WithNullLogContentHash_FailsWithExplicitCompatibilityError()
    {
        await using var fixture = new StoreFixture(new FixedTimeProvider(DateTimeOffset.Parse("2025-01-01T00:00:00Z")));
        RunResponse run = await PrepareThroughS4Async(fixture, "legacy-null-log-content");
        Assert.That(await fixture.Store.GenerateLogsAsync(run.RunId, new LogObservationOptions(), new DrillingObservationOptions()), Is.True);
        await using (var connection = new SqliteConnection(fixture.ConnectionString))
        {
            await connection.OpenAsync(); await using SqliteCommand legacy = connection.CreateCommand();
            legacy.CommandText = "DROP TRIGGER TR_DrillingObservationArtifacts_NoUpdate; UPDATE DrillingObservationArtifacts SET LogContentHash=NULL WHERE RunId=$run;"; legacy.Parameters.AddWithValue("$run", run.RunId); await legacy.ExecuteNonQueryAsync();
        }
        var restarted = new DrillingOperationsStore(fixture.ConnectionString, TimeProvider.System); await restarted.InitializeAsync();
        PersistenceIntegrityException error = Assert.ThrowsAsync<PersistenceIntegrityException>(async () => await restarted.GetDrillingObservationArtifactAsync(run.RunId))!;
        Assert.That(error.Message, Does.Contain("predates stable log-content provenance"));
    }

    [Test]
    public async Task S5PersistsCombinedImmutableArtifact_RestartsAndDetectsTamper()
    {
        await using var factory=new ApiFactory();using HttpClient client=factory.CreateInternalClient();using var bind=new HttpRequestMessage(HttpMethod.Post,$"/drillingoperations/api/scenarios/{TestData.ScenarioId}/bind"){Content=JsonContent.Create(TestData.Binding())};bind.Headers.Add("Idempotency-Key","drilling-observation-bind");(await client.SendAsync(bind)).EnsureSuccessStatusCode();using var create=new HttpRequestMessage(HttpMethod.Post,"/drillingoperations/api/runs"){Content=JsonContent.Create(TestData.Run())};create.Headers.Add("Idempotency-Key","drilling-observation-run");using HttpResponseMessage created=await client.SendAsync(create);created.EnsureSuccessStatusCode();RunResponse run=(await created.Content.ReadFromJsonAsync<RunResponse>(CanonicalJson.SerializerOptions))!;await Wait(client,run.RunId);
        var store=factory.Services.GetRequiredService<DrillingOperationsStore>();DrillingObservationArtifact artifact=(await store.GetDrillingObservationArtifactAsync(run.RunId))!;Assert.That(artifact,Is.Not.Null);Assert.That(await store.GenerateLogsAsync(run.RunId,new LogObservationOptions(),new DrillingObservationOptions()),Is.False);var restarted=new DrillingOperationsStore($"Data Source={factory.DatabasePath}",TimeProvider.System);await restarted.InitializeAsync();DrillingObservationArtifact replay=(await restarted.GetDrillingObservationArtifactAsync(run.RunId))!;await using var connection=new SqliteConnection($"Data Source={factory.DatabasePath}");await connection.OpenAsync();await using(var count=connection.CreateCommand()){count.CommandText="SELECT COUNT(*) FROM DrillingObservationArtifacts WHERE RunId=$run;";count.Parameters.AddWithValue("$run",run.RunId);Assert.That(Convert.ToInt32(await count.ExecuteScalarAsync()),Is.EqualTo(1));}await using var stage=connection.CreateCommand();stage.CommandText="SELECT InputJson,OutputJson,OutputHash FROM RunStages WHERE RunId=$run AND Stage=5;";stage.Parameters.AddWithValue("$run",run.RunId);await using SqliteDataReader reader=await stage.ExecuteReaderAsync();Assert.That(await reader.ReadAsync(),Is.True);string stageInput=reader.GetString(0),stageOutput=reader.GetString(1),stageHash=reader.GetString(2);await reader.CloseAsync();Assert.Multiple(()=>{Assert.That(CanonicalJson.Serialize(replay),Is.EqualTo(CanonicalJson.Serialize(artifact)));Assert.That(stageInput,Does.Contain(DrillingOperationsStore.CombinedS5ObservationContractVersion).And.Contain(artifact.ArtifactId).And.Contain(artifact.OutputHash));Assert.That(stageOutput,Does.Contain(artifact.ArtifactId).And.Contain(artifact.OutputHash));Assert.That(stageHash,Is.EqualTo(DeterministicIdentity.Sha256(stageOutput)));Assert.That(CanonicalJson.Serialize(artifact),Does.Not.Contain("oilSaturation").And.Not.Contain("worldId").And.Not.Contain("bindingId"));});await using(var persisted=connection.CreateCommand()){persisted.CommandText="SELECT CanonicalOutputJson,OutputHash,CreatedUtc FROM DrillingObservationArtifacts WHERE ArtifactId=$id;";persisted.Parameters.AddWithValue("$id",artifact.ArtifactId);await using SqliteDataReader outputReader=await persisted.ExecuteReaderAsync();Assert.That(await outputReader.ReadAsync(),Is.True);string canonicalOutput=outputReader.GetString(0);Assert.Multiple(()=>{Assert.That(canonicalOutput.Contains("createdUtc",StringComparison.OrdinalIgnoreCase),Is.False);Assert.That(outputReader.GetString(1),Is.EqualTo(DeterministicIdentity.Sha256(canonicalOutput)));Assert.That(outputReader.GetString(2),Is.EqualTo(artifact.CreatedUtc.ToString("O")));});}await using var blocked=connection.CreateCommand();blocked.CommandText="UPDATE DrillingObservationSeries SET SamplesHash=$hash WHERE ArtifactId=$id;";blocked.Parameters.AddWithValue("$hash",TestData.HashD);blocked.Parameters.AddWithValue("$id",artifact.ArtifactId);Assert.ThrowsAsync<SqliteException>(async()=>await blocked.ExecuteNonQueryAsync());await using var tamper=connection.CreateCommand();tamper.CommandText="DROP TRIGGER TR_DrillingObservationSeries_NoUpdate;UPDATE DrillingObservationSeries SET SamplesHash=$hash WHERE ArtifactId=$id;";tamper.Parameters.AddWithValue("$hash",TestData.HashD);tamper.Parameters.AddWithValue("$id",artifact.ArtifactId);await tamper.ExecuteNonQueryAsync();Assert.ThrowsAsync<PersistenceIntegrityException>(async()=>await restarted.GetDrillingObservationArtifactAsync(run.RunId));
    }
    private static async Task<(string InputJson,string InputHash,string OutputJson,string OutputHash)> ReadS5CommitmentAsync(string connectionString, string runId)
    {
        await using var connection = new SqliteConnection(connectionString); await connection.OpenAsync(); await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT InputJson,InputHash,OutputJson,OutputHash FROM RunStages WHERE RunId=$run AND Stage=5;"; command.Parameters.AddWithValue("$run", runId); await using SqliteDataReader reader = await command.ExecuteReaderAsync(); Assert.That(await reader.ReadAsync(), Is.True);
        return (reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
    }
    private static async Task<RunResponse> PrepareThroughS4Async(StoreFixture fixture, string suffix)
    {
        await fixture.InitializeAsync(); RunResponse run = await StoreFixture.BindAndCreateAsync(fixture.Store, suffix: suffix);
        await fixture.Store.CompleteS0Async(run.RunId); await fixture.Store.MaterializePlanAsync(run.RunId, TestData.ScenarioSnapshot(), TestData.PredictionSnapshot()); await fixture.Store.ExecuteDeterministicDrillingAsync(run.RunId, new()); await fixture.Store.GenerateSurveyAsync(run.RunId, new());
        TruthBindingResponse binding = (await fixture.Store.GetBindingAsync(run.ScenarioId))!; DrillingExecutionArtifact execution = (await fixture.Store.GetDrillingExecutionAsync(run.RunId))!;
        using var stageClient = new HttpClient(new StageASamplingHandler(TestData.Upstream)) { BaseAddress = new Uri("http://reservoir.test/") };
        TruthSamplingEnvelope envelope = await new ReservoirSamplingClient(stageClient).SampleAsync(binding, execution, new TruthSamplingOptions(), CancellationToken.None);
        Assert.That(await fixture.Store.PersistTruthSamplesAsync(run.RunId, envelope), Is.True); return run;
    }
    private static async Task Wait(HttpClient client,string runId){for(int i=0;i<300;i++){RunResponse run=(await client.GetFromJsonAsync<RunResponse>($"/drillingoperations/api/runs/{runId}",CanonicalJson.SerializerOptions))!;if(run.CurrentStage==RunStageKind.S6DesignCompletion&&run.Status==RunStatus.AwaitingApproval)return;await Task.Delay(20);}throw new AssertionException("Run did not reach S6.");}
}






internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}



