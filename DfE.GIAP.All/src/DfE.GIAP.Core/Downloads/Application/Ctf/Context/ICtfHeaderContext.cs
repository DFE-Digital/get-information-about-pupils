namespace DfE.GIAP.Core.Downloads.Application.Ctf.Context;

public interface ICtfHeaderContext
{
    bool IsEstablishment { get; set; }
    string EstablishedNumber { get; set; }
    string LocalAuthorityNumber { get; set; }
}
