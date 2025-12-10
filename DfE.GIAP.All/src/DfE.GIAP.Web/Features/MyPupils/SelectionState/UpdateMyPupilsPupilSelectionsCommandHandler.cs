using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public class UpdateMyPupilsPupilSelectionsCommandHandler : IUpdateMyPupilsPupilSelectionsCommandHandler
{
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateQueryHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    public UpdateMyPupilsPupilSelectionsCommandHandler(
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateQueryHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(getMyPupilsStateQueryHandler);
        _getMyPupilsStateQueryHandler = getMyPupilsStateQueryHandler;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateCommandHandler);
        _pupilSelectionStateCommandHandler = pupilSelectionStateCommandHandler;
    }

    // TODO Chain of Responsibility
    public void Handle(MyPupilsFormStateRequestDto formDto)
    {
        MyPupilsPupilSelectionState selectionState = _getMyPupilsStateQueryHandler.GetPupilSelections();
        // Track a manual selection/deselection of pupils on the current page


        // Update SelectionState
        if (formDto.SelectAllState == MyPupilsFormSelectionModeRequestDto.SelectAll)
        {
            selectionState.SelectAll();
        }
        else if (formDto.SelectAllState == MyPupilsFormSelectionModeRequestDto.DeselectAll)
        {
            foreach (string upn in formDto.CurrentPupils)
            {
                selectionState.Deselect(upn);
            }
            selectionState.DeselectAll();
        }
        else // Manual selection so returns to modes
        {
            List<string> selectedPupilsOnPage = formDto.SelectedPupils?.ToList() ?? [];
            List<string> deselectedPupilsOnPage = formDto.CurrentPupils.Except(selectedPupilsOnPage).ToList();

            if (selectionState.Mode == SelectionMode.All)
            {
                foreach (string upn in deselectedPupilsOnPage)
                {
                    selectionState.Deselect(upn);
                }
                foreach (string upn in selectedPupilsOnPage)
                {
                    selectionState.Select(upn);
                }
            }
            else
            {
                foreach (string upn in selectedPupilsOnPage)
                {
                    selectionState.Select(upn);
                }
                foreach (string upn in deselectedPupilsOnPage)
                {
                    selectionState.Deselect(upn);
                }
            }
        }

        _pupilSelectionStateCommandHandler.StoreInSession(selectionState);
    }
}
