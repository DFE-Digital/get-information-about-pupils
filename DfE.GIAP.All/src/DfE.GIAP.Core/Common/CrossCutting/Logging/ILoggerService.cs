namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

/// <summary>
/// Defines a contract for a logging service that provides a unified entry point
/// for writing log messages with optional metadata and contextual information.
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// Writes a log entry with the specified level, message, and optional details.
    /// </summary>
    void LogTrace(
        LogLevel level,
        string message,
        Exception? exception = null,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? context = null);
}
