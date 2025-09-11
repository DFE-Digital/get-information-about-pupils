using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.Infrastructure.Logging;

public interface ILogSink
{
    public void Log(LogEntry logEntry);
}

public class ConsoleSink : ILogSink
{
    private readonly IEnumerable<LogLevel> _acceptedLevels;
    private readonly ILogFormatter _formatter;

    public ConsoleSink(IOptions<LoggingOptions> options, ILogFormatter formatter)
    {
        _acceptedLevels = options.Value?.Sinks["Console"]?.AcceptedLogLevels ?? new();
        _formatter = formatter;
    }

    public void Log(LogEntry logEntry)
    {
        if (!_acceptedLevels.Contains(logEntry.Level)) return;

        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetLogLevelColor(logEntry.Level);

        Console.WriteLine($"[{logEntry.Timestamp:HH:mm:ss} {logEntry.Level.ToString().ToUpper()} {logEntry.Message}]");
        if (logEntry.Exception is not null)
        {
            Console.WriteLine(logEntry.Exception);
        }

        Console.ForegroundColor = originalColor;
    }

    private static ConsoleColor GetLogLevelColor(LogLevel level) => level switch
    {
        LogLevel.Verbose => ConsoleColor.DarkGray,
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _ => ConsoleColor.White,
    };
}

public class AzureApplicationInsightsSink : ILogSink
{
    private readonly IEnumerable<LogLevel> _acceptedLevels;
    private readonly TelemetryClient _telemetryClient;

    public AzureApplicationInsightsSink(IOptions<LoggingOptions> options, TelemetryClient telemetryClient)
    {
        _acceptedLevels = options.Value.Sinks["AzureAppInsight"]?.AcceptedLogLevels ?? new();
        _telemetryClient = telemetryClient;
    }


    public void Log(LogEntry logEntry)
    {
        if (!_acceptedLevels.Contains(logEntry.Level)) return;

        if (logEntry.Exception is not null)
        {
            _telemetryClient.TrackException(logEntry.Exception);
        }
        else
        {
            TraceTelemetry telemetryLog = new(logEntry.Message, ToSeverityLevel(logEntry.Level));
            _telemetryClient.TrackTrace(telemetryLog);
        }
    }

    private static SeverityLevel ToSeverityLevel(LogLevel level) => level switch
    {
        LogLevel.Verbose => SeverityLevel.Verbose,
        LogLevel.Debug => SeverityLevel.Verbose,
        LogLevel.Information => SeverityLevel.Information,
        LogLevel.Warning => SeverityLevel.Warning,
        LogLevel.Error => SeverityLevel.Error,
        LogLevel.Critical => SeverityLevel.Critical,
        _ => SeverityLevel.Information,
    };
}
