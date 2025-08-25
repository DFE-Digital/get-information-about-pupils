using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.PupilDtoPresentationHandlers;

public interface IPupilDtoPresentationHandler
{
    IEnumerable<PupilDto> Handle(IEnumerable<PupilDto> pupils, MyPupilsPresentationState options);
}
