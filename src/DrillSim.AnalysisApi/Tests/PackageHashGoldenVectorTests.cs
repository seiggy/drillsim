using System.Text.Json.Nodes;
using DrillSim.AnalysisApi.Infrastructure;
using DrillSim.AnalysisApi.Models;

namespace DrillSim.AnalysisApi.Tests;

[TestFixture]
public sealed class PackageHashGoldenVectorTests
{
    internal const string ExpectedSha256 =
        "be2b0989f969928e5d74a5c5540d7c2b19cf63c923d6806dd79908ab7c45b556";

    [Test]
    public void Compute_FixedCrossServiceVector_MatchesGoldenSha256()
    {
        Guid fieldId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid clusterId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        JsonNode field = new JsonObject
        {
            ["Name"] = "Golden field",
            ["Nested"] = new JsonObject { ["z"] = 2, ["a"] = 1 },
            ["MetaInfo"] = new JsonObject { ["ID"] = fieldId }
        };
        JsonNode cluster = new JsonObject
        {
            ["MetaInfo"] = new JsonObject { ["ID"] = clusterId },
            ["FieldID"] = fieldId,
            ["Name"] = "Golden cluster"
        };

        string actual = new CanonicalJsonHasher().Compute(
            fieldId,
            field,
            [cluster],
            [],
            [],
            [],
            [],
            [],
            new SourceCounts(1, 1, 0, 0, 0, 0, 0),
            ["z-gap", "a-gap"]);

        Assert.That(actual, Is.EqualTo(ExpectedSha256));
    }
}
