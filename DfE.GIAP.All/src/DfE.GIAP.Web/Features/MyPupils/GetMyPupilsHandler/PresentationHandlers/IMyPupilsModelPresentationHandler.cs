using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler.PresentationHandlers;

public interface IMyPupilsModelPresentationHandler
{
    MyPupilsModel Handle(MyPupilsModel myPupils, MyPupilsPresentationState state);
}
