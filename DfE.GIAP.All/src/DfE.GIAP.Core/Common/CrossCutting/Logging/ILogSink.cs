using DfE.GIAP.Core.Common.Domain;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

// Application
public interface ILogSink
{
    string Name { get; }
    public void Log(LogEntry logEntry);
}
public interface ITraceSink : ILogSink { }
public interface IBusinessEventSink : ILogSink { }


// Infrastructure
public class ConsoleTraceLogSink : ITraceSink
{
    public string Name => "ConsoleTrace";
    public void Log(LogEntry logEntry)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetLogLevelColor(logEntry.Level);

        Console.WriteLine($"[{logEntry.Timestamp:HH:mm:ss} {logEntry.Level.ToString().ToUpper()}] {logEntry.Message}");
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

// Infrastructure
public class AzureApplicationInsightTraceSink : ITraceSink
{
    public string Name => "AzureAppInsightTrace";
    private readonly TelemetryClient _telemetryClient;

    public AzureApplicationInsightTraceSink(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void Log(LogEntry logEntry)
    {
        if (logEntry.Exception is not null)
        {
            _telemetryClient.TrackException(logEntry.Exception);
        }
        else
        {
            TraceTelemetry telemetry = new TraceTelemetry(logEntry.Message, ToSeverityLevel(logEntry.Level));
            _telemetryClient.TrackTrace(telemetry);
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

public class AzureApplicationInsightEventSink : IBusinessEventSink
{
    public string Name => "AzureAppInsightBusiness";
    private readonly TelemetryClient _telemetryClient;

    public AzureApplicationInsightEventSink(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void Log(LogEntry logEntry)
    {
        Dictionary<string, string> properties = new()
        {
            { "LogLevel", logEntry.Level.ToString() },
            { "Context", logEntry.Context?.ToString() ?? "None" }
        };
        _telemetryClient.TrackEvent(logEntry.Message, properties);
    }
}
