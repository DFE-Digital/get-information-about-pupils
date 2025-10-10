using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
internal static class AzureIndexEntityDtoExtensions
{
    internal static IEnumerable<DecoratedSearchIndexDto> ToDecoratedSearchIndexDto(
        this IEnumerable<AzureIndexEntity> azureIndexDtos,
        PupilType pupilType)
            => azureIndexDtos?.Select(t => new DecoratedSearchIndexDto(t, pupilType)) ?? [];
}
