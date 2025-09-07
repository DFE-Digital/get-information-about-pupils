using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;

public record GetMyPupilsForUserRequest(
    string UserId,
    MyPupilsState State);
