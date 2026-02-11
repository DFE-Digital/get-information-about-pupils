using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;

public class FurtherEducationPupilToPpOutputRecordMapper
    : IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>
{
    public IEnumerable<FurtherEducationPPOutputRecord> Map(FurtherEducationPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.PupilPremium is null || !input.PupilPremium.Any())
            return Enumerable.Empty<FurtherEducationPPOutputRecord>();

        return input.PupilPremium.Select(pp => new FurtherEducationPPOutputRecord
        {
            ULN = input.UniqueLearnerNumber,
            Forename = input.Forename,
            Surname = input.Surname,
            Sex = input.Sex,
            DOB = input.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
            ACAD_YEAR = pp.AcademicYear,
            NCYear = pp.NationalCurriculumYear,
            Pupil_Premium_FTE = pp.FullTimeEquivalent
        });
    }
}
