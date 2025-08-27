using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Extensions;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModels;

public class MyPupilsFormStateWithPupilsViewModel
{
    private const int PUPILS_DISPLAYED_PER_PAGE = 20;
    private readonly MyPupilsPresentationState _presentationState;
    private readonly MyPupilsPupilSelectionState _selectionState;

    public MyPupilsFormStateWithPupilsViewModel(
        PupilsViewModel pupils,
        MyPupilsPresentationState presentationState,
        MyPupilsPupilSelectionState selectionState)
    {
        ArgumentNullException.ThrowIfNull(pupils);
        Pupils = pupils;

        ArgumentNullException.ThrowIfNull(presentationState);
        _presentationState = presentationState;

        ArgumentNullException.ThrowIfNull(selectionState);
        _selectionState = selectionState;
    }

    public PupilsViewModel Pupils { get; }
    public bool IsAnyPupilsSelected => _selectionState.IsAnyPupilSelected;
    public string CurrentSortField => _presentationState.SortBy;
    public string CurrentSortDirection => _presentationState.SortDirection.ToFormSortDirection();
    public int PageNumber => _presentationState.Page;
    public bool IsDisplayPreviousPage => PageNumber > 2; // If enabled for Page 2, it will show 1, 1, 2
    public bool IsDisplayNextPage => Pupils.Count == PUPILS_DISPLAYED_PER_PAGE;
    public bool IsNoPupilsRemaining => !Pupils.Pupils.Any();
}
