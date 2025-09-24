using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Enums;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

public interface ILoggerService
{
    void LogTrace(
        LogLevel level,
        string message,
        Exception? exception = null,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? context = null);
}
