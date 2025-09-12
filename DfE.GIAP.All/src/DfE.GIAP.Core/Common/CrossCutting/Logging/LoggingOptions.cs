namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public class LoggingOptions
{
    public const string SectionName = "LoggingOptions";

    public TraceLoggingConfig Trace { get; set; } = new();
    public BusinessEventLoggingConfig BusinessEvents { get; set; } = new();
}

public class TraceLoggingConfig
{
    public Dictionary<string, SinkConfig> Sinks { get; set; } = new();
}

public class BusinessEventLoggingConfig
{
    public List<string> Sinks { get; set; } = new();
}

public class SinkConfig
{
    public List<LogLevel> AcceptedLogLevels { get; set; } = new();
}

