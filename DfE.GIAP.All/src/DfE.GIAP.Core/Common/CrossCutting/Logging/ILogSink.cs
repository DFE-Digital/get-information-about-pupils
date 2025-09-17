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
            TraceTelemetry telemetry = new(logEntry.Payload.Message, ToSeverityLevel(logEntry.Payload.Level));
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
        LogLevel.Debug => SeverityLevel.Verbose,
        LogLevel.Information => SeverityLevel.Information,
        LogLevel.Error => SeverityLevel.Error,
        _ => SeverityLevel.Information,
    };
}
