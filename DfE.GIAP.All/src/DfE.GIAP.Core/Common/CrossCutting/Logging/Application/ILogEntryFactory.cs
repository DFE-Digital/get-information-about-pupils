using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

public interface ILogEntryFactory<TPayloadOptions, TPayload>
{
    Log<TPayload> Create(TPayloadOptions options);
}
