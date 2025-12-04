using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;

internal sealed class GetPupilViewModelsHandler : IGetPupilViewModelsHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel> _mapToViewModel;

    public GetPupilViewModelsHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<MyPupilsModelSelectionStateDecorator, PupilsViewModel> mapToViewModel)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;
    }

    public async Task<PupilsViewModel> GetPupilsAsync(GetPupilViewModelsRequest request)
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
