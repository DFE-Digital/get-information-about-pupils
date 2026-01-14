using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Sinks;

/// <summary>
/// Defines a contract for logging business events to an event sink.
/// </summary>
/// <remarks>Implementations of this interface can be used to record, persist, or forward business events for
/// auditing, monitoring, or integration purposes. The behavior of the event sink depends on the specific
/// implementation, such as writing to a database, sending to an external system, or logging to a file.</remarks>
public interface IEventSink
{
    void Log(BusinessEvent evt);
}
