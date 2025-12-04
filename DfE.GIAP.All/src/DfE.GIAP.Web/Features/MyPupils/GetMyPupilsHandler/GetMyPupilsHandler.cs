using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsHandler.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels.Mapper;

namespace DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;

internal sealed class GetMyPupilsHandler : IGetMyPupilsHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IMapper<PupilsSelectionContext, MyPupilsPresentationModel> _mapToViewModel;
    private readonly IMyPupilsModelPresentationHandler _presentationHandler;

    public GetMyPupilsHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IMyPupilsModelPresentationHandler presentationHandler,
        IMapper<PupilsSelectionContext, MyPupilsPresentationModel> mapToViewModel)
    {
        ArgumentNullException.ThrowIfNull(useCase);
        _useCase = useCase;

        ArgumentNullException.ThrowIfNull(presentationHandler);
        _presentationHandler = presentationHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;
    }

    public async Task<MyPupilsResponse> GetPupilsAsync(MyPupilsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        GetMyPupilsResponse response =
            await _useCase.HandleRequestAsync(
                new GetMyPupilsRequest(request.UserId));

        MyPupilsModel outputtedPupilModels = _presentationHandler.Handle(response.MyPupils, request.State.PresentationState);

        MyPupilsPresentationModel viewModel =
            _mapToViewModel.Map(
                new PupilsSelectionContext(
                    outputtedPupilModels, request.State.SelectionState));

        return new MyPupilsResponse(viewModel);
    }
}
