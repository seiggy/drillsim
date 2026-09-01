using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NORCE.Drilling.CartographicProjection.Model;
using Model;

namespace NORCE.Drilling.CartographicProjection.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class CartographicProjectionTypeController : ControllerBase
    {
        private readonly ILogger<CartographicProjectionType> _logger;

        public CartographicProjectionTypeController(ILogger<CartographicProjectionType> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns the list of enum codes of all CartographicProjection present in the microservice database at endpoint CartographicProjection/api/CartographicProjectionType
        /// </summary>
        /// <returns>the list of enum codes of all CartographicProjection present in the microservice database at endpoint CartographicProjection/api/CartographicProjectionType</returns>
        [HttpGet(Name = "GetAllCartographicProjectionTypeId")]
        public ActionResult<IEnumerable<string>> GetAllCartographicProjectionTypeId()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicProjectionTypeIdPerDay();
            var ids = CartographicProjectionType.Get();
            if (ids != null)
            {
                return Ok(ids);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the CartographicProjectionType identified by its enum code from the microservice database, at endpoint CartographicProjection/api/CartographicProjectionType/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the CartographicProjectionType identified by its enum code from the microservice database, at endpoint CartographicProjection/api/CartographicProjectionType/id</returns>
        [HttpGet("{id}", Name = "GetCartographicProjectionTypeById")]
        public ActionResult<Model.CartographicProjectionType?> GetCartographicProjectionTypeById(string id)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetCartographicProjectionTypeByIdPerDay();
            if (!id.Equals(Guid.Empty))
            {
                if (Enum.TryParse(id, out ProjectionType idEnum))
                {
                    var val = CartographicProjectionType.Get(idEnum);
                    if (val != null)
                    {
                        return Ok(val);
                    }
                    else
                    {
                        return NotFound();
                    }
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
        /// Returns the list of all CartographicProjectionType present in the microservice database, at endpoint CartographicProjection/api/CartographicProjectionType/HeavyData
        /// </summary>
        /// <returns>the list of all CartographicProjection present in the microservice database, at endpoint CartographicProjection/api/CartographicProjectionType/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllCartographicProjectionType")]
        public ActionResult<IEnumerable<Model.CartographicProjectionType?>> GetAllCartographicProjectionType()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicProjectionTypePerDay();
            var vals = CartographicProjectionType.GetAll();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
