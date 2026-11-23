using System.IO.Compression;
using System.Text.Json;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// Processes an uploaded zip: routes each data file in it to the <see cref="IIngestDatasetProcessor"/> for its
/// dataset, publishes its metadata documents, then moves the zip to <c>processed/</c>, or to <c>error/</c> if anything
/// failed.
/// </summary>
/// <remarks>
/// Every data file is processed and committed on its own, even if another file in the zip fails; each one gets an
/// <see cref="IngestRun"/>. The metadata documents are only published once every data file has succeeded.
/// Processing a zip again (e.g. replaying it) processes all of its data files again.
/// </remarks>
public class IngestFileProcessor(
    IngestStorage storage,
    IngestDatasetProcessorRegistry processorRegistry,
    IngestMetadataPublisher metadataPublisher,
    IDbContextFactory<GiapDbContext> dbContextFactory,
    TimeProvider timeProvider,
    ILogger<IngestFileProcessor> logger)
{
    /// <exception cref="IngestFileFailedException">The zip, or a data file in it, failed.</exception>
    public async Task ProcessAsync(string path, CancellationToken cancellationToken = default)
    {
        (IngestFolder folder, string zipName) = IngestFolder.ParseFilePath(path);

        await using GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        // Kept open for BinaryCopy
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        IngestFile file = await dbContext.IngestFiles.SingleOrDefaultAsync(f => f.Path == path, cancellationToken)
            ?? throw new InvalidOperationException($"There's no ingest file with path '{path}'.");

        file.Status = IngestFileStatus.Processing;
        file.Attempts++;
        file.Error = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Processing {Path} from {BlobName} (attempt {Attempt}).", path, file.BlobName, file.Attempts);

        string? error;

        try
        {
            await using FileStream zipStream = new(
                Path.GetTempFileName(),
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            await storage.DownloadToAsync(file.BlobName, zipStream, cancellationToken);
            zipStream.Position = 0;

            error = await ProcessZipAsync(dbContext, file, folder, zipName, zipStream, cancellationToken);

            file.BlobName = await storage.MoveAsync(
                file.BlobName,
                folder,
                zipName,
                error is null ? IngestFolder.ProcessedFolderName : IngestFolder.ErrorFolderName,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The zip is left where it is; it has to be replayed to process it again
            file.Status = IngestFileStatus.Failed;
            file.Error = ex.Message;
            file.LastProcessedAt = timeProvider.GetUtcNow().UtcDateTime;
            await dbContext.SaveChangesAsync(CancellationToken.None);

            throw;
        }

        file.Status = error is null ? IngestFileStatus.Succeeded : IngestFileStatus.Failed;
        file.Error = error;
        file.LastProcessedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (error is not null)
        {
            throw new IngestFileFailedException(path, error);
        }

        logger.LogInformation("Processed {Path}; it has been moved to {BlobName}.", path, file.BlobName);
    }

    // Returns why the zip failed, or null if it succeeded
    private async Task<string?> ProcessZipAsync(
        GiapDbContext dbContext,
        IngestFile file,
        IngestFolder folder,
        string zipName,
        Stream zipStream,
        CancellationToken cancellationToken)
    {
        if (!zipName.StartsWith(folder.ZipNamePrefix, StringComparison.Ordinal))
        {
            return $"The name of a zip uploaded to {folder.Name} must start with {folder.ZipNamePrefix}.";
        }

        if (!zipName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return "Only zips can be uploaded.";
        }

        ZipArchive archive;

        try
        {
            archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        }
        catch (InvalidDataException)
        {
            return "The file isn't a valid zip.";
        }

        using (archive)
        {
            ZipArchiveEntry[] dataFiles = [.. archive.Entries.Where(IsDataFile)];

            if (dataFiles.Length == 0)
            {
                return "The zip doesn't contain any data files.";
            }

            int failed = 0;

            foreach (ZipArchiveEntry dataFile in dataFiles)
            {
                IngestRun run = await ProcessDataFileAsync(dbContext, file, folder, dataFile, cancellationToken);

                if (run.Status == IngestRunStatus.Failed)
                {
                    failed++;
                }
            }

            if (failed > 0)
            {
                return $"{failed} of the {dataFiles.Length} data files in the zip failed.";
            }

            return await PublishMetadataDocumentsAsync(file, archive, cancellationToken);
        }
    }

    // Returns why publishing failed, or null if it succeeded
    private async Task<string?> PublishMetadataDocumentsAsync(IngestFile file, ZipArchive archive, CancellationToken cancellationToken)
    {
        foreach (ZipArchiveEntry document in archive.Entries.Where(IsMetadataDocument))
        {
            try
            {
                await using Stream content = await document.OpenAsync(cancellationToken);
                await metadataPublisher.PublishAsync(document.Name, content, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to publish {DocumentName} from {Path}.", document.FullName, file.Path);

                return $"Publishing the metadata document {document.FullName} failed: {ex.Message}";
            }

            logger.LogInformation("Published {DocumentName} from {Path}.", document.FullName, file.Path);
        }

        return null;
    }

    /// <summary>
    /// Whether a zip entry is one of the zip's data files, rather than a metadata document, hidden file or folder.
    /// </summary>
    public static bool IsDataFile(ZipArchiveEntry entry) =>
        !IsHiddenOrFolder(entry) && !IngestDataFileName.IsMetadataDocument(entry.Name);

    private static bool IsMetadataDocument(ZipArchiveEntry entry) =>
        !IsHiddenOrFolder(entry) && IngestDataFileName.IsMetadataDocument(entry.Name);

    // Hidden files include macOS's __MACOSX/ and ._ files
    private static bool IsHiddenOrFolder(ZipArchiveEntry entry) =>
        entry.Name.Length == 0
        || entry.Name.StartsWith('.')
        || entry.FullName.StartsWith("__MACOSX/", StringComparison.Ordinal);

    private async Task<IngestRun> ProcessDataFileAsync(
        GiapDbContext dbContext,
        IngestFile file,
        IngestFolder folder,
        ZipArchiveEntry dataFile,
        CancellationToken cancellationToken)
    {
        IngestRun run = new()
        {
            Id = Guid.CreateVersion7(),
            FilePath = file.Path,
            Attempt = file.Attempts,
            DataFileName = dataFile.FullName,
            Status = IngestRunStatus.Running,
            StartedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        dbContext.IngestRuns.Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            IngestDataFileAttributes attributes = IngestDataFileName.Parse(dataFile.Name);
            run.Dataset = attributes.Dataset;
            run.AcademicYear = attributes.AcademicYear;
            run.IngestVersion = attributes.IngestVersion;

            if (!folder.Datasets.Contains(attributes.Dataset))
            {
                throw new IngestDataFileException($"{attributes.Dataset} data files can't be uploaded to {folder.Name}.");
            }

            IIngestDatasetProcessor processor = processorRegistry.GetProcessor(attributes.Dataset)
                ?? throw new IngestDataFileException($"{attributes.Dataset} data files can't be processed yet.");

            IngestDataFileResult result;
            await using (Stream content = await dataFile.OpenAsync(cancellationToken))
            {
                result = await processor.ProcessAsync(new IngestDataFile(dataFile.Name, attributes, content), cancellationToken);
            }

            if (result.RejectedRows.Count > 0)
            {
                await BinaryCopy.CopyAsync(
                    dbContext,
                    result.RejectedRows.Select(r => new IngestRejectedRow
                    {
                        RunId = run.Id,
                        RowNumber = r.RowNumber,
                        Data = JsonSerializer.Serialize(r.Row),
                        Reasons = [.. r.Reasons]
                    }),
                    cancellationToken: cancellationToken);
            }

            run.RecordsRead = result.RecordsRead;
            run.RecordsWritten = result.RecordsWritten;
            run.RecordsRemoved = result.RecordsRemoved;
            run.RecordsRejected = result.RejectedRows.Count;
            run.Status = IngestRunStatus.Succeeded;

            logger.LogInformation(
                "Processed {DataFileName} from {Path}: {RecordsRead} read, {RecordsWritten} written, {RecordsRemoved} removed and {RecordsRejected} rejected.",
                run.DataFileName,
                file.Path,
                run.RecordsRead,
                run.RecordsWritten,
                run.RecordsRemoved,
                run.RecordsRejected);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            run.Status = IngestRunStatus.Failed;
            run.Error = ex.Message;

            logger.LogError(ex, "Failed to process {DataFileName} from {Path}.", run.DataFileName, file.Path);
        }

        run.FinishedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);

        return run;
    }
}

/// <summary>
/// A data file in a zip can't be processed.
/// </summary>
public sealed class IngestDataFileException(string message) : Exception(message);

public sealed class IngestFileFailedException(string path, string reason) : Exception($"{path} failed: {reason}")
{
    public string Path { get; } = path;
}
