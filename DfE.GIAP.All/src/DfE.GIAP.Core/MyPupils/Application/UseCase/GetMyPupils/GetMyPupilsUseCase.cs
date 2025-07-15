using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Domain.User;
using DfE.GIAP.Core.Common.Domain.User.Repository;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IMapper<IAuthorisationContext, MyPupilsAuthorisationContext> _mapApplicationAuthorisationToMyPupilsAuthorisationContext;
    private readonly IMapper<MyPupil, MyPupilItem> _myPupilToMyPupilItemMapper;
    public GetMyPupilsUseCase(
        IMapper<IAuthorisationContext, MyPupilsAuthorisationContext> mapAuthorisationToMyPupilsAuthorisationContext,
        IMapper<MyPupil, MyPupilItem> myPupilToMyPupilItemMapper,
        IUserReadOnlyRepository userReadOnlyRepository)
    {
        _mapApplicationAuthorisationToMyPupilsAuthorisationContext = mapAuthorisationToMyPupilsAuthorisationContext;
        _myPupilToMyPupilItemMapper = myPupilToMyPupilItemMapper;
        _userReadOnlyRepository = userReadOnlyRepository;
    }
    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        MyPupilsAuthorisationContext context = _mapApplicationAuthorisationToMyPupilsAuthorisationContext.Map(request.authContext);

        UserAggregateRoot? user = await _userReadOnlyRepository.GetByIdAsync(
            new UserIdentifier(request.authContext.UserId),
            context);

        IEnumerable<MyPupilItem> outputPupils =
            user.GetMyPupils()
                .Select(_myPupilToMyPupilItemMapper.Map);

        return new GetMyPupilsResponse(outputPupils);
    }
}

public record GetMyPupilsRequest(IAuthorisationContext authContext) : IUseCaseRequest<GetMyPupilsResponse>;
public record GetMyPupilsResponse(IEnumerable<MyPupilItem> MyPupils);

public record MyPupilItem() { } // IMapper<MyPupil -> this> 


