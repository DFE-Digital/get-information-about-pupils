using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;

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
            DOB = input.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
            NCYear = sen.NationalCurriculumYear,
            ACAD_YEAR = sen.AcademicYear,
            SEN_PROVISION = sen.Provision
        });
    }
}
