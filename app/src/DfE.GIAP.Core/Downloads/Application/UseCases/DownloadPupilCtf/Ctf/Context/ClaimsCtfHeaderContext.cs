namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Context;

public class ClaimsCtfHeaderContext : ICtfHeaderContext
{
    public bool IsEstablishment { get; set; }
    public string LocalAuthorityNumber { get; set; } = string.Empty;
    public string EstablishedNumber { get; set; } = string.Empty;
}
