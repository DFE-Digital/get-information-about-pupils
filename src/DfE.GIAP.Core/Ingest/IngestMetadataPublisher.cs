using Azure.Storage.Blobs;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// Publishes the metadata documents shipped in uploaded zips to the <c>metadata</c> container, in the same storage
/// account as the <c>ingest</c> container.
/// </summary>
/// <remarks>
/// The Glossary page still lists the documents the old pipeline publishes to the old downloads storage account; it will
/// read from this container once the new pipeline is live.
/// </remarks>
public class IngestMetadataPublisher(BlobContainerClient containerClient)
{
    public const string ContainerName = "metadata";

    public Task CreateContainerIfNotExistsAsync(CancellationToken cancellationToken = default) =>
        containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

    /// <summary>
    /// Uploads a metadata document, replacing any document that's already there with the same name.
    /// </summary>
    public async Task PublishAsync(string name, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(content);

        await containerClient.GetBlobClient(name).UploadAsync(content, overwrite: true, cancellationToken);
    }
}
