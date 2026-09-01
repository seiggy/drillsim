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
    public class BaseOilController : ControllerBase
    {
        private readonly ILogger<BaseOilManager> _logger;
        private readonly BaseOilManager _baseOilManager;

        public BaseOilController(ILogger<BaseOilManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _baseOilManager = BaseOilManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all BaseOil present in the microservice database at endpoint DrillingFluid/api/BaseOil
        /// </summary>
        /// <returns>the list of Guid of all BaseOil present in the microservice database at endpoint DrillingFluid/api/BaseOil</returns>
        [HttpGet(Name = "GetAllBaseOilId")]
        public ActionResult<IEnumerable<Guid?>> GetAllBaseOilId()
        {
            var ids = _baseOilManager.GetAllBaseOilId();
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
        /// Returns the list of MetaInfo of all BaseOil present in the microservice database, at endpoint DrillingFluid/api/BaseOil/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all BaseOil present in the microservice database, at endpoint DrillingFluid/api/BaseOil/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllBaseOilMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo?>> GetAllBaseOilMetaInfo()
        {
            var vals = _baseOilManager.GetAllBaseOilMetaInfo();
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
        /// Returns the BaseOil identified by its Guid from the microservice database, at endpoint DrillingFluid/api/BaseOil/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the BaseOil identified by its Guid from the microservice database, at endpoint DrillingFluid/api/BaseOil/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetBaseOilById")]
        public ActionResult<BaseOil?> GetBaseOilById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _baseOilManager.GetBaseOilById(id);
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
        /// Returns the list of all BaseOil present in the microservice database, at endpoint DrillingFluid/api/BaseOil/HeavyData
        /// </summary>
        /// <returns>the list of all BaseOil present in the microservice database, at endpoint DrillingFluid/api/BaseOil/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllBaseOil")]
        public ActionResult<IEnumerable<BaseOil?>> GetAllBaseOil()
        {
            var vals = _baseOilManager.GetAllBaseOil();
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
        /// Performs calculation on the given BaseOil and adds it to the microservice database, at the endpoint DrillingFluid/api/BaseOil
        /// </summary>
        /// <param name="baseOil"></param>
        /// <returns>true if the given BaseOil has been added successfully to the microservice database, at the endpoint DrillingFluid/api/BaseOil</returns>
        [HttpPost(Name = "PostBaseOil")]
        public ActionResult PostBaseOil([FromBody] Model.BaseOil? data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _baseOilManager.GetBaseOilById(data.MetaInfo.ID);
                if (existingData == null)
                {
                    if (_baseOilManager.AddBaseOil(data))
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
                    _logger.LogWarning("The given BaseOil already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given BaseOil is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given BaseOil and updates it in the microservice database, at the endpoint DrillingFluid/api/BaseOil/id
        /// </summary>
        /// <param name="baseOil"></param>
        /// <returns>true if the given BaseOil has been updated successfully to the microservice database, at the endpoint DrillingFluid/api/BaseOil/id</returns>
        [HttpPut("{id}", Name = "PutBaseOilById")]
        public ActionResult PutBaseOilById(Guid id, [FromBody] Model.BaseOil data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _baseOilManager.GetBaseOilById(id);
                if (existingData != null)
                {
                    if (_baseOilManager.UpdateBaseOilById(id, data))
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
                    _logger.LogWarning("The given BaseOil has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given BaseOil is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the BaseOil of given ID from the microservice database, at the endpoint DrillingFluid/api/BaseOil/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the BaseOil was deleted from the microservice database, at the endpoint DrillingFluid/api/BaseOil/id</returns>
        [HttpDelete("{id}", Name = "DeleteBaseOilById")]
        public ActionResult DeleteBaseOilById(Guid id)
        {
            if (_baseOilManager.GetBaseOilById(id) != null)
            {
                if (_baseOilManager.DeleteBaseOilById(id))
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
                _logger.LogWarning("The BaseOil of given ID does not exist");
                return NotFound();
            }
        }
    }
}
