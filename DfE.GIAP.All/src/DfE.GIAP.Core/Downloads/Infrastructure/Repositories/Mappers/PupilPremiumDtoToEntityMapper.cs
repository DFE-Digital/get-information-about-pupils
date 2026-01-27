using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

internal class PupilPremiumDtoToEntityMapper : IMapper<PupilPremiumPupilDto, PupilPremiumPupil>
{
    public PupilPremiumPupil Map(PupilPremiumPupilDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new PupilPremiumPupil
        {
            UniquePupilNumber = input.UniquePupilNumber,
            UniqueReferenceNumber = input.UniqueReferenceNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            DOB = input.DOB,
            ConcatenatedName = input.ConcatenatedName,
            Sex = input.Sex,
            PupilPremium = input.PupilPremium?.Select(ppDto => new PupilPremiumEntry
            {
                UniquePupilNumber = ppDto.UniquePupilNumber,
                Surname = ppDto.Surname,
                Forename = ppDto.Forename,
                Sex = ppDto.Sex,
                DOB = ppDto.DOB,
                NCYear = ppDto.NCYear,
                DeprivationPupilPremium = ppDto.DeprivationPupilPremium,
                ServiceChildPremium = ppDto.ServiceChildPremium,
                AdoptedfromCarePremium = ppDto.AdoptedfromCarePremium,
                LookedAfterPremium = ppDto.LookedAfterPremium,
                PupilPremiumFTE = ppDto.PupilPremiumFTE,
                PupilPremiumCashAmount = ppDto.PupilPremiumCashAmount,
                PupilPremiumFYStartDate = ppDto.PupilPremiumFYStartDate,
                PupilPremiumFYEndDate = ppDto.PupilPremiumFYEndDate,
                LastFSM = ppDto.LastFSM,
                MODSERVICE = ppDto.MODSERVICE,
                CENSUSSERVICEEVER6 = ppDto.CENSUSSERVICEEVER6,
            }).ToList() ?? []
        };
    }
}
