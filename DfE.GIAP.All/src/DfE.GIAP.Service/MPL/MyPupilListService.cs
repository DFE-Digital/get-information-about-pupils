using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.MPL
{
    /// <summary>
    /// Gives functions that allow the system to interrogate the users MPL
    /// </summary>
    public class MyPupilListService : IMyPupilListService
    {
        private readonly ICommonService _commonService;

        /// <summary>
        /// Creates an instance of MyPupilListService
        /// </summary>
        /// <param name="commonService">DI injected common service, used to fetch the user profile.</param>
        public MyPupilListService(
            ICommonService commonService
            )
        {
            _commonService = commonService;
        }

        /// <summary>
        /// Fetches the MPL from a users profile. Also filters out "0" UPNs and blank UPNs.
        /// </summary>
        /// <param name="userId">the users ID</param>
        /// <returns>a list of UPNs the user has in their MPL</returns>
        public async Task<IEnumerable<MyPupilListItem>> GetMyPupilListLearnerNumbers(string userId)
        {
            var userProfile = await _commonService.GetUserProfile(new UserProfile()
            {
                UserId = userId
            });

            if (userProfile != null && userProfile.MyPupilList != null)
            {
                var mpl=  userProfile.MyPupilList.
                    Where(x => x.PupilId != "0").
                    Where(x => !string.IsNullOrWhiteSpace(x.PupilId));
                return mpl;
            }

            return new List<MyPupilListItem>();
        }

        /// <summary>
        /// Wraps the common service update user profile function so the MPL can be updated.
        /// </summary>
        /// <param name="learnerNumbers"></param>
        /// <param name="userId"></param>
        /// <param name="details"></param>
        /// <returns>void</returns>
        public async Task UpdateMyPupilList(IEnumerable<MyPupilListItem> myPupilListItems, string userId, AzureFunctionHeaderDetails details)
        {
            _ = await _commonService.CreateOrUpdateUserProfile(new UserProfile()
            {
                UserId = userId,
                IsPupilListUpdated = true,
                MyPupilList = myPupilListItems
            }, details);
        }

        /// <summary>
        /// Shorthand method to update all pupils in user's current MPL that match the UPNs in pupilsToUpdate to markedValue boolean
        /// </summary>
        /// <param name="pupilsToUpdate">List of UPNs for pupils</param>
        /// <param name="markedValue">Boolean of value to set</param>
        /// <param name="userId">User's Id</param>
        /// <param name="details">Azure auth header</param>
        public async Task UpdatePupilMasks(IEnumerable<string> pupilsToUpdate, bool markedValue, string userId, AzureFunctionHeaderDetails details)
        {
            var fullMPL = await GetMyPupilListLearnerNumbers(userId);
            foreach (var item in fullMPL)
            {
                if (pupilsToUpdate.Contains(item.PupilId))
                {
                    item.IsMasked = markedValue;
                }
            }

            var userProfile = new UserProfile
            {
                UserId = userId,
                IsPupilListUpdated = true,
                MyPupilList = fullMPL
            };

            _ = await _commonService.CreateOrUpdateUserProfile(userProfile, details);
        }
    }
}
