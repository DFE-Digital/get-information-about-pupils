using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Models.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.MPL
{
    public interface IMyPupilListService
    {
        Task UpdateMyPupilList(IEnumerable<MyPupilListItem> myPupilListItems, string userId, AzureFunctionHeaderDetails details);
        Task<IEnumerable<MyPupilListItem>> GetMyPupilListLearnerNumbers(string userId);
        Task UpdatePupilMasks(IEnumerable<string> pupilsToUpdate, bool markedValue, string userId, AzureFunctionHeaderDetails details);
    }
}
