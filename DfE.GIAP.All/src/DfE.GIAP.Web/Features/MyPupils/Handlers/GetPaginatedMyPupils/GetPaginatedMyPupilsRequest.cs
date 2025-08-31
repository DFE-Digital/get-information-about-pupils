using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

public record GetPaginatedMyPupilsRequest(string UserId, MyPupilsPresentationState PresentationState);
