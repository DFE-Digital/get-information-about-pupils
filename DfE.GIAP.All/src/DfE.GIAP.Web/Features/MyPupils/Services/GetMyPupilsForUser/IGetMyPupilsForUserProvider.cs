using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

public interface IGetMyPupilsForUserProvider
{
    Task<PupilsViewModel> GetPupilsAsync(GetMyPupilsForUserRequest request);
}
