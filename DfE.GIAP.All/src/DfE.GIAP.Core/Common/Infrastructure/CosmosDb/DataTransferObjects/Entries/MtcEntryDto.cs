using Newtonsoft.Json;

namespace DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;

public class MtcEntryDto
{
    [JsonProperty("ACADYR")]
    public string? ACADYR { get; set; }

    [JsonProperty("PupilMatchingRef")]
    public string? PupilMatchingRef { get; set; }

    [JsonProperty("UPN")]
    public string? UPN { get; set; }

    [JsonProperty("Surname")]
    public string? Surname { get; set; }

    [JsonProperty("Forename")]
    public string? Forename { get; set; }

    [JsonProperty("Sex")]
    public string? Sex { get; set; }

    [JsonProperty("DOB")]
    public string? DOB { get; set; }

    [JsonProperty("LA")]
    public string? LA { get; set; }

    [JsonProperty("LA_9Code")]
    public string? LA_9Code { get; set; }

    [JsonProperty("ESTAB")]
    public string? Estab { get; set; }

    [JsonProperty("LAestab")]
    public string? LAEstab { get; set; }

    [JsonProperty("URN")]
    public string? URN { get; set; }

    [JsonProperty("ToECode")]
    public string? ToECode { get; set; }

    [JsonProperty("FormMark")]
    public string? FormMark { get; set; }

    [JsonProperty("PupilStatus")]
    public string? PupilStatus { get; set; }

    [JsonProperty("ReasonNotTakingCheck")]
    public string? ReasonNotTakingCheck { get; set; }
}
