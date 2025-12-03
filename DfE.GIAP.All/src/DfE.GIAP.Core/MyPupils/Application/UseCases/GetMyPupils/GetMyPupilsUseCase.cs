using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IMyPupilsReadOnlyRepository _myPupilsReadOnlyRepository;
    private readonly IAggregatePupilsForMyPupilsApplicationService _aggregatePupilsForMyPupilsApplicationService;
    private readonly IMapper<Pupil, MyPupilModel> _mapPupilToPupilDtoMapper;

    public GetMyPupilsUseCase(
        IMyPupilsReadOnlyRepository myPupilsReadOnlyRepository,
        IAggregatePupilsForMyPupilsApplicationService aggregatePupilsForMyPupilsApplicationService,
        IMapper<Pupil, MyPupilModel> mapPupilToPupilDtoMapper)
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
        MyPupilsId id = new(request.MyPupilsId);

        MyPupilsAggregate? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(id);

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

        List<MyPupilModel> myPupilsDtos =
            (await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(myPupilUniquePupilNumbers))
                .Select(_mapPupilToPupilDtoMapper.Map)
                .ToList();

        return new GetMyPupilsResponse(
            MyPupils: MyPupilsModel.Create(myPupilsDtos));
    }
}
