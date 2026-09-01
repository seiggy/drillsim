using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.DrillingFluid.Service.Managers;

namespace NORCE.Drilling.DrillingFluid.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class DrillingFluidController : ControllerBase
    {
        private readonly ILogger<DrillingFluidManager> _logger;
        private readonly DrillingFluidManager _drillingFluidManager;

        public DrillingFluidController(ILogger<DrillingFluidManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _drillingFluidManager = DrillingFluidManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all DrillingFluid present in the microservice database at endpoint DrillingFluid/api/DrillingFluid
        /// </summary>
        /// <returns>the list of Guid of all DrillingFluid present in the microservice database at endpoint DrillingFluid/api/DrillingFluid</returns>
        [HttpGet(Name = "GetAllDrillingFluidId")]
        public ActionResult<IEnumerable<Guid>> GetAllDrillingFluidId()
        {
            var ids = _drillingFluidManager.GetAllDrillingFluidId();
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
        /// Returns the list of MetaInfo of all DrillingFluid present in the microservice database, at endpoint DrillingFluid/api/DrillingFluid/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all DrillingFluid present in the microservice database, at endpoint DrillingFluid/api/DrillingFluid/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllDrillingFluidMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllDrillingFluidMetaInfo()
        {
            var vals = _drillingFluidManager.GetAllDrillingFluidMetaInfo();
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
        /// Returns the DrillingFluid identified by its Guid from the microservice database, at endpoint DrillingFluid/api/DrillingFluid/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the DrillingFluid identified by its Guid from the microservice database, at endpoint DrillingFluid/api/DrillingFluid/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetDrillingFluidById")]
        public ActionResult<Model.DrillingFluid?> GetDrillingFluidById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _drillingFluidManager.GetDrillingFluidById(id);
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
        /// Returns the list of all DrillingFluidLight present in the microservice database, at endpoint DrillingFluid/api/DrillingFluid/LightData
        /// </summary>
        /// <returns>the list of all DrillingFluidLight present in the microservice database, at endpoint DrillingFluid/api/DrillingFluid/LightData</returns>
        [HttpGet("LightData", Name = "GetAllDrillingFluidLight")]
        public ActionResult<IEnumerable<Model.DrillingFluidLight>> GetAllDrillingFluidLight()
        {
            var vals = _drillingFluidManager.GetAllDrillingFluidLight();
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
        /// Returns the list of all DrillingFluid present in the microservice database, at endpoint DrillingFluid/api/DrillingFluid/HeavyData
        /// </summary>
        /// <returns>the list of all DrillingFluid present in the microservice database, at endpoint DrillingFluid/api/DrillingFluid/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllDrillingFluid")]
        public ActionResult<IEnumerable<Model.DrillingFluid?>> GetAllDrillingFluid()
        {
            var vals = _drillingFluidManager.GetAllDrillingFluid();
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
        /// Performs calculation on the given DrillingFluid and adds it to the microservice database, at the endpoint DrillingFluid/api/DrillingFluid
        /// </summary>
        /// <param name="drillingFluid"></param>
        /// <returns>true if the given DrillingFluid has been added successfully to the microservice database, at the endpoint DrillingFluid/api/DrillingFluid</returns>
        [HttpPost(Name = "PostDrillingFluid")]
        public ActionResult PostDrillingFluid([FromBody] Model.DrillingFluid? data)
        {
            // Check if drillingFluid exists in the database through ID
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _drillingFluidManager.GetDrillingFluidById(data.MetaInfo.ID);
                if (existingData == null)
                {   
                    //  If drillingFluid was not found, call AddDrillingFluid, where the drillingFluid.Calculate()
                    // method is called. 
                    if (_drillingFluidManager.AddDrillingFluid(data))
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
                    _logger.LogWarning("The given DrillingFluid already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluid is null, badly formed, or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluid and updates it in the microservice database, at the endpoint DrillingFluid/api/DrillingFluid/id
        /// </summary>
        /// <param name="drillingFluid"></param>
        /// <returns>true if the given DrillingFluid has been updated successfully to the microservice database, at the endpoint DrillingFluid/api/DrillingFluid/id</returns>
        [HttpPut("{id}", Name = "PutDrillingFluidById")]
        public ActionResult PutDrillingFluidById(Guid id, [FromBody] Model.DrillingFluid? data)
        {
            // Check if DrillingFluid is in the data base
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _drillingFluidManager.GetDrillingFluidById(id);
                if (existingData != null)
                {
                    if (_drillingFluidManager.UpdateDrillingFluidById(id, data))
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
                    _logger.LogWarning("The given DrillingFluid has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluid is null, badly formed, or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the DrillingFluid of given ID from the microservice database, at the endpoint DrillingFluid/api/DrillingFluid/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the DrillingFluid was deleted from the microservice database, at the endpoint DrillingFluid/api/DrillingFluid/id</returns>
        [HttpDelete("{id}", Name = "DeleteDrillingFluidById")]
        public ActionResult DeleteDrillingFluidById(Guid id)
        {
            if (_drillingFluidManager.GetDrillingFluidById(id) != null)
            {
                if (_drillingFluidManager.DeleteDrillingFluidById(id))
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
                _logger.LogWarning("The DrillingFluid of given ID does not exist");
                return NotFound();
            }
        }
    }
}
