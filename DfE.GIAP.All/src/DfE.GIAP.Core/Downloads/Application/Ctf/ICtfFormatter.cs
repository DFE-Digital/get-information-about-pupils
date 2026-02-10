namespace DfE.GIAP.Core.Downloads.Application.Ctf;

public interface ICtfFormatter
{
    string ContentType { get; }
    byte[] Format(CtfHeader header, IEnumerable<CtfPupil> pupils);
}
