using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogEntryFactory<TPayloadOptions, TPayload>
{
    Log<TPayload> Create(TPayloadOptions options);
}
