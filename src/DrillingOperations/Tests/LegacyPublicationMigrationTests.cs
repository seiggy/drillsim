using Microsoft.Data.Sqlite;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class LegacyPublicationMigrationTests
{
    [Test]
    public async Task ExactLegacyAdapterState_MigratesOnce_WithNormalHashChainedAudit()
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();string scenario=Guid.NewGuid().ToString("D");RunResponse created=await StoreFixture.BindAndCreateAsync(fixture.Store,scenario,"legacy-exact");await MakeLegacyAsync(fixture.ConnectionString,created.RunId);
        var restarted=new DrillingOperationsStore(fixture.ConnectionString,TimeProvider.System);await restarted.InitializeAsync();RunResponse run=(await restarted.GetRunAsync(created.RunId))!;StageResponse s8=(await restarted.GetStagesAsync(created.RunId)).Single(x=>x.Stage==RunStageKind.S8PublishReveal);IReadOnlyList<AuditResponse> audit=await restarted.GetAuditAsync(scenario);
        Assert.Multiple(()=>{Assert.That(run.Status,Is.EqualTo(RunStatus.ReadyToReveal));Assert.That(run.CurrentStage,Is.EqualTo(RunStageKind.S8PublishReveal));Assert.That(run.UpdatedUtc,Is.GreaterThan(DateTimeOffset.Parse("2025-01-01T00:00:00Z")));Assert.That(s8.Status,Is.EqualTo(StageStatus.Pending));Assert.That(s8.StartedUtc,Is.Null);Assert.That(s8.EndedUtc,Is.Null);Assert.That(s8.DiagnosticCode,Is.Null);Assert.That(audit.Count(x=>x.Action=="run.ready-to-reveal"&&x.SubjectId==created.RunId),Is.EqualTo(1));});Assert.That(await restarted.VerifyAuditChainAsync(scenario),Is.True);
        await restarted.InitializeAsync();Assert.That((await restarted.GetAuditAsync(scenario)).Count(x=>x.Action=="run.ready-to-reveal"&&x.SubjectId==created.RunId),Is.EqualTo(1));
    }

    [TestCase("DifferentDiagnostic",false,false)]
    [TestCase("PublicationAdapterUnavailable",true,false)]
    [TestCase("PublicationAdapterUnavailable",false,true)]
    public async Task NonmatchingLegacyRows_RemainUnchanged(string diagnostic,bool addPlan,bool addSideEffect)
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();string scenario=Guid.NewGuid().ToString("D");RunResponse created=await StoreFixture.BindAndCreateAsync(fixture.Store,scenario,"legacy-nonmatch-"+Guid.NewGuid().ToString("N"));await MakeLegacyAsync(fixture.ConnectionString,created.RunId,diagnostic,addPlan,addSideEffect);var restarted=new DrillingOperationsStore(fixture.ConnectionString,TimeProvider.System);await restarted.InitializeAsync();RunResponse run=(await restarted.GetRunAsync(created.RunId))!;StageResponse s8=(await restarted.GetStagesAsync(created.RunId)).Single(x=>x.Stage==RunStageKind.S8PublishReveal);Assert.Multiple(()=>{Assert.That(run.Status,Is.EqualTo(RunStatus.AwaitingDependency));Assert.That(s8.Status,Is.EqualTo(StageStatus.AwaitingDependency));Assert.That(s8.DiagnosticCode,Is.EqualTo(diagnostic));Assert.That((restarted.GetAuditAsync(scenario).Result).Any(x=>x.Action=="run.ready-to-reveal"),Is.False);});
    }

    [TestCase("no-plan")]
    [TestCase("prepared-receipt")]
    [TestCase("final-receipt")]
    [TestCase("legacy-receipt")]
    [TestCase("publication-side-effect")]
    [TestCase("clock-side-effect")]
    [TestCase("run-diagnostic")]
    [TestCase("run-status")]
    [TestCase("current-stage")]
    [TestCase("stage-diagnostic")]
    [TestCase("stage-status")]
    [TestCase("state-diagnostic")]
    [TestCase("state-status")]
    [TestCase("callback-identity-conflict")]
    [TestCase("corrupt-verified-operation")]
    [TestCase("no-unverified-operation")]
    public async Task NormalizationRecovery_RejectsAnyNonmatchingInvariant(string variant)
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();string scenario=Guid.NewGuid().ToString("D");RunResponse created=await StoreFixture.BindAndCreateAsync(fixture.Store,scenario,"normalization-negative-"+variant);await MakeLegacyNormalizationFailureAsync(fixture.ConnectionString,created.RunId,variant);var restarted=new DrillingOperationsStore(fixture.ConnectionString,TimeProvider.System);await restarted.InitializeAsync();RunResponse run=(await restarted.GetRunAsync(created.RunId))!;StageResponse s8=(await restarted.GetStagesAsync(created.RunId)).Single(x=>x.Stage==RunStageKind.S8PublishReveal);Assert.Multiple(()=>{Assert.That(run.Status,Is.EqualTo(variant=="run-status"?RunStatus.PublishFailed:RunStatus.Failed));Assert.That(s8.Status,Is.EqualTo(variant=="stage-status"?StageStatus.AwaitingDependency:StageStatus.Failed));Assert.That((restarted.GetAuditAsync(scenario).Result).Any(x=>x.Action=="publication.normalization-recovery"),Is.False);});
    }

    static async Task MakeLegacyNormalizationFailureAsync(string connectionString,string runId,string variant)
    {
        const string legacy="PublicationEntityIdentityConflict",retry="PublicationReadAfterWriteMismatch",json="{\"value\":1}";string hash=DeterministicIdentity.Sha256(json),planId=Guid.NewGuid().ToString("D"),verifiedId=Guid.NewGuid().ToString("D"),pendingId=Guid.NewGuid().ToString("D"),now="2026-09-04T00:00:00.0000000+00:00";await using var connection=new SqliteConnection(connectionString);await connection.OpenAsync();await using(var seed=connection.CreateCommand()){seed.CommandText="""
         UPDATE Runs SET Status='Failed',CurrentStage=$s8,UpdatedUtc=$now,EndedUtc=$now,DiagnosticCode=$legacy,PublicationCount=0,ClockAdvanceCount=0 WHERE RunId=$run;
         UPDATE RunStages SET Status='Failed',StartedUtc=$now,EndedUtc=$now,Diagnostics=$legacy WHERE RunId=$run AND Stage=$s8;
         INSERT INTO PublicationPlans(PublicationPlanId,RunId,ScenarioId,RevealId,ClonedFieldId,ObservationModelVersion,ValidTimeUtc,ManifestJson,ManifestHash,OperationCount,CreatedUtc)
          SELECT $plan,RunId,ScenarioId,$reveal,$field,'legacy',$now,$json,$hash,2,$now FROM Runs WHERE RunId=$run;
         INSERT INTO PublicationOperations(OperationId,PublicationPlanId,Sequence,TargetService,Route,ReadRoute,EntityId,RecordKind,CanonicalPayloadJson,PayloadHash)
          VALUES($verified,$plan,1,'FieldService','/field/api/Field','/field/api/Field/verified','verified','Field',$json,$hash),
                ($pending,$plan,2,'TrajectoryService','/trajectory/api/Trajectory','/trajectory/api/Trajectory/pending?includeCalculatedStations=true','pending','Trajectory',$json,$hash);
         INSERT INTO PublicationOperationStates(OperationId,Status,AttemptCount,ResultHash,ResultBusinessJson,VerifiedUtc) VALUES($verified,'Verified',1,$hash,$json,$now);
         INSERT INTO PublicationOperationStates(OperationId,Status,AttemptCount,Diagnostic) VALUES($pending,'AwaitingDependency',1,$retry);
         INSERT INTO PublicationStates(PublicationPlanId,Status,AttemptCount,VerifiedOperationCount,Diagnostic,UpdatedUtc) VALUES($plan,'AwaitingDependency',1,1,$retry,$now);
         """;seed.Parameters.AddWithValue("$s8",(int)RunStageKind.S8PublishReveal);seed.Parameters.AddWithValue("$now",now);seed.Parameters.AddWithValue("$legacy",legacy);seed.Parameters.AddWithValue("$retry",retry);seed.Parameters.AddWithValue("$run",runId);seed.Parameters.AddWithValue("$plan",planId);seed.Parameters.AddWithValue("$reveal",Guid.NewGuid().ToString("D"));seed.Parameters.AddWithValue("$field",Guid.NewGuid().ToString("D"));seed.Parameters.AddWithValue("$verified",verifiedId);seed.Parameters.AddWithValue("$pending",pendingId);seed.Parameters.AddWithValue("$json",json);seed.Parameters.AddWithValue("$hash",hash);await seed.ExecuteNonQueryAsync();}
        string mutation=variant switch{"no-plan"=>"DROP TRIGGER TR_PublicationOperations_NoDelete;DROP TRIGGER TR_PublicationPlans_NoDelete;DELETE FROM PublicationOperationStates WHERE OperationId IN ($verified,$pending);DELETE FROM PublicationOperations WHERE PublicationPlanId=$plan;DELETE FROM PublicationStates WHERE PublicationPlanId=$plan;DELETE FROM PublicationPlans WHERE PublicationPlanId=$plan;CREATE TRIGGER TR_PublicationOperations_NoDelete BEFORE DELETE ON PublicationOperations BEGIN SELECT RAISE(ABORT,'Publication operations are immutable');END;CREATE TRIGGER TR_PublicationPlans_NoDelete BEFORE DELETE ON PublicationPlans BEGIN SELECT RAISE(ABORT,'Publication plans are immutable');END","prepared-receipt"=>"UPDATE PublicationStates SET PreparedReceiptJson=$json,PreparedReceiptHash=$hash WHERE PublicationPlanId=$plan","final-receipt"=>"UPDATE PublicationStates SET FinalReceiptJson=$json,FinalReceiptHash=$hash WHERE PublicationPlanId=$plan","legacy-receipt"=>"UPDATE PublicationStates SET ReceiptJson=$json,ReceiptHash=$hash WHERE PublicationPlanId=$plan","publication-side-effect"=>"UPDATE Runs SET PublicationCount=1 WHERE RunId=$run","clock-side-effect"=>"UPDATE Runs SET ClockAdvanceCount=1 WHERE RunId=$run","run-diagnostic"=>"UPDATE Runs SET DiagnosticCode='DifferentDiagnostic' WHERE RunId=$run","run-status"=>"UPDATE Runs SET Status='PublishFailed' WHERE RunId=$run","current-stage"=>"UPDATE Runs SET CurrentStage=$s7 WHERE RunId=$run","stage-diagnostic"=>"UPDATE RunStages SET Diagnostics='DifferentDiagnostic' WHERE RunId=$run AND Stage=$s8","stage-status"=>"UPDATE RunStages SET Status='AwaitingDependency' WHERE RunId=$run AND Stage=$s8","state-diagnostic"=>"UPDATE PublicationStates SET Diagnostic='DifferentDiagnostic' WHERE PublicationPlanId=$plan","state-status"=>"UPDATE PublicationStates SET Status='Staged' WHERE PublicationPlanId=$plan","callback-identity-conflict"=>"UPDATE Runs SET DiagnosticCode='RevealCallbackMismatch' WHERE RunId=$run;UPDATE RunStages SET Diagnostics='RevealCallbackMismatch' WHERE RunId=$run AND Stage=$s8","corrupt-verified-operation"=>"UPDATE PublicationOperationStates SET ResultHash=$bad WHERE OperationId=$verified","no-unverified-operation"=>"UPDATE PublicationOperationStates SET Status='Verified',ResultHash=$hash,ResultBusinessJson=$json,VerifiedUtc=$now WHERE OperationId=$pending",_=>throw new ArgumentOutOfRangeException(nameof(variant))};await using var alter=connection.CreateCommand();alter.CommandText=mutation;alter.Parameters.AddWithValue("$json",json);alter.Parameters.AddWithValue("$hash",hash);alter.Parameters.AddWithValue("$bad",TestData.HashB);alter.Parameters.AddWithValue("$plan",planId);alter.Parameters.AddWithValue("$run",runId);alter.Parameters.AddWithValue("$s8",(int)RunStageKind.S8PublishReveal);alter.Parameters.AddWithValue("$s7",(int)RunStageKind.S7RunProduction);alter.Parameters.AddWithValue("$verified",verifiedId);alter.Parameters.AddWithValue("$pending",pendingId);alter.Parameters.AddWithValue("$now",now);await alter.ExecuteNonQueryAsync();
    }

    static async Task MakeLegacyAsync(string connectionString,string runId,string diagnostic="PublicationAdapterUnavailable",bool addPlan=false,bool addSideEffect=false)
    {
        await using var connection=new SqliteConnection(connectionString);await connection.OpenAsync();await using var command=connection.CreateCommand();command.CommandText="""
            UPDATE RunStages SET Status=$completed WHERE RunId=$run AND Stage=$s7;
            UPDATE RunStages SET Status=$awaiting,StartedUtc=$started,EndedUtc=$ended,Diagnostics=$diagnostic WHERE RunId=$run AND Stage=$s8;
            UPDATE Runs SET Status=$runStatus,CurrentStage=$s8,UpdatedUtc=$started,PublicationCount=$publicationCount,ClockAdvanceCount=$publicationCount WHERE RunId=$run;
            """;command.Parameters.AddWithValue("$completed",StageStatus.Completed.ToString());command.Parameters.AddWithValue("$awaiting",StageStatus.AwaitingDependency.ToString());command.Parameters.AddWithValue("$started","2025-01-01T00:00:00.0000000+00:00");command.Parameters.AddWithValue("$ended","2025-01-01T00:01:00.0000000+00:00");command.Parameters.AddWithValue("$diagnostic",diagnostic);command.Parameters.AddWithValue("$runStatus",RunStatus.AwaitingDependency.ToString());command.Parameters.AddWithValue("$s7",(int)RunStageKind.S7RunProduction);command.Parameters.AddWithValue("$s8",(int)RunStageKind.S8PublishReveal);command.Parameters.AddWithValue("$publicationCount",addSideEffect?1:0);command.Parameters.AddWithValue("$run",runId);await command.ExecuteNonQueryAsync();if(addPlan){await using var plan=connection.CreateCommand();plan.CommandText="INSERT INTO PublicationPlans(PublicationPlanId,RunId,ScenarioId,RevealId,ClonedFieldId,ObservationModelVersion,ValidTimeUtc,ManifestJson,ManifestHash,OperationCount,CreatedUtc) SELECT $id,RunId,ScenarioId,$reveal,$field,'legacy','2025-01-01T00:00:00.0000000+00:00','{}',$hash,0,'2025-01-01T00:00:00.0000000+00:00' FROM Runs WHERE RunId=$run;";plan.Parameters.AddWithValue("$id",Guid.NewGuid().ToString("D"));plan.Parameters.AddWithValue("$reveal",Guid.NewGuid().ToString("D"));plan.Parameters.AddWithValue("$field",Guid.NewGuid().ToString("D"));plan.Parameters.AddWithValue("$hash",new string('a',64));plan.Parameters.AddWithValue("$run",runId);await plan.ExecuteNonQueryAsync();}}
}
