using System.Threading;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Service.Mcp.Resources;

/// <summary>
/// Provides embeddings for arbitrary text so that semantic search can be executed.
/// </summary>
public interface ITextEmbeddingGenerator
{
    Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken);
}
