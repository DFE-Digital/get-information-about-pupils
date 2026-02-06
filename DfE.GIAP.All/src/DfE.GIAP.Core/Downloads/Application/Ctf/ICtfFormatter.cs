namespace DfE.GIAP.Core.Downloads.Application.Ctf;

// OUTPUT FORMATTER
public interface ICtfFormatter
{
    string ContentType { get; }
    byte[] Format(CtfHeader header, IEnumerable<CtfPupil> pupils);
}
