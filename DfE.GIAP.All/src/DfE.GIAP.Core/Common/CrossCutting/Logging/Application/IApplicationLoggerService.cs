namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

/// <summary>
/// Defines a contract for a logging service that provides a unified entry point
/// for writing log messages with optional metadata and contextual information.
/// </summary>
public interface IApplicationLoggerService
{
    /// <summary>
    /// Writes a trace-level log entry with the specified message, log level, and optional contextual information.
    /// </summary>
    /// <remarks>Use this method to record detailed diagnostic information for tracing application behavior.
    /// Trace-level logs are typically used for debugging and may be filtered out in production environments depending
    /// on the configured log level.</remarks>
    /// <param name="level">The severity level of the log entry. Determines how the log is categorized and filtered.</param>
    /// <param name="message">The message to include in the log entry. Provides details about the event or operation being traced.</param>
    /// <param name="exception">An optional exception associated with the log entry. If specified, exception details are included in the log.</param>
    /// <param name="category">An optional category name used to group or classify the log entry. Can be <see langword="null"/> to omit
    /// categorization.</param>
    /// <param name="source">An optional source identifier indicating where the log entry originated. Can be <see langword="null"/> if not
    /// applicable.</param>
    /// <param name="context">An optional dictionary containing additional contextual data to include with the log entry. Keys represent
    /// context names; values represent associated data. Can be <see langword="null"/> if no extra context is needed.</param>
    void LogTrace(
        LogLevel level,
        string message,
        Exception? exception = null,
        string? category = null,
        string? source = null,
        Dictionary<string, object>? context = null);
}
