using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.User.Infrastructure.Repository;

namespace DfE.GIAP.Core.IntegrationTests.MyPupils.Extensions;
internal static class AzureIndexEntityExtensions
{
    internal static List<MyPupilItemDto> MapToMyPupilsItemDto(this IEnumerable<AzureIndexEntity> indexDtos)
    {
        return indexDtos.Select((indexDto) => new MyPupilItemDto()
        {
            Id = Guid.NewGuid(),
            UPN = indexDto.UPN
        }).ToList();
    }

    internal static IEnumerable<PupilDto> MapToPupilDto(this IEnumerable<AzureIndexEntity> indexDtos)
    {
        return indexDtos.Select(pupil => new PupilDto()
        {
            Id = pupil.id,
            UniquePupilNumber = pupil.UPN,
            DateOfBirth = pupil.DOB?.ToString("yyyy-MM-dd") ?? string.Empty,
            Forename = pupil.Forename,
            Surname = pupil.Surname,
            Sex = pupil.Sex?.ToString() ?? string.Empty,
            IsPupilPremium = false,
            LocalAuthorityCode = int.Parse(pupil.LocalAuthority),
        });
    }
}
