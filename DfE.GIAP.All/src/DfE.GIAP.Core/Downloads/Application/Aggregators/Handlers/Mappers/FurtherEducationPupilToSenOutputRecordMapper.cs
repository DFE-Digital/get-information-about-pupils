using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class FurtherEducationPupilToSenOutputRecordMapper
    : IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>
{
    public IEnumerable<FurtherEducationSENOutputRecord> Map(FurtherEducationPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.specialEducationalNeeds is null || !input.specialEducationalNeeds.Any())
            return Enumerable.Empty<FurtherEducationSENOutputRecord>();

        return input.specialEducationalNeeds.Select(sen => new FurtherEducationSENOutputRecord
        {
            ULN = input.UniqueLearnerNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            Sex = input.Sex,
            DOB = input.DOB.ToShortDateString(),
            NCYear = sen.NationalCurriculumYear,
            ACAD_YEAR = sen.AcademicYear,
            SEN_PROVISION = sen.Provision
        });
    }
}
