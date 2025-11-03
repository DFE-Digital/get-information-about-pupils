using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;
using Microsoft.Extensions.Hosting;

namespace DfE.GIAP.Service.Download;

public class DownloadService : IDownloadService
{
    private AzureAppSettings _azureAppSettings;
    private readonly IApiService _apiProcessorService;
    private readonly IEventLogging _eventLogging;
    private readonly IHostEnvironment _hostEnvironment;

    public DownloadService(
        IOptions<AzureAppSettings> azureFunctionUrls,
        IApiService apiProcessorService,
        IEventLogging eventLogging,
        IHostEnvironment hostEnvironment)
    {
        _azureAppSettings = azureFunctionUrls.Value;
        _apiProcessorService = apiProcessorService;
        _eventLogging = eventLogging;
        _hostEnvironment = hostEnvironment;
    }


    public async Task<ReturnFile> GetCSVFile(string[] selectedPupils,
                                             string[] sortOrder,
                                             string[] selectedDownloadOptions,
                                             bool confirmationGiven,
                                             AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                             ReturnRoute returnRoute)
    {
        var getCSVFile = _azureAppSettings.DownloadPupilsByUPNsCSVUrl;
        switch (returnRoute)
        {
            case ReturnRoute.NationalPupilDatabase:
                _eventLogging.TrackEvent(1107, $"NPD UPN CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.NonNationalPupilDatabase:
                _eventLogging.TrackEvent(1110, $"NPD non-UPN CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.MyPupilList:
                _eventLogging.TrackEvent(1117, $"MPL CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;
        }

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, DataTypes = selectedDownloadOptions, ConfirmationGiven = confirmationGiven, FileType = "csv" };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, ReturnFile>(getCSVFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        return response;
    }

    public async Task<ReturnFile> GetFECSVFile(string[] selectedPupils,
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

    public async Task<ReturnFile> GetTABFile(string[] selectedPupils,
                                             string[] sortOrder,
                                             string[] selectedDownloadOptions,
                                             bool confirmationGiven,
                                             AzureFunctionHeaderDetails azureFunctionHeaderDetails,
                                             ReturnRoute returnRoute)
    {
        var getTABFile = _azureAppSettings.DownloadPupilsByUPNsCSVUrl;

        switch (returnRoute)
        {
            case ReturnRoute.NationalPupilDatabase:
                _eventLogging.TrackEvent(1108, $"NPD UPN TAB download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.NonNationalPupilDatabase:
                _eventLogging.TrackEvent(1111, $"NPD non-UPN TAB download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.MyPupilList:
                _eventLogging.TrackEvent(1118, $"MPL TAB download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;
        }

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, DataTypes = selectedDownloadOptions, ConfirmationGiven = confirmationGiven, FileType = "tab" };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, ReturnFile>(getTABFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

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

        switch (returnRoute)
        {
            case ReturnRoute.PupilPremium:
                _eventLogging.TrackEvent(1112, $"PP UPN CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.NonPupilPremium:
                _eventLogging.TrackEvent(1113, $"PP non-UPN CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;

            case ReturnRoute.MyPupilList:
                _eventLogging.TrackEvent(1119, $"MPL PP CSV download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                break;
        }

        var requestBody = new DownloadRequest { UPNs = selectedPupils, SortOrder = sortOrder, UserOrganisation = userOrganisation, ConfirmationGiven = confirmationGiven };
        var response = await _apiProcessorService.PostAsync<DownloadRequest, ReturnFile>(getFile.ConvertToUri(), requestBody, azureFunctionHeaderDetails).ConfigureAwait(false);

        return response;
    }
}
