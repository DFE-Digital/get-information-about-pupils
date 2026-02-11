using DfE.GIAP.Core.Downloads.Application.Ctf;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public interface ICtfHeaderHandler
{
    CtfHeader Build(ICtfHeaderContext context);
}
