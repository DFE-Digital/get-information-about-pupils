using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.ClearPupilSelections;

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
