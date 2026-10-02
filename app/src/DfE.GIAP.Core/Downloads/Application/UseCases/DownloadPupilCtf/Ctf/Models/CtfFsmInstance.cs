namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Models;

public class CtfFsmInstance
{
    public DateTime? FSMstartDate { get; set; }

    public DateTime? FSMEligibilityVerificationDate { get; set; }

    public DateTime? FSMendDate { get; set; }

    public string? UKcountry { get; set; }

    public string? FSMCategory { get; set; }
}
