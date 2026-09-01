using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Model;

namespace OSDC.UnitConversion.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class PhysicalQuantityController : ControllerBase
    {
        private readonly ILogger _logger;

        public PhysicalQuantityController(ILogger<PhysicalQuantityController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the list of Guid of all PhysicalQuantity present in the microservice database at endpoint UnitConversion/api/PhysicalQuantity
        /// </summary>
        /// <returns>the list of Guid of all PhysicalQuantity present in the microservice database at endpoint UnitConversion/api/PhysicalQuantity</returns>
        [HttpGet(Name = "GetAllPhysicalQuantityId")]
        public ActionResult<IEnumerable<Guid>> GetAllPhysicalQuantityId()
        {
            UsageStatistics.Instance.IncrementPhysicalQuantityControllerGetAllIDPerDay();
            List<Guid> ids = [];
            try
            {
                HashSet<BasePhysicalQuantity> quantityHashSet = [];
                quantityHashSet.UnionWith(BasePhysicalQuantity.AvailableBasePhysicalQuantities);
                quantityHashSet.UnionWith(DrillingPhysicalQuantity.AvailablePhysicalQuantities);
                foreach (BasePhysicalQuantity qty in quantityHashSet)
                {
                    ids.Add(qty.ID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problem while getting available physical quantities");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(ids);
        }

        /// <summary>
        /// Returns the PhysicalQuantity identified by its Guid, at endpoint UnitConversion/api/PhysicalQuantity/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the PhysicalQuantity identified by its Guid, at endpoint UnitConversion/api/PhysicalQuantity/id</returns>
        [HttpGet("{id}", Name = "GetPhysicalQuantityById")]
        public ActionResult<DrillingPhysicalQuantity> GetPhysicalQuantityById(Guid id)
        {
            UsageStatistics.Instance.IncrementPhysicalQuantityControllerGetByIDPerDay();
            if (!id.Equals(Guid.Empty))
            {
                BasePhysicalQuantity qty = DrillingPhysicalQuantity.GetQuantity(id);
                if (qty != null)
                {
                    return Ok(qty);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns the list of all PhysicalQuantity present in the microservice database, at endpoint UnitConversion/api/PhysicalQuantity/HeavyData
        /// </summary>
        /// <returns>the list of all PhysicalQuantity present in the microservice database, at endpoint UnitConversion/api/PhysicalQuantity/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllPhysicalQuantity")]
        public ActionResult<IEnumerable<DrillingPhysicalQuantity>> GetAllPhysicalQuantity()
        {
            UsageStatistics.Instance.IncrementPhysicalQuantityControllerGetAllPerDay();
            try
            {
                HashSet<BasePhysicalQuantity> quantityHashSet = [];
                quantityHashSet.UnionWith(BasePhysicalQuantity.AvailableBasePhysicalQuantities);
                quantityHashSet.UnionWith(DrillingPhysicalQuantity.AvailablePhysicalQuantities);
                return Ok(quantityHashSet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problem while getting available physical quantities");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
