using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OSDC.Drilling.EarthMagneticField.Model;

namespace OSDC.Drilling.EarthMagneticField.Service.Controllers;

[Produces("application/json")]
[Route("[controller]")]
[ApiController]
public class EarthMagneticFieldController(
    EarthMagneticFieldEvaluator evaluator,
    UsageStatisticsEarthMagneticField statistics,
    IOptions<EarthMagneticFieldServiceOptions> options) : ControllerBase
{
    /// <summary>Returns installed model information and public conventions for service discovery.</summary>
    [HttpGet(Name = "GetEarthMagneticFieldEntry")]
    public ActionResult<EarthMagneticFieldServiceInfo> GetEarthMagneticFieldEntry() => GetModelInfoResponse();

    /// <summary>Synchronously evaluates WMM2025 or IGRF14 at a batch of WGS84 positions and UTC instants.</summary>
    /// <remarks>This operation is stateless. Latitude/longitude use SI radians, depth is metres positive downward from WGS84, UTC is mandatory, and results use north-east-down teslas with angles in radians. One invalid sample rejects the complete request.</remarks>
    [HttpPost("Evaluate", Name = "EvaluateEarthMagneticField")]
    [ProducesResponseType(typeof(EvaluateEarthMagneticFieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EarthMagneticFieldValidationProblem), StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<EvaluateEarthMagneticFieldResponse> Evaluate(
        [FromBody] EvaluateEarthMagneticFieldRequest request, CancellationToken cancellationToken)
    {
        statistics.IncrementEvaluation(false, request?.Samples?.Count ?? 0);
        try
        {
            return Ok(evaluator.Evaluate(request, options.Value.MaximumSamplesPerRequest, cancellationToken));
        }
        catch (EarthMagneticFieldValidationException exception)
        {
            statistics.IncrementFailedEvaluation();
            return UnprocessableEntity(new EarthMagneticFieldValidationProblem
            {
                Message = exception.Message,
                Errors = exception.Errors.ToList()
            });
        }
    }

    /// <summary>Returns WMM2025 and IGRF14 identities, validity bounds, conventions, and coefficient hashes.</summary>
    [HttpGet("ModelInfo", Name = "GetEarthMagneticFieldModelInfo")]
    public ActionResult<EarthMagneticFieldServiceInfo> GetEarthMagneticFieldModelInfo() => GetModelInfoResponse();

    private ActionResult<EarthMagneticFieldServiceInfo> GetModelInfoResponse()
    {
        statistics.IncrementModelInfo();
        return Ok(evaluator.ServiceInfo);
    }
}
