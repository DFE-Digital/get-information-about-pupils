using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;

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
            telemetry.Properties["CorrelationId"] = logEntry.CorrelationId ?? "None";
            telemetry.Properties["UserId"] = logEntry.UserID ?? "None";
            telemetry.Properties["SessionId"] = logEntry.SessionId ?? "None";
            telemetry.Properties["Category"] = logEntry.Payload.Category ?? "None";
            telemetry.Properties["Source"] = logEntry.Payload.Source ?? "Unknown";

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
        LogLevel.Trace => SeverityLevel.Verbose,
        LogLevel.Information => SeverityLevel.Information,
        LogLevel.Warning => SeverityLevel.Warning,
        LogLevel.Error => SeverityLevel.Error,
        LogLevel.Critical => SeverityLevel.Critical,
        _ => SeverityLevel.Information,
    };
}
