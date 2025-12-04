using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

public interface IGetPupilViewModelsHandler
{
    Task<PupilsViewModel> GetPupilsAsync(GetPupilViewModelsRequest request);
}
