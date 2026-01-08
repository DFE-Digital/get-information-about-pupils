namespace DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

/// <summary>
/// Defines a generic interface for interacting with blob storage systems.
/// </summary>
public interface IBlobStorageProvider
{
    /// <summary>
    /// Asynchronously retrieves the names of blobs within the specified directory of a container.
    /// </summary>
    /// <remarks>This method performs a case-sensitive search for blobs within the specified directory. 
    /// Ensure that the container and directory names are correctly specified to avoid unexpected results.</remarks>
    /// <param name="containerName">The name of the container that contains the blobs. Cannot be null or empty.</param>
    /// <param name="directory">The directory within the container to search for blobs. Use an empty string to search the root directory.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of blob
    /// names. If no blobs are found, the collection will be empty.</returns>
    Task<IEnumerable<string>> ListBlobsByNamesAsync(string containerName, string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a collection of blobs and their metadata from the specified container and directory.
    /// </summary>
    /// <param name="containerName">The name of the container from which to list blobs. Cannot be null or empty.</param>
    /// <param name="directory">The directory within the container to search for blobs. Use an empty string to list blobs at the root level.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{T}"/> of
    /// <see cref="BlobItemMetadata"/> objects, each representing a blob and its associated metadata. The collection will be
    /// empty if no blobs are found.</returns>
    Task<IEnumerable<BlobItemMetadata>> ListBlobsWithMetadataAsync(string containerName, string directory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a blob exists in the specified container.
    /// </summary>
    /// <remarks>This method performs an asynchronous check to determine the existence of a blob in the
    /// specified container. It does not modify the state of the container or blob.</remarks>
    /// <param name="containerName">The name of the container where the blob is located. Cannot be null or empty.</param>
    /// <param name="blobPath">The path of the blob within the container. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns><see langword="true"/> if the blob exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> BlobExistsAsync(string containerName, string blobPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously downloads the specified blob as a stream.
    /// </summary>
    /// <remarks>This method retrieves the blob's content as a stream, which can be used for reading or
    /// processing the data. Ensure that the specified container</remarks>
    /// <param name="containerName">The name of the container that contains the blob.</param>
    /// <param name="blobPath">The path of the blob within the container.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Stream"/> containing the blob's data. The caller is responsible for disposing of the stream.</returns>
    Task<Stream> DownloadBlobAsStreamAsync(string containerName, string blobPath, CancellationToken cancellationToken = default);
}
