using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IMyPupilsPresentationModelHandler _presentationHandler;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly IMapper<MyPupilsModel, MyPupilsPresentationPupilModels> _mapper;

    public MyPupilsPresentationService(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IMyPupilsPresentationModelHandler handler,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
        IMapper<MyPupilsModel, MyPupilsPresentationPupilModels> mapper)
    {
        ArgumentNullException.ThrowIfNull(useCase);
        _useCase = useCase;

        ArgumentNullException.ThrowIfNull(handler);
        _presentationHandler = handler;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }

    public async Task<MyPupilsPresentationResponse> GetPupils(
        string userId,
        int pageNumber,
        string sortField,
        string sortDirection)
    {
        UserId id = new(userId);

        MyPupilsPupilSelectionState selectionState = _getMyPupilsStateProvider.GetPupilSelections();

        GetMyPupilsResponse response =
            await _useCase.HandleRequestAsync(
                new GetMyPupilsRequest(id.Value));

        MyPupilsPresentationQueryModel updatedPresentationModel =
            new(
                pageNumber,
                sortField,
                sortDirection == "asc" ? SortDirection.Ascending : SortDirection.Descending);

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
                await _useCase.HandleRequestAsync(
                    new GetMyPupilsRequest(userId));

            return response.MyPupils.Identifiers.Except(
                    state.GetDeselectedExceptions());
        }

        return state.GetExplicitSelections();
    }
}
