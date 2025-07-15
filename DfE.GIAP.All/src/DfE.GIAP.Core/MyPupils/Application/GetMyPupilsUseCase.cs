using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.MyPupils.Application;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    public Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        // Hydrate UserAggregate via Repository
        // Map User.Pupils to ReadOnly MyPupilItem
        // TODO consider actions to delete will need identifiers attached to each item (what is it currently doing?)
        return Task.FromResult(new GetMyPupilsResponse([]));
    }
}

public record GetMyPupilsRequest(AuthorisationContext authContext) : IUseCaseRequest<GetMyPupilsResponse>;

public record GetMyPupilsResponse(IEnumerable<MyPupilItem> item);
public record AuthorisationContext(); // Containing UserIdentifier and Web implements 
public record MyPupilItem(); // IMapper<DomainMyPupil -> this> 
