using System.Text.Json;
namespace DrillingOperations.Tests;

[TestFixture]
public sealed class DeterministicScoringTests
{
 [Test]
 public void HandComputableFixture_ProducesDualBasisCoverageFluidAndProductionMetrics()
 {
  var fixture=Fixture();IReadOnlyList<ScorecardMetric> metrics=DeterministicScoringModel.Calculate("Reservoir",fixture.Prediction,fixture.Baselines,fixture.Truth,fixture.Logs,fixture.Completion,fixture.Survey,fixture.ProductionTruth,fixture.Series);
  Assert.Multiple(()=>{Assert.That(Value(metrics,"formationTopP50AbsoluteError.HiddenTruth"),Is.EqualTo(2).Within(1e-12));Assert.That(Value(metrics,"formationTopP90P10Coverage.HiddenTruth"),Is.EqualTo(1));Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.HiddenTruth"),Is.EqualTo(10).Within(1e-12));Assert.That(Value(metrics,"fluidClassPrecision.HiddenTruth"),Is.EqualTo(5d/6).Within(1e-12));Assert.That(Value(metrics,"fluidClassRecall.HiddenTruth"),Is.EqualTo(1).Within(1e-12));Assert.That(Value(metrics,"fluidClassWeightedF1.HiddenTruth"),Is.EqualTo(.92).Within(1e-12));Assert.That(Value(metrics,"monthlyOilRmse.HiddenTruth"),Is.Zero.Within(1e-12));Assert.That(Value(metrics,"monthlyGasRmse.HiddenTruth"),Is.Zero.Within(1e-12));Assert.That(Value(metrics,"cumulativeOilRelativeError.Y5.HiddenTruth"),Is.Zero.Within(1e-12));Assert.That(metrics.Single(x=>x.Name=="rankingRegret.HiddenTruth").Status,Is.EqualTo(ScoreMetricStatus.Unavailable));Assert.That(metrics.Single(x=>x.Name=="rankingRegret.HiddenTruth").Limitation,Does.Contain("single decision"));Assert.That(metrics.All(x=>x.Value is null||double.IsFinite(x.Value.Value)),Is.True);Assert.That(metrics.Where(x=>x.Status==ScoreMetricStatus.Scored).All(x=>x.Value is not null&&!string.IsNullOrWhiteSpace(x.Unit)&&x.Limitation is null),Is.True);Assert.That(metrics.Where(x=>x.Status==ScoreMetricStatus.Unavailable).All(x=>x.Value is null&&!string.IsNullOrWhiteSpace(x.Limitation)),Is.True);});
 }
 [Test]
 public void MetricContract_RejectsScoredLimitationsAndUnavailableWithoutReasons()
 {
  Assert.Multiple(()=>
  {
   Assert.Throws<ArgumentException>(()=>ScoreMetricContract.Validate([new("bad",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Scored,1,"m",null,null,"not allowed")]));
   Assert.Throws<ArgumentException>(()=>ScoreMetricContract.Validate([new("bad",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Unavailable,null,"m",null,null,null)]));
   Assert.DoesNotThrow(()=>ScoreMetricContract.Validate([new("score",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Scored,1,"m",null,null,null),new("missing",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Unavailable,null,"m",null,null,"Not inferable.")]));
  });
 }
 [Test]
 public void PersistenceRejectsInvalidMetricContractBeforeOpeningDatabase()
 {
  string json=CanonicalJson.Serialize(new AnalysisScorecardRequest(Guid.NewGuid().ToString("D"),Guid.NewGuid().ToString("D"),Guid.NewGuid().ToString("D"),ScoreArtifactIntegrity.ModelVersion,TestData.HashA,"expectedPaydirtP50AbsoluteError.HiddenTruth",[new("bad",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Scored,1,"m",null,null,"not allowed")],DateTimeOffset.UtcNow));var draft=new ScorecardDraft(Guid.NewGuid().ToString("D"),Guid.NewGuid().ToString("D"),TestData.ScenarioId,Guid.NewGuid().ToString("D"),ScoreArtifactIntegrity.ModelVersion,TestData.HashA,"{}",json,DeterministicIdentity.Sha256(json),"expectedPaydirtP50AbsoluteError.HiddenTruth",[new("bad",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Scored,1,"m",null,null,"not allowed")],DateTimeOffset.UtcNow);var store=new DrillingOperationsStore("Data Source=invalid-metric-contract.db",TimeProvider.System);Assert.ThrowsAsync<PersistenceIntegrityException>(async()=>await store.PersistScorecardAsync(draft));
 }
 [Test]
 public void CoverageEdges_AreInclusive_AndMaterialMissFailsCoverage()
 {
  var f=Fixture();var atEdge=f.Prediction with{Formations=[f.Prediction.Formations![0] with{TopTrueVerticalDepthM=new(102,105,120)}]};var metrics=Calculate(f,atEdge);Assert.That(Value(metrics,"formationTopP90P10Coverage.HiddenTruth"),Is.EqualTo(1));var miss=atEdge with{Formations=[atEdge.Formations![0] with{TopTrueVerticalDepthM=new(103,105,120)}]};Assert.That(Value(Calculate(f,miss),"formationTopP90P10Coverage.HiddenTruth"),Is.Zero);
 }
 [Test]
 public void MissingMonths_AreExcluded_AndZeroDenominatorsAreUnavailable()
 {
  var f=Fixture(zeroWater:true,missingMonth:true);var metrics=Calculate(f,f.Prediction);ScorecardMetric observedRmse=metrics.Single(x=>x.Name=="monthlyOilRmse.RevealedObservation");Assert.Multiple(()=>{Assert.That(observedRmse.Limitation,Is.Null);Assert.That(Value(metrics,"monthlyOilScoredMonthCount.HiddenTruth"),Is.EqualTo(60));Assert.That(Value(metrics,"monthlyOilScoredMonthCount.RevealedObservation"),Is.EqualTo(59));Assert.That(metrics.Single(x=>x.Name=="initialWaterRateRelativeError.HiddenTruth").Status,Is.EqualTo(ScoreMetricStatus.Unavailable));Assert.That(metrics.Single(x=>x.Name=="monthlyWaterNrmse.HiddenTruth").Status,Is.EqualTo(ScoreMetricStatus.Unavailable));Assert.That(metrics.Single(x=>x.Name=="cumulativeWaterRelativeError.Y1.HiddenTruth").Status,Is.EqualTo(ScoreMetricStatus.Unavailable));});
 }
 [Test]
 public void ContactsBaselinesAndObservationGaps_AreDeterministicAndHandComputable()
 {
  var f=Fixture(missingMonth:false);IReadOnlyList<ScorecardMetric> metrics=Calculate(f,f.Prediction);
  Assert.Multiple(()=>
  {
   Assert.That(Value(metrics,"owcDepthP50AbsoluteError.HiddenTruth"),Is.EqualTo(25).Within(1e-12));
   Assert.That(Value(metrics,"owcDepthP50AbsoluteError.RevealedObservation"),Is.EqualTo(25).Within(1e-12));
   Assert.That(Value(metrics,"owcDepthP50AbsoluteError.ObservationGap"),Is.Zero.Within(1e-12));
   Assert.That(Value(metrics,"formationTopP50AbsoluteError.ObservationGap"),Is.EqualTo(2).Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.RevealedObservation"),Is.EqualTo(40).Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.ObservationGap"),Is.EqualTo(30).Within(1e-12));
   Assert.That(Value(metrics,"fluidClassPrecision.RevealedObservation"),Is.EqualTo(.5).Within(1e-12));
   Assert.That(Value(metrics,"fluidClassRecall.RevealedObservation"),Is.EqualTo(1).Within(1e-12));
   Assert.That(Value(metrics,"fluidClassWeightedF1.RevealedObservation"),Is.EqualTo(.7).Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.Baseline.NearestWell"),Is.EqualTo(20).Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.Baseline.FieldMean"),Is.EqualTo(10).Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.Baseline.FourNeighborIdw"),Is.Zero.Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtP50AbsoluteError.Baseline.UncertaintyAwareRank1"),Is.EqualTo(10).Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtBestBaselineAbsoluteError.Baseline"),Is.Zero.Within(1e-12));
   Assert.That(Value(metrics,"expectedPaydirtAiMinusBestBaselineAbsoluteError.Baseline"),Is.EqualTo(10).Within(1e-12));
  });
 }
 [Test]
 public void GasOilAndGasWaterContacts_AreScoredWhenTransitionsAreDeterministicallyInferable()
 {
  var f=Fixture(missingMonth:false);TruthSampleDto gas=f.Truth[0] with{OilSaturation=.3,WaterSaturation=0,GasSaturation=.7};LogObservationSample gasLog=f.Logs.Samples[0] with{RhobKgM3=2100,NphiFraction=.1,DeepResistivityOhmM=10};
  var gocPrediction=f.Prediction with{ContactPredictions=[new("GOC",new(110,120,130)),new("OWC",new(140,150,160))]};var goc=f with{Prediction=gocPrediction,Truth=[gas,f.Truth[1],f.Truth[2]],Logs=f.Logs with{Samples=[gasLog,f.Logs.Samples[1],f.Logs.Samples[2]]}};IReadOnlyList<ScorecardMetric> gocMetrics=Calculate(goc,goc.Prediction);
  TruthSampleDto water=f.Truth[1] with{OilSaturation=0,WaterSaturation=1,GasSaturation=0};LogObservationSample waterLog=f.Logs.Samples[1] with{RhobKgM3=2400,NphiFraction=.25,DeepResistivityOhmM=1};var gwcPrediction=f.Prediction with{ContactPredictions=[new("GWC",new(110,120,130))]};var gwc=f with{Prediction=gwcPrediction,Truth=[gas,water,f.Truth[2]],Logs=f.Logs with{Samples=[gasLog,waterLog,f.Logs.Samples[2]]}};IReadOnlyList<ScorecardMetric> gwcMetrics=Calculate(gwc,gwc.Prediction);
  Assert.Multiple(()=>
  {
   Assert.That(Value(gocMetrics,"gocDepthP50AbsoluteError.HiddenTruth"),Is.EqualTo(5).Within(1e-12));
   Assert.That(Value(gocMetrics,"gocDepthP50AbsoluteError.RevealedObservation"),Is.EqualTo(5).Within(1e-12));
   Assert.That(Value(gwcMetrics,"gwcDepthP50AbsoluteError.HiddenTruth"),Is.EqualTo(5).Within(1e-12));
   Assert.That(Value(gwcMetrics,"gwcDepthP50AbsoluteError.RevealedObservation"),Is.EqualTo(5).Within(1e-12));
  });
 }
 [Test]
 public void ContactsAndUnsupportedRankingMetrics_AreExplicitlyUnavailable()
 {
  var f=Fixture();IReadOnlyList<ScorecardMetric> metrics=Calculate(f,f.Prediction);
  Assert.Multiple(()=>
  {
   foreach(string contact in new[]{"goc","gwc"})
   {
    Assert.That(metrics.Single(x=>x.Name==$"{contact}DepthP50AbsoluteError.HiddenTruth").Status,Is.EqualTo(ScoreMetricStatus.Unavailable));
    Assert.That(metrics.Single(x=>x.Name==$"{contact}DepthP50AbsoluteError.RevealedObservation").Status,Is.EqualTo(ScoreMetricStatus.Unavailable));
   }
   foreach(string name in new[]{"targetDistanceToBestHiddenCandidate.HiddenTruth","spearmanRankCorrelation.HiddenTruth","rankingRegret.HiddenTruth","crps.HiddenTruth","pit.HiddenTruth","empiricalCalibrationCoverage.HiddenTruth"})
   {
    ScorecardMetric metric=metrics.Single(x=>x.Name==name);Assert.That(metric.Status,Is.EqualTo(ScoreMetricStatus.Unavailable));Assert.That(metric.Value,Is.Null);Assert.That(metric.Limitation,Is.Not.Empty);
   }
  });
 }
 [TestCase(80,10,30)]
 [TestCase(90,0,20)]
 [TestCase(100,0,10)]
 [TestCase(102,0,12)]
 [TestCase(110,0,20)]
 [TestCase(120,10,30)]
 public void AbsoluteErrorBoundsCoverTheWholeQuantileInterval(double actual,double lower,double upper)
 {
  var f=Fixture();f=f with{Truth=f.Truth.Select(x=>x with{ReservoirTopDepthM=actual}).ToArray()};
  var metrics=Calculate(f,f.Prediction);var metric=metrics.Single(x=>x.Name=="formationTopP50AbsoluteError.HiddenTruth");
  Assert.Multiple(()=>
  {
   Assert.That(metric.Value,Is.EqualTo(Math.Abs(100-actual)));
   Assert.That(metric.LowerBound,Is.EqualTo(lower));
   Assert.That(metric.UpperBound,Is.EqualTo(upper));
   Assert.DoesNotThrow(()=>ScoreMetricContract.ValidateForPublication(metrics));
  });
 }
 [Test]
 public void DegenerateQuantileIntervalHasExactAbsoluteErrorBounds()
 {
  var f=Fixture();var prediction=f.Prediction with{Formations=[f.Prediction.Formations![0] with{TopTrueVerticalDepthM=new(102,102,102)}]};
  var metric=Calculate(f,prediction).Single(x=>x.Name=="formationTopP50AbsoluteError.HiddenTruth");
  Assert.Multiple(()=>{Assert.That(metric.Value,Is.Zero);Assert.That(metric.LowerBound,Is.Zero);Assert.That(metric.UpperBound,Is.Zero);});
 }
 [Test]
 public void ContactAndBaselineErrorBoundsIncludeZeroWithoutChangingImmutablePredictions()
 {
  var f=Fixture();f=f with{Prediction=f.Prediction with{ContactPredictions=[new("OWC",new(150,175,200))]},
   Baselines=f.Baselines.Select(x=>x with{ExpectedPaydirtM=new(180,200,220)}).ToArray()};
  string prediction=CanonicalJson.Serialize(f.Prediction),baselines=CanonicalJson.Serialize(f.Baselines);
  var metrics=Calculate(f,f.Prediction);
  foreach(var metric in metrics.Where(x=>x.Name is "owcDepthP50AbsoluteError.HiddenTruth" or "owcDepthP50AbsoluteError.RevealedObservation"||x.Name.StartsWith("expectedPaydirtP50AbsoluteError.Baseline.",StringComparison.Ordinal)))
  {
   Assert.That(metric.Value,Is.Zero);
   Assert.That(metric.LowerBound,Is.Zero);
   Assert.That(metric.UpperBound,Is.EqualTo(metric.Basis==ScoreMetricBasis.Baseline?20:25));
  }
  Assert.That(CanonicalJson.Serialize(f.Prediction),Is.EqualTo(prediction));
  Assert.That(CanonicalJson.Serialize(f.Baselines),Is.EqualTo(baselines));
  Assert.DoesNotThrow(()=>ScoreMetricContract.ValidateForPublication(metrics));
 }
 [TestCase(0,1,2)]
 [TestCase(3,1,2)]
 [TestCase(1,null,2)]
 [TestCase(1,0,null)]
 [TestCase(1,2,0)]
 [TestCase(1,0,1e16)]
 public void OutboundBoundsValidationRejectsIncompatibleNewMetricsButDoesNotInvalidateHistoricalReads(double value,double? lower,double? upper)
 {
  ScorecardMetric[] metrics=[new("error",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Scored,value,"m",lower,upper,null)];
  string prior=CanonicalJson.Serialize(metrics);
  Assert.DoesNotThrow(()=>ScoreMetricContract.Validate(metrics));
  Assert.Throws<ArgumentException>(()=>ScoreMetricContract.ValidateForPublication(metrics));
  Assert.That(CanonicalJson.Serialize(metrics),Is.EqualTo(prior));
 }
 [Test]
 public void InvalidNewBoundsAreRejectedBeforeScorecardPersistence()
 {
  ScorecardMetric[] metrics=[new("error",ScoreMetricBasis.HiddenTruth,ScoreMetricStatus.Scored,0,"m",1,2,null)];
  var draft=new ScorecardDraft("card","run",TestData.ScenarioId,"reveal",ScoreArtifactIntegrity.ModelVersion,
   TestData.HashA,"{}","{}",TestData.HashB,"error",metrics,DateTimeOffset.UtcNow);
  var store=new DrillingOperationsStore("Data Source=invalid-bound-contract.db",TimeProvider.System);
  var error=Assert.ThrowsAsync<PersistenceIntegrityException>(()=>store.PersistScorecardAsync(draft));
  Assert.That(error!.Message,Does.Contain("bounds containing its scored value"));
 }
 static IReadOnlyList<ScorecardMetric> Calculate(F f,AnalysisPredictionBodyDto p)=>DeterministicScoringModel.Calculate("Reservoir",p,f.Baselines,f.Truth,f.Logs,f.Completion,f.Survey,f.ProductionTruth,f.Series);
 static double Value(IReadOnlyList<ScorecardMetric> metrics,string name)=>metrics.Single(x=>x.Name==name).Value!.Value;
 static F Fixture(bool zeroWater=false,bool missingMonth=true)
 {
  var prediction=new AnalysisPredictionBodyDto("candidate",[new(0,100,0,0),new(200,200,0,0)],TestData.HashB,[new("Reservoir",new(90,100,110),new(190,200,210))],new(180,190,210),["Oil","Water"],[new("OWC",new(140,150,160))],[new(1,1200,60,zeroWater?0:240),new(3,3600,180,zeroWater?0:720),new(5,6000,300,zeroWater?0:1200)],["fixture"],["well:11111111-1111-4111-8111-111111111111"],"fixture");
  var baselines=new[]{"NearestWell","FieldMean","FourNeighborIdw","UncertaintyAwareRank1"}.Select((kind,i)=>new AnalysisBaselineDto(Guid.NewGuid(),TestData.HashA,Guid.Parse(TestData.ScenarioId),kind,"baseline-v1",TestData.HashB,"candidate",0,0,[],null,null,new(170+i*10,180+i*10,190+i*10),null,null,null,"fixture baseline")).ToArray();
  var truth=new[]{new TruthSampleDto(0,0,0,100,102,198,.2,1e-13,.7,true,1e7,.7,.3,0),new TruthSampleDto(100,0,0,150,102,198,.2,1e-13,.7,true,1e7,.7,.3,0),new TruthSampleDto(200,0,0,200,102,198,.2,1e-13,.7,true,1e7,0,1,0)};
  var logs=new LogObservationArtifact("logs","run",TestData.ScenarioId,"binding",LogObservationOptions.CurrentModelVersion,"cal",TestData.HashA,[],[new(0,20,2300,.2,10,.216,["Valid"]),new(100,20,2300,.2,10,.216,["Valid"]),new(200,20,2400,.25,1,.216,["Valid"])],[new(0,1e7,1,"Valid"),new(200,1.2e7,1,"Valid")],new(0,0,2,0,0),TestData.HashB,TestData.HashC,DateTimeOffset.Parse("2025-01-02T00:00:00Z"));
  var survey=new SurveyArtifact("survey","run","execution","model","cal",TestData.HashA,[Station(0,100),Station(100,150),Station(200,200)],TestData.HashB,TestData.HashC,TestData.HashD,0,1,DateTimeOffset.Parse("2025-01-02T00:00:00Z"));var options=new CompletionDesignOptions();var opening=new CompletionOpening("opening","Reservoir","Perforated",0,150,.1,0,1,1);var metadata=new CompletionBindingMetadata("completion","world","binding",Guid.Parse(TestData.ScenarioId),Guid.NewGuid(),options.ModelVersion,1,1,TestData.HashA,DateTimeOffset.Parse("2025-01-02T00:00:00Z"));var completion=new CompletionDesign("completion-design","run",TestData.ScenarioId,"logs",TestData.HashA,"Reservoir",options.ModelVersion,options.CalibrationVersion,CanonicalJson.Serialize(options),options.Hash(),"{}",TestData.HashA,[opening],DeterministicIdentity.Sha256(CanonicalJson.Serialize(new[]{opening})),metadata,TestData.HashB,"Approved",DateTimeOffset.Parse("2025-01-02T00:00:00Z"),DateTimeOffset.Parse("2025-01-02T00:00:00Z"),"operator");
  var monthly=Enumerable.Range(1,60).Select(month=>new MonthlyProductionTruth(month,month*ProductionExecutionOptions.YearSeconds/12,1,zeroWater?0:1,1,month*100,zeroWater?0:month*20,month*5,1e7,"Rate",null)).ToArray();var checkpoints=new[]{new ProductionCheckpoint(1,ProductionExecutionOptions.YearSeconds,"s1",null,new(1200,zeroWater?0:240,60),0),new ProductionCheckpoint(3,3*ProductionExecutionOptions.YearSeconds,"s3","s1",new(3600,zeroWater?0:720,180),0),new ProductionCheckpoint(5,5*ProductionExecutionOptions.YearSeconds,"s5","s3",new(6000,zeroWater?0:1200,300),0)};var productionTruth=new ProductionRunResponse("production","world","completion",ProductionExecutionOptions.CurrentModelVersion,checkpoints,monthly);var months=Enumerable.Range(1,60).Select(month=>new MeterMonth(month,100,zeroWater?0:20,5,missingMonth&&month==2?null:100,zeroWater?0:20,5,month*100,zeroWater?0:month*20,month*5,month*100,zeroWater?0:month*20,month*5,.01,"Valid")).ToArray();var observedCheckpoints=new[]{1,3,5}.Select(y=>new ProductionObservableCheckpoint(y,$"Y{y}",y*1200,zeroWater?0:y*240,y*60)).ToArray();var series=new ProductionSeriesArtifact("series","run","production",ProductionMeterOptions.CurrentModelVersion,"cal",TestData.HashA,months,observedCheckpoints,null,null,new(missingMonth?1:0,0,missingMonth?59:60),TestData.HashB,TestData.HashC,DateTimeOffset.Parse("2025-01-02T00:00:00Z"));return new(prediction,baselines,truth,logs,completion,survey,productionTruth,series);
 }
 static SurveyStation Station(double md,double tvd)=>new(md,0,0,tvd,0,0,new(1,0,0,0,1,0,0,0,1),"Valid",1,1);
 sealed record F(AnalysisPredictionBodyDto Prediction,IReadOnlyList<AnalysisBaselineDto> Baselines,IReadOnlyList<TruthSampleDto> Truth,LogObservationArtifact Logs,CompletionDesign Completion,SurveyArtifact Survey,ProductionRunResponse ProductionTruth,ProductionSeriesArtifact Series);
}
