using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Ingest;

public static class CompositionRoot
{
    public static IServiceCollection AddIngestDependencies(this IServiceCollection services, string storageConnectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(storageConnectionString);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(sp => new IngestStorage(
            new BlobContainerClient(storageConnectionString, IngestStorage.ContainerName),
            sp.GetRequiredService<TimeProvider>()));
        services.TryAddSingleton(new IngestMetadataPublisher(
            new BlobContainerClient(storageConnectionString, IngestMetadataPublisher.ContainerName)));
        services.TryAddSingleton<IngestDatasetProcessorRegistry>();
        services.TryAddTransient<IngestScheduler>();
        services.TryAddTransient<IngestFileProcessor>();

        return services;
    }
}
