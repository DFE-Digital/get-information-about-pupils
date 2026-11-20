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

        services.AddOptions<BlobStorageOptions>()
            .BindConfiguration(BlobStorageOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.AccountName), "BlobStorage: AccountName is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.AccountKey), "BlobStorage: AccountKey is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ContainerName), "BlobStorage: ContainerName is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.EndpointSuffix), "BlobStorage: EndpointSuffix is required")
            .ValidateOnStart();

        services.TryAddSingleton<IBlobStorageProvider>(sp =>
        {
            BlobStorageOptions options = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            BlobServiceClient blobServiceClient = new(options.ConnectionString);

            return new AzureBlobStorageProvider(blobServiceClient);
        });

        return services;
    }
}
