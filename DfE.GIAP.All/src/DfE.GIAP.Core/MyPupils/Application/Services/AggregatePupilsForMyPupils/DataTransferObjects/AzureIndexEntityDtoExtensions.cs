using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
internal static class AzureIndexEntityDtoExtensions
{
    internal static IEnumerable<AzureIndexEntityWithPupilType> ToDecoratedSearchIndexDto(
        this IEnumerable<AzureIndexEntity> azureIndexDtos,
        PupilType pupilType)
            => azureIndexDtos?.Select(t => new AzureIndexEntityWithPupilType(t, pupilType)) ?? [];
}
