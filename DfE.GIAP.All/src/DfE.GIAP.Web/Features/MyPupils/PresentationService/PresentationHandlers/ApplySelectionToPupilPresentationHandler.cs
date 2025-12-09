using DfE.GIAP.Web.Features.MyPupils.State.Models;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class ApplySelectionToPupilPresentationHandler : IMyPupilsPresentationModelHandler
{
    public MyPupilsPresentationPupilModels Handle(MyPupilsPresentationPupilModels pupils, MyPupilsState state)
    {
        foreach (MyPupilsPresentationPupilModel pupil in pupils.Values)
        {
            pupil.IsSelected = state.SelectionState.IsPupilSelected(pupil.UniquePupilNumber);
        }
        return pupils;
    }
}
