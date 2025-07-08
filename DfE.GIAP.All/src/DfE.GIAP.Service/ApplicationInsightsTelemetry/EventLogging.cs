using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DfE.GIAP.Service.ApplicationInsightsTelemetry;

public class EventLogging : IEventLogging
{
    private readonly TelemetryClient _telemetryClient;

    public EventLogging(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackEvent(int eventId, string eventDescription, string clientId, string sessionId, string filePath)
    {
        var dataPath = System.IO.Path.Combine(filePath, "logevents.json");

        using StreamReader file = File.OpenText(dataPath);
        var serializer = new JsonSerializer();
        var eventList = (List<LogEvent>)serializer.Deserialize(file, typeof(List<LogEvent>));
        var trackedEvent = eventList.FirstOrDefault(e => e.EventID == eventId);

        _telemetryClient.Context.Session.Id = sessionId;
        _telemetryClient.Context.User.Id = clientId;
        _telemetryClient.TrackEvent(trackedEvent.EventName, new Dictionary<string, string>()
        {
            ["EventID"] = trackedEvent.EventID.ToString(),
            ["Framework Function"] = trackedEvent.FrameworkFunction,
            ["ClientID"] = clientId,
            ["SessionID"] = sessionId,
            ["Event Name"] = trackedEvent.EventName,
            ["Check Point"] = trackedEvent.CheckPoint,
            ["Status"] = trackedEvent.Status,
            ["Entity Type"] = trackedEvent.EntityType,
            ["Description"] = eventDescription
        });
    }
}