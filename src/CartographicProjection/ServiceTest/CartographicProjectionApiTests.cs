using System.Net.Http.Headers;
using NORCE.Drilling.CartographicProjection.ModelShared;

namespace ServiceTest;

public class CartographicProjectionApiTests
{
    private static readonly string host = "https://localhost:5001/"; // set to http://localhost:8080/ if running without https
    private static HttpClient httpClient = default!;
    private static Client client = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var handler = new HttpClientHandler
        {
            // Dev convenience: bypass TLS validation (do not use in production)
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(host + "CartographicProjection/api/")
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client = new Client(httpClient.BaseAddress!.ToString(), httpClient);
    }

    [Test]
    public async Task CartographicProjection_CRUD_Flow_Works()
    {
        var id = Guid.NewGuid();
        var body = new CartographicProjection
        {
            MetaInfo = new MetaInfo { ID = id },
            Name = "UTM 32N Test",
            ProjectionType = ProjectionType.UTM,
            Zone = 32,
            IsSouth = false
        };

        // Create
        await client.PostCartographicProjectionAsync(body);

        // List IDs contains new id
        var ids = await client.GetAllCartographicProjectionIdAsync();
        Assert.That(ids, Is.Not.Null);
        Assert.That(ids.Any(x => x == id), Is.True);

        // MetaInfo contains new id
        var meta = (List<MetaInfo?>)await client.GetAllCartographicProjectionMetaInfoAsync();
        Assert.That(meta.Any(m => m != null && m.ID == id), Is.True);

        // Get by id
        var fetched = await client.GetCartographicProjectionByIdAsync(id);
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched.Name, Is.EqualTo(body.Name));
        Assert.That(fetched.Zone, Is.EqualTo(32));
        Assert.That(fetched.ProjectionType, Is.EqualTo(ProjectionType.UTM));

        // Light/Heavy lists contain new item
        var lights = (List<CartographicProjectionLight>)await client.GetAllCartographicProjectionLightAsync();
        Assert.That(lights.Any(x => x.Name == body.Name), Is.True);
        var heavies = (List<CartographicProjection>)await client.GetAllCartographicProjectionAsync();
        Assert.That(heavies.Any(x => x.MetaInfo?.ID == id), Is.True);

        // Update
        body.Name = "UTM 32N Test (edited)";
        await client.PutCartographicProjectionByIdAsync(id, body);
        var updated = await client.GetCartographicProjectionByIdAsync(id);
        Assert.That(updated.Name, Is.EqualTo(body.Name));

        // Delete
        await client.DeleteCartographicProjectionByIdAsync(id);
        try
        {
            _ = await client.GetCartographicProjectionByIdAsync(id);
            Assert.Fail("Expected 404 after deletion");
        }
        catch (ApiException ex)
        {
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        httpClient?.Dispose();
    }
}

