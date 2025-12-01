using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Sinks;

public interface IEventSink
{
    void Log(BusinessEvent evt);
}
