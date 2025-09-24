using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Sinks;

public interface ILogSink<TPayload>
{
    string Name { get; }
    void Log(Log<TPayload> entry);
}
