using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class GetMyPupilsUseCase : IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>
{
    private readonly IMyPupilsReadOnlyRepository _myPupilsReadOnlyRepository;
    private readonly IAggregatePupilsForMyPupilsApplicationService _aggregatePupilsForMyPupilsApplicationService;
    private readonly IMapper<Pupil, MyPupilsModel> _mapPupilToPupilDtoMapper;

    public GetMyPupilsUseCase(
        IMyPupilsReadOnlyRepository myPupilsReadOnlyRepository,
        IAggregatePupilsForMyPupilsApplicationService aggregatePupilsForMyPupilsApplicationService,
        IMapper<Pupil, MyPupilsModel> mapPupilToPupilDtoMapper)
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

        MyPupilsId id = new(userId);

        MyPupilsAggregate? myPupils = await _myPupilsReadOnlyRepository.GetMyPupilsOrDefaultAsync(id);

        if (myPupils is null || myPupils.HasNoPupils)
        {
            return
                new GetMyPupilsResponse(
                    MyPupilsModels.Create(
                        pupils: []));
        }

        UniquePupilNumbers myPupilUniquePupilNumbers =
            UniquePupilNumbers.Create(
                myPupils.GetMyPupils());

        List<MyPupilsModel> myPupilsDtos =
            (await _aggregatePupilsForMyPupilsApplicationService.GetPupilsAsync(myPupilUniquePupilNumbers))
                .Select(_mapPupilToPupilDtoMapper.Map)
                .ToList();

        return new GetMyPupilsResponse(
            MyPupils: MyPupilsModels.Create(myPupilsDtos));
    }
}
