namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public interface ICtfPupilBuilder
{
    Task<IEnumerable<CtfPupil>> Build(IEnumerable<string> selectedPupilIds);
}
