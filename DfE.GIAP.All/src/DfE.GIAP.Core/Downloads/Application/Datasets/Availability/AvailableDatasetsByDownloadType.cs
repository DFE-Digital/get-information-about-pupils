using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.Datasets.Availability;

/// <summary>
/// Provides supported datasets for each download type.
/// </summary>
public static class AvailableDatasetsByDownloadType
{
    private static readonly Dictionary<DownloadType, HashSet<Dataset>> s_map = new()
    {
        [DownloadType.FurtherEducation] = new()
        {
            Dataset.PP,
            Dataset.SEN
        },
        [DownloadType.PupilPremium] = new()
        {
            Dataset.PP
        },
        [DownloadType.NPD] = new()
        {
            Dataset.Census_Autumn,
            Dataset.Census_Spring,
            Dataset.Census_Summer,
            Dataset.EYFSP,
            Dataset.KS1,
            Dataset.KS2,
            Dataset.KS4,
            Dataset.Phonics,
            Dataset.MTC
        }
    };

    /// <summary>
    /// Returns the list of datasets supported for a given download type.
    /// </summary>
    public static IReadOnlyCollection<Dataset> GetSupportedDatasets(DownloadType type)
    {
        return s_map.TryGetValue(type, out HashSet<Dataset>? datasets)
            ? datasets
            : Array.Empty<Dataset>();
    }

    /// <summary>
    /// Checks if a specific dataset is supported for a given download type.
    /// </summary>
    public static bool IsSupported(DownloadType type, Dataset dataset)
    {
        return s_map.TryGetValue(type, out HashSet<Dataset>? datasets) && datasets.Contains(dataset);
    }
}
