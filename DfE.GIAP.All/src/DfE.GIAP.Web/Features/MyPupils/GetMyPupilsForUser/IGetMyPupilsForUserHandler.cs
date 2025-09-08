using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;

public interface IGetMyPupilsForUserHandler
{
    Task<PupilsViewModel> HandleAsync(GetMyPupilsForUserRequest request);
}
