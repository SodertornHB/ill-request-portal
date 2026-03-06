using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IllRequestPortal.Logic.Settings;

namespace IllRequestPortal.Web.ApiController
{
    [Route("api/v1/[controller]s")]
    [ApiController]
    public partial class PatronController: ControllerBase
    {
        protected readonly ILogger<PatronController> logger;
        private readonly IKohaPatronGetHttpService kohaPatronGetHttpService;
        private readonly KohaApiSettings kohaApiSettings;

        public PatronController(ILogger<PatronController> logger,
            IKohaPatronGetHttpService kohaPatronGetHttpService,
            IOptions<KohaApiSettings> kohaApiSettingsOptions)
        {
            this.logger = logger;
            this.kohaPatronGetHttpService = kohaPatronGetHttpService;
            this.kohaApiSettings = kohaApiSettingsOptions.Value;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get([FromQuery] string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return BadRequest();

            string url = $"{kohaApiSettings.BaseUrl}/patrons?cardnumber={cardNumber}";

            kohaPatronGetHttpService.OverrideDefaultBearerToken(kohaApiSettings.AuthenticationHeaderValue);

            var patrons = await kohaPatronGetHttpService.FetchAll(url);

            var patron = patrons.FirstOrDefault();

            if (patron == null)
                return NotFound();

            return Ok(patron);
        }

    }
}