using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.AggregatePupilsForMyPupils.DataTransferObjects;
internal static class AzureIndexEntityExtensions
{
    internal static IEnumerable<DecoratedSearchIndexDto> DecorateDtoWithPupilType(
        this IEnumerable<AzureIndexEntity> azureIndexDtos,
        PupilType pupilType) =>
            azureIndexDtos?.Select(indexDto => new DecoratedSearchIndexDto(indexDto, pupilType)) ?? [];
}
