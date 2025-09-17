using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

internal sealed class MyPupilsViewModelFactory : IMyPupilsViewModelFactory
{
    public MyPupilsViewModel CreateViewModel(
        MyPupilsState state,
        PupilsViewModel pupils,
        string? error = "",
        bool isDeleteSuccessful = false)
    {
        ArgumentNullException.ThrowIfNull(state);

        PupilsViewModel pupilViewModels = pupils ?? PupilsViewModel.Create([]);

        MyPupilsViewModel myPupilsViewModel = new(pupilViewModels)
        {
            PageNumber = state.PresentationState.Page,
            SortDirection = state.PresentationState.SortDirection == SortDirection.Ascending ? "asc" : "desc",
            SortField = state.PresentationState.SortBy,
            IsAnyPupilsSelected = state.SelectionState.IsAnyPupilSelected,
            SelectAll = state.SelectionState.IsAllPupilsSelected,
            IsDeleteSuccessful = isDeleteSuccessful,
            Error = string.IsNullOrEmpty(error) ? MyPupilsErrorViewModel.NOOP() : MyPupilsErrorViewModel.Create(error)
        };

        return myPupilsViewModel;
    }
}
