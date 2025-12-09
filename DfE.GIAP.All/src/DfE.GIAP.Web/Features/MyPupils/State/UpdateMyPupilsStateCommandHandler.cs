using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public class UpdateMyPupilsStateCommandHandler : IUpdateMyPupilsStateCommandHandler
{
    private readonly IGetMyPupilsStateQueryHandler _getMyPupilsStateQueryHandler;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateCommandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    public UpdateMyPupilsStateCommandHandler(
        IGetMyPupilsStateQueryHandler getMyPupilsStateQueryHandler,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateCommandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> pupilSelectionStateCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(getMyPupilsStateQueryHandler);
        _getMyPupilsStateQueryHandler = getMyPupilsStateQueryHandler;

        ArgumentNullException.ThrowIfNull(presentationStateCommandHandler);
        _presentationStateCommandHandler = presentationStateCommandHandler;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateCommandHandler);
        _pupilSelectionStateCommandHandler = pupilSelectionStateCommandHandler;
    }

    // TODO Chain of Responsibility
    public void Handle(MyPupilsFormStateRequestDto formDto)
    {
        MyPupilsState state = _getMyPupilsStateQueryHandler.GetState();
        // Track a manual selection/deselection of pupils on the current page


        // Update SelectionState
        if (formDto.SelectAllState == MyPupilsFormSelectionModeRequestDto.SelectAll)
        {
            state.SelectionState.SelectAll();
        }
        else if (formDto.SelectAllState == MyPupilsFormSelectionModeRequestDto.DeselectAll)
        {
            foreach (string upn in formDto.CurrentPupils)
            {
                state.SelectionState.Deselect(upn);
            }
            state.SelectionState.DeselectAll();
        }
        else // Manual selection so returns to modes
        {
            List<string> selectedPupilsOnPage = formDto.SelectedPupils?.ToList() ?? [];
            List<string> deselectedPupilsOnPage = formDto.CurrentPupils.Except(selectedPupilsOnPage).ToList();

            if (state.SelectionState.Mode == SelectionMode.All)
            {
                foreach (string upn in deselectedPupilsOnPage)
                {
                    state.SelectionState.Deselect(upn);
                }
                foreach (string upn in selectedPupilsOnPage)
                {
                    state.SelectionState.Select(upn);
                }
            }
            else
            {
                foreach (string upn in selectedPupilsOnPage)
                {
                    state.SelectionState.Select(upn);
                }
                foreach (string upn in deselectedPupilsOnPage)
                {
                    state.SelectionState.Deselect(upn);
                }
            }
        }

        // Update PresentationState
        _pupilSelectionStateCommandHandler.StoreInSession(state.SelectionState);

        _presentationStateCommandHandler.StoreInSession(
            new MyPupilsPresentationState(
                Page: formDto.PageNumber,
                SortBy: formDto.SortField,
                SortDirection: formDto.SortDirection == "asc" ? SortDirection.Ascending : SortDirection.Descending));
    }
}
