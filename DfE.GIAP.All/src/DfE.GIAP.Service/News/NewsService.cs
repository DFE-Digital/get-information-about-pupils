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

    public async Task<Article> UpdateNewsProperty(UpdateNewsDocumentRequestBody requestBody)
    {
        var updateNewsDocument = _azureAppSettings.UpdateNewsPropertyUrl;
        var response = await _apiProcessorService.PostAsync<UpdateNewsDocumentRequestBody, Article>(updateNewsDocument.ConvertToUri(), requestBody, null).ConfigureAwait(false);

        return response;
    }
}
