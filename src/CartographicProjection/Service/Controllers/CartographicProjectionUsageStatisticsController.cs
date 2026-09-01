using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.CartographicProjection.Model;
using NORCE.Drilling.CartographicProjection.Service.Managers;
using Model;

namespace NORCE.Drilling.CartographicProjection.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class CartographicProjectionUsageStatisticsController : ControllerBase
    {
        private readonly ILogger _logger;

        public CartographicProjectionUsageStatisticsController(ILogger<CartographicProjectionUsageStatisticsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the usage statistics present in the microservice database at endpoint CartographicProjection/api/CartographicProjectionUsageStatistics
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetCartographicProjectionUsageStatistics")]
        public ActionResult<UsageStatisticsCartographicProjection> GetCartographicProjectionUsageStatistics()
        {
            if (UsageStatisticsCartographicProjection.Instance != null)
            {
                return Ok(UsageStatisticsCartographicProjection.Instance);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
