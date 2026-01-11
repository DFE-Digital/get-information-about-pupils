using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
public sealed record AzureIndexEntityWithPupilType(
    AzureIndexEntity SearchIndexDto,
    PupilType PupilType);
