using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.ClearSelections;

public sealed class ClearMyPupilsPupilSelectionsHandler : IClearMyPupilsPupilSelectionsHandler
{
    private readonly IGetMyPupilsPupilSelectionProvider _stateProvider;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _sessionCommandHandler;

    public ClearMyPupilsPupilSelectionsHandler(
        IGetMyPupilsPupilSelectionProvider stateProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> sessionCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(stateProvider);
        _stateProvider = stateProvider;

        ArgumentNullException.ThrowIfNull(sessionCommandHandler);
        _sessionCommandHandler = sessionCommandHandler;
    }

    public void Handle()
    {
        MyPupilsPupilSelectionState selectionState = _stateProvider.GetPupilSelections();
        selectionState.ResetState();
        _sessionCommandHandler.StoreInSession(selectionState);
    }
}
