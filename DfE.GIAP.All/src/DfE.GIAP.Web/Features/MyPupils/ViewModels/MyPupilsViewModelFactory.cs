using DfE.GIAP.Web.Features.MyPupils.Routes;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

internal sealed class MyPupilsViewModelFactory : IMyPupilsViewModelFactory
{
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly IGetPupilViewModelsForUserHandler _getMyPupilsForUserHandler;

    public MyPupilsViewModelFactory(
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        IGetPupilViewModelsForUserHandler getPupilViewModelsForUserHandler)
    {
        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(getPupilViewModelsForUserHandler);
        _getMyPupilsForUserHandler = getPupilViewModelsForUserHandler;
    }

    public async Task<MyPupilsViewModel> CreateViewModelAsync(
        string userId,
        MyPupilsViewModelContext context)
    {
        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        GetMyPupilsForUserRequest request = new(userId, state);

        PupilsViewModel pupilsResponse = await _getMyPupilsForUserHandler.GetPupilsAsync(request);

        MyPupilsViewModel myPupilsViewModel = new(pupilsResponse)
        {
            PageNumber = state.PresentationState.Page,
            SortDirection = state.PresentationState.SortDirection == SortDirection.Ascending ? "asc" : "desc",
            SortField = state.PresentationState.SortBy,
            IsAnyPupilsSelected = state.SelectionState.IsAnyPupilSelected,
            SelectAll = state.SelectionState.IsAllPupilsSelected,
            IsDeleteSuccessful = context.IsDeleteSuccessful,
            Error = context.Error
        };

        return myPupilsViewModel;
    }
}
