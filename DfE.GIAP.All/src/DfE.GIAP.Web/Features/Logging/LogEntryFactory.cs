using DfE.GIAP.Core.Common.CrossCutting.Logging.Application;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Application.Models;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging.Models;

public class LogEntryFactory : ILogEntryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LogEntryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public LogEntry<TPayload> CreateLogEntry<TPayload, TPayloadOptions>(TPayloadOptions options)
    {
        ILogPayloadEnricher<TPayload, TPayloadOptions> payloadEnricher = _serviceProvider
            .GetRequiredService<ILogPayloadEnricher<TPayload, TPayloadOptions>>();

        TPayload payload = payloadEnricher.Enrich(options);

        return new LogEntry<TPayload>
        {
            Timestamp = DateTime.UtcNow,
            Payload = payload
        };
    }
}
