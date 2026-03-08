using Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Http
{
    public interface IKohaPatronGetHttpService : IGetHttpService<KohaPatronEntity>
    {
    }

    public class KohaPatronGetHttpService : GetHttpService<KohaPatronEntity>, IKohaPatronGetHttpService
    {
        public KohaPatronGetHttpService(
            IHttpClient client,
            ILogger<HttpService<GetHttpService<KohaPatronEntity>>> logger)
            : base(client, logger)
        {
        }
    }
}