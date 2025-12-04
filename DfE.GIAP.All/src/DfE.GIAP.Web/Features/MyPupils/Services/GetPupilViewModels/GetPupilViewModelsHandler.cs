using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils.PresentationHandlers;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;

internal sealed class GetPupilViewModelsHandler : IGetPupilViewModelsHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IMapper<PupilsSelectionContext, PupilsViewModel> _mapToViewModel;
    private readonly IMyPupilDtosPresentationHandler _presentationHandler;

    public GetPupilViewModelsHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IMyPupilDtosPresentationHandler presentationHandler,
        IMapper<PupilsSelectionContext, PupilsViewModel> mapToViewModel)
    {
        ArgumentNullException.ThrowIfNull(useCase);
        _useCase = useCase;

        ArgumentNullException.ThrowIfNull(presentationHandler);
        _presentationHandler = presentationHandler;

        ArgumentNullException.ThrowIfNull(mapToViewModel);
        _mapToViewModel = mapToViewModel;    
    }

    public async Task<PupilsViewModel> GetPupilsAsync(GetPupilViewModelsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        GetMyPupilsResponse response =
            await _useCase.HandleRequestAsync(
                new GetMyPupilsRequest(request.UserId));

        MyPupilsModel outputtedPupilModels = _presentationHandler.Handle(response.MyPupils, request.State.PresentationState);

        PupilsViewModel viewModel =
            _mapToViewModel.Map(
                new PupilsSelectionContext(
                    outputtedPupilModels, request.State.SelectionState));

        return viewModel;
    }
}
