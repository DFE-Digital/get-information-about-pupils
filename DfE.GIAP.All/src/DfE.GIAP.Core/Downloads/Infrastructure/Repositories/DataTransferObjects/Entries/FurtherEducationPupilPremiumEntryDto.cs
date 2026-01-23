using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;

public class FurtherEducationPupilPremiumEntryDto
{
    [JsonProperty("NCYear")]
    public string? NationalCurriculumYear { get; set; }

    [JsonProperty("Pupil_Premium_FTE")]
    public string? FullTimeEquivalent { get; set; }

    [JsonProperty("ACAD_YEAR")]
    public string? AcademicYear { get; set; }
}
