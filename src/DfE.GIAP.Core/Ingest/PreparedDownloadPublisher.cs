using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// Publishes the prepared downloads written from ingested data files to the <c>downloads</c> container, in the same
/// storage account as the <c>ingest</c> container.
/// </summary>
/// <remarks>
/// The container has the old pipeline's <c>giapdownloads</c> layout, e.g. <c>LA/201/pp2425_201_1.csv</c>. The
/// Prepared Downloads page still lists the files the old pipeline publishes to the old downloads storage account; it
/// will read from this container once the new pipeline is live.
/// </remarks>
public class PreparedDownloadPublisher(BlobContainerClient containerClient)
{
    public const string ContainerName = "downloads";

    public Task CreateContainerIfNotExistsAsync(CancellationToken cancellationToken = default) =>
        containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

    /// <summary>
    /// Uploads a CSV file, replacing any file that's already there with the same name.
    /// </summary>
    public async Task PublishAsync(string blobName, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);
        ArgumentNullException.ThrowIfNull(content);

        await containerClient.GetBlobClient(blobName).UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "text/csv" } },
            cancellationToken);
    }
}
