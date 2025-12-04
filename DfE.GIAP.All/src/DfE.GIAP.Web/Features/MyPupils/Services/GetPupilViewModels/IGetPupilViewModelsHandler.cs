using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

public interface IGetPupilViewModelsHandler
{
    Task<PupilsViewModel> GetPupilsAsync(GetPupilViewModelsRequest request);
}
