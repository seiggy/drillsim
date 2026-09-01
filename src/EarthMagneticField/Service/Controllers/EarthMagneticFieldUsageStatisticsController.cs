using Microsoft.AspNetCore.Mvc;
using OSDC.Drilling.EarthMagneticField.Model;

namespace OSDC.Drilling.EarthMagneticField.Service.Controllers;

[Produces("application/json")]
[Route("[controller]")]
[ApiController]
public class EarthMagneticFieldUsageStatisticsController(UsageStatisticsEarthMagneticField statistics) : ControllerBase
{
    /// <summary>Returns in-memory usage counters for this service replica. This operation is intentionally not exposed as an MCP tool.</summary>
    [HttpGet(Name = "GetEarthMagneticFieldUsageStatistics")]
    public ActionResult<UsageStatisticsEarthMagneticField> GetEarthMagneticFieldUsageStatistics()
    {
        statistics.IncrementStatistics();
        return Ok(statistics);
    }
}
