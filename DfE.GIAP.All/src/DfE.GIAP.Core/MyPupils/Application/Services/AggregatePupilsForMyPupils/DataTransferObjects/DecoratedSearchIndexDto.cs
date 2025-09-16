using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
public sealed record DecoratedSearchIndexDto(
    AzureIndexEntity SearchIndexDto,
    PupilType PupilType);
