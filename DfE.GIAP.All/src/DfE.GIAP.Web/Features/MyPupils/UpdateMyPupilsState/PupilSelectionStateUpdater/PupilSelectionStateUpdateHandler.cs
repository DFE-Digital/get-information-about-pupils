using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState.PupilSelectionStateUpdater;

public sealed class PupilSelectionStateUpdateHandler : IPupilSelectionStateUpdateHandler
{
    public void Handle(MyPupilsPupilSelectionState state, IEnumerable<UniquePupilNumber> currentPageOfPupils, MyPupilsFormStateRequestDto input)
    {
        if (input.SelectAllState == MyPupilsFormSelectionStateRequestDto.SelectAll)
        {
            state.UpsertPupilSelectionState(currentPageOfPupils, isSelected: true);
            state.SelectAllPupils();
        }
        else if (input.SelectAllState == MyPupilsFormSelectionStateRequestDto.DeselectAll)
        {
            state.UpsertPupilSelectionState(currentPageOfPupils, isSelected: false);
            state.DeselectAllPupils();
        }
        else
        {
            // Manually track selections/deselections
            IEnumerable<UniquePupilNumber> selectedPupils = input.SelectedPupils.ToUniquePupilNumbers() ?? [];
            IEnumerable<UniquePupilNumber> deselectedPupils = currentPageOfPupils.Except(selectedPupils);

            state.UpsertPupilSelectionState(selectedPupils, isSelected: true);
            state.UpsertPupilSelectionState(deselectedPupils, isSelected: false);
        }
    }
}
