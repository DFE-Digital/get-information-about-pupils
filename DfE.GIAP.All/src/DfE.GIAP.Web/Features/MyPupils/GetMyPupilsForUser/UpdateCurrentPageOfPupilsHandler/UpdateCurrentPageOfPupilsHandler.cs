using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.UpdateCurrentPageOfPupilsHandler;

public sealed class UpdateCurrentPageOfPupilsHandler : IUpdateCurrentPageOfPupilsStateHandler
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _sessionQueryHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _sessionCommandHandler;

    public UpdateCurrentPageOfPupilsHandler(
        ISessionQueryHandler<MyPupilsPupilSelectionState> sessionQueryHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> sessionCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(sessionQueryHandler);
        _sessionQueryHandler = sessionQueryHandler;

        ArgumentNullException.ThrowIfNull(sessionCommandHandler);
        _sessionCommandHandler = sessionCommandHandler;
    }
    public void Handle(UniquePupilNumbers currentPageOfPupils)
    {
        SessionQueryResponse<MyPupilsPupilSelectionState> sessionQueryResponse = _sessionQueryHandler.GetSessionObject();
        MyPupilsPupilSelectionState state =
            sessionQueryResponse.HasValue ? sessionQueryResponse.Value : new();

        state.CurrentPageOfPupils = currentPageOfPupils.GetUniquePupilNumbers();

        _sessionCommandHandler.StoreInSession(state);
    }
}
