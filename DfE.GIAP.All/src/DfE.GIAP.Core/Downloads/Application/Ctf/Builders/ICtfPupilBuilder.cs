namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

// PUPIL AGGREGATIONS
public interface ICtfPupilBuilder
{
    Task<IEnumerable<CtfPupil>> Build(IEnumerable<string> selectedPupilIds);
}
