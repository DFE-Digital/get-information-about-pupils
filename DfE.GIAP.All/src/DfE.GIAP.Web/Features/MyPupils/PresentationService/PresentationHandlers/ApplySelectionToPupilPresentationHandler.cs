using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class ApplySelectionToPupilPresentationHandler : IMyPupilsPresentationModelHandler
{
    public MyPupilsPresentationPupilModels Handle(
        MyPupilsPresentationPupilModels pupils,
        MyPupilsPresentationQueryModel query,
        MyPupilsPupilSelectionState selectionState)
    {
        foreach (MyPupilsPresentationPupilModel pupil in pupils.Values)
        {
            pupil.IsSelected = selectionState.IsPupilSelected(pupil.UniquePupilNumber);
        }
        return pupils;
    }
}
