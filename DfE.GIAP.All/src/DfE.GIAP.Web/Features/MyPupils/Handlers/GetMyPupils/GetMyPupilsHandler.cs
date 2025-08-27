using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.ViewModels;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;

public sealed class GetMyPupilsHandler : IGetMyPupilsHandler
{
    private readonly IGetPaginatedMyPupilsHandler _getPaginatedMyPupilsQueryHandler;
    private readonly IMapper<PupilDtoWithSelectionStateDecorator, PupilViewModel> _mapper;

    public GetMyPupilsHandler(
        IGetPaginatedMyPupilsHandler getPaginatedMyPupilsQueryHandler,
        IMapper<PupilDtoWithSelectionStateDecorator, PupilViewModel> mapper)
    {
        ArgumentNullException.ThrowIfNull(getPaginatedMyPupilsQueryHandler);
        _getPaginatedMyPupilsQueryHandler = getPaginatedMyPupilsQueryHandler;

        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }

    public async Task<PupilsViewModel> HandleAsync(GetMyPupilsRequest request)
    {
        GetPaginatedMyPupilsRequest paginatedMyPupilsRequest = new(request.UserId, request.PresentationState);

        IEnumerable<PupilDtoWithSelectionStateDecorator> results =
            (await _getPaginatedMyPupilsQueryHandler.GetPaginatedPupilsAsync(paginatedMyPupilsRequest))
                .Select((pupil)
                    => PupilDtoWithSelectionStateDecorator.Create(
                        pupil,
                        isSelected: request.SelectionState.IsPupilSelected(pupil.UniquePupilNumber)));

        IEnumerable<PupilViewModel> pupils = results.Select(_mapper.Map);

        return PupilsViewModel.Create(pupils);
    }
}
