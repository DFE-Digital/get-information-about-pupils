namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

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
