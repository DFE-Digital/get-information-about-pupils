using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;

public interface IPupilDtosPresentationHandler
{
    PupilDtos Handle(PupilDtos pupils, MyPupilsPresentationState state);
}
