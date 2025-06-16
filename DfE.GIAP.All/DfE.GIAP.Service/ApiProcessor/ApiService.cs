using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Helpers;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Service.ApiProcessor
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            HttpClient httpClient,
            ILogger<ApiService> logger
            )
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TApiModel> GetAsync<TApiModel>(Uri url)
            where TApiModel : class
        {

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            try
            {
                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                var responseString = response.Content != null ?
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false) : String.Empty;

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }

                return JsonConvert.DeserializeObject<TApiModel>(responseString);
            }
            catch (Exception ex)
            {
                var sanitizedUrl = url.AbsoluteUri.Replace("\n", "").Replace("\r", "");
                _logger.LogError(ex, $"Exception getting data '{ex.Message}' from {sanitizedUrl}.");
            }

            return default;
        }

        public async Task<List<TApiModel>> GetToListAsync<TApiModel>(Uri url)
            where TApiModel : class
        {
            return await GetAsync<List<TApiModel>>(url);
        }

        public async Task<HttpStatusCode> PostAsync<TModel>(Uri url, TModel model)
            where TModel : class
        {
            HttpResponseMessage response = null;
            try
            {
                using var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = url,
                    Content = model != null ? new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, MediaTypeNames.Application.Json) : null
                };

                response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception getting data '{ex.Message}' from {url.AbsoluteUri}.");
            }

            return response?.StatusCode ?? HttpStatusCode.BadRequest;
        }

        public async Task<TResponseModel> PostAsync<TRequestModel, TResponseModel>(Uri url, TRequestModel model, AzureFunctionHeaderDetails headerDetails)
            where TRequestModel : class
            where TResponseModel : class
        {
            HttpResponseMessage response = null;
            try
            {
                using var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = url,
                    Content = model != null ? new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, MediaTypeNames.Application.Json) : null
                };

                if (headerDetails != null)
                {
                    _httpClient.ConfigureHeaders(headerDetails);
                }

                response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseModel = JsonConvert.DeserializeObject<TResponseModel>(responseContent);
                return responseModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception getting data '{ex.Message}' from {url.AbsoluteUri}.");
            }

            return default;
        }

        public async Task<HttpStatusCode> DeleteAsync(Uri url)
        {
            HttpResponseMessage response = null;
            try
            {
                using var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = url,
                };

                response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception getting data '{ex.Message}' from {url.AbsoluteUri}.");
            }

            return response?.StatusCode ?? HttpStatusCode.BadRequest;
        }
    }
}
