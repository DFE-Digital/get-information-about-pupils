using System.IO.Compression;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Core.PupilData;
using Microsoft.EntityFrameworkCore;

namespace DfE.GIAP.Verification.Sources;

/// <summary>
/// A data file in one of the zips being compared.
/// </summary>
/// <param name="FilePath">The zip's <see cref="IngestFile.Path"/>, e.g. <c>ks2/NPD_KS2_2025_U.zip</c>.</param>
/// <param name="DataFileName">The data file's name in the zip, as in <see cref="IngestRun.DataFileName"/>.</param>
/// <param name="Attributes"><see langword="null"/> if the name isn't a valid data file name.</param>
public sealed record ScopedDataFile(string FilePath, string DataFileName, IngestFolder Folder, IngestDataFileAttributes? Attributes);

/// <summary>
/// What a comparison of particular zips covers: their data files, and the pupils and learners in them.
/// </summary>
/// <param name="FirstSeenAt">When the first of the zips was uploaded.</param>
public sealed record DataFileScope(
    IReadOnlyList<ScopedDataFile> DataFiles,
    IReadOnlyDictionary<RecordContainer, IReadOnlySet<string>> Keys,
    DateTime FirstSeenAt)
{
    /// <summary>
    /// The column that identifies a pupil or learner in each dataset's files, and the container they're in. Children
    /// looked after files are only written to prepared downloads, so they aren't here.
    /// </summary>
    public static IReadOnlyDictionary<IngestDataset, (RecordContainer Container, string KeyColumn)> KeyColumns { get; } =
        NpdDatasetDefinition.All
            .ToDictionary(d => d.Key, d => (RecordContainer.NationalPupilDatabase, d.Value.PupilMatchingRef))
            .Concat(new Dictionary<IngestDataset, (RecordContainer, string)>
            {
                [IngestDataset.PupilPremium] = (RecordContainer.PupilPremium, "PupilMatchingRef"),
                [IngestDataset.FurtherEducationPupilPremium] = (RecordContainer.FurtherEducation, "ULN"),
                [IngestDataset.FurtherEducationSpecialEducationalNeeds] = (RecordContainer.FurtherEducation, "ULN")
            })
            .ToDictionary();

    /// <summary>
    /// Reads every key in the data files of the zips, including those of rows that were rejected, from the zips
    /// themselves; so a pupil the new pipeline failed to write is still compared.
    /// </summary>
    /// <param name="filePaths">The zips' <see cref="IngestFile.Path"/>s.</param>
    public static async Task<DataFileScope> ReadAsync(
        IReadOnlyCollection<string> filePaths,
        IDbContextFactory<GiapDbContext> dbContextFactory,
        IngestStorage ingestStorage,
        CancellationToken cancellationToken = default)
    {
        List<IngestFile> files;
        await using (GiapDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            files = await dbContext.IngestFiles.AsNoTracking().Where(f => filePaths.Contains(f.Path)).ToListAsync(cancellationToken);
        }

        string[] unknown = [.. filePaths.Except(files.Select(f => f.Path))];
        if (unknown.Length > 0)
        {
            throw new InvalidOperationException($"The new pipeline hasn't seen these files: {string.Join(", ", unknown)}.");
        }

        List<ScopedDataFile> dataFiles = [];
        Dictionary<RecordContainer, HashSet<string>> keys = RecordContainer.All.ToDictionary(c => c, _ => new HashSet<string>(StringComparer.Ordinal));

        foreach (IngestFile file in files.OrderBy(f => f.FirstSeenAt))
        {
            (IngestFolder folder, _) = IngestFolder.ParseFilePath(file.Path);

            string zipPath = Path.GetTempFileName();
            try
            {
                await using (FileStream zip = File.Create(zipPath))
                {
                    await ingestStorage.DownloadToAsync(file.BlobName, zip, cancellationToken);
                }

                using ZipArchive archive = ZipFile.OpenRead(zipPath);
                foreach (ZipArchiveEntry entry in archive.Entries.Where(IngestFileProcessor.IsDataFile))
                {
                    IngestDataFileAttributes? attributes = TryParse(entry.Name);
                    dataFiles.Add(new ScopedDataFile(file.Path, entry.FullName, folder, attributes));

                    if (attributes is not null && KeyColumns.TryGetValue(attributes.Dataset, out (RecordContainer Container, string KeyColumn) key))
                    {
                        await using Stream content = entry.Open();
                        await ReadKeysAsync(content, key.KeyColumn, keys[key.Container], cancellationToken);
                    }
                }
            }
            finally
            {
                File.Delete(zipPath);
            }
        }

        return new DataFileScope(
            dataFiles,
            keys.ToDictionary(k => k.Key, k => (IReadOnlySet<string>)k.Value),
            files.Count == 0 ? DateTime.MaxValue : files.Min(f => f.FirstSeenAt));
    }

    internal static async Task ReadKeysAsync(Stream content, string keyColumn, HashSet<string> keys, CancellationToken cancellationToken)
    {
        try
        {
            using DataFileReader reader = await DataFileReader.OpenAsync(content, [keyColumn], cancellationToken);
            int column = reader.GetColumn(keyColumn);

            await foreach (DataFileRow row in reader.ReadRowsAsync(cancellationToken))
            {
                if (row.Values[column] is { } key)
                {
                    keys.Add(key);
                }
            }
        }
        catch (IngestDataFileException)
        {
            // A file with no key column failed in both pipelines; its results are still compared
        }
    }

    /// <returns><see langword="null"/> if the name isn't a valid data file name.</returns>
    internal static IngestDataFileAttributes? TryParse(string dataFileName)
    {
        try
        {
            return IngestDataFileName.Parse(dataFileName);
        }
        catch (IngestDataFileNameException)
        {
            return null;
        }
    }
}
