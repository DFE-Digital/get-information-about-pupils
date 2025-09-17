using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

public interface IGetPupilViewModelsHandler
{
    Task<PupilsViewModel> GetPupilsAsync(GetPupilViewModelsRequest request);
}
