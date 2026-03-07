using Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Http
{
    public interface IKohaGetHttpService : IGetHttpService<KohaPatronEntity>
    {
    }

    public class KohaGetHttpService : GetHttpService<KohaPatronEntity>, IKohaGetHttpService
    {
        public KohaGetHttpService(
            IHttpClient client,
            ILogger<HttpService<GetHttpService<KohaPatronEntity>>> logger)
            : base(client, logger)
        {
        }
    }
}