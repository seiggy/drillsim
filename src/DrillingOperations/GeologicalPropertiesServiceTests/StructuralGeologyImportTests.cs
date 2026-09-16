using System.Net;
using System.Text.Json;

namespace DrillingOperations.GeologicalPropertiesServiceTests;

public sealed partial class PublicationGeologicalPropertiesImportTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task StructuralGeologyWithoutTrajectory_PreservesEvidenceAndRemainsReceiptGated(bool explicitNull)
    {
        Guid scenario = Guid.NewGuid(), reveal = Guid.NewGuid(), id = Guid.NewGuid(), bore = Guid.NewGuid();
        var receipt = new ReceiptHandler(scenario, reveal);
        string database = Path.Combine(Path.GetTempPath(), $"structural-geology-{Guid.NewGuid():N}.db");
        await using var factory = new Factory(receipt, database);
        using var client = factory.CreateClient();
        var payload = new Dictionary<string, object?>
        {
            ["metaInfo"] = new { id },
            ["wellBoreID"] = bore,
            ["name"] = "Structural formation evidence without a survey",
            ["petrophysics"] = new
            {
                formationTops = new[]
                {
                    new
                    {
                        id = Guid.NewGuid(),
                        formationName = "SOGNEFJORD FM",
                        depths = new[] { new { reference = 0, value = 1400d, unit = "m", datum = "MD", positiveDown = true } }
                    }
                }
            }
        };
        if (explicitNull) payload["trajectoryID"] = null;

        using (var stage = Request(HttpMethod.Put, $"/geologicalproperties/api/internal/publication/records/{id:D}/stage",
                   new { scenarioId = scenario, revealId = reveal }))
            (await client.SendAsync(stage)).EnsureSuccessStatusCode();
        using (var import = Request(HttpMethod.Post, "/geologicalproperties/api/internal/publication/GeologicalProperties", payload))
            Assert.That((await client.SendAsync(import)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using (var activate = Request(HttpMethod.Put, $"/geologicalproperties/api/internal/publication/records/{id:D}/activate"))
            (await client.SendAsync(activate)).EnsureSuccessStatusCode();

        string path = $"/geologicalproperties/api/GeologicalProperties/{id:D}";
        Assert.That((await client.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        using (var read = Request(HttpMethod.Get, path))
        {
            using var response = await client.SendAsync(read);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Multiple(() =>
            {
                Assert.That(P(document.RootElement, "WellBoreID").GetGuid(), Is.EqualTo(bore));
                Assert.That(P(document.RootElement, "TrajectoryID").ValueKind, Is.EqualTo(JsonValueKind.Null));
                var top = P(P(document.RootElement, "Petrophysics"), "FormationTops")[0];
                Assert.That(P(top, "FormationName").GetString(), Is.EqualTo("SOGNEFJORD FM"));
                Assert.That(P(P(top, "Depths")[0], "Value").GetDouble(), Is.EqualTo(1400d));
            });
        }
        receipt.Committed = true;
        Assert.That((await client.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase(null, null)]
    [TestCase("00000000-0000-0000-0000-000000000000", null)]
    [TestCase("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000")]
    public async Task StructuralImport_StillRejectsMissingWellboreOrEmptyReferences(string? bore, string? trajectory)
    {
        Guid scenario = Guid.NewGuid(), reveal = Guid.NewGuid(), id = Guid.NewGuid();
        string database = Path.Combine(Path.GetTempPath(), $"invalid-geology-{Guid.NewGuid():N}.db");
        await using var factory = new Factory(new ReceiptHandler(scenario, reveal), database);
        using var client = factory.CreateClient();
        using (var stage = Request(HttpMethod.Put, $"/geologicalproperties/api/internal/publication/records/{id:D}/stage",
                   new { scenarioId = scenario, revealId = reveal }))
            (await client.SendAsync(stage)).EnsureSuccessStatusCode();
        using var import = Request(HttpMethod.Post, "/geologicalproperties/api/internal/publication/GeologicalProperties",
            new { metaInfo = new { id }, wellBoreID = bore, trajectoryID = trajectory });
        Assert.That((await client.SendAsync(import)).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using var read = Request(HttpMethod.Get, $"/geologicalproperties/api/GeologicalProperties/{id:D}");
        Assert.That((await client.SendAsync(read)).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
