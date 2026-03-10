using IllRequestPortal.Logic.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IllRequestPortal.Web.Controllers
{
    public partial class IllRequestController 
    {

        [HttpPost]
        public async Task<IActionResult> MarkAsRegisteredInLibris([FromServices] IIllRequestServiceExtended illRequestServiceExtended, int id)
        {
            var request = await illRequestServiceExtended.MarkAsRegisteredInLibris(id);

            if (request == null) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}