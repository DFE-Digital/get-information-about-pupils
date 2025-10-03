using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

/// <summary>
/// Defines a factory contract for creating strongly-typed log entries
/// from a given set of payload options.
/// </summary>
/// <typeparam name="TPayloadOptions">
/// The type that contains the options or input data used to construct
/// the payload for the log entry.
/// </typeparam>
/// <typeparam name="TPayload">
/// The type of the payload that will be wrapped in the log entry.
/// </typeparam>
public interface ILogEntryFactory<TPayloadOptions, TPayload>
{
    /// <summary>
    /// Creates a new <see cref="Log{TPayload}"/> instance using the specified options.
    /// </summary>
    /// <param name="options">
    /// The options or input data used to build the payload for the log entry.
    /// </param>
    /// <returns>
    /// A <see cref="Log{TPayload}"/> containing the constructed payload and
    /// associated metadata (e.g., correlation ID, timestamp).
    /// </returns>
    Log<TPayload> Create(TPayloadOptions options);
}

