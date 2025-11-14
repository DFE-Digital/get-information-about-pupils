using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;

public record UpdateMyPupilsStateRequest(
    UserId UserId,
    MyPupilsState State,
    MyPupilsFormStateRequestDto UpdateStateInput);
