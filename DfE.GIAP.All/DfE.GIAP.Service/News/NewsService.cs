using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Models.News;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Service.News;

public class NewsService : INewsService
{
    private readonly IApiService _apiProcessorService;
    private readonly AzureAppSettings _azureAppSettings;

    public NewsService(IApiService apiProcessorService, IOptions<AzureAppSettings> azureAppSettings)
    {
        _apiProcessorService = apiProcessorService;
        _azureAppSettings = azureAppSettings.Value;
    }

    public async Task<List<Article>> GetNewsArticles(RequestBody requestBody)
    {
        var queryNewsArticles = _azureAppSettings.QueryNewsArticlesUrl;
        var response = await _apiProcessorService.PostAsync<RequestBody, IEnumerable<Article>>(queryNewsArticles.ConvertToUri(), requestBody, null).ConfigureAwait(false);

        if (requestBody.ARCHIVED)
            return response.OrderByDescending(x => x.ModifiedDate).ToList();

        return response.OrderByDescending(x => x.Pinned).ThenByDescending(x => x.ModifiedDate).ToList();
    }

    public async Task<Article> UpdateNewsArticle(UpdateNewsRequestBody requestBody)
    {
        var updateNewsArticle = _azureAppSettings.UpdateNewsPropertyUrl;
        var response = await _apiProcessorService.PostAsync<UpdateNewsRequestBody, Article>(updateNewsArticle.ConvertToUri(), requestBody, null).ConfigureAwait(false);

        return response;
    }

    public async Task<Article> UpdateNewsDocument(UpdateNewsDocumentRequestBody requestBody)
    {
        var updateNewsDocument = _azureAppSettings.UpdateNewsDocumentUrl;
        var response = await _apiProcessorService.PostAsync<UpdateNewsDocumentRequestBody, Article>(updateNewsDocument.ConvertToUri(), requestBody, null).ConfigureAwait(false);

        return response;
    }

    public async Task<HttpStatusCode> DeleteNewsArticle(string newsId)
    {
        var deleteNewsArticle = _azureAppSettings.DeleteNewsArticleUrl;
        var builder = new UriBuilder(deleteNewsArticle);
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["ID"] = newsId;
        builder.Query = query.ToString();

        var response = await _apiProcessorService.DeleteAsync(builder.Uri).ConfigureAwait(false);

        return response;
    }

    public async Task<Article> UpdateNewsProperty(UpdateNewsDocumentRequestBody requestBody)
    {
        var updateNewsDocument = _azureAppSettings.UpdateNewsPropertyUrl;
        var response = await _apiProcessorService.PostAsync<UpdateNewsDocumentRequestBody, Article>(updateNewsDocument.ConvertToUri(), requestBody, null).ConfigureAwait(false);

        return response;
    }
}
