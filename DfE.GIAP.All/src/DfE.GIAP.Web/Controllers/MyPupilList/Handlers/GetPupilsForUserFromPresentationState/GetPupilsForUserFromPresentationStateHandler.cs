using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Extensions;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.PupilDtoPresentationHandlers;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Mapper;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState;

public sealed class GetPupilsForUserFromPresentationStateHandler : IGetPupilsForUserFromPresentationStateHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IMyPupilsPresentationStateProvider _pupilsPresentationStateProvider;
    private readonly IPupilSelectionStateProvider _pupilSelectionStateProvider;
    private readonly IEnumerable<IPupilDtoPresentationHandler> _pupilDtoPresentationHandlers;
    private readonly IMapper<PupilDtoWithSelectionState, PupilPresentationViewModel> _mapper;

    public GetPupilsForUserFromPresentationStateHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IEnumerable<IPupilDtoPresentationHandler> pupilDtoPresentationHandlers,
        IPupilSelectionStateProvider pupilSelectionStateProvider,
        IMyPupilsPresentationStateProvider pupilsPresentationOptionsProvider,
        IMapper<PupilDtoWithSelectionState, PupilPresentationViewModel> mapper)
    {
        _getMyPupilsUseCase = getMyPupilsUseCase;
        _pupilDtoPresentationHandlers = pupilDtoPresentationHandlers;
        _pupilSelectionStateProvider = pupilSelectionStateProvider;
        _pupilsPresentationStateProvider = pupilsPresentationOptionsProvider;
        _mapper = mapper;
    }

    public async Task<GetPupilsForUserFromPresentationStateResponse> GetPupilsForUserFromPresentationStateAsync(string userId)
    {
        GetMyPupilsRequest getPupilsRequest = new(userId);
        GetMyPupilsResponse response = await _getMyPupilsUseCase.HandleRequestAsync(getPupilsRequest);

        MyPupilsPresentationState presentPupilOptions = _pupilsPresentationStateProvider.Get();

        IEnumerable<PupilDto> results =
            _pupilDtoPresentationHandlers.Aggregate(
                response.Pupils,
                (current, handler) => handler.Handle(current, presentPupilOptions));

        IPupilsSelectionState pupilSelectionState = _pupilSelectionStateProvider.GetState();

        IEnumerable<PupilDtoWithSelectionState> pupilDtosWithSelectionState =
            results.Select((pupilDto)
                => new PupilDtoWithSelectionState(
                    pupilDto,
                    pupilSelectionState.IsPupilSelected(pupilDto.UniquePupilNumber)));

        IEnumerable<PupilPresentationViewModel> pupilPresentationViewModels = pupilDtosWithSelectionState.Select(_mapper.Map);

        return new GetPupilsForUserFromPresentationStateResponse(
            pupilPresentationViewModels,
            pupilPresentationViewModels.Any(t => t.IsSelected),
            pupilSelectionState.IsAllPupilsSelected,
            presentPupilOptions.Page,
            presentPupilOptions.SortBy,
            presentPupilOptions.SortDirection.ToFormSortDirection());
    }
}
