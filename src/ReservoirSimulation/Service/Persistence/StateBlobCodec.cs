using System.Buffers.Binary;
using System.IO.Compression;
using ReservoirSimulation.Domain;
using ReservoirSimulation.Simulation;

namespace ReservoirSimulation.Persistence;

internal static class StateBlobCodec
{
    internal static byte[] Compress(double[] values)
    {
        var bytes = new byte[checked(values.Length * sizeof(double))];
        for (int index = 0; index < values.Length; index++)
            BinaryPrimitives.WriteInt64LittleEndian(
                bytes.AsSpan(index * sizeof(double), sizeof(double)),
                BitConverter.DoubleToInt64Bits(values[index]));

        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true))
            brotli.Write(bytes);
        return output.ToArray();
    }

    internal static double[] Decompress(byte[] blob, int expectedLength)
    {
        try
        {
            return DecompressCore(blob, expectedLength);
        }
        catch (InvalidDataException exception)
        {
            throw new PersistenceIntegrityException("State blob is not valid Brotli data.", exception);
        }
    }

    internal static string Checksum(SimulationState state) => DeterministicEncoding.ArrayChecksum(
        state.PressurePa, state.OilSaturation, state.WaterSaturation, state.GasSaturation);

    private static double[] DecompressCore(byte[] blob, int expectedLength)
    {
        if (expectedLength < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedLength));
        int expectedByteLength = checked(expectedLength * sizeof(double));
        int maximumBlobLength = checked(Math.Max(expectedByteLength * 2, expectedByteLength + 4_096));
        if (blob.Length > maximumBlobLength)
            throw new PersistenceIntegrityException("Compressed state blob exceeds its bounded encoded size.");

        var bytes = new byte[expectedByteLength];
        using var input = new MemoryStream(blob, writable: false);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        int read = 0;
        while (read < bytes.Length)
        {
            int count = brotli.Read(bytes, read, bytes.Length - read);
            if (count == 0)
                throw new PersistenceIntegrityException(
                    $"State blob decoded {read} bytes; expected {expectedByteLength}.");
            read += count;
        }
        if (brotli.ReadByte() != -1)
            throw new PersistenceIntegrityException("State blob contains more values than the world grid.");

        var values = new double[expectedLength];
        for (int index = 0; index < values.Length; index++)
            values[index] = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(
                bytes.AsSpan(index * sizeof(double), sizeof(double))));
        return values;
    }
}
