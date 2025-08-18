using Azure.Storage.Blobs;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
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

            string connectionString = $"DefaultEndpointsProtocol=https;AccountName={options.AccountName};AccountKey={options.AccountKey};EndpointSuffix={options.EndpointSuffix}";

            BlobServiceClient blobServiceClient = new(connectionString);
            return new AzureBlobStorageProvider(blobServiceClient);
        });

        return services;
    }
}
