using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;

public interface IPupilPresentationHandler
{
    IEnumerable<PupilDto> Handle(IEnumerable<PupilDto> pupils, MyPupilsPresentationState options);
}
