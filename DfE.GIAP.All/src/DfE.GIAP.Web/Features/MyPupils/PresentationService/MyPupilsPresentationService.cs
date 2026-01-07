using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.ClearSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsUseCase;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IMyPupilsPresentationModelHandler _presentationHandler;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly IClearMyPupilsPupilSelectionsHandler _clearMyPupilsPupilSelectionsCommandHandler;
    private readonly IMapper<MyPupilsModels, MyPupilsPresentationPupilModels> _mapPupilsToPresentablePupils;

    public MyPupilsPresentationService(
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsUseCase,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IMyPupilsPresentationModelHandler handler,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
        IClearMyPupilsPupilSelectionsHandler clearMyPupilsPupilSelectionsCommandHandler,
        IMapper<MyPupilsModels, MyPupilsPresentationPupilModels> mapper)
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
        _mapPupilsToPresentablePupils = mapper;
    }

    public async Task DeletePupilsAsync(
        string userId,
        IEnumerable<string> selectedPupilUpnsOnPage)
    {
        List<string> selectedPupilsToDelete = selectedPupilUpnsOnPage?.ToList() ?? [];

        // Enrich SelectedPupils with all other selected pupils
        selectedPupilsToDelete.AddRange(
            await GetSelectedPupilsAsync(userId));

        await _deletePupilsUseCase.HandleRequestAsync(
            new DeletePupilsFromMyPupilsRequest(
                UserId: userId,
                DeletePupilUpns: selectedPupilsToDelete.Distinct()));

        _clearMyPupilsPupilSelectionsCommandHandler.Handle();
    }

    public async Task<MyPupilsPresentationResponse> GetPupilsAsync(
        string userId,
        MyPupilsQueryRequestDto? query)
    {
        UserId id = new(userId);
        query ??= new();

        MyPupilsPupilSelectionState selectionState = _getMyPupilsStateProvider.GetPupilSelections() ??
            MyPupilsPupilSelectionState.CreateDefault();

        GetMyPupilsResponse response =
            await _getMyPupilsUseCase.HandleRequestAsync(
                new GetMyPupilsRequest(id.Value));

        MyPupilsPresentationQueryModel updatedPresentation = new(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            sortBy: query.SortField,
            sortDirection: query.SortDirection);

        MyPupilsPresentationPupilModels handledPupilModels =
            _presentationHandler.Handle(
                pupils: _mapPupilsToPresentablePupils.Map(response.MyPupils) ?? MyPupilsPresentationPupilModels.Create([]),
                updatedPresentation,
                selectionState);

        return new MyPupilsPresentationResponse()
        {
            MyPupils = handledPupilModels,
            PageNumber = updatedPresentation.Page.Value,
            SortedDirection = updatedPresentation.Sort.Direction switch
            {
                SortDirection.Ascending => "asc",
                SortDirection.Descending => "desc",
                _ => string.Empty
            },
            SortedField = updatedPresentation.Sort.Field,
            IsAnyPupilsSelected = selectionState.IsAnyPupilSelected,
            TotalPages =
                response.MyPupils == null || response.MyPupils.Count <= updatedPresentation.PageSize ? 1 :
                        (int)Math.Ceiling(response.MyPupils.Count / (double)updatedPresentation.PageSize)

        };
    }

    public async Task<IEnumerable<string>> GetSelectedPupilsAsync(string userId)
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

        return state.GetManualSelections();
    }
}
