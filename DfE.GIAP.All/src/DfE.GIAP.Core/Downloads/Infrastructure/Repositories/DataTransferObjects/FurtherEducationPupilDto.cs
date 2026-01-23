using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

public class FurtherEducationPupilDto
{
    [JsonProperty("ULN")]
    public string? UniqueLearnerNumber { get; set; }

    [JsonProperty("Forename")]
    public string? Forename { get; set; }

    [JsonProperty("Surname")]
    public string? Surname { get; set; }

    [JsonProperty("Sex")]
    public string? Sex { get; set; }

    [JsonProperty("DOB")]
    public DateTime DOB { get; set; }

    [JsonProperty("ConcatenatedName")]
    public string? ConcatenatedName { get; set; }

    [JsonProperty("Pupil_Premium")]
    public List<FurtherEducationPupilPremiumEntryDto> PupilPremium { get; set; } = new();

    [JsonProperty("SEN")]
    public List<SpecialEducationalNeedsEntryDto> specialEducationalNeeds { get; set; } = new();
}
