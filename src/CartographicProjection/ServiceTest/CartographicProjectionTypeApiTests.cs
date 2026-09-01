using System.Net.Http.Headers;
using NORCE.Drilling.CartographicProjection.ModelShared;

namespace ServiceTest;

public class CartographicProjectionTypeApiTests
{
    private static readonly string host = "https://localhost:5001/"; // set to http://localhost:8080/ if running without https
    private static HttpClient httpClient = default!;
    private static Client client = default!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var handler = new HttpClientHandler
        {
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
    public async Task ProjectionType_List_Contains_UTM()
    {
        var ids = (List<string>)await client.GetAllCartographicProjectionTypeIdAsync();
        Assert.That(ids, Is.Not.Null);
        Assert.That(ids, Does.Contain("UTM"));
    }

    [Test]
    public async Task ProjectionType_GetById_UTM_HasExpectedFlags()
    {
        var type = await client.GetCartographicProjectionTypeByIdAsync("UTM");
        Assert.That(type, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(type.UseZone, Is.True);
            Assert.That(type.UseSouth, Is.True);
        });
    }

    [Test]
    public async Task ProjectionType_HeavyData_NotEmpty()
    {
        var all = (List<CartographicProjectionType?>)await client.GetAllCartographicProjectionTypeAsync();
        Assert.That(all, Is.Not.Null);
        Assert.That(all, Is.Not.Empty);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        httpClient?.Dispose();
    }
}

