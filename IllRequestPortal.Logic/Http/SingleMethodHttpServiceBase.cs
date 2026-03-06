
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace IllRequestPortal.Logic.Http
{
    public interface ISingleMethodHttpServiceBase
    {
        void OverrideDefaultBearerToken(string token);
    }

    public class SingleMethodHttpServiceBase : ISingleMethodHttpServiceBase
    {
        protected IHttpClient client;
        protected readonly ILogger logger;

        public SingleMethodHttpServiceBase(IHttpClient client,
            ILogger logger)
        {
            this.client = client;
            this.logger = logger;
        }
       
        
        public void OverrideDefaultBearerToken(string token)
        {
            client.SetBearerToken(token);
        }

        protected static Uri CombineUrls(string url, string id)
        {
            if (!url.EndsWith("/")) url += "/";
            return new Uri(url + id);
        }

        protected void LogHttpException(HttpRequestException e)
        {
            logger.LogError(e, $"{e.Message} Status code: {e.StatusCode}");
        }

    }
}
