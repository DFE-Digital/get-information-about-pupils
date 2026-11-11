using Sentry;
using Sentry.Extensibility;

namespace DfE.GIAP.Web.Features.Logging.Sentry;

public sealed class RemoveRequestUrlEventProcessor : ISentryEventProcessor
{
    public SentryEvent Process(SentryEvent @event)
    {
        // URLs and query strings can contain pupil identifiers and search terms.
        @event.Request.Url = null;
        @event.Request.QueryString = null;

        return @event;
    }
}
