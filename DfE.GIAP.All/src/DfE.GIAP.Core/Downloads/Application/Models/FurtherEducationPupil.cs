using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Application.Models;

public class FurtherEducationPupil
{
    [JsonProperty("ULN")]
    public string? UniqueLearnerNumber { get; set; }

    [JsonProperty("Forename")]
    public string? Forename { get; set; }

    [JsonProperty("Surname")]
    public string? Surname { get; set; }

    [JsonProperty("Gender")]
    public string? Gender { get; set; }

    [JsonProperty("DOB")]
    public DateTime DOB { get; set; }

    [JsonProperty("ConcatenatedName")]
    public string? ConcatenatedName { get; set; }

    [JsonProperty("Pupil_Premium")]
    public List<PupilPremiumEntry> PupilPremium { get; set; } = new();

    [JsonProperty("SEN")]
    public List<SpecialEducationalNeeds> specialEducationalNeeds { get; set; } = new();
}

public class PupilPremiumEntry
{
    [JsonProperty("NCYear")]
    public string? NationalCurriculumYear { get; set; }

    [JsonProperty("Pupil_Premium_FTE")]
    public string? FullTimeEquivalent { get; set; }

    [JsonProperty("ACAD_YEAR")]
    public string? AcademicYear { get; set; }
}

public class SpecialEducationalNeeds
{
    [JsonProperty("NCYear")]
    public string? NationalCurriculumYear { get; set; }

    [JsonProperty("SEN_PROVISION")]
    public string? Provision { get; set; }

    [JsonProperty("ACAD_YEAR")]
    public string? AcademicYear { get; set; }
}
