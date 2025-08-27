using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.UpdateMyPupilsState;

public record UpdateMyPupilsStateRequest(
    string UserId,
    MyPupilsPresentationState CurrentPresentationState,
    MyPupilsFormStateRequestDto UpdateStateInput);
