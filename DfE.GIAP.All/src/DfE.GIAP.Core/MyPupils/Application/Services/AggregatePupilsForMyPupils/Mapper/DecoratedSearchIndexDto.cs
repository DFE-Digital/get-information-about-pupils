using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
public sealed record DecoratedSearchIndexDto(
    AzureIndexEntity SearchIndexDto,
    PupilType PupilType);
