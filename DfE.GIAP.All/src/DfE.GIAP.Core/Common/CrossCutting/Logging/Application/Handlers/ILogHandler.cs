using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Handlers;

public interface ILogHandler<TPayload>
{
    void Handle(LogEntry<TPayload> entry);
}
