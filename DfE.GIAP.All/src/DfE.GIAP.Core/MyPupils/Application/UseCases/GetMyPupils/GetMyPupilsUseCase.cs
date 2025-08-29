using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.Core.User.Application.Repository;

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
        User.Application.User user = await _userReadOnlyRepository.GetUserByIdAsync(userId);

        if (!user.UniquePupilNumbers.Any())
        {
            return new GetMyPupilsResponse(PupilDtos.Empty());
        }

        List<PupilDto> pupilDtos =
            (await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(user.UniquePupilNumbers))
                .Select(_mapPupilToPupilDtoMapper.Map)
                    .ToList();

        return new GetMyPupilsResponse(
            PupilDtos.Create(pupilDtos));
    }
}
