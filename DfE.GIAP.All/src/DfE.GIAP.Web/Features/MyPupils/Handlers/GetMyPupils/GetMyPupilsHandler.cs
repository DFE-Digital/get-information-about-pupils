using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel.Factory;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public sealed class GetMyPupilsHandler : IGetMyPupilsHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IPupilsViewModelFactory _pupilsViewModelFactory;

    public GetMyPupilsHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IPupilsViewModelFactory pupilsViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(pupilsViewModelFactory);
        _pupilsViewModelFactory = pupilsViewModelFactory;
    }

    public async Task<PupilsViewModel> HandleAsync(GetMyPupilsRequest request)
    {
        GetPaginatedMyPupilsRequest paginatedPupilsRequest = new(
            request.UserId,
            request.State.PresentationState);

        PaginatedMyPupilsResponse response = (await _getPaginatedMyPupilsQueryHandler.HandleAsync(paginatedPupilsRequest));

        return _pupilsViewModelFactory.CreateViewModel(response.Pupils, request.State.SelectionState);
    }
}
