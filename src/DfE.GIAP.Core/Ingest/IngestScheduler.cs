using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Jobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// Decides which zips need processing and enqueues a <see cref="ProcessIngestFileJob"/> for each.
/// </summary>
/// <remarks>
/// A file is only enqueued by whoever moves it to <see cref="IngestFileStatus.Pending"/>, so it's safe for two of
/// these to run at once.
/// </remarks>
public class IngestScheduler(
    IngestStorage storage,
    IDbContextFactory<GiapDbContext> dbContextFactory,
    IBackgroundJobClient backgroundJobClient,
    TimeProvider timeProvider,
    ILogger<IngestScheduler> logger)
{
    /// <summary>
    /// Enqueues the zips in the <c>upload/</c> folders that haven't been processed, oldest first.
    /// </summary>
    /// <remarks>
    /// A zip with the same path as one that's already been processed successfully is a duplicate; it's moved to
    /// <c>error/</c> without being processed. One with the same path as a zip that failed is processed again.
    /// </remarks>
    public async Task ScanUploadsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<IngestUpload> uploads = await storage.ListUploadsAsync(cancellationToken);

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        foreach (IngestUpload upload in uploads)
        {
            string path = upload.Folder.GetFilePath(upload.ZipName);
            DateTime now = timeProvider.GetUtcNow().UtcDateTime;

            int inserted = await dbContext.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO ingest_files (path, blob_name, status, attempts, first_seen_at)
                VALUES ({path}, {upload.BlobName}, {nameof(IngestFileStatus.Pending)}, 0, {now})
                ON CONFLICT DO NOTHING
                """,
                cancellationToken);

            if (inserted == 1)
            {
                Enqueue(path);
                continue;
            }

            IngestFile file = await dbContext.IngestFiles.AsNoTracking().SingleAsync(f => f.Path == path, cancellationToken);

            // A file that failed before it could be moved is still at its upload path; it's only processed again when
            // it's replayed
            if (file.Status == IngestFileStatus.Failed && file.BlobName != upload.BlobName)
            {
                int updated = await dbContext.IngestFiles
                    .Where(f => f.Path == path && f.Status == IngestFileStatus.Failed && f.BlobName == file.BlobName)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(f => f.Status, IngestFileStatus.Pending).SetProperty(f => f.BlobName, upload.BlobName),
                        cancellationToken);

                if (updated == 1)
                {
                    logger.LogInformation("{Path} was uploaded again after it failed; it will be processed again.", path);
                    Enqueue(path);
                }
            }
            else if (file.Status == IngestFileStatus.Succeeded)
            {
                string errorBlobName = await storage.MoveAsync(upload.BlobName, upload.Folder, IngestFolder.ErrorFolderName, cancellationToken);

                logger.LogWarning(
                    "{Path} was uploaded again after it was processed successfully, so it has been moved to {BlobName} without being processed. Replay the file to process it again.",
                    path,
                    errorBlobName);
            }
        }
    }

    /// <summary>
    /// Enqueues every file whose latest processing failed, oldest first.
    /// </summary>
    public async Task RequeueFailedAsync(CancellationToken cancellationToken = default)
    {
        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        string[] paths = await dbContext.IngestFiles
            .Where(f => f.Status == IngestFileStatus.Failed)
            .OrderBy(f => f.FirstSeenAt)
            .Select(f => f.Path)
            .ToArrayAsync(cancellationToken);

        foreach (string path in paths)
        {
            int updated = await dbContext.IngestFiles
                .Where(f => f.Path == path && f.Status == IngestFileStatus.Failed)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.Status, IngestFileStatus.Pending), cancellationToken);

            if (updated == 1)
            {
                Enqueue(path);
            }
        }

        logger.LogInformation("Requeued {Count} failed ingest files.", paths.Length);
    }

    private void Enqueue(string path) =>
        backgroundJobClient.Enqueue<ProcessIngestFileJob>(job => job.ExecuteAsync(path, CancellationToken.None));
}
