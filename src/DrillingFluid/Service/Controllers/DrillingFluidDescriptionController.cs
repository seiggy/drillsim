using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using NORCE.Drilling.DrillingFluid.Model;
using NORCE.Drilling.DrillingFluid.Service.Managers;

namespace NORCE.Drilling.DrillingFluid.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class DrillingFluidDescriptionController : ControllerBase
    {
        private readonly ILogger<DrillingFluidDescriptionManager> _logger;
        private readonly DrillingFluidDescriptionManager _drillingFluidDescriptionManager;

        public DrillingFluidDescriptionController(ILogger<DrillingFluidDescriptionManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _drillingFluidDescriptionManager = DrillingFluidDescriptionManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all DrillingFluidDescription present in the microservice database at endpoint DrillingFluid/api/DrillingFluidDescription
        /// </summary>
        /// <returns>the list of Guid of all DrillingFluidDescription present in the microservice database at endpoint DrillingFluid/api/DrillingFluidDescription</returns>
        [HttpGet(Name = "GetAllDrillingFluidDescriptionId")]
        public ActionResult<IEnumerable<Guid?>> GetAllDrillingFluidDescriptionId()
        {
            var ids = _drillingFluidDescriptionManager.GetAllDrillingFluidDescriptionId();
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
        /// Returns the list of MetaInfo of all DrillingFluidDescription present in the microservice database, at endpoint DrillingFluid/api/DrillingFluidDescription/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all DrillingFluidDescription present in the microservice database, at endpoint DrillingFluid/api/DrillingFluidDescription/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllDrillingFluidDescriptionMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo?>> GetAllDrillingFluidDescriptionMetaInfo()
        {
            var vals = _drillingFluidDescriptionManager.GetAllDrillingFluidDescriptionMetaInfo();
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
        /// Returns the DrillingFluidDescription identified by its Guid from the microservice database, at endpoint DrillingFluid/api/DrillingFluidDescription/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the DrillingFluidDescription identified by its Guid from the microservice database, at endpoint DrillingFluid/api/DrillingFluidDescription/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetDrillingFluidDescriptionById")]
        public ActionResult<DrillingFluidDescription?> GetDrillingFluidDescriptionById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _drillingFluidDescriptionManager.GetDrillingFluidDescriptionById(id);
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
        /// Returns the list of all DrillingFluidDescription present in the microservice database, at endpoint DrillingFluid/api/DrillingFluidDescription/HeavyData
        /// </summary>
        /// <returns>the list of all DrillingFluidDescription present in the microservice database, at endpoint DrillingFluid/api/DrillingFluidDescription/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllDrillingFluidDescription")]
        public ActionResult<IEnumerable<DrillingFluidDescription?>> GetAllDrillingFluidDescription()
        {
            var vals = _drillingFluidDescriptionManager.GetAllDrillingFluidDescription();
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
        /// Performs calculation on the given DrillingFluidDescription and adds it to the microservice database, at the endpoint DrillingFluid/api/DrillingFluidDescription
        /// </summary>
        /// <param name="drillingFluidDescription"></param>
        /// <returns>true if the given DrillingFluidDescription has been added successfully to the microservice database, at the endpoint DrillingFluid/api/DrillingFluidDescription</returns>
        [HttpPost(Name = "PostDrillingFluidDescription")]
        public ActionResult PostDrillingFluidDescription([FromBody] Model.DrillingFluidDescription? data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _drillingFluidDescriptionManager.GetDrillingFluidDescriptionById(data.MetaInfo.ID);
                if (existingData == null)
                {
                    if (_drillingFluidDescriptionManager.AddDrillingFluidDescription(data))
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
                    _logger.LogWarning("The given DrillingFluidDescription already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluidDescription is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluidDescription and updates it in the microservice database, at the endpoint DrillingFluid/api/DrillingFluidDescription/id
        /// </summary>
        /// <param name="drillingFluidDescription"></param>
        /// <returns>true if the given DrillingFluidDescription has been updated successfully to the microservice database, at the endpoint DrillingFluid/api/DrillingFluidDescription/id</returns>
        [HttpPut("{id}", Name = "PutDrillingFluidDescriptionById")]
        public ActionResult PutDrillingFluidDescriptionById(Guid id, [FromBody] Model.DrillingFluidDescription data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _drillingFluidDescriptionManager.GetDrillingFluidDescriptionById(id);
                if (existingData != null)
                {
                    if (_drillingFluidDescriptionManager.UpdateDrillingFluidDescriptionById(id, data))
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
                    _logger.LogWarning("The given DrillingFluidDescription has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluidDescription is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the DrillingFluidDescription of given ID from the microservice database, at the endpoint DrillingFluid/api/DrillingFluidDescription/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the DrillingFluidDescription was deleted from the microservice database, at the endpoint DrillingFluid/api/DrillingFluidDescription/id</returns>
        [HttpDelete("{id}", Name = "DeleteDrillingFluidDescriptionById")]
        public ActionResult DeleteDrillingFluidDescriptionById(Guid id)
        {
            if (_drillingFluidDescriptionManager.GetDrillingFluidDescriptionById(id) != null)
            {
                if (_drillingFluidDescriptionManager.DeleteDrillingFluidDescriptionById(id))
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
                _logger.LogWarning("The DrillingFluidDescription of given ID does not exist");
                return NotFound();
            }
        }
    }
}
