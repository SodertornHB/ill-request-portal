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
        Task<LibrisBiblioLookupResult> FetchBiblio(string standardNumber, string queryField);
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

        public async Task<LibrisBiblioLookupResult> FetchBiblio(string standardNumber, string queryField)
        {
            var normalized = StandardNumberUtility.Normalize(standardNumber);
            LibrisBiblioLookupResult result = await FetchAndConvert(queryField, normalized);
            if (result == null || result.IsEmpty())
            {
                normalized = StandardNumberUtility.Normalize(standardNumber, removeDashed: true);
                result = await FetchAndConvert(queryField, normalized);
                if (result == null) return null;
            }

            if (!string.IsNullOrEmpty(result.Id) && result.IsEmpty())
            {
                var fixedUrl = FixUrl(result.Id);

                var json = await jsonGetHttpService.FetchSingle(fixedUrl);

                result = Convert(json, queryField);
            }

            return result;
        }
        private async Task<LibrisBiblioLookupResult> FetchAndConvert(string queryField, string normalized)
        {
            string url = string.Empty;
            if (queryField == null) url = $"{librisApiSettings.BaseUrl}/find?q={normalized}&_limit=1";
            else url = $"{librisApiSettings.BaseUrl}/find?q={queryField}:{normalized}&_limit=1";

            var json = await jsonGetHttpService.FetchSingle(url);
            return Convert(json, queryField);
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
