using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserAggregateRootFactory _userAggregateRootFactory;

    public GetMyPupilsUseCase(
        IUserAggregateRootFactory userAggregateRootFactory)
    {
        _userAggregateRootFactory = userAggregateRootFactory;
    }

    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        PupilQuery pupilQuery =
            request.Options == null ?
                PupilQuery.Default :
                    new(
                        request.Options.Page,
                        request.Options.Order.Field,
                        request.Options.Order.Direction);

        UserAggregateRoot aggregateRoot = await _userAggregateRootFactory.CreateAsync(request.AuthContext, pupilQuery);

        IEnumerable<PupilDto> outputPupils = aggregateRoot.GetMyPupils();

        return new GetMyPupilsResponse(outputPupils);
    }
}
