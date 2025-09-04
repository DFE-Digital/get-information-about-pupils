using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public sealed class GetMyPupilsHandler : IGetMyPupilsHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel> _mapToViewModel;

    public GetMyPupilsHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel> mapToViewModel)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;
    }

    public async Task<PupilsViewModel> HandleAsync(GetMyPupilsRequest request)
    {
        GetPaginatedMyPupilsRequest paginatedPupilsRequest = new(
            request.UserId,
            request.State.PresentationState);

        PaginatedMyPupilsResponse response = (await _getPaginatedMyPupilsQueryHandler.HandleAsync(paginatedPupilsRequest));

        MyPupilsDtoSelectionStateDecorator mappable = new(response.Pupils, request.State.SelectionState);

        return _mapToViewModel.Map(mappable);
    }
}
