using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;

public interface ILogHandler<TPayload>
{
    void Handle(Log<TPayload> entry);
}
