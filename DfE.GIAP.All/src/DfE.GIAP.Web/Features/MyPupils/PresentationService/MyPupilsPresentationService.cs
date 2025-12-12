using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Handlers;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsUseCase;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IMyPupilsPresentationModelHandler _presentationHandler;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly IClearMyPupilsPupilSelectionsCommandHandler _clearMyPupilsPupilSelectionsCommandHandler;
    private readonly IMapper<MyPupilsModel, MyPupilsPresentationPupilModels> _mapper;

    public MyPupilsPresentationService(
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsUseCase,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IMyPupilsPresentationModelHandler handler,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
        IClearMyPupilsPupilSelectionsCommandHandler clearMyPupilsPupilSelectionsCommandHandler,
        IMapper<MyPupilsModel, MyPupilsPresentationPupilModels> mapper)
    {
        ArgumentNullException.ThrowIfNull(getMyPupilsUseCase);
        _getMyPupilsUseCase = getMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(deletePupilsUseCase);
        _deletePupilsUseCase = deletePupilsUseCase;

        ArgumentNullException.ThrowIfNull(handler);
        _presentationHandler = handler;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(clearMyPupilsPupilSelectionsCommandHandler);
        _clearMyPupilsPupilSelectionsCommandHandler = clearMyPupilsPupilSelectionsCommandHandler;
        
        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }

    public async Task DeletePupils(
        string userId,
        IEnumerable<string> selectedPupilUpnsOnPage)
    {
        List<string> selectedPupils = selectedPupilUpnsOnPage.ToList();

        // Enrich SelectedPupils with all other selected pupils
        selectedPupils.AddRange(
            await GetSelectedPupilUniquePupilNumbers(userId));

        await _deletePupilsUseCase.HandleRequestAsync(
            new DeletePupilsFromMyPupilsRequest(
                UserId: userId,
                DeletePupilUpns: selectedPupils.Distinct()));

        _clearMyPupilsPupilSelectionsCommandHandler.Handle();
    }

    public async Task<MyPupilsPresentationResponse> GetPupils(
        string userId,
        MyPupilsQueryRequestDto query)
    {
        UserId id = new(userId);

        MyPupilsPupilSelectionState selectionState = _getMyPupilsStateProvider.GetPupilSelections();

        GetMyPupilsResponse response =
            await _getMyPupilsUseCase.HandleRequestAsync(
                new GetMyPupilsRequest(id.Value));

        MyPupilsPresentationQueryModel updatedPresentationModel = new(query.PageNumber, query.SortField, query.SortDirection);

        MyPupilsState currentState = new(updatedPresentationModel, selectionState);

        MyPupilsPresentationPupilModels handledPupilModels =
            _presentationHandler.Handle(
                pupils: _mapper.Map(response.MyPupils),
                state: currentState);

        return MyPupilsPresentationResponse.Create(
            handledPupilModels,
            updatedPresentationModel,
            selectionState);
    }

    public async Task<IEnumerable<string>> GetSelectedPupilUniquePupilNumbers(string userId)
    {
        MyPupilsPupilSelectionState state = _getMyPupilsStateProvider.GetPupilSelections();

        if (state.Mode == SelectionMode.All)
        {
            GetMyPupilsResponse response =
                await _getMyPupilsUseCase.HandleRequestAsync(
                    new GetMyPupilsRequest(userId));

            return response.MyPupils.Identifiers.Except(
                    state.GetDeselectedExceptions());
        }

        return state.GetExplicitSelections();
    }
}
