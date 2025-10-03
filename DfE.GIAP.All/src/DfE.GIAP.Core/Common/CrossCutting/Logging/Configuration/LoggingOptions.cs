namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Configuration;

/// <summary>
/// Represents the root configuration options for the logging system.
/// Typically bound from a configuration section (e.g., appsettings.json).
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// The name of the configuration section used to bind these options.
    /// </summary>
    public const string SectionName = "LoggingOptions";

    /// <summary>
    /// Gets or sets the configuration for trace-level logging,
    /// including the available sinks and their settings.
    /// </summary>
    public TraceLogConfig Trace { get; set; } = new();
}

/// <summary>
/// Represents configuration for trace logging, including
/// the collection of sinks that can receive log entries.
/// </summary>
public class TraceLogConfig
{
    /// <summary>
    /// Gets or sets the collection of log sinks, keyed by sink name.
    /// Each sink defines its own configuration and accepted log levels.
    /// </summary>
    public Dictionary<string, SinkConfig> Sinks { get; set; } = new();
}

/// <summary>
/// Represents configuration settings for an individual log sink.
/// </summary>
public class SinkConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether this sink is enabled.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of log levels that this sink will accept.
    /// Only log entries matching one of these levels will be written to the sink.
    /// </summary>
    public List<LogLevel> AcceptedLogLevels { get; set; } = new();
}


