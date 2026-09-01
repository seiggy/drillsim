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
    public class UnitConversionSetController : ControllerBase
    {
        private readonly ILogger<UnitConversionSetManager> _logger;
        private readonly UnitConversionSetManager _unitConversionSetManager;

        public UnitConversionSetController(ILogger<UnitConversionSetManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _unitConversionSetManager = UnitConversionSetManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all UnitConversionSet present in the microservice database at endpoint UnitConversion/api/UnitConversionSet
        /// </summary>
        /// <returns>the list of Guid of all UnitConversionSet present in the microservice database at endpoint UnitConversion/api/UnitConversionSet</returns>
        [HttpGet(Name = "GetAllUnitConversionSetId")]
        public ActionResult<IEnumerable<Guid>> GetAllUnitConversionSetId()
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerGetAllIDPerDay();
            var ids = _unitConversionSetManager.GetAllUnitConversionSetId();
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
        /// Returns the list of MetaInfo of all UnitConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitConversionSet/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all UnitConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitConversionSet/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllUnitConversionSetMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllUnitConversionSetMetaInfo()
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerGetAllMetaInfoPerDay();
            var vals = _unitConversionSetManager.GetAllUnitConversionSetMetaInfo();
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
        /// Returns the UnitConversionSet identified by its Guid from the microservice database, at endpoint UnitConversion/api/UnitConversionSet/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the UnitConversionSet identified by its Guid from the microservice database, at endpoint UnitConversion/api/UnitConversionSet/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetUnitConversionSetById")]
        public ActionResult<Model.UnitConversionSet> GetUnitConversionSetById(Guid id)
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerGetByIDPerDay();
            if (!id.Equals(Guid.Empty))
            {
                var val = _unitConversionSetManager.GetUnitConversionSetById(id);
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
        /// Returns the list of all UnitConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitConversionSet/HeavyData
        /// </summary>
        /// <returns>the list of all UnitConversionSet present in the microservice database, at endpoint UnitConversion/api/UnitConversionSet/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllUnitConversionSet")]
        public ActionResult<IEnumerable<Model.UnitConversionSet>> GetAllUnitConversionSet()
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerGetAllPerDay();
            var vals = _unitConversionSetManager.GetAllUnitConversionSet();
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
        /// Performs calculation on the given UnitConversionSet and adds it to the microservice database, at the endpoint UnitConversion/api/UnitConversionSet
        /// </summary>
        /// <param name="unitConversionSet"></param>
        /// <returns>true if the given UnitConversionSet has been added successfully to the microservice database, at the endpoint UnitConversion/api/UnitConversionSet</returns>
        [HttpPost(Name = "PostUnitConversionSet")]
        public ActionResult PostUnitConversionSet([FromBody] Model.UnitConversionSet value)
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerPostPerDay();
            if (value != null && value.MetaInfo != null && value.MetaInfo.ID != Guid.Empty)
            {
                Model.UnitConversionSet? unitConversionSet = _unitConversionSetManager.GetUnitConversionSetById(value.MetaInfo.ID);
                if (unitConversionSet == null)
                {
                    if (_unitConversionSetManager.AddUnitConversionSet(value))
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
                    _logger.LogWarning("The given UnitConversionSet already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given UnitConversionSet is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given UnitConversionSet and updates it in the microservice database, at the endpoint UnitConversion/api/UnitConversionSet/id
        /// </summary>
        /// <param name="unitConversionSet"></param>
        /// <returns>true if the given UnitConversionSet has been updated successfully to the microservice database, at the endpoint UnitConversion/api/UnitConversionSet/id</returns>
        [HttpPut("{id}", Name = "PutUnitConversionSetById")]
        public ActionResult PutUnitConversionSetById(Guid id, [FromBody] Model.UnitConversionSet value)
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerPutPerDay();
            if (value != null && value.MetaInfo != null && value.MetaInfo.ID.Equals(id))
            {
                Model.UnitConversionSet? unitConversionSet = _unitConversionSetManager.GetUnitConversionSetById(id);
                if (unitConversionSet != null)
                {
                    if (_unitConversionSetManager.UpdateUnitConversionSetById(id, value))
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
                    _logger.LogWarning("The given UnitConversionSet has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given UnitConversionSet is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the UnitConversionSet of given ID from the microservice database, at the endpoint UnitConversion/api/UnitConversionSet/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the UnitConversionSet was deleted from the microservice database, at the endpoint UnitConversion/api/UnitConversionSet/id</returns>
        [HttpDelete("{id}", Name = "DeleteUnitConversionSetById")]
        public ActionResult DeleteUnitConversionSetById(Guid id)
        {
            UsageStatistics.Instance.IncrementUnitConversionSetControllerDeletePerDay();
            if (_unitConversionSetManager.GetUnitConversionSetById(id) != null)
            {
                if (_unitConversionSetManager.DeleteUnitConversionSetById(id))
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
                _logger.LogWarning("The UnitConversionSet of given ID does not exist");
                return NotFound();
            }
        }
    }
}
