using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.UnitConversion.Model;

namespace OSDC.UnitConversion.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class UnitSystemConversionSetController : ControllerBase
    {
        private readonly ILogger<UnitSystemConversionSetManager> _logger;
        private readonly UnitSystemConversionSetManager _unitSystemConversionSetManager;

        public UnitSystemConversionSetController(ILogger<UnitSystemConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _unitSystemConversionSetManager = UnitSystemConversionSetManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all UnitSystemConversionSet present in the microservice database at endpoint UnitConversion/api/UnitSystemConversionSet
        /// </summary>
        /// <returns>the list of Guid of all UnitSystemConversionSet present in the microservice database at endpoint UnitConversion/api/UnitSystemConversionSet</returns>
        [HttpGet(Name = "GetAllUnitSystemConversionSetId")]
        public ActionResult<IEnumerable<Guid>> GetAllUnitSystemConversionSetId()
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerGetAllIDPerDay();
            var ids = _unitSystemConversionSetManager.GetAllUnitSystemConversionSetId();
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
        /// Returns the list of MetaInfo of all UnitSystemConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitSystemConversionSet/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all UnitSystemConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitSystemConversionSet/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllUnitSystemConversionSetMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllUnitSystemConversionSetMetaInfo()
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerGetAllMetaInfoPerDay();
            var vals = _unitSystemConversionSetManager.GetAllUnitSystemConversionSetMetaInfo();
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
        /// Returns the UnitSystemConversionSet identified by its Guid from the microservice database, at endpoint UnitConversion/api/UnitSystemConversionSet/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the UnitSystemConversionSet identified by its Guid from the microservice database, at endpoint UnitConversion/api/UnitSystemConversionSet/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetUnitSystemConversionSetById")]
        public ActionResult<Model.UnitSystemConversionSet> GetUnitSystemConversionSetById(Guid id)
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerGetByIDPerDay();
            if (!id.Equals(Guid.Empty))
            {
                var val = _unitSystemConversionSetManager.GetUnitSystemConversionSetById(id);
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
        /// Returns the list of all UnitSystemConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitSystemConversionSet/HeavyData
        /// </summary>
        /// <returns>the list of all UnitSystemConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitSystemConversionSet/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllUnitSystemConversionSet")]
        public ActionResult<IEnumerable<Model.UnitSystemConversionSet>> GetAllUnitSystemConversionSet()
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerGetAllPerDay();
            var vals = _unitSystemConversionSetManager.GetAllUnitSystemConversionSet();
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
        /// Performs calculation on the given UnitSystemConversionSet and adds it to the microservice database, at the endpoint UnitConversion/api/UnitSystemConversionSet
        /// </summary>
        /// <param name="unitSystemConversionSet"></param>
        /// <returns>true if the given UnitSystemConversionSet has been added successfully to the microservice database, at the endpoint UnitConversion/api/UnitSystemConversionSet</returns>
        [HttpPost(Name = "PostUnitSystemConversionSet")]
        public ActionResult PostUnitSystemConversionSet([FromBody] Model.UnitSystemConversionSet value)
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerPostPerDay();
            if (value != null && value.MetaInfo != null && value.MetaInfo.ID != Guid.Empty)
            {
                Model.UnitSystemConversionSet? unitSystemConversionSet = _unitSystemConversionSetManager.GetUnitSystemConversionSetById(value.MetaInfo.ID);
                if (unitSystemConversionSet == null)
                {
                    if (_unitSystemConversionSetManager.AddUnitSystemConversionSet(value))
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
                    _logger.LogWarning("The given UnitSystemConversionSet already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given UnitSystemConversionSet is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given UnitSystemConversionSet and updates it in the microservice database, at the endpoint UnitConversion/api/UnitSystemConversionSet/id
        /// </summary>
        /// <param name="unitSystemConversionSet"></param>
        /// <returns>true if the given UnitSystemConversionSet has been updated successfully to the microservice database, at the endpoint UnitConversion/api/UnitSystemConversionSet/id</returns>
        [HttpPut("{id}", Name = "PutUnitSystemConversionSetById")]
        public ActionResult PutUnitSystemConversionSetById(Guid id, [FromBody] Model.UnitSystemConversionSet value)
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerPutPerDay();
            if (value != null && value.MetaInfo != null && value.MetaInfo.ID.Equals(id))
            {
                Model.UnitSystemConversionSet? unitSystemConversionSet = _unitSystemConversionSetManager.GetUnitSystemConversionSetById(id);
                if (unitSystemConversionSet != null)
                {
                    if (_unitSystemConversionSetManager.UpdateUnitSystemConversionSetById(id, value))
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
                    _logger.LogWarning("The given UnitSystemConversionSet has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given UnitSystemConversionSet is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the UnitSystemConversionSet of given ID from the microservice database, at the endpoint UnitConversion/api/UnitSystemConversionSet/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the UnitSystemConversionSet was deleted from the microservice database, at the endpoint UnitConversion/api/UnitSystemConversionSet/id</returns>
        [HttpDelete("{id}", Name = "DeleteUnitSystemConversionSetById")]
        public ActionResult DeleteUnitSystemConversionSetById(Guid id)
        {
            UsageStatistics.Instance.IncrementUnitSystemConversionSetControllerDeletePerDay();
            if (_unitSystemConversionSetManager.GetUnitSystemConversionSetById(id) != null)
            {
                if (_unitSystemConversionSetManager.DeleteUnitSystemConversionSetById(id))
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
                _logger.LogWarning("The UnitSystemConversionSet of given ID does not exist");
                return NotFound();
            }
        }
    }
}
