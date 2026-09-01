using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OSDC.Drilling.EarthMagneticField.Model;

namespace OSDC.Drilling.EarthMagneticField.Service.Mcp.Tools;

public sealed class EvaluateEarthMagneticFieldMcpTool : IMcpTool
{
    private readonly EarthMagneticFieldEvaluator evaluator_;
    private readonly UsageStatisticsEarthMagneticField statistics_;
    private readonly int maximumSamples_;

    public EvaluateEarthMagneticFieldMcpTool(EarthMagneticFieldEvaluator evaluator,
        UsageStatisticsEarthMagneticField statistics, IOptions<EarthMagneticFieldServiceOptions> options)
    {
        evaluator_ = evaluator;
        statistics_ = statistics;
        maximumSamples_ = options.Value.MaximumSamplesPerRequest;
        InputSchema = EarthMagneticFieldMcpSchemas.EvaluateInput(maximumSamples_);
    }

    public string Name => "earth_magnetic_field_evaluate";
    public string Description => "Synchronously evaluates WMM2025 or IGRF14 for one or more independent samples. This is stateless: the result is returned directly and no GUID, calculation order, dataset, or result is persisted. Model is WMM2025 (valid 2025-01-01T00:00:00Z through 2030-01-01T00:00:00Z; depth -850000 to 1000 m) or IGRF14 (valid 1900-01-01T00:00:00Z through 2030-01-01T00:00:00Z; depth -600000 to 1000 m). Latitude and Longitude MUST be WGS84 SI radians. Depth is SI metres positive downward from the WGS84 ellipsoid. DateTimeUtc is a UTC evaluation instant and MUST contain Z or +00:00; local or unspecified times are rejected. Results are north-east-down magnetic flux density in SI teslas. Declination is radians positive east of geodetic north; inclination is radians positive downward. Samples preserve input order. GeographicLib degrees, positive-up height, east-north-up ordering, nanoteslas, and decimal year are private boundary details. Validation is atomic: one invalid sample rejects the whole request with isError=true and structuredContent shaped as {Error, Message, Errors:[{SampleIndex, Property, Code, Message}]}.";
    public JsonNode InputSchema { get; }
    public JsonNode OutputSchema { get; } = EarthMagneticFieldMcpSchemas.EvaluateOutput();

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        try
        {
            EvaluateEarthMagneticFieldRequest request = arguments?.Deserialize<EvaluateEarthMagneticFieldRequest>(JsonSettings.Options)
                ?? throw new EarthMagneticFieldValidationException(
                    [new(null, "Request", "required", "An object containing Model and Samples is required.")]);
            statistics_.IncrementEvaluation(true, request.Samples?.Count ?? 0);
            EvaluateEarthMagneticFieldResponse response = evaluator_.Evaluate(request, maximumSamples_, cancellationToken);
            return Task.FromResult(JsonSerializer.SerializeToNode(response, JsonSettings.Options));
        }
        catch (JsonException exception)
        {
            statistics_.IncrementFailedEvaluation();
            throw new EarthMagneticFieldValidationException(
                [new(null, "DateTimeUtc", "invalid_format", exception.Message)]);
        }
        catch
        {
            statistics_.IncrementFailedEvaluation();
            throw;
        }
    }
}
