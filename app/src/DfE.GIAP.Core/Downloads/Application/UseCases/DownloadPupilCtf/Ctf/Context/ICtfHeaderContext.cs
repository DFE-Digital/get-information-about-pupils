namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Context;

public interface ICtfHeaderContext
{
    bool IsEstablishment { get; set; }
    string EstablishedNumber { get; set; }
    string LocalAuthorityNumber { get; set; }
}
