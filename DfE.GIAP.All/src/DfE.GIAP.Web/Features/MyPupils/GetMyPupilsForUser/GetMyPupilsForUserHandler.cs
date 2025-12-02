using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;

internal sealed class GetMyPupilsForUserHandler : IGetMyPupilsForUserHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel> _mapToViewModel;

    public GetMyPupilsForUserHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel> mapToViewModel)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;
    }

    public async Task<PupilsViewModel> HandleAsync(GetMyPupilsForUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.State);

        GetPaginatedMyPupilsRequest paginatedPupilsRequest = new(
            request.UserId,
            request.State.PresentationState);

        PaginatedMyPupilsResponse response = await _getPaginatedMyPupilsQueryHandler.HandleAsync(paginatedPupilsRequest);
        MyPupilsModelSelectionStateDecorator mappable = new(response.Pupils, request.State.SelectionState);
        PupilsViewModel viewModel = _mapToViewModel.Map(mappable);

        return viewModel;
    }
}
