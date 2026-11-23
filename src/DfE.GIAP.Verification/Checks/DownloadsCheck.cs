using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DfE.GIAP.Verification.Comparison;
using DfE.GIAP.Verification.Reports;

namespace DfE.GIAP.Verification.Checks;

/// <summary>
/// Compares the prepared downloads in the new pipeline's <c>downloads</c> container with those in the old pipeline's
/// <c>giapdownloads</c> container.
/// </summary>
public sealed class DownloadsCheck(BlobContainerClient newContainer, BlobContainerClient oldContainer, CheckOptions options)
{
    // The folders prepared downloads are written to; giapdownloads has other files too, e.g. metadata documents
    private static readonly string[] _prefixes = ["LA/", "School/", "MAT/", "SAT/"];

    /// <param name="since">
    /// Only files changed on either side since then are compared, or every file if <see langword="null"/>.
    /// </param>
    public async Task CompareAsync(DateTime? since, VerificationReport report, CancellationToken cancellationToken = default)
    {
        Dictionary<string, DateTimeOffset?> newFiles = await ListAsync(newContainer, cancellationToken);
        Dictionary<string, DateTimeOffset?> oldFiles = await ListAsync(oldContainer, cancellationToken);

        string[] paths =
        [
            .. newFiles.Keys.Union(oldFiles.Keys)
                .Where(p => since is null || newFiles.GetValueOrDefault(p) >= since || oldFiles.GetValueOrDefault(p) >= since)
                .Order(StringComparer.Ordinal)
        ];

        await Parallel.ForEachAsync(
            paths,
            new ParallelOptions { MaxDegreeOfParallelism = options.Concurrency, CancellationToken = cancellationToken },
            async (path, ct) =>
            {
                if (!newFiles.ContainsKey(path))
                {
                    report.Add(new Difference(DownloadComparer.Comparison, DifferenceKinds.MissingInNew, path.Split('/')[0], Field: null, path));
                }
                else if (!oldFiles.ContainsKey(path))
                {
                    report.Add(new Difference(DownloadComparer.Comparison, DifferenceKinds.MissingInOld, path.Split('/')[0], Field: null, path));
                }
                else
                {
                    byte[] newContent = (await newContainer.GetBlobClient(path).DownloadContentAsync(ct)).Value.Content.ToArray();
                    byte[] oldContent = (await oldContainer.GetBlobClient(path).DownloadContentAsync(ct)).Value.Content.ToArray();

                    foreach (Difference difference in DownloadComparer.Compare(path, newContent, oldContent))
                    {
                        report.Add(difference);
                    }
                }

                report.AddCompared(DownloadComparer.Comparison, 1);
            });
    }

    private static async Task<Dictionary<string, DateTimeOffset?>> ListAsync(BlobContainerClient container, CancellationToken cancellationToken)
    {
        Dictionary<string, DateTimeOffset?> files = new(StringComparer.Ordinal);

        foreach (string prefix in _prefixes)
        {
            await foreach (BlobItem item in container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
            {
                files[item.Name] = item.Properties.LastModified;
            }
        }

        return files;
    }
}
