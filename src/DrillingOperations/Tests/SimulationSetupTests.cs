using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class SimulationSetupTests
{
    private static string Route => $"/drillingoperations/api/scenarios/{TestData.ScenarioId}/setup";
    private static PrepareSimulationRequest Request => new("Demo operator", "profile-one", "Preview", 12345, TestData.HashA);

    [Test]
    public async Task SetupRoutesRequireInternalAuthorization()
    {
        await using var factory = new ApiFactory();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage read = await client.GetAsync(Route);
        using HttpResponseMessage write = await client.PostAsJsonAsync(Route, Request);
        Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(write.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task PreparationCreatesVerifiedImmutableBindingAndAuditedConfigurationWithoutStartingRun()
    {
        var upstream = new SetupUpstream();
        await using var factory = new ApiFactory(upstream.Respond);
        using HttpClient client = factory.CreateInternalClient();
        using HttpResponseMessage read = await client.GetAsync(Route);
        SimulationSetupView before = (await read.Content.ReadFromJsonAsync<SimulationSetupView>())!;
        Assert.That(before.Available, Is.True);
        Assert.That(before.Prepared, Is.False);
        Assert.That(before.Profiles, Has.Count.EqualTo(1));
        using HttpResponseMessage response = await client.SendAsync(Post(Request, "prepare-one"));
        string json = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), json);
        Assert.That(json, Does.Not.Contain("world-opaque-17").And.Not.Contain("stage-a-test-key"));
        SimulationSetupResult result = JsonSerializer.Deserialize<SimulationSetupResult>(json, CanonicalJson.SerializerOptions)!;
        DrillingOperationsStore store = factory.Services.GetRequiredService<DrillingOperationsStore>();
        TruthBindingResponse binding = (await store.GetBindingAsync(TestData.ScenarioId))!;
        Assert.Multiple(() =>
        {
            Assert.That(binding.WorldId, Is.EqualTo("world-opaque-17"));
            Assert.That(binding.ApprovedSealedPredictionHash, Is.EqualTo(Request.ReviewedSealHash));
            Assert.That(result.Configuration.ProfileId, Is.EqualTo(Request.ProfileId));
            Assert.That(result.Configuration.RealizationSeed, Is.EqualTo(Request.RealizationSeed));
            Assert.That(result.Configuration.PreparedBy, Is.EqualTo(Request.Actor));
            Assert.That(upstream.PrepareCalls, Is.EqualTo(1));
            Assert.That(upstream.OperatorKeySeen, Is.True);
        });
        using HttpResponseMessage afterRead = await client.GetAsync(Route);
        SimulationSetupView after = (await afterRead.Content.ReadFromJsonAsync<SimulationSetupView>())!;
        Assert.That(after.Prepared, Is.True);
        Assert.That(after.Available, Is.False);
        Assert.That(after.Current, Is.EqualTo(result.Configuration));
        await using var connection = new SqliteConnection(store.ConnectionString);
        await connection.OpenAsync();
        await using SqliteCommand check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Runs;";
        Assert.That(Convert.ToInt64(await check.ExecuteScalarAsync()), Is.Zero);
        check.CommandText = "SELECT COUNT(*) FROM AuditEntries WHERE Action='simulation.prepared';";
        Assert.That(Convert.ToInt64(await check.ExecuteScalarAsync()), Is.EqualTo(1));
        check.CommandText = "UPDATE SimulationSetups SET ResultJson='{}';";
        Assert.ThrowsAsync<SqliteException>(() => check.ExecuteNonQueryAsync());
    }

    [Test]
    public async Task PreparationReplayDoesNotRequireUpstreamsAndChangedInputConflicts()
    {
        var upstream = new SetupUpstream();
        await using var factory = new ApiFactory(upstream.Respond);
        using HttpClient client = factory.CreateInternalClient();
        using HttpResponseMessage first = await client.SendAsync(Post(Request, "replay-setup"));
        string original = await first.Content.ReadAsStringAsync();
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created), original);
        int calls = upstream.Calls;
        upstream.Unavailable = true;
        using HttpResponseMessage replay = await client.SendAsync(Post(Request, "replay-setup"));
        Assert.That(replay.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(await replay.Content.ReadAsStringAsync(), Is.EqualTo(original));
        Assert.That(upstream.Calls, Is.EqualTo(calls));
        using HttpResponseMessage different = await client.SendAsync(Post(Request with { RealizationSeed = 7 }, "replay-setup"));
        Assert.That(different.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(upstream.Calls, Is.EqualTo(calls));
    }

    [TestCase("unapproved")]
    [TestCase("seal")]
    [TestCase("profile")]
    [TestCase("prepared-world-field")]
    public async Task AuthorityMismatchDoesNotBindOrStart(string mismatch)
    {
        var upstream = new SetupUpstream { Mismatch = mismatch };
        await using var factory = new ApiFactory(upstream.Respond);
        using HttpClient client = factory.CreateInternalClient();
        PrepareSimulationRequest request = mismatch == "seal" ? Request with { ReviewedSealHash = TestData.HashD } :
            mismatch == "profile" ? Request with { ProfileId = "foreign-profile" } : Request;
        using HttpResponseMessage response = await client.SendAsync(Post(request, "invalid-authority"));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(await factory.Services.GetRequiredService<DrillingOperationsStore>().GetBindingAsync(TestData.ScenarioId), Is.Null);
        if (mismatch != "prepared-world-field") Assert.That(upstream.PrepareCalls, Is.Zero);
    }

    [TestCase("actor")]
    [TestCase("resolution")]
    [TestCase("seed")]
    [TestCase("hash")]
    [TestCase("unknown")]
    [TestCase("missing")]
    public async Task InvalidSetupNeverContactsAuthorities(string kind)
    {
        var upstream = new SetupUpstream();
        await using var factory = new ApiFactory(upstream.Respond);
        using HttpClient client = factory.CreateInternalClient();
        object request = kind switch
        {
            "actor" => Request with { Actor = "" },
            "resolution" => Request with { Resolution = "unsupported" },
            "seed" => Request with { RealizationSeed = -1 },
            "hash" => Request with { ReviewedSealHash = "invalid" },
            "unknown" => new { Request.Actor, Request.ProfileId, Request.Resolution, Request.RealizationSeed, Request.ReviewedSealHash, worldId = "not-allowed" },
            _ => new { Request.Actor, Request.ProfileId, Request.Resolution, Request.ReviewedSealHash }
        };
        using HttpResponseMessage response = await client.SendAsync(Post(request, "invalid-input"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(upstream.Calls, Is.Zero);
    }

    private static HttpRequestMessage Post(object body, string key)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, Route) { Content = JsonContent.Create(body) };
        message.Headers.Add("Idempotency-Key", key);
        return message;
    }

    private sealed class SetupUpstream
    {
        public int Calls { get; private set; }
        public int PrepareCalls { get; private set; }
        public bool OperatorKeySeen { get; private set; }
        public bool Unavailable { get; set; }
        public string? Mismatch { get; init; }
        public HttpResponseMessage Respond(HttpRequestMessage request)
        {
            Calls++;
            if (Unavailable) return new(HttpStatusCode.ServiceUnavailable);
            string path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("/setup-profiles", StringComparison.Ordinal))
                return TestData.Json(HttpStatusCode.OK, new
                {
                    fieldId = TestData.FieldId, reservoirName = "SOGNEFJORD FM",
                    profiles = new[] { new SimulationModelProfile("profile-one", "Demo model",
                        "Conditioned synthetic model for workflow demonstrations.", "reservoir-hidden-world-v3") }
                });
            if (path.EndsWith("/worlds/setup", StringComparison.Ordinal))
            {
                PrepareCalls++;
                OperatorKeySeen = request.Headers.TryGetValues("X-DrillSim-Operator-Key", out var keys) && keys.Single() == "stage-a-test-key";
                return TestData.Json(HttpStatusCode.OK, new ReservoirWorldDto("world-opaque-17",
                    Mismatch == "prepared-world-field" ? Guid.NewGuid() : TestData.FieldId,
                    "SOGNEFJORD FM", "reservoir-hidden-world-v3", new("calibration-opaque-9", TestData.HashC)));
            }
            if (path == $"/api/scenarios/{TestData.ScenarioId}" && Mismatch == "unapproved")
                return TestData.Json(HttpStatusCode.OK, TestData.ScenarioSnapshot() with { Status = "PredictionSealed" });
            return TestData.Upstream(request);
        }
    }
}
