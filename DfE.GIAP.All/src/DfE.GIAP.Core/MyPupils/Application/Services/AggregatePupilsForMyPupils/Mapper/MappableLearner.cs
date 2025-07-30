using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
internal record MappableLearner(
    UniquePupilNumber uniquePupilNumber,
    Learner Learner,
    PupilType PupilType);
