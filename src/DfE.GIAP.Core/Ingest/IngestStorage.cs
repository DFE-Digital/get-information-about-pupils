using System.Globalization;
using System.Net;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DfE.GIAP.Core.Ingest;

/// <param name="BlobName">E.g. <c>ks2/upload/NPD_KS2_2025_U.zip</c>.</param>
public sealed record IngestUpload(IngestFolder Folder, string ZipName, string BlobName, DateTimeOffset CreatedOn);

/// <summary>
/// The blob container that zips are uploaded to for ingestion.
/// </summary>
public class IngestStorage(BlobContainerClient containerClient, TimeProvider timeProvider)
{
    public const string ContainerName = "ingest";

    public const string ConnectionName = "IngestStorage";

    public Task CreateContainerIfNotExistsAsync(CancellationToken cancellationToken = default) =>
        containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

    /// <summary>
    /// Lists the zips in every folder's <c>upload/</c> folder, oldest first. Blobs in subfolders are ignored.
    /// </summary>
    public async Task<IReadOnlyList<IngestUpload>> ListUploadsAsync(CancellationToken cancellationToken = default)
    {
        List<IngestUpload> uploads = [];

        foreach (IngestFolder folder in IngestFolder.All)
        {
            await foreach (BlobHierarchyItem item in containerClient.GetBlobsByHierarchyAsync(
                delimiter: "/",
                prefix: folder.UploadPrefix,
                cancellationToken: cancellationToken))
            {
                if (item.IsBlob)
                {
                    uploads.Add(new IngestUpload(
                        folder,
                        ZipName: item.Blob.Name[folder.UploadPrefix.Length..],
                        BlobName: item.Blob.Name,
                        CreatedOn: item.Blob.Properties.CreatedOn ?? DateTimeOffset.MinValue));
                }
            }
        }

        return [.. uploads.OrderBy(u => u.CreatedOn).ThenBy(u => u.BlobName, StringComparer.Ordinal)];
    }

    public async Task DownloadToAsync(string blobName, Stream destination, CancellationToken cancellationToken = default) =>
        await containerClient.GetBlobClient(blobName).DownloadToAsync(destination, cancellationToken);

    /// <summary>
    /// Moves a zip into its folder's <paramref name="targetFolderName"/> folder, and returns its new blob name.
    /// </summary>
    /// <param name="zipName">The zip's name as it was uploaded; the new blob name is based on this, not
    /// <paramref name="blobName"/>, so a zip moved again isn't given a second timestamp.</param>
    /// <remarks>
    /// The zip is given a suffix of the UTC time it was moved, e.g. <c>NPD_KS2_2025_U.20261120T101500Z.zip</c>, and a
    /// blob that's already there is never overwritten. A zip that's already in the target folder isn't moved.
    /// </remarks>
    public async Task<string> MoveAsync(
        string blobName,
        IngestFolder folder,
        string zipName,
        string targetFolderName,
        CancellationToken cancellationToken = default)
    {
        string targetPrefix = $"{folder.Name}/{targetFolderName}/";

        if (blobName.StartsWith(targetPrefix, StringComparison.Ordinal))
        {
            return blobName;
        }

        BlobClient source = containerClient.GetBlobClient(blobName);

        string timestamp = timeProvider.GetUtcNow().ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        string targetBlobName = targetPrefix + Path.GetFileNameWithoutExtension(zipName) + "." + timestamp + Path.GetExtension(zipName);

        if (!await TryCopyAsync(source, targetBlobName, cancellationToken))
        {
            throw new InvalidOperationException($"Can't move '{blobName}' because '{targetBlobName}' already exists.");
        }

        await source.DeleteAsync(cancellationToken: cancellationToken);

        return targetBlobName;
    }

    // Copies the blob within the container unless the target already exists
    private async Task<bool> TryCopyAsync(BlobClient source, string targetBlobName, CancellationToken cancellationToken)
    {
        BlobClient target = containerClient.GetBlobClient(targetBlobName);

        try
        {
            CopyFromUriOperation operation = await target.StartCopyFromUriAsync(
                source.Uri,
                new BlobCopyFromUriOptions { DestinationConditions = new BlobRequestConditions { IfNoneMatch = ETag.All } },
                cancellationToken);
            await operation.WaitForCompletionAsync(cancellationToken);

            return true;
        }
        catch (RequestFailedException ex)
            when (ex.Status is (int)HttpStatusCode.Conflict or (int)HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }
}
