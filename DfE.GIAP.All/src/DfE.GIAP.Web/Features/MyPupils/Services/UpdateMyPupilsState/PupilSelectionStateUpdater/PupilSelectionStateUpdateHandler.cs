using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState.PupilSelectionStateUpdater;

internal sealed class PupilSelectionStateUpdateHandler : IPupilSelectionStateUpdateHandler
{
    public void Handle(MyPupilsPupilSelectionState state, IEnumerable<UniquePupilNumber> currentPageOfPupils, MyPupilsFormStateRequestDto updateState)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(currentPageOfPupils);
        ArgumentNullException.ThrowIfNull(updateState);

        if (updateState.SelectAllState == MyPupilsFormSelectionStateRequestDto.SelectAll)
        {
            state.UpsertPupilSelectionState(currentPageOfPupils, isSelected: true);
            state.SelectAllPupils();
            return;
        }
        if (updateState.SelectAllState == MyPupilsFormSelectionStateRequestDto.DeselectAll)
        {
            state.UpsertPupilSelectionState(currentPageOfPupils, isSelected: false);
            state.DeselectAllPupils();
            return;
        }
        // Manually track selections/deselections
        IEnumerable<UniquePupilNumber> selectedPupils = updateState.SelectedPupils.ToUniquePupilNumbers() ?? [];
        IEnumerable<UniquePupilNumber> deselectedPupils = currentPageOfPupils.Except(selectedPupils);

        state.UpsertPupilSelectionState(selectedPupils, isSelected: true);
        state.UpsertPupilSelectionState(deselectedPupils, isSelected: false);
    }
}
