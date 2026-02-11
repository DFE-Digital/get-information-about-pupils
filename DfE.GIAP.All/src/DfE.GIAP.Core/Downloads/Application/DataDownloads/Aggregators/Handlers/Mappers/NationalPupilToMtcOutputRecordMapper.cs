using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;

public class NationalPupilToMtcOutputRecordMapper : IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>
{
    public IEnumerable<MTCOutputRecord> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.MTC is null || !input.MTC.Any())
            return Enumerable.Empty<MTCOutputRecord>();

        return input.MTC.Select(mtcEntry => new MTCOutputRecord
        {
            ACADYR = mtcEntry?.ACADYR,
            PupilMatchingRef = mtcEntry?.PupilMatchingRef,
            UPN = mtcEntry?.UPN,
            Surname = mtcEntry?.Surname,
            Forename = mtcEntry?.Forename,
            Sex = mtcEntry?.Sex,
            DOB = mtcEntry?.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
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
