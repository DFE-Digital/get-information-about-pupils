using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils.PresentationHandlers;

public interface IMyPupilDtosPresentationHandler
{
    MyPupilDtos Handle(MyPupilDtos myPupils, MyPupilsPresentationState state);
}
