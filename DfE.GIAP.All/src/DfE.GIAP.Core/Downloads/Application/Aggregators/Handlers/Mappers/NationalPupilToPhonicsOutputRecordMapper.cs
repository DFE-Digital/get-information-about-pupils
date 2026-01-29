using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToPhonicsOutputRecordMapper : IMapper<NationalPupil, IEnumerable<PhonicsOutput>>
{
    public IEnumerable<PhonicsOutput> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Phonics is null || !input.Phonics.Any())
            return Enumerable.Empty<PhonicsOutput>();

        return input.Phonics.Select(phonicsEntry => new PhonicsOutput
        {
            Phonics_ACADYR = phonicsEntry?.AcademicYear,
            Phonics_PUPILMATCHINGREF = phonicsEntry?.PupilMatchingReference,
            Phonics_ID = phonicsEntry?.Id,
            Phonics_UPN = phonicsEntry?.UniquePupilNumber,
            Phonics_SURNAME = phonicsEntry?.SurName,
            Phonics_FORENAMES = phonicsEntry?.ForeNames,
            Phonics_DOB = phonicsEntry?.DOB?.ToShortDateString(),
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
