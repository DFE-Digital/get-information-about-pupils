using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;

public record GetPaginatedMyPupilsRequest(string UserId, MyPupilsPresentationState PresentationState);
