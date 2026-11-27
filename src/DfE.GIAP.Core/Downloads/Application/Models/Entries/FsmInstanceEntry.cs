namespace DfE.GIAP.Core.Downloads.Application.Models.Entries;

public class FsmInstanceEntry
{
    public DateTime? FSMstartDate { get; set; }

    public DateTime? FSMEligibilityVerificationDate { get; set; }

    public DateTime? FSMendDate { get; set; }

    public string? UKcountry { get; set; }

    public string? FSMCategory { get; set; }
}
