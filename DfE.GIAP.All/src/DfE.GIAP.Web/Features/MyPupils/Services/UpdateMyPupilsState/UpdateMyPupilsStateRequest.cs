using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState;

public record UpdateMyPupilsStateRequest(
    UserId UserId,
    MyPupilsState State,
    MyPupilsFormStateRequestDto UpdateStateInput);
