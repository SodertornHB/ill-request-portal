
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Logic.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Http
{
   public interface IGetHttpService<TResonseModel> : ISingleMethodHttpServiceBase
    {
        Task<TResonseModel> FetchSingle(string url);
        Task<IEnumerable<TResonseModel>> FetchAll(string url);
    }

    public class GetHttpService<TResonseModel> : SingleMethodHttpServiceBase, IGetHttpService<TResonseModel>
    {
        public GetHttpService(IHttpClient client,
            ILogger<HttpService<GetHttpService<TResonseModel>>> logger)
            : base(client, logger)
        { }
        public virtual async Task<IEnumerable<TResonseModel>> FetchAll(string url) => await GenericGet(url, JsonConvert.DeserializeObject<IEnumerable<TResonseModel>>);

        public virtual async Task<TResonseModel> FetchSingle(string url) => await GenericGet(url, JsonConvert.DeserializeObject<TResonseModel>);

        protected async Task<T> GenericGet<T>(string url, Func<string, T> mapper)
        {
            try
            {
                var response = await client.Get(new Uri(url));
                response.CheckStatus();
                logger.LogDebug($"Get data from {url}: {response.Content}");
                return mapper(response.Content);
            }
            catch (HttpRequestException e)
            {
                LogHttpException(e);
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                throw;
            }
        }   
    }
}
