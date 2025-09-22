using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Configuration;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Common;
public static class CompositionRoot
{
    public static IServiceCollection AddFeaturesSharedDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCosmosDbDependencies();
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IEnumerable<ITextSanitiserHandler>), Array.Empty<ITextSanitiserHandler>()));
        services.AddSingleton<ITextSanitiserInvoker, TextSanitisationInvoker>();

        services.AddCustomLogging(configuration);

        return services;
    }

    // TODO: Thrown together, sort later
    private static IServiceCollection AddCustomLogging(this IServiceCollection services, IConfiguration configuration)
    {
        configuration.GetSection(LoggingOptions.SectionName).Get<LoggingOptions>();

        services.AddScoped<ILoggerService, LoggerService>();

        services.AddSingleton<ITraceLogHandler, TraceLogHandler>();
        services.AddSingleton<ITraceLogSink, AzureAppInsightTraceSink>();

        return services;
    }
}
