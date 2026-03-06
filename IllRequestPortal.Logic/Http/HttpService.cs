
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Http
{
    public interface IHttpService<T>
    {
        Task<T> Get(string url, string id);
        Task<IEnumerable<T>> Get(string url);
        Task<T> Post(string url, T model);
        Task Put(string url, string id, T model);
        Task Delete(string url, string id);
        void OverrideDefaultBearerToken(string token);
    }
    public class HttpService<T> : IHttpService<T>
    {
        protected IHttpClient client;
        protected readonly ILogger logger;

        public HttpService(IHttpClient client,
            ILogger<HttpService<T>> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public virtual async Task<T> Get(string url, string id)
        {
            try
            {
                var uri = CombineUrls(url, id);
                var response = await client.Get(uri);
                response.CheckStatus();
                logger.LogDebug($"Get data from {uri}: {response.Content}");
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e, $"{e.Message} Status code: {e.StatusCode}");
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> Get(string url)
        {
            try
            {
                var response = await client.Get(new Uri(url));
                response.CheckStatus();
                logger.LogDebug($"Get data from {url}: {response.Content}");
                return JsonConvert.DeserializeObject<IEnumerable<T>>(response.Content);
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

        public virtual async Task<T> Post(string url, T model)
        {
            try
            {
                var content = JsonConvert.SerializeObject(model);
                var response = await client.Post(new Uri(url), content);
                response.CheckStatus();
                logger.LogDebug($"Post data to {url}: {response.Content}");
                return JsonConvert.DeserializeObject<T>(response.Content);
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

        public virtual async Task Put(string url, string id, T model)
        {
            try
            {
                var content = JsonConvert.SerializeObject(model);
                var uri = CombineUrls(url, id);
                var response = await client.Put(uri, content);
                response.CheckStatus();
                logger.LogDebug($"Put data from {uri}: {response.Content}");
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

        public virtual async Task Delete(string url, string id)
        {
            try
            {
                var uri = CombineUrls(url, id);
                var response = await client.Delete(uri);
                response.CheckStatus();
                logger.LogDebug($"Delete data from {uri}");
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

        public void OverrideDefaultBearerToken(string token)
        {
            client.SetBearerToken(token);
        }

        #region protected

        protected static Uri CombineUrls(string url, string id)
        {
            if (!url.EndsWith("/")) url += "/";
            return new Uri(url + id);
        }

        protected void LogHttpException(HttpRequestException e)
        {
            logger.LogError(e, $"{e.Message} Status code: {e.StatusCode}");
        }

        #endregion
    }
}
