using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IllRequestPortal.Logic.Settings;
using static Logic.Util.StandardNumberUtil;
using static Logic.Model.BibliographicRecordConstants;
using IllRequestPortal.Web.ViewModel;
using IllRequestPortal.Logic.Http;
using System.Linq;
using System;
using System.Collections;
using Logic.Model;
using System.Collections.Generic;
using IllRequestPortal.Logic.Services;

namespace IllRequestPortal.Web.ApiController
{
    [Route("api/v1/bibliographic-records")]
    [ApiController]
    public partial class BibliographicRecordsController: ControllerBase
    {
        protected readonly ILogger<BibliographicRecordsController> logger;
        private readonly IKohaGetHttpService kohaGetHttpService;
        private readonly ILibrisService librisService;
        private readonly KohaApiSettings kohaApiSettings;

        public BibliographicRecordsController(ILogger<BibliographicRecordsController> logger,
            IKohaGetHttpService kohaGetHttpService,
            ILibrisService librisService,
            IOptions<KohaApiSettings> kohaApiSettingsOptions)
        {
            this.logger = logger;
            this.kohaGetHttpService = kohaGetHttpService;
            this.librisService = librisService;
            this.kohaApiSettings = kohaApiSettingsOptions.Value;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string standardNumber)
        {
            if (string.IsNullOrWhiteSpace(standardNumber))
                return BadRequest();

            var records = await FetchFromKoha(standardNumber);

            if (records.Any())
            {
                return Ok(new LookupBibliographicRecordResponse
                {
                    Status = LookupStatuses.FoundInKoha,
                    Message = "Item already exists in Koha"
                });
            }

            var librisMatch = await librisService.FetchBiblio(standardNumber);

            if (librisMatch != null)
            {
                return Ok(new LookupBibliographicRecordResponse
                {
                    Status = LookupStatuses.FoundInLibris,
                    Message = "Bibliographic data found in Libris",
                    Title = librisMatch.Title,
                    Author = librisMatch.Author,
                    PublicationYear = librisMatch.PublicationYear,
                    Edition = librisMatch.Edition,
                    MaterialType = librisMatch.MaterialType
                });
            }

            return Ok(new LookupBibliographicRecordResponse
            {
                Status = LookupStatuses.NotFound,
                Message = "No matching record found"
            });
        }

        private async Task<IEnumerable<KohaPatronEntity>> FetchFromKoha(string standardNumber)
        {
            // den här funkar https://kosodertorn-opac.prod.imcode.com/api/v1/app.pl/api/v1/biblios?q={%22isbn%22%3A%229789100139117%22}
            // måste har accept header application/json satt 
            return new List<KohaPatronEntity>();
            // har inte hittat något sätt att hämta från koha vi isbn 

            //var normalized = StandardNumberUtility.Normalize(standardNumber);
            //var queryField = StandardNumberUtility.Detect(normalized) == "ISSN" ? "issn" : "isbn";
            //var q = Uri.EscapeDataString($"{{\"{queryField}\":\"{normalized}\"}}");
            //string url = $"{kohaApiSettings.BaseUrl}/biblios?q={q}";

            //kohaGetHttpService.OverrideDefaultBearerToken(kohaApiSettings.AuthenticationHeaderValue);

            //var records = await kohaGetHttpService.FetchAll(url);
            //return records;

        }
    }
}