using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler;

public interface IPupilDtoPresentationHandler
{
    IEnumerable<PupilDto> Handle(IEnumerable<PupilDto> pupils, PresentPupilsOptions options);
}
