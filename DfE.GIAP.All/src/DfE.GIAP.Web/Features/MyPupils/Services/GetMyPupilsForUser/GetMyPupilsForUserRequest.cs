using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

public record GetMyPupilsForUserRequest(
    string UserId,
    MyPupilsState State);
