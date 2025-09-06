using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public record GetMyPupilsRequest(
    string UserId,
    MyPupilsState State);
