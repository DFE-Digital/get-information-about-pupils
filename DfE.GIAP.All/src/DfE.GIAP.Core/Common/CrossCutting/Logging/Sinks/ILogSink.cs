using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;

public interface ILogSink<TPayload>
{
    string Name { get; }
    void Log(Log<TPayload> entry);
}
