using Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Http
{
    public interface IKohaBiblioGetHttpService : IGetHttpService<KohaGetBiblioHttpModel>
    {
    }

    public class BiblioPatronGetHttpService : GetHttpService<KohaGetBiblioHttpModel>, IKohaBiblioGetHttpService
    {
        public BiblioPatronGetHttpService(
            IHttpClient client,
            ILogger<HttpService<GetHttpService<KohaGetBiblioHttpModel>>> logger)
            : base(client, logger)
        {
        }
    }
}