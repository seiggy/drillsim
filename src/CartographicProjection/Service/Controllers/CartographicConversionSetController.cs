using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.CartographicProjection.Service.Managers;
using System.Threading.Tasks;
using Model;

namespace NORCE.Drilling.CartographicProjection.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class CartographicConversionSetController : ControllerBase
    {
        private readonly ILogger<CartographicConversionSetManager> _logger;
        private readonly CartographicConversionSetManager _cartographicConversionSetManager;

        public CartographicConversionSetController(ILogger<CartographicConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _cartographicConversionSetManager = CartographicConversionSetManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all CartographicConversionSet present in the microservice database at endpoint CartographicProjection/api/CartographicConversionSet
        /// </summary>
        /// <returns>the list of Guid of all CartographicConversionSet present in the microservice database at endpoint CartographicProjection/api/CartographicConversionSet</returns>
        [HttpGet(Name = "GetAllCartographicConversionSetId")]
        public ActionResult<IEnumerable<Guid>> GetAllCartographicConversionSetId()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicConversionSetIdPerDay();
            var ids = _cartographicConversionSetManager.GetAllCartographicConversionSetId();
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
        /// Returns the list of MetaInfo of all CartographicConversionSet present in the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all CartographicConversionSet present in the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllCartographicConversionSetMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllCartographicConversionSetMetaInfo()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicConversionSetMetaInfoPerDay();
            var vals = _cartographicConversionSetManager.GetAllCartographicConversionSetMetaInfo();
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
        /// Returns the CartographicConversionSet identified by its Guid from the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the CartographicConversionSet identified by its Guid from the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetCartographicConversionSetById")]
        public ActionResult<Model.CartographicConversionSet?> GetCartographicConversionSetById(Guid id)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetCartographicConversionSetByIdPerDay();
            if (!id.Equals(Guid.Empty))
            {
                var val = _cartographicConversionSetManager.GetCartographicConversionSetById(id);
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
        /// Returns the list of all CartographicConversionSetLight present in the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/LightData
        /// </summary>
        /// <returns>the list of all CartographicConversionSetLight present in the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/LightData</returns>
        [HttpGet("LightData", Name = "GetAllCartographicConversionSetLight")]
        public ActionResult<IEnumerable<Model.CartographicConversionSetLight>> GetAllCartographicConversionSetLight()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicConversionSetLightPerDay();
            var vals = _cartographicConversionSetManager.GetAllCartographicConversionSetLight();
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
        /// Returns the list of all CartographicConversionSet present in the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/HeavyData
        /// </summary>
        /// <returns>the list of all CartographicConversionSet present in the microservice database, at endpoint CartographicProjection/api/CartographicConversionSet/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllCartographicConversionSet")]
        public ActionResult<IEnumerable<Model.CartographicConversionSet?>> GetAllCartographicConversionSet()
        {
            UsageStatisticsCartographicProjection.Instance.IncrementGetAllCartographicConversionSetPerDay();
            var vals = _cartographicConversionSetManager.GetAllCartographicConversionSet();
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
        /// Performs calculation on the given CartographicConversionSet and adds it to the microservice database, at the endpoint CartographicProjection/api/CartographicConversionSet
        /// </summary>
        /// <param name="cartographicConversionSet"></param>
        /// <returns>true if the given CartographicConversionSet has been added successfully to the microservice database, at the endpoint CartographicProjection/api/CartographicConversionSet</returns>
        [HttpPost(Name = "PostCartographicConversionSet")]
        public async Task<ActionResult> PostCartographicConversionSet([FromBody] Model.CartographicConversionSet? data)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementPostCartographicConversionSetPerDay();
            // Check if cartographicConversionSet exists in the database through ID
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _cartographicConversionSetManager.GetCartographicConversionSetById(data.MetaInfo.ID);
                if (existingData == null)
                {   
                    //  If cartographicConversionSet was not found, call AddCartographicConversionSet, where the cartographicConversionSet.Calculate()
                    // method is called. 
                    if (await _cartographicConversionSetManager.AddCartographicConversionSet(data))
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
                    _logger.LogWarning("The given CartographicConversionSet already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given CartographicConversionSet is null, badly formed, or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given CartographicConversionSet and updates it in the microservice database, at the endpoint CartographicProjection/api/CartographicConversionSet/id
        /// </summary>
        /// <param name="cartographicConversionSet"></param>
        /// <returns>true if the given CartographicConversionSet has been updated successfully to the microservice database, at the endpoint CartographicProjection/api/CartographicConversionSet/id</returns>
        [HttpPut("{id}", Name = "PutCartographicConversionSetById")]
        public async Task<ActionResult> PutCartographicConversionSetById(Guid id, [FromBody] Model.CartographicConversionSet? data)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementPutCartographicConversionSetByIdPerDay();
            // Check if CartographicConversionSet is in the data base
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _cartographicConversionSetManager.GetCartographicConversionSetById(id);
                if (existingData != null)
                {
                    if (await _cartographicConversionSetManager.UpdateCartographicConversionSetById(id, data))
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
                    _logger.LogWarning("The given CartographicConversionSet has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given CartographicConversionSet is null, badly formed, or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the CartographicConversionSet of given ID from the microservice database, at the endpoint CartographicProjection/api/CartographicConversionSet/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the CartographicConversionSet was deleted from the microservice database, at the endpoint CartographicProjection/api/CartographicConversionSet/id</returns>
        [HttpDelete("{id}", Name = "DeleteCartographicConversionSetById")]
        public ActionResult DeleteCartographicConversionSetById(Guid id)
        {
            UsageStatisticsCartographicProjection.Instance.IncrementDeleteCartographicConversionSetByIdPerDay();
            if (_cartographicConversionSetManager.GetCartographicConversionSetById(id) != null)
            {
                if (_cartographicConversionSetManager.DeleteCartographicConversionSetById(id))
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
                _logger.LogWarning("The CartographicConversionSet of given ID does not exist");
                return NotFound();
            }
        }
    }
}
