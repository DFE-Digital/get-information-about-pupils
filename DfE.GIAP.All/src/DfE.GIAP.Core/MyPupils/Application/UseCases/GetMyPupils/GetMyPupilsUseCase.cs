using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IMyPupilsReadOnlyRepository _myPupilsReadOnlyRepository;
    private readonly IAggregatePupilsForMyPupilsApplicationService _aggregatePupilsForMyPupilsApplicationService;
    private readonly IMapper<Pupil, MyPupilDto> _mapPupilToPupilDtoMapper;

    public GetMyPupilsUseCase(
        IMyPupilsReadOnlyRepository myPupilsReadOnlyRepository,
        IAggregatePupilsForMyPupilsApplicationService aggregatePupilsForMyPupilsApplicationService,
        IMapper<Pupil, MyPupilDto> mapPupilToPupilDtoMapper)
    {
        ArgumentNullException.ThrowIfNull(myPupilsReadOnlyRepository);
        _myPupilsReadOnlyRepository = myPupilsReadOnlyRepository;

        ArgumentNullException.ThrowIfNull(aggregatePupilsForMyPupilsApplicationService);
        _aggregatePupilsForMyPupilsApplicationService = aggregatePupilsForMyPupilsApplicationService;

        ArgumentNullException.ThrowIfNull(mapPupilToPupilDtoMapper);
        _mapPupilToPupilDtoMapper = mapPupilToPupilDtoMapper;
    }

    public async Task<GetMyPupilsResponse> HandleRequestAsync(GetMyPupilsRequest request)
    {
        UserId userId = new(request.UserId);
        Repositories.MyPupils? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(userId);

        if (myPupils is null || myPupils.Pupils.IsEmpty)
        {
            MyPupilDtos emptyPupils = MyPupilDtos.Create(pupils: []);
            return new GetMyPupilsResponse(emptyPupils);
        }

        MyPupilDtos aggregatedPupilDtos =
            MyPupilDtos.Create(
                pupils: (await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(myPupils.Pupils))
                            .Select(_mapPupilToPupilDtoMapper.Map)
                                .ToList());

        return new GetMyPupilsResponse(aggregatedPupilDtos);
    }
}
