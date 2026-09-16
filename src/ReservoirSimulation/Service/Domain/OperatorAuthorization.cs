using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace ReservoirSimulation.Domain;

internal sealed class OperatorKeyValidator
{
    internal const string HeaderName = "X-DrillSim-Operator-Key";
    private readonly byte[] _expectedDigest;

    internal OperatorKeyValidator(string operatorKey)
    {
        if (string.IsNullOrEmpty(operatorKey))
            throw new ArgumentException("Operator key must not be empty.", nameof(operatorKey));
        _expectedDigest = Digest(operatorKey);
    }

    internal bool IsAuthorized(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(HeaderName, out StringValues values) || values.Count != 1 || values[0] is null)
            return false;
        byte[] presentedDigest = Digest(values[0]!);
        return CryptographicOperations.FixedTimeEquals(_expectedDigest, presentedDigest);
    }

    private static byte[] Digest(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
}

