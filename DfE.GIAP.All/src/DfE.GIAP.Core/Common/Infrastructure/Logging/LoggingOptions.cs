namespace DfE.GIAP.Core.Common.Infrastructure.Logging;

public class LoggingOptions
{
    public const string SectionName = "LoggingOptions";

    public Dictionary<string, SinkConfig> Sinks { get; set; } = new();
}

public class SinkConfig
{
    public List<LogLevel> AcceptedLogLevels { get; set; } = new();
}

