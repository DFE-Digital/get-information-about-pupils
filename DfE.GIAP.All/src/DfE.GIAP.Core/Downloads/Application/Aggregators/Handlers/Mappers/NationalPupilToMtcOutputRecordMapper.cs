using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToMtcOutputRecordMapper : IMapper<NationalPupil, MTCOutput>
{
    public NationalPupilToMtcOutputRecordMapper()
    {
    }

    public MTCOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        MtcEntry? mtcEntry = input.MTC?.FirstOrDefault();
        return new MTCOutput
        {
            ACADYR = mtcEntry?.ACADYR,
            PupilMatchingRef = mtcEntry?.PupilMatchingRef,
            UPN = mtcEntry?.UPN,
            Surname = mtcEntry?.Surname,
            Forename = mtcEntry?.Forename,
            Sex = mtcEntry?.Sex,
            DOB = mtcEntry?.DOB,
            LA = mtcEntry?.LA,
            LA_9Code = mtcEntry?.LA_9Code,
            ESTAB = mtcEntry?.Estab,
            LAESTAB = mtcEntry?.LAEstab,
            URN = mtcEntry?.URN,
            ToECode = mtcEntry?.ToECode,
            FormMark = mtcEntry?.FormMark,
            PupilStatus = mtcEntry?.PupilStatus,
            ReasonNotTakingCheck = mtcEntry?.ReasonNotTakingCheck
        };
    }
}
