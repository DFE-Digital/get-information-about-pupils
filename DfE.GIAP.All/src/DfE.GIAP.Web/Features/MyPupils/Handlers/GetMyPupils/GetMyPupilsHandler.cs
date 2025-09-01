using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public sealed class GetMyPupilsHandler : IGetMyPupilsHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<PupilDtoWithSelectionStateDecorator, PupilViewModel> _mapDtoToPupilViewModel;

    public GetMyPupilsHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<PupilDtoWithSelectionStateDecorator, PupilViewModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapper);
        _mapDtoToPupilViewModel = mapper;
    }

    public async Task<PupilsViewModel> HandleAsync(GetMyPupilsRequest request)
    {
        GetPaginatedMyPupilsRequest paginatedPupilsRequest = new(
            request.UserId,
            request.State.PresentationState);

        IEnumerable<PupilDtoWithSelectionStateDecorator> paginatedMyPupilsWithSelectionState =
            (await _getPaginatedMyPupilsQueryHandler.HandleAsync(paginatedPupilsRequest))
                .Pupils.Select((pupil)
                    => PupilDtoWithSelectionStateDecorator.Create(
                            pupil,
                            isSelected: request.State.SelectionState.IsPupilSelected(pupil.UniquePupilNumber)));

        IEnumerable<PupilViewModel> pupilViewModels = paginatedMyPupilsWithSelectionState.Select(_mapDtoToPupilViewModel.Map);
        return PupilsViewModel.Create(pupilViewModels);
    }
}
