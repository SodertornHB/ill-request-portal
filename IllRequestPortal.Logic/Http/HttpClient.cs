
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Http
{
    public interface IHttpClient
    {
        Task<HttpResponse> Get(Uri url);
        Task<HttpResponse> Post(Uri url, string content);
        Task<HttpResponse> Put(Uri url, string content);
        Task<HttpResponse> Delete(Uri url);
        void SetBearerToken(string token);
    }

    public class HttpClient : IHttpClient
    {
        protected readonly IHttpClientFactory clientFactory;
        protected readonly ILogger<HttpClient> logger;
        protected AuthenticationSettings settings;

        public HttpClient(IHttpClientFactory clientFactory,
            ILogger<HttpClient> logger,
            IOptions<AuthenticationSettings> settings)
        {
            this.clientFactory = clientFactory;
            this.logger = logger;
            this.settings = settings.Value;
        }

        public async Task<HttpResponse> Get(Uri url)
        {
            return await SendRequest(HttpMethod.Get, url);
        }

        public async Task<HttpResponse> Post(Uri url, string content)
        {
            return await SendRequest(HttpMethod.Post, url, content);
        }

        public async Task<HttpResponse> Put(Uri url, string content)
        {
            return await SendRequest(HttpMethod.Put, url, content);
        }

        public async Task<HttpResponse> Delete(Uri url)
        {
            return await SendRequest(HttpMethod.Delete, url);
        }

        public void SetBearerToken(string token)
        {
            settings.BearerToken = token;
        }

        protected virtual async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            var client = clientFactory.CreateClient();
            client.SetBearerTokenIfExists(settings.BearerToken);
            return await client.SendAsync(request);        
        }

        #region private

        private async Task<HttpResponse> SendRequest(HttpMethod method, Uri url, string requestContent = "")
        {
            logger.LogDebug($"Sending {method} request to {url}. Content: {requestContent}");
            var request = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(requestContent)) request.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");
            var response = await Send(request);
            string responseContent = await GetContent(response);
            logger.LogDebug($"Response code {response.StatusCode}. Content: {responseContent}");
            return new HttpResponse(response.StatusCode, response.IsSuccessStatusCode, responseContent, response.Headers);
        }

        private static async Task<string> GetContent(HttpResponseMessage response)
        {
            try
            {
                var contenttype = response.Content.Headers.FirstOrDefault(h => h.Key.Equals("Content-Type"));

                var rawencoding = contenttype.Value.First();

                if (rawencoding.Contains("utf8") || rawencoding.Contains("UTF-8"))
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    return Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        #endregion
    }

    #region models 

    public class HttpResponse
    {
        public HttpResponse(HttpStatusCode statusCode, bool isSuccess) : this(statusCode,  isSuccess, string.Empty, null)
        { }

        public HttpResponse(HttpStatusCode statusCode, bool isSuccess, string content, HttpResponseHeaders headers)
        {
            StatusCode = statusCode;
            IsSuccess = isSuccess;
            Content = content;
            Headers = headers;
        }

        public string Content { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public bool IsSuccess { get; set; }

        public HttpResponseHeaders Headers { get; private set; }

        /// <throws>HttpRequestException</throws>
        public void CheckStatus()
        {
            if (!IsSuccess)
            {
                throw new HttpRequestException($"Request failed ({StatusCode}). {Content}",null, StatusCode);
            }
        }
    }

    #endregion

    #region Extensions 
    
    public static class HttpClientExtensions
    {
        public static void SetBearerTokenIfExists(this System.Net.Http.HttpClient client, string bearerToken)
        {
            if (!string.IsNullOrEmpty(bearerToken)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }

    #endregion
}
