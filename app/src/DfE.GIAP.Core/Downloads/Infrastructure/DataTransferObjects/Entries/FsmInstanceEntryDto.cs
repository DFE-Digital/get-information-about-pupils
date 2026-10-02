using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.DataTransferObjects.Entries;

public class FsmInstanceEntryDto
{
    [JsonProperty("FSMstartDate")]
    public DateTime? FSMstartDate { get; set; }

    [JsonProperty("FSMEligibilityVerificationDate")]
    public DateTime? FSMEligibilityVerificationDate { get; set; }

    [JsonProperty("FSMendDate")]
    public DateTime? FSMendDate { get; set; }

    [JsonProperty("UKcountry")]
    public string? UKcountry { get; set; }

    [JsonProperty("FSMCategory")]
    public string? FSMCategory { get; set; }
}
