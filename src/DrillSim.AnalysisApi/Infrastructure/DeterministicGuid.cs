using System.Text.Json.Nodes;

namespace DrillSim.AnalysisApi.Infrastructure;

internal static class DeterministicGuid
{
    public static Guid Create(JsonNode identity)
    {
        byte[] hash = Convert.FromHexString(CanonicalJsonHasher.ComputeCanonicalSha256(identity));
        Span<byte> bytes = stackalloc byte[16];
        hash.AsSpan(0, bytes.Length).CopyTo(bytes);
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x80); // RFC 9562 UUID version 8.
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes, bigEndian: true);
    }
}
