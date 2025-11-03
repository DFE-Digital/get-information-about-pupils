using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;

public interface IEventLogSink : ILogSink<BusinessEventPayload> { }
