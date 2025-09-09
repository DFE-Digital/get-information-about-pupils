using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Routes.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState.PupilSelectionStateUpdater;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.Services.UpdateMyPupilsState;

internal sealed class UpdateMyPupilsStateHandler : IUpdateMyPupilsStateHandler
{
    private readonly IPupilSelectionStateUpdateHandler _pupilSelectionStateHandler;
    private readonly IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> _formStateDtoToPresentationStateMapper;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateSessionComandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsHandler;


    public UpdateMyPupilsStateHandler(
        IPupilSelectionStateUpdateHandler pupilSelectionStateHandler,
        IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> formDtoToPresentationStateMapper,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateSessionComandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsHandler)
    {
        ArgumentNullException.ThrowIfNull(pupilSelectionStateHandler);
        _pupilSelectionStateHandler = pupilSelectionStateHandler;

        ArgumentNullException.ThrowIfNull(formDtoToPresentationStateMapper);
        _formStateDtoToPresentationStateMapper = formDtoToPresentationStateMapper;

        ArgumentNullException.ThrowIfNull(presentationStateSessionComandHandler);
        _presentationStateSessionComandHandler = presentationStateSessionComandHandler;

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;

        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsHandler);
        _getPaginatedMyPupilsHandler = getPaginatedMyPupilsHandler;
    }

    public async Task HandleAsync(UpdateMyPupilsStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.State);
        ArgumentNullException.ThrowIfNull(request.UpdateStateInput);

        // Update SelectionState
        MyPupilsPupilSelectionState pupilSelectionState = request.State.SelectionState;

        PaginatedMyPupilsResponse paginatedMyPupilsResponse = await _getPaginatedMyPupilsHandler.HandleAsync(
            new GetPaginatedMyPupilsRequest(
                request.UserId,
                request.State.PresentationState));

        _pupilSelectionStateHandler.Handle(
            state: request.State.SelectionState,
            currentPageOfPupils: paginatedMyPupilsResponse.Pupils.Identifiers,
            updateState: request.UpdateStateInput);

        _selectionStateSessionCommandHandler.StoreInSession(pupilSelectionState);

        // Update PresentationState
        MyPupilsPresentationState myPupilsPresentationState = _formStateDtoToPresentationStateMapper.Map(request.UpdateStateInput);
        _presentationStateSessionComandHandler.StoreInSession(myPupilsPresentationState);
    }
}
