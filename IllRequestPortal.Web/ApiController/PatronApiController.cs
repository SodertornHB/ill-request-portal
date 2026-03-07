using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IllRequestPortal.Logic.Settings;
using IllRequestPortal.Logic.Http;

namespace IllRequestPortal.Web.ApiController
{
    [Route("api/v1/[controller]s")]
    [ApiController]
    public partial class PatronController: ControllerBase
    {
        protected readonly ILogger<PatronController> logger;
        private readonly IKohaGetHttpService kohaGetHttpService;
        private readonly KohaApiSettings kohaApiSettings;

        public PatronController(ILogger<PatronController> logger,
            IKohaGetHttpService kohaPatronGetHttpService,
            IOptions<KohaApiSettings> kohaApiSettingsOptions)
        {
            this.logger = logger;
            this.kohaGetHttpService = kohaPatronGetHttpService;
            this.kohaApiSettings = kohaApiSettingsOptions.Value;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get([FromQuery] string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return BadRequest();

            string url = $"{kohaApiSettings.BaseUrl}/patrons?cardnumber={cardNumber}";

            kohaGetHttpService.OverrideDefaultBearerToken(kohaApiSettings.AuthenticationHeaderValue);

            var patrons = await kohaGetHttpService.FetchAll(url);

            var patron = patrons.FirstOrDefault();

            if (patron == null)
                return NotFound();

            return Ok(patron);
        }

    }
}