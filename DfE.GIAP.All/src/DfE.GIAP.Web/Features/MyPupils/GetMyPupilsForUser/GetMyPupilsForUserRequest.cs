using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;

public record GetMyPupilsForUserRequest(
    UserId UserId,
    MyPupilsState State);
