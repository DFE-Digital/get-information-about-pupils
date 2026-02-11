using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;

public class NationalPupilToPhonicsOutputRecordMapper : IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>
{
    public IEnumerable<PhonicsOutputRecord> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Phonics is null || !input.Phonics.Any())
            return Enumerable.Empty<PhonicsOutputRecord>();

        return input.Phonics.Select(phonicsEntry => new PhonicsOutputRecord
        {
            Phonics_ACADYR = phonicsEntry?.AcademicYear,
            Phonics_PUPILMATCHINGREF = phonicsEntry?.PupilMatchingReference,
            Phonics_ID = phonicsEntry?.Id,
            Phonics_UPN = phonicsEntry?.UniquePupilNumber,
            Phonics_SURNAME = phonicsEntry?.SurName,
            Phonics_FORENAMES = phonicsEntry?.ForeNames,
            Phonics_DOB = phonicsEntry?.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
            Phonics_SEX = phonicsEntry?.SEX,
            Phonics_LA = phonicsEntry?.LocalAuthority,
            Phonics_ESTAB = phonicsEntry?.Establishment,
            Phonics_URN = phonicsEntry?.UniqueReferenceNumber,
            Phonics_NCYEARACTUAL = phonicsEntry?.NationalCurriculumYearActual,
            Phonics_TOE_CODE = phonicsEntry?.TypeOfEstablishmentCode,
            Phonics_PHONICS_MARK = phonicsEntry?.Phonics_Mark,
            Phonics_PHONICS_OUTCOME = phonicsEntry?.Phonics_Outcome,
            Phonics_VERSION = phonicsEntry?.Version
        });
    }
}
