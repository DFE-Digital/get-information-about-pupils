using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;

internal class AzureAppInsightEventSink : IEventLogSink
{
    public string Name => "AzureAppInsightEvent";
    private readonly TelemetryClient _telemetryClient;

    public AzureAppInsightEventSink(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void Log(Log<BusinessEventPayload> entry)
    {
        BusinessEventPayload payload = entry.Payload;

        EventTelemetry telemetry = new(payload.EventCategory)
        {
            Timestamp = entry.Timestamp
        };

        telemetry.Properties["EventAction"] = payload.EventAction;
        telemetry.Properties["EventStatus"] = payload.EventStatus;
        telemetry.Properties["Description"] = payload.Description;
        telemetry.Properties["UserId"] = payload.UserId;
        telemetry.Properties["SessionId"] = payload.SessionId;

        if (payload.Context is not null)
        {
            foreach (KeyValuePair<string, object> kvp in payload.Context)
            {
                telemetry.Properties[kvp.Key] = kvp.Value?.ToString() ?? null;
            }
        }

        _telemetryClient.TrackEvent(telemetry);
    }
}
