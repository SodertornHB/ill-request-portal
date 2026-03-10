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
using Logic.Model;
using IllRequestPortal.Logic.Services;

namespace IllRequestPortal.Web.ApiController
{
    [Route("api/v1/bibliographic-records")]
    [ApiController]
    public partial class BibliographicRecordsController: ControllerBase
    {
        protected readonly ILogger<BibliographicRecordsController> logger;
        private readonly IKohaPatronGetHttpService kohaGetHttpService;
        private readonly IKohaBiblioGetHttpService kohaBiblioGetHttpService;
        private readonly ILibrisService librisService;
        private readonly KohaApiSettings kohaApiSettings;

        public BibliographicRecordsController(ILogger<BibliographicRecordsController> logger,
            IKohaPatronGetHttpService kohaGetHttpService,
            IKohaBiblioGetHttpService kohaBiblioGetHttpService,
            ILibrisService librisService,
            IOptions<KohaApiSettings> kohaApiSettingsOptions)
        {
            this.logger = logger;
            this.kohaGetHttpService = kohaGetHttpService;
            this.kohaBiblioGetHttpService = kohaBiblioGetHttpService;
            this.librisService = librisService;
            this.kohaApiSettings = kohaApiSettingsOptions.Value;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string standardNumber)
        {
            if (string.IsNullOrWhiteSpace(standardNumber))
                return BadRequest();

            KohaBiblio record = await FetchFromKoha(standardNumber);

            if (record!=null)
            {
                return base.Ok(new LookupBibliographicRecordResponse
                {
                    Status = LookupStatuses.FoundInKoha,
                    Message = "Item already exists in Koha",
                    BiblioId = record.BiblioId,
                    Title = record.GetTitleAndSubtitle(),
                    Author = record.Author,
                    PublicationYear = record.PublicationYear,
                    Edition = string.Empty
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
                    Edition = librisMatch.Edition
                });
            }

            return Ok(new LookupBibliographicRecordResponse
            {
                Status = LookupStatuses.NotFound,
                Message = "No matching record found"
            });
        }

        private async Task<KohaBiblio> FetchFromKoha(string standardNumber)
        {
            var normalized = StandardNumberUtility.Normalize(standardNumber);
            var queryField = StandardNumberUtility.Detect(normalized) == "ISSN" ? "issn" : "isbn";
            var q = Uri.EscapeDataString($"{{\"{queryField}\":\"{normalized}\"}}");
            string url = $"{kohaApiSettings.BaseUrl}/biblios?q={q}";

            kohaBiblioGetHttpService.OverrideDefaultBearerToken(kohaApiSettings.AuthenticationHeaderValue);

            var records = await kohaBiblioGetHttpService.FetchAll(url);
            return records.FirstOrDefault();
        }
    }
}