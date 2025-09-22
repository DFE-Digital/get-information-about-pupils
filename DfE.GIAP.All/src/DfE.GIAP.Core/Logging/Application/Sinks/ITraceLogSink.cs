using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Logging.Application.Models;

namespace DfE.GIAP.Core.Logging.Application.Sinks;

public interface ITraceLogSink : ILogSink<TracePayload> { }
