using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Handlers;

/// <summary>
/// Defines a contract for handling log entries before they are passed
/// to one or more log sinks.
/// </summary>
/// <typeparam name="TPayload">
/// The type of the payload contained within the log entry.
/// </typeparam>
public interface ILogHandler<TPayload>
{
    /// <summary>
    /// Processes the specified log entry.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="Log{TPayload}"/> instance containing the payload and
    /// associated metadata to be handled (e.g., enriched, filtered, or routed).
    /// </param>
    void Handle(Log<TPayload> entry);
}
