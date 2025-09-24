using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Enums;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

public record TracePayload(
    string Message,
    string CorrelationId,
    string UserID,
    string SessionId,
    LogLevel Level = LogLevel.Information,
    Exception? Exception = null,
    string? Category = null,
    string? Source = null,
    Dictionary<string, object>? Context = null);
