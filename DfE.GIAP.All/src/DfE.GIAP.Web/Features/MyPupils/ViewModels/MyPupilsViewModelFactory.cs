using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

internal sealed class MyPupilsViewModelFactory : IMyPupilsViewModelFactory
{
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly IGetMyPupilsForUserProvider _getMyPupilsForUserHandler;
    public MyPupilsViewModelFactory(
        IGetMyPupilsStateProvider getMyPupilsStateProvider,
        IGetMyPupilsForUserProvider getMyPupilsForUserHandler)
    {

        _getMyPupilsStateProvider = getMyPupilsStateProvider;
        _getMyPupilsForUserHandler = getMyPupilsForUserHandler;
    }
    public async Task<MyPupilsViewModel> CreateViewModelAsync(UserId userId, MyPupilsErrorViewModel error = null)
    {
        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        GetMyPupilsForUserRequest request = new(userId, state);

        PupilsViewModel pupilsResponse = await _getMyPupilsForUserHandler.GetPupilsAsync(request);

        MyPupilsViewModel myPupilsViewModel = new(
            pupils: pupilsResponse,
            error: !string.IsNullOrEmpty(error?.Message) ? error : null)
        {
            PageNumber = state.PresentationState.Page,
            SortDirection = state.PresentationState.SortDirection == SortDirection.Ascending ? "asc" : "desc",
            SortField = state.PresentationState.SortBy,
            IsAnyPupilsSelected = state.SelectionState.IsAnyPupilSelected,
            SelectAll = state.SelectionState.IsAllPupilsSelected,
        };

        return myPupilsViewModel;
    }
}
