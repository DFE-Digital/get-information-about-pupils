using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers.Options;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers;

public interface IPupilDtoPresentationHandler
{
    IEnumerable<PupilDto> Handle(IEnumerable<PupilDto> pupils, PupilsPresentationOptions options);
}
