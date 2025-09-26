using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
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
        MyPupilsId id = new(request.UserId);

        Domain.AggregateRoot.MyPupils? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(id);

        if (myPupils is null || myPupils.HasNoPupils)
        {
            return
                new GetMyPupilsResponse(
                    MyPupilsModel.Create(
                        pupils: []));
        }

        UniquePupilNumbers myPupilUniquePupilNumbers =
            UniquePupilNumbers.Create(
                myPupils.GetMyPupils());

        MyPupilsModel aggregatedPupils =
            MyPupilsModel.Create(
                pupils: (await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(myPupilUniquePupilNumbers))
                        .Select(_mapPupilToPupilDtoMapper.Map)
                            .ToList());

        return new GetMyPupilsResponse(aggregatedPupils);
    }
}
