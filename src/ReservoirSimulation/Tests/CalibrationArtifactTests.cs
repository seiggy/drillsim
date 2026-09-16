using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class CalibrationArtifactTests
{
    [Test]
    public void Create_ChangingOnlyCalibrationSha_ChangesWorldIdentityButNotTruth()
    {
        WorldGenerationRequest firstRequest = TestData.ConditionedRequest();
        WorldGenerationRequest secondRequest = firstRequest with
        {
            CalibrationArtifact = TestData.CalibrationArtifact(
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")
        };
        var factory = new ReservoirWorldFactory();

        ReservoirWorld first = factory.Create(firstRequest);
        ReservoirWorld second = factory.Create(secondRequest);

        Assert.Multiple(() =>
        {
            Assert.That(second.Summary.WorldId, Is.Not.EqualTo(first.Summary.WorldId));
            Assert.That(ReservoirWorldFactory.TruthChecksum(second),
                Is.EqualTo(ReservoirWorldFactory.TruthChecksum(first)));
            Assert.That(first.Summary.CalibrationArtifact.Id, Is.EqualTo("test-calibration-v1"));
            Assert.That(second.Summary.CalibrationArtifact.Sha256,
                Is.EqualTo("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));
        });
    }

    [Test]
    public void Create_MalformedCalibrationReferences_AreRejected()
    {
        WorldGenerationRequest valid = TestData.ConditionedRequest();
        CalibrationArtifactReference?[] invalidArtifacts =
        [
            null,
            new CalibrationArtifactReference
            {
                Id = " ",
                Sha256 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            },
            new CalibrationArtifactReference
            {
                Id = "artifact",
                Sha256 = "short"
            },
            new CalibrationArtifactReference
            {
                Id = "artifact",
                Sha256 = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            }
        ];

        foreach (CalibrationArtifactReference? artifact in invalidArtifacts)
        {
            WorldGenerationRequest request = valid with { CalibrationArtifact = artifact! };
            Assert.Throws<ReservoirValidationException>(() => new ReservoirWorldFactory().Create(request));
        }
    }
}
