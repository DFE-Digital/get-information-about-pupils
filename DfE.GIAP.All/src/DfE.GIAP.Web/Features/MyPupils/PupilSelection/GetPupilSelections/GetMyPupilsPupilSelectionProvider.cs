using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Options;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Query;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;

internal sealed class GetMyPupilsPupilSelectionProvider : IGetMyPupilsPupilSelectionProvider
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;
    private readonly SessionCacheKey _sessionKey;

    public GetMyPupilsPupilSelectionProvider(
        ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler,
        IOptions<MyPupilSelectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(selectionStateSessionQueryHandler);
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _sessionKey = new(options.Value.SelectionsSessionKey);
    }

    public MyPupilsPupilSelectionState GetPupilSelections()
    {
        SessionQueryResponse<MyPupilsPupilSelectionState> selectionStateResponse = _selectionStateSessionQueryHandler.Handle(_sessionKey);

        MyPupilsPupilSelectionState selectionState =
            selectionStateResponse.HasValue ?
                selectionStateResponse.Value :
                    MyPupilsPupilSelectionState.CreateDefault();

        return selectionState;
    }
}
