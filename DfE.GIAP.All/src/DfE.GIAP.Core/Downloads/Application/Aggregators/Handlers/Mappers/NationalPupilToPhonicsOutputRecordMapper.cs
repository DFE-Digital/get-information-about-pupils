using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToPhonicsOutputRecordMapper : IMapper<NationalPupil, PhonicsOutput>
{
    public PhonicsOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        PhonicsEntry? phonicsEntry = input.Phonics?.FirstOrDefault();
        return new PhonicsOutput
        {
            Phonics_ACADYR = phonicsEntry?.AcademicYear,
            Phonics_PUPILMATCHINGREF = phonicsEntry?.PupilMatchingReference,
            Phonics_ID = phonicsEntry?.Id,
            Phonics_UPN = phonicsEntry?.UniquePupilNumber,
            Phonics_SURNAME = phonicsEntry?.SurName,
            Phonics_FORENAMES = phonicsEntry?.ForeNames,
            Phonics_DOB = phonicsEntry?.DateOfBirth,
            Phonics_SEX = phonicsEntry?.SEX,
            Phonics_LA = phonicsEntry?.LocalAuthority,
            Phonics_ESTAB = phonicsEntry?.Establishment,
            Phonics_URN = phonicsEntry?.UniqueReferenceNumber,
            Phonics_NCYEARACTUAL = phonicsEntry?.NationalCurriculumYearActual,
            Phonics_TOE_CODE = phonicsEntry?.TypeOfEstablishmentCode,
            Phonics_PHONICS_MARK = phonicsEntry?.Phonics_Mark,
            Phonics_PHONICS_OUTCOME = phonicsEntry?.Phonics_Outcome,
            Phonics_VERSION = phonicsEntry?.Version
        };
    }
}
