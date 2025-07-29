using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupilsDomainService.Mapper;
internal record MappableLearner(
    UniquePupilNumber uniquePupilNumber,
    Learner Learner,
    PupilType PupilType);
