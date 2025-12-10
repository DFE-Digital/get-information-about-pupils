using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public interface IMyPupilsPresentationModelHandler
{
    MyPupilsPresentationPupilModels Handle(MyPupilsPresentationPupilModels pupils, MyPupilsState state);
}
