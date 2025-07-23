using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IMapper<IAuthorisationContext, PupilAuthorisationContext> _authContextMapper;
    private readonly IMapper<MyPupilsQueryOptions?, PupilSelectionDomainCriteria> _mapPupilQueryToPupilSelectionCriteria;
    private readonly IAggregatePupilsForMyPupilsDomainService _aggregatePupilsForMyPupilsDomainService;

    public GetMyPupilsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IMapper<IAuthorisationContext, PupilAuthorisationContext> authContextMapper,
        IAggregatePupilsForMyPupilsDomainService aggregatePupilsForMyPupilsDomainService,
        IMapper<MyPupilsQueryOptions?, PupilSelectionDomainCriteria> mapPupilQueryToPupilSelectionCriteria)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _authContextMapper = authContextMapper;
        _aggregatePupilsForMyPupilsDomainService = aggregatePupilsForMyPupilsDomainService;
        _mapPupilQueryToPupilSelectionCriteria = mapPupilQueryToPupilSelectionCriteria;
    }

    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        UserId userId = new(request.AuthContext.UserId);

        User.Application.Repository.UserReadRepository.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        PupilAuthorisationContext pupilAuthorisationContext = _authContextMapper.Map(request.AuthContext);

        UserAggregateRoot userAggregateRoot = new(
            userId,
            user.PupilIds,
            _aggregatePupilsForMyPupilsDomainService);

        PupilSelectionDomainCriteria pupilSelectionCriteria = _mapPupilQueryToPupilSelectionCriteria.Map(request.Options);

        IEnumerable<PupilDto> pupilDtos = await userAggregateRoot.GetMyPupils(
            pupilAuthorisationContext,
            pupilSelectionCriteria);

        return new GetMyPupilsResponse(pupilDtos);
    }
}


internal sealed class MapMyPupilsQueryOptionsToPupilSelectionCriteriaMapper : IMapper<MyPupilsQueryOptions?, PupilSelectionDomainCriteria>
{
    public PupilSelectionDomainCriteria Map(MyPupilsQueryOptions? input)
    {
        if(input is null)
        {
            return PupilSelectionDomainCriteria.Default;
        }

        return new PupilSelectionDomainCriteria(
            SortBy: input.Order.Field,
            Direction: input.Order.Direction switch
            {
                Direction.Descending => SortDirection.Descending,
                _ => SortDirection.Ascending
            },
            Page: input.Page.Value);
    }
}
