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
        private readonly DiscoverySettings discoverySettings;
        private readonly LibrisApiSettings librisApiSettings;

        public BibliographicRecordsController(ILogger<BibliographicRecordsController> logger,
            IKohaPatronGetHttpService kohaGetHttpService,
            IKohaBiblioGetHttpService kohaBiblioGetHttpService,
            ILibrisService librisService,
            IOptions<KohaApiSettings> kohaApiSettingsOptions,
            IOptions<DiscoverySettings> discoverySettingsOptions,
            IOptions<LibrisApiSettings> librisApiSettingsOptions)
        {
            this.logger = logger;
            this.kohaGetHttpService = kohaGetHttpService;
            this.kohaBiblioGetHttpService = kohaBiblioGetHttpService;
            this.librisService = librisService;
            this.kohaApiSettings = kohaApiSettingsOptions.Value;
            this.discoverySettings = discoverySettingsOptions.Value;
            this.librisApiSettings = librisApiSettingsOptions.Value;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string standardNumber, [FromQuery] string queryField)
        {
            if (string.IsNullOrWhiteSpace(standardNumber))
                return BadRequest();

            KohaGetBiblioHttpModel record = await FetchFromKoha(standardNumber, queryField);

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
                    Edition = string.Empty,
                    Volume = record.GetVolume(),
                    KohaUrl = BuildKohaUrl(record.BiblioId)
                });
            }

            LibrisBiblioLookupResult librisMatch = await librisService.FetchBiblio(standardNumber, queryField);

            if (librisMatch != null && !librisMatch.IsEmpty())
            {
                return Ok(new LookupBibliographicRecordResponse
                {
                    Status = LookupStatuses.FoundInLibris,
                    Message = "Bibliographic data found in Libris",
                    Title = librisMatch.Title,
                    Author = librisMatch.Author,
                    PublicationYear = librisMatch.PublicationYear,
                    Edition = librisMatch.Edition,
                    Volume = librisMatch.Volume,
                    LibrisUrl = BuildLibrisUrl(librisMatch.Id)
                });
            }

            return Ok(new LookupBibliographicRecordResponse
            {
                Status = LookupStatuses.NotFound,
                Message = "No matching record found"
            });
        }

        private string BuildKohaUrl(int biblioId)
        {
            if (biblioId == 0 || string.IsNullOrEmpty(discoverySettings.RecordUrlTemplate))
                return string.Empty;
            return discoverySettings.RecordUrlTemplate.Replace("{biblioId}", biblioId.ToString());
        }

        private string BuildLibrisUrl(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            var clean = id.Split('#')[0];
            if (!clean.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                clean = $"{librisApiSettings.BaseUrl}/{clean.TrimStart('/')}";
            return clean;
        }

        private async Task<KohaGetBiblioHttpModel> FetchFromKoha(string standardNumber, string queryField)
        {
            var normalized = StandardNumberUtility.Normalize(standardNumber);
            
            var q = Uri.EscapeDataString($"{{\"{queryField}\":\"{normalized}\"}}");
            string url = $"{kohaApiSettings.BaseUrl}/biblios?q={q}";

            kohaBiblioGetHttpService.OverrideDefaultBearerToken(kohaApiSettings.AuthenticationHeaderValue);

            var records = await kohaBiblioGetHttpService.FetchAll(url);
            return records.FirstOrDefault();
        }
    }
}