using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Abstraction.Query.Extensions;

namespace DfE.GIAP.Web.Features.MyPupils.State;

internal sealed class GetMyPupilsStateHandler : IGetMyPupilsStateHandler
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;
    private readonly ISessionQueryHandler<MyPupilsPresentationState> _presentationStateQueryHandler;

    public GetMyPupilsStateHandler(
        ISessionQueryHandler<MyPupilsPresentationState> presentationStateQueryHandler,
        ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(presentationStateQueryHandler);
        _presentationStateQueryHandler = presentationStateQueryHandler;

        ArgumentNullException.ThrowIfNull(selectionStateSessionQueryHandler);
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;
        
    }
    public MyPupilsState GetState()
    {
        SessionQueryResponse<MyPupilsPresentationState> presentationStateResponse =
            _presentationStateQueryHandler.GetSessionObject();

        MyPupilsPresentationState presentationState =
            presentationStateResponse.TryGetValueOrDefaultWith(() => MyPupilsPresentationState.CreateDefault());

        SessionQueryResponse<MyPupilsPupilSelectionState> selectionStateResponse =
            _selectionStateSessionQueryHandler.GetSessionObject();

        MyPupilsPupilSelectionState selectionState =
            selectionStateResponse.TryGetValueOrDefaultWith(() => new MyPupilsPupilSelectionState());

        return new(presentationState, selectionState);
    }
}
