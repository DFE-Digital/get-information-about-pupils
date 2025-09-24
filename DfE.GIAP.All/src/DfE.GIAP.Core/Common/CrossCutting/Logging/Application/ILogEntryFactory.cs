using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

public interface ILogEntryFactory
{
    LogEntry<TPayload> CreateLogEntry<TPayload, TPayloadOptions>(TPayloadOptions options);
}
