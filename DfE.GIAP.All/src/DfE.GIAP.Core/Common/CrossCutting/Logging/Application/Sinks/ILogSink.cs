using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Sinks;

/// <summary>
/// Defines a contract for a log sink that receives and persists log entries
/// of a specified payload type.
/// </summary>
/// <typeparam name="TPayload">
/// The type of the payload contained within the log entry.
/// </typeparam>
public interface ILogSink<TPayload>
{
    /// <summary>
    /// Gets the unique name of the sink, used to identify the log destination
    /// (e.g., "Console", "File").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Writes the specified log entry to the sink.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="Log{TPayload}"/> instance containing the payload and
    /// associated metadata to be logged.
    /// </param>
    void Log(Log<TPayload> entry);
}
