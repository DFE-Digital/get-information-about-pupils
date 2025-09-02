using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;

internal sealed class GetPaginatedMyPupilsHandler : IGetPaginatedMyPupilsHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _useCase;
    private readonly IMyPupilDtosPresentationHandler _presentationHandler;

    public GetPaginatedMyPupilsHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> useCase,
        IMyPupilDtosPresentationHandler presentationHandler)
    {
        ArgumentNullException.ThrowIfNull(useCase);
        _useCase = useCase;

        ArgumentNullException.ThrowIfNull(presentationHandler);
        _presentationHandler = presentationHandler;
    }

    public async Task<PaginatedMyPupilsResponse> HandleAsync(GetPaginatedMyPupilsRequest request)
    {
        GetMyPupilsRequest getPupilsRequest = new(request.UserId);
        GetMyPupilsResponse response = await _useCase.HandleRequestAsync(getPupilsRequest);
        MyPupilDtos results = _presentationHandler.Handle(response.MyPupils, request.PresentationState);
        return new(results);
    }
}

public record PaginatedMyPupilsResponse(MyPupilDtos Pupils);
