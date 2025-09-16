using DfE.GIAP.Core.Common.Domain;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public interface ILogSink<TPayload>
{
    string Name { get; }
    void Log(LogEntry<TPayload> entry);
}

public interface ITraceLogSink : ILogSink<TracePayload> { }
public interface IAuditLogSink : ILogSink<AuditPayload> { }



public class ConsoleTraceSink : ITraceLogSink
{
    public string Name => "ConsoleTrace";
    public void Log(LogEntry<TracePayload> logEntry)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetLogLevelColor(logEntry.Level);

        Console.WriteLine($"[{logEntry.Timestamp:HH:mm:ss} {logEntry.Level.ToString().ToUpper()}] {logEntry.Payload.Message}");
        if (logEntry.Payload.Exception is not null)
        {
            Console.WriteLine(logEntry.Payload.Exception);
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

public class AzureAppInsightTraceSink : ITraceLogSink
{
    public string Name => "AzureAppInsightTrace";
    private readonly TelemetryClient _telemetryClient;

    public AzureAppInsightTraceSink(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void Log(LogEntry<TracePayload> logEntry)
    {
        if (logEntry.Payload.Exception is not null)
        {
            _telemetryClient.TrackException(logEntry.Payload.Exception);
        }
        else
        {
            TraceTelemetry telemetry = new(logEntry.Payload.Message, ToSeverityLevel(logEntry.Level));
            telemetry.Properties["Category"] = logEntry.Payload.Category ?? "None";
            telemetry.Properties["Source"] = logEntry.Payload.Source ?? "Unknown";
            telemetry.Properties["CorrelationId"] = logEntry.CorrelationId ?? "None";

            if (logEntry.Payload.Context is not null)
            {
                foreach (KeyValuePair<string, object> kvp in logEntry.Payload.Context)
                {
                    telemetry.Properties[kvp.Key] = kvp.Value?.ToString() ?? "null";
                }
            }

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

public class AzureAppInsightAuditSink : IAuditLogSink
{
    public string Name => "AzureAppInsightBusiness";
    private readonly TelemetryClient _telemetryClient;

    public AzureAppInsightAuditSink(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void Log(LogEntry<AuditPayload> logEntry)
    {
        Dictionary<string, string> props = new()
        {
            { "EventName", logEntry.Payload.EventName },
            { "Category", logEntry.Payload.Category ?? "None" },
            { "Source", logEntry.Payload.Source ?? "Unknown" },
            { "CorrelationId", logEntry.CorrelationId ?? "None" }
        };

        if (logEntry.Payload.Data is not null)
        {
            foreach (KeyValuePair<string, object> kvp in logEntry.Payload.Data)
            {
                props[kvp.Key] = kvp.Value?.ToString() ?? "null";
            }
        }

        _telemetryClient.TrackEvent(logEntry.Payload.EventName, props);
    }
}
