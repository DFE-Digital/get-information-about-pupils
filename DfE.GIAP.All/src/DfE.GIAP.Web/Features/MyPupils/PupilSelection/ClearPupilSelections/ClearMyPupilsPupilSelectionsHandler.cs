using DfE.GIAP.Web.Shared.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.ClearSelections;

public sealed class ClearMyPupilsPupilSelectionsHandler : IClearMyPupilsPupilSelectionsHandler
{
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _sessionCommandHandler;

    public ClearMyPupilsPupilSelectionsHandler(
        ISessionCommandHandler<MyPupilsPupilSelectionState> sessionCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(sessionCommandHandler);
        _sessionCommandHandler = sessionCommandHandler;
    }

    public void Handle()
    {
        _sessionCommandHandler.StoreInSession(
            MyPupilsPupilSelectionState.CreateDefault());
    }
}
