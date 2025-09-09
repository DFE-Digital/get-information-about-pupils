using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

internal sealed class GetMyPupilsForUserProvider : IGetMyPupilsForUserProvider
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel> _mapToViewModel;

    public GetMyPupilsForUserProvider(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel> mapToViewModel)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;
    }

    public async Task<PupilsViewModel> GetPupilsAsync(GetMyPupilsForUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.State);

        GetPaginatedMyPupilsRequest paginatedPupilsRequest = new(
            request.UserId,
            request.State.PresentationState);

        PaginatedMyPupilsResponse response = await _getPaginatedMyPupilsQueryHandler.HandleAsync(paginatedPupilsRequest);

        MyPupilsDtoSelectionStateDecorator mappable = new(response.Pupils, request.State.SelectionState);

        PupilsViewModel viewModel = _mapToViewModel.Map(mappable);

        return viewModel;
    }
}
