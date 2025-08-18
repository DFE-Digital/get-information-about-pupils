using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation;

public interface IMyPupilsPresentationService
{
    Task<GetPupilsForUserFromPresentationStateResponse> GetPupilsForUserFromPresentationStateAsync(string userId);
    Task<IEnumerable<string>> GetSelectedPupilsForUserAsync(string userId);
    void UpdatePresentationState(MyPupilsFormStateRequestDto updateStateRequest);
    void ClearPresentationState();
}
