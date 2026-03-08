using Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Http
{
    public interface IKohaBiblioGetHttpService : IGetHttpService<KohaBiblio>
    {
    }

    public class BiblioPatronGetHttpService : GetHttpService<KohaBiblio>, IKohaBiblioGetHttpService
    {
        public BiblioPatronGetHttpService(
            IHttpClient client,
            ILogger<HttpService<GetHttpService<KohaBiblio>>> logger)
            : base(client, logger)
        {
        }
    }
}