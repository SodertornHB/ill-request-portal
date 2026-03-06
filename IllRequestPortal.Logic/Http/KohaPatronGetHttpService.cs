using IllRequestPortal.Logic.Http;
using Logic.Model;
using Microsoft.Extensions.Logging;

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