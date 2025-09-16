using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

public interface IGetPupilViewModelsForUserHandler
{
    Task<PupilsViewModel> GetPupilsAsync(GetMyPupilsForUserRequest request);
}
