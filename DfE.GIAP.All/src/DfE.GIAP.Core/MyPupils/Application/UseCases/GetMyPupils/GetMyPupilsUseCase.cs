using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserAggregateRootFactory _userAggregateFactory;
    private readonly IMapper<Pupil, PupilItemPresentationModel> _myPupilToMyPupilItemMapper;
    public GetMyPupilsUseCase(
        IMapper<Pupil, PupilItemPresentationModel> myPupilToMyPupilItemMapper,
        IUserAggregateRootFactory userAggregateFactory)
    {
        _myPupilToMyPupilItemMapper = myPupilToMyPupilItemMapper;
        _userAggregateFactory = userAggregateFactory;
    }
    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        UserAggregateRoot userAggregate = await _userAggregateFactory.CreateAsync(request.AuthContext);
        IEnumerable<PupilItemPresentationModel> outputPupils =
            userAggregate.GetMyPupils()
                .Select(_myPupilToMyPupilItemMapper.Map);

        return new GetMyPupilsResponse(outputPupils);
    }
}
