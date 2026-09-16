using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

[TestFixture]
[NonParallelizable]
public sealed class WorldSetupApiTests
{
    private const string WorldsPath = "/reservoirsimulation/api/worlds";
    private WorldSetupTestStore _store = null!;
    private ApiSecurityTests.ReservoirApiFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public async Task CreateApi()
    {
        _store = new WorldSetupTestStore();
        await _store.InitializeAsync();
        _factory = new ApiSecurityTests.ReservoirApiFactory(_store.ConnectionString);
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-DrillSim-Operator-Key", ApiSecurityTests.OperatorKey);
    }

    [TearDown]
    public async Task DisposeApi()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _store.DisposeAsync();
    }

    [TestCase("GET", null)]
    [TestCase("GET", "wrong-key")]
    [TestCase("POST", null)]
    [TestCase("POST", "wrong-key")]
    public async Task SetupRoutes_RequireOperatorAuthorizationBeforeParsingInput(string method, string? key)
    {
        using HttpClient anonymous = _factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method),
            WorldsPath + (method == "GET" ? "/setup-profiles" : "/setup"));
        if (key is not null)
            request.Headers.Add("X-DrillSim-Operator-Key", key);
        if (method == "POST")
            request.Content = new StringContent("{\"unknown\":true}", Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await anonymous.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Catalog_OnlyReturnsSafeProfileMetadata_AndSetupReturnsIdempotentPrivateSummary()
    {
        WorldGenerationRequest template = TestData.ConditionedRequest() with
        {
            CalibrationArtifact = TestData.CalibrationArtifact() with
            {
                Id = @"C:\private\conditioning\calibration.json"
            }
        };
        WorldSummary original = await Seed(template);
        using HttpResponseMessage catalogResponse = await _client.GetAsync(CatalogPath(template));
        string catalogJson = await catalogResponse.Content.ReadAsStringAsync();
        WorldSetupProfileCatalog catalog = (await catalogResponse.Content.ReadFromJsonAsync<WorldSetupProfileCatalog>())!;
        WorldSetupRequest request = WorldSetupTests.Setup(template, catalog.Profiles.Single().ProfileId, "Preview", 25);
        using HttpResponseMessage first = await _client.PostAsJsonAsync(WorldsPath + "/setup", request);
        using HttpResponseMessage second = await _client.PostAsJsonAsync(WorldsPath + "/setup", request);
        string firstJson = await first.Content.ReadAsStringAsync();
        string secondJson = await second.Content.ReadAsStringAsync();
        WorldSummary summary = (await first.Content.ReadFromJsonAsync<WorldSummary>())!;
        using JsonDocument document = JsonDocument.Parse(catalogJson);
        JsonElement profile = document.RootElement.GetProperty("profiles")[0];
        using HttpResponseMessage read = await _client.GetAsync(WorldsPath + "/" + summary.WorldId);

        Assert.Multiple(() =>
        {
            Assert.That(catalogResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(document.RootElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(new[] { "fieldId", "reservoirName", "profiles" }));
            Assert.That(profile.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(new[] { "profileId", "name", "description", "worldModelVersion" }));
            Assert.That(catalogJson, Does.Not.Contain(original.WorldId));
            Assert.That(catalogJson, Does.Not.Contain("worldId"));
            Assert.That(catalogJson, Does.Not.Contain("seed").IgnoreCase);
            Assert.That(catalogJson, Does.Not.Contain("conditioningPoints"));
            Assert.That(catalogJson, Does.Not.Contain("pressurePa"));
            Assert.That(catalogJson, Does.Not.Contain("porosity"));
            Assert.That(catalogJson, Does.Not.Contain("calibrationArtifact"));
            Assert.That(catalogJson, Does.Not.Contain("private"));
            Assert.That(catalogJson, Does.Not.Contain(template.CalibrationArtifact.Sha256));
            Assert.That(catalog.Profiles.Single().Description, Does.Contain("synthetic"));
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(firstJson, Is.EqualTo(secondJson));
            Assert.That(summary.WorldId, Is.Not.EqualTo(original.WorldId));
            Assert.That(summary.ModelVersion, Is.EqualTo(ReservoirWorldFactory.ModelVersion));
            Assert.That(summary.CalibrationArtifact, Is.EqualTo(template.CalibrationArtifact));
            Assert.That(summary.Grid, Is.EqualTo(new WorldGridSummary(16, 16, 8, 2_048)));
            Assert.That(firstJson, Does.Not.Contain("conditioningPoints"));
            Assert.That(firstJson, Does.Not.Contain("pressurePa"));
            Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
        Assert.That(await _store.Repository.CountStatesAsync(summary.WorldId), Is.Zero);
    }

    [Test]
    public async Task Catalog_EmptyScope_IsAnHonestEmptyList()
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await Seed(template);
        using HttpResponseMessage response = await _client.GetAsync(CatalogPath(
            template with { FieldId = Guid.NewGuid() }));
        WorldSetupProfileCatalog? catalog = await response.Content.ReadFromJsonAsync<WorldSetupProfileCatalog>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(catalog!.Profiles, Is.Empty);
        });
    }

    [TestCase("fieldId")]
    [TestCase("reservoirName")]
    [TestCase("profileId")]
    [TestCase("resolution")]
    [TestCase("realizationSeed")]
    public async Task Setup_RequiresEveryProperty_IncludingZeroValuedSeed(string property)
    {
        JsonObject body = ValidBody();
        body.Remove(property);
        using HttpResponseMessage response = await PostJson(body.ToJsonString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("worldId", "\"rsw_hidden\"")]
    [TestCase("seed", "17")]
    [TestCase("grid", "{\"countX\":64}")]
    [TestCase("conditioningPoints", "[]")]
    [TestCase("calibrationArtifact", "{}")]
    public async Task Setup_RejectsUnknownOrPrivateControls(string property, string jsonValue)
    {
        JsonObject body = ValidBody();
        body[property] = JsonNode.Parse(jsonValue);
        using HttpResponseMessage response = await PostJson(body.ToJsonString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCaseSource(nameof(InvalidProperties))]
    public async Task Setup_RejectsInvalidValues(string property, string jsonValue)
    {
        JsonObject body = ValidBody();
        body[property] = JsonNode.Parse(jsonValue);
        using HttpResponseMessage response = await PostJson(body.ToJsonString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("")]
    [TestCase("{}")]
    [TestCase("null")]
    [TestCase("[]")]
    [TestCase("{")]
    public async Task Setup_RejectsMissingOrMalformedBodies(string body)
    {
        using HttpResponseMessage response = await PostJson(body);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("")]
    [TestCase("?fieldId=72cf31ce-5d1a-44ca-a7cd-d3d2bfb34eb0")]
    [TestCase("?reservoirName=Test%20Sand")]
    [TestCase("?fieldId=invalid&reservoirName=Test%20Sand")]
    [TestCase("?fieldId=00000000-0000-0000-0000-000000000000&reservoirName=Test%20Sand")]
    [TestCase("?fieldId=72cf31ce5d1a44caa7cdd3d2bfb34eb0&reservoirName=Test%20Sand")]
    [TestCase("?fieldId=72CF31CE-5D1A-44CA-A7CD-D3D2BFB34EB0&reservoirName=Test%20Sand")]
    [TestCase("?fieldId=72cf31ce-5d1a-44ca-a7cd-d3d2bfb34eb0&reservoirName=")]
    [TestCase("?fieldId=72cf31ce-5d1a-44ca-a7cd-d3d2bfb34eb0&reservoirName=%20%20")]
    public async Task Catalog_RejectsMissingOrInvalidScope(string query)
    {
        using HttpResponseMessage response = await _client.GetAsync(WorldsPath + "/setup-profiles" + query);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Catalog_RejectsOverlongReservoirName()
    {
        using HttpResponseMessage response = await _client.GetAsync(CatalogPath(
            TestData.ConditionedRequest() with { ReservoirName = new string('s', 201) }));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("field")]
    [TestCase("reservoir")]
    [TestCase("profile")]
    public async Task Setup_RejectsProfilesOutsideTheSelectedScope(string mismatch)
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await Seed(template);
        WorldSetupProfileCatalog catalog = (await _client.GetFromJsonAsync<WorldSetupProfileCatalog>(
            CatalogPath(template)))!;
        WorldSetupRequest request = WorldSetupTests.Setup(template, catalog.Profiles.Single().ProfileId);
        request = mismatch switch
        {
            "field" => request with { FieldId = Guid.NewGuid() },
            "reservoir" => request with { ReservoirName = "Another reservoir" },
            "profile" => request with { ProfileId = "rsp_" + new string('0', 64) },
            _ => throw new ArgumentOutOfRangeException(nameof(mismatch))
        };

        using HttpResponseMessage response = await _client.PostAsJsonAsync(WorldsPath + "/setup", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase(0, "Preview", 16, 16, 8)]
    [TestCase(int.MaxValue, "Standard", 64, 64, 20)]
    public async Task Setup_AcceptsSeedBoundsAndExactResolutionChoices(
        int seed, string resolution, int countX, int countY, int countZ)
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        await Seed(template);
        WorldSetupProfileCatalog catalog = (await _client.GetFromJsonAsync<WorldSetupProfileCatalog>(
            CatalogPath(template)))!;
        using HttpResponseMessage response = await _client.PostAsJsonAsync(WorldsPath + "/setup",
            WorldSetupTests.Setup(template, catalog.Profiles.Single().ProfileId, resolution, seed));
        WorldSummary summary = (await response.Content.ReadFromJsonAsync<WorldSummary>())!;
        WorldGenerationRequest persisted = ReservoirWorldFactory.RequestFromCanonicalJson(
            (await _store.Repository.LoadWorldSpecAsync(summary.WorldId))!.CanonicalRequestJson);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(summary.Grid, Is.EqualTo(new WorldGridSummary(
                countX, countY, countZ, countX * countY * countZ)));
            Assert.That(persisted.Seed, Is.EqualTo(seed));
        });
    }

    [TestCase("checksum")]
    [TestCase("canonical")]
    [TestCase("identity")]
    [TestCase("invalid-request")]
    [TestCase("invalid-json")]
    public async Task SetupRoutes_ReportIntegrityFailureWithoutLeakingPrivateSpecifications(string corruption)
    {
        WorldGenerationRequest template = TestData.ConditionedRequest();
        WorldSummary original = await Seed(template);
        WorldSetupProfileCatalog catalog = (await _client.GetFromJsonAsync<WorldSetupProfileCatalog>(
            CatalogPath(template)))!;
        await _store.CorruptAsync(original.WorldId, corruption);

        using HttpResponseMessage read = await _client.GetAsync(CatalogPath(template));
        using HttpResponseMessage setup = await _client.PostAsJsonAsync(WorldsPath + "/setup",
            WorldSetupTests.Setup(template, catalog.Profiles.Single().ProfileId));
        string readJson = await read.Content.ReadAsStringAsync();
        string setupJson = await setup.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(read.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(setup.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(readJson, Does.Contain("integrity"));
            Assert.That(readJson, Does.Not.Contain(original.WorldId));
            Assert.That(setupJson, Does.Not.Contain(original.WorldId));
            Assert.That(readJson, Does.Not.Contain("conditioningPoints"));
            Assert.That(readJson, Does.Not.Contain("countX"));
        });
    }

    private static IEnumerable<TestCaseData> InvalidProperties()
    {
        foreach ((string property, string value) in new[]
        {
            ("fieldId", "\"00000000-0000-0000-0000-000000000000\""),
            ("fieldId", "\"72CF31CE-5D1A-44CA-A7CD-D3D2BFB34EB0\""),
            ("fieldId", "\"72cf31ce5d1a44caa7cdd3d2bfb34eb0\""),
            ("fieldId", "\"invalid\""), ("fieldId", "null"), ("fieldId", "1"),
            ("reservoirName", "null"), ("reservoirName", "\"\""), ("reservoirName", "\"   \""),
            ("reservoirName", JsonSerializer.Serialize(new string('s', 201))),
            ("profileId", "null"), ("profileId", "\"\""), ("profileId", "\"rsw_hidden-world\""),
            ("profileId", JsonSerializer.Serialize("rsp_" + new string('G', 64))),
            ("resolution", "null"), ("resolution", "\"\""), ("resolution", "\"preview\""),
            ("resolution", "\"standard\""), ("resolution", "\" Preview\""), ("resolution", "\"Standard \""),
            ("resolution", "\"Full\""), ("resolution", "\"0\""), ("resolution", "0"),
            ("realizationSeed", "-1"), ("realizationSeed", "2147483648"), ("realizationSeed", "1.5"),
            ("realizationSeed", "\"1\""), ("realizationSeed", "null"), ("realizationSeed", "true")
        })
            yield return new TestCaseData(property, value);
    }

    private static JsonObject ValidBody() => JsonSerializer.SerializeToNode(
        WorldSetupTests.Setup(TestData.ConditionedRequest(), "rsp_" + new string('a', 64)),
        new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();

    private async Task<WorldSummary> Seed(WorldGenerationRequest request)
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(WorldsPath, request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<WorldSummary>())!;
    }

    private Task<HttpResponseMessage> PostJson(string json) =>
        _client.PostAsync(WorldsPath + "/setup", new StringContent(json, Encoding.UTF8, "application/json"));

    private static string CatalogPath(WorldGenerationRequest request) =>
        WorldsPath + $"/setup-profiles?fieldId={request.FieldId:D}&reservoirName=" +
        Uri.EscapeDataString(request.ReservoirName);
}
