using Microsoft.ApplicationInsights;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Events;

public interface IEventSink
{
    void Write(BusinessEvent evt);
}

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
