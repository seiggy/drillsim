using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using Model;
using NORCE.Drilling.CartographicProjection.Service.Managers;

namespace NORCE.Drilling.CartographicProjection.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class CartographicProjectionController : ControllerBase
    {
        private readonly ILogger<CartographicProjectionManager> _logger;
        private readonly CartographicProjectionManager _cartographicProjectionManager;

        public CartographicProjectionController(ILogger<CartographicProjectionManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _cartographicProjectionManager = CartographicProjectionManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all CartographicProjection present in the microservice database at endpoint CartographicProjection/api/CartographicProjection
        /// </summary>
        /// <returns>the list of Guid of all CartographicProjection present in the microservice database at endpoint CartographicProjection/api/CartographicProjection</returns>
        [HttpGet(Name = "GetAllCartographicProjectionId")]
        public ActionResult<IEnumerable<Guid?>> GetAllCartographicProjectionId()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicProjectionIdPerDay();
            var ids = _cartographicProjectionManager.GetAllCartographicProjectionId();
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
        /// Returns the list of MetaInfo of all CartographicProjection present in the microservice database, at endpoint CartographicProjection/api/CartographicProjection/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all CartographicProjection present in the microservice database, at endpoint CartographicProjection/api/CartographicProjection/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllCartographicProjectionMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo?>> GetAllCartographicProjectionMetaInfo()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicProjectionMetaInfoPerDay();
            var vals = _cartographicProjectionManager.GetAllCartographicProjectionMetaInfo();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the CartographicProjection identified by its Guid from the microservice database, at endpoint CartographicProjection/api/CartographicProjection/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the CartographicProjection identified by its Guid from the microservice database, at endpoint CartographicProjection/api/CartographicProjection/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetCartographicProjectionById")]
        public ActionResult<Model.CartographicProjection?> GetCartographicProjectionById(Guid id)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetCartographicProjectionByIdPerDay();
            if (!id.Equals(Guid.Empty))
            {
                var val = _cartographicProjectionManager.GetCartographicProjectionById(id);
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
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns the list of all CartographicProjectionLight present in the microservice database, at endpoint CartographicProjection/api/CartographicProjection/LightData
        /// </summary>
        /// <returns>the list of all CartographicProjectionLight present in the microservice database, at endpoint CartographicProjection/api/CartographicProjection/LightData</returns>
        [HttpGet("LightData", Name = "GetAllCartographicProjectionLight")]
        public ActionResult<IEnumerable<Model.CartographicProjectionLight>> GetAllCartographicProjectionLight()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicProjectionLightPerDay();
            var vals = _cartographicProjectionManager.GetAllCartographicProjectionLight();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Returns the list of all CartographicProjection present in the microservice database, at endpoint CartographicProjection/api/CartographicProjection/HeavyData
        /// </summary>
        /// <returns>the list of all CartographicProjection present in the microservice database, at endpoint CartographicProjection/api/CartographicProjection/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllCartographicProjection")]
        public ActionResult<IEnumerable<Model.CartographicProjection?>> GetAllCartographicProjection()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicProjectionPerDay();
            var vals = _cartographicProjectionManager.GetAllCartographicProjection();
            if (vals != null)
            {
                return Ok(vals);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Performs calculation on the given CartographicProjection and adds it to the microservice database, at the endpoint CartographicProjection/api/CartographicProjection
        /// </summary>
        /// <param name="cartographicProjection"></param>
        /// <returns>true if the given CartographicProjection has been added successfully to the microservice database, at the endpoint CartographicProjection/api/CartographicProjection</returns>
        [HttpPost(Name = "PostCartographicProjection")]
        public ActionResult PostCartographicProjection([FromBody] Model.CartographicProjection? data)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementPostCartographicProjectionPerDay();
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _cartographicProjectionManager.GetCartographicProjectionById(data.MetaInfo.ID);
                if (existingData == null)
                {
                    if (_cartographicProjectionManager.AddCartographicProjection(data))
                    {
                        return Ok(); // status=OK is used rather than status=Created because NSwag auto-generated controllers use 200 (OK) rather than 201 (Created) as return codes
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
                else
                {
                    _logger.LogWarning("The given CartographicProjection already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given CartographicProjection is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given CartographicProjection and updates it in the microservice database, at the endpoint CartographicProjection/api/CartographicProjection/id
        /// </summary>
        /// <param name="cartographicProjection"></param>
        /// <returns>true if the given CartographicProjection has been updated successfully to the microservice database, at the endpoint CartographicProjection/api/CartographicProjection/id</returns>
        [HttpPut("{id}", Name = "PutCartographicProjectionById")]
        public ActionResult PutCartographicProjectionById(Guid id, [FromBody] Model.CartographicProjection data)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementPutCartographicProjectionByIdPerDay();
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _cartographicProjectionManager.GetCartographicProjectionById(id);
                if (existingData != null)
                {
                    if (_cartographicProjectionManager.UpdateCartographicProjectionById(id, data))
                    {
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
                else
                {
                    _logger.LogWarning("The given CartographicProjection has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given CartographicProjection is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the CartographicProjection of given ID from the microservice database, at the endpoint CartographicProjection/api/CartographicProjection/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the CartographicProjection was deleted from the microservice database, at the endpoint CartographicProjection/api/CartographicProjection/id</returns>
        [HttpDelete("{id}", Name = "DeleteCartographicProjectionById")]
        public ActionResult DeleteCartographicProjectionById(Guid id)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementDeleteCartographicProjectionByIdPerDay();
            if (_cartographicProjectionManager.GetCartographicProjectionById(id) != null)
            {
                if (_cartographicProjectionManager.DeleteCartographicProjectionById(id))
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                _logger.LogWarning("The CartographicProjection of given ID does not exist");
                return NotFound();
            }
        }
    }
}
