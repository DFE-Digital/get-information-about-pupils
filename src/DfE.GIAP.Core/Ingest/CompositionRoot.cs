using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Ingest;

public static class CompositionRoot
{
    public static IServiceCollection AddIngestDependencies(this IServiceCollection services, string storageConnectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(storageConnectionString);

        services.AddBlobStorageOptions();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(sp => new IngestStorage(
            new BlobContainerClient(storageConnectionString, IngestStorage.ContainerName),
            sp.GetRequiredService<TimeProvider>()));
        services.TryAddSingleton(sp => new IngestMetadataPublisher(new BlobContainerClient(
            sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value.ConnectionString,
            IngestMetadataPublisher.ContainerName)));
        services.TryAddSingleton<IngestDatasetProcessorRegistry>();
        services.TryAddTransient<IngestScheduler>();
        services.TryAddTransient<IngestFileProcessor>();

        return services;
    }
}
