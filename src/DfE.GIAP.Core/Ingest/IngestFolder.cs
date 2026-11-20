using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.Ingest;

/// <summary>
/// A dataset folder in the ingest container. Zips are uploaded to its <c>upload/</c> folder and moved to
/// <c>processed/</c> or <c>error/</c> once they've been processed.
/// </summary>
/// <remarks>
/// The folders and zip name prefixes are the old pipeline's.
/// </remarks>
public sealed class IngestFolder
{
    public const string UploadFolderName = "upload";
    public const string ProcessedFolderName = "processed";
    public const string ErrorFolderName = "error";

    private IngestFolder(string name, string zipNamePrefix, params IngestDataset[] datasets)
    {
        Name = name;
        ZipNamePrefix = zipNamePrefix;
        Datasets = datasets;
    }

    public static IReadOnlyList<IngestFolder> All { get; } =
    [
        new("census", "NPD_Census_", IngestDataset.CensusAutumn, IngestDataset.CensusSpring, IngestDataset.CensusSummer),
        new("eyfsp", "NPD_EY", IngestDataset.Eyfsp),
        new("ks1", "NPD_KS1_", IngestDataset.Ks1),
        new("ks2", "NPD_KS2_", IngestDataset.Ks2),
        new("ks4", "NPD_KS4_", IngestDataset.Ks4),
        new("mtc", "NPD_MTC_", IngestDataset.Mtc),
        new("phonics", "NPD_PHONICS_", IngestDataset.Phonics),
        new("pupil-premium", "NPD_pupil-premium_", IngestDataset.PupilPremium),
        new("fe_pp", "FE_PP_", IngestDataset.FurtherEducationPupilPremium),
        new("fe_sen", "FE_SEN_", IngestDataset.FurtherEducationSpecialEducationalNeeds)
    ];

    public string Name { get; }

    /// <summary>
    /// What the name of every zip uploaded to the folder must start with; case-sensitive.
    /// </summary>
    public string ZipNamePrefix { get; }

    /// <summary>
    /// The datasets the folder's data files can contain.
    /// </summary>
    public IReadOnlyCollection<IngestDataset> Datasets { get; }

    public string UploadPrefix => $"{Name}/{UploadFolderName}/";

    public static IngestFolder Get(string name) =>
        All.SingleOrDefault(f => f.Name == name) ?? throw new ArgumentException($"There's no ingest folder named '{name}'.", nameof(name));

    /// <summary>
    /// The <see cref="IngestFile.Path"/> for a zip uploaded to this folder.
    /// </summary>
    public string GetFilePath(string zipName) => $"{Name}/{zipName}";

    public static (IngestFolder Folder, string ZipName) ParseFilePath(string filePath)
    {
        int separator = filePath.IndexOf('/');

        if (separator <= 0)
        {
            throw new ArgumentException($"'{filePath}' isn't an ingest file path.", nameof(filePath));
        }

        return (Get(filePath[..separator]), filePath[(separator + 1)..]);
    }
}
