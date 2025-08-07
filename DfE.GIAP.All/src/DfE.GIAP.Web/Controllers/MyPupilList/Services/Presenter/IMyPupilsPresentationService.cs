using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.FormState;
using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presenter;

public interface IMyPupilsPresentationService
{
    Task<IEnumerable<PupilPresentatationModel>> GetPupilsForUserAsync(
        string userId,
        MyPupilsFormState state);
}
