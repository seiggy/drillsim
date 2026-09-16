using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text.Json;
namespace DrillingOperations;
public sealed partial class DrillingOperationsStore
{
    private async Task InitializeExecutionSchemaAsync(SqliteConnection c, CancellationToken ct) { await using SqliteCommand q=c.CreateCommand(); q.CommandText=ExecutionSchema; await q.ExecuteNonQueryAsync(ct); }

    public async Task<bool> MaterializePlanAsync(string runId, AnalysisScenarioDto scenario, AnalysisPredictionDto prediction, CancellationToken ct=default)
    {
        await using SqliteConnection c=await OpenAsync(ct); await using SqliteTransaction tx=c.BeginTransaction(deferred:false);
        RunResponse run=await ReadRunAsync(c,tx,runId,ct)??throw new RunStageFailureException("RunMissing","Run does not exist.");
        TruthBindingResponse binding=await ReadBindingAsync(c,tx,run.ScenarioId,ct)??throw new RunStageFailureException("BindingMissing","Binding does not exist.");
        IReadOnlyList<StageResponse> stages=await ReadStagesAsync(c,tx,runId,ct);
        if(stages.Single(x=>x.Stage==RunStageKind.S0BindWorld).Status!=StageStatus.Completed) throw new RunStageFailureException("IllegalStageProgression","S0 must complete first.");
        try { ConfiguredPredictionIntegrity.ValidateApproved(prediction, scenario); }
        catch (ScoringException) { throw new RunStageFailureException("ConfiguredPredictionIntegrityMismatch", "Configured source, candidate, geometry or baseline verification failed."); }
        if(stages.Single(x=>x.Stage==RunStageKind.S1MaterializePlan).Status==StageStatus.Completed){await tx.CommitAsync(ct);return false;}
        IReadOnlyList<PlanPathStation> path=ValidateApprovedPlan(run,binding,scenario,prediction);
        string pathJson=CanonicalJson.Serialize(path),pathHash=DeterministicIdentity.Sha256(pathJson);
        string wellId=DeterministicIdentity.Create("future-scenario-well-v1",run.ScenarioId,binding.BindingId,prediction.Seal!.Sha256);
        string boreId=DeterministicIdentity.Create("future-scenario-wellbore-v1",run.ScenarioId,binding.BindingId,prediction.Seal.Sha256);
        string trajectoryId=DeterministicIdentity.Create("future-planned-trajectory-v1",run.ScenarioId,binding.BindingId,prediction.Seal.Sha256,pathHash);
        string artifactJson=PlanArtifactJson(runId,run.ScenarioId,binding.BindingId,wellId,boreId,trajectoryId,prediction.Body!.CandidateId,pathHash,prediction.Seal.Sha256,prediction.Revision,prediction.Body.FieldPackageSha256);
        string artifactHash=DeterministicIdentity.Sha256(artifactJson),planId=DeterministicIdentity.Create("materialized-plan-v1",artifactJson);
        DateTimeOffset now=timeProvider.GetUtcNow();
        string input=CanonicalJson.Serialize(new{binding.BindingId,bindingInputHash=DeterministicIdentity.Sha256(CanonicalJson.Serialize(ToRequest(binding))),prediction.Revision,predictionSealSha256=prediction.Seal.Sha256,pathHash});
        string output=CanonicalJson.Serialize(new{planId,artifactHash,pathHash,stationCount=path.Count,trajectoryRole=TrajectoryMetadata.Planned,verticalDirection=TrajectoryMetadata.VerticalDirection,referenceFrame=TrajectoryMetadata.ReferenceFrame});
        await using SqliteCommand q=Command(c,tx,"INSERT INTO MaterializedPlans (PlanId,RunId,ScenarioId,BindingId,ScenarioWellId,ScenarioWellBoreId,PlannedTrajectoryId,CandidateId,CanonicalPathJson,PathHash,SourcePredictionSealSha256,SourcePredictionRevision,SourcePackageSha256,CanonicalInputJson,InputHash,CanonicalOutputJson,OutputHash,ArtifactHash,CreatedUtc) VALUES($plan,$run,$scenario,$binding,$well,$bore,$trajectory,$candidate,$path,$pathHash,$seal,$revision,$package,$input,$inputHash,$output,$outputHash,$artifactHash,$created);");
        Add(q,"$plan",planId);Add(q,"$run",runId);Add(q,"$scenario",run.ScenarioId);Add(q,"$binding",binding.BindingId);Add(q,"$well",wellId);Add(q,"$bore",boreId);Add(q,"$trajectory",trajectoryId);Add(q,"$candidate",prediction.Body.CandidateId);Add(q,"$path",pathJson);Add(q,"$pathHash",pathHash);Add(q,"$seal",prediction.Seal.Sha256);Add(q,"$revision",prediction.Revision);Add(q,"$package",prediction.Body.FieldPackageSha256);Add(q,"$input",input);Add(q,"$inputHash",DeterministicIdentity.Sha256(input));Add(q,"$output",output);Add(q,"$outputHash",DeterministicIdentity.Sha256(output));Add(q,"$artifactHash",artifactHash);Add(q,"$created",Format(now)); await q.ExecuteNonQueryAsync(ct);
        await AppendAuditAsync(c,tx,run.ScenarioId,"plan.materialized",planId,CanonicalJson.Serialize(new{planId,artifactHash,pathHash}),ct);
        await CompleteStageAsync(c,tx,run,RunStageKind.S1MaterializePlan,input,output,artifactHash,now,ct); await tx.CommitAsync(ct); return true;
    }

    public async Task<bool> ExecuteDeterministicDrillingAsync(string runId,DrillingExecutionOptions options,CancellationToken ct=default)
    {
        options.Validate(); await using SqliteConnection c=await OpenAsync(ct); await using SqliteTransaction tx=c.BeginTransaction(deferred:false);
        RunResponse run=await ReadRunAsync(c,tx,runId,ct)??throw new RunStageFailureException("RunMissing","Run does not exist.");
        IReadOnlyList<StageResponse> stages=await ReadStagesAsync(c,tx,runId,ct);
        if(stages.Single(x=>x.Stage==RunStageKind.S1MaterializePlan).Status!=StageStatus.Completed) throw new RunStageFailureException("IllegalStageProgression","S1 must complete first.");
        if(stages.Single(x=>x.Stage==RunStageKind.S2ExecuteDrilling).Status==StageStatus.Completed){await tx.CommitAsync(ct);return false;}
        MaterializedPlan plan=await ReadMaterializedPlanAsync(c,tx,runId,ct)??throw new RunStageFailureException("MaterializedPlanMissing","Plan missing.");
        string optionsHash=options.Hash(); string input=CanonicalJson.Serialize(new{plan.PlanId,plan.ArtifactHash,optionsHash,modelVersion=DeterministicDrillingModel.ModelVersion,options.HardwareCalibrationVersion}); string inputHash=DeterministicIdentity.Sha256(input);
        var execution=DeterministicDrillingModel.Execute(runId,plan.Stations,options); string executionId=DeterministicIdentity.Create("drilling-execution-v1",runId,inputHash);
        string canonicalOutput=CanonicalJson.Serialize(new{executionId,runId,plan.PlanId,modelVersion=DeterministicDrillingModel.ModelVersion,optionsHash,options.HardwareCalibrationVersion,execution.Stations,execution.Timeline,startSimulatedSeconds=0d,endSimulatedSeconds=execution.DurationSeconds,trajectoryRole=TrajectoryMetadata.AsDrilledTruth,verticalDirection=TrajectoryMetadata.VerticalDirection,referenceFrame=TrajectoryMetadata.ReferenceFrame}); string outputHash=DeterministicIdentity.Sha256(canonicalOutput); DateTimeOffset now=timeProvider.GetUtcNow();
        await using SqliteCommand q=Command(c,tx,"INSERT INTO DrillingExecutions (ExecutionId,RunId,PlanId,ModelVersion,OptionsHash,HardwareCalibrationVersion,CanonicalInputJson,InputHash,CanonicalOutputJson,OutputHash,AsDrilledPathJson,TimelineJson,StartSimulatedSeconds,EndSimulatedSeconds,CreatedUtc) VALUES($id,$run,$plan,$model,$options,$calibration,$input,$inputHash,$output,$outputHash,$path,$timeline,0,$duration,$created);");
        Add(q,"$id",executionId);Add(q,"$run",runId);Add(q,"$plan",plan.PlanId);Add(q,"$model",DeterministicDrillingModel.ModelVersion);Add(q,"$options",optionsHash);Add(q,"$calibration",options.HardwareCalibrationVersion);Add(q,"$input",input);Add(q,"$inputHash",inputHash);Add(q,"$output",canonicalOutput);Add(q,"$outputHash",outputHash);Add(q,"$path",CanonicalJson.Serialize(execution.Stations));Add(q,"$timeline",CanonicalJson.Serialize(execution.Timeline));Add(q,"$duration",execution.DurationSeconds);Add(q,"$created",Format(now));await q.ExecuteNonQueryAsync(ct);
        string stageOutput=CanonicalJson.Serialize(new{executionId,artifactHash=outputHash,stationCount=execution.Stations.Count,startSimulatedSeconds=0d,endSimulatedSeconds=execution.DurationSeconds,modelVersion=DeterministicDrillingModel.ModelVersion});
        await AppendAuditAsync(c,tx,run.ScenarioId,"drilling.executed",executionId,CanonicalJson.Serialize(new{executionId,artifactHash=outputHash,modelVersion=DeterministicDrillingModel.ModelVersion}),ct);
        await CompleteStageAsync(c,tx,run,RunStageKind.S2ExecuteDrilling,input,stageOutput,outputHash,now,ct);
        await using SqliteCommand metadata=Command(c,tx,"UPDATE RunStages SET Diagnostics=$diagnostics WHERE RunId=$run AND Stage=$stage;");Add(metadata,"$diagnostics","FallbackModel:"+DeterministicDrillingModel.ModelVersion);Add(metadata,"$run",runId);Add(metadata,"$stage",(int)RunStageKind.S2ExecuteDrilling);await metadata.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);return true;
    }

    public async Task<bool> SetAwaitingDependencyAsync(string runId,RunStageKind stage,string code,CancellationToken ct=default)
    {
        await using SqliteConnection c=await OpenAsync(ct);await using SqliteTransaction tx=c.BeginTransaction(deferred:false);
        RunResponse run=await ReadRunAsync(c,tx,runId,ct)??throw new RunStageFailureException("RunMissing","Run missing.");
        IReadOnlyList<StageResponse> stages=await ReadStagesAsync(c,tx,runId,ct);StageResponse current=stages.Single(x=>x.Stage==stage);
        if(current.Status==StageStatus.AwaitingDependency){await tx.CommitAsync(ct);return false;}
        if(stage==RunStageKind.S0BindWorld||stages.Single(x=>x.Stage==(RunStageKind)((int)stage-1)).Status!=StageStatus.Completed)throw new RunStageFailureException("IllegalStageProgression","Dependency stage prerequisites are incomplete.");
        string input=CanonicalJson.Serialize(new{prerequisiteStage=(RunStageKind)((int)stage-1)});await SetAwaitingDependencyAsync(c,tx,run,stage,input,code,timeProvider.GetUtcNow(),ct);await tx.CommitAsync(ct);return true;
    }

    public async Task<MaterializedPlan?> GetMaterializedPlanAsync(string runId,CancellationToken ct=default){await using SqliteConnection c=await OpenAsync(ct);return await ReadMaterializedPlanAsync(c,null,runId,ct);}
    public async Task<DrillingExecutionArtifact?> GetDrillingExecutionAsync(string runId,CancellationToken ct=default){await using SqliteConnection c=await OpenAsync(ct);return await ReadDrillingExecutionAsync(c,null,runId,ct);}

    private static IReadOnlyList<PlanPathStation> ValidateApprovedPlan(RunResponse run,TruthBindingResponse binding,AnalysisScenarioDto scenario,AnalysisPredictionDto prediction)
    {
        if(scenario.ScenarioId.ToString("D",CultureInfo.InvariantCulture)!=run.ScenarioId||scenario.Status!="HumanApproved"||scenario.SourceFieldId==Guid.Empty||string.IsNullOrWhiteSpace(scenario.ReservoirName)||scenario.WorldModelVersion!=binding.WorldModelVersion||prediction.ScenarioId!=scenario.ScenarioId||prediction.Seal is null||prediction.Approval is null||prediction.Body is null) throw new RunStageFailureException("AuthoritativePredictionChanged","Scenario is no longer approved.");
        if(prediction.Seal.Sha256!=prediction.Approval.SealedSha256||prediction.Seal.Sha256!=binding.ApprovedSealedPredictionHash||prediction.Body.FieldPackageSha256!=binding.SourcePackageSha256||prediction.Revision<1) throw new RunStageFailureException("AuthoritativePredictionChanged","Seal, package, or revision changed.");
        if(string.IsNullOrWhiteSpace(prediction.Body.CandidateId)||prediction.Body.CandidateId.Length>200||prediction.Body.ProposedWellPath is not{Count:>=2 and<=2000})throw new RunStageFailureException("ApprovedPlanInvalid","Candidate or path invalid.");
        var result=new List<PlanPathStation>(prediction.Body.ProposedWellPath.Count);double priorMd=-1;
        foreach(AnalysisPathStationDto x in prediction.Body.ProposedWellPath){if(!double.IsFinite(x.MeasuredDepthM)||!double.IsFinite(x.TrueVerticalDepthM)||!double.IsFinite(x.EastingM)||!double.IsFinite(x.NorthingM)||x.MeasuredDepthM<0||x.TrueVerticalDepthM<0||x.MeasuredDepthM<=priorMd ||Math.Abs(x.EastingM)>1e9||Math.Abs(x.NorthingM)>1e9)throw new RunStageFailureException("ApprovedPlanInvalid","Stations must be finite, bounded, nonnegative, and strictly increasing in MD.");result.Add(new(x.MeasuredDepthM,x.TrueVerticalDepthM,x.EastingM,x.NorthingM));priorMd=x.MeasuredDepthM;}return result;
    }

    private static string PlanArtifactJson(string runId,string scenarioId,string bindingId,string wellId,string boreId,string trajectoryId,string candidateId,string pathHash,string seal,int revision,string packageHash)=>CanonicalJson.Serialize(new{runId,scenarioId,bindingId,scenarioWellId=wellId,scenarioWellBoreId=boreId,plannedTrajectoryId=trajectoryId,candidateId,pathHash,sourcePredictionSealSha256=seal,revision,sourcePackageSha256=packageHash,trajectoryRole=TrajectoryMetadata.Planned,verticalDirection=TrajectoryMetadata.VerticalDirection,referenceFrame=TrajectoryMetadata.ReferenceFrame});

    private static async Task<MaterializedPlan?> ReadMaterializedPlanAsync(SqliteConnection c,SqliteTransaction? tx,string runId,CancellationToken ct)
    {
        await using SqliteCommand q=Command(c,tx,"""SELECT PlanId,RunId,ScenarioId,BindingId,ScenarioWellId,ScenarioWellBoreId,PlannedTrajectoryId,CandidateId,CanonicalPathJson,PathHash,SourcePredictionSealSha256,SourcePredictionRevision,SourcePackageSha256,ArtifactHash,CreatedUtc,CanonicalInputJson,InputHash,CanonicalOutputJson,OutputHash FROM MaterializedPlans WHERE RunId=$run;""");Add(q,"$run",runId);await using SqliteDataReader r=await q.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))return null;
        string pathJson=r.GetString(8),pathHash=DeterministicIdentity.Sha256(pathJson);PlanPathStation[] stations=JsonSerializer.Deserialize<PlanPathStation[]>(pathJson,CanonicalJson.SerializerOptions)??throw new PersistenceIntegrityException("Plan path invalid.");
        if(CanonicalJson.Serialize(stations)!=pathJson)throw new PersistenceIntegrityException("Plan path is not canonical.");
        string artifactJson=PlanArtifactJson(r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetString(7),pathHash,r.GetString(10),r.GetInt32(11),r.GetString(12));string artifactHash=DeterministicIdentity.Sha256(artifactJson);
        if(r.GetString(9)!=pathHash||r.GetString(13)!=artifactHash||r.GetString(0)!=DeterministicIdentity.Create("materialized-plan-v1",artifactJson)||DeterministicIdentity.Sha256(r.GetString(15))!=r.GetString(16)||DeterministicIdentity.Sha256(r.GetString(17))!=r.GetString(18))throw new PersistenceIntegrityException("Materialized plan integrity failure.");
        return new(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetString(7),stations,pathHash,r.GetString(10),r.GetInt32(11),r.GetString(12),artifactHash,Parse(r.GetString(14)));
    }

    private static async Task<DrillingExecutionArtifact?> ReadDrillingExecutionAsync(SqliteConnection c,SqliteTransaction? tx,string runId,CancellationToken ct)
    {
        await using SqliteCommand q=Command(c,tx,"""SELECT ExecutionId,RunId,PlanId,ModelVersion,OptionsHash,HardwareCalibrationVersion,CanonicalInputJson,InputHash,CanonicalOutputJson,OutputHash,AsDrilledPathJson,TimelineJson,StartSimulatedSeconds,EndSimulatedSeconds,CreatedUtc FROM DrillingExecutions WHERE RunId=$run;""");Add(q,"$run",runId);await using SqliteDataReader r=await q.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))return null;
        string inputHash=DeterministicIdentity.Sha256(r.GetString(6)),outputHash=DeterministicIdentity.Sha256(r.GetString(8));DrilledPathStation[] stations=JsonSerializer.Deserialize<DrilledPathStation[]>(r.GetString(10),CanonicalJson.SerializerOptions)??throw new PersistenceIntegrityException("Drilled path invalid.");DrillingTimelinePoint[] timeline=JsonSerializer.Deserialize<DrillingTimelinePoint[]>(r.GetString(11),CanonicalJson.SerializerOptions)??throw new PersistenceIntegrityException("Timeline invalid.");
        string expectedOutput=CanonicalJson.Serialize(new{executionId=r.GetString(0),runId=r.GetString(1),planId=r.GetString(2),modelVersion=r.GetString(3),optionsHash=r.GetString(4),hardwareCalibrationVersion=r.GetString(5),stations,timeline,startSimulatedSeconds=r.GetDouble(12),endSimulatedSeconds=r.GetDouble(13),trajectoryRole=TrajectoryMetadata.AsDrilledTruth,verticalDirection=TrajectoryMetadata.VerticalDirection,referenceFrame=TrajectoryMetadata.ReferenceFrame});
        if(inputHash!=r.GetString(7)||outputHash!=r.GetString(9)||r.GetString(0)!=DeterministicIdentity.Create("drilling-execution-v1",runId,inputHash)||CanonicalJson.Serialize(stations)!=r.GetString(10)||CanonicalJson.Serialize(timeline)!=r.GetString(11)||expectedOutput!=r.GetString(8)||r.GetString(3)!=DeterministicDrillingModel.ModelVersion)throw new PersistenceIntegrityException("Drilling execution integrity failure.");
        return new(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),stations,timeline,r.GetDouble(12),r.GetDouble(13),inputHash,outputHash,Parse(r.GetString(14)));
    }

    private const string ExecutionSchema="""
      CREATE TABLE IF NOT EXISTS MaterializedPlans(PlanId TEXT PRIMARY KEY,RunId TEXT NOT NULL UNIQUE,ScenarioId TEXT NOT NULL,BindingId TEXT NOT NULL,ScenarioWellId TEXT NOT NULL,ScenarioWellBoreId TEXT NOT NULL,PlannedTrajectoryId TEXT NOT NULL,CandidateId TEXT NOT NULL,CanonicalPathJson TEXT NOT NULL,PathHash TEXT NOT NULL,SourcePredictionSealSha256 TEXT NOT NULL,SourcePredictionRevision INTEGER NOT NULL,SourcePackageSha256 TEXT NOT NULL,CanonicalInputJson TEXT NOT NULL,InputHash TEXT NOT NULL,CanonicalOutputJson TEXT NOT NULL,OutputHash TEXT NOT NULL,ArtifactHash TEXT NOT NULL,CreatedUtc TEXT NOT NULL,FOREIGN KEY(RunId)REFERENCES Runs(RunId),FOREIGN KEY(BindingId)REFERENCES TruthBindings(BindingId));
      CREATE TRIGGER IF NOT EXISTS TR_MaterializedPlans_NoUpdate BEFORE UPDATE ON MaterializedPlans BEGIN SELECT RAISE(ABORT,'Materialized plans are immutable');END;
      CREATE TRIGGER IF NOT EXISTS TR_MaterializedPlans_NoDelete BEFORE DELETE ON MaterializedPlans BEGIN SELECT RAISE(ABORT,'Materialized plans are immutable');END;
      CREATE TABLE IF NOT EXISTS DrillingExecutions(ExecutionId TEXT PRIMARY KEY,RunId TEXT NOT NULL UNIQUE,PlanId TEXT NOT NULL UNIQUE,ModelVersion TEXT NOT NULL,OptionsHash TEXT NOT NULL,HardwareCalibrationVersion TEXT NOT NULL,CanonicalInputJson TEXT NOT NULL,InputHash TEXT NOT NULL,CanonicalOutputJson TEXT NOT NULL,OutputHash TEXT NOT NULL,AsDrilledPathJson TEXT NOT NULL,TimelineJson TEXT NOT NULL,StartSimulatedSeconds REAL NOT NULL,EndSimulatedSeconds REAL NOT NULL,CreatedUtc TEXT NOT NULL,FOREIGN KEY(RunId)REFERENCES Runs(RunId),FOREIGN KEY(PlanId)REFERENCES MaterializedPlans(PlanId));
      CREATE TRIGGER IF NOT EXISTS TR_DrillingExecutions_NoUpdate BEFORE UPDATE ON DrillingExecutions BEGIN SELECT RAISE(ABORT,'Drilling executions are immutable');END;
      CREATE TRIGGER IF NOT EXISTS TR_DrillingExecutions_NoDelete BEFORE DELETE ON DrillingExecutions BEGIN SELECT RAISE(ABORT,'Drilling executions are immutable');END;
      """;
}
