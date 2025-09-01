using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.UpdateMyPupilsState;

public record UpdateMyPupilsStateRequest(
    string UserId,
    MyPupilsState State,
    MyPupilsFormStateRequestDto UpdateStateInput);
