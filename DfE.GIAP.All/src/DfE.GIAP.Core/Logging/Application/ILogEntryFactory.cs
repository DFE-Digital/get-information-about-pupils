using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Logging.Application;

public interface ILogEntryFactory
{
    LogEntry<TPayload> CreateLogEntry<TPayload, TPayloadOptions>(TPayloadOptions options);
}
