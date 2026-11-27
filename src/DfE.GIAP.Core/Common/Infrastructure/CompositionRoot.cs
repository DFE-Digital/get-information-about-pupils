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
            .ValidateDataAnnotations()
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
