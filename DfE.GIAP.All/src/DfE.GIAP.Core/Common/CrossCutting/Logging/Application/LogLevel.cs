namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application;

/// <summary>
/// Defines the severity levels for log entries, indicating the importance
/// and type of event being logged.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Logs that contain the most detailed messages. These messages may contain
    /// sensitive application data and are typically only enabled during development.
    /// </summary>
    Trace,

    /// <summary>
    /// Logs that are used for interactive investigation during development.
    /// These logs should primarily contain information useful for debugging.
    /// </summary>
    Debug,

    /// <summary>
    /// Logs that track the general flow of the application. These logs should
    /// have long-term value and highlight significant runtime events.
    /// </summary>
    Information,

    /// <summary>
    /// Logs that highlight abnormal or unexpected events in the application flow,
    /// but do not cause the application to stop.
    /// </summary>
    Warning,

    /// <summary>
    /// Logs that highlight errors and failures that require attention,
    /// but the application is still able to continue running.
    /// </summary>
    Error,

    /// <summary>
    /// Logs that describe unrecoverable application or system crashes,
    /// or catastrophic failures that require immediate attention.
    /// </summary>
    Critical
}
