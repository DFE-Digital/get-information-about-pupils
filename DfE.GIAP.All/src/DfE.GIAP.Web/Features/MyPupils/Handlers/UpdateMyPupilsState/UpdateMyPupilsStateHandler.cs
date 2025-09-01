using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Session.Abstraction.Command;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.UpdateMyPupilsState;

public sealed class UpdateMyPupilsStateHandler : IUpdateMyPupilsStateHandler
{
    private readonly IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> _formDtoToPresentationStateMapper;
    private readonly ISessionCommandHandler<MyPupilsPresentationState> _presentationStateSessionComandHandler;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;

    public UpdateMyPupilsStateHandler(
        IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> formDtoToPresentationStateMapper,
        ISessionCommandHandler<MyPupilsPresentationState> presentationStateSessionComandHandler,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler)
    {
        _formDtoToPresentationStateMapper = formDtoToPresentationStateMapper;
        _presentationStateSessionComandHandler = presentationStateSessionComandHandler;
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;
    }

    public void Handle(UpdateMyPupilsStateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        MyPupilsPresentationState updatedPresentationState = _formDtoToPresentationStateMapper.Map(request.UpdateStateInput);

        MyPupilsPupilSelectionState selectionState = request.State.SelectionState;

        IEnumerable<string> currentPageOfPupils = request.State.SelectionState.CurrentPageOfPupils;

        if (request.UpdateStateInput.SelectAllState == MyPupilsFormSelectAllStateRequestDto.SelectAll)
        {
            selectionState.UpsertPupilWithSelectedState(currentPageOfPupils, isSelected: true);
            selectionState.SelectAllPupils();
        }
        else if (request.UpdateStateInput.SelectAllState == MyPupilsFormSelectAllStateRequestDto.DeselectAll)
        {
            selectionState.UpsertPupilWithSelectedState(currentPageOfPupils, isSelected: false);
            selectionState.DeselectAllPupils();
        }
        else
        {
            IEnumerable<string> selectedPupilsOnPage = request.UpdateStateInput.SelectedPupils ?? Enumerable.Empty<string>();
            IEnumerable<string> deselectedOnPage = currentPageOfPupils.Except(selectedPupilsOnPage);

            selectionState.UpsertPupilWithSelectedState(selectedPupilsOnPage, isSelected: true);
            selectionState.UpsertPupilWithSelectedState(deselectedOnPage, isSelected: false);
        }

        _presentationStateSessionComandHandler.StoreInSession(updatedPresentationState);
        _selectionStateSessionCommandHandler.StoreInSession(selectionState);
    }
}
