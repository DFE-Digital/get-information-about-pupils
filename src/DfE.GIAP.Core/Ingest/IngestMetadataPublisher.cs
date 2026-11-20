using Azure.Storage.Blobs;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// Publishes the metadata documents shipped in uploaded zips to the downloads container, where the Glossary page lists
/// them for users to download.
/// </summary>
public class IngestMetadataPublisher(BlobContainerClient downloadsContainerClient)
{
    public const string ContainerName = "giapdownloads";

    // The folder the Glossary page lists; see BlobStoragePathContext
    public const string FolderPath = "AllUsers/Metadata/";

    /// <summary>
    /// Uploads a metadata document, replacing any document that's already there with the same name.
    /// </summary>
    public async Task PublishAsync(string name, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(content);

        await downloadsContainerClient.GetBlobClient(FolderPath + name).UploadAsync(content, overwrite: true, cancellationToken);
    }
}
