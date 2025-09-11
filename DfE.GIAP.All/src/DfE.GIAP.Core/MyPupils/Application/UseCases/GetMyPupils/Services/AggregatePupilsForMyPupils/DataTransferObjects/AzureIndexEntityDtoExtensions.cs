using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Dto;
internal static class AzureIndexEntityDtoExtensions
{
    internal static IEnumerable<DecoratedSearchIndexDto> ToDecoratedSearchIndexDto(
        this IEnumerable<AzureIndexEntity> azureIndexDtos,
        PupilType pupilType)
            => azureIndexDtos?.Select(t => new DecoratedSearchIndexDto(t, pupilType)) ?? [];
}
