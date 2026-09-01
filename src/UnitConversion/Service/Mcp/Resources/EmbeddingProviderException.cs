using System;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

public sealed class EmbeddingProviderException : Exception
{
    public EmbeddingProviderException(string message)
        : base(message)
    {
    }

    public EmbeddingProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
