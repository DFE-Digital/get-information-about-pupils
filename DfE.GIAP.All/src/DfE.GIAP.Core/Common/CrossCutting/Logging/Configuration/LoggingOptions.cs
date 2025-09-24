using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Enums;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Configuration;

public class LoggingOptions
{
    public const string SectionName = "LoggingOptions";

    public TraceLogConfig Trace { get; set; } = new();
}

public class TraceLogConfig
{
    public Dictionary<string, SinkConfig> Sinks { get; set; } = new();
}

public class SinkConfig
{
    public bool Enabled { get; set; } = true;
    public List<LogLevel> AcceptedLogLevels { get; set; } = new();
}

