using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.ApiProcessor;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Service.Common;

public class CommonService : ICommonService
{
    private readonly IApiService _apiProcessorService;
    private readonly AzureAppSettings _azureAppSettings;

    public CommonService(
        IApiService apiProcessorService,
        IOptions<AzureAppSettings> azureAppSettings)
    {
        _apiProcessorService = apiProcessorService;
        _azureAppSettings = azureAppSettings.Value;
    }


    public async Task<bool> CreateLoggingEvent(LoggingEvent loggingEvent)
    {
        try
        {
            var query = _azureAppSettings.LoggingEventUrl;
            var response = await _apiProcessorService.PostAsync<LoggingEvent, string>(query.ConvertToUri(), loggingEvent, null).ConfigureAwait(false);

            return Convert.ToBoolean(response);
        }
        catch
        {
            return false;
        }

    }

    public async Task<bool> CreateOrUpdateUserProfile(UserProfile userProfile,
                                                      AzureFunctionHeaderDetails azureFunctionHeaderDetails)
    {
        try
        {
            var query = _azureAppSettings.CreateOrUpdateUserProfileUrl;
            var response = await _apiProcessorService.PostAsync<UserProfile, string>(query.ConvertToUri(), userProfile, azureFunctionHeaderDetails).ConfigureAwait(false);

            return Convert.ToBoolean(response);
        }
        catch
        {
            return false;
        }

    }

    public async Task<UserProfile> GetUserProfile(UserProfile userProfile)
    {
        try
        {
            var query = _azureAppSettings.GetUserProfileUrl;
            var response = await _apiProcessorService.PostAsync<UserProfile, UserProfile>(query.ConvertToUri(), userProfile, null).ConfigureAwait(false);

            return response;
        }
        catch
        {
            return null;
        }

    }


}
