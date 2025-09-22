using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Logging.Application;
using DfE.GIAP.Core.Logging.Application.Handlers;
using DfE.GIAP.Core.Logging.Application.Sinks;
using DfE.GIAP.Core.Logging.Infrastructure.Sinks;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Logging;
public static class CompositionRoot
{
    public static IServiceCollection AddLoggingDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies();
    }

    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddSingleton<ITraceLogHandler, TraceLogHandler>();

        return services;
    }

    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ITraceLogSink, AzureAppInsightTraceSink>();

        return services;
    }
}
