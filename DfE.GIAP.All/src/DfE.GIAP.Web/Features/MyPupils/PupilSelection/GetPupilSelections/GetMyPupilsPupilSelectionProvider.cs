using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;

internal sealed class GetMyPupilsPupilSelectionProvider : IGetMyPupilsPupilSelectionProvider
{
    private static readonly SessionCacheKey selectionStateCacheKey = new("MyPupilsPupilSelections");
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;

    public GetMyPupilsPupilSelectionProvider(
        ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(selectionStateSessionQueryHandler);
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;

    }

    public MyPupilsPupilSelectionState GetPupilSelections()
    {
        SessionQueryResponse<MyPupilsPupilSelectionState> selectionStateResponse = _selectionStateSessionQueryHandler.Handle(selectionStateCacheKey);

        MyPupilsPupilSelectionState selectionState =
            selectionStateResponse.HasValue ?
                selectionStateResponse.Value :
                    MyPupilsPupilSelectionState.CreateDefault();

        return selectionState;
    }
}
