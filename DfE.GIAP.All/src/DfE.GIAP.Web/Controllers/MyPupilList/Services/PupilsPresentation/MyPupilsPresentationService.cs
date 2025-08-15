using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Extensions;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Response;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presenter;

public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    
    private readonly IPupilSelectionStateProvider _pupilSelectionStateProvider;
    private readonly ISessionProvider _sessionProvider;
    private readonly IMapper<MyPupilsFormStateRequestDto, PresentPupilsOptions> _mapPresentOptions;
    private readonly IMapper<PupilDtoWithPupilSelectionStateDto, PupilPresentatationViewModel> _mapToViewModel;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IEnumerable<IPupilDtoPresentationHandler> _pupilDtoPresentationHandlers;

    public MyPupilsPresentationService(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IEnumerable<IPupilDtoPresentationHandler> pupilPresentationHandlers,
        ISessionProvider sessionProvider,
        IMapper<MyPupilsFormStateRequestDto, PresentPupilsOptions> mapPresentOptions,
        IPupilSelectionStateProvider pupilSelectionStateProvider,
        IMapper<PupilDtoWithPupilSelectionStateDto, PupilPresentatationViewModel> mapToViewModel)
    {
        _getMyPupilsUseCase = getMyPupilsUseCase;
        _pupilDtoPresentationHandlers = pupilPresentationHandlers;
        _sessionProvider = sessionProvider;
        _mapPresentOptions = mapPresentOptions;
        _pupilSelectionStateProvider = pupilSelectionStateProvider;
        _mapToViewModel = mapToViewModel;
    }

    public async Task<GetPupilsForUserFromPresentationStateResponse> GetPupilsForUserFromPresentationStateAsync(string userId)
    {
        GetMyPupilsRequest getPupilsRequest = new(userId);
        GetMyPupilsResponse response = await _getMyPupilsUseCase.HandleRequestAsync(getPupilsRequest);

        PresentPupilsOptions options =
            _sessionProvider.ContainsSessionKey(nameof(PresentPupilsOptions)) ?
                _sessionProvider.GetSessionValueOrDefault<PresentPupilsOptions>(nameof(PresentPupilsOptions)) :
                    PresentPupilsOptions.Default;

        IEnumerable<PupilDto> results =
            _pupilDtoPresentationHandlers.Aggregate(
                response.Pupils,
                (current, handler) => handler.Handle(current, options));

        PupilsSelectionState pupilSelectionState = _pupilSelectionStateProvider.GetState();

        IEnumerable<PupilPresentatationViewModel> pupilSelectionStates
            = results
                .Select(t => new PupilDtoWithPupilSelectionStateDto(t, pupilSelectionState.IsPupilSelected(t.UniquePupilNumber)))
                .Select(_mapToViewModel.Map);


        return new GetPupilsForUserFromPresentationStateResponse(
            pupilSelectionStates,
            pupilSelectionStates.Any(t => t.IsSelected),
            pupilSelectionState.IsAllPupilsSelected,
            options.Page,
            options.SortBy,
            options.SortDirection.ToFormSortDirection());
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

        return pupilsSelectionState.SelectedPupils;
    }


    // TODO state pattern
    public void UpdatePresentationState(MyPupilsFormStateRequestDto updateStateRequest)
    {

        PresentPupilsOptions options = _mapPresentOptions.Map(updateStateRequest);

        _sessionProvider.SetSessionValue(nameof(PresentPupilsOptions), options);

        PupilsSelectionState selectionState = _pupilSelectionStateProvider.GetState();

        IEnumerable<string> currentPageOfPupils =
            updateStateRequest.CurrentPageOfPupils.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ReplaceLineEndings().Trim())
                .Where(UniquePupilNumberValidator.Validate); // TODO move out of Domain into cross-cutting or creating an application-validator?

        selectionState.AddPupils(currentPageOfPupils);

        if (updateStateRequest.SelectAll.HasValue && updateStateRequest.SelectAll.Value)
        {
            selectionState.SelectAll();

        }
        else if (updateStateRequest.SelectAll.HasValue && !updateStateRequest.SelectAll.Value)
        {
            selectionState.DeselectAll();
        }
        else
        {
            // No SelectAll specified — apply individual selections only
            IEnumerable<string> selected = updateStateRequest.SelectedPupils ?? Enumerable.Empty<string>();
            IEnumerable<string> deselected = currentPageOfPupils.Except(selected);

            foreach (string upn in selected)
            {
                selectionState.MarkSelected(upn);
            }

            foreach (string upn in deselected)
            {
                selectionState.MarkDeselected(upn);
            }

        }

        _pupilSelectionStateProvider.UpdateState(selectionState);        
    }

    public void ClearPresentationState() => _pupilSelectionStateProvider.GetState().Clear();
}

