using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Domain.Models.User;

namespace DfE.GIAP.Service.Common;

public interface ICommonService
{
    Task<bool> CreateLoggingEvent(LoggingEvent loggingEvent);
    Task<bool> CreateOrUpdateUserProfile(UserProfile userProfile, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
    Task<UserProfile> GetUserProfile(UserProfile userProfile);
}
