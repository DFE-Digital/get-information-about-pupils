using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.Users.Application.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;
public sealed class MyPupilsPresentationService : IMyPupilsPresentationService
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IMyPupilsPresentationModelHandler _presentationHandler;
    private readonly IGetMyPupilsStateQueryHandler _getMyPupilsStateProvider;
    private readonly IMapper<MyPupilsModel, MyPupilsPresentationPupilModels> _mapper;

    public MyPupilsPresentationService(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IMyPupilsPresentationModelHandler handler,
        IGetMyPupilsStateQueryHandler getMyPupilsStateProvider,
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

    public async Task<MyPupilsPresentationResponse> GetPupils(string userId)
    {
        UserId id = new(userId);

        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        GetMyPupilsResponse response =
            await _useCase.HandleRequestAsync(
                new GetMyPupilsRequest(id.Value));

        MyPupilsPresentationPupilModels handledPupilModels =
            _presentationHandler.Handle(
                pupils: _mapper.Map(response.MyPupils),
                state);

        return MyPupilsPresentationResponse.Create(handledPupilModels, state);
    }

    public async Task<IEnumerable<string>> GetSelectedPupilUniquePupilNumbers(string userId)
    {
        MyPupilsState state = _getMyPupilsStateProvider.GetState();

        if (state.SelectionState.Mode == SelectionMode.All)
        {
            GetMyPupilsResponse response =
                await _useCase.HandleRequestAsync(
                    new GetMyPupilsRequest(userId));

            return response.MyPupils.Identifiers.Except(
                    state.SelectionState.GetDeselectedExceptions());
        }

        return state.SelectionState.GetExplicitSelections();
    }
}
