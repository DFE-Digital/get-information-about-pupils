using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.User.Domain.Aggregate;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserAggregateFactory _userAggregateFactory;
    private readonly IMapper<MyPupil, MyPupilItem> _myPupilToMyPupilItemMapper;
    public GetMyPupilsUseCase(
        IMapper<MyPupil, MyPupilItem> myPupilToMyPupilItemMapper,
        IUserAggregateFactory userAggregateFactory)
    {
        _myPupilToMyPupilItemMapper = myPupilToMyPupilItemMapper;
        _userAggregateFactory = userAggregateFactory;
    }
    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        UserIdentifier userIdentifier = new(request.AuthContext.UserId);
        UserAggregateRoot userAggregate = await _userAggregateFactory.Create(request.AuthContext);
        IEnumerable<MyPupilItem> outputPupils = userAggregate.GetMyPupils().Select(_myPupilToMyPupilItemMapper.Map);
        return new GetMyPupilsResponse(outputPupils);
    }
}

public record GetMyPupilsRequest(IAuthorisationContext AuthContext) : IUseCaseRequest<GetMyPupilsResponse>;
public record GetMyPupilsResponse(IEnumerable<MyPupilItem> MyPupils);


public record MyPupilItem() { } // IMapper<MyPupil -> this> 

internal sealed class MapMyPupilToMyPupilItem : IMapper<MyPupil, MyPupilItem>
{
    public MyPupilItem Map(MyPupil input)
    {
        // TODO surfaced to presentation
        return new();
    }
}
