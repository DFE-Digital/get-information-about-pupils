namespace DfE.GIAP.Service.ApplicationInsightsTelemetry;

public interface IEventLogging
{
    void TrackEvent(int eventId, string eventDescription, string clientId, string sessionId, string filePath);
}