using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.MaskPupilIdentifier.AuthorisationContext;

namespace DfE.GIAP.Core.MyPupils.Application;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IGetMyPupilsHandler _handler;
    private readonly IMapper<AuthorisationContext, MyPupilsAuthorisationContext> _mapAuthorisationToMyPupilsAuthorisationContext;
    private readonly IMapper<MyPupil, MyPupilItem> _myPupilToMyPupilItemMapper;
    public GetMyPupilsUseCase(
        IGetMyPupilsHandler handler,
        IMapper<AuthorisationContext, MyPupilsAuthorisationContext> mapAuthorisationToMyPupilsAuthorisationContext,
        IMapper<MyPupil, MyPupilItem> myPupilToMyPupilItemMapper)
    {
        _handler = handler;
        _mapAuthorisationToMyPupilsAuthorisationContext = mapAuthorisationToMyPupilsAuthorisationContext;
        _myPupilToMyPupilItemMapper = myPupilToMyPupilItemMapper;
    }
    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        // Hydrate UserAggregate via Repository
        // Map User.Pupils to ReadOnly MyPupilItem
        // TODO consider actions to delete will need identifiers attached to each item (what is it currently doing?)
        MyPupilsAuthorisationContext context = _mapAuthorisationToMyPupilsAuthorisationContext.Map(request.authContext);
        IEnumerable<MyPupil> pupils = await _handler.Get(context);
        IEnumerable<MyPupilItem> outputPupils = pupils.Select(_myPupilToMyPupilItemMapper.Map);
        return new GetMyPupilsResponse(outputPupils);
    }
}

public record GetMyPupilsRequest(AuthorisationContext authContext) : IUseCaseRequest<GetMyPupilsResponse>;

public record GetMyPupilsResponse(IEnumerable<MyPupilItem> MyPupils);
public record AuthorisationContext(); // Containing UserIdentifier and Web implements 
public record MyPupilItem(); // IMapper<DomainMyPupil -> this> 
