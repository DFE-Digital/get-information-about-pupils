using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;

public record GetPaginatedMyPupilsRequest(UserId UserId, MyPupilsPresentationState PresentationState);
