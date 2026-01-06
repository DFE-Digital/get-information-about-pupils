using DfE.GIAP.Web.Features.MyPupils.Areas.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.UpdatePupilSelections;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;

public class UpdateMyPupilsPupilSelectionsCommandHandler : IUpdateMyPupilsPupilSelectionsCommandHandler
{
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateQueryHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _pupilSelectionStateCommandHandler;
    private readonly IEvaluationHandler<MyPupilsPupilSelectionState> _stateHandler;
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

        _stateHandler.Evaluate(selectionState);
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

public record UpdateMyPupilsSelectionStateRequest
{
    public UpdateMyPupilsSelectionStateRequest(MyPupilsFormStateRequestDto updateRequest, MyPupilsPupilSelectionState currentState)
    {
        ArgumentNullException.ThrowIfNull(updateRequest);
        Request = updateRequest;

        ArgumentNullException.ThrowIfNull(currentState);
        State = currentState;
    }

    public MyPupilsFormStateRequestDto Request { get; init; }
    public MyPupilsPupilSelectionState State { get; init; }
}

public interface IEvaluationHandler<TIn>
{
    void Evaluate(TIn input);
}

public interface IChainedHandler<TIn, TOut> : ICommandHandler<TIn>
{
    void ChainNext(ICommandHandler<TIn> handler);
}

public interface ICommandHandler<TIn>
{
    bool CanHandle(TIn input);
    void Handle(TIn input);
}

internal sealed class SelectAllPupilsCommandHandler : ICommandHandler<UpdateMyPupilsSelectionStateRequest>
{
    public bool CanHandle(UpdateMyPupilsSelectionStateRequest input) => input.Request.SelectAllState == MyPupilsFormSelectionModeRequestDto.SelectAll;

    public void Handle(UpdateMyPupilsSelectionStateRequest input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.State.SelectAll();
    }
}
