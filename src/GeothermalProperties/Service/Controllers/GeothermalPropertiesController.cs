using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.GeothermalProperties.Model;
using NORCE.Drilling.GeothermalProperties.Service.Managers;

namespace NORCE.Drilling.GeothermalProperties.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class GeothermalPropertiesController : ControllerBase
    {
        private readonly ILogger<GeothermalPropertiesManager> _logger;
        private readonly GeothermalPropertiesManager _geothermalPropertiesManager;

        public GeothermalPropertiesController(ILogger<GeothermalPropertiesManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _geothermalPropertiesManager = GeothermalPropertiesManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all GeothermalProperties present in the microservice database at endpoint GeothermalProperties/api/GeothermalProperties
        /// </summary>
        /// <returns>the list of Guid of all GeothermalProperties present in the microservice database at endpoint GeothermalProperties/api/GeothermalProperties</returns>
        [HttpGet(Name = "GetAllGeothermalPropertiesId")]
        public ActionResult<IEnumerable<Guid?>> GetAllGeothermalPropertiesId()
        {
            var ids = _geothermalPropertiesManager.GetAllGeothermalPropertiesId();
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
        /// Returns the list of MetaInfo of all GeothermalProperties present in the microservice database, at endpoint GeothermalProperties/api/GeothermalProperties/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all GeothermalProperties present in the microservice database, at endpoint GeothermalProperties/api/GeothermalProperties/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllGeothermalPropertiesMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo?>> GetAllGeothermalPropertiesMetaInfo()
        {
            var vals = _geothermalPropertiesManager.GetAllGeothermalPropertiesMetaInfo();
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
        /// Returns the GeothermalProperties identified by its Guid from the microservice database, at endpoint GeothermalProperties/api/GeothermalProperties/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the GeothermalProperties identified by its Guid from the microservice database, at endpoint GeothermalProperties/api/GeothermalProperties/id</returns>
        [HttpGet("{id}", Name = "GetGeothermalPropertiesById")]
        public ActionResult<Model.GeothermalProperties?> GetGeothermalPropertiesById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _geothermalPropertiesManager.GetGeothermalPropertiesById(id);
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
        /// Returns the list of all GeothermalProperties present in the microservice database, at endpoint GeothermalProperties/api/GeothermalProperties/HeavyData
        /// </summary>
        /// <returns>the list of all GeothermalProperties present in the microservice database, at endpoint GeothermalProperties/api/GeothermalProperties/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllGeothermalProperties")]
        public ActionResult<IEnumerable<Model.GeothermalProperties?>> GetAllGeothermalProperties()
        {
            var vals = _geothermalPropertiesManager.GetAllGeothermalProperties();
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
        /// Performs calculation on the given GeothermalProperties and adds it to the microservice database, at the endpoint GeothermalProperties/api/GeothermalProperties
        /// </summary>
        /// <param name="geothermalProperties"></param>
        /// <returns>true if the given GeothermalProperties has been added successfully to the microservice database, at the endpoint GeothermalProperties/api/GeothermalProperties</returns>
        [HttpPost(Name = "PostGeothermalProperties")]
        public ActionResult PostGeothermalProperties([FromBody] Model.GeothermalProperties? data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _geothermalPropertiesManager.GetGeothermalPropertiesById(data.MetaInfo.ID);
                if (existingData == null)
                {
                    if (_geothermalPropertiesManager.AddGeothermalProperties(data))
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
                    _logger.LogWarning("The given GeothermalProperties already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given GeothermalProperties is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given GeothermalProperties and updates it in the microservice database, at the endpoint GeothermalProperties/api/GeothermalProperties/id
        /// </summary>
        /// <param name="geothermalProperties"></param>
        /// <returns>true if the given GeothermalProperties has been updated successfully to the microservice database, at the endpoint GeothermalProperties/api/GeothermalProperties/id</returns>
        [HttpPut("{id}", Name = "PutGeothermalPropertiesById")]
        public ActionResult PutGeothermalPropertiesById(Guid id, [FromBody] Model.GeothermalProperties data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _geothermalPropertiesManager.GetGeothermalPropertiesById(id);
                if (existingData != null)
                {
                    if (_geothermalPropertiesManager.UpdateGeothermalPropertiesById(id, data))
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
                    _logger.LogWarning("The given GeothermalProperties has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given GeothermalProperties is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the GeothermalProperties of given ID from the microservice database, at the endpoint GeothermalProperties/api/GeothermalProperties/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the GeothermalProperties was deleted from the microservice database, at the endpoint GeothermalProperties/api/GeothermalProperties/id</returns>
        [HttpDelete("{id}", Name = "DeleteGeothermalPropertiesById")]
        public ActionResult DeleteGeothermalPropertiesById(Guid id)
        {
            if (_geothermalPropertiesManager.GetGeothermalPropertiesById(id) != null)
            {
                if (_geothermalPropertiesManager.DeleteGeothermalPropertiesById(id))
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
                _logger.LogWarning("The GeothermalProperties of given ID does not exist");
                return NotFound();
            }
        }
    }
}
