using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Http
{
    public interface IJsonGetHttpService : IGetHttpService<string >
    {
    }

    public class JsonGetHttpService : GetHttpService<string >, IJsonGetHttpService
    {
        public JsonGetHttpService(
            IHttpClient client,
            ILogger<HttpService<GetHttpService<string >>> logger)
            : base(client, logger)
        {
        }
        public override async Task<string> FetchSingle(string url)
        {
            return await GenericGet(url, x => x);
        }
    }
}