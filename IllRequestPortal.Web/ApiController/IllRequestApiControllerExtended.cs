
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
    public partial class IllRequestController : ControllerBase
    {

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var item = await service.UpdateStatus(id, request.Status);

            if (item == null)
                return NotFound();

            return Ok();
        }
        public class UpdateStatusRequest
        {
            public string Status { get; set; }
        }
    }
}