using System.Text.Json;

namespace DrillingOperations.Tests;

[TestFixture]
public sealed class PackageHashGoldenVectorTests
{
    private const string ExpectedSha256 =
        "be2b0989f969928e5d74a5c5540d7c2b19cf63c923d6806dd79908ab7c45b556";

    [Test]
    public void ComputeAnalysisPackageHash_FixedCrossServiceVector_MatchesGoldenSha256()
    {
        Guid fieldId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid clusterId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        JsonElement field = JsonSerializer.SerializeToElement(new
        {
            Name = "Golden field",
            Nested = new { z = 2, a = 1 },
            MetaInfo = new { ID = fieldId }
        });
        JsonElement cluster = JsonSerializer.SerializeToElement(new
        {
            MetaInfo = new { ID = clusterId },
            FieldID = fieldId,
            Name = "Golden cluster"
        });

        string actual = PublicationJson.ComputeAnalysisPackageHash(
            fieldId,
            field,
            [cluster],
            [],
            [],
            [],
            [],
            [],
            new AnalysisPackageCounts(1, 1, 0, 0, 0, 0, 0),
            ["z-gap", "a-gap"]);

        Assert.That(actual, Is.EqualTo(ExpectedSha256));
    }
}
