using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Common.Infrastructure.Logging;
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
        Dictionary<string, SinkConfig> sinks = options?.Sinks ?? new();

        foreach ((string sinkName, _) in sinks)
        {
            switch (sinkName)
            {
                case "Console":
                    services.AddConsoleSink();
                    break;

                case "AzureAppInsight":
                    services.AddAzureAppInsightsSink();
                    break;
            }
        }

        services.TryAddSingleton<ILoggerService, LoggerService>();
        return services;
    }


    internal static IServiceCollection AddConsoleSink(this IServiceCollection services)
    {
        //services.Configure<ConsoleSinkOptions>(config.GetSection("Logging:Console"));
        // example: register other console related config or logic here, file paths ect
        services.TryAddSingleton<ILogSink, ConsoleSink>();

        return services;
    }

    internal static IServiceCollection AddAzureAppInsightsSink(this IServiceCollection services)
    {
        // services.Configure<AzureSinkOptions>(config.GetSection("Logging:Azure"));
        // exmaple: register app insights here
        services.TryAddSingleton<ILogSink, AzureApplicationInsightsSink>();

        return services;
    }
}
