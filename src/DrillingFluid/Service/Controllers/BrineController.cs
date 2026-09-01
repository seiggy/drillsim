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
    public class BrineController : ControllerBase
    {
        private readonly ILogger<BrineManager> _logger;
        private readonly BrineManager _brineManager;

        public BrineController(ILogger<BrineManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _brineManager = BrineManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all Brine present in the microservice database at endpoint DrillingFluid/api/Brine
        /// </summary>
        /// <returns>the list of Guid of all Brine present in the microservice database at endpoint DrillingFluid/api/Brine</returns>
        [HttpGet(Name = "GetAllBrineId")]
        public ActionResult<IEnumerable<Guid?>> GetAllBrineId()
        {
            var ids = _brineManager.GetAllBrineId();
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
        /// Returns the list of MetaInfo of all Brine present in the microservice database, at endpoint DrillingFluid/api/Brine/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all Brine present in the microservice database, at endpoint DrillingFluid/api/Brine/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllBrineMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo?>> GetAllBrineMetaInfo()
        {
            var vals = _brineManager.GetAllBrineMetaInfo();
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
        /// Returns the Brine identified by its Guid from the microservice database, at endpoint DrillingFluid/api/Brine/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the Brine identified by its Guid from the microservice database, at endpoint DrillingFluid/api/Brine/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetBrineById")]
        public ActionResult<Brine?> GetBrineById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _brineManager.GetBrineById(id);
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
        /// Returns the list of all Brine present in the microservice database, at endpoint DrillingFluid/api/Brine/HeavyData
        /// </summary>
        /// <returns>the list of all Brine present in the microservice database, at endpoint DrillingFluid/api/Brine/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllBrine")]
        public ActionResult<IEnumerable<Brine?>> GetAllBrine()
        {
            var vals = _brineManager.GetAllBrine();
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
        /// Performs calculation on the given Brine and adds it to the microservice database, at the endpoint DrillingFluid/api/Brine
        /// </summary>
        /// <param name="brine"></param>
        /// <returns>true if the given Brine has been added successfully to the microservice database, at the endpoint DrillingFluid/api/Brine</returns>
        [HttpPost(Name = "PostBrine")]
        public ActionResult PostBrine([FromBody] Model.Brine? data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _brineManager.GetBrineById(data.MetaInfo.ID);
                if (existingData == null)
                {
                    if (_brineManager.AddBrine(data))
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
                    _logger.LogWarning("The given Brine already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given Brine is null or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given Brine and updates it in the microservice database, at the endpoint DrillingFluid/api/Brine/id
        /// </summary>
        /// <param name="brine"></param>
        /// <returns>true if the given Brine has been updated successfully to the microservice database, at the endpoint DrillingFluid/api/Brine/id</returns>
        [HttpPut("{id}", Name = "PutBrineById")]
        public ActionResult PutBrineById(Guid id, [FromBody] Model.Brine data)
        {
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _brineManager.GetBrineById(id);
                if (existingData != null)
                {
                    if (_brineManager.UpdateBrineById(id, data))
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
                    _logger.LogWarning("The given Brine has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given Brine is null or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the Brine of given ID from the microservice database, at the endpoint DrillingFluid/api/Brine/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the Brine was deleted from the microservice database, at the endpoint DrillingFluid/api/Brine/id</returns>
        [HttpDelete("{id}", Name = "DeleteBrineById")]
        public ActionResult DeleteBrineById(Guid id)
        {
            if (_brineManager.GetBrineById(id) != null)
            {
                if (_brineManager.DeleteBrineById(id))
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
                _logger.LogWarning("The Brine of given ID does not exist");
                return NotFound();
            }
        }
    }
}
