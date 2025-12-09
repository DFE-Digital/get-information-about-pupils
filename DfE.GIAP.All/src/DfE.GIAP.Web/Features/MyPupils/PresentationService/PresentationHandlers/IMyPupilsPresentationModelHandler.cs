using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Models;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public interface IMyPupilsPresentationModelHandler
{
    MyPupilsPresentationPupilModels Handle(MyPupilsPresentationPupilModels pupils, MyPupilsState state);
}
