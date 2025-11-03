using DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public class EventLogger : IEventLogger
{
    private readonly IEnumerable<IEventLogHandler> _handlers;
    private readonly ILogEntryFactory<BusinessEventPayloadOptions, BusinessEventPayload> _factory;

    public EventLogger(
        IEnumerable<IEventLogHandler> handlers,
        ILogEntryFactory<BusinessEventPayloadOptions, BusinessEventPayload> factory)
    {
        _handlers = handlers;
        _factory = factory;
    }

    public void LogUserSignIn()
    {
        BusinessEventPayloadOptions options = new()
        {
            EventCategory = BusinessEventCategory.UserSignIn,
            EventAction = "Login",
            EventStatus = "Some event status",
            Description = "Captures user authentication and session creation.",
            Context = new Dictionary<string, object>
            {
                ["UserStatus"] = "What is a user status?",
            }
        };

        Log(options);
    }

    public void LogSearch(string eventAction, string eventStatus, string description, string dataset)
    {
        BusinessEventPayloadOptions options = new()
        {
            EventCategory = BusinessEventCategory.Search,
            EventAction = eventAction,
            EventStatus = eventStatus,
            Description = description,
            Context = new Dictionary<string, object>
            {
                ["Dataset"] = dataset,
            }
        };

        Log(options);
    }

    public void LogDownload(string eventAction, string eventStatus, string description, string dataset)
    {
        BusinessEventPayloadOptions options = new()
        {
            EventCategory = BusinessEventCategory.Download,
            EventAction = eventAction,
            EventStatus = eventStatus,
            Description = description,
            Context = new Dictionary<string, object>
            {
                ["Dataset"] = dataset
            }
        };

        Log(options);
    }

    private void Log(BusinessEventPayloadOptions options)
    {
        Log<BusinessEventPayload> logEntry = _factory.Create(options);
        foreach (IEventLogHandler handler in _handlers)
            handler.Handle(logEntry);
    }
}

