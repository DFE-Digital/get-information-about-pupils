using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Extensions;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation;

public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    private readonly IPresentPupilOptionsProvider _presentPupilOptionsProvider;
    private readonly IPupilSelectionStateProvider _pupilSelectionStateProvider;
    private readonly IMapper<MyPupilsFormStateRequestDto, PresentPupilsOptions> _mapPresentOptions;
    private readonly IMapper<PupilDtoWithPupilSelectionStateDto, PupilPresentatationViewModel> _mapToViewModel;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IEnumerable<IPupilDtoPresentationHandler> _pupilDtoPresentationHandlers;

    public MyPupilsPresentationService(
        IPresentPupilOptionsProvider presentPupilOptionsProvider,
        IPupilSelectionStateProvider pupilSelectionStateProvider,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IEnumerable<IPupilDtoPresentationHandler> pupilPresentationHandlers,
        IMapper<MyPupilsFormStateRequestDto, PresentPupilsOptions> mapPresentOptions,
        IMapper<PupilDtoWithPupilSelectionStateDto, PupilPresentatationViewModel> mapToViewModel)
    {
        _pupilSelectionStateProvider = pupilSelectionStateProvider;
        _presentPupilOptionsProvider = presentPupilOptionsProvider;
        _getMyPupilsUseCase = getMyPupilsUseCase;
        _pupilDtoPresentationHandlers = pupilPresentationHandlers;
        _mapPresentOptions = mapPresentOptions;
        _mapToViewModel = mapToViewModel;

    }

    public async Task<GetPupilsForUserFromPresentationStateResponse> GetPupilsForUserFromPresentationStateAsync(string userId)
    {
        GetMyPupilsRequest getPupilsRequest = new(userId);
        GetMyPupilsResponse response = await _getMyPupilsUseCase.HandleRequestAsync(getPupilsRequest);

        PresentPupilsOptions presentPupilOptions = _presentPupilOptionsProvider.GetOptions();

        IEnumerable<PupilDto> results =
            _pupilDtoPresentationHandlers.Aggregate(
                response.Pupils,
                (current, handler) => handler.Handle(current, presentPupilOptions));

        PupilsSelectionState pupilSelectionState = _pupilSelectionStateProvider.GetState();

        IEnumerable<PupilPresentatationViewModel> pupilSelectionStates
            = results
                .Select(t => new PupilDtoWithPupilSelectionStateDto(t, pupilSelectionState.IsPupilSelected(t.UniquePupilNumber)))
                .Select(_mapToViewModel.Map);


        return new GetPupilsForUserFromPresentationStateResponse(
            pupilSelectionStates,
            pupilSelectionStates.Any(t => t.IsSelected),
            pupilSelectionState.IsAllPupilsSelected,
            presentPupilOptions.Page,
            presentPupilOptions.SortBy,
            presentPupilOptions.SortDirection.ToFormSortDirection());
    }

    public async Task<IEnumerable<string>> GetSelectedPupilsForUserAsync(string userId)
    {
        PupilsSelectionState pupilsSelectionState = _pupilSelectionStateProvider.GetState();

        if (pupilsSelectionState.IsAllPupilsSelected)
        {
            GetMyPupilsRequest getPupilsRequest = new(userId);
            GetMyPupilsResponse response = await _getMyPupilsUseCase.HandleRequestAsync(getPupilsRequest);
            return response.Pupils.Select(t => t.UniquePupilNumber);
        }

        return pupilsSelectionState.GetSelectedPupils();
    }


    // TODO state pattern
    public void UpdatePresentationState(MyPupilsFormStateRequestDto updateStateRequest)
    {
        _presentPupilOptionsProvider.SetOptions(
            options: _mapPresentOptions.Map(updateStateRequest));

        PupilsSelectionState selectionState = _pupilSelectionStateProvider.GetState();

        IEnumerable<string> currentPageOfPupils = updateStateRequest.ParseCurrentPageOfPupils();

        selectionState.AddPupils(currentPageOfPupils);

        if (updateStateRequest.SelectAll.HasValue && updateStateRequest.SelectAll.Value)
        {
            selectionState.SelectAllPupils();
            _pupilSelectionStateProvider.UpdateState(selectionState);
            return;
        }

        if (updateStateRequest.SelectAll.HasValue && !updateStateRequest.SelectAll.Value)
        {
            selectionState.DeselectAllPupils();
            _pupilSelectionStateProvider.UpdateState(selectionState);
            return;
        }

        // No SelectAll specified — apply individual selections only
        IEnumerable<string> selected = updateStateRequest.SelectedPupils ?? Enumerable.Empty<string>();
        IEnumerable<string> deselected = currentPageOfPupils.Except(selected);

        selectionState.UpdatePupilSelectionState(selected, isSelected: true);
        selectionState.UpdatePupilSelectionState(deselected, isSelected: false);
        _pupilSelectionStateProvider.UpdateState(selectionState);
    }

    public void ClearPresentationState() => _pupilSelectionStateProvider.GetState().ResetState();
}

