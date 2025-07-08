using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.LoggingEvent;
using DfE.GIAP.Domain.Models.User;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Common
{
    public interface ICommonService
    {

        Task<bool> GetLatestNewsStatus(string userId);
        Task<bool> SetLatestNewsStatus(string userId);
        Task<bool> CreateLoggingEvent(LoggingEvent loggingEvent);

        Task<bool> CreateOrUpdateUserProfile(UserProfile userProfile, AzureFunctionHeaderDetails azureFunctionHeaderDetails);

        Task<UserProfile> GetUserProfile(UserProfile userProfile);
    }
}
