using IllRequestPortal.Logic.Http;
using IllRequestPortal.Logic.Settings;
using Logic.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using static Logic.Util.StandardNumberUtil;

namespace IllRequestPortal.Logic.Services
{
    public partial interface ILibrisService
    {
        Task<LibrisBiblioLookupResult> FetchBiblio(string standardNumber);

    }

    public partial class LibrisService : ILibrisService
    {
        private readonly ILogger<LibrisService> logger;
        private readonly IJsonGetHttpService jsonGetHttpService;
        private readonly LibrisApiSettings librisApiSettings;

        public LibrisService(ILogger<LibrisService> logger,
           IJsonGetHttpService jsonGetHttpService,
           IOptions<LibrisApiSettings> apiSettingsOption)
        {
            this.logger = logger;
            this.jsonGetHttpService = jsonGetHttpService;
            this.librisApiSettings = apiSettingsOption.Value;
        }

        public async Task<LibrisBiblioLookupResult> FetchBiblio(string standardNumber)
        {
            var normalized = StandardNumberUtility.Normalize(standardNumber);
            string type = StandardNumberUtility.GetStandardNumberType(standardNumber);
            string url = string.Empty;
            if (type == null) url = $"{librisApiSettings.BaseUrl}/find?q={normalized}&_limit=1";
            else url = $"{librisApiSettings.BaseUrl}/find?q={type}:{normalized}&_limit=1";

            var json = await jsonGetHttpService.FetchSingle(url);
            var result = Convert(json, type);
            if (result == null) return null;

            if (string.IsNullOrEmpty(result.Title) && string.IsNullOrEmpty(result.Author))
            {
                var fixedUrl = FixUrl(result.Id);

                json = await jsonGetHttpService.FetchSingle(fixedUrl);

                result = Convert(json, type);
            }

            return result;
        }

        private LibrisBiblioLookupResult Convert(string json, string type)
        {
            LibrisBiblioLookupResult result = null;
            if (type == null || type == "isbn")
            {
                result = JsonToLibrisBiblioLookupResult.ConvertIsbn(json, logger);
                if (string.IsNullOrEmpty(result?.Id))
                {
                    result = JsonToLibrisBiblioLookupResult.ConvertIsbnFromGraph(json, logger);
                    if (string.IsNullOrEmpty(result.Id))
                    {
                        return null;
                    }
                }
            }
            else
            {
                result = JsonToLibrisBiblioLookupResult.ConvertIssn(json, logger);
            }
            return result;
        }

        private string FixUrl(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return id;

            var clean = id.Split('#')[0];

            if (!clean.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                clean = $"{librisApiSettings.BaseUrl}/{clean}";

            if (!clean.EndsWith("/data.jsonld", StringComparison.OrdinalIgnoreCase))
                clean = clean.TrimEnd('/') + "/data.jsonld";

            return clean;
        }
    }
}
