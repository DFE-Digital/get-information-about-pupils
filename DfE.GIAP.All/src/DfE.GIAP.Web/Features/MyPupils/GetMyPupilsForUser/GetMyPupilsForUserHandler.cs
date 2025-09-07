using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.UpdateCurrentPageOfPupilsHandler;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;

internal sealed class GetMyPupilsForUserHandler : IGetMyPupilsForUserHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel> _mapToViewModel;
    private readonly IUpdateCurrentPageOfPupilsStateHandler _updateCurrentPageOfPupilsStateHandler;

    public GetMyPupilsForUserHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel> mapToViewModel,
        IUpdateCurrentPageOfPupilsStateHandler updateCurrentPageOfPupilsStateHandler)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;

        ArgumentNullException.ThrowIfNull(updateCurrentPageOfPupilsStateHandler);
        _updateCurrentPageOfPupilsStateHandler = updateCurrentPageOfPupilsStateHandler;
    }

    public async Task<PupilsViewModel> HandleAsync(GetMyPupilsForUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.State);

        GetPaginatedMyPupilsRequest paginatedPupilsRequest = new(
            request.UserId,
            request.State.PresentationState);

        PaginatedMyPupilsResponse response = await _getPaginatedMyPupilsQueryHandler.HandleAsync(paginatedPupilsRequest);
        MyPupilsDtoSelectionStateDecorator mappable = new(response.Pupils, request.State.SelectionState);
        PupilsViewModel viewModel = _mapToViewModel.Map(mappable);

        // CurrentPageOfPupils state update - required for UpdateMyPupilsState to infer DeselectedPupils
        _updateCurrentPageOfPupilsStateHandler.Handle(
            pupils: UniquePupilNumbers.Create(
                response.Pupils.Values.Select(
                    (pupil) => pupil.UniquePupilNumber)));

        return viewModel;
    }
}
