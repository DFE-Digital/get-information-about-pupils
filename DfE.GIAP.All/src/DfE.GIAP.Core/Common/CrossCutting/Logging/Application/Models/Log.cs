namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

/// <summary>
/// Represents a structured log entry that contains a strongly-typed payload
/// along with associated metadata such as timestamp.
/// </summary>
/// <typeparam name="TPayload">
/// The type of the payload being logged. This allows logs to carry
/// strongly-typed, structured data rather than plain text.
/// </typeparam>
public record Log<TPayload>
{
    /// <summary>
    /// Gets the UTC timestamp indicating when the log entry was created.
    /// Defaults to <see cref="DateTime.UtcNow"/> at instantiation.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the strongly-typed payload associated with this log entry.
    /// This property is required and represents the core data being logged.
    /// </summary>
    public required TPayload Payload { get; init; }
}

