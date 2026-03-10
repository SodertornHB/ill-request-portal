using IllRequestPortal.Logic.Http;
using IllRequestPortal.Logic.Settings;
using Logic.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Logic.Util.StandardNumberUtil;

namespace IllRequestPortal.Logic.Services
{
    public partial interface ILibrisService 
    {
        Task<LibrisBiblioLookupResult> FetchBiblio(string standardNumber);

    }

    public partial class LibrisService :  ILibrisService
    {
        private readonly IJsonGetHttpService jsonGetHttpService;
        private readonly LibrisApiSettings librisApiSettings;

        public LibrisService(ILogger<LibrisService> logger,
           IJsonGetHttpService jsonGetHttpService,
           IOptions<LibrisApiSettings> apiSettingsOption)
        {
            this.jsonGetHttpService = jsonGetHttpService;
            this.librisApiSettings = apiSettingsOption.Value;
        }

        public async Task<LibrisBiblioLookupResult> FetchBiblio(string standardNumber)
        {
            var normalized = StandardNumberUtility.Normalize(standardNumber);
            var url = $"{librisApiSettings.BaseUrl}/find?q={normalized}&_limit=1";

            var json = await jsonGetHttpService.FetchSingle(url);
            var result = Convert(json);
            if (result == null) return null;

            if (string.IsNullOrEmpty(result.Title) && string.IsNullOrEmpty(result.Author))
            {
                var fixedUrl = FixUrl(result.Id);

                json = await jsonGetHttpService.FetchSingle(fixedUrl);

                result = Convert(json);
            }

            return result;
        }

        private static LibrisBiblioLookupResult Convert(string json)
        {
            var result = JsonToLibrisBiblioLookupResult.Convert(json);
            if (string.IsNullOrEmpty(result.Id))
            {
                result = JsonToLibrisBiblioLookupResult.ConvertFromGraph(json);
                if (string.IsNullOrEmpty(result.Id))
                {
                    return null;
                }
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

    public static class JsonToLibrisBiblioLookupResult
    {
        public static LibrisBiblioLookupResult Convert(string json)
        {
            var result = new LibrisBiblioLookupResult();

            if (string.IsNullOrWhiteSpace(json))
                return result;

            var root = JObject.Parse(json);

            var items = root["items"] as JArray;
            if (items == null || !items.Any())
                return result;

            var instance = items
                .OfType<JObject>()
                .FirstOrDefault();

            if (instance == null)
                return result;

            result.Id = (string)instance["@id"] ?? "";

            var work = instance["instanceOf"] as JObject;

            var titleObject =
                (work?["hasTitle"] as JArray)?.OfType<JObject>().FirstOrDefault()
                ?? (instance["hasTitle"] as JArray)?.OfType<JObject>().FirstOrDefault();

            var mainTitle = (string)titleObject?["mainTitle"] ?? "";
            var subtitle = (string)titleObject?["subtitle"] ?? "";

            result.Title = string.IsNullOrWhiteSpace(subtitle)
                ? mainTitle
                : $"{mainTitle}: {subtitle}";

            var contributionArray =
                (work?["contribution"] as JArray)
                ?? (instance["contribution"] as JArray);

            var firstContribution = contributionArray?
                .OfType<JObject>()
                .FirstOrDefault();

            var agent = firstContribution?["agent"] as JObject;

            var givenName = (string)agent?["givenName"] ?? "";
            var familyName = (string)agent?["familyName"] ?? "";
            var agentLabel = ((agent?["label"] as JArray)?.FirstOrDefault()?.ToString()) ?? "";

            var combinedName = $"{givenName} {familyName}".Trim();

            result.Author = !string.IsNullOrWhiteSpace(combinedName)
                ? combinedName
                : agentLabel;

            var publicationArray = instance["publication"] as JArray;
            var firstPublication = publicationArray?
                .OfType<JObject>()
                .FirstOrDefault();

            result.PublicationYear = (string)firstPublication?["year"] ?? "";
            result.Edition = (string)instance["editionStatement"] ?? "";

            var identifiedBy = instance["identifiedBy"] as JArray;
            if (identifiedBy != null)
            {
                var identifier = identifiedBy
                    .OfType<JObject>()
                    .FirstOrDefault(x =>
                        string.Equals((string)x["@type"], "ISBN", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals((string)x["@type"], "ISSN", StringComparison.OrdinalIgnoreCase));

                result.IsbnIssn = (string)identifier?["value"] ?? "";
            }

            var materialType = (string)work?["@type"] ?? (string)instance["@type"] ?? "";

            result.MaterialType = materialType switch
            {
                "Text" => IllRequestConstants.MaterialTypes.Book,
                "Instance" => IllRequestConstants.MaterialTypes.Book,
                _ => materialType
            };

            var holdings = instance["@reverse"]?["itemOf"] as JArray;

            if (holdings != null)
            {
                result.ExistsInLocalCatalog = holdings
                    .OfType<JObject>()
                    .Any(h =>
                        ((string)h["heldBy"]?["@id"])?
                        .EndsWith("/library/D", StringComparison.OrdinalIgnoreCase) == true);
            }

            return result;
        }
        public static LibrisBiblioLookupResult ConvertFromGraph(string json)
        {
            var result = new LibrisBiblioLookupResult();

            if (string.IsNullOrWhiteSpace(json))
                return result;

            var root = JObject.Parse(json);

            var entity = FindMainBibliographicEntity(root);

            if (entity == null)
                return result;

            result.Id = (string)entity["@id"] ?? "";

            var titleObject = (entity["hasTitle"] as JArray)?
                .OfType<JObject>()
                .FirstOrDefault(x => string.Equals((string)x["@type"], "Title", StringComparison.OrdinalIgnoreCase))
                ?? (entity["hasTitle"] as JArray)?
                    .OfType<JObject>()
                    .FirstOrDefault();

            var mainTitle = (string)titleObject?["mainTitle"] ?? "";
            var subtitle = (string)titleObject?["subtitle"] ?? "";

            result.Title = string.IsNullOrWhiteSpace(subtitle)
                ? mainTitle
                : $"{mainTitle}: {subtitle}";

            var work = entity["instanceOf"] as JObject;

            var contribution = (work?["contribution"] as JArray)?
                .OfType<JObject>()
                .FirstOrDefault();

            var agent = contribution?["agent"] as JObject;

            var givenName = (string)agent?["givenName"] ?? "";
            var familyName = (string)agent?["familyName"] ?? "";
            var label = ((agent?["label"] as JArray)?.FirstOrDefault()?.ToString()) ?? "";

            var author = $"{givenName} {familyName}".Trim();
            result.Author = !string.IsNullOrWhiteSpace(author) ? author : label;

            var publication = (entity["publication"] as JArray)?
                .OfType<JObject>()
                .FirstOrDefault();

            result.PublicationYear =
                (string)publication?["year"]
                ?? (string)publication?["startYear"]
                ?? "";

            result.Edition = (string)entity["editionStatement"] ?? "";

            var identifiedBy = entity["identifiedBy"] as JArray;
            if (identifiedBy != null)
            {
                var identifier = identifiedBy
                    .OfType<JObject>()
                    .FirstOrDefault(x =>
                        string.Equals((string)x["@type"], "ISBN", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals((string)x["@type"], "ISSN", StringComparison.OrdinalIgnoreCase));

                result.IsbnIssn = (string)identifier?["value"] ?? "";
            }

            var materialType = (string)work?["@type"] ?? (string)entity["@type"] ?? "";

            result.MaterialType = materialType switch
            {
                "Text" => IllRequestConstants.MaterialTypes.Book,
                "Monograph" => IllRequestConstants.MaterialTypes.Book,
                "Serial" => IllRequestConstants.MaterialTypes.Journal,
                _ => materialType
            };

            return result;
        }

        private static JObject? FindMainBibliographicEntity(JObject root)
        {
            var rootGraph = root["@graph"] as JArray;
            if (rootGraph == null || !rootGraph.Any())
                return null;

            var directEntity = rootGraph
                .OfType<JObject>()
                .FirstOrDefault(IsBibliographicEntity);

            if (directEntity != null)
                return directEntity;

            foreach (var node in rootGraph.OfType<JObject>())
            {
                var nestedGraph = node["@graph"] as JArray;
                if (nestedGraph == null || !nestedGraph.Any())
                    continue;

                var nestedEntity = nestedGraph
                    .OfType<JObject>()
                    .FirstOrDefault(IsBibliographicEntity);

                if (nestedEntity != null)
                    return nestedEntity;
            }

            return null;
        }

        private static bool IsBibliographicEntity(JObject node)
        {
            if (node["hasTitle"] == null)
                return false;

            if (node["identifiedBy"] != null)
                return true;

            if (node["instanceOf"] != null)
                return true;

            var type = (string)node["@type"] ?? "";

            return type == "PhysicalResource" || type == "Instance";
        }
    }
}
