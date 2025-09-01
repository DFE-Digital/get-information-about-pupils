using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.Session.Abstractions.Query;

namespace DfE.GIAP.Web.Features.MyPupils.State;

public sealed class GetMyPupilsStateHandler : IGetMyPupilsStateHandler
{
    private readonly ISessionQueryHandler<MyPupilsPresentationState> _presentationStateSessionQueryHandler;
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;

    public GetMyPupilsStateHandler(
        ISessionQueryHandler<MyPupilsPresentationState> presentationStateSessionQueryHandler,
        ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler)
    {
        _presentationStateSessionQueryHandler = presentationStateSessionQueryHandler;
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;
    }

    public MyPupilsState GetState()
    {
        SessionQueryResponse<MyPupilsPresentationState> sessionPresentationStateResponse = _presentationStateSessionQueryHandler.GetSessionObject();

        MyPupilsPresentationState myPupilsPresentationState = sessionPresentationStateResponse.HasValue ?
            sessionPresentationStateResponse.Value :
                MyPupilsPresentationState.CreateDefault();

        SessionQueryResponse<MyPupilsPupilSelectionState> selectionState = _selectionStateSessionQueryHandler.GetSessionObject();

        MyPupilsPupilSelectionState myPupilsSelectionState = selectionState.HasValue ?
            selectionState.Value :
                MyPupilsPupilSelectionState.CreateDefault();

        return new MyPupilsState(
            myPupilsPresentationState,
            myPupilsSelectionState);
    }
}
