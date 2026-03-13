
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//--------------------------------------------------------------------------------------------------------------------

using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection;

namespace IllRequestPortal.Web.ApiController
{ 
    [Route("api/v1/[controller]s")]
    [ApiController]
    public partial class IllRequestController: ControllerBase
    {
        protected readonly ILogger<IllRequestController> logger;
        protected readonly IIllRequestServiceExtended service;

        public IllRequestController(ILogger<IllRequestController> logger, IIllRequestServiceExtended service)
        {
            this.logger = logger;
            this.service = service;
        }
      

        [HttpGet]
        public virtual async Task<IActionResult> Get()
        {
            var entities = await service.GetAll();
            if (!entities.Any()) logger.LogInformation("No content found.");
            return Ok(entities);            
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(int id)
        {
            
            var entity = await service.Get(id);
            if (entity == null) return NotFound();
            return Ok(entity);            
        }

        [HttpGet("search")]
        public async Task<IActionResult> Get([FromQuery] Dictionary<string, string> filters)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));

            var entityType = typeof(IllRequest);
            foreach (var key in filters.Keys)
            {
                var propertyInfo = entityType.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null) throw new ArgumentException($"Invalid filter parameter: {key}");
            }

            var entities = await service.Get(filters);
            if (!entities.Any()) return NotFound();
            return Ok(entities);
        }

        [HttpGet("since/{id}")]
        public async Task<IActionResult> GetSince(int id)
        {
            var items = await service.GetSince(id);
            if (items == null || !items.Any())
            {
                logger.LogInformation("No logs found since id {Id}.", id);
                return NoContent();
            }

            return Ok(items);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] IllRequest entity)
        {
            var newEntity = await service.Insert(entity);
            return CreatedAtAction(nameof(Post), new {id = newEntity.Id }, newEntity);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Put(int id, [FromBody] IllRequest entity)
        {
            if (!await service.Exists(id)) return NotFound();
            entity.Id = id;
            await service.Update(entity);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await service.Exists(id)) return NotFound();
            await service.Delete(id);
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}