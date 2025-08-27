using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

public sealed class GetPaginatedMyPupilsHandler : IGetPaginatedMyPupilsHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IEnumerable<IPupilPresentationHandler> _presentationHandler;

    public GetPaginatedMyPupilsHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IEnumerable<IPupilPresentationHandler> presentationHandler)
    {
        _presentationHandler = presentationHandler;
        _useCase = useCase;
    }

    public async Task<IEnumerable<PupilDto>> GetPaginatedPupilsAsync(GetPaginatedMyPupilsRequest request)
    {
        GetMyPupilsRequest getPupilsRequest = new(request.UserId);
        GetMyPupilsResponse response = await _useCase.HandleRequestAsync(getPupilsRequest);

        IEnumerable<PupilDto> results =
            _presentationHandler.Aggregate(
                seed: response.Pupils,
                func: (currentPupils, handler) => handler.Handle(currentPupils, request.PresentationState));

        return results;
    }
}

