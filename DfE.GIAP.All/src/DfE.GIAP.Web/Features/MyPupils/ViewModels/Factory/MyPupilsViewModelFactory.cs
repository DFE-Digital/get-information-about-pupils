using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;

internal sealed class MyPupilsViewModelFactory : IMyPupilsViewModelFactory
{
    public MyPupilsViewModel CreateViewModel(
        MyPupilsState state,
        PupilsViewModel pupils,
        MyPupilsViewModelContext context)
    {
        ArgumentNullException.ThrowIfNull(state);

        MyPupilsViewModelContext guardedContext = context ?? MyPupilsViewModelContext.Default();

        MyPupilsViewModel myPupilsViewModel = new(pupils ?? PupilsViewModel.Create([]))
        {
            PageNumber = state.PresentationState.Page,
            SortDirection = state.PresentationState.SortDirection == SortDirection.Ascending ? "asc" : "desc",
            SortField = state.PresentationState.SortBy,
            IsAnyPupilsSelected = state.SelectionState.IsAnyPupilSelected,
            SelectAll = state.SelectionState.IsAllPupilsSelected,
            IsDeleteSuccessful = guardedContext.IsDeletePupilsSuccessful,
            Error = string.IsNullOrEmpty(guardedContext.Error) ? MyPupilsErrorViewModel.NOOP() : MyPupilsErrorViewModel.Create(guardedContext.Error)
        };

        return myPupilsViewModel;
    }
}
