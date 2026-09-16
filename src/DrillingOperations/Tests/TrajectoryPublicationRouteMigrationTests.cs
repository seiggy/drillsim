using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class TrajectoryPublicationRouteMigrationTests
{
    [Test]
    public async Task LegacyUnverifiedTrajectoryRoute_MigratesOnce_RetriesWithMaterializedStations_WhileVerifiedRouteStaysImmutable()
    {
        await using var fixture=new StoreFixture();await fixture.InitializeAsync();string scenario=Guid.NewGuid().ToString("D");RunResponse run=await StoreFixture.BindAndCreateAsync(fixture.Store,scenario,"trajectory-route");Guid pendingId=Guid.NewGuid(),verifiedId=Guid.NewGuid();string payload=CanonicalJson.Serialize(new{MetaInfo=new{ID=pendingId},Name="authoritative",SurveyStationList=new[]{new{MD=0d,Abscissa=0d,X=0d,Y=0d,TVD=0d,Inclination=0d,Azimuth=0d},new{MD=10d,Abscissa=10d,X=1d,Y=2d,TVD=9d,Inclination=.1d,Azimuth=.2d}}});await SeedAsync(fixture.ConnectionString,run,scenario,pendingId,verifiedId,payload);

        var restarted=new DrillingOperationsStore(fixture.ConnectionString,TimeProvider.System);await restarted.InitializeAsync();IReadOnlyList<PublicationWriteOperation> incomplete=await restarted.GetIncompletePublicationOperationsAsync(run.RunId);PublicationWriteOperation migrated=incomplete.Single();string verifiedRoute;await using(var connection=new SqliteConnection(fixture.ConnectionString)){await connection.OpenAsync();await using var query=connection.CreateCommand();query.CommandText="SELECT ReadRoute FROM PublicationOperations WHERE OperationId=$id";query.Parameters.AddWithValue("$id",verifiedId.ToString("D"));verifiedRoute=(string)(await query.ExecuteScalarAsync())!;}
        Assert.Multiple(()=>{Assert.That(migrated.ReadRoute,Does.EndWith("?includeCalculatedStations=true"));Assert.That(verifiedRoute,Does.Not.Contain("?"));});

        var handler=new TrajectoryReadHandler();var client=new OntologyPublicationClient(new SingleClientFactory(new HttpClient(handler){BaseAddress=new Uri("http://trajectory.test/")}));PublicationWriteReceipt receipt=await client.WriteAndVerifyAsync(migrated,scenario,Guid.NewGuid().ToString("D"),Guid.NewGuid().ToString("D"),default);PublicationWriteReceipt retryReceipt=await client.WriteAndVerifyAsync(migrated,scenario,Guid.NewGuid().ToString("D"),Guid.NewGuid().ToString("D"),default);using var actual=JsonDocument.Parse(receipt.CanonicalBusinessJson);Assert.Multiple(()=>{Assert.That(handler.LastReadQuery,Does.Contain("includeCalculatedStations=true"));Assert.That(PublicationJson.TryProperty(actual.RootElement,"SurveyStationList",out var stations)&&stations.ValueKind==JsonValueKind.Array&&stations.GetArrayLength()==2,Is.True);Assert.That(receipt.BusinessContentHash,Is.EqualTo(PublicationJson.BusinessContentHash(actual.RootElement)));using var expected=JsonDocument.Parse(migrated.CanonicalPayloadJson);Assert.That(receipt.BusinessContentHash,Is.Not.EqualTo(PublicationJson.BusinessContentHash(expected.RootElement)),"Evidence must authenticate the enriched actual read-back.");Assert.That(retryReceipt.BusinessContentHash,Is.EqualTo(receipt.BusinessContentHash));Assert.That(handler.ConflictCount,Is.EqualTo(1));Assert.That(stations[1].TryGetProperty("Z",out var z)&&z.GetDouble()==9d,Is.True);});

        IReadOnlyList<AuditResponse> audit=await restarted.GetAuditAsync(scenario);Assert.That(audit.Count(x=>x.Action=="publication.trajectory-read-route-migrated"&&x.SubjectId==pendingId.ToString("D")),Is.EqualTo(1));await restarted.InitializeAsync();Assert.That((await restarted.GetAuditAsync(scenario)).Count(x=>x.Action=="publication.trajectory-read-route-migrated"),Is.EqualTo(1));
    }

    static async Task SeedAsync(string connectionString,RunResponse run,string scenario,Guid pendingId,Guid verifiedId,string pendingPayload)
    {
        string plan=Guid.NewGuid().ToString("D"),now="2025-01-01T00:00:00.0000000+00:00",verifiedPayload=CanonicalJson.Serialize(new{MetaInfo=new{ID=verifiedId},Name="verified",SurveyStationList=new[]{new{MD=0d}}});await using var connection=new SqliteConnection(connectionString);await connection.OpenAsync();await using var command=connection.CreateCommand();command.CommandText="""
          INSERT INTO PublicationPlans(PublicationPlanId,RunId,ScenarioId,RevealId,ClonedFieldId,ObservationModelVersion,ValidTimeUtc,ManifestJson,ManifestHash,OperationCount,CreatedUtc)
          VALUES($plan,$run,$scenario,$reveal,$field,'legacy',$now,'{}',$manifestHash,2,$now);
          INSERT INTO PublicationOperations(OperationId,PublicationPlanId,Sequence,TargetService,Route,ReadRoute,EntityId,RecordKind,CanonicalPayloadJson,PayloadHash)
          VALUES($pending,$plan,1,'TrajectoryService','/trajectory/api/Trajectory','/trajectory/api/Trajectory/'||$pending,$pending,'Trajectory',$pendingPayload,$pendingHash),
                ($verified,$plan,2,'TrajectoryService','/trajectory/api/Trajectory','/trajectory/api/Trajectory/'||$verified,$verified,'Trajectory',$verifiedPayload,$verifiedHash);
          INSERT INTO PublicationOperationStates(OperationId,Status,AttemptCount) VALUES($pending,'AwaitingDependency',1);
          INSERT INTO PublicationOperationStates(OperationId,Status,AttemptCount,ResultHash,ResultBusinessJson,VerifiedUtc) VALUES($verified,'Verified',1,$verifiedHash,$verifiedPayload,$now);
          """;command.Parameters.AddWithValue("$plan",plan);command.Parameters.AddWithValue("$run",run.RunId);command.Parameters.AddWithValue("$scenario",scenario);command.Parameters.AddWithValue("$reveal",Guid.NewGuid().ToString("D"));command.Parameters.AddWithValue("$field",Guid.NewGuid().ToString("D"));command.Parameters.AddWithValue("$now",now);command.Parameters.AddWithValue("$manifestHash",DeterministicIdentity.Sha256("{}"));command.Parameters.AddWithValue("$pending",pendingId.ToString("D"));command.Parameters.AddWithValue("$verified",verifiedId.ToString("D"));command.Parameters.AddWithValue("$pendingPayload",pendingPayload);command.Parameters.AddWithValue("$pendingHash",DeterministicIdentity.Sha256(pendingPayload));command.Parameters.AddWithValue("$verifiedPayload",verifiedPayload);command.Parameters.AddWithValue("$verifiedHash",DeterministicIdentity.Sha256(verifiedPayload));await command.ExecuteNonQueryAsync();
    }

    sealed class SingleClientFactory(HttpClient client):IHttpClientFactory{public HttpClient CreateClient(string name)=>client;}
    sealed class TrajectoryReadHandler:HttpMessageHandler
    {
        string? stored;internal string? LastReadQuery;internal int ConflictCount;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            if(request.Method==HttpMethod.Put)return TestData.Json(HttpStatusCode.OK,new{});
            if(request.Method==HttpMethod.Get){LastReadQuery=request.RequestUri!.Query;if(stored is null)return TestData.Json(HttpStatusCode.NotFound,new{});if(!LastReadQuery.Contains("includeCalculatedStations=true",StringComparison.OrdinalIgnoreCase))return TestData.Json(HttpStatusCode.OK,new{MetaInfo=new{ID=Guid.NewGuid()},SurveyStationList=(object?)null});return new(HttpStatusCode.OK){Content=new StringContent(stored,Encoding.UTF8,"application/json")};}
            if(request.Method==HttpMethod.Post){if(stored is not null){ConflictCount++;return TestData.Json(HttpStatusCode.Conflict,new{});}string expected=await request.Content!.ReadAsStringAsync(cancellationToken);JsonObject trajectory=(JsonObject)JsonNode.Parse(expected)!;JsonArray stations=(JsonArray)trajectory.First(x=>x.Key.Equals("SurveyStationList",StringComparison.OrdinalIgnoreCase)).Value!;foreach(JsonObject station in stations.OfType<JsonObject>()){double x=station.First(x=>x.Key.Equals("X",StringComparison.OrdinalIgnoreCase)).Value!.GetValue<double>(),y=station.First(x=>x.Key.Equals("Y",StringComparison.OrdinalIgnoreCase)).Value!.GetValue<double>(),tvd=station.First(x=>x.Key.Equals("TVD",StringComparison.OrdinalIgnoreCase)).Value!.GetValue<double>();station["Z"]=tvd;station["RiemannianNorth"]=x;station["RiemannianEast"]=y;station["Latitude"]=1.063747450603627;station["Longitude"]=-0.124567890123457;station["BoreholeRadius"]=0d;}stored=trajectory.ToJsonString();return TestData.Json(HttpStatusCode.OK,new{});}
            return TestData.Json(HttpStatusCode.NotFound,new{});
        }
    }
}
