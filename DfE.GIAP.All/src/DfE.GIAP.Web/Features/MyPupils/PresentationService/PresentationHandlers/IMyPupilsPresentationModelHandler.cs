using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public interface IMyPupilsPresentationModelHandler
{
    MyPupilsPresentationPupilModels Handle(
        MyPupilsPresentationPupilModels pupils,
        MyPupilsPresentationQueryModel query,
        MyPupilsPupilSelectionState selectionState);
}