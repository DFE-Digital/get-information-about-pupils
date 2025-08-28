using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

public sealed class GetPaginatedMyPupilsHandler : IGetPaginatedMyPupilsHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IPupilDtosPresentationHandler _presentationHandler;

    public GetPaginatedMyPupilsHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IPupilDtosPresentationHandler presentationHandler)
    {
        _presentationHandler = presentationHandler;
        _useCase = useCase;
    }

    public async Task<PupilDtos> GetPaginatedPupilsAsync(GetPaginatedMyPupilsRequest request)
    {
        GetMyPupilsRequest getPupilsRequest = new(request.UserId);
        GetMyPupilsResponse response = await _useCase.HandleRequestAsync(getPupilsRequest);

        PupilDtos results = _presentationHandler.Handle(response.PupilDtos, request.PresentationState);

        return results;
    }
}
