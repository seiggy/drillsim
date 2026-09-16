using Microsoft.Data.Sqlite;
namespace DrillingOperations.Tests;

[TestFixture]
[NonParallelizable]
public sealed class StageExecutionTests
{
    [Test]
    public async Task PlanAndExecution_PersistAcrossRestart_ExactlyOnce_AndDetectTamper()
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();RunResponse run=await StoreFixture.BindAndCreateAsync(fixture.Store);
        await fixture.Store.CompleteS0Async(run.RunId);await fixture.Store.MaterializePlanAsync(run.RunId,TestData.ScenarioSnapshot(),TestData.PredictionSnapshot());await fixture.Store.ExecuteDeterministicDrillingAsync(run.RunId,new());
        MaterializedPlan plan=(await fixture.Store.GetMaterializedPlanAsync(run.RunId))!;DrillingExecutionArtifact execution=(await fixture.Store.GetDrillingExecutionAsync(run.RunId))!;
        var restarted=new DrillingOperationsStore(fixture.ConnectionString,TimeProvider.System);await restarted.InitializeAsync();
        Assert.Multiple(()=>{Assert.That(CanonicalJson.Serialize(restarted.GetMaterializedPlanAsync(run.RunId).Result),Is.EqualTo(CanonicalJson.Serialize(plan)));Assert.That(CanonicalJson.Serialize(restarted.GetDrillingExecutionAsync(run.RunId).Result),Is.EqualTo(CanonicalJson.Serialize(execution)));});
        Assert.That(await restarted.MaterializePlanAsync(run.RunId,TestData.ScenarioSnapshot(),TestData.PredictionSnapshot()),Is.False);
        Assert.That(await restarted.ExecuteDeterministicDrillingAsync(run.RunId,new()),Is.False);
        await using var connection=new SqliteConnection(fixture.ConnectionString);await connection.OpenAsync();await using SqliteCommand mutation=connection.CreateCommand();
        mutation.CommandText="UPDATE MaterializedPlans SET PathHash=$hash WHERE RunId=$run;";mutation.Parameters.AddWithValue("$hash",TestData.HashD);mutation.Parameters.AddWithValue("$run",run.RunId);
        Assert.ThrowsAsync<SqliteException>(async()=>await mutation.ExecuteNonQueryAsync());
        await using SqliteCommand tamper=connection.CreateCommand();tamper.CommandText="DROP TRIGGER TR_MaterializedPlans_NoUpdate; UPDATE MaterializedPlans SET PathHash=$hash WHERE RunId=$run;";tamper.Parameters.AddWithValue("$hash",TestData.HashD);tamper.Parameters.AddWithValue("$run",run.RunId);await tamper.ExecuteNonQueryAsync();
        Assert.ThrowsAsync<PersistenceIntegrityException>(async()=>await restarted.GetMaterializedPlanAsync(run.RunId));
    }

    [Test]
    public async Task DrillingExecution_IsImmutable_AndDetectsTamper()
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();RunResponse run=await StoreFixture.BindAndCreateAsync(fixture.Store);
        await fixture.Store.CompleteS0Async(run.RunId);await fixture.Store.MaterializePlanAsync(run.RunId,TestData.ScenarioSnapshot(),TestData.PredictionSnapshot());await fixture.Store.ExecuteDeterministicDrillingAsync(run.RunId,new());
        await using var connection=new SqliteConnection(fixture.ConnectionString);await connection.OpenAsync();
        await using SqliteCommand mutation=connection.CreateCommand();mutation.CommandText="UPDATE DrillingExecutions SET OutputHash=$hash WHERE RunId=$run;";mutation.Parameters.AddWithValue("$hash",TestData.HashD);mutation.Parameters.AddWithValue("$run",run.RunId);Assert.ThrowsAsync<SqliteException>(async()=>await mutation.ExecuteNonQueryAsync());
        await using SqliteCommand deletion=connection.CreateCommand();deletion.CommandText="DELETE FROM DrillingExecutions WHERE RunId=$run;";deletion.Parameters.AddWithValue("$run",run.RunId);Assert.ThrowsAsync<SqliteException>(async()=>await deletion.ExecuteNonQueryAsync());
        await using SqliteCommand tamper=connection.CreateCommand();tamper.CommandText="DROP TRIGGER TR_DrillingExecutions_NoUpdate; UPDATE DrillingExecutions SET OutputHash=$hash WHERE RunId=$run;";tamper.Parameters.AddWithValue("$hash",TestData.HashD);tamper.Parameters.AddWithValue("$run",run.RunId);await tamper.ExecuteNonQueryAsync();
        Assert.ThrowsAsync<PersistenceIntegrityException>(async()=>await fixture.Store.GetDrillingExecutionAsync(run.RunId));
    }

    [TestCase("seal")]
    [TestCase("package")]
    public async Task S1_RejectsChangedAuthoritativePrediction(string mismatch)
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();RunResponse run=await StoreFixture.BindAndCreateAsync(fixture.Store);await fixture.Store.CompleteS0Async(run.RunId);
        AnalysisPredictionDto prediction=TestData.PredictionSnapshot();
        prediction=mismatch=="seal"?prediction with{Seal=new AnalysisSealDto(TestData.HashD),Approval=new AnalysisApprovalDto(TestData.HashD)}:prediction with{Body=prediction.Body! with{FieldPackageSha256=TestData.HashD}};
        RunStageFailureException exception=Assert.ThrowsAsync<RunStageFailureException>(async()=>await fixture.Store.MaterializePlanAsync(run.RunId,TestData.ScenarioSnapshot(),prediction))!;
        Assert.That(exception.DiagnosticCode,Is.EqualTo("AuthoritativePredictionChanged"));
    }

    [Test]
    public void DeterministicDrilling_IsStableDistinctAndWithinBounds()
    {
        PlanPathStation[] planned=TestData.PredictionSnapshot().Body!.ProposedWellPath!.Select(x=>new PlanPathStation(x.MeasuredDepthM,x.TrueVerticalDepthM,x.EastingM,x.NorthingM)).ToArray();
        var options=new DrillingExecutionOptions{MaxLateralDeviationM=8,MaxTvdDeviationM=3,MaxDoglegDegreesPer30M=5};
        IReadOnlyList<PlanPathStation> dense=DeterministicDrillingModel.Densify(planned,options.OutputStationSpacingM);
        var first=DeterministicDrillingModel.Execute("11111111-1111-4111-8111-111111111111",planned,options);
        var replay=DeterministicDrillingModel.Execute("11111111-1111-4111-8111-111111111111",planned,options);
        var other=DeterministicDrillingModel.Execute("55555555-5555-4555-8555-555555555555",planned,options);
        Assert.That(CanonicalJson.Serialize(first),Is.EqualTo(CanonicalJson.Serialize(replay)));Assert.That(CanonicalJson.Serialize(first.Stations),Is.Not.EqualTo(CanonicalJson.Serialize(other.Stations)));
        Assert.Multiple(()=>
        {
            Assert.That(first.Stations[0],Is.EqualTo(new DrilledPathStation(planned[0].MeasuredDepthM,planned[0].TrueVerticalDepthM,planned[0].EastingM,planned[0].NorthingM)));
            Assert.That(first.Stations.Zip(first.Stations.Skip(1),(a,b)=>b.MeasuredDepthM>a.MeasuredDepthM),Has.All.True);
            Assert.That(first.Stations.Select(x=>x.TrueVerticalDepthM),Has.All.GreaterThanOrEqualTo(0));
            Assert.That(first.Timeline.Zip(first.Timeline.Skip(1),(a,b)=>b.SimulatedElapsedSeconds>a.SimulatedElapsedSeconds),Has.All.True);
            Assert.That(first.Stations.Zip(dense,(actual,plan)=>Math.Sqrt(Math.Pow(actual.EastingM-plan.EastingM,2)+Math.Pow(actual.NorthingM-plan.NorthingM,2))).Max(),Is.LessThanOrEqualTo(options.MaxLateralDeviationM));
            Assert.That(first.Stations.Zip(dense,(actual,plan)=>Math.Abs(actual.TrueVerticalDepthM-plan.TrueVerticalDepthM)).Max(),Is.LessThanOrEqualTo(options.MaxTvdDeviationM));
            Assert.That(DeterministicDrillingModel.MaximumDogleg(first.Stations),Is.LessThanOrEqualTo(options.MaxDoglegDegreesPer30M+1e-8));
        });
    }

    [Test]
    public async Task S1_AcceptsHorizontalAndUpwardTvd_WhenMdStrictlyIncreases()
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();RunResponse run=await StoreFixture.BindAndCreateAsync(fixture.Store);await fixture.Store.CompleteS0Async(run.RunId);
        AnalysisPredictionDto approved=TestData.PredictionSnapshot();
        approved=approved with{Body=approved.Body! with{ProposedWellPath=[new(0,100,500000,6700000),new(400,100,500100,6700000),new(800,70,500200,6700010)]}};
        Assert.That(await fixture.Store.MaterializePlanAsync(run.RunId,TestData.ScenarioSnapshot(),approved),Is.True);
        Assert.That((await fixture.Store.GetMaterializedPlanAsync(run.RunId))!.Stations.Select(x=>x.TrueVerticalDepthM),Is.EqualTo(new[]{100d,100d,70d}));
    }

    [Test]
    public void SparseTwoStationPath_IsDensifiedDeviatedBoundedAndDeterministic()
    {
        PlanPathStation[] sparse=[new(0,10,1000,2000),new(1200,0,2200,2000)];
        var options=new DrillingExecutionOptions{OutputStationSpacingM=30,MaxLateralDeviationM=7,MaxTvdDeviationM=3,MaxDoglegDegreesPer30M=5};
        var first=DeterministicDrillingModel.Execute("11111111-1111-4111-8111-111111111111",sparse,options);
        var replay=DeterministicDrillingModel.Execute("11111111-1111-4111-8111-111111111111",sparse,options);
        var other=DeterministicDrillingModel.Execute("55555555-5555-4555-8555-555555555555",sparse,options);
        DrilledPathStation td=first.Stations[^1];double tdLateral=Math.Sqrt(Math.Pow(td.EastingM-sparse[^1].EastingM,2)+Math.Pow(td.NorthingM-sparse[^1].NorthingM,2));double tdTvd=Math.Abs(td.TrueVerticalDepthM-sparse[^1].TrueVerticalDepthM);
        Assert.Multiple(()=>
        {
            Assert.That(first.Stations.Count,Is.GreaterThan(2).And.LessThanOrEqualTo(DeterministicDrillingModel.MaximumOutputStations));
            Assert.That(first.Stations[0],Is.EqualTo(new DrilledPathStation(0,10,1000,2000)));
            Assert.That(first.Stations[^1].MeasuredDepthM,Is.EqualTo(1200));
            Assert.That(first.Stations.Zip(first.Stations.Skip(1),(a,b)=>b.MeasuredDepthM-a.MeasuredDepthM),Has.All.LessThanOrEqualTo(options.OutputStationSpacingM+1e-9));
            Assert.That(first.Stations.Select(x=>x.TrueVerticalDepthM),Has.All.GreaterThanOrEqualTo(0));
            Assert.That(tdLateral,Is.LessThanOrEqualTo(options.MaxLateralDeviationM+1e-9));
            Assert.That(tdTvd,Is.LessThanOrEqualTo(options.MaxTvdDeviationM+1e-9));
            Assert.That(tdLateral+tdTvd,Is.GreaterThan(1e-8));
            Assert.That(DeterministicDrillingModel.MaximumDogleg(first.Stations),Is.LessThanOrEqualTo(options.MaxDoglegDegreesPer30M+1e-8));
            Assert.That(CanonicalJson.Serialize(first),Is.EqualTo(CanonicalJson.Serialize(replay)));
            Assert.That(CanonicalJson.Serialize(first.Stations),Is.Not.EqualTo(CanonicalJson.Serialize(other.Stations)));
        });
    }

    [Test]
    public void Densification_EnforcesSpacingAndOutputCountBounds()
    {
        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(()=>new DrillingExecutionOptions{OutputStationSpacingM=0.5}.Validate());
        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(()=>new DrillingExecutionOptions{OutputStationSpacingM=501}.Validate());
        PlanPathStation[] enormous=[new(0,0,0,0),new(20000,1,20000,0)];
        RunStageFailureException failure=Assert.Throws<RunStageFailureException>(()=>DeterministicDrillingModel.Densify(enormous,1))!;
        Assert.That(failure.DiagnosticCode,Is.EqualTo("OutputStationLimitExceeded"));
    }

    [Test]
    public void Options_AreValidated_AndHardwareCalibrationChangesHash()
    {
        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(()=>new DrillingExecutionOptions{NominalRopMPerHour=double.NaN}.Validate());
        string first=new DrillingExecutionOptions{HardwareCalibrationVersion="hardware-a"}.Hash();string second=new DrillingExecutionOptions{HardwareCalibrationVersion="hardware-b"}.Hash();
        Assert.That(second,Is.Not.EqualTo(first));
    }

    [Test]
    public async Task CompletedS2_StopsAtS3_WithoutPublicationOrClockAdvance()
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();RunResponse run=await StoreFixture.BindAndCreateAsync(fixture.Store);
        await fixture.Store.CompleteS0Async(run.RunId);await fixture.Store.MaterializePlanAsync(run.RunId,TestData.ScenarioSnapshot(),TestData.PredictionSnapshot());await fixture.Store.ExecuteDeterministicDrillingAsync(run.RunId,new());await fixture.Store.SetAwaitingDependencyAsync(run.RunId,RunStageKind.S3GenerateSurvey,"SurveyObservationAdapterUnavailable");
        IReadOnlyList<StageResponse> stages=await fixture.Store.GetStagesAsync(run.RunId);var counts=await fixture.Store.GetSideEffectCountsAsync(run.RunId);
        Assert.Multiple(()=>{Assert.That(stages.Take(3).Select(x=>x.Status),Has.All.EqualTo(StageStatus.Completed));Assert.That(stages[2].DiagnosticCode,Is.EqualTo("FallbackModel:deterministic-kinematic-drilling-v1"));Assert.That(stages[3].Status,Is.EqualTo(StageStatus.AwaitingDependency));Assert.That(stages[3].DiagnosticCode,Is.EqualTo("SurveyObservationAdapterUnavailable"));Assert.That(counts.PublicationCount,Is.Zero);Assert.That(counts.ClockAdvanceCount,Is.Zero);});
    }
}
