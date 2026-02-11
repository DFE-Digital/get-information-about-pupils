using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;

public class PupilPremiumPupilToPupilPremiumOutputRecordMapper
    : IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>
{
    public IEnumerable<PupilPremiumOutputRecord> Map(PupilPremiumPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.PupilPremium is null || !input.PupilPremium.Any())
            return Enumerable.Empty<PupilPremiumOutputRecord>();

        return input.PupilPremium.Select(ppEntry => new PupilPremiumOutputRecord
        {
            UPN = ppEntry.UniquePupilNumber,
            Surname = ppEntry.Surname,
            Forename = ppEntry.Forename,
            Sex = ppEntry.Sex,
            DOB = ppEntry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
            NCYear = ppEntry.NCYear,
            DeprivationPupilPremium = ppEntry.DeprivationPupilPremium,
            ServiceChildPremium = ppEntry.ServiceChildPremium,
            AdoptedfromCarePremium = ppEntry.AdoptedfromCarePremium,
            LookedAfterPremium = ppEntry.LookedAfterPremium,
            PupilPremiumFTE = ppEntry.PupilPremiumFTE,
            PupilPremiumCashAmount = ppEntry.PupilPremiumCashAmount,
            PupilPremiumFYStartDate = ppEntry.PupilPremiumFYStartDate,
            PupilPremiumFYEndDate = ppEntry.PupilPremiumFYEndDate,
            LastFSM = ppEntry.LastFSM
        });
    }
}
