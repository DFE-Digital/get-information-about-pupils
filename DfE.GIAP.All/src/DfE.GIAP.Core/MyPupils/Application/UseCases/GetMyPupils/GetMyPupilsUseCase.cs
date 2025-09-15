using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

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
        Domain.AggregateRoot.MyPupils? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(request.UserId);

        if (myPupils is null || myPupils.GetMyPupils().Count == 0)
        {
            MyPupilsModel emptyPupils = MyPupilsModel.Create(pupils: []);
            return new GetMyPupilsResponse(emptyPupils);
        }

        MyPupilsModel aggregatedPupilDtos =
            MyPupilsModel.Create(
                pupils: (await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(UniquePupilNumbers.Create(uniquePupilNumbers: myPupils.GetMyPupils())))
                    .Select(_mapPupilToPupilDtoMapper.Map)
                        .ToList());

        return new GetMyPupilsResponse(aggregatedPupilDtos);
    }
}
