using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogHandler<TPayload>
{
    void Handle(LogEntry<TPayload> entry);
}

public interface ITraceLogHandler : ILogHandler<TracePayload> { }
