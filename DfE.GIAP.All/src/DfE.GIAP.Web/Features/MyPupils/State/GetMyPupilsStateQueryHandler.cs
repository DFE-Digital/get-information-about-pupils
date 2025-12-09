using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Abstraction.Query.Extensions;

namespace DfE.GIAP.Web.Features.MyPupils.State;

internal sealed class GetMyPupilsStateQueryHandler : IGetMyPupilsStateQueryHandler
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;
    private readonly ISessionQueryHandler<MyPupilsPresentationState> _presentationStateQueryHandler;

    public GetMyPupilsStateQueryHandler(
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
            _presentationStateQueryHandler.Handle();

        MyPupilsPresentationState presentationState =
            presentationStateResponse.TryGetValueOrDefaultWith(() => MyPupilsPresentationState.CreateDefault());

        SessionQueryResponse<MyPupilsPupilSelectionState> selectionStateResponse =
            _selectionStateSessionQueryHandler.Handle();

        MyPupilsPupilSelectionState selectionState =
            selectionStateResponse.TryGetValueOrDefaultWith(() => new MyPupilsPupilSelectionState());

        return new(presentationState, selectionState);
    }
}
