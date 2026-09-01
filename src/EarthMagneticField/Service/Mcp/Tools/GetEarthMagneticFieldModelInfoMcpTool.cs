using System.Text.Json;
using System.Text.Json.Nodes;
using OSDC.Drilling.EarthMagneticField.Model;

namespace OSDC.Drilling.EarthMagneticField.Service.Mcp.Tools;

public sealed class GetEarthMagneticFieldModelInfoMcpTool(EarthMagneticFieldEvaluator evaluator) : IMcpTool
{
    public string Name => "earth_magnetic_field_get_model_info";
    public string Description => "Returns the installed WMM2025 and IGRF14 identities and reproducibility metadata: UTC validity bounds, WGS84 ellipsoidal-depth bounds, degree/order, release dates, GeographicLib version, north-east-down and SI conventions, concurrency mode, and SHA-256 hashes of metadata and coefficients. Consult this before choosing a model or evaluating dates and depths near validity boundaries. It performs no evaluation and persists nothing.";
    public JsonNode InputSchema { get; } = JsonNode.Parse("""{"type":"object","properties":{},"additionalProperties":false}""")!;
    public JsonNode OutputSchema { get; } = EarthMagneticFieldMcpSchemas.ServiceInfo();

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken) =>
        Task.FromResult(JsonSerializer.SerializeToNode(evaluator.ServiceInfo, JsonSettings.Options));
}
