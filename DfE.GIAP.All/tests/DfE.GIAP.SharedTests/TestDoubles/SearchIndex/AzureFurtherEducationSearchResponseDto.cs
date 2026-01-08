using Newtonsoft.Json;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
public sealed class AzureFurtherEducationSearchResponseDto
{
    [JsonProperty("@search.score")]
    public double Score { get; set; }

    public string ULN { get; set; }

    public string Surname { get; set; }

    public string Forename { get; set; }

    public string Sex { get; set; }

    public string Gender { get; set; }

    public string? DOB { get; set; }

    public string LocalAuthority { get; set; }

    public string id { get; set; }
}
