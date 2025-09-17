namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public class LoggingOptions
{
    public const string SectionName = "LoggingOptions";

    public TraceLogConfig Trace { get; set; } = new();
    public AuditLogConfig Audit { get; set; } = new();
}

public class TraceLogConfig
{
    public Dictionary<string, SinkConfig> Sinks { get; set; } = new();
}

public class AuditLogConfig
{
    public List<string> Sinks { get; set; } = new();
}

public class SinkConfig
{
    public List<LogLevel> AcceptedLogLevels { get; set; } = new();
}

