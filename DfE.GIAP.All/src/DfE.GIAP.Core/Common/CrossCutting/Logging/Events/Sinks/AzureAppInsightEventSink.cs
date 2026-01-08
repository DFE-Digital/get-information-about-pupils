using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;
using Microsoft.ApplicationInsights;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Sinks;

public class AzureAppInsightEventSink : IEventSink
{
    private readonly TelemetryClient _telemetryClient;

    public AzureAppInsightEventSink(TelemetryClient telemetryClient)
    {
        ArgumentNullException.ThrowIfNull(telemetryClient);
        _telemetryClient = telemetryClient;
    }

    public void Log(BusinessEvent evt)
    {
        _telemetryClient.Context.User.Id = evt.UserId;
        _telemetryClient.Context.Session.Id = evt.SessionId;
        _telemetryClient.TrackEvent(evt.EventName, evt.ToProperties());
    }
}
