using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogSink<TPayload>
{
    string Name { get; }
    void Log(LogEntry<TPayload> entry);
}
