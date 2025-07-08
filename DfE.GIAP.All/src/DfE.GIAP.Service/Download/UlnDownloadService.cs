using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using DfE.GIAP.Common.Enums;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;

namespace DfE.GIAP.Service.Download
{
    public class UlnDownloadService : IUlnDownloadService
    {
        private readonly IApiService _apiProcessorService;
        private AzureAppSettings _azureAppSettings;
        private readonly IEventLogging _eventLogging;
        private readonly IHostEnvironment _hostEnvironment;

        public UlnDownloadService(
            IApiService apiProcessorService,
            IOptions<AzureAppSettings> azureFunctionUrls,
            IEventLogging eventLogging,
            IHostEnvironment hostEnvironment)
        {
            _apiProcessorService = apiProcessorService;
            _azureAppSettings = azureFunctionUrls.Value;
            _eventLogging = eventLogging;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<ReturnFile> GetUlnCSVFile(string[] selectedPupils,
                                         string[] selectedDownloadOptions,
                                         bool confirmationGiven,
                                         AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                         ReturnRoute returnRoute)
        {
            var getCSVFile = _azureAppSettings.DownloadPupilsByULNsUrl;

            switch (returnRoute)
            {
                case ReturnRoute.UniqueLearnerNumber:
                    _eventLogging.TrackEvent(1114, $"FE ULN CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                    break;
                case ReturnRoute.NonUniqueLearnerNumber:
                    _eventLogging.TrackEvent(1115, $"FE non-ULN CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                    break;
            }

            var requestBody = new DownloadUlnRequest { ULNs = selectedPupils, DataTypes = selectedDownloadOptions, ConfirmationGiven = confirmationGiven };
            var response = await _apiProcessorService.PostAsync<DownloadUlnRequest, ReturnFile>(getCSVFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }

        public async Task<IEnumerable<DownloadUlnDataType>> CheckForNoDataAvailable(string[] selectedPupils, string[] selectedDownloadOptions, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            var getCSVFile = _azureAppSettings.DownloadPupilsByULNsUrl;

            var requestBody = new DownloadUlnRequest { ULNs = selectedPupils, DataTypes = selectedDownloadOptions, CheckOnly = true};
            var response = await _apiProcessorService.PostAsync<DownloadUlnRequest, IEnumerable<DownloadUlnDataType>>(getCSVFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

            return response;
        }
    }
}
