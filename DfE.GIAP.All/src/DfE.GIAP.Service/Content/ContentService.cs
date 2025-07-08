using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.BlobStorage;
using DfE.GIAP.Service.Helpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Content
{
    public class ContentService : IContentService
    {
        private readonly IApiService _apiService;
        private AzureAppSettings _azureAppSettings;

        public ContentService(IApiService apiService,
                              IOptions<AzureAppSettings> azureAppSettings)
        {
            _apiService = apiService;
            _azureAppSettings = azureAppSettings.Value;
        }

        public async Task<CommonResponseBody> GetContent(DocumentType documentType)
        {
            var commonResponseBody = await GetDataSetFromApi(documentType).ConfigureAwait(false);
            return commonResponseBody;
        }

        private async Task<CommonResponseBody> GetDataSetFromApi(DocumentType type)
        {
            var query = $"{_azureAppSettings.GetContentByIDUrl}&ID={type}";
            var response = await _apiService.GetAsync<CommonResponseBody>(query.ConvertToUri()).ConfigureAwait(false);

            return response;
        }

        public async Task<CommonResponseBody> SetDocumentToPublished(CommonRequestBody requestBody, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            var query = _azureAppSettings.UpdateNewsPropertyUrl;
            var response = await _apiService.PostAsync<CommonRequestBody, CommonResponseBody>(query.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }

        public async Task<CommonResponseBody> AddOrUpdateDocument(CommonRequestBody requestBody, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            var query = _azureAppSettings.UpdateNewsDocumentUrl;
            var response = await _apiService.PostAsync<CommonRequestBody, CommonResponseBody>(query.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }
    }
}
