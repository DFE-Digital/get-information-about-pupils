using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Services;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IAggregatePupilsForMyPupilsApplicationService _aggregatePupilsForMyPupilsApplicationService;
    private readonly IMapper<Pupil, PupilDto> _mapPupilToPupilDtoMapper;

    public GetMyPupilsUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IAggregatePupilsForMyPupilsApplicationService aggregatePupilsForMyPupilsApplicationService,
        IMapper<Pupil, PupilDto> mapPupilToPupilDtoMapper)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _aggregatePupilsForMyPupilsApplicationService = aggregatePupilsForMyPupilsApplicationService;
        _mapPupilToPupilDtoMapper = mapPupilToPupilDtoMapper;
    }

    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        UserId userId = new(request.UserId);

        User.Application.Repository.UserReadRepository.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        IEnumerable<Pupil> pupils = await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(user.UniquePupilNumbers, request.Options);

        IEnumerable<PupilDto> pupilDtos = pupils.Select(_mapPupilToPupilDtoMapper.Map);

        return new GetMyPupilsResponse(pupilDtos);
    }
}


internal sealed class MapPupilToPupilDtoMapper : IMapper<Pupil, PupilDto>
{
    public PupilDto Map(Pupil pupil)
    {
        return new PupilDto()
        {
            UniquePupilNumber = pupil.Identifier.Value,
            DateOfBirth = pupil.DateOfBirth?.ToString() ?? string.Empty,
            Forename = pupil.Forename,
            Surname = pupil.Surname,
            Sex = pupil.Sex,
            IsPupilPremium = pupil.IsOfPupilType(PupilType.PupilPremium),
            LocalAuthorityCode = pupil.LocalAuthorityCode,
        };
    }
}
