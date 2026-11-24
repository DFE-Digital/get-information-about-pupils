using System.IO.Compression;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Verification.Anonymisation;

/// <param name="Rows">How many rows each data file had, by its name in the zip.</param>
public sealed record ZipAnonymisationResult(IReadOnlyDictionary<string, int> Rows);

/// <summary>
/// Anonymises every data file in a zip, and copies everything else in it unchanged, with the same names.
/// </summary>
public sealed class ZipAnonymiser(DataFileAnonymiser dataFiles, AnonymisationClassification classification)
{
    /// <param name="source">The real zip; it must be seekable.</param>
    /// <param name="destination">Where the anonymised zip is written; it doesn't need to be seekable.</param>
    /// <exception cref="AnonymisationException">A data file can't be anonymised.</exception>
    public ZipAnonymisationResult Anonymise(Stream source, Stream destination, IngestFolder folder)
    {
        FolderClassification folderClassification = classification.GetFolder(folder)
            ?? throw new AnonymisationException($"None of the {folder.Name} data files are classified.");

        using ZipArchive sourceArchive = new(source, ZipArchiveMode.Read, leaveOpen: true);
        using ZipArchive destinationArchive = new(destination, ZipArchiveMode.Create, leaveOpen: true);

        Dictionary<string, int> rows = [];
        foreach (ZipArchiveEntry entry in sourceArchive.Entries)
        {
            if (entry.FullName.EndsWith('/'))
            {
                destinationArchive.CreateEntry(entry.FullName);
                continue;
            }

            ZipArchiveEntry copy = destinationArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
            copy.LastWriteTime = entry.LastWriteTime;

            using Stream input = entry.Open();
            using Stream output = copy.Open();

            if (IngestFileProcessor.IsDataFile(entry))
            {
                try
                {
                    rows[entry.FullName] = dataFiles.Anonymise(input, output, folderClassification);
                }
                catch (AnonymisationException ex)
                {
                    throw new AnonymisationException($"{entry.FullName}: {ex.Message}");
                }
            }
            else
            {
                // Metadata documents describe the dataset, and hidden files are skipped by the pipelines
                input.CopyTo(output);
            }
        }

        return new ZipAnonymisationResult(rows);
    }
}
