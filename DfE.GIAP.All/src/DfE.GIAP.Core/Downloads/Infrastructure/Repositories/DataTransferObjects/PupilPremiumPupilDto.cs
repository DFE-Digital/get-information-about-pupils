using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects.Entries;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;

public class PupilPremiumPupilDto
{
    [JsonProperty("UPN")]
    public string? UniquePupilNumber { get; set; }

    [JsonProperty("URN")]
    public string? UniqueReferenceNumber { get; set; }

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
    public List<PupilPremiumEntryDto> PupilPremium { get; set; } = new();
}
