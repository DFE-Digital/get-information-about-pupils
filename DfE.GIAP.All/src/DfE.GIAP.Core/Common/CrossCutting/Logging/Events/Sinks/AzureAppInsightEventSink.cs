using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;
using Microsoft.ApplicationInsights;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Sinks;

public class AzureAppInsightEventSink : IEventSink
{
    private readonly TelemetryClient _telemetryClient;

    public AzureAppInsightEventSink(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void Write(BusinessEvent evt)
    {
        _telemetryClient.TrackEvent(evt.EventName, evt.ToProperties());
    }
}
