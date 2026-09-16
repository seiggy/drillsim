using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace DrillSim.AnalysisApi.Infrastructure;

public sealed class InternalCallbackKeyValidator
{
    public const string HeaderName = "X-DrillSim-Internal-Key";
    private readonly byte[] _expectedDigest;

    public InternalCallbackKeyValidator(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length > 1_024)
            throw new ArgumentException("Internal callback key must contain between 1 and 1024 characters.", nameof(key));
        _expectedDigest = SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }

    public bool IsAuthorized(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(HeaderName, out StringValues values) || values.Count != 1 || values[0] is null)
            return false;

        byte[] presentedDigest = SHA256.HashData(Encoding.UTF8.GetBytes(values[0]!));
        return CryptographicOperations.FixedTimeEquals(_expectedDigest, presentedDigest);
    }
}
