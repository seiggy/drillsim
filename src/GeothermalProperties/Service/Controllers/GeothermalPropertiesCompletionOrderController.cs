using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.GeothermalProperties.Service.Managers;

namespace NORCE.Drilling.GeothermalProperties.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class GeothermalPropertiesCompletionOrderController : ControllerBase
    {
        private readonly ILogger<GeothermalPropertiesCompletionOrderManager> _logger;
        private readonly GeothermalPropertiesCompletionOrderManager _geothermalPropertiesCompletionOrderManager;
        public GeothermalPropertiesCompletionOrderController(ILogger<GeothermalPropertiesCompletionOrderManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _geothermalPropertiesCompletionOrderManager = GeothermalPropertiesCompletionOrderManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all GeothermalPropertiesCompletionOrder present in the microservice database at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder
        /// </summary>
        /// <returns>the list of Guid of all GeothermalPropertiesCompletionOrder present in the microservice database at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder</returns>
        [HttpGet(Name = "GetAllGeothermalPropertiesCompletionOrderId")]
        public ActionResult<IEnumerable<Guid>> GetAllGeothermalPropertiesCompletionOrderId()
        {
            var ids = _geothermalPropertiesCompletionOrderManager.GetAllGeothermalPropertiesCompletionOrderId();
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
        /// Returns the list of MetaInfo of all GeothermalPropertiesCompletionOrder present in the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all GeothermalPropertiesCompletionOrder present in the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllGeothermalPropertiesCompletionOrderMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllGeothermalPropertiesCompletionOrderMetaInfo()
        {
            var vals = _geothermalPropertiesCompletionOrderManager.GetAllGeothermalPropertiesCompletionOrderMetaInfo();
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
        /// Returns the GeothermalPropertiesCompletionOrder identified by its Guid from the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the GeothermalPropertiesCompletionOrder identified by its Guid from the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/id</returns>
        [HttpGet("{id}", Name = "GetGeothermalPropertiesCompletionOrderById")]
        public ActionResult<Model.GeothermalPropertiesCompletionOrder?> GetGeothermalPropertiesCompletionOrderById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _geothermalPropertiesCompletionOrderManager.GetGeothermalPropertiesCompletionOrderById(id);
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
        /// Returns the list of all GeothermalPropertiesCompletionOrderLight present in the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/LightData
        /// </summary>
        /// <returns>the list of all GeothermalPropertiesCompletionOrderLight present in the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/LightData</returns>
        [HttpGet("LightData", Name = "GetAllGeothermalPropertiesCompletionOrderLight")]
        public ActionResult<IEnumerable<Model.GeothermalPropertiesCompletionOrderLight>> GetAllGeothermalPropertiesCompletionOrderLight()
        {
            var vals = _geothermalPropertiesCompletionOrderManager.GetAllGeothermalPropertiesCompletionOrderLight();
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
        /// Returns the list of all GeothermalPropertiesCompletionOrder present in the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/HeavyData
        /// </summary>
        /// <returns>the list of all GeothermalPropertiesCompletionOrder present in the microservice database, at endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllGeothermalPropertiesCompletionOrder")]
        public ActionResult<IEnumerable<Model.GeothermalPropertiesCompletionOrder?>> GetAllGeothermalPropertiesCompletionOrder()
        {
            var vals = _geothermalPropertiesCompletionOrderManager.GetAllGeothermalPropertiesCompletionOrder();
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
        /// Performs calculation on the given GeothermalPropertiesCompletionOrder and adds it to the microservice database, at the endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder
        /// </summary>
        /// <param name="geothermalPropertiesCompletionOrder"></param>
        /// <returns>true if the given GeothermalPropertiesCompletionOrder has been added successfully to the microservice database, at the endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder</returns>
        [HttpPost(Name = "PostGeothermalPropertiesCompletionOrder")]
        public ActionResult PostGeothermalPropertiesCompletionOrder([FromBody] Model.GeothermalPropertiesCompletionOrder? data)
        {
            // Check if geothermalPropertiesCompletionOrder exists in the database through ID
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _geothermalPropertiesCompletionOrderManager.GetGeothermalPropertiesCompletionOrderById(data.MetaInfo.ID);
                if (existingData == null)
                {   
                    //  If geothermalPropertiesCompletionOrder was not found, call AddGeothermalPropertiesCompletionOrder, where the geothermalPropertiesCompletionOrder.Calculate()
                    // method is called. 
                    if (_geothermalPropertiesCompletionOrderManager.AddGeothermalPropertiesCompletionOrder(data))
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
                    _logger.LogWarning("The given GeothermalPropertiesCompletionOrder already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given GeothermalPropertiesCompletionOrder is null, badly formed, or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given GeothermalPropertiesCompletionOrder and updates it in the microservice database, at the endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/id
        /// </summary>
        /// <param name="geothermalPropertiesCompletionOrder"></param>
        /// <returns>true if the given GeothermalPropertiesCompletionOrder has been updated successfully to the microservice database, at the endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/id</returns>
        [HttpPut("{id}", Name = "PutGeothermalPropertiesCompletionOrderById")]
        public ActionResult PutGeothermalPropertiesCompletionOrderById(Guid id, [FromBody] Model.GeothermalPropertiesCompletionOrder? data)
        {
            // Check if GeothermalPropertiesCompletionOrder is in the data base
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _geothermalPropertiesCompletionOrderManager.GetGeothermalPropertiesCompletionOrderById(id);
                if (existingData != null)
                {
                    if (_geothermalPropertiesCompletionOrderManager.UpdateGeothermalPropertiesCompletionOrderById(id, data))
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
                    _logger.LogWarning("The given GeothermalPropertiesCompletionOrder has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given GeothermalPropertiesCompletionOrder is null, badly formed, or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the GeothermalPropertiesCompletionOrder of given ID from the microservice database, at the endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the GeothermalPropertiesCompletionOrder was deleted from the microservice database, at the endpoint GeothermalProperties/api/GeothermalPropertiesCompletionOrder/id</returns>
        [HttpDelete("{id}", Name = "DeleteGeothermalPropertiesCompletionOrderById")]
        public ActionResult DeleteGeothermalPropertiesCompletionOrderById(Guid id)
        {
            if (_geothermalPropertiesCompletionOrderManager.GetGeothermalPropertiesCompletionOrderById(id) != null)
            {
                if (_geothermalPropertiesCompletionOrderManager.DeleteGeothermalPropertiesCompletionOrderById(id))
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
                _logger.LogWarning("The GeothermalPropertiesCompletionOrder of given ID does not exist");
                return NotFound();
            }
        }
    }
}
