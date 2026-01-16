using DfE.GIAP.Web.Features.MyPupils.Controllers;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
#nullable enable
public interface IMyPupilsPresentationService
{
    Task DeletePupilsAsync(
        string userId,
        IEnumerable<string> selectedPupilUpns);

    Task<MyPupilsPresentationResponse> GetPupilsAsync(
        string userId,
        MyPupilsQueryRequestDto? query);

    Task<IEnumerable<string>> GetSelectedPupilsAsync(string userId);
}
