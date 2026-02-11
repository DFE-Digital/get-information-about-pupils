using DfE.GIAP.Core.Downloads.Application.Ctf;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public interface ICtfPupilHandler
{
    Task<IEnumerable<CtfPupil>> BuildAsync(IEnumerable<string> selectedPupilIds);
}
