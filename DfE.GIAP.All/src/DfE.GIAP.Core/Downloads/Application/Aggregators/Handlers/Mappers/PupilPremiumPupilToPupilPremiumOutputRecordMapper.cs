using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class PupilPremiumPupilToPupilPremiumOutputRecordMapper : IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>
{
    public PupilPremiumOutputRecord Map(PupilPremiumPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        PupilPremiumEntry? ppEntry = input.PupilPremium?.FirstOrDefault();
        return new PupilPremiumOutputRecord
        {
            UniquePupilNumber = input.UniquePupilNumber,
            Surname = input.Surname,
            Forename = input.Forename,
            Sex = input.Sex,
            DOB = input.DOB.ToShortDateString(),
            NCYear = ppEntry?.NCYear,
            DeprivationPupilPremium = ppEntry?.DeprivationPupilPremium,
            ServiceChildPremium = ppEntry?.ServiceChildPremium,
            AdoptedfromCarePremium = ppEntry?.AdoptedfromCarePremium,
            LookedAfterPremium = ppEntry?.LookedAfterPremium,
            PupilPremiumFTE = ppEntry?.PupilPremiumFTE,
            PupilPremiumCashAmount = ppEntry?.PupilPremiumCashAmount,
            PupilPremiumFYStartDate = ppEntry?.PupilPremiumFYStartDate,
            PupilPremiumFYEndDate = ppEntry?.PupilPremiumFYEndDate,
            LastFSM = ppEntry?.LastFSM
        };
    }
}
