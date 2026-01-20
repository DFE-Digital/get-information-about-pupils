using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class FurtherEducationPupilToSenOutputRecordMapper : IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>
{
    public FurtherEducationSENOutputRecord Map(FurtherEducationPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        SpecialEducationalNeedsEntry? sen = input.specialEducationalNeeds?.FirstOrDefault();
        return new FurtherEducationSENOutputRecord
        {
            ULN = input.UniqueLearnerNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            Sex = input.Sex,
            DOB = input.DOB.ToShortDateString(),
            NCYear = sen?.NationalCurriculumYear,
            ACAD_YEAR = sen?.AcademicYear,
            SEN_PROVISION = sen?.Provision
        };
    }
}
