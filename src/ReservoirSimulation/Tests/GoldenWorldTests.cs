using System.Buffers.Binary;
using System.Security.Cryptography;
using ReservoirSimulation.Contracts;
using ReservoirSimulation.Domain;

namespace ReservoirSimulation.Tests;

[TestFixture]
public sealed class GoldenWorldTests
{
    // v3: shared-datum contact hydrostatics changed pressure truth; immutable v2 specs must not regenerate.
    [Test]
    public void Create_GoldenTruthChecksum_MatchesVersionedGenerator()
    {
        ReservoirWorld world = new ReservoirWorldFactory().Create(TestData.ConditionedRequest());
        TestContext.Out.WriteLine($"v3 worldId={world.Summary.WorldId}, truthChecksum={TruthChecksum(world)}");

        Assert.Multiple(() =>
        {
            Assert.That(ReservoirWorldFactory.ModelVersion, Is.EqualTo("reservoir-hidden-world-v3"));
            Assert.That(world.Summary.ModelVersion, Is.EqualTo(ReservoirWorldFactory.ModelVersion));
            Assert.That(world.Summary.WorldId, Is.EqualTo("rsw_7b47cd565ac817eb57fe0fa19704f2e29d570e0d8a61c0568822b552b8790636"));
            Assert.That(TruthChecksum(world), Is.EqualTo("e6a91f7e61bd61b89a935ba32b8f021376b05626935baf486b8bc72532c19566"));
        });
    }

    [Test]
    public void Create_V3ContactPressureTruth_DiffersFromV2IndependentColumnAnchors()
    {
        WorldGenerationRequest request = TestData.UniformWorldRequest(2, 1, 2) with
        {
            StructuralConditioningPoints =
            [
                new StructuralConditioningPoint
                {
                    EastingM = 0, NorthingM = 0,
                    ReservoirTopDepthM = 1_000, ReservoirBaseDepthM = 1_050
                },
                new StructuralConditioningPoint
                {
                    EastingM = 700, NorthingM = 0,
                    ReservoirTopDepthM = 1_020, ReservoirBaseDepthM = 1_070
                }
            ],
            FluidContacts = new FluidContactOptions
            {
                TransitionThicknessM = 0,
                GasWater =
                [
                    new FluidContactPoint
                    {
                        EastingM = 0, NorthingM = 0, ContactDepthTvdM = 1_040
                    }
                ]
            }
        };
        ReservoirWorld world = new ReservoirWorldFactory().Create(request);
        double firstTopPressure = world.PressurePa[world.Grid.CellIndex(0, 0, 0)];
        double secondTopPressure = world.PressurePa[world.Grid.CellIndex(1, 0, 0)];

        // v2 independently anchored every top cell at 25 MPa; v3 uses one shared 3D hydrostatic datum.
        Assert.That(Math.Max(Math.Abs(firstTopPressure - 25_000_000),
            Math.Abs(secondTopPressure - 25_000_000)), Is.GreaterThan(1));
    }


    private static string TruthChecksum(ReservoirWorld world)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(hash, world.TopDepthM);
        Append(hash, world.BaseDepthM);
        Append(hash, world.CellDepthM);
        Append(hash, world.CellThicknessM);
        Append(hash, world.NetToGross);
        Append(hash, world.Porosity);
        Append(hash, world.LogPermeability);
        Append(hash, world.PressurePa);
        Append(hash, world.OilSaturation);
        Append(hash, world.WaterSaturation);
        Append(hash, world.GasSaturation);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void Append(IncrementalHash hash, double[] values)
    {
        var buffer = new byte[8];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, values.Length);
        hash.AppendData(buffer.AsSpan(0, 4));
        foreach (double value in values)
        {
            BinaryPrimitives.WriteInt64LittleEndian(buffer, BitConverter.DoubleToInt64Bits(value));
            hash.AppendData(buffer);
        }
    }
}
