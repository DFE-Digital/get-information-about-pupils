using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Common.Infrastructure;
internal static class CompositionRoot
{
    internal static IServiceCollection AddBlobStorageProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IBlobStorageProvider>(sp =>
        {
            BlobStorageOptions options = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;

            ArgumentException.ThrowIfNullOrWhiteSpace(options.AccountName);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.AccountKey);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ContainerName);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.EndpointSuffix);

            string connectionString =
            $"AccountName={options.AccountName};" +
            $"AccountKey={options.AccountKey};" +
            $"EndpointSuffix={options.EndpointSuffix};" +
            $"DefaultEndpointsProtocol=https;";

            BlobServiceClient blobServiceClient = new(connectionString);
            return new AzureBlobStorageProvider(blobServiceClient);
        });

        return services;
    }

    // TODO: Thrown together, sort later
    internal static IServiceCollection AddCustomLogging(this IServiceCollection services, IConfiguration configuration)
    {
        LoggingOptions options = configuration.GetSection(LoggingOptions.SectionName).Get<LoggingOptions>();

        services.AddSingleton<ILoggerService, LoggerService>();

        services.AddSingleton<ITraceLogHandler, TraceLogHandler>();
        services.AddSingleton<IAuditLogHandler, AuditLogHandler>();

        services.AddSingleton<ITraceLogSink, AzureAppInsightTraceSink>();
        services.AddSingleton<ITraceLogSink, ConsoleTraceSink>();
        services.AddSingleton<IAuditLogSink, AzureAppInsightAuditSink>();

        return services;
    }
}
