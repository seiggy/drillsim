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
    public class DrillingFluidOrderController : ControllerBase
    {
        private readonly ILogger<DrillingFluidOrderManager> _logger;
        private readonly DrillingFluidOrderManager _drillingFluidOrderManager;

        public DrillingFluidOrderController(ILogger<DrillingFluidOrderManager> logger, SqlConnectionManager connectionManager)
        {
            _logger = logger;
            _drillingFluidOrderManager = DrillingFluidOrderManager.GetInstance(_logger, connectionManager);
        }

        /// <summary>
        /// Returns the list of Guid of all DrillingFluidOrder present in the microservice database at endpoint DrillingFluidOrder/api/DrillingFluidOrder
        /// </summary>
        /// <returns>the list of Guid of all DrillingFluidOrder present in the microservice database at endpoint DrillingFluidOrder/api/DrillingFluidOrder</returns>
        [HttpGet(Name = "GetAllDrillingFluidOrderId")]
        public ActionResult<IEnumerable<Guid>> GetAllDrillingFluidOrderId()
        {
            var ids = _drillingFluidOrderManager.GetAllDrillingFluidOrderId();
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
        /// Returns the list of MetaInfo of all DrillingFluidOrder present in the microservice database, at endpoint DrillingFluidOrder/api/DrillingFluidOrder/MetaInfo
        /// </summary>
        /// <returns>the list of MetaInfo of all DrillingFluidOrder present in the microservice database, at endpoint DrillingFluidOrder/api/DrillingFluidOrder/MetaInfo</returns>
        [HttpGet("MetaInfo", Name = "GetAllDrillingFluidOrderMetaInfo")]
        public ActionResult<IEnumerable<MetaInfo>> GetAllDrillingFluidOrderMetaInfo()
        {
            var vals = _drillingFluidOrderManager.GetAllDrillingFluidOrderMetaInfo();
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
        /// Returns the DrillingFluidOrder identified by its Guid from the microservice database, at endpoint DrillingFluidOrder/api/DrillingFluidOrder/MetaInfo/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>the DrillingFluidOrder identified by its Guid from the microservice database, at endpoint DrillingFluidOrder/api/DrillingFluidOrder/MetaInfo/id</returns>
        [HttpGet("{id}", Name = "GetDrillingFluidOrderById")]
        public ActionResult<Model.DrillingFluidOrder?> GetDrillingFluidOrderById(Guid id)
        {
            if (!id.Equals(Guid.Empty))
            {
                var val = _drillingFluidOrderManager.GetDrillingFluidOrderById(id);
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
        /// Returns the list of all DrillingFluidOrder present in the microservice database, at endpoint DrillingFluidOrder/api/DrillingFluidOrder/HeavyData
        /// </summary>
        /// <returns>the list of all DrillingFluidOrder present in the microservice database, at endpoint DrillingFluidOrder/api/DrillingFluidOrder/HeavyData</returns>
        [HttpGet("HeavyData", Name = "GetAllDrillingFluidOrder")]
        public ActionResult<IEnumerable<Model.DrillingFluidOrder?>> GetAllDrillingFluidOrder()
        {
            var vals = _drillingFluidOrderManager.GetAllDrillingFluidOrder();
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
        /// Performs calculation on the given DrillingFluidOrder and adds it to the microservice database, at the endpoint DrillingFluidOrder/api/DrillingFluidOrder
        /// </summary>
        /// <param name="drillingFluidOrder"></param>
        /// <returns>true if the given DrillingFluidOrder has been added successfully to the microservice database, at the endpoint DrillingFluidOrder/api/DrillingFluidOrder</returns>
        [HttpPost(Name = "PostDrillingFluidOrder")]
        public ActionResult PostDrillingFluidOrder([FromBody] Model.DrillingFluidOrder? data)
        {
            // Check if drillingFluidOrder exists in the database through ID
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID != Guid.Empty)
            {
                var existingData = _drillingFluidOrderManager.GetDrillingFluidOrderById(data.MetaInfo.ID);
                if (existingData == null)
                {
                    //  If drillingFluidOrder was not found, call AddDrillingFluidOrder, where the drillingFluidOrder.Calculate()
                    // method is called. 
                    if (_drillingFluidOrderManager.AddDrillingFluidOrder(data))
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
                    _logger.LogWarning("The given DrillingFluidOrder already exists and will not be added");
                    return StatusCode(StatusCodes.Status409Conflict);
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluidOrder is null, badly formed, or its ID is empty");
                return BadRequest();
            }
        }

        /// <summary>
        /// Performs calculation on the given DrillingFluidOrder and updates it in the microservice database, at the endpoint DrillingFluidOrder/api/DrillingFluidOrder/id
        /// </summary>
        /// <param name="drillingFluidOrder"></param>
        /// <returns>true if the given DrillingFluidOrder has been updated successfully to the microservice database, at the endpoint DrillingFluidOrder/api/DrillingFluidOrder/id</returns>
        [HttpPut("{id}", Name = "PutDrillingFluidOrderById")]
        public ActionResult PutDrillingFluidOrderById(Guid id, [FromBody] Model.DrillingFluidOrder? data)
        {
            // Check if DrillingFluidOrder is in the data base
            if (data != null && data.MetaInfo != null && data.MetaInfo.ID.Equals(id))
            {
                var existingData = _drillingFluidOrderManager.GetDrillingFluidOrderById(id);
                if (existingData != null)
                {
                    if (_drillingFluidOrderManager.UpdateDrillingFluidOrderById(id, data))
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
                    _logger.LogWarning("The given DrillingFluidOrder has not been found in the database");
                    return NotFound();
                }
            }
            else
            {
                _logger.LogWarning("The given DrillingFluidOrder is null, badly formed, or its does not match the ID to update");
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the DrillingFluidOrder of given ID from the microservice database, at the endpoint DrillingFluidOrder/api/DrillingFluidOrder/id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>true if the DrillingFluidOrder was deleted from the microservice database, at the endpoint DrillingFluidOrder/api/DrillingFluidOrder/id</returns>
        [HttpDelete("{id}", Name = "DeleteDrillingFluidOrderById")]
        public ActionResult DeleteDrillingFluidOrderById(Guid id)
        {
            if (_drillingFluidOrderManager.GetDrillingFluidOrderById(id) != null)
            {
                if (_drillingFluidOrderManager.DeleteDrillingFluidOrderById(id))
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
                _logger.LogWarning("The DrillingFluidOrder of given ID does not exist");
                return NotFound();
            }
        }
    }
}
