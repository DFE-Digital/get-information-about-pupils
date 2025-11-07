using DfE.GIAP.Core.Common.CrossCutting.Logging.Handlers;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common.CrossCutting.Logging;

public static class CompositionRoot
{
    public static IServiceCollection AddCrossCuttingLoggingDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies();
    }

    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<IApplicationLogger, ApplicationLogger>();
        services.AddScoped<IEventLogger, EventLogger>(); 
        services.AddSingleton<ITraceLogHandler, TraceLogHandler>();
        services.AddSingleton<IEventLogHandler, EventLogHandler>();

        return services;
    }

    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ITraceLogSink, AzureAppInsightTraceSink>();
        services.AddSingleton<IEventLogSink, AzureAppInsightEventSink>();

        return services;
    }
}
