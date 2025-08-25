namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

/// <summary>
/// Defines a generic interface for interacting with blob storage systems.
/// </summary>
public interface IBlobStorageProvider
{
    /// <summary>
    /// Lists the names of blobs within a specified virtual directory.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="directory">The virtual directory path within the container.</param>
    /// <returns>A collection of blob names.</returns>
    Task<IEnumerable<string>> ListBlobsAsync(string containerName, string directory);

    /// <summary>
    /// Lists blobs along with their metadata within a specified virtual directory.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="directory">The virtual directory path within the container.</param>
    /// <returns>A collection of blob metadata objects.</returns>
    Task<IEnumerable<BlobItemInfo>> ListBlobsWithMetadataAsync(string containerName, string directory);

    /// <summary>
    /// Checks whether a blob exists at the specified path.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobPath">The full path of the blob within the container.</param>
    /// <returns>True if the blob exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string containerName, string blobPath);

    /// <summary>
    /// Downloads the contents of a blob as a stream.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobPath">The full path of the blob within the container.</param>
    /// <returns>A stream containing the blob's contents.</returns>
    Task<Stream> DownloadAsync(string containerName, string blobPath);
}
