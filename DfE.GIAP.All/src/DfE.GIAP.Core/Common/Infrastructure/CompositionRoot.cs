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

        services.AddSingleton<ILogRouter, LogRouter>();
        services.AddSingleton<ILogMediator, LogMediator>();

        services.AddSingleton<ILogSink, AzureApplicationInsightsSink>();
        services.AddSingleton<ILogSink, ConsoleSink>();

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
