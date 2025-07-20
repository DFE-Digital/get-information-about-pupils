using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserAggregateRootFactory _userAggregateFactory;
    public GetMyPupilsUseCase(IUserAggregateRootFactory userAggregateFactory)
    {
        _userAggregateFactory = userAggregateFactory;
    }

    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        UserAggregateRoot userAggregate = await _userAggregateFactory.CreateAsync(request.AuthContext);
        IEnumerable<PupilItemPresentationModel> outputPupils =
            userAggregate.GetMyPupils()
                .Select(pupil => new PupilItemPresentationModel(pupil));

        return new GetMyPupilsResponse(outputPupils);
    }
}
