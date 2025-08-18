using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

public class AzureBlobStorageProvider : IBlobStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageProvider(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<IEnumerable<string>> ListBlobNamesAsync(string containerName, string directory)
    {
        BlobContainerClient containerClient = _blobServiceClient
            .GetBlobContainerClient(containerName);
        List<string> blobNames = new();

        await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: directory))
        {
            blobNames.Add(blob.Name);
        }

        return blobNames;
    }

    public async Task<IEnumerable<BlobItemInfo>> ListBlobsWithMetadataAsync(string containerName, string directory)
    {
        BlobContainerClient containerClient = _blobServiceClient
            .GetBlobContainerClient(containerName);

        List<BlobItemInfo> result = new();
        await foreach (BlobItem blob in containerClient.GetBlobsAsync(prefix: directory))
        {
            result.Add(new BlobItemInfo
            {
                Name = blob.Name,
                LastModified = blob.Properties.LastModified,
                Size = blob.Properties.ContentLength,
                ContentType = blob.Properties.ContentType
            });
        }

        return result;
    }

    public async Task<bool> ExistsAsync(string containerName, string blobPath)
    {
        BlobClient blobClient = _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobPath);

        return await blobClient.ExistsAsync();
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobPath)
    {
        BlobClient blobClient = _blobServiceClient
            .GetBlobContainerClient(containerName)
            .GetBlobClient(blobPath);

        Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }
}
