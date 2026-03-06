
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Http
{
    public interface IPostHttpService<TRequestModel, TResponseModel> : ISingleMethodHttpServiceBase
    {
        Task<TResponseModel> Post(string url, TRequestModel model);
    }

    public class PostHttpService<TRequestModel, TResponseModel> : SingleMethodHttpServiceBase, IPostHttpService<TRequestModel, TResponseModel>
    {
        public PostHttpService(IHttpClient client,
            ILogger<HttpService<TRequestModel>> logger)
            : base(client, logger)
        { }

        public virtual async Task<TResponseModel> Post(string url, TRequestModel model)
        {
            try
            {
                var content = JsonConvert.SerializeObject(model);
                var response = await client.Post(new Uri(url), content);
                response.CheckStatus();
                logger.LogDebug($"Post data to {url}: {response.Content}");
                return JsonConvert.DeserializeObject<TResponseModel>(response.Content);
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
