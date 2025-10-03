using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils.PresentationHandlers;

public interface IMyPupilDtosPresentationHandler
{
    MyPupilsModel Handle(MyPupilsModel myPupils, MyPupilsPresentationState state);
}
