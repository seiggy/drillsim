using System.Net;
using System.Text;
using System.Text.Json;
using DrillSim.PublicationGate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class PublicationVisibilityGateTests
{
    [Test]
    public async Task StagedEntities_AreFilteredFromCollectionsParentsPackagesAndMcp_UntilReceiptCommits()
    {
        string database=Path.Combine(Path.GetTempPath(),$"publication-gate-{Guid.NewGuid():N}.db");
        try
        {
            Guid scenario=Guid.NewGuid(),reveal=Guid.NewGuid(),source=Guid.NewGuid();Guid[] staged=Enumerable.Range(0,7).Select(_=>Guid.NewGuid()).ToArray();var receipt=new GateReceiptHandler(scenario,reveal);var store=new ScenarioPublicationGateStore($"Data Source={database}",new HttpClient(receipt){BaseAddress=new Uri("http://analysis.test/")});foreach(Guid id in staged)await store.StageAsync(id,new(scenario,reveal),default);
            string package=JsonSerializer.Serialize(new{field=Entity(staged[0]),clusters=new[]{Entity(staged[1]),Entity(source)},wells=new[]{Entity(staged[2])},wellBores=new[]{Entity(staged[3])},wellBoreArchitectures=new[]{Entity(staged[4])},trajectories=new[]{Entity(staged[5])},geologicalProperties=new[]{Entity(staged[6])}});
            (int status,string hidden)=await Invoke(store,"/api/fields/x/package","application/json",package);Assert.Multiple(()=>{Assert.That(status,Is.EqualTo(200));foreach(Guid id in staged)Assert.That(hidden,Does.Not.Contain(id.ToString("D")));Assert.That(hidden,Does.Contain(source.ToString("D")));});
            (status,_)=await Invoke(store,$"/Well/ByCluster/{staged[1]:D}","application/json",package);Assert.That(status,Is.EqualTo(404));
             string metadata=JsonSerializer.Serialize(new[]{new{ID=staged[0],Name="staged"},new{ID=source,Name="source"}});(_,string filteredMetadata)=await Invoke(store,"/field/api/Field/MetaInfo","application/json",metadata);Assert.Multiple(()=>{Assert.That(filteredMetadata,Does.Not.Contain(staged[0].ToString("D")));Assert.That(filteredMetadata,Does.Contain(source.ToString("D")));});
            string mcp=JsonSerializer.Serialize(new{jsonrpc="2.0",result=new{content=new[]{new{type="text",text=JsonSerializer.Serialize(new[]{Entity(staged[2]),Entity(source)})}}}});(_,string filteredMcp)=await Invoke(store,"/mcp","application/json",mcp);Assert.Multiple(()=>{Assert.That(filteredMcp,Does.Not.Contain(staged[2].ToString("D")));Assert.That(filteredMcp,Does.Contain(source.ToString("D")));});
            receipt.Committed=true;(status,string visible)=await Invoke(store,"/api/fields/x/package","application/json",package);Assert.Multiple(()=>{Assert.That(status,Is.EqualTo(200));foreach(Guid id in staged)Assert.That(visible,Does.Contain(id.ToString("D")));});
        }
        finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();if(File.Exists(database))File.Delete(database); }
    }

    [Test]
    public async Task InternalRead_BypassesGateOnlyWithConstantTimeKey()
    {
        string database=Path.Combine(Path.GetTempPath(),$"publication-gate-{Guid.NewGuid():N}.db");try{Guid id=Guid.NewGuid();var store=new ScenarioPublicationGateStore($"Data Source={database}",new HttpClient(new GateReceiptHandler(Guid.NewGuid(),Guid.NewGuid())){BaseAddress=new Uri("http://analysis.test/")});await store.StageAsync(id,new(Guid.NewGuid(),Guid.NewGuid()),default);string entity=JsonSerializer.Serialize(Entity(id));(int denied,_)=await Invoke(store,$"/Field/{id:D}","application/json",entity);(int allowed,string body)=await Invoke(store,$"/Field/{id:D}","application/json",entity,"publication-import-test-key");(int deniedWrite,_)=await Invoke(store,"/field/api/Field","application/json",entity,method:"POST",requestBody:entity);(int allowedWrite,_)=await Invoke(store,"/field/api/Field","application/json",entity,"publication-import-test-key","POST",entity);Assert.Multiple(()=>{Assert.That(denied,Is.EqualTo(404));Assert.That(allowed,Is.EqualTo(200));Assert.That(deniedWrite,Is.EqualTo(409));Assert.That(allowedWrite,Is.EqualTo(200));Assert.That(body,Does.Contain(id.ToString("D")));});}finally{Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();if(File.Exists(database))File.Delete(database);}
    }


    [Test]
    public async Task ActivatedMarkers_RemainHiddenUntilFinalReceipt_ThenPersistVisibleDuringAnalysisOutage()
    {
        string database=Path.Combine(Path.GetTempPath(),$"publication-gate-{Guid.NewGuid():N}.db");try{Guid scenario=Guid.NewGuid(),reveal=Guid.NewGuid(),id=Guid.NewGuid();var receipt=new GateReceiptHandler(scenario,reveal);var store=new ScenarioPublicationGateStore($"Data Source={database}",new HttpClient(receipt){BaseAddress=new Uri("http://analysis.test/")});await store.StageAsync(id,new(scenario,reveal),default);await store.ActivateAsync(id,default);Assert.That((await store.GetAsync(id,default))!.State,Is.EqualTo("Activated"));Assert.That(await store.GetHiddenIdsAsync(default),Does.Contain(id));receipt.Committed=true;Assert.That(await store.GetHiddenIdsAsync(default),Does.Not.Contain(id));Assert.That((await store.GetAsync(id,default))!.State,Is.EqualTo("Visible"));receipt.Unavailable=true;receipt.Committed=false;var restarted=new ScenarioPublicationGateStore($"Data Source={database}");Assert.That(await restarted.GetHiddenIdsAsync(default),Does.Not.Contain(id));string entity=JsonSerializer.Serialize(Entity(id));(int denied,_)=await Invoke(restarted,"/field/api/Field","application/json",entity,method:"POST",requestBody:entity);Assert.That(denied,Is.EqualTo(409));}finally{Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();if(File.Exists(database))File.Delete(database);}
    }

    [Test]
    public async Task MissingAnalysisConfiguration_FailsClosedForExistingStagedRows()
    {
        string database=Path.Combine(Path.GetTempPath(),$"publication-gate-{Guid.NewGuid():N}.db");try{Guid id=Guid.NewGuid();var seeded=new ScenarioPublicationGateStore($"Data Source={database}");await seeded.StageAsync(id,new(Guid.NewGuid(),Guid.NewGuid()),default);var restarted=new ScenarioPublicationGateStore($"Data Source={database}");Assert.That(await restarted.GetHiddenIdsAsync(default),Does.Contain(id));string entity=JsonSerializer.Serialize(Entity(id));(int status,string body)=await Invoke(restarted,$"/Field/{id:D}","application/json",entity);Assert.Multiple(()=>{Assert.That(status,Is.EqualTo(404));Assert.That(body,Is.Empty);});}finally{Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();if(File.Exists(database))File.Delete(database);}
    }

    [Test]
    public void PartialGateConfiguration_FailsStartup()
    {
        WebApplicationBuilder builder=WebApplication.CreateBuilder();builder.Configuration["ConnectionStrings:Sqlite"]="Data Source=:memory:";builder.Configuration["DRILLSIM_PUBLICATION_IMPORT_KEY"]="key";builder.Configuration["AnalysisApiUrl"]=null;Assert.Throws<InvalidOperationException>(()=>builder.AddScenarioPublicationGate());
    }

    [Test]
    public async Task ResponseFiltering_RecomputesMarkersCreatedDuringDownstreamExecution()
    {
        string database=Path.Combine(Path.GetTempPath(),$"publication-gate-{Guid.NewGuid():N}.db");try{Guid id=Guid.NewGuid();var store=new ScenarioPublicationGateStore($"Data Source={database}");var context=new DefaultHttpContext();context.Request.Path="/field/api/Field";context.Response.ContentType="application/json";context.Response.Body=new MemoryStream();var middleware=new ScenarioPublicationGateMiddleware(async c=>{await store.StageAsync(id,new(Guid.NewGuid(),Guid.NewGuid()),default);await c.Response.WriteAsync(JsonSerializer.Serialize(new[]{Entity(id)}));},store,null);await middleware.InvokeAsync(context);context.Response.Body.Position=0;Assert.That(await new StreamReader(context.Response.Body).ReadToEndAsync(),Does.Not.Contain(id.ToString("D")));}finally{Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();if(File.Exists(database))File.Delete(database);}
    }

    static object Entity(Guid id)=>new{metaInfo=new{id},name="record"};
    static async Task<(int Status,string Body)> Invoke(ScenarioPublicationGateStore store,string path,string contentType,string body,string? key=null,string method="GET",string? requestBody=null)
    {
        var context=new DefaultHttpContext();context.Request.Path=path;context.Request.Method=method;if(requestBody is not null){context.Request.ContentType="application/json";context.Request.Body=new MemoryStream(Encoding.UTF8.GetBytes(requestBody));}if(key is not null)context.Request.Headers["X-DrillSim-Publication-Key"]=key;context.Response.Body=new MemoryStream();var middleware=new ScenarioPublicationGateMiddleware(async c=>{c.Response.StatusCode=200;c.Response.ContentType=contentType;await c.Response.WriteAsync(body);},store,"publication-import-test-key");await middleware.InvokeAsync(context);context.Response.Body.Position=0;return(context.Response.StatusCode,await new StreamReader(context.Response.Body).ReadToEndAsync());
    }
    sealed class GateReceiptHandler(Guid scenario,Guid reveal):HttpMessageHandler
    {
        internal bool Committed;internal bool Unavailable;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>Task.FromResult(Unavailable?TestData.Json(HttpStatusCode.ServiceUnavailable,new{}):Committed?TestData.Json(HttpStatusCode.OK,new{scenarioId=scenario,revealId=reveal,status="Revealed"}):TestData.Json(HttpStatusCode.NotFound,new{}));
    }
}

