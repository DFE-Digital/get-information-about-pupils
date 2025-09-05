using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

public class AzureBlobStorageProvider : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageProvider(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<IEnumerable<string>> ListBlobsByNamesAsync(string containerName, string directory, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient
            .GetBlobContainerClient(containerName);

        List<string> blobNames = new();
        await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: directory, cancellationToken: cancellationToken))
        {
            blobNames.Add(blob.Name);
        }

        return blobNames;
    }

    public async Task<IEnumerable<BlobItemMetadata>> ListBlobsWithMetadataAsync(string containerName, string directory, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = _blobServiceClient
            .GetBlobContainerClient(containerName);

        List<BlobItemMetadata> result = new();
        await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: directory, cancellationToken: cancellationToken))
        {
            result.Add(new BlobItemMetadata
            {
                Name = blob.Name,
                LastModified = blob.Properties.LastModified,
                Size = blob.Properties.ContentLength,
                ContentType = blob.Properties.ContentType
            });
        }

        return result;
    }

    public async Task<bool> BlobExistsAsync(string containerName, string blobPath, CancellationToken cancellationToken = default)
    {
        BlobClient blobClient = _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobPath);

        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task<Stream> DownloadBlobAsStreamAsync(string containerName, string blobPath, CancellationToken cancellationToken = default)
    {
        BlobClient blobClient = _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobPath);

        if (!await BlobExistsAsync(containerName, blobPath, cancellationToken))
            throw new FileNotFoundException($"Blob '{blobPath}' not found in container '{containerName}'.");

        Response<BlobDownloadInfo> response = await blobClient.DownloadAsync(cancellationToken);
        return response.Value.Content;
    }
}
