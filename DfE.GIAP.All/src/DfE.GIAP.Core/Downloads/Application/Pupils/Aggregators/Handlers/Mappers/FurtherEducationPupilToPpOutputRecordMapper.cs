using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers.Mappers;

public class FurtherEducationPupilToPpOutputRecordMapper : IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>
{
    public FurtherEducationPPOutputRecord Map(FurtherEducationPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        FurtherEducationPupilPremiumEntry? ppEntry = input.PupilPremium?.FirstOrDefault();
        return new FurtherEducationPPOutputRecord
        {
            ULN = input.UniqueLearnerNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            Sex = input.Sex,
            DOB = input.DOB.ToShortDateString(),
            ACAD_YEAR = ppEntry?.AcademicYear,
            NCYear = ppEntry?.NationalCurriculumYear,
            Pupil_Premium_FTE = ppEntry?.FullTimeEquivalent
        };
    }
}
