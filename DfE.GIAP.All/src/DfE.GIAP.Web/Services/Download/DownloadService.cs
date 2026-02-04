using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using Microsoft.Extensions.Options;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Web.Services.ApiProcessor;

namespace DfE.GIAP.Web.Services.Download;

public class DownloadService : IDownloadService
{
    private readonly AzureAppSettings _azureAppSettings;
    private readonly IApiService _apiProcessorService;
    private readonly IEventLogger _eventLogger;

    public DownloadService(
        IOptions<AzureAppSettings> azureFunctionUrls,
        IApiService apiProcessorService,
        IEventLogger eventLogger)
    {
        _azureAppSettings = azureFunctionUrls.Value;
        _apiProcessorService = apiProcessorService;
        _eventLogger = eventLogger;
    }


    public async Task<ReturnFile> GetCSVFile(string[] selectedPupils,
                                             string[] sortOrder,
                                             string[] selectedDownloadOptions,
                                             bool confirmationGiven,
                                             AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                             ReturnRoute returnRoute)
    {
        var getCSVFile = _azureAppSettings.DownloadPupilsByUPNsCSVUrl;

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, DataTypes = selectedDownloadOptions, ConfirmationGiven = confirmationGiven, FileType = "csv" };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, ReturnFile>(getCSVFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        string loggingBatchId = Guid.NewGuid().ToString();
        foreach (string dataset in requestBody.DataTypes)
        {
            // TODO: Temp quick solution
            if (Enum.TryParse(dataset, out Dataset datasetEnum))
            {
                _eventLogger.LogDownload(
                    Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                    DownloadFileFormat.CSV,
                    DownloadEventType.NPD,
                    loggingBatchId,
                    datasetEnum);
            }
        }

        return response;
    }

    public async Task<ReturnFile> GetFECSVFile(string[] selectedPupils,
                                     string[] selectedDownloadOptions,
                                     bool confirmationGiven,
                                     AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                     ReturnRoute returnRoute)
    {
        var getCSVFile = _azureAppSettings.DownloadPupilsByULNsUrl;

        var requestBody = new DownloadUlnRequest { ULNs = selectedPupils, DataTypes = selectedDownloadOptions, ConfirmationGiven = confirmationGiven };
        var response = await _apiProcessorService.PostAsync<DownloadUlnRequest, ReturnFile>(getCSVFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        string loggingBatchId = Guid.NewGuid().ToString();
        foreach (string dataset in requestBody.DataTypes)
        {
            // TODO: Temp quick solution
            if (Enum.TryParse(dataset, out Dataset datasetEnum))
            {
                _eventLogger.LogDownload(
                    Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                    DownloadFileFormat.CSV,
                    DownloadEventType.FE,
                    loggingBatchId,
                    datasetEnum);
            }
        }

        return response;
    }

    public async Task<ReturnFile> GetTABFile(string[] selectedPupils,
                                             string[] sortOrder,
                                             string[] selectedDownloadOptions,
                                             bool confirmationGiven,
                                             AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                             ReturnRoute returnRoute)
    {
        var getTABFile = _azureAppSettings.DownloadPupilsByUPNsCSVUrl;

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, DataTypes = selectedDownloadOptions, ConfirmationGiven = confirmationGiven, FileType = "tab" };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, ReturnFile>(getTABFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        string loggingBatchId = Guid.NewGuid().ToString();
        foreach (string dataset in requestBody.DataTypes)
        {
            // TODO: Temp quick solution
            if (Enum.TryParse(dataset, out Dataset datasetEnum))
            {
                _eventLogger.LogDownload(
                    Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                    DownloadFileFormat.TAB,
                    DownloadEventType.NPD,
                    loggingBatchId,
                    datasetEnum);
            }
        }

        return response;
    }

    public async Task<IEnumerable<CheckDownloadDataType>> CheckForNoDataAvailable(string[] selectedPupils, string[] sortOrder, string[] selectedDownloadOptions, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
    {
        var getCSVFile = _azureAppSettings.DownloadPupilsByUPNsCSVUrl;

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, DataTypes = selectedDownloadOptions, FileType = "csv", CheckOnly = true };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, IEnumerable<CheckDownloadDataType>>(getCSVFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        return response;
    }

    public async Task<ReturnFile> GetPupilPremiumCSVFile(string[] selectedPupils,
                                                         string[] sortOrder,
                                                         bool confirmationGiven,
                                                         AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                                         ReturnRoute returnRoute,
                                                         UserOrganisation userOrganisation = null)
    {
        var getFile = _azureAppSettings.DownloadPupilPremiumByUPNFforCSVUrl;

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, UserOrganisation = userOrganisation, ConfirmationGiven = confirmationGiven };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, ReturnFile>(getFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        _eventLogger.LogDownload(
            Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
            DownloadFileFormat.CSV,
            DownloadEventType.PP);

        return response;
    }
}
