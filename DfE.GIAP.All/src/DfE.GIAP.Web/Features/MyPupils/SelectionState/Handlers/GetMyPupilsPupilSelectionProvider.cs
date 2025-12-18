using DfE.GIAP.Web.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Handlers;

internal sealed class GetMyPupilsPupilSelectionProvider : IGetMyPupilsPupilSelectionProvider
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;

    public GetMyPupilsPupilSelectionProvider(
        ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(selectionStateSessionQueryHandler);
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;

    }

    public MyPupilsPupilSelectionState GetPupilSelections()
    {
        SessionQueryResponse<MyPupilsPupilSelectionState> selectionStateResponse = _selectionStateSessionQueryHandler.Handle();

        MyPupilsPupilSelectionState selectionState =
            selectionStateResponse.HasValue ?
                selectionStateResponse.Value :
                    MyPupilsPupilSelectionState.CreateDefault();

        return selectionState;
    }
}
