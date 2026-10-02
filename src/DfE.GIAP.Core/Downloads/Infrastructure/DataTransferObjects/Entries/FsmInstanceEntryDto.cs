using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Infrastructure.DataTransferObjects.Entries;

public class FsmInstanceEntryDto
{
    [JsonProperty(nameof(FSMstartDate))]
    public DateTime? FSMstartDate { get; set; }

    [JsonProperty(nameof(FSMEligibilityVerificationDate))]
    public DateTime? FSMEligibilityVerificationDate { get; set; }

    [JsonProperty(nameof(FSMendDate))]
    public DateTime? FSMendDate { get; set; }

    [JsonProperty(nameof(UKcountry))]
    public string? UKcountry { get; set; }

    [JsonProperty(nameof(FSMCategory))]
    public string? FSMCategory { get; set; }
}
