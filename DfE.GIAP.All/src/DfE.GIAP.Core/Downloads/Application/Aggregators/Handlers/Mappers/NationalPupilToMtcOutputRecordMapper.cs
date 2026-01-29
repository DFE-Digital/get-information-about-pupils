using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToMtcOutputRecordMapper : IMapper<NationalPupil, IEnumerable<MTCOutput>>
{
    public IEnumerable<MTCOutput> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.MTC is null || !input.MTC.Any())
            return Enumerable.Empty<MTCOutput>();

        return input.MTC.Select(mtcEntry => new MTCOutput
        {
            ACADYR = mtcEntry?.ACADYR,
            PupilMatchingRef = mtcEntry?.PupilMatchingRef,
            UPN = mtcEntry?.UPN,
            Surname = mtcEntry?.Surname,
            Forename = mtcEntry?.Forename,
            Sex = mtcEntry?.Sex,
            DOB = mtcEntry?.DOB?.ToShortDateString(),
            LA = mtcEntry?.LA,
            LA_9Code = mtcEntry?.LA_9Code,
            ESTAB = mtcEntry?.Estab,
            LAESTAB = mtcEntry?.LAEstab,
            URN = mtcEntry?.URN,
            ToECode = mtcEntry?.ToECode,
            FormMark = mtcEntry?.FormMark,
            PupilStatus = mtcEntry?.PupilStatus,
            ReasonNotTakingCheck = mtcEntry?.ReasonNotTakingCheck
        });
    }
}
