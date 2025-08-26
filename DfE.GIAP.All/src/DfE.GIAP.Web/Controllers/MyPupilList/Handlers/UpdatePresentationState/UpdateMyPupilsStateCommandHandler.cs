using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.UpdatePresentationState;

public sealed class UpdateMyPupilsStateCommandHandler : IUpdateMyPupilsStateCommandHandler
{
    private readonly IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> _formStateToPresentationStateMapper;
    private readonly IMyPupilsPresentationStateProvider _presentPupilOptionsProvider;
    private readonly IPupilSelectionStateProvider _pupilSelectionStateProvider;

    public UpdateMyPupilsStateCommandHandler(
        IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState> formStateToPresentationStateMapper,
        IMyPupilsPresentationStateProvider presentPupilOptionsProvider,
        IPupilSelectionStateProvider pupilSelectionStateProvider)
    {
        ArgumentNullException.ThrowIfNull(formStateToPresentationStateMapper);
        _formStateToPresentationStateMapper = formStateToPresentationStateMapper;

        ArgumentNullException.ThrowIfNull(presentPupilOptionsProvider);
        _presentPupilOptionsProvider = presentPupilOptionsProvider;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateProvider);
        _pupilSelectionStateProvider = pupilSelectionStateProvider;
    }

    public void Handle(MyPupilsFormStateRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _presentPupilOptionsProvider.Set(options: _formStateToPresentationStateMapper.Map(request));

        IMyPupilsPupilSelectionState selectionState = _pupilSelectionStateProvider.GetState();

        IEnumerable<string> currentPageOfPupils = request.ParseCurrentPageOfPupils();

        selectionState.AddPupils(currentPageOfPupils);

        if (request.IsSelectAllPupils)
        {
            selectionState.SelectAllPupils();
            _pupilSelectionStateProvider.SetState(selectionState);
            return;
        }

        if (request.IsDeselectAllPupils)
        {
            selectionState.DeselectAllPupils();
            _pupilSelectionStateProvider.SetState(selectionState);
            return;
        }

        // No SelectAll specified — apply individual selections only
        IEnumerable<string> selected = request.SelectedPupils ?? Enumerable.Empty<string>();
        IEnumerable<string> deselected = currentPageOfPupils.Except(selected);

        selectionState.UpdatePupilSelectionState(selected, isSelected: true);
        selectionState.UpdatePupilSelectionState(deselected, isSelected: false);
        _pupilSelectionStateProvider.SetState(selectionState);
    }
}
